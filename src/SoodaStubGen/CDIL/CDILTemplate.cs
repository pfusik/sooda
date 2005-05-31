using System;
using System.IO;

namespace Sooda.StubGen.CDIL
{
	public class CDILTemplate
	{
        public static string Get(string name)
        {
            using (Stream stream = typeof(CDILTemplate).Assembly.GetManifestResourceStream("Sooda.StubGen.CDIL.Templates." + name))
            {
                StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8, false);
                return sr.ReadToEnd();
            }
        }
	}
}
