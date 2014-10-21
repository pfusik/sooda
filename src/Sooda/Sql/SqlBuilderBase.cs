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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;

using Sooda.QL;
using Sooda.ObjectMapper;
using Sooda.ObjectMapper.FieldHandlers;
using Sooda.Schema;

namespace Sooda.Sql
{
    public abstract class SqlBuilderBase : ISqlBuilder
    {
        private bool _useSafeLiterals = true;

        static string HashString(string input)
        {
            int sum = 0;
            for (int i = 0; i < input.Length; i++)
                sum += i * input[i];
            sum &= 0xffff;
            return sum.ToString("x4");
        }

        public string GetTruncatedIdentifier(string identifier)
        {
            if (identifier.Length <= MaxIdentifierLength)
                return identifier;
            string hash = HashString(identifier);
            return identifier.Substring(0, MaxIdentifierLength - 5) + "_" + hash;
        }

        public bool UseSafeLiterals
        {
            get { return _useSafeLiterals; }
            set { _useSafeLiterals = value; }
        }

        public virtual string GetDDLCommandTerminator()
        {
            return ";" + Environment.NewLine;
        }

        public virtual SqlOuterJoinSyntax OuterJoinSyntax
        {
            get
            {
                return SqlOuterJoinSyntax.Ansi;
            }
        }

        public virtual string StringConcatenationOperator
        {
            get { return "||"; }
        }

        public virtual int MaxIdentifierLength
        {
            get
            {
                return 30;
            }
        }

        public void GenerateCreateTableField(TextWriter xtw, Sooda.Schema.FieldInfo fieldInfo)
        {
            xtw.Write('\t');
            xtw.Write(fieldInfo.DBColumnName);
            xtw.Write(' ');
            xtw.Write(GetSQLDataType(fieldInfo));
            xtw.Write(' ');
            xtw.Write(GetSQLNullable(fieldInfo));
        }

        void TerminateDDL(TextWriter xtw, string additionalSettings)
        {
            if (!string.IsNullOrEmpty(additionalSettings))
            {
                xtw.Write(' ');
                xtw.Write(additionalSettings);
            }
            xtw.Write(GetDDLCommandTerminator());
        }

        public void GenerateCreateTable(TextWriter xtw, Sooda.Schema.TableInfo tableInfo, string additionalSettings)
        {
            xtw.WriteLine("create table {0} (", tableInfo.DBTableName);
            Dictionary<string, bool> processedFields = new Dictionary<string, bool>();
            for (int i = 0; i < tableInfo.Fields.Count; ++i)
            {
                if (!processedFields.ContainsKey(tableInfo.Fields[i].DBColumnName))
                {
                    GenerateCreateTableField(xtw, tableInfo.Fields[i]);
                    if (i == tableInfo.Fields.Count - 1)
                        xtw.WriteLine();
                    else
                        xtw.WriteLine(',');
                    processedFields.Add(tableInfo.Fields[i].DBColumnName, true);
                }
            }
            xtw.Write(')');
            TerminateDDL(xtw, additionalSettings);
        }

        public virtual string GetAlterTableStatement(Sooda.Schema.TableInfo tableInfo)
        {
            return String.Format("alter table {0} add primary key", tableInfo.DBTableName);
        }

        public void GeneratePrimaryKey(TextWriter xtw, Sooda.Schema.TableInfo tableInfo, string additionalSettings)
        {
            bool first = true;

            foreach (Sooda.Schema.FieldInfo fi in tableInfo.Fields)
            {
                if (fi.IsPrimaryKey)
                {
                    if (first)
                    {
                        xtw.Write(GetAlterTableStatement(tableInfo));
                        xtw.Write(" (");
                    }
                    else
                    {
                        xtw.Write(", ");
                    }
                    xtw.Write(fi.DBColumnName);
                    first = false;
                }
            }
            if (!first)
            {
                xtw.Write(')');
                TerminateDDL(xtw, additionalSettings);
            }
        }

