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
                output.WriteLine("delete from KeyGen;");
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
