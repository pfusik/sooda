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
using System.Collections;
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

        private string HashString(string input)
        {
            int tmp = 0;
            for (int i = 0; i < input.Length; i++)
            {
                tmp += i*input[i];
                tmp = tmp % 65536;
            }
            return tmp.ToString("x4");
        }

        public string GetTruncatedIdentifier(string identifier)
        {
            if (identifier.Length < MaxIdentifierLength)
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

        public virtual int MaxIdentifierLength
        {
            get
            {
                return 30;
            }
        }

        public void GenerateCreateTableField(TextWriter xtw, Sooda.Schema.FieldInfo fieldInfo)
        {
            Console.Write("\t{0} {1} {2}", fieldInfo.DBColumnName, GetSQLDataType(fieldInfo), fieldInfo.IsNullable ? "null" : "not null");
        }

        public void GenerateCreateTable(TextWriter xtw, Sooda.Schema.TableInfo tableInfo, string additionalSettings)
        {
            xtw.WriteLine("create table {0} (", tableInfo.DBTableName);
            Hashtable processedFields = new Hashtable();
            for (int i = 0; i < tableInfo.Fields.Count; ++i)
            {
                if (!processedFields.ContainsKey(tableInfo.Fields[i].DBColumnName))
                {
                    GenerateCreateTableField(xtw, tableInfo.Fields[i]);
                    if (i == tableInfo.Fields.Count - 1)
                        Console.WriteLine();
                    else
                        Console.WriteLine(",");
                    processedFields.Add(tableInfo.Fields[i].DBColumnName, 0);
                }
            }
            xtw.Write(")");
            if (additionalSettings != "")
                xtw.Write(" " + additionalSettings);
            xtw.Write(GetDDLCommandTerminator());
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
                        xtw.Write("alter table {0} add primary key (", tableInfo.DBTableName);
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
                xtw.Write(")");
                if (additionalSettings != "")
                    xtw.Write(" " + additionalSettings);
                xtw.Write(GetDDLCommandTerminator());
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

        public virtual string GetConstraintName(string tableName, string foreignKey)
        {
            return GetTruncatedIdentifier(String.Format("FK_{0}_{1}", tableName, foreignKey));
        }

        public abstract string GetSQLDataType(Sooda.Schema.FieldInfo fi);
        public abstract string GetSQLOrderBy(Sooda.Schema.FieldInfo fi, bool start);

        protected virtual bool SetDbTypeFromValue(IDbDataParameter parameter, object value, SoqlLiteralValueModifiers modifiers)
        {
            object o = paramTypes[value.GetType()];
            if (o == null)
                return false;
            parameter.DbType = (DbType)o;
            return true;
        }

        private static Hashtable paramTypes = new Hashtable();
        private static bool[] _isCharSafe = new bool[128];

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

            // we-re very conservative about what 'safe' means
            for(char c = 'A'; c <= 'Z'; ++c)
                _isCharSafe[(int)c] = true;
            for(char c = 'a'; c <= 'z'; ++c)
                _isCharSafe[(int)c] = true;
            for(char c = '0'; c <= '9'; ++c)
                _isCharSafe[(int)c] = true;
            _isCharSafe[(int)' '] = true;
            _isCharSafe[(int)'.'] = true;
            _isCharSafe[(int)','] = true;
            _isCharSafe[(int)'-'] = true;
            _isCharSafe[(int)'%'] = true;
            _isCharSafe[(int)'_'] = true;
            _isCharSafe[(int)'@'] = true;
        }

        public virtual string QuoteFieldName(string s)
        {
            return String.Concat("[", s, "]");
        }

        public abstract SqlTopSupportMode TopSupport
        {
            get;
        }

        protected bool IsStringSafeForLiteral(string v)
        {
            if (v.Length > 500)
                return false;
            for (int i = 0; i < v.Length; ++i)
            {
                int ch = (int)v[i];
                if (ch < 32)
                    return false; // ASCII control characters
                if (ch >= 128)
                    return false; // high code characters - may require some quoting
                if (!_isCharSafe[ch])
                    return false;
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

        protected string AddParameterFromValue(IDbCommand command, object v, SoqlLiteralValueModifiers modifiers)
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
                if (command.CommandText != "")
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

                if (c == '\'')  // locate the string
                {
                    int stringStartPos = i;
                    int stringEndPos = -1;

                    for (int j = i + 1; j < query.Length; ++j)
                    {
                        if (query[j] == '\'')
                        {
                            // possible end of string, need to check for double apostrophes,
                            // which don't mean EOS

                            if (j + 1 < query.Length && query[j + 1] == '\'')
                            {
                                j++;
                                continue;
                            }

                            stringEndPos = j;
                            break;
                        }
                    }

                    if (stringEndPos == -1)
                    {
                        throw new ArgumentException("Query has unbalanced quotes");
                    }

                    string stringValue = query.Substring(stringStartPos + 1, stringEndPos - stringStartPos - 1);
                    bool requireParameter = false;

                    // dates and ansi-string definitely require parameters
                    if (stringEndPos + 1 < query.Length && (query[stringEndPos + 1] == 'D' || query[stringEndPos + 1] == 'A'))
                    {
                        requireParameter = true;
                    }

                    if (!requireParameter)
                    {
                        if (!IsStringSafeForLiteral(stringValue) && !isRaw)
                            requireParameter = true;
                    }

                    if (!UseSafeLiterals && !isRaw)
                        requireParameter = true;

                    if (requireParameter)
                    {
                        // replace double quotes with single quotes
                        stringValue = stringValue.Replace("''", "'");
                        string paramName;

                        if (stringEndPos + 1 < query.Length && query[stringEndPos + 1] == 'D')
                        {
                            // datetime literal
                            paramName = AddParameterFromValue(command, DateTime.ParseExact(stringValue, "yyyyMMddHH:mm:ss", CultureInfo.InvariantCulture), null);
                            stringEndPos++;
                        }
                        else if (stringEndPos + 1 < query.Length && query[stringEndPos + 1] == 'A')
                        {
                            paramName = AddParameterFromValue(command, stringValue, SoqlLiteralValueModifiers.AnsiString);
                            stringEndPos++;
                        }
                        else
                        {
                            paramName = AddParameterFromValue(command, stringValue, null);
                        }
                        sb.Append(paramName);

                        i = stringEndPos;
                    }
                    else
                    {
                        sb.Append("'");
                        sb.Append(stringValue);
                        sb.Append("'");
                        i = stringEndPos;
                    }
                }
                else if (c == '{')
                {
                    char c1 = query[i + 1];
                    object v;

                    if (c1 == 'L')
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

                        for (i = valueStartPos; i < query.Length; ++i)
                        {
                            if (query[i] == '\\')
                            {
                                i++;
                                anyEscape = true;
                                continue;
                            }
                            if (query[i] == '}')
                            {
                                break;
                            }
                        }

                        string literalValue = query.Substring(valueStartPos, i - valueStartPos);
                        if (anyEscape)
                        {
                            literalValue = literalValue.Replace("\\}", "}");
                            literalValue = literalValue.Replace("\\\\", "\\");
                        }

                        SoodaFieldHandler fieldHandler = FieldHandlerFactory.GetFieldHandler(fdt);
                        v = fieldHandler.RawDeserialize(literalValue);

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
                            sb.Append("'");
                            sb.Append((string)v);
                            sb.Append("'");
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
                    else
                    {
                        char c2 = query[i + 2];
                        int paramNumber;
                        SoqlLiteralValueModifiers modifiers = null;

                        if (c2 == '}' || c2 == ':')
                        {
                            paramNumber = c1 - '0';
                            i += 2;
                        }
                        else
                        {
                            paramNumber = (c1 - '0') * 10 + (c2 - '0');
                            if (query[i + 3] != '}' && query[i + 3] != ':')
                                throw new NotSupportedException("Max 99 positional parameters are supported");
                            i += 3;
                        }
                        if (query[i] == ':')
                        {
                            int startPos = i + 1;
                            int endPos = query.IndexOf('}', startPos);
                            if (endPos < 0)
                                throw new ArgumentException("Missing '}' in parameter specification");

                            modifiers = SoqlParser.ParseLiteralValueModifiers(query.Substring(startPos, endPos - startPos));
                            i = endPos;
                        }
                        v = par[paramNumber];

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
                            sb.Append("'");
                            sb.Append((string)v);
                            sb.Append("'");
                        }
                        else
                        {
                            sb.Append(AddNumberedParameter(command, v, modifiers, paramNames, paramNumber));
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

        protected abstract string AddNumberedParameter(IDbCommand command, object v, SoqlLiteralValueModifiers modifiers, StringCollection paramNames, int paramNumber);

        protected abstract string GetNameForParameter(int pos);
    }
}