        public void GenerateForeignKeys(TextWriter xtw, Sooda.Schema.TableInfo tableInfo)
        {
            foreach (Sooda.Schema.FieldInfo fi in tableInfo.Fields)
            {
                if (fi.References != null)
                {
                    xtw.Write("alter table {0} add constraint {1} foreign key ({2}) references {3}({4})",
                            tableInfo.DBTableName, GetConstraintName(tableInfo.DBTableName, fi.DBColumnName), fi.DBColumnName,
                            fi.ReferencedClass.UnifiedTables[0].DBTableName, fi.ReferencedClass.GetFirstPrimaryKeyField().DBColumnName
                            );
                    xtw.Write(GetDDLCommandTerminator());
                }
            }
        }

        public void GenerateIndices(TextWriter xtw, Sooda.Schema.TableInfo tableInfo, string additionalSettings)
        {
            foreach (Sooda.Schema.FieldInfo fi in tableInfo.Fields)
            {
                if (fi.References != null)
                {
                    xtw.Write("create index {0} on {1} ({2})",
                            GetIndexName(tableInfo.DBTableName, fi.DBColumnName), tableInfo.DBTableName, fi.DBColumnName);
                    TerminateDDL(xtw, additionalSettings);
                }
            }
        }

        public virtual string GetConstraintName(string tableName, string foreignKey)
        {
            return GetTruncatedIdentifier(String.Format("FK_{0}_{1}", tableName, foreignKey));
        }

        public virtual string GetIndexName(string tableName, string column)
        {
            return GetTruncatedIdentifier(String.Format("IDX_{0}_{1}", tableName, column));
        }

        public abstract string GetSQLDataType(Sooda.Schema.FieldInfo fi);
        public abstract string GetSQLOrderBy(Sooda.Schema.FieldInfo fi, bool start);

        public virtual string GetSQLNullable(Sooda.Schema.FieldInfo fi)
        {
            return fi.IsNullable ? "null" : "not null";
        }

        protected virtual bool SetDbTypeFromValue(IDbDataParameter parameter, object value, SoqlLiteralValueModifiers modifiers)
        {
            DbType dbType;
            if (!paramTypes.TryGetValue(value.GetType(), out dbType))
                return false;
            parameter.DbType = dbType;
            return true;
        }

        private static readonly Dictionary<Type, DbType> paramTypes = new Dictionary<Type, DbType>();

        static SqlBuilderBase()
        {
            paramTypes[typeof(SByte)] = DbType.SByte;
            paramTypes[typeof(Int16)] = DbType.Int16;
            paramTypes[typeof(Int32)] = DbType.Int32;
            paramTypes[typeof(Int64)] = DbType.Int64;
            paramTypes[typeof(Single)] = DbType.Single;
            paramTypes[typeof(Double)] = DbType.Double;
            paramTypes[typeof(String)] = DbType.String;
            paramTypes[typeof(Boolean)] = DbType.Boolean;
            paramTypes[typeof(Decimal)] = DbType.Decimal;
            paramTypes[typeof(Guid)] = DbType.Guid;
            paramTypes[typeof(TimeSpan)] = DbType.Int32;
            paramTypes[typeof(byte[])] = DbType.Binary;
            paramTypes[typeof(System.Drawing.Image)] = DbType.Binary;
            paramTypes[typeof(System.Drawing.Bitmap)] = DbType.Binary;
        }

