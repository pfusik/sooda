using System;
using System.Xml;

using Sooda.Schema;
using Sooda.Sql;

namespace SoodaSchemaTool
{
	public class ConsoleRunner
	{
		private static void GenerateDDLForSchema(string schemaFileName)
		{
			XmlTextReader xr = new XmlTextReader(schemaFileName);
			SchemaInfo schemaInfo = SchemaManager.ReadAndValidateSchema(xr);

			SqlDataSource sds = new SqlDataSource("default");
			sds.SqlBuilder = new SqlServerBuilder();
			sds.GenerateDdlForSchema(schemaInfo, Console.Out);
		}

		public static int Main(string[] args)
		{
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
