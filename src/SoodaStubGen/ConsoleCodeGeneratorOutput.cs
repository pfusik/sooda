using System;
using Sooda.CodeGen;

namespace SoodaStubGen
{
	public class ConsoleCodeGeneratorOutput : ICodeGeneratorOutput
	{
        public void Verbose(string s, params object[] p)
        {
            Console.WriteLine(s, p);
        }

        public void Info(string s, params object[] p)
        {
            Console.WriteLine(s, p);
        }

        public void Warning(string s, params object[] p)
        {
            Console.WriteLine(s, p);
        }
    }
}
