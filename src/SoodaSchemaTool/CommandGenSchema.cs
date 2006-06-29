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
            XmlSerializer ser = new XmlSerializer(typeof(SchemaInfo));

            using (FileStream fs = File.Create(OutputFile))
            {
                ser.Serialize(fs, schemaInfo);
            }


            return 0;
        }
	}
}
