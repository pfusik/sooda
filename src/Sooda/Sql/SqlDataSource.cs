// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using Sooda.Schema;
using Sooda.QL;
using Sooda.Utils;

using Sooda.Logging;

namespace Sooda.Sql
{
    public class SqlDataSource : Sooda.SoodaDataSource
    {
        static Logger logger = LogManager.GetLogger("Sooda.SqlDataSource");
        static Logger sqllogger = LogManager.GetLogger("Sooda.SQL");

        public IDbConnection Connection;
        public IDbTransaction Transaction;
        public ISqlBuilder SqlBuilder = null;
        public bool DisableTransactions = false;
        private IDbCommand _updateCommand = null;
        public bool SupportsUpdateBatch = false;
        public bool StripWhitespaceInLogs = false;
        public bool IndentQueries = false;
        private IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;
        private double queryTimeTraceWarn = 10.0;
        private double queryTimeTraceInfo = 2.0;
        public int CommandTimeout = 30;

        public SqlDataSource(string name) : base(name)
        {
            string s = GetParameter("queryTimeTraceInfo", false);
            if (s != null && s.Length > 0) queryTimeTraceInfo = Convert.ToDouble(s);
            s = GetParameter("queryTimeTraceWarn", false);
            if (s != null && s.Length > 0) queryTimeTraceWarn = Convert.ToDouble(s);
        }

        public SqlDataSource(Sooda.Schema.DataSourceInfo dataSourceInfo) : this(dataSourceInfo.Name)
        {
        }

        public override IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set { _isolationLevel = value; }
        }

        public override void Open()
        {
            string connectionTypeName = GetParameter("connectionType", false);
            if (connectionTypeName == null)
                connectionTypeName = "sqlclient";

            Type connectionType = null;
            switch (connectionTypeName)
            {
                case "sqlclient":
                    connectionType = typeof(System.Data.SqlClient.SqlConnection);
                    break;

                default:
                    connectionType = Type.GetType(connectionTypeName);
                    break;
            }
            
            string connectionString = GetParameter("connectionString", true);
            Connection = (IDbConnection)Activator.CreateInstance(connectionType, new object[] { connectionString });

            if (GetParameter("disableTransactions", false) == "true")
                this.DisableTransactions = true;

            if (GetParameter("stripWhitespaceInLogs", false) == "true")
                this.StripWhitespaceInLogs = true;

            if (GetParameter("indentQueries", false) == "true")
                this.IndentQueries = true;

            string s = GetParameter("commandTimeout", false);
            if (s != null)
                CommandTimeout = Convert.ToInt32(s);

            string dialect = GetParameter("sqlDialect", false);
            if (dialect == null)
                dialect = "microsoft";

            switch (dialect)
            {
                default:
                case "msde":
                case "mssql":
                case "microsoft":
                    this.SqlBuilder = new SqlServerBuilder();
                    this.SupportsUpdateBatch = true;
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
                this.SupportsUpdateBatch = false;

            Connection.Open();
            if (!DisableTransactions)
            {
                Transaction = Connection.BeginTransaction(IsolationLevel);
            };
        }

        public override bool IsOpen
        {
            get { return (Connection != null) && (Connection.State == ConnectionState.Open); }
        }

        public override void Rollback()
        {
            if (!DisableTransactions)
            {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = Connection.BeginTransaction(IsolationLevel);
            };
        }

        public override void Commit()
        {
            if (!DisableTransactions)
            {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = Connection.BeginTransaction(IsolationLevel);
            };
        }

        public override void Close()
        {
            if (!DisableTransactions && Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
            };
            if (Connection != null)
            {
                Connection.Dispose();
            }
        }

        public override void BeginSaveChanges()
        {
            _updateCommand = Connection.CreateCommand();
            _updateCommand.CommandTimeout = CommandTimeout;
            if (!DisableTransactions)
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

            if (SupportsUpdateBatch)
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
                    query.Append("(");
                    query.Append(fi.DBColumnName);
                    query.Append(" = {");
                    query.Append(pos++);
                    query.Append(':');
                    query.Append(fi.DataType);
                    query.Append("}");
                    query.Append(")");
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
            cmd.CommandTimeout = CommandTimeout;

            if (!DisableTransactions)
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
                StringBuilder query2 = new StringBuilder();

                query2.Append("insert into " + tableName + "(" + leftColumnName + "," + rightColumnName + ") values({0},{1})");
                SqlBuilder.BuildCommandWithParameters(_updateCommand, true, query2.ToString(), parameters, false);
                FlushUpdateCommand(false);
            }
        }