        public virtual string QuoteIdentifier(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!(c >= 'A' && c <= 'Z')
                 && !(c >= 'a' && c <= 'z')
                 && !(c >= '0' && c <= '9')
                 && !(c == '_' && i > 0))
                    return "\"" + s + "\"";
            }
            return s;
        }

        public abstract SqlTopSupportMode TopSupport
        {
            get;
        }

        protected bool IsStringSafeForLiteral(string v)
        {
            if (v.Length > 500)
                return false;
            foreach (char ch in v)
            {
                switch (ch)
                {
                    // we are very conservative about what 'safe' means
                    case ' ':
                    case '.':
                    case ',':
                    case '-':
                    case '%':
                    case '_':
                    case '@':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        public virtual string EndInsert(string tableName)
        {
            return "";
        }

        public virtual string BeginInsert(string tableName)
        {
            return "";
        }

        protected virtual string AddParameterFromValue(IDbCommand command, object v, SoqlLiteralValueModifiers modifiers)
        {
            IDbDataParameter p = command.CreateParameter();
            p.Direction = ParameterDirection.Input;

            p.ParameterName = GetNameForParameter(command.Parameters.Count);
            if (modifiers != null)
                FieldHandlerFactory.GetFieldHandler(modifiers.DataTypeOverride).SetupDBParameter(p, v);
            else
            {
                SetDbTypeFromValue(p, v, modifiers);
                p.Value = v;
            }
            command.Parameters.Add(p);
            return p.ParameterName;
        }

        public void BuildCommandWithParameters(System.Data.IDbCommand command, bool append, string query, object[] par, bool isRaw)
        {
            if (append)
            {
                if (command.CommandText == null)
                    command.CommandText = "";
                else if (command.CommandText.Length > 0)
                    command.CommandText += ";\n";
            }
            else
            {
                command.CommandText = "";
                command.Parameters.Clear();
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(query.Length * 2);
            StringCollection paramNames = new StringCollection();

            for (int i = 0; i < query.Length; ++i)
            {
                char c = query[i];

                if (c == '\'')
                {
                    int j = ++i;
                    for (;;++j)
                    {
                        if (j >= query.Length)
                            throw new ArgumentException("Query has unbalanced quotes");
                        if (query[j] == '\'')
                        {
                            if (j + 1 >= query.Length || query[j + 1] != '\'')
                                break;
                            // double apostrophe
                            j++;
                        }
                    }

                    string stringValue = query.Substring(i, j - i);
                    char modifier = j + 1 < query.Length ? query[j + 1] : ' ';
                    string paramName;

                    switch (modifier)
                    {
                        case 'V':
                            sb.Append('\'');
                            sb.Append(stringValue);
                            sb.Append('\'');
                            j++;
                            break;
                        case 'D':
                            paramName = AddParameterFromValue(command, DateTime.ParseExact(stringValue, "yyyyMMddHH:mm:ss", CultureInfo.InvariantCulture), null);
                            sb.Append(paramName);
                            j++;
                            break;
                        case 'A':
                            stringValue = stringValue.Replace("''", "'");
                            paramName = AddParameterFromValue(command, stringValue, SoqlLiteralValueModifiers.AnsiString);
                            sb.Append(paramName);
                            j++;
                            break;
                        default:
                            if (!isRaw && (!UseSafeLiterals || !IsStringSafeForLiteral(stringValue)))
                            {
                                stringValue = stringValue.Replace("''", "'");
                                paramName = AddParameterFromValue(command, stringValue, null);
                                sb.Append(paramName);
                            }
                            else
                            {
                                sb.Append('\'');
                                sb.Append(stringValue);
                                sb.Append('\'');
                            }
                            break;
                    }
                    i = j;
                }
                else if (c == '{')
                {
                    c = query[i + 1];

                    if (c == 'L')
                    {
                        // {L:fieldDataTypeName:value

                        int startPos = i + 3;
                        int endPos = query.IndexOf(':', startPos);
                        if (endPos < 0)
                            throw new ArgumentException("Missing ':' in literal specification");

                        SoqlLiteralValueModifiers modifier = SoqlParser.ParseLiteralValueModifiers(query.Substring(startPos, endPos - startPos));
                        FieldDataType fdt = modifier.DataTypeOverride;

                        int valueStartPos = endPos + 1;
                        bool anyEscape = false;

                        for (i = valueStartPos; i < query.Length && query[i] != '}'; ++i)
                        {
                            if (query[i] == '\\')
                            {
                                i++;
                                anyEscape = true;
                            }
                        }

                        string literalValue = query.Substring(valueStartPos, i - valueStartPos);
                        if (anyEscape)
                        {
                            literalValue = literalValue.Replace("\\}", "}");
                            literalValue = literalValue.Replace("\\\\", "\\");
                        }

                        SoodaFieldHandler fieldHandler = FieldHandlerFactory.GetFieldHandler(fdt);
                        object v = fieldHandler.RawDeserialize(literalValue);

                        if (v == null)
                        {
                            sb.Append("null");
                        }
                        else if (UseSafeLiterals && v is int)
                        {
                            sb.Append((int)v);
                        }
                        else if (UseSafeLiterals && v is string && IsStringSafeForLiteral((string)v))
                        {
                            sb.Append('\'');
                            sb.Append((string)v);
                            sb.Append('\'');
                        }
                        else
                        {
                            IDbDataParameter p = command.CreateParameter();
                            p.Direction = ParameterDirection.Input;
                            p.ParameterName = GetNameForParameter(command.Parameters.Count);
                            fieldHandler.SetupDBParameter(p, v);
                            command.Parameters.Add(p);
                            sb.Append(p.ParameterName);
                        }
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        i++;
                        int paramNumber = 0;
                        do
                        {
                            paramNumber = paramNumber * 10 + c - '0';
                            c = query[++i];
                        } while (c >= '0' && c <= '9');

                        SoqlLiteralValueModifiers modifiers = null;
                        if (c == ':')
                        {
                            int startPos = i + 1;
                            i = query.IndexOf('}', startPos);
                            if (i < 0)
                                throw new ArgumentException("Missing '}' in parameter specification");
                            modifiers = SoqlParser.ParseLiteralValueModifiers(query.Substring(startPos, i - startPos));
                        }
                        else if (c != '}')
                            throw new ArgumentException("Missing '}' in parameter specification");

                        object v = par[paramNumber];

                        if (v is SoodaObject)
                        {
                            v = ((SoodaObject)v).GetPrimaryKeyValue();
                        }

                        if (v == null)
                        {
                            sb.Append("null");
                        }
                        else if (UseSafeLiterals && v is int)
                        {
                            sb.Append((int)v);
                        }
                        else if (UseSafeLiterals && v is string && IsStringSafeForLiteral((string)v))
                        {
                            sb.Append('\'');
                            sb.Append((string)v);
                            sb.Append('\'');
                        }
                        else
                        {
                            sb.Append(AddNumberedParameter(command, v, modifiers, paramNames, paramNumber));
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Unexpected character in parameter specification");
                    }
                }
                else if (c == '(' || c == ' ' || c == ',' || c == '=' || c == '>' || c == '<' || c == '+' || c == '-' || c == '*' || c == '/')
                {
                    sb.Append(c);
                    if (i < query.Length - 1)
                    {
                        c = query[i + 1];
                        if (c >= '0' && c <= '9' && !UseSafeLiterals)
                        {
                            int v = 0;
                            double f = 0;
                            double dp = 0;
                            bool isDouble = false;
                            do
                            {
                                if (c != '.')
                                {
                                    if (!isDouble)
                                        v = v * 10 + c - '0';
                                    else
                                    {
                                        f = f + dp * (c - '0');
                                        dp = dp * 0.1;
                                    }
                                }
                                else
                                {
                                   isDouble = true;
                                   f = v;
                                   dp = 0.1;
                                }
                                i++;
                                if (i < query.Length - 1)
                                    c = query[i+1];
                            } while (((c >= '0' && c <= '9') || c == '.') && (i < query.Length - 1));
                            if (!isDouble)
                            {
                                string paramName = AddParameterFromValue(command, v, null);
                                sb.Append(paramName);
                            }
                            else
                            {
                                string paramName = AddParameterFromValue(command, f, null);
                                sb.Append(paramName);
                            }
                        }
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            command.CommandText += sb.ToString();
        }

        public virtual bool IsFatalException(IDbConnection connection, Exception e)
        {
            return true;
        }

        public virtual bool IsNullValue(object val, Sooda.Schema.FieldInfo fi)
        {
            return val == null;
        }

        protected abstract string AddNumberedParameter(IDbCommand command, object v, SoqlLiteralValueModifiers modifiers, StringCollection paramNames, int paramNumber);

        protected abstract string GetNameForParameter(int pos);
    }
}
