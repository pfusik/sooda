//
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission.
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
using System.Collections.Specialized;

using System.Xml;
using Sooda.Schema;
using Sooda;
using Sooda.QL;

namespace Sooda.Sql {
    public class SqlDataSource : Sooda.SoodaDataSource {
        static NLog.Logger logger = NLog.LogManager.GetLogger("Sooda.SqlDataSource");
        static NLog.Logger sqllogger = NLog.LogManager.GetLogger("Sooda.SQL");

        public IDbConnection Connection;
        public IDbTransaction Transaction;
        public ISqlBuilder SqlBuilder = null;
        public bool DisableTransactions = false;

        public SqlDataSource(string name) : base(name) {}

        public override void Open() {
            Type connectionType = Type.GetType(GetParameter("connectionType", true));
            string connectionString = GetParameter("connectionString", true);
            Connection = (IDbConnection)Activator.CreateInstance(connectionType, new object[] { connectionString });

            if (GetParameter("disableTransactions", false) != null)
                this.DisableTransactions = true;

            string dialect = GetParameter("sqlDialect", false);
            if (dialect == null)
                dialect = "microsoft";

            switch (dialect) {
            default:
            case "msde":
            case "mssql":
            case "microsoft":
                this.SqlBuilder = new SqlServerBuilder();
                break;

            case "postgres":
            case "postgresql":
                this.SqlBuilder = new PostgreSqlBuilder();
                break;

            case "mysql":
            case "mysql4":
                this.SqlBuilder = new MySqlBuilder();
                break;
            };

            Connection.Open();
            if (!DisableTransactions) {
                Transaction = Connection.BeginTransaction();
            };
        }

        public override void Rollback() {
            if (!DisableTransactions) {
                Transaction.Rollback();
                Transaction.Dispose();
                Transaction = Connection.BeginTransaction();
            };
        }

        public override void Commit() {
            if (!DisableTransactions) {
                Transaction.Commit();
                Transaction.Dispose();
                Transaction = Connection.BeginTransaction();
            };
        }

        public override void Close() {
            if (!DisableTransactions && Transaction != null) {
                Transaction.Rollback();
                Transaction.Dispose();
            };
            if (Connection != null) {
                Connection.Dispose();
            }
        }

