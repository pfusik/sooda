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
using Sooda.Schema;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Sooda.ObjectMapper
{
    public class SchemaLoader
    {
        private static SchemaInfo schemaInfo = null;
        public static SchemaInfo GetSchemaFromAssembly(System.Reflection.Assembly ass)
        {
            if (schemaInfo == null)
            {
                DateTime dt0 = DateTime.Now;
                lock (typeof(SchemaLoader))
                {
                    if (schemaInfo == null)
                    {
                        foreach (string name in ass.GetManifestResourceNames())
                        {
                            if (name.EndsWith("_DBSchema.bin"))
                            {
                                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                                using (Stream resourceStream = ass.GetManifestResourceStream(name))
                                {
                                    schemaInfo = (SchemaInfo)bf.Deserialize(resourceStream);
                                    schemaInfo.Resolve();
                                }
                                break;
                            };
                            if (name.EndsWith("_DBSchema.xml"))
                            {
                                using (Stream resourceStream = ass.GetManifestResourceStream(name))
                                {
                                    XmlSerializer ser = new XmlSerializer(typeof(Sooda.Schema.SchemaInfo));
                                    XmlTextReader reader = new XmlTextReader(resourceStream);

                                    schemaInfo = (Sooda.Schema.SchemaInfo)ser.Deserialize(reader);
                                    schemaInfo.Resolve();
                                }
                                break;
                            };
                        }
                        if (schemaInfo == null)
                        {
                            throw new InvalidOperationException("_DBSchema.xml not embedded in " + ass.CodeBase);
                        };
                    };
                }
                DateTime dt1 = DateTime.Now;
                //Console.WriteLine("Schema loaded in {0}", dt1 - dt0);
            };
            return schemaInfo;
        }
    } // class _DatabaseSchemaLoader
}
