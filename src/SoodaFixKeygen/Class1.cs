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
using System.Xml;
using Sooda.Schema;
using System.IO;

namespace SoodaFixKeygen
{
        class Class1
        {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SoodaFixKeyGen [project.SchemaFilename] [output_sql]");
                return 1;
            }
            SchemaInfo schemaInfo;

            XmlTextReader reader = new XmlTextReader(args[0]);
            schemaInfo = SchemaManager.ReadAndValidateSchema(reader, Path.GetDirectoryName(args[0]));
            reader.Close();

            using (StreamWriter output = new StreamWriter(args[1], false, System.Text.Encoding.Default))
            {
                output.WriteLine("delete from KeyGen");
                output.WriteLine("GO");
                bool first = true;
                output.WriteLine("insert into KeyGen");
                foreach (ClassInfo classInfo in schemaInfo.Classes)
                {
                    if (classInfo.GetPrimaryKeyFields().Length != 1)
                        continue;

                    FieldInfo fieldInfo = classInfo.GetFirstPrimaryKeyField();
                    if (fieldInfo.DataType == FieldDataType.Integer || fieldInfo.DataType == FieldDataType.Long)
                    {
                        if (!first)
                            output.WriteLine("union");
                    
                        output.WriteLine("select '{0}',coalesce(max({1}),0) + 1 from {2}", 
                            classInfo.Name, fieldInfo.DBColumnName, classInfo.UnifiedTables[0].DBTableName);
                        first = false;
                    }
                }
            }
            return 0;
        }
        }
}
