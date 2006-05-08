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
using System.Xml;

using Sooda.Schema;
using Sooda.Sql;
using System.Reflection;
using System.Collections;

namespace SoodaSchemaTool 
{
    public class ConsoleRunner 
    {
        private static Hashtable commands = new Hashtable();

        private static void GenerateDDLForSchema(string schemaFileName) 
        {
            XmlTextReader xr = new XmlTextReader(schemaFileName);
            SchemaInfo schemaInfo = SchemaManager.ReadAndValidateSchema(xr, Path.GetDirectoryName(schemaFileName));

            SqlDataSource sds = new SqlDataSource(schemaInfo.GetDataSourceInfo("default"));
            sds.SqlBuilder = new SqlServerBuilder();
            sds.GenerateDdlForSchema(schemaInfo, Console.Out);
        }

        private static void Usage()
        {
            Console.WriteLine("Copyright (c) 2005 by Jaroslaw Kowalski. All rights reserved.");
            Console.WriteLine("Usage: SoodaSchemaTool command [arguments]");
            Console.WriteLine();
            Console.WriteLine("Where command can be one of:");
            Console.WriteLine();
            foreach (string s in commands.Keys)
            {
                Command c = (Command)Activator.CreateInstance((Type)commands[s]);

                Console.WriteLine("    {0} - {1}", s, c.Description);
            }
        }

        public static int Main(string[] args) 
        {
            foreach (Type t in Assembly.GetCallingAssembly().GetTypes())
            {
                CommandAttribute ca = (CommandAttribute)Attribute.GetCustomAttribute(t, typeof(CommandAttribute));
                if (ca != null)
                {
                    commands[ca.Name] = t;
                }
            }
            if (args.Length == 0)
            {
                Usage();
                return 1;
            }

            switch (args[0]) 
            {
                case "genddl":
                    GenerateDDLForSchema(args[1]);
                    break;
            }
            return 0;
        }
    }
}