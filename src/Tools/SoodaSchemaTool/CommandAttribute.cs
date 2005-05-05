using System;

namespace SoodaSchemaTool
{
    [AttributeUsage(AttributeTargets.Class)]
	public class CommandAttribute : Attribute
	{
        private string _name;

		public CommandAttribute(string name)
		{
            _name = name;
		}

        public string Name
        {
            get { return _name; }
        }
	}
}
