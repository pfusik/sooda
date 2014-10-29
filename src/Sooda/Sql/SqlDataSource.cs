//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Text;
using System.Data;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Sooda.Schema;
using Sooda.QL;
using Sooda.Utils;

using Sooda.Logging;

namespace Sooda.Sql
{
    public class SqlDataSource : Sooda.SoodaDataSource
    {
        protected static readonly Logger logger = LogManager.GetLogger("Sooda.SqlDataSource");
        protected static readonly Logger sqllogger = LogManager.GetLogger("Sooda.SQL");

        private IDbCommand _updateCommand = null;
        private IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

        bool OwnConnection = false;
        public IDbTransaction Transaction;
        public ISqlBuilder SqlBuilder = null;
        public bool DisableTransactions = false;
        public bool DisableUpdateBatch = false;
        public bool StripWhitespaceInLogs = false;
        public bool IndentQueries = false;
        public bool UpperLike = false;
        public double QueryTimeTraceWarn = 10.0;
        public double QueryTimeTraceInfo = 2.0;
        public int CommandTimeout = 30;
        public Type ConnectionType;
        public string ConnectionString;
        public string CreateTable = "";
        public string CreateIndex = "";

        public SqlDataSource(string name) : base(name)
        {
            string s = GetParameter("queryTimeTraceInfo", false);
            if (s != null && s.Length > 0)
                QueryTimeTraceInfo = Convert.ToDouble(s);

            s = GetParameter("queryTimeTraceWarn", false);
            if (s != null && s.Length > 0)
                QueryTimeTraceWarn = Convert.ToDouble(s);

            s = GetParameter("commandTimeout", false);
            if (s != null && s.Length > 0)
                CommandTimeout = Convert.ToInt32(s);

            if (GetParameter("disableTransactions", false) == "true")
                this.DisableTransactions = true;

            if (GetParameter("stripWhitespaceInLogs", false) == "true")
                this.StripWhitespaceInLogs = true;

            if (GetParameter("indentQueries", false) == "true")
                this.IndentQueries = true;

            if (GetParameter("upperLike", false) == "true")
                this.UpperLike = true;

            string at = GetParameter("createTable", false);
            if (at != null)
                this.CreateTable = at;

            at = GetParameter("createIndex", false);
            if (at != null)
                this.CreateIndex = at;

            string dialect = GetParameter("sqlDialect", false);
            if (dialect == null)
                dialect = "microsoft";

            this.DisableUpdateBatch = true;

            switch (dialect)
            {
                default:
                case "msde":
                case "mssql":
                case "microsoft":
                    this.SqlBuilder = new SqlServerBuilder();
                    this.DisableUpdateBatch = false;
                    break;

                case "postgres":
                case "postgresql":
                    this.SqlBuilder = new PostgreSqlBuilder();
                    break;

                case "mysql":
                case "mysql4":
                    this.SqlBuilder = new MySqlBuilder();
                    break;

                case "oracle":
                    this.SqlBuilder = new OracleBuilder();
                    break;
            }

            if (GetParameter("useSafeLiterals", false) == "false")
                this.SqlBuilder.UseSafeLiterals = false;

            if (GetParameter("indentQueries", false) == "true")
                this.IndentQueries = true;

            if (GetParameter("disableUpdateBatch", false) == "true")
                this.DisableUpdateBatch = true;

            string connectionTypeName = GetParameter("connectionType", false);
            if (connectionTypeName == null)
                connectionTypeName = "sqlclient";

            switch (connectionTypeName)
            {
                case "sqlclient":
                    ConnectionType = typeof(System.Data.SqlClient.SqlConnection);
                    break;

                default:
                    ConnectionType = Type.GetType(connectionTypeName);
                    break;
            }

            ConnectionString = GetParameter("connectionString", false);
        }

        public SqlDataSource(Sooda.Schema.DataSourceInfo dataSourceInfo) : this(dataSourceInfo.Name)
        {
        }

        public override IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set { _isolationLevel = value; }
        }

        protected virtual void BeginTransaction()
        {
            Transaction = Connection.BeginTransaction(IsolationLevel);
        }

