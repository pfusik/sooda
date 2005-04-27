using System;

namespace Sooda.StubGen.CDIL
{
	public class CDILContext
	{
        public static readonly CDILContext Null = new CDILContext();
        private object[] _params = new object[100];

        public string Format(string s)
        {
            return String.Format(s, _params);
        }

        public string this[int p]
        {
            get
            {
                return (string)_params[p];
            }
            set
            {
                _params[p] = value;
            }
        }
	}
}
