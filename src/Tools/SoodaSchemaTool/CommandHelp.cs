using System;

namespace SoodaSchemaTool
{
    [Command("help")]
	public class CommandHelp : Command
	{
		public CommandHelp()
		{
		}

        public override string Description
        {
            get
            {
                return "display command usage information";
            }
        }

	}
}