        public override void Open()
        {
            if (ConnectionString == null)
                throw new SoodaDatabaseException("connectionString parameter not defined for datasource: " + Name);
            if (ConnectionType == null)
                throw new SoodaDatabaseException("connectionType parameter not defined for datasource: " + Name);
            string stries = SoodaConfig.GetString("sooda.connectionopenretries", "2");
            int tries;
            try
            {
                tries = Convert.ToInt32(stries);
            }
            catch
            {
                tries = 2;
            }
            int maxtries = tries;
            while(tries > 0)
            {
                try
                {
                    Connection = (IDbConnection)Activator.CreateInstance(ConnectionType, new object[] { ConnectionString });
                    OwnConnection = true;
                    Connection.Open();
                    if (!DisableTransactions)
                    {
                        BeginTransaction();
                        if (this.SqlBuilder is OracleBuilder && SoodaConfig.GetString("sooda.oracleClientAutoCommitBugWorkaround", "false") == "true")
                        {
                            // http://social.msdn.microsoft.com/forums/en-US/adodotnetdataproviders/thread/d4834ce2-482f-40ec-ad90-c3f9c9c4d4b1/
                            // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=351746
                            Connection.GetType().GetProperty("TransactionState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(Connection, 1, null);
                        }
                    }
                    tries = 0;
                }
                catch(Exception e)
                {
                    tries--;
                    logger.Warn("Exception on Open#{0}: {1}", maxtries-tries, e);
                    if (tries == 0 || SqlBuilder.IsFatalException(Connection, e))
                        throw e;
                }
            }
        }

        public override bool IsOpen
        {
            get { return Connection != null && Connection.State == ConnectionState.Open; }
        }

        public override void Rollback()
        {
            if (OwnConnection && !DisableTransactions)
            {
                if (Transaction != null)
                {
                    Transaction.Rollback();
                    Transaction.Dispose();
                    Transaction = null;
                }
                BeginTransaction();
            }
        }

        public override void Commit()
        {
            if (OwnConnection && !DisableTransactions)
            {
                if (Transaction != null)
                {
                    Transaction.Commit();
                    Transaction.Dispose();
                    Transaction = null;
                }
                BeginTransaction();
            }
        }

        public override void Close()
        {
            if (OwnConnection)
            {
                if (!DisableTransactions && Transaction != null)
                {
                    Transaction.Rollback();
                    Transaction.Dispose();
                    Transaction = null;
                }
                if (Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        public override void BeginSaveChanges()
        {
            _updateCommand = Connection.CreateCommand();
            try
            {
                _updateCommand.CommandTimeout = CommandTimeout;
            }
            catch (NotSupportedException e)
            {
                logger.Debug("CommandTimeout not supported. {0}", e.Message);
            }
            if (Transaction != null)
                _updateCommand.Transaction = this.Transaction;
            _updateCommand.CommandText = "";
        }

        public override void FinishSaveChanges()
        {
            FlushUpdateCommand(true);
            _updateCommand = null;
        }

        private void FlushUpdateCommand(bool final)
        {
            bool doExecute = true;

            if (!DisableUpdateBatch)
            {
                if (_updateCommand.Parameters.Count < 100)
                    doExecute = false;
            }

            if (final || doExecute)
            {
                if (_updateCommand.CommandText != "")
                {
                    TimedExecuteNonQuery(_updateCommand);
                    _updateCommand.Parameters.Clear();
                    _updateCommand.CommandText = "";
                }
            }
        }

        void DoDeletes(SoodaObject obj)
        {
            ClassInfo ci = obj.GetClassInfo();
            object[] queryParams = new object[ci.GetPrimaryKeyFields().Length];
            for (int i = 0; i < queryParams.Length; ++i)
                queryParams[i] = SoodaTuple.GetValue(obj.GetPrimaryKeyValue(), i);

            for (int i = ci.UnifiedTables.Count - 1; i >= 0; --i)
            {
                TableInfo table = ci.UnifiedTables[i];

                StringBuilder query = new StringBuilder();
                query.Append("delete from ");
                query.Append(table.DBTableName);
                query.Append(" where ");
                int pos = 0;
                foreach (FieldInfo fi in ci.GetPrimaryKeyFields())
                {
                    if (pos > 0)
                        query.Append(" and ");
                    query.Append('(');
                    query.Append(fi.DBColumnName);
                    query.Append(" = {");
                    query.Append(pos++);
                    query.Append(':');
                    query.Append(fi.DataType);
                    query.Append("})");
                }

                SqlBuilder.BuildCommandWithParameters(_updateCommand, true, query.ToString(), queryParams, true);
                FlushUpdateCommand(false);
            }
        }

        public override void SaveObjectChanges(SoodaObject obj, bool isPrecommit)
        {
            if (obj.IsMarkedForDelete())
            {
                DoDeletes(obj);
                return;
            }
            if (obj.IsInsertMode() && !obj.InsertedIntoDatabase)
            {
                DoInserts(obj, isPrecommit);
                obj.InsertedIntoDatabase = true;
            }
            else
            {
                DoUpdates(obj);
            }
        }

        public override IDataReader LoadObjectTable(SoodaObject obj, object keyVal, int tableNumber, out TableInfo[] loadedTables)
        {
            ClassInfo classInfo = obj.GetClassInfo();
            IDbCommand cmd = Connection.CreateCommand();
            try
            {
                cmd.CommandTimeout = CommandTimeout;
            }
            catch(NotSupportedException e)
            {
                logger.Debug("CommandTimeout not supported. {0}", e.Message);
            }

            if (Transaction != null)
                cmd.Transaction = this.Transaction;

            SqlBuilder.BuildCommandWithParameters(cmd, false, GetLoadingSelectStatement(classInfo, classInfo.UnifiedTables[tableNumber], out loadedTables), SoodaTuple.GetValuesArray(keyVal), false);
            IDataReader reader = TimedExecuteReader(cmd);
            if (reader.Read())
                return reader;
            else
            {
                reader.Dispose();
                return null;
            }
        }

        public override IDataReader LoadObject(SoodaObject obj, object keyVal, out TableInfo[] loadedTables)
        {
            return LoadObjectTable(obj, keyVal, 0, out loadedTables);
        }

        public override void MakeTuple(string tableName, string leftColumnName, string rightColumnName, object leftVal, object rightVal, int mode)
        {
            object[] parameters = new object[] { leftVal, rightVal };
            string query = "delete from " + tableName + " where " + leftColumnName + "={0} and " + rightColumnName + "={1}";
            SqlBuilder.BuildCommandWithParameters(_updateCommand, true, query, parameters, false);
            FlushUpdateCommand(false);

            if (mode == 1)
            {
                query = "insert into " + tableName + "(" + leftColumnName + "," + rightColumnName + ") values({0},{1})";
                SqlBuilder.BuildCommandWithParameters(_updateCommand, true, query, parameters, false);
                FlushUpdateCommand(false);
            }
        }

        private string SoqlToSql(SoqlQueryExpression queryExpression, SchemaInfo schemaInfo, bool generateColumnAliases)
        {
            StringWriter sw = new StringWriter();
            SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, schemaInfo, SqlBuilder);
            converter.IndentOutput = this.IndentQueries;
            converter.GenerateColumnAliases = generateColumnAliases;
            converter.UpperLike = this.UpperLike;
            //logger.Trace("Converting {0}", queryExpression);
            converter.ConvertQuery(queryExpression);
            string query = sw.ToString();
            //logger.Trace("Converted as {0}", query);
            return query;
        }

        public override IDataReader LoadMatchingPrimaryKeys(SchemaInfo schemaInfo, ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int startIdx, int pageCount)
        {
            try
            {
                SoqlQueryExpression queryExpression = new SoqlQueryExpression();
                foreach (FieldInfo fi in classInfo.GetPrimaryKeyFields())
                {
                    queryExpression.SelectExpressions.Add(new SoqlPathExpression(fi.Name));
                    queryExpression.SelectAliases.Add("");

                }
                if (schemaInfo.GetSubclasses(classInfo).Count > 0)
                {
                    queryExpression.SelectExpressions.Add(new SoqlPathExpression(classInfo.SubclassSelectorField.Name));
                    queryExpression.SelectAliases.Add("");
                }
                queryExpression.StartIdx = startIdx;
                queryExpression.PageCount = pageCount;
                queryExpression.From.Add(classInfo.Name);
                queryExpression.FromAliases.Add("");
                if (whereClause != null && whereClause.WhereExpression != null)
                {
                    queryExpression.WhereClause = whereClause.WhereExpression;
                }

                if (orderBy != null)
                {
                    queryExpression.SetOrderBy(orderBy);
                }

                string query = SoqlToSql(queryExpression, schemaInfo, false);

                IDbCommand cmd = Connection.CreateCommand();
                try
                {
                    cmd.CommandTimeout = CommandTimeout;
                }
                catch (NotSupportedException e)
                {
                    logger.Debug("CommandTimeout not supported. {0}", e.Message);
                }

                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                SqlBuilder.BuildCommandWithParameters(cmd, false, query, whereClause.Parameters, false);

                return TimedExecuteReader(cmd);
            }
            catch (Exception ex)
            {
                logger.Error("Exception in LoadMatchingPrimaryKeys: {0}", ex);
                throw;
            }
        }

        public override IDataReader LoadObjectList(SchemaInfo schemaInfo, ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int startIdx, int pageCount, SoodaSnapshotOptions options, out TableInfo[] tables)
        {
            try
            {
                Queue<_QueueItem> queue = new Queue<_QueueItem>();

                List<TableInfo> tablesArrayList = new List<TableInfo>(classInfo.UnifiedTables.Count);
                SoqlQueryExpression queryExpression = new SoqlQueryExpression();
                queryExpression.StartIdx = startIdx;
                queryExpression.PageCount = pageCount;
                queryExpression.From.Add(classInfo.Name);
                queryExpression.FromAliases.Add("");
                foreach (TableInfo ti in classInfo.UnifiedTables)
                {
                    tablesArrayList.Add(ti);
                    foreach (FieldInfo fi in ti.Fields)
                    {
                        SoqlPathExpression pathExpr = new SoqlPathExpression(fi.Name);
                        queryExpression.SelectExpressions.Add(pathExpr);
                        queryExpression.SelectAliases.Add("");

                        if (fi.ReferencedClass != null && fi.PrefetchLevel > 0 && ((options & SoodaSnapshotOptions.PrefetchRelated) != 0))
                        {
                            _QueueItem item = new _QueueItem();
                            item.classInfo = fi.ReferencedClass;
                            item.level = fi.PrefetchLevel;
                            item.prefix = pathExpr;
                            queue.Enqueue(item);
                        }
                    }
                }

                while (queue.Count > 0)
                {
                    _QueueItem it = queue.Dequeue();

                    foreach (TableInfo ti in it.classInfo.UnifiedTables)
                    {
                        tablesArrayList.Add(ti);

                        foreach (FieldInfo fi in ti.Fields)
                        {
                            // TODO - this relies on the fact that path expressions
                            // are never reconstructed or broken. We simply share previous prefix
                            // perhaps it's cleaner to Clone() the expression here

                            SoqlPathExpression extendedExpression = new SoqlPathExpression(it.prefix, fi.Name);

                            queryExpression.SelectExpressions.Add(extendedExpression);
                            queryExpression.SelectAliases.Add("");

                            if (it.level >= 1 && fi.PrefetchLevel > 0 && fi.ReferencedClass != null)
                            {
                                _QueueItem newItem = new _QueueItem();
                                newItem.classInfo = fi.ReferencedClass;
                                newItem.prefix = extendedExpression;
                                newItem.level = it.level - 1;
                                queue.Enqueue(newItem);
                            }
                        }
                    }
                }

                if (whereClause != null && whereClause.WhereExpression != null)
                {
                    queryExpression.WhereClause = whereClause.WhereExpression;
                }

                if (orderBy != null)
                {
                    queryExpression.SetOrderBy(orderBy);
                }

                string query = SoqlToSql(queryExpression, schemaInfo, false);

                IDbCommand cmd = Connection.CreateCommand();

                try
                {
                    cmd.CommandTimeout = CommandTimeout;
                }
                catch (NotSupportedException e)
                {
                    logger.Debug("CommandTimeout not supported. {0}", e.Message);
                }

                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                SqlBuilder.BuildCommandWithParameters(cmd, false, query, whereClause.Parameters, false);

                tables = tablesArrayList.ToArray();
                return TimedExecuteReader(cmd);
            }
            catch (Exception ex)
            {
                logger.Error("Exception in LoadObjectList: {0}", ex);
                throw;
            }
        }

        public override IDataReader ExecuteQuery(Sooda.QL.SoqlQueryExpression query, SchemaInfo schema, object[] parameters)
        {
            try
            {
                string queryText = SoqlToSql(query, schema, false);
                return ExecuteRawQuery(queryText, parameters);
            }
            catch (Exception ex)
            {
                logger.Error("Exception in ExecuteQuery: {0}", ex);
                throw;
            }
        }

        public override IDataReader ExecuteRawQuery(string queryText, object[] parameters)
        {
            try
            {
                IDbCommand cmd = Connection.CreateCommand();
                try
                {
                    cmd.CommandTimeout = CommandTimeout;
                }
                catch (NotSupportedException e)
                {
                    logger.Debug("CommandTimeout not supported. {0}", e.Message);
                }

                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                SqlBuilder.BuildCommandWithParameters(cmd, false, queryText, parameters, true);
                return TimedExecuteReader(cmd);
            }
            catch (Exception ex)
            {
                logger.Error("Exception in ExecuteRawQuery: {0}", ex);
                throw;
            }
        }

        public override int ExecuteNonQuery(string queryText, params object[] parameters)
        {
            try
            {
                using (IDbCommand cmd = Connection.CreateCommand())
                {
                    try
                    {
                        cmd.CommandTimeout = CommandTimeout;
                    }
                    catch (NotSupportedException e)
                    {
                        logger.Debug("CommandTimeout not supported. {0}", e.Message);
                    }
                    if (Transaction != null)
                        cmd.Transaction = this.Transaction;

                    SqlBuilder.BuildCommandWithParameters(cmd, false, queryText, parameters, true);
                    return TimedExecuteNonQuery(cmd);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception in ExecuteNonQuery: {0}", ex);
                throw;
            }
        }

        public override IDataReader LoadRefObjectList(SchemaInfo schema, RelationInfo relationInfo, int masterColumn, object masterValue, out TableInfo[] tables)
        {
            try
            {
                if (masterColumn == 0)
                    tables = relationInfo.GetRef1ClassInfo().UnifiedTables[0].ArraySingleton;
                else
                    tables = relationInfo.GetRef2ClassInfo().UnifiedTables[0].ArraySingleton;

                string query = GetLoadRefObjectSelectStatement(relationInfo, masterColumn);

                IDbCommand cmd = Connection.CreateCommand();
                try
                {
                    cmd.CommandTimeout = CommandTimeout;
                }
                catch (NotSupportedException e)
                {
                    logger.Debug("CommandTimeout not supported. {0}", e.Message);
                }

                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                SqlBuilder.BuildCommandWithParameters(cmd, false, query, new object[] { masterValue }, false);
                return TimedExecuteReader(cmd);
            }
            catch (Exception ex)
            {
                logger.Error("Exception in LoadRefObjectList: {0}", ex);
                throw;
            }
        }

        void DoInserts(SoodaObject obj, bool isPrecommit)
        {
            foreach (TableInfo table in obj.GetClassInfo().DatabaseTables)
            {
                DoInsertsForTable(obj, table, isPrecommit);
            }
        }

        void DoInsertsForTable(SoodaObject obj, TableInfo table, bool isPrecommit)
        {
            StringBuilder builder = new StringBuilder(500);
            builder.Append("insert into ");
            builder.Append(table.DBTableName);
            builder.Append('(');

            ArrayList par = new ArrayList();

            bool comma = false;
            for (int i = 0; i < table.Fields.Count; ++i)
            {
                if (table.Fields[i].ReadOnly)
                    continue;
                if (comma)
                    builder.Append(',');
                comma = true;
                builder.Append(table.Fields[i].DBColumnName);
            }

            builder.Append(") values (");
            comma = false;
            for (int i = 0; i < table.Fields.Count; ++i)
            {
                if (table.Fields[i].ReadOnly)
                    continue;
                if (comma)
                    builder.Append(',');
                comma = true;

                object val = obj.GetFieldValue(table.Fields[i].ClassUnifiedOrdinal);
                if (!table.Fields[i].IsNullable && SqlBuilder.IsNullValue(val, table.Fields[i]))
                {
                    if (!isPrecommit)
                        throw new SoodaDatabaseException(obj.GetObjectKeyString() + "." + table.Fields[i].Name + " cannot be null on commit.");
                    val = table.Fields[i].PrecommitTypedValue;
                    if (val == null)
                        throw new SoodaDatabaseException(obj.GetObjectKeyString() + "." + table.Fields[i].Name + " is null on precommit and no 'precommitValue' has been defined for it.");
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Using precommit value of {0} for {1}.{2}", val, table.NameToken, table.Fields[i].Name);
                    }
                }

                builder.Append('{');
                int fieldnum = par.Add(val);
                builder.Append(fieldnum);
                builder.Append(':');
                builder.Append(table.Fields[i].DataType);
                builder.Append('}');
            }
            builder.Append(')');
            SqlBuilder.BuildCommandWithParameters(_updateCommand, true, builder.ToString(), par.ToArray(), false);
            FlushUpdateCommand(false);
        }

        void DoUpdates(SoodaObject obj)
        {
            foreach (TableInfo table in obj.GetClassInfo().DatabaseTables)
            {
                DoUpdatesForTable(obj, table);
            }
        }

        void DoUpdatesForTable(SoodaObject obj, TableInfo table)
        {
            StringBuilder builder = new StringBuilder(500);
            builder.Append("update ");
            builder.Append(table.DBTableName);
            builder.Append(" set ");

            ArrayList par = new ArrayList();
            bool anyChange = false;
            for (int i = 0; i < table.Fields.Count; ++i)
            {
                int fieldNumber = table.Fields[i].ClassUnifiedOrdinal;

                if (obj.IsFieldDirty(fieldNumber))
                {
                    if (anyChange)
                        builder.Append(", ");
                    builder.Append(table.Fields[i].DBColumnName);
                    builder.Append("={");
                    int fieldnum = par.Add(obj.GetFieldValue(fieldNumber));
                    builder.Append(fieldnum);
                    builder.Append(':');
                    builder.Append(table.Fields[i].DataType);
                    builder.Append('}');
                    anyChange = true;
                }
            }
            if (!anyChange)
                return;

            builder.Append(" where ");
            int pkordinal = 0;
            foreach (FieldInfo fi in table.Fields)
            {
                if (fi.IsPrimaryKey)
                {
                    if (pkordinal > 0)
                        builder.Append(" and ");
                    builder.Append('(');
                    builder.Append(fi.DBColumnName);
                    builder.Append("={");
                    builder.Append(par.Add(SoodaTuple.GetValue(obj.GetPrimaryKeyValue(), pkordinal)));
                    builder.Append(':');
                    builder.Append(fi.DataType);
                    builder.Append("})");
                    pkordinal++;
                }
            }
            SqlBuilder.BuildCommandWithParameters(_updateCommand, true, builder.ToString(), par.ToArray(), false);
            FlushUpdateCommand(false);
        }

        string StripWhitespace(string s)
        {
            if (!StripWhitespaceInLogs)
                return s;

            return s.Replace("\n", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
        }

        private string LogCommand(IDbCommand cmd)
        {
            StringBuilder txt = new StringBuilder();
            if (IndentQueries)
                txt.Append("\n");
            txt.Append(StripWhitespace(cmd.CommandText));
            if (cmd.Parameters.Count > 0)
            {
                txt.Append(" [");
                foreach (IDataParameter par in cmd.Parameters)
                {
                    txt.AppendFormat(" {0}:{1}={2}", par.ParameterName, par.DbType, par.Value);
                }
                txt.AppendFormat(" ]", Connection.GetHashCode());
            }
            // txt.AppendFormat(" DataSource: {0}", this.Name);
            return txt.ToString();
        }

        public void ExecuteRaw(string sql)
        {
            using (IDbCommand cmd = Connection.CreateCommand())
            {
                try
                {
                    cmd.CommandTimeout = CommandTimeout;
                }
                catch (NotSupportedException e)
                {
                    logger.Debug("CommandTimeout not supported. {0}", e.Message);
                }
                if (Transaction != null)
                    cmd.Transaction = this.Transaction;

                cmd.CommandText = sql;
                TimedExecuteNonQuery(cmd);
            }
        }

        class TableLoadingCache
        {
            public readonly string SelectStatement;
            public readonly TableInfo[] LoadedTables;
            public TableLoadingCache(string selectStatement, TableInfo[] loadedTables)
            {
                SelectStatement = selectStatement;
                LoadedTables = loadedTables;
            }
        }

        Dictionary<TableInfo, TableLoadingCache> tableLoadingCache = new Dictionary<TableInfo, TableLoadingCache>();
        Dictionary<RelationInfo, string>[] cacheLoadRefObjectSelectStatement = new Dictionary<RelationInfo, string>[] { new Dictionary<RelationInfo, string>(), new Dictionary<RelationInfo, string>() };

        class _QueueItem
        {
            public ClassInfo classInfo;
            public SoqlPathExpression prefix;
            public int level;
        }

        private string GetLoadingSelectStatement(ClassInfo classInfo, TableInfo tableInfo, out TableInfo[] loadedTables)
        {
            TableLoadingCache cache;
            if (tableLoadingCache.TryGetValue(tableInfo, out cache))
            {
                loadedTables = cache.LoadedTables;
                return cache.SelectStatement;
            }

            Queue<_QueueItem> queue = new Queue<_QueueItem>();
            List<TableInfo> additional = new List<TableInfo>();
            additional.Add(tableInfo);

            SoqlQueryExpression queryExpression = new SoqlQueryExpression();
            queryExpression.From.Add(classInfo.Name);
            queryExpression.FromAliases.Add("");

            foreach (FieldInfo fi in tableInfo.Fields)
            {
                SoqlPathExpression pathExpr = new SoqlPathExpression(fi.Name);
                queryExpression.SelectExpressions.Add(pathExpr);
                queryExpression.SelectAliases.Add("");

                if (fi.ReferencedClass != null && fi.PrefetchLevel > 0)
                {
                    _QueueItem item = new _QueueItem();
                    item.classInfo = fi.ReferencedClass;
                    item.level = fi.PrefetchLevel;
                    item.prefix = pathExpr;
                    queue.Enqueue(item);
                }
            }

            // TODO - add prefetching
            while (queue.Count > 0)
            {
                _QueueItem it = queue.Dequeue();

                foreach (TableInfo ti in it.classInfo.UnifiedTables)
                {
                    additional.Add(ti);

                    foreach (FieldInfo fi in ti.Fields)
                    {
                        // TODO - this relies on the fact that path expressions
                        // are never reconstructed or broken. We simply share previous prefix
                        // perhaps it's cleaner to Clone() the expression here

                        SoqlPathExpression extendedExpression = new SoqlPathExpression(it.prefix, fi.Name);

                        queryExpression.SelectExpressions.Add(extendedExpression);
                        queryExpression.SelectAliases.Add("");

                        if (it.level >= 1 && fi.PrefetchLevel > 0 && fi.ReferencedClass != null)
                        {
                            _QueueItem newItem = new _QueueItem();
                            newItem.classInfo = fi.ReferencedClass;
                            newItem.prefix = extendedExpression;
                            newItem.level = it.level - 1;
                            queue.Enqueue(newItem);
                        }
                    }
                }
            }

            queryExpression.WhereClause = null;

            int parameterPos = 0;

            foreach (FieldInfo fi in tableInfo.Fields)
            {
                if (fi.IsPrimaryKey)
                {
                    SoqlBooleanRelationalExpression expr = Soql.FieldEqualsParam(fi.Name, parameterPos);

                    if (parameterPos == 0)
                    {
                        queryExpression.WhereClause = expr;
                    }
                    else
                    {
                        queryExpression.WhereClause = new SoqlBooleanAndExpression(queryExpression.WhereClause, expr);
                    }
                    parameterPos++;
                }
            }

            string query = SoqlToSql(queryExpression, tableInfo.OwnerClass.Schema, false);

            // logger.Debug("Loading statement for table {0}: {1}", tableInfo.NameToken, query);

            loadedTables = additional.ToArray();
            tableLoadingCache[tableInfo] = new TableLoadingCache(query, loadedTables);
            return query;
        }

        private string GetLoadRefObjectSelectStatement(RelationInfo relationInfo, int masterColumn)
        {
            string query;
            if (cacheLoadRefObjectSelectStatement[masterColumn].TryGetValue(relationInfo, out query))
                return query;
            string soqlQuery = String.Format("select mt.{0}.* from {2} mt where mt.{1} = {{0}}",
                relationInfo.Table.Fields[masterColumn].Name,
                relationInfo.Table.Fields[1 - masterColumn].Name,
                relationInfo.Name);
            query = SoqlToSql(SoqlParser.ParseQuery(soqlQuery), relationInfo.Schema, false);
            cacheLoadRefObjectSelectStatement[masterColumn][relationInfo] = query;
            return query;
        }

        private void UnifyTable(Dictionary<string, TableInfo> tables, TableInfo ti, bool isInherited)
        {
            TableInfo baseTable;
            if (!tables.TryGetValue(ti.DBTableName, out baseTable))
            {
                baseTable = new TableInfo();
                baseTable.DBTableName = ti.DBTableName;

                tables[ti.DBTableName] = baseTable;
                isInherited = false;
            }

            foreach (FieldInfo fi in ti.Fields)
            {
                bool found = false;

                foreach (FieldInfo fi0 in baseTable.Fields)
                {
                    if (fi0.Name == fi.Name)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    baseTable.Fields.Add(fi);
                    if (isInherited)
                        fi.IsNullable = true;
                }
            }
        }

        private IDataReader TimedExecuteReader(IDbCommand cmd)
        {
            StopWatch sw = StopWatch.Create();

            try
            {
                sw.Start();
                IDataReader retval = cmd.ExecuteReader(CmdBehavior);
                sw.Stop();
                return retval;
            }
            catch (Exception ex)
            {
                sw.Stop();
                if (sqllogger.IsErrorEnabled)
                    sqllogger.Error("Error while executing: {0}\nException: {1}", LogCommand(cmd), ex);
                throw ex;
            }
            finally
            {
                double timeInSeconds = sw.Seconds;

                if (Statistics != null)
                    Statistics.RegisterQueryTime(timeInSeconds);

                SoodaStatistics.Global.RegisterQueryTime(timeInSeconds);
                if (timeInSeconds > QueryTimeTraceWarn && sqllogger.IsWarnEnabled)
                {
                    sqllogger.Warn("Query time: {0} ms. {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
                else if (timeInSeconds > QueryTimeTraceInfo && sqllogger.IsInfoEnabled)
                {
                    sqllogger.Info("Query time: {0} ms: {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
                else if (sqllogger.IsTraceEnabled)
                {
                    sqllogger.Trace("Query time: {0} ms. {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
            }
        }

        private int TimedExecuteNonQuery(IDbCommand cmd)
        {
            StopWatch sw = StopWatch.Create();

            try
            {
                sw.Start();
                int retval = cmd.ExecuteNonQuery();
                sw.Stop();
                return retval;
            }
            catch (Exception ex)
            {
                sw.Stop();
                if (sqllogger.IsErrorEnabled)
                {
                    sqllogger.Error("Error while executing: {0}\nException: {1}", LogCommand(cmd), ex);
                }
                throw ex;
            }
            finally
            {
                double timeInSeconds = sw.Seconds;

                if (Statistics != null)
                    Statistics.RegisterQueryTime(timeInSeconds);

                SoodaStatistics.Global.RegisterQueryTime(timeInSeconds);
                if (timeInSeconds > QueryTimeTraceWarn && sqllogger.IsWarnEnabled)
                {
                    sqllogger.Warn("Non-query time: {0} ms. {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
                else if (timeInSeconds > QueryTimeTraceInfo && sqllogger.IsInfoEnabled)
                {
                    sqllogger.Info("Non-query time: {0} ms. {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
                else if (sqllogger.IsTraceEnabled)
                {
                    sqllogger.Trace("Non-query time: {0} ms.{1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
            }
        }

        public virtual void GenerateDdlForSchema(SchemaInfo schema, TextWriter tw)
        {
            Dictionary<string, TableInfo> tables = new Dictionary<string, TableInfo>();
            Dictionary<string, string> processed = new Dictionary<string, string>();

            while (processed.Count < schema.Classes.Count)
            {
                foreach (ClassInfo ci in schema.Classes)
                {
                    if (!processed.ContainsKey(ci.Name))
                    {
                        bool isInherited = ci.InheritsFromClass != null;
                        if (!isInherited || processed.ContainsKey(ci.InheritsFromClass.Name))
                        {
                            foreach (TableInfo ti in ci.UnifiedTables)
                            {
                                UnifyTable(tables, ti, isInherited);
                            }
                            processed.Add(ci.Name, ci.Name);
                        }
                    }
                }
            }

            foreach (RelationInfo ri in schema.Relations)
            {
                UnifyTable(tables, ri.Table, false);
            }

            List<string> names = new List<string>();

            foreach (TableInfo ti in tables.Values)
            {
                names.Add(ti.DBTableName);
            }

            names.Sort();

            foreach (string s in names)
            {
                tw.WriteLine("--- table {0}", s);
                SqlBuilder.GenerateCreateTable(tw, tables[s], this.CreateTable, null);
            }

            foreach (string s in names)
            {
                SqlBuilder.GeneratePrimaryKey(tw, tables[s], this.CreateIndex, null);
            }

            foreach (string s in names)
            {
                SqlBuilder.GenerateForeignKeys(tw, tables[s], null);
            }

            foreach (string s in names)
            {
                SqlBuilder.GenerateIndices(tw, tables[s], this.CreateIndex, null);
            }
        }
    }
}
