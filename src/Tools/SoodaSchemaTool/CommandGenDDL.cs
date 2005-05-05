using System;

namespace SoodaSchemaTool
{
    [Command("genddl")]
	public class CommandGenDDL : Command
	{
		public CommandGenDDL()
		{
		}

        public override string Description
        {
            get
            {
                return "generate DDL from schema";
            }
        }

	}
}
