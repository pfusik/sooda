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
using System.Data;
using System.Data.SqlClient;

using Sooda.Schema;

namespace Sooda.Sql
{
    public class SqlServerBuilder : SqlBuilderNamedArg
    {
        public SqlServerBuilder() { }

        public override string StringConcatenationOperator
        {
            get { return "+"; }
        }

        public override string GetDDLCommandTerminator()
        {
            return Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;
        }

        public override string GetSQLDataType(Sooda.Schema.FieldInfo fi)
        {
            switch (fi.DataType)
            {
                case FieldDataType.Integer:
                    return "int";

                case FieldDataType.AnsiString:
                    if (fi.Size > 4000)
                        return "text";
                    else
                        return "varchar(" + fi.Size + ")";

                case FieldDataType.String:
                    if (fi.Size > 4000)
                        return "ntext";
                    else
                        return "nvarchar(" + fi.Size + ")";

                case FieldDataType.Decimal:
                    if (fi.Size < 0)
                        return "decimal";
                    else if (fi.Precision < 0)
                        return "decimal(" + fi.Size + ")";
                    else
                        return "decimal(" + fi.Size + "," + fi.Precision + ")";

                case FieldDataType.Guid:
                    return "uniqueidentifier";

                case FieldDataType.Double:
                    if (fi.Size < 0)
                        return "float";
                    else if (fi.Precision < 0)
                        return "float(" + fi.Size + ")";
                    else
                        return "float(" + fi.Size + "," + fi.Precision + ")";

                case FieldDataType.Float:
                    if (fi.Size < 0)
                        return "float";
                    else if (fi.Precision < 0)
                        return "float(" + fi.Size + ")";
                    else
                        return "float(" + fi.Size + "," + fi.Precision + ")";

                case FieldDataType.DateTime:
                    return "datetime";

                case FieldDataType.Image:
                    return "image";

                case FieldDataType.Long:
                    return "bigint";

                case FieldDataType.BooleanAsInteger:
                    return "int";

                case FieldDataType.TimeSpan:
                    return "int";

                case FieldDataType.Boolean:
                    return "bit";

                case FieldDataType.Blob:
                    return "varbinary(max)";

                default:
                    throw new NotImplementedException(String.Format("Datatype {0} not supported for this database", fi.DataType.ToString()));
            }
        }

        protected override string GetNameForParameter(int pos)
        {
            return "@p" + pos.ToString();
        }

        public override SqlTopSupportMode TopSupport
        {
            get
            {
                return SqlTopSupportMode.MSSQLRowNum;
                // return SqlTopSupportMode.MSSQL2012;
            }
        }

        public override int MaxIdentifierLength
        {
            get
            {
                return 128;
            }
        }

        public override string EndInsert(string tableName)
        {
            return "set identity_insert " + tableName + " off ";
        }

        public override string BeginInsert(string tableName)
        {
            return "set identity_insert " + tableName + " on ";
        }

        public override string GetSQLOrderBy(Sooda.Schema.FieldInfo fi, bool start)
        {
            switch (fi.DataType)
            {
                case FieldDataType.AnsiString:
                    if (fi.Size > 4000)
                        return start ? "convert(varchar(3999), " : ")";
                    else
                        return "";

                case FieldDataType.String:
                    if (fi.Size > 4000)
                        return start ? "convert(nvarchar(3999), " : ")";
                    else
                        return "";

                default:
                    return "";
            }
        }

        public override string GetAlterTableStatement(Sooda.Schema.TableInfo tableInfo)
        {
            string ident = GetTruncatedIdentifier(String.Format("PK_{0}", tableInfo.DBTableName));
            return String.Format("alter table {0} add constraint {1} primary key", tableInfo.DBTableName, ident);
        }

        public override bool IsFatalException(IDbConnection connection, Exception e)
        {
            SqlConnection.ClearAllPools();
            return false;
        }
    }
}
