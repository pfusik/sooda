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