        public override void DeleteObject(SoodaObject obj) {
            using (IDbCommand cmd = Connection.CreateCommand()) {
                if (!DisableTransactions)
                    cmd.Transaction = this.Transaction;

#warning FIX ME

                ClassInfo ci = obj.GetClassInfo();
                SqlBuilder.BuildCommandWithParameters(cmd, "delete from " + ci.UnifiedTables[0].DBTableName + " where " + ci.GetPrimaryKeyField().DBColumnName + " = {0}", obj.GetPrimaryKeyValue());
                LogCommand(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        public override void SaveObjectChanges(SoodaObject obj) {
            using (IDbCommand cmd = Connection.CreateCommand()) {
                if (!DisableTransactions)
                    cmd.Transaction = this.Transaction;

                if (obj.IsInsertMode()) {
                    DoInserts(obj, cmd);
                } else {
                    DoUpdates(obj, cmd);
                }
            }
        }

        public override IDataReader LoadObjectTable(SoodaObject obj, object keyVal, int tableNumber, out TableInfo[] loadedTables) {
            ClassInfo classInfo = obj.GetClassInfo();
            IDbCommand cmd = Connection.CreateCommand();

            if (!DisableTransactions)
                cmd.Transaction = this.Transaction;

            SqlBuilder.BuildCommandWithParameters(cmd, GetLoadingSelectStatement(classInfo, classInfo.UnifiedTables[tableNumber], out loadedTables), new object[] { keyVal });
            LogCommand(cmd);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                return reader;
            else {
                reader.Dispose();
                return null;
            }

            return null;
        }

        public override IDataReader LoadObject(SoodaObject obj, object keyVal, out TableInfo[] loadedTables) {
            return LoadObjectTable(obj, keyVal, 0, out loadedTables);
        }

        public override void MakeTuple(string tableName, string leftColumnName, string rightColumnName, object leftVal, object rightVal, int mode) {
            IDbCommand deleteCommand = Connection.CreateCommand();
            IDbCommand insertCommand = Connection.CreateCommand();
            if (!DisableTransactions) {
                deleteCommand.Transaction = this.Transaction;
                insertCommand.Transaction = this.Transaction;
            }

            object[] parameters = new object[] { leftVal, rightVal };
            string query = "delete from " + tableName + " where " + leftColumnName + "={0} and " + rightColumnName + "={1}";
            SqlBuilder.BuildCommandWithParameters(deleteCommand, query, parameters);

            if (mode == 1) {
                query = "insert into " + tableName + "(" + leftColumnName + "," + rightColumnName + ") values({0},{1})";
                SqlBuilder.BuildCommandWithParameters(insertCommand, query, parameters);
            }

            if (mode == -1) {
                LogCommand(deleteCommand);
                deleteCommand.ExecuteNonQuery();
            } else if (mode == 1) {
                try {
                    // try real insert

                    // tran.Log(insertCommand.CommandText, "SQL", this);
                    LogCommand(insertCommand);
                    insertCommand.ExecuteNonQuery();
                } catch (Exception) {
                    // tran.Log("INSERT failed, deleting and retrying", "SQL", this);
                    // tran.Log(deleteCommand.CommandText, "SQL", this);
                    LogCommand(deleteCommand);
                    deleteCommand.ExecuteNonQuery();
                    // tran.Log(insertCommand.CommandText, "SQL", this);
                    LogCommand(insertCommand);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        public override IDataReader LoadObjectList(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, out TableInfo[] tables) {
			try
			{
				tables = classInfo.UnifiedTables[0].ArraySingleton;

				SoqlQueryExpression queryExpression = new SoqlQueryExpression();
				queryExpression.From.Add(classInfo.Name);
				queryExpression.FromAliases.Add("obj");
				queryExpression.SelectExpressions.Add(new SoqlAsteriskExpression(new SoqlPathExpression("obj")));
				queryExpression.SelectAliases.Add("");
				if (whereClause != null && whereClause.WhereExpression != null) 
				{
					queryExpression.WhereClause = whereClause.WhereExpression;
				}

				//queryExpression.O

				StringWriter sw = new StringWriter();
				SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, classInfo.Schema, SqlBuilder);
				converter.ConvertQuery(queryExpression);

				string query = sw.ToString();

				IDbCommand cmd = Connection.CreateCommand();

				if (!DisableTransactions)
					cmd.Transaction = this.Transaction;

				SqlBuilder.BuildCommandWithParameters(cmd, query, whereClause.Parameters);
				LogCommand(cmd);
				return cmd.ExecuteReader();
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
				converter.ConvertQuery(query);

				string queryText = sw.ToString();

				return ExecuteRawQuery(queryText, schema, parameters);
			}
			catch (Exception ex)
			{
				logger.Error("Exception in ExecuteQuery: {0}", ex);
				throw;
			}
		}

		public override IDataReader ExecuteRawQuery(string queryText, SchemaInfo schema, object[] parameters) 
		{
			try 
			{
				IDbCommand cmd = Connection.CreateCommand();

				if (!DisableTransactions)
					cmd.Transaction = this.Transaction;

				SqlBuilder.BuildCommandWithParameters(cmd, queryText, parameters);
				LogCommand(cmd);
				return cmd.ExecuteReader();
			}
			catch (Exception ex)
			{
				logger.Error("Exception in ExecuteRawQuery: {0}", ex);
				throw;
			}
		}

        public override IDataReader LoadRefObjectList(RelationInfo relationInfo, int masterColumn, object masterValue, out TableInfo[] tables) {
			try 
			{
				if (masterColumn == 0)
					tables = relationInfo.GetRef1ClassInfo().UnifiedTables[0].ArraySingleton;
				else
					tables = relationInfo.GetRef2ClassInfo().UnifiedTables[0].ArraySingleton;

				string query = GetLoadRefObjectSelectStatement(relationInfo, masterColumn);

				IDbCommand cmd = Connection.CreateCommand();

				if (!DisableTransactions)
					cmd.Transaction = this.Transaction;

				SqlBuilder.BuildCommandWithParameters(cmd, query, new object[] { masterValue });
				LogCommand(cmd);
				return cmd.ExecuteReader();
			}
			catch (Exception ex)
			{
				logger.Error("Exception in LoadRefObjectList: {0}", ex);
				throw;
			}
        }

        void DoInserts(SoodaObject obj, IDbCommand cmd) {
            foreach (TableInfo table in obj.GetClassInfo().MergedTables) {
                DoInsertsForTable(obj, table, cmd);
            }
        }

        void DoInsertsForTable(SoodaObject obj, TableInfo table, IDbCommand cmd) {
            ClassInfo info = obj.GetClassInfo();
            StringBuilder builder = new StringBuilder(500);
            builder.Append("insert into ");
            builder.Append(table.DBTableName);
            builder.Append("(");

            ArrayList par = new ArrayList();

            for (int i = 0; i < table.Fields.Count; ++i) {
                if (i != 0)
                    builder.Append(",");
                builder.Append(table.Fields[i].DBColumnName);
            };

            builder.Append(") values (");
            for (int i = 0; i < table.Fields.Count; ++i) {
                if (i > 0)
                    builder.Append(",");

                object val = obj.GetFieldValue(table.Fields[i].ClassUnifiedOrdinal);
                builder.Append('{');
                int fieldnum = par.Add(val);
                builder.Append(fieldnum);
                builder.Append('}');
            };
            builder.Append(")");
            SqlBuilder.BuildCommandWithParameters(cmd, builder.ToString(), par.ToArray());
            LogCommand(cmd);
            cmd.ExecuteNonQuery();
        }

        void DoUpdates(SoodaObject obj, IDbCommand cmd) {
            foreach (TableInfo table in obj.GetClassInfo().MergedTables) {
                DoUpdatesForTable(obj, table, cmd);
            }
        }

        void DoUpdatesForTable(SoodaObject obj, TableInfo table, IDbCommand cmd) {
            ClassInfo info = obj.GetClassInfo();
            StringBuilder builder = new StringBuilder(500);
            builder.Append("update ");
            builder.Append(table.DBTableName);
            builder.Append(" set ");

            ArrayList par = new ArrayList();
            bool first = true;

#warning FIX ME

            for (int i = 0; i < table.Fields.Count; ++i) {
                int fieldNumber = table.Fields[i].ClassUnifiedOrdinal;

                if (obj.IsFieldDirty(fieldNumber)) {
                    if (!first) {
                        builder.Append(", ");
                    }
                    builder.Append(table.Fields[i].DBColumnName);
                    builder.Append("={");
                    int fieldnum = par.Add(obj.GetFieldValue(fieldNumber));
                    builder.Append(fieldnum);
                    builder.Append("}");
                    first = false;
                };
            };
            builder.Append(" where ");
            builder.Append(table.TablePrimaryKeyField.DBColumnName);
            builder.Append("={");
            builder.Append(par.Add(obj.GetPrimaryKeyValue()));
            builder.Append('}');
            SqlBuilder.BuildCommandWithParameters(cmd, builder.ToString(), par.ToArray());
            LogCommand(cmd);
            cmd.ExecuteNonQuery();
        }

        private void LogCommand(IDbCommand cmd) {
            StringBuilder txt = new StringBuilder();
            txt.Append(cmd.CommandText);
            txt.Append(" [");
            foreach (IDataParameter par in cmd.Parameters) {
                txt.AppendFormat(" {0}={1}", par.ParameterName, par.Value);
            }
            txt.Append(" ]");

#if A

            txt.Replace('\n', ' ');
            txt.Replace('\r', ' ');
            int lastlen;

            do {
                lastlen = txt.Length;
                txt.Replace("  ", " ");
            } while (txt.Length != lastlen);
#endif

            sqllogger.Debug(txt.ToString());
            // Console.WriteLine("Executing: {0}", cmd.CommandText);
        }

        public void ExecuteRaw(string sql) {
            using (IDbCommand cmd = Connection.CreateCommand()) {
                // Console.WriteLine("state: {0}", Connection.State);
                if (!DisableTransactions)
                    cmd.Transaction = this.Transaction;

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private Hashtable cacheLoadingSelectStatement = new Hashtable();
        private Hashtable cacheLoadedTables = new Hashtable();
        private Hashtable[] cacheLoadRefObjectSelectStatement = new Hashtable[] { new Hashtable(), new Hashtable() };

        class _QueueItem {
            public ClassInfo classInfo;
            public SoqlPathExpression prefix;
            public int level;
        }

        private string GetLoadingSelectStatement(ClassInfo classInfo, TableInfo tableInfo, out TableInfo[] loadedTables) {
            if (!cacheLoadingSelectStatement.Contains(tableInfo)) {
                Queue queue = new Queue();
                ArrayList additional = new ArrayList();
                additional.Add(tableInfo);

                SoqlQueryExpression queryExpression = new SoqlQueryExpression();
                queryExpression.From.Add(classInfo.Name);
                queryExpression.FromAliases.Add("obj");

                foreach (FieldInfo fi in tableInfo.Fields) {
                    SoqlPathExpression pathExpr = new SoqlPathExpression("obj", fi.Name);
                    queryExpression.SelectExpressions.Add(pathExpr);
                    queryExpression.SelectAliases.Add("");

                    if (fi.ReferencedClass != null && fi.PrefetchLevel > 0) {
                        _QueueItem item = new _QueueItem();
                        item.classInfo = fi.ReferencedClass;
                        item.level = fi.PrefetchLevel;
                        item.prefix = pathExpr;

                        queue.Enqueue(item);
                    }
                }

                // TODO - add prefetching
                while (queue.Count > 0) {
                    _QueueItem it = (_QueueItem)queue.Dequeue();

                    foreach (FieldInfo fi in it.classInfo.UnifiedFields) {
                        // TODO - this relies on the fact that path expressions
                        // are never reconstructed or broken. We simply share previous prefix
                        // perhaps it's cleaner to Clone() the expression here

                        SoqlPathExpression extendedExpression = new SoqlPathExpression(it.prefix, fi.Name);

                        queryExpression.SelectExpressions.Add(extendedExpression);
                        queryExpression.SelectAliases.Add("");

                        if (it.level >= 1 && fi.PrefetchLevel > 0 && fi.ReferencedClass != null) {
                            _QueueItem newItem = new _QueueItem();
                            newItem.classInfo = fi.ReferencedClass;
                            newItem.prefix = extendedExpression;
                            newItem.level = it.level - 1;
                            queue.Enqueue(newItem);
                        }
                    }
                }

                queryExpression.WhereClause = new SoqlBooleanRelationalExpression(
                                                  new SoqlPathExpression("obj", tableInfo.TablePrimaryKeyField.Name),
                                                  new SoqlParameterLiteralExpression(0),
                                                  SoqlRelationalOperator.Equal);

                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, tableInfo.OwnerClass.Schema, SqlBuilder);
                converter.ConvertQuery(queryExpression);

                string query = sw.ToString();

                cacheLoadingSelectStatement[tableInfo] = query;
                cacheLoadedTables[tableInfo] = additional.ToArray(typeof(TableInfo));
            }
            loadedTables = (TableInfo[])cacheLoadedTables[tableInfo];
            return (string)cacheLoadingSelectStatement[tableInfo];
        }

        private string GetLoadRefObjectSelectStatement(RelationInfo relationInfo, int masterColumn) {
            if (!cacheLoadRefObjectSelectStatement[masterColumn].Contains(relationInfo)) {
                string soqlQuery = "";

                soqlQuery = String.Format("select mt.{0}.* from {2} mt where mt.{1} = {{0}}",
                                          relationInfo.Table.Fields[masterColumn].Name,
                                          relationInfo.Table.Fields[1 - masterColumn].Name,
                                          relationInfo.Name);

                StringWriter sw = new StringWriter();
                SoqlToSqlConverter converter = new SoqlToSqlConverter(sw, relationInfo.Schema, SqlBuilder);
                converter.ConvertQuery(SoqlParser.ParseQuery(soqlQuery));
                string sqlQuery = sw.ToString();

                cacheLoadRefObjectSelectStatement[masterColumn][relationInfo] = sqlQuery;
            };
            return (string)cacheLoadRefObjectSelectStatement[masterColumn][relationInfo];
        }

        private void UnifyTable(Hashtable tables, TableInfo ti) {
            TableInfo baseTable;

            baseTable = (TableInfo)tables[ti.DBTableName];
            if (baseTable == null) {
                baseTable = new TableInfo();
                baseTable.DBTableName = ti.DBTableName;

                tables[ti.DBTableName] = baseTable;
            }

            foreach (FieldInfo fi in ti.Fields) {
                bool found = false;

                foreach (FieldInfo fi0 in baseTable.Fields) {
                    if (fi0.Name == fi.Name) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    baseTable.Fields.Add(fi);
                }
            }
        }

        public virtual void GenerateDdlForSchema(SchemaInfo schema, TextWriter tw) {
            Hashtable tables = new Hashtable();

            foreach (ClassInfo ci in schema.Classes) {
                foreach (TableInfo ti in ci.UnifiedTables) {
                    UnifyTable(tables, ti);
                }
            }

            foreach (RelationInfo ri in schema.Relations) {
                UnifyTable(tables, ri.Table);
            }

            ArrayList names = new ArrayList();

            foreach (TableInfo ti in tables.Values) {
                names.Add(ti.DBTableName);
            }

            names.Sort();

            foreach (string s in names) {
                tw.WriteLine("--- table {0}", s);
                SqlBuilder.GenerateCreateTable(tw, (TableInfo)tables[s]);
            }
        }
    }
}
