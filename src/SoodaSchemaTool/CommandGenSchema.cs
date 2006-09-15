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
using System.Collections;

namespace SoodaSchemaTool
{
    [Command("genschema", "Generate schema (*.xml) from database structure")]
    public class CommandGenSchema : Command, ISchemaImporterOptions
	{
        private string _databaseType = "mssql";
        private string _connectionString;
        private string _outputFile;

		public CommandGenSchema()
		{
		}

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

        public string OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
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

            SchemaInfo schemaInfo = importer.GetSchemaFromDatabase(this);
            AutoDetectRelations(schemaInfo);
            schemaInfo.Resolve();
            RemoveReferencePrimaryKeys(schemaInfo);
            AutoDetectCollections(schemaInfo);

            DataSourceInfo dsi = new DataSourceInfo();
            dsi.Name = "default";
            dsi.DataSourceType = "Sooda.Sql.SqlDataSource";
            schemaInfo.DataSources.Add(dsi);
            foreach (ClassInfo ci in schemaInfo.Classes)
            {
                int pkCount = 0;
                foreach (FieldInfo fi in ci.LocalTables[0].Fields)
                {
                    if (fi.IsPrimaryKey)
                    {
                        pkCount++;
                    }
                }
                if (pkCount == 0)
                    Console.WriteLine("WARNING: Table {0} doesn't have a primary key.", ci.Name);
            }
            XmlSerializer ser = new XmlSerializer(typeof(SchemaInfo));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", SchemaInfo.XmlNamespace);

            using (FileStream fs = File.Create(OutputFile))
            {
                ser.Serialize(fs, schemaInfo, ns);
            }

            return 0;
        }

        private void AutoDetectRelations(SchemaInfo si)
        {
            foreach (ClassInfo ci in new ArrayList(si.Classes))
            {
                if (ci.LocalTables[0].Fields.Count != 2)
                    continue;

                if (!(ci.LocalTables[0].Fields[0].IsPrimaryKey && ci.LocalTables[0].Fields[1].IsPrimaryKey))
                    continue;

                if (ci.LocalTables[0].Fields[0].References == null || ci.LocalTables[0].Fields[1].References == null)
                    continue;

                RelationInfo ri = new RelationInfo();
                ri.Name = ci.Name;
                ri.Table.DBTableName = ci.LocalTables[0].DBTableName;
                ri.Table.Fields.Add(ci.LocalTables[0].Fields[0]);
                ri.Table.Fields.Add(ci.LocalTables[0].Fields[1]);
                si.Relations.Add(ri);

                Console.WriteLine("Converting {0} to a relation", ci.Name);
                si.Classes.Remove(ci);
            }
        }

        private void RemoveReferencePrimaryKeys(SchemaInfo si)
        {
            // for classes, primary keys which are references are not supported
            foreach (ClassInfo ci in si.Classes)
            {
                foreach (FieldInfo fi in ci.UnifiedFields)
                {
                    if (fi.IsPrimaryKey)
                        fi.References = null;
                }
            }
        }

        private void AutoDetectCollections(SchemaInfo si)
        {
            int counter = 0;
            foreach (ClassInfo ci in si.Classes)
            {
                foreach (FieldInfo fi in ci.LocalTables[0].Fields)
                {
                    if (fi.ReferencedClass != null)
                    {
                        ArrayList al = new ArrayList();
                        if (fi.ReferencedClass.Collections1toN != null)
                        {
                            al.AddRange(fi.ReferencedClass.Collections1toN);
                        }
                        CollectionOnetoManyInfo coll = new CollectionOnetoManyInfo();
                        coll.Name = "CollectionOf" + ci.Name + "" + counter++;
                        coll.ClassName = ci.Name;
                        coll.ForeignFieldName = fi.Name;
                        al.Add(coll);
                        
                        fi.ReferencedClass.Collections1toN = (CollectionOnetoManyInfo[])al.ToArray(typeof(CollectionOnetoManyInfo));
                        // ci.Collections1toN
                    }
                }
            }
            foreach (RelationInfo ri in si.Relations)
            {
                CollectionManyToManyInfo mm;
                ArrayList al;

                al = new ArrayList();
                if (ri.Table.Fields[0].ReferencedClass.CollectionsNtoN != null)
                    al.AddRange(ri.Table.Fields[0].ReferencedClass.CollectionsNtoN);

                mm = new CollectionManyToManyInfo();
                mm.Name = "CollectionOf" + ri.Table.Fields[1].ReferencedClass.Name + "" + counter++;
                mm.Relation = ri.Name;
                mm.ForeignField = ri.Table.Fields[0].Name;
                al.Add(mm);

                ri.Table.Fields[0].ReferencedClass.CollectionsNtoN = (CollectionManyToManyInfo[])al.ToArray(typeof(CollectionManyToManyInfo));

                al = new ArrayList();
                if (ri.Table.Fields[1].ReferencedClass.CollectionsNtoN != null)
                    al.AddRange(ri.Table.Fields[1].ReferencedClass.CollectionsNtoN);

                mm = new CollectionManyToManyInfo();
                mm.Name = "CollectionOf" + ri.Table.Fields[0].ReferencedClass.Name + "" + al.Count;
                mm.Relation = ri.Name;
                mm.ForeignField = ri.Table.Fields[1].Name;
                al.Add(mm);

                ri.Table.Fields[1].ReferencedClass.CollectionsNtoN = (CollectionManyToManyInfo[])al.ToArray(typeof(CollectionManyToManyInfo));
            
            }
        }
	}
}