        public override IDataReader LoadMatchingPrimaryKeys(SchemaInfo schemaInfo, ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount)
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
                queryExpression.TopCount = topCount;
                queryExpression.From.Add(classInfo.Name);
                queryExpression.FromAliases.Add("obj");
                if (whereClause != null && whereClause.WhereExpression != null)
                {
                    queryExpression.WhereClause = whereClause.WhereExpression;
                }

                if (orderBy != null)
                {
                    queryExpression.SetOrderBy(orderBy);
                }

                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, schemaInfo, SqlBuilder);
                converter.IndentOutput = this.IndentQueries;
                converter.GenerateColumnAliases = false;
                //logger.Trace("Converting {0}", queryExpression);
                converter.ConvertQuery(queryExpression);
                string query = sw.ToString();

                //logger.Trace("Converted as {0}", query);

                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandTimeout = CommandTimeout;

                if (!DisableTransactions)
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

        public override IDataReader LoadObjectList(SchemaInfo schemaInfo, ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount, SoodaSnapshotOptions options, out TableInfo[] tables)
        {
            try
            {
                Queue queue = new Queue();

                ArrayList tablesArrayList = new ArrayList(classInfo.UnifiedTables.Count);
                SoqlQueryExpression queryExpression = new SoqlQueryExpression();
                queryExpression.TopCount = topCount;
                queryExpression.From.Add(classInfo.Name);
                queryExpression.FromAliases.Add("obj");
                foreach (TableInfo ti in classInfo.UnifiedTables)
                {
                    tablesArrayList.Add(ti);
                    foreach (FieldInfo fi in ti.Fields)
                    {
                        SoqlPathExpression pathExpr = new SoqlPathExpression("obj", fi.Name);
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
                    _QueueItem it = (_QueueItem)queue.Dequeue();

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

                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, schemaInfo, SqlBuilder);
                converter.IndentOutput = this.IndentQueries;
                converter.GenerateColumnAliases = false;
                //logger.Trace("Converting {0}", queryExpression);
                converter.ConvertQuery(queryExpression);
                string query = sw.ToString();

                //logger.Trace("Converted as {0}", query);

                IDbCommand cmd = Connection.CreateCommand();
                cmd.CommandTimeout = CommandTimeout;

                if (!DisableTransactions)
                    cmd.Transaction = this.Transaction;

                SqlBuilder.BuildCommandWithParameters(cmd, false, query, whereClause.Parameters, false);

                tables = (TableInfo[])tablesArrayList.ToArray(typeof(TableInfo));
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
                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, schema, SqlBuilder);
                converter.IndentOutput = this.IndentQueries;
                converter.ConvertQuery(query);

                string queryText = sw.ToString();

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
                cmd.CommandTimeout = CommandTimeout;

                if (!DisableTransactions)
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

        public override int ExecuteNonQuery(string queryText, object[] parameters)
        {
            try
            {
                using (IDbCommand cmd = Connection.CreateCommand())
                {
                    cmd.CommandTimeout = CommandTimeout;
                    if (!DisableTransactions)
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
                cmd.CommandTimeout = CommandTimeout;

                if (!DisableTransactions)
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
            foreach (TableInfo table in obj.GetClassInfo().MergedTables)
            {
                DoInsertsForTable(obj, table, isPrecommit);
            }
        }

        void DoInsertsForTable(SoodaObject obj, TableInfo table, bool isPrecommit)
        {
            ClassInfo info = obj.GetClassInfo();
            StringBuilder builder = new StringBuilder(500);
            builder.Append("insert into ");
            builder.Append(table.DBTableName);
            builder.Append("(");

            ArrayList par = new ArrayList();

            for (int i = 0; i < table.Fields.Count; ++i)
            {
                if (i != 0)
                    builder.Append(",");
                builder.Append(table.Fields[i].DBColumnName);
            };

            builder.Append(") values (");
            for (int i = 0; i < table.Fields.Count; ++i)
            {
                if (i > 0)
                    builder.Append(",");

                object val = obj.GetFieldValue(table.Fields[i].ClassUnifiedOrdinal);
                if (val == null && !table.Fields[i].IsNullable)
                {
                    if (isPrecommit)
                    {
                        if (table.Fields[i].PrecommitTypedValue != null)
                        {
                            val = table.Fields[i].PrecommitTypedValue;
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("Using precommit value of {0} for {1}.{2}", val, table.NameToken, table.Fields[i].Name);
                            }
                        }
                        else
                        {
                            throw new SoodaDatabaseException(obj.GetObjectKeyString() + "." + table.Fields[i].Name + " is null on precommit and no 'precommitValue' has been defined for it.");
                        }
                    }
                    else
                        throw new SoodaDatabaseException(obj.GetObjectKeyString() + "." + table.Fields[i].Name + " cannot be null on commit.");
                }

                builder.Append('{');
                int fieldnum = par.Add(val);
                builder.Append(fieldnum);
                builder.Append(':');
                builder.Append(table.Fields[i].DataType);
                builder.Append('}');
            };
            builder.Append(")");
            SqlBuilder.BuildCommandWithParameters(_updateCommand, true, builder.ToString(), par.ToArray(), false);
            FlushUpdateCommand(false);
        }

        void DoUpdates(SoodaObject obj)
        {
            foreach (TableInfo table in obj.GetClassInfo().MergedTables)
            {
                DoUpdatesForTable(obj, table);
            }
        }

        void DoUpdatesForTable(SoodaObject obj, TableInfo table)
        {
            ClassInfo info = obj.GetClassInfo();
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
                    {
                        builder.Append(", ");
                    }
                    builder.Append(table.Fields[i].DBColumnName);
                    builder.Append("={");
                    int fieldnum = par.Add(obj.GetFieldValue(fieldNumber));
                    builder.Append(fieldnum);
                    builder.Append(':');
                    builder.Append(table.Fields[i].DataType);
                    builder.Append("}");
                    anyChange = true;
                };
            };
            builder.Append(" where ");
            int pkordinal = 0;
            foreach (FieldInfo fi in table.Fields)
            {
                if (fi.IsPrimaryKey)
                {
                    if (pkordinal > 0)
                    {
                        builder.Append(" and ");
                    }
                    builder.Append("(");
                    builder.Append(fi.DBColumnName);
                    builder.Append("={");
                    builder.Append(par.Add(SoodaTuple.GetValue(obj.GetPrimaryKeyValue(), pkordinal)));
                    builder.Append(':');
                    builder.Append(fi.DataType);
                    builder.Append('}');
                    builder.Append(")");
                    pkordinal++;
                }
            }
            if (anyChange)
            {
                SqlBuilder.BuildCommandWithParameters(_updateCommand, true, builder.ToString(), par.ToArray(), false);
                FlushUpdateCommand(false);
            }
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
                cmd.CommandTimeout = CommandTimeout;
                if (!DisableTransactions)
                    cmd.Transaction = this.Transaction;

                cmd.CommandText = sql;
                TimedExecuteNonQuery(cmd);
            }
        }

        private Hashtable cacheLoadingSelectStatement = new Hashtable();
        private Hashtable cacheLoadedTables = new Hashtable();
        private Hashtable[] cacheLoadRefObjectSelectStatement = new Hashtable[] { new Hashtable(), new Hashtable() };

        class _QueueItem
        {
            public ClassInfo classInfo;
            public SoqlPathExpression prefix;
            public int level;
        }

        private string GetLoadingSelectStatement(ClassInfo classInfo, TableInfo tableInfo, out TableInfo[] loadedTables)
        {
            if (!cacheLoadingSelectStatement.Contains(tableInfo))
            {
                Queue queue = new Queue();
                ArrayList additional = new ArrayList();
                additional.Add(tableInfo);

                SoqlQueryExpression queryExpression = new SoqlQueryExpression();
                queryExpression.From.Add(classInfo.Name);
                queryExpression.FromAliases.Add("obj");

                foreach (FieldInfo fi in tableInfo.Fields)
                {
                    SoqlPathExpression pathExpr = new SoqlPathExpression("obj", fi.Name);
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
                    _QueueItem it = (_QueueItem)queue.Dequeue();

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
                        SoqlBooleanRelationalExpression expr =
                            new SoqlBooleanRelationalExpression(
                                    new SoqlPathExpression("obj", fi.Name),
                                    new SoqlParameterLiteralExpression(parameterPos),
                                    SoqlRelationalOperator.Equal);

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

                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, tableInfo.OwnerClass.Schema, SqlBuilder);
                converter.IndentOutput = this.IndentQueries;
                converter.GenerateColumnAliases = false;
                converter.ConvertQuery(queryExpression);

                string query = sw.ToString();

                // logger.Debug("Loading statement for table {0}: {1}", tableInfo.NameToken, query);

                cacheLoadingSelectStatement[tableInfo] = query;
                cacheLoadedTables[tableInfo] = additional.ToArray(typeof(TableInfo));
            }
            loadedTables = (TableInfo[])cacheLoadedTables[tableInfo];
            return (string)cacheLoadingSelectStatement[tableInfo];
        }

        private string GetLoadRefObjectSelectStatement(RelationInfo relationInfo, int masterColumn)
        {
            if (!cacheLoadRefObjectSelectStatement[masterColumn].Contains(relationInfo))
            {
                string soqlQuery = "";

                soqlQuery = String.Format("select mt.{0}.* from {2} mt where mt.{1} = {{0}}",
                        relationInfo.Table.Fields[masterColumn].Name,
                        relationInfo.Table.Fields[1 - masterColumn].Name,
                        relationInfo.Name);

                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, relationInfo.Schema, SqlBuilder);
                converter.IndentOutput = this.IndentQueries;
                converter.GenerateColumnAliases = false;
                converter.ConvertQuery(SoqlParser.ParseQuery(soqlQuery));
                string sqlQuery = sw.ToString();

                cacheLoadRefObjectSelectStatement[masterColumn][relationInfo] = sqlQuery;
            };
            return (string)cacheLoadRefObjectSelectStatement[masterColumn][relationInfo];
        }

        private void UnifyTable(Hashtable tables, TableInfo ti)
        {
            TableInfo baseTable;

            baseTable = (TableInfo)tables[ti.DBTableName];
            if (baseTable == null)
            {
                baseTable = new TableInfo();
                baseTable.DBTableName = ti.DBTableName;

                tables[ti.DBTableName] = baseTable;
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
                }
            }
        }

        private IDataReader TimedExecuteReader(IDbCommand cmd)
        {
            StopWatch sw = StopWatch.Create();

            try
            {
                sw.Start();
                IDataReader retval = cmd.ExecuteReader();
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
                if (timeInSeconds > queryTimeTraceWarn && sqllogger.IsWarnEnabled)
                {
                    sqllogger.Warn("Query time: {0} ms. {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
                else if (timeInSeconds > queryTimeTraceInfo && sqllogger.IsInfoEnabled)
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
                if (timeInSeconds > queryTimeTraceWarn && sqllogger.IsWarnEnabled)
                {
                    sqllogger.Warn("Non-query time: {0} ms. {1}", Math.Round(timeInSeconds * 1000.0, 3), LogCommand(cmd));
                }
                else if (timeInSeconds > queryTimeTraceInfo && sqllogger.IsInfoEnabled)
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
            Hashtable tables = new Hashtable();

            foreach (ClassInfo ci in schema.Classes)
            {
                foreach (TableInfo ti in ci.UnifiedTables)
                {
                    UnifyTable(tables, ti);
                }
            }

            foreach (RelationInfo ri in schema.Relations)
            {
                UnifyTable(tables, ri.Table);
            }

            ArrayList names = new ArrayList();

            foreach (TableInfo ti in tables.Values)
            {
                names.Add(ti.DBTableName);
            }

            names.Sort();

            foreach (string s in names)
            {
                tw.WriteLine("--- table {0}", s);
                SqlBuilder.GenerateCreateTable(tw, (TableInfo)tables[s]);
            }

            foreach (string s in names)
            {
                SqlBuilder.GeneratePrimaryKey(tw, (TableInfo)tables[s]);
            }

            foreach (string s in names)
            {
                SqlBuilder.GenerateForeignKeys(tw, (TableInfo)tables[s]);
            }
        }
    }
}
