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
using System.Data.OracleClient;

using Sooda.QL;
using Sooda.Schema;

namespace Sooda.Sql
{
    public class OracleBuilder : SqlBuilderPositionalArg
    {
        public override string GetDDLCommandTerminator()
        {
            return Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;
        }

        public override string GetSQLDataType(Sooda.Schema.FieldInfo fi)
        {
            switch (fi.DataType)
            {
                case FieldDataType.Integer:
                case FieldDataType.BooleanAsInteger:
                case FieldDataType.TimeSpan:
                case FieldDataType.Long:
                    return "integer";

                case FieldDataType.AnsiString:
                    if (fi.Size >= 4000)
                        return "clob";
                    return "varchar2(" + fi.Size + ")";

                case FieldDataType.String:
                    if (fi.Size >= 2000)
                        return "nclob";
                    return "nvarchar2(" + fi.Size + ")";

                case FieldDataType.Decimal:
                    if (fi.Size < 0)
                        return "number";
                    if (fi.Precision < 0)
                        return "number(" + fi.Size + ")";
                    return "number(" + fi.Size + "," + fi.Precision + ")";

                case FieldDataType.Double:
                case FieldDataType.Float:
                    if (fi.Size < 0)
                        return "float";
                    if (fi.Precision < 0)
                        return "float(" + fi.Size + ")";
                    return "float(" + fi.Size + "," + fi.Precision + ")";

                case FieldDataType.DateTime:
                    return "date";

                case FieldDataType.Image:
                    return "blob";

                case FieldDataType.Boolean:
                    return "byte";

                case FieldDataType.Blob:
                    return "blob";

                default:
                    throw new NotImplementedException(String.Format("Datatype {0} not supported for this database", fi.DataType));
            }
        }

        public override string GetSQLNullable(Sooda.Schema.FieldInfo fi)
        {
            switch (fi.DataType)
            {
                case FieldDataType.AnsiString:
                case FieldDataType.String:
                    if (fi.Size < 4000)
                        // IsNull works fine for Oracle clob, but for nvarchar2 isnull('') = true - contrary to ansi SQL-92
                        return "null";
                    break;
            }

            return fi.IsNullable ? "null" : "not null";
        }

        protected override string GetNameForParameter(int pos)
        {
            return ":p" + pos;
        }

        public override string QuoteFieldName(string s)
        {
            return String.Concat("\"", s, "\"");
        }

        public override SqlTopSupportMode TopSupport
        {
            get
            {
                return SqlTopSupportMode.OracleRowNum;
            }
        }

        public override SqlOuterJoinSyntax OuterJoinSyntax
        {
            get
            {
                return SqlOuterJoinSyntax.Oracle;
            }
        }

        public override string GetSQLOrderBy(Sooda.Schema.FieldInfo fi, bool start)
        {
            switch (fi.DataType)
            {
                case FieldDataType.AnsiString:
                    if (fi.Size > 2000)
                        return start ? "cast(substr(" : ", 0, 2000) as varchar2(2000))";
                    return "";

                case FieldDataType.String:
                    if (fi.Size > 2000)
                        return start ? "cast(substr(" : ", 0, 2000) as nvarchar2(2000))";
                    return "";

                default:
                    return "";
            }

        }

        public override string GetAlterTableStatement(Sooda.Schema.TableInfo tableInfo)
        {
            string ident = GetTruncatedIdentifier("PK_" + tableInfo.DBTableName);
            return String.Format("alter table {0} add constraint {1} primary key", tableInfo.DBTableName, ident);
        }

        protected override string AddParameterFromValue(IDbCommand command, object v, SoqlLiteralValueModifiers modifiers)
        {
            string paramName = base.AddParameterFromValue(command, v, modifiers);
            OracleParameter param = (OracleParameter)command.Parameters[paramName];
            if (param.DbType == DbType.String && v.ToString().Length > 2000)
                param.OracleType = OracleType.NClob;
            return paramName;
        }

        public override bool IsFatalException(IDbConnection connection, Exception e)
        {
#if DOTNET2 && !MONO
            #pragma warning disable 618
            OracleConnection.ClearAllPools();
            #pragma warning restore 618
#endif
            return false;
        }

        // for Oracle empty string is also null string
        public override bool IsNullValue(object val, Sooda.Schema.FieldInfo fi)
        {
            if (val == null)
               return true;
            if ((fi.DataType == FieldDataType.AnsiString) || (fi.DataType == FieldDataType.String))
                return ((string)val).Length == 0;
            return false;
        }

    }
}
