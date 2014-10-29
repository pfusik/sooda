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
using System.IO;
using System.Xml.Serialization;

namespace SoodaSchemaTool
{
    [Command("updateschema", "Update schema.")]
    public class CommandUpdateSchema : Command, ISchemaImporterOptions
    {
        string _databaseType = "mssql";
        string _connectionString;
        string _schemaFile;
        bool _updateTypes = false;
        bool _updateSizes = false;
        bool _updateNullable = false;
        bool _updatePrimaryKeys = false;
        string _tableName = null;
        string _outputSchemaFile = null;

        public string DatabaseType
        {
            get { return _databaseType; }
            set { _databaseType = value; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public string SchemaFile
        {
            get { return _schemaFile; }
            set { _schemaFile = value; }
        }

        public string OutputSchemaFile
        {
            get { return _outputSchemaFile; }
            set { _outputSchemaFile = value; }
        }

        public bool UpdateTypes
        {
            get { return _updateTypes; }
            set { _updateTypes = value; }
        }

        public bool UpdateSizes
        {
            get { return _updateSizes; }
            set { _updateSizes = value; }
        }

        public bool UpdateNullable
        {
            get { return _updateNullable; }
            set { _updateNullable = value; }
        }

        public bool UpdatePrimaryKeys
        {
            get { return _updatePrimaryKeys; }
            set { _updatePrimaryKeys = value; }
        }

        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public override int Run(string[] args)
        {
            SchemaImporter importer = null;

            switch (DatabaseType)
            {
                case "mssql":
                case "sqlserver":
                    importer = new MSSqlSchemaImporter();
                    break;
            }

            SchemaInfo dbSchemaInfo = importer.GetSchemaFromDatabase(this);

            XmlSerializer ser = new XmlSerializer(typeof(SchemaInfo));

            SchemaInfo currentSchemaInfo;
            using (FileStream fs = File.OpenRead(SchemaFile))
            {
                currentSchemaInfo = (SchemaInfo)ser.Deserialize(fs);
            }

            if (UpdateTypes || UpdateSizes || UpdateNullable || UpdatePrimaryKeys)
            {
                foreach (ClassInfo ci in currentSchemaInfo.Classes)
                {
                    foreach (TableInfo ti in ci.LocalTables)
                    {
                        if (TableName == null || ti.DBTableName == TableName)
                        {
                            foreach (FieldInfo fi in ti.Fields)
                            {
                                UpdateFieldFromDbSchema(ti, fi, dbSchemaInfo);
                            }
                        }
                    }
                }
            }

            if (OutputSchemaFile == null)
            {
                string oldFile = Path.ChangeExtension(SchemaFile, ".old");
                if (File.Exists(oldFile))
                    File.Delete(oldFile);
                File.Move(SchemaFile, oldFile);
                using (FileStream fs = File.Create(SchemaFile))
                {
                    ser.Serialize(fs, currentSchemaInfo);
                }
            }
            else
            {
                using (FileStream fs = File.Create(OutputSchemaFile))
                {
                    ser.Serialize(fs, currentSchemaInfo);
                }
            }

            return 0;
        }

        private FieldInfo FindDBColumnInfo(SchemaInfo schema, string table, string column)
        {
            foreach (ClassInfo ci in schema.Classes)
            {
                if (ci.Name == table)
                {
                    TableInfo ti = ci.LocalTables[0];
                    foreach (FieldInfo fi in ti.Fields)
                    {
                        if (0 == String.Compare(fi.DBColumnName, column, true))
                            return fi;
                    }
                }
            }
            return null;
        }

        private void UpdateFieldFromDbSchema(TableInfo ti, FieldInfo fi, SchemaInfo dbSchema)
        {
            FieldInfo dbfi = FindDBColumnInfo(dbSchema, ti.DBTableName, fi.DBColumnName);
            if (dbfi == null)
            {
                Console.WriteLine("WARNING. FIELD NOT FOUND IN DB: " + ti.DBTableName + "," + fi.DBColumnName);
                return;
            }
            if (UpdateTypes)
                fi.DataType = dbfi.DataType;
            if (UpdateSizes)
                fi.Size = dbfi.Size;
            if (UpdateNullable)
                fi.IsNullable = dbfi.IsNullable;
            if (UpdatePrimaryKeys)
                fi.IsPrimaryKey = dbfi.IsPrimaryKey;
        }
    }
}
