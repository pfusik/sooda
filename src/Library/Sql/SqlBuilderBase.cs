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

		public abstract string GetSQLDataType(Sooda.Schema.FieldInfo fi);

		public virtual string CreatePrimaryKey(string name, string table, string column) {
			return String.Format("ALTER TABLE {1} ADD CONSTRAINT {0} PRIMARY KEY ({2})",
					name, table, column);
		}

		public virtual string CreatePrimaryKey2(string name, string table, string column1, string column2) {
			return String.Format("ALTER TABLE {1} ADD CONSTRAINT {0} PRIMARY KEY ({2},{3})",
					name, table, column1, column2);
		}

		public virtual string CreateForeignKey(string name, string sourceTable, string sourceColumn, string destTable, string destColumn) {
			return String.Format("ALTER TABLE {1} ADD CONSTRAINT {0} FOREIGN KEY ({2}) REFERENCES {3}({4})",
					name, sourceTable, sourceColumn, destTable, destColumn);
		}

		public abstract void BuildCommandWithParameters(IDbCommand command, string query, object[] par);

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
			paramTypes[typeof(byte[])] = DbType.Binary;
			paramTypes[typeof(System.Drawing.Image)] = DbType.Binary;
			paramTypes[typeof(System.Drawing.Bitmap)] = DbType.Binary;
		}

		public string QuoteFieldName(string s) 
		{
			return String.Concat("[", s, "]");
		}

		public abstract SqlTopSupportMode TopSupport
		{
			get;
		}
	}
}
