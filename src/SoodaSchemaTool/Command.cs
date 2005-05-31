using System;

namespace SoodaSchemaTool
{
	public abstract class Command
	{
		public Command()
		{
		}

        public abstract string Description
        {
            get;
        }
	}
}
