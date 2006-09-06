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
using System.IO;
using System.Data;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Threading;

using Sooda;
using Sooda.Sql;
using Sooda.Schema;
using Sooda.QL;
using Sooda.ObjectMapper;

using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Sooda.Caching;

using System.Security.Principal;
using System.Security.Permissions;

using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects.TypedQueries;
using Sooda.UnitTests.TestCases;

[assembly: SoodaStubAssembly(typeof(Sooda.UnitTests.Objects._DatabaseSchema))]
[assembly: SoodaConfig(XmlConfigFileName = "Sooda.config.xml")]

namespace ConsoleTest
{
    class Class1
    {
        static void Main(string[] args)
        {
            // connect to the data source 'default' defined 
            // in the configuration file

            using (SqlDataSource sds = new SqlDataSource("default"))
            {
                sds.Open();

                // SOQL textual query
                string soqlQuery = @"
                    SELECT Name, PrimaryGroup.Name, PrimaryGroup.Members.Count
                    FROM Contact 
                    WHERE PrimaryGroup.Manager.Name = {0} OR Name = {1}";

                // parse the query to a SoqlQueryExpression object
                SoqlQueryExpression queryExpression = SoqlParser.ParseQuery(soqlQuery);

                // get the schema reference
                SchemaInfo schema = Sooda.UnitTests.BaseObjects._DatabaseSchema.GetSchema();

                // prepare parameters
                object[] parameters = new object[] 
                {
                    "Mary Manager", // positional parameter {0}
                    "Eva Employee" // positional parameter {1}
                };

                // execute query
                using (IDataReader reader = sds.ExecuteQuery(queryExpression, schema, parameters))
                {
                    // iterate the result set
                    while (reader.Read())
                    {
                        Console.WriteLine("name: {0} group: {1} members: {2}", 
                            reader.GetString(0), 
                            reader.GetString(1), 
                            reader.GetInt32(2));
                    }
                }
            }
        }
    }
}

