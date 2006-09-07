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
using System.Xml;

using Sooda.Schema;
using Sooda.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace SoodaSchemaTool
{
    public class MSSqlSchemaImporter : SchemaImporter
    {
        private DataSet dataSet = new DataSet();
        private ISchemaImporterOptions _options;
        private bool _haveIdentityColumns = false;

        public override SchemaInfo GetSchemaFromDatabase(ISchemaImporterOptions options)
        {
            _options = options;
            using (SqlConnection conn = new SqlConnection(options.ConnectionString))
            {
                SqlDataAdapter dataAdapter;

                conn.Open();
                Console.WriteLine("Loading tables...");
                dataAdapter = new SqlDataAdapter(SqlStrings.TablesQuery, conn);
                dataAdapter.Fill(dataSet, "Tables");

                Console.WriteLine("Loading datatypes...");
                dataAdapter.SelectCommand.CommandText = "exec sp_datatype_info";
                dataAdapter.Fill(dataSet, "DataTypes");

                Console.WriteLine("Loading columns...");
                dataAdapter.SelectCommand.CommandText = "exec sp_Columns @table_name='%'";
                dataAdapter.Fill(dataSet, "Columns");

                Console.WriteLine("Loading primary keys...");
                dataAdapter.SelectCommand.CommandText = SqlStrings.PrimaryKeysQuery;
                dataAdapter.Fill(dataSet, "PrimaryKeys");

                Console.WriteLine("Loading foreign keys...");
                dataAdapter.SelectCommand.CommandText = SqlStrings.ForeignKeysQuery;
                dataAdapter.Fill(dataSet, "ForeignKeys");
            }

            SchemaInfo si = new SchemaInfo();

            foreach (DataRow dr in dataSet.Tables["Tables"].Rows)
            {
                DumpTable(si, (string)dr["TABLE_OWNER"], (string)dr["TABLE_NAME"]);
            }

            DumpForeignKeys(si);

            if (_haveIdentityColumns)
                Console.WriteLine("WARNING: Identity columns are not supported. Sooda will not use them, but will provide its own pre-generated key.");

            return si;
        }

        private string MakePascalCase(string str)
        {
            if (str != str.ToUpper() && str != str.ToLower())
                return str.Replace(" ","");

            string[] pieces = str.Split('_',' ');
            StringBuilder sb = new StringBuilder();
            foreach (string s in pieces)
            {
                if (s.Length == 1)
                {
                    sb.Append(s.ToUpper());
                }
                else if (s.Length == 2)
                {
                    sb.Append(Char.ToUpper(s[0]));
                    if (s[1] == 'y')
                    {
                        sb.Append('y');
                    }
                    else
                    {
                        sb.Append(Char.ToUpper(s[1]));
                    }
                }
                else
                {
                    sb.Append(Char.ToUpper(s[0]));
                    sb.Append(s.Substring(1).ToLower());
                }
            }
            return sb.ToString();
        }

        private void DumpForeignKeys(SchemaInfo schemaInfo)
        {
            DataRow[] fkeys = dataSet.Tables["ForeignKeys"].Select("1=1", "SOURCE_TABLE_NAME, FKEY_NAME");

            for (int i = 0; i < fkeys.Length; ++i)
            {
                DataRow r = fkeys[i];
                string curSrcTableOwner = r["SOURCE_TABLE_OWNER"].ToString();
                string curSrcTableName = r["SOURCE_TABLE_NAME"].ToString();
                string curSrcColumnName = r["SOURCE_COLUMN_NAME"].ToString();
                string curDstTableOwner = r["DEST_TABLE_OWNER"].ToString();
                string curDstTableName = r["DEST_TABLE_NAME"].ToString();
                string curDstColumnName = r["DEST_COLUMN_NAME"].ToString();

                foreach (ClassInfo sourceTable in schemaInfo.Classes)
                {
                    if (sourceTable.LocalTables[0].DBTableName == curSrcTableName)
                    {
                        foreach (FieldInfo fi in sourceTable.LocalTables[0].Fields)
                        {
                            if (fi.DBColumnName == curSrcColumnName)
                            {
                                fi.References = MakePascalCase(curDstTableName);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void GetSoodaFieldAttributes(FieldInfo fi, DataRow r, bool recursive)
        {
            string typeName = r["TYPE_NAME"].ToString();

            switch (typeName)
            {
                case "int":
                case "smallint":
                case "tinyint":
                    fi.DataType = FieldDataType.Integer;
                    break;

                case "int identity":
                case "tinyint identity":
                case "smallint identity":
                    fi.DataType = FieldDataType.Integer;
                    _haveIdentityColumns = true;
                    break;

                case "char":
                case "varchar":
                    fi.DataType = FieldDataType.AnsiString;
                    fi.Size = Convert.ToInt32(r["PRECISION"]);
                    break;

                case "text":
                    fi.DataType = FieldDataType.AnsiString;
                    fi.Size = unchecked((1 << 31) - 1);
                    break;

                case "ntext":
                    fi.DataType = FieldDataType.String;
                    fi.Size = unchecked((1 << 31) - 1);
                    break;

                case "nchar":
                case "nvarchar":
                    fi.DataType = FieldDataType.String;
                    fi.Size = Convert.ToInt32(r["PRECISION"]);
                    break;

                case "datetime":
                    fi.DataType = FieldDataType.DateTime;
                    break;

                case "smallmoney":
                case "money":
                case "decimal":
                    fi.DataType = FieldDataType.Decimal;
                    break;

                case "float":
                case "numeric":
                case "real":
                    fi.DataType = FieldDataType.Double;
                    break;

                case "bigint":
                    fi.DataType = FieldDataType.Long;
                    break;

                case "image":
                    fi.DataType = FieldDataType.Image;
                    break;

                case "binary":
                case "varbinary":
                case "blob":
                    fi.DataType = FieldDataType.Blob;
                    break;

                case "bit":
                    fi.DataType = FieldDataType.Boolean;
                    break;

                case "uniqueidentifier":
                    fi.DataType = FieldDataType.Guid;
                    break;

                default:
                    if (recursive)
                        throw new Exception("Unable to determine the base type for " + typeName);

                    DataRow[] udt = dataSet.Tables["DataTypes"].Select("DATA_TYPE='" + r["DATA_TYPE"] + "' and USERTYPE < 256");
                    if (udt.Length < 1)
                        throw new Exception("Unsupported data type: " + typeName);
                    GetSoodaFieldAttributes(fi, udt[0], true);
                    break;
            }
            fi.IsNullable = (Convert.ToInt32(r["NULLABLE"]) != 0);
        }

        private void DumpTable(SchemaInfo schemaInfo, string owner, string table)
        {
            Console.WriteLine("Dumping table {0}.{1}", owner, table);

            ClassInfo ci = new ClassInfo();
            ci.Name = MakePascalCase(table);

            TableInfo ti = new TableInfo();
            ci.LocalTables = new TableInfoCollection();
            ti.DBTableName = table;
            ci.LocalTables.Add(ti);

            foreach (DataRow r in dataSet.Tables["Columns"].Select("TABLE_NAME='" + table + "' and TABLE_OWNER='" + owner + "'", "ORDINAL_POSITION"))
            {
                try
                {
                    string columnName = r["COLUMN_NAME"].ToString();

                    FieldInfo fi = new FieldInfo();
                    fi.Name = MakePascalCase(columnName);
                    fi.DBColumnName = columnName;
                    GetSoodaFieldAttributes(fi, r, false);
                    ti.Fields.Add(fi);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WARNING: {0}", ex.Message);
                }
            }

            bool hasPrimaryKey = false;

            foreach (DataRow r in dataSet.Tables["PrimaryKeys"].Select("TABLE_NAME='" + table + "' and TABLE_OWNER='" + owner + "'"))
            {
                string column = Convert.ToString(r["COLUMN_NAME"]);
                foreach (FieldInfo fi in ti.Fields)
                {
                    if (0 == String.Compare(fi.DBColumnName, column, true))
                    {
                        fi.IsPrimaryKey = true;
                        hasPrimaryKey = true;
                    }
                }
            }
            if (!hasPrimaryKey)
            {
                Console.WriteLine("WARNING: Created artificial primary key from the first column of the " + ti.DBTableName + " table. This may be incorrect.");
                ti.Fields[0].IsPrimaryKey = true;
            }
            schemaInfo.Classes.Add(ci);
        }

        #region SQL Strings

        class SqlStrings
        {
            public const string TablesQuery = @"
select	o.name as TABLE_NAME, 
	user_name(o.uid) as TABLE_OWNER
from
	dbo.sysobjects o
where	OBJECTPROPERTY(o.id, N'IsTable') = 1 
	and OBJECTPROPERTY(o.id, N'IsMSShipped')=0 
	and OBJECTPROPERTY(o.id, N'IsSystemTable')=0
order by TABLE_OWNER, TABLE_NAME
";

            public const string ViewsQuery = @"
select	o.name as VIEW_NAME, 
	user_name(o.uid) as VIEW_OWNER
from
	dbo.sysobjects o
where	OBJECTPROPERTY(o.id, N'IsView') = 1 
	and OBJECTPROPERTY(o.id, N'IsMSShipped')=0 
order by VIEW_OWNER, VIEW_NAME
";

            public const string ProceduresQuery = @"
select
	o.name as PROCEDURE_NAME,  
	user_name(o.uid) as PROCEDURE_OWNER,
	OBJECTPROPERTY(o.id, N'ExecIsStartup') as IS_STARTUP, 
	OBJECTPROPERTY(o.id, N'ExecIsQuotedIdentOn') as QUOTED_IDENTIFIERS, 
	OBJECTPROPERTY(o.id, N'ExecIsAnsiNullsOn') as ANSI_NULLS
from
	dbo.sysobjects o 
where (OBJECTPROPERTY(o.id, N'IsProcedure') = 1 or 
	OBJECTPROPERTY(o.id, N'IsExtendedProc') = 1 or 
	OBJECTPROPERTY(o.id, N'IsReplProc') = 1) and 
	o.name not like N'#%%' and
	OBJECTPROPERTY(o.id, N'IsMSShipped') = 0
order by o.name";

            public const string PrimaryKeysQuery = @"
SELECT	x.name as CONSTRAINT_NAME,
	s.name as TABLE_NAME, 
	u.name as TABLE_OWNER, 
	c.name as COLUMN_NAME
FROM	sysindexes x 
	INNER JOIN sysobjects s ON x.id = s.id 
	LEFT JOIN sysusers u on s.uid = u.uid 
	INNER JOIN sysindexkeys k ON k.id = x.id AND k.indid = x.indid 
	INNER JOIN syscolumns c ON c.id = x.id AND c.colid = k.colid 
	JOIN sysobjects cs ON cs.parent_obj = x.id and cs.xtype ='PK' and cs.name = x.name 
ORDER BY TABLE_OWNER,TABLE_NAME
";
            public const string ForeignKeysQuery = @"
SELECT	fkeyobject.name as FKEY_NAME, 
	t0.name as SOURCE_TABLE_NAME, 
	t0usr.name as SOURCE_TABLE_OWNER, 
	sc.name as SOURCE_COLUMN_NAME, 
	t1.name as DEST_TABLE_NAME, 
	t1usr.name as DEST_TABLE_OWNER, 
	tc.name as DEST_COLUMN_NAME, 
	OBJECTPROPERTY(fkeyobject.id, 'CnstIsNotRepl') as NOT_FOR_REPLICATION,
	OBJECTPROPERTY(fkeyobject.id, 'CnstIsDisabled') as IS_DISABLED , 
	OBJECTPROPERTY(fkeyobject.id, 'CnstIsDeleteCascade') as IS_CASCADE_DELETE, 
	OBJECTPROPERTY(fkeyobject.id, 'CnstIsUpdateCascade') as IS_CASCADE_UPDATE 
FROM	sysobjects fkeyobject
	JOIN sysforeignkeys fk ON fkeyobject.id = fk.constid 
	LEFT JOIN sysobjects t0  ON fk.fkeyid = t0.id 
	LEFT JOIN sysusers   t0usr  ON t0.uid   = t0usr.uid 
	LEFT JOIN sysobjects t1  ON fk.rkeyid = t1.id 
	LEFT JOIN sysusers   t1usr ON t1.uid   = t1usr.uid 
	LEFT JOIN syscolumns sc   ON fk.fkeyid = sc.id AND fk.fkey = sc.colid 
	LEFT JOIN syscolumns tc ON fk.rkeyid = tc.id AND fk.rkey = tc.colid 
WHERE fkeyobject.xtype = 'F' 
ORDER BY fkeyobject.id, fk.keyno
";
            public const string PermissionsQuery = @"
select	su.name as GRANTEE,
	case sp.action 
		when 26 then 'REFERENCES' 
		when 178 then 'CREATE FUNCTION'
		when 193 then 'SELECT'
		when 195 then 'INSERT'
		when 196 then 'DELETE'
		when 197 then 'UPDATE'
		when 198 then 'CREATE TABLE'
		when 203 then 'CREATE DATABASE'
		when 207 then 'CREATE VIEW'
		when 222 then 'CREATE PROCEDURE'
		when 224 then 'EXECUTE'
		when 228 then 'BACKUP DATABASE'
		when 233 then 'CREATE DEFAULT'
		when 235 then 'BACKUP LOG'
		when 236 then 'CREATE RULE'
	end as PRIVILEGE,
	case sp.protecttype 
		when 205 then 'GRANT'
		when 206 then 'DENY'
	end as PROTECT_TYPE,
	user_name(so.uid) as SCHEMA_NAME,
	so.name as OBJECT_NAME,
	case sp.action when 224 then 1 else 0 end as ORDERING
from	sysprotects sp 
	left outer join sysusers su on su.uid = sp.uid
	left outer join sysobjects so on (sp.id = so.id)
where
	OBJECTPROPERTY(so.id, N'IsMSShipped')=0
order by ORDERING,3,4,2,1
";

            public const string IndexesQuery = @"
	select	TABLE_CATALOG		= db_name(),
		TABLE_SCHEMA		= user_name(o.uid),
		TABLE_NAME		= o.name,
		INDEX_CATALOG		= db_name(),		
		INDEX_SCHEMA		= user_name(o.uid),
		INDEX_NAME		= x.name,
		PRIMARY_KEY		= convert(bit,(x.status & 0x800)/0x800),
		""UNIQUE""		= convert(bit,(x.status & 2)/2),
        ""CLUSTERED""		= convert(bit,(x.status & 16)/16),
        ""TYPE""			= convert(smallint, 1),
        FILL_FACTOR		= convert(int, x.OrigFillFactor),
        INITIAL_SIZE		= convert(int,null),
        NULLS			= convert(int,null),
        SORT_BOOKMARKS		= convert(bit,0),
        AUTO_UPDATE		= convert(bit,1),
        NULL_COLLATION		= convert(int,4),
        ORDINAL_POSITION 	= convert(int, xk.keyno),
        COLUMN_NAME		= c.name,
        COLUMN_GUID		= convert(uniqueidentifier,null),
        COLUMN_PROPID		= convert(int,null),
        COLLATION	= convert(smallint,
                          case when indexkey_property(o.id, x.indid, xk.keyno, 'IsDescending') =1
        then 2
        else 1
        end),
        CARDINALITY		= case when (x.status & 2) = 2 then x.rows else null end,
        PAGES			= convert(int, x.dpages),
        FILTER_CONDITION	= convert(nvarchar(1),null),
        INTEGRATED		= convert(bit,(x.status & 16)/16) 
		
        from	sysobjects o, sysindexes x, syscolumns c, sysindexkeys xk
                                  where	o.type in ('U')
        and	x.id = o.id
        and	o.id = c.id
        and	o.id = xk.id
        and	x.indid = xk.indid
        and	c.colid = xk.colid
                                                                                              and	xk.keyno <= x.keycnt
        and     (x.status & 32) = 0  -- No hypothetical indexes
order by 8 desc, 4, 5, 6, 17
";


            public static bool TypeNeedsPrecision(string typeName)
            {
                switch (typeName)
                {
                    case "char":
                    case "decimal":
                    // case "float":
                    case "nchar":
                    case "nvarchar":
                    case "numeric":
                    case "smallmoney":
                    case "varbinary":
                    case "varchar":
                        return true;

                    default:
                        return false;
                }

            }

            public static bool TypeNeedsScale(string typeName)
            {
                switch (typeName)
                {
                    case "decimal":
                    case "numeric":
                    case "smallmoney":
                        return true;

                    default:
                        return false;
                }
            }

        }
        #endregion
    }
}
