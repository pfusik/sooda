using System;

namespace SoodaSchemaTool
{
    [Command("dumpschema")]
	public class CommandDumpSchema : Command
	{
		public CommandDumpSchema()
		{
		}

        public override string Description
        {
            get
            {
                return "generate schema based on a existing database.";
            }
        }

	}
}
