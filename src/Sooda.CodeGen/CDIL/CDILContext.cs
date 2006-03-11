using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Sooda.CodeGen.CDIL
{
    public class CDILContext
    {
        public static readonly CDILContext Null = new CDILContext();
        private Hashtable _params = new Hashtable();

        public string Format(string s)
        {
            StringBuilder result = new StringBuilder();

            int startingPos = 0;
            int pos = s.IndexOf("${", startingPos);

            while (pos >= 0)
            {
                if (pos != startingPos)
                {
                    result.Append(s, startingPos, pos - startingPos);
                }
                int pos2 = s.IndexOf("}", pos + 2);
                if (pos2 >= 0)
                {
                    startingPos = pos2 + 1;
                    string item = s.Substring(pos + 2, pos2 - pos - 2);
                    string replacement = Convert.ToString(_params[item]);

                    result.Append(replacement);
                    pos = s.IndexOf("${", startingPos);
                }
                else
                {
                    break;
                }
            }
            if (startingPos != s.Length)
            {
                result.Append(s, startingPos, s.Length - startingPos);
            }

            // Console.WriteLine("res: {0}", result);

            return result.ToString();
        }

        public object this[string name]
        {
            get
            {
                return _params[name];
            }
            set
            {
                _params[name] = value;
            }
        }
    }
}
