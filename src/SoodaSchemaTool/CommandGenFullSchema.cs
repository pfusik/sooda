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
using System.Xml.Serialization;
using Sooda.Schema;
using Sooda.Sql;
using System.IO;

namespace SoodaSchemaTool
{
    [Command("genfullschema","Generate single file for merged schema")]
    public class CommandGenFullSchema : Command
    {
        public CommandGenFullSchema()
        {
        }

        public override int Run(string[] args)
        {
            string schemaFileName = args[0];
            string outschemaFileName = args[1];

            XmlTextReader xr = new XmlTextReader(schemaFileName);
            SchemaInfo schemaInfo = SchemaManager.ReadAndValidateSchema(xr, Path.GetDirectoryName(schemaFileName));
            schemaInfo.Includes.Clear();
            schemaInfo.Classes.SortByName();
            schemaInfo.Relations.SortByName();

            XmlSerializer ser = new XmlSerializer(typeof(SchemaInfo));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", SchemaInfo.XmlNamespace);

            using (FileStream fs = File.Create(outschemaFileName))
            {
                try
                {
                    ser.Serialize(fs, schemaInfo, ns);
                }
                finally
                {
                    fs.Flush();
                    fs.Close();
                }
            }

            return 0;
        }

    }
}
