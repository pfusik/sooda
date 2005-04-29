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
using System.Data;
using System.IO;
using System.Collections;

namespace Sooda.Sql {
	public abstract class SqlBuilderBase : ISqlBuilder {
		public virtual string GetDDLCommandTerminator() {
			return ";" + Environment.NewLine;
		}

		public virtual SqlOuterJoinSyntax OuterJoinSyntax
		{
			get {
				return SqlOuterJoinSyntax.Ansi;
			}
		}

		public void GenerateCreateTableField(TextWriter xtw, Sooda.Schema.FieldInfo fieldInfo) {
			Console.Write("\t{0} {1} {2}", fieldInfo.DBColumnName, GetSQLDataType(fieldInfo), fieldInfo.IsNullable ? "null" : "not null");
		}

		public void GenerateCreateTable(TextWriter xtw, Sooda.Schema.TableInfo tableInfo) {
			xtw.WriteLine("create table {0} (", tableInfo.DBTableName);
			for (int i = 0; i < tableInfo.Fields.Count; ++i) {
				GenerateCreateTableField(xtw, tableInfo.Fields[i]);
				if (i == tableInfo.Fields.Count - 1)
					Console.WriteLine();
				else
					Console.WriteLine(",");
			}
			xtw.Write(")");
			xtw.Write(GetDDLCommandTerminator());
		}

		public void GeneratePrimaryKey(TextWriter xtw, Sooda.Schema.TableInfo tableInfo) {
            bool first = true;
            
            foreach (Sooda.Schema.FieldInfo fi in tableInfo.Fields) {
                if (fi.IsPrimaryKey) {
                    if (first) {
                        xtw.Write("alter table {0} add primary key (", tableInfo.DBTableName);
                    } else {
                        xtw.Write(", ");
                    }
                    xtw.Write(fi.DBColumnName);
                    first = false;
                }
            }
            if (!first)
            {
                xtw.Write(")");
                xtw.Write(GetDDLCommandTerminator());
            }
		}

		public void GenerateForeignKeys(TextWriter xtw, Sooda.Schema.TableInfo tableInfo) {
            foreach (Sooda.Schema.FieldInfo fi in tableInfo.Fields) {
                if (fi.References != null) {
                    xtw.Write("alter table {0} add constraint FK_{0}_{1} foreign key ({2}) references {3}({4})", 
                            tableInfo.DBTableName, fi.DBColumnName, fi.DBColumnName,
                            fi.ReferencedClass.LocalTables[0].DBTableName, fi.ReferencedClass.GetFirstPrimaryKeyField().DBColumnName
                            );
                    xtw.Write(GetDDLCommandTerminator());
                }
            }
        }
        
		public abstract string GetSQLDataType(Sooda.Schema.FieldInfo fi);

		public abstract void BuildCommandWithParameters(IDbCommand command, bool append, string query, object[] par);

		protected virtual bool SetDbTypeFromClrType(IDbDataParameter parameter, Type clrType) {
			object o = paramTypes[clrType];
			if (o == null)
				return false;
			parameter.DbType = (DbType)o;
			return true;
		}

		private static Hashtable paramTypes = new Hashtable();

		static SqlBuilderBase() {
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

		public virtual string QuoteFieldName(string s) 
		{
			return String.Concat("[", s, "]");
		}

		public abstract SqlTopSupportMode TopSupport
		{
			get;
		}
	}
}
