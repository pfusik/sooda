/*

Copyright (c) 2002,2003 Jaroslaw Kowalski <jaak@polbox.com>
All rights reserved.

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions 
are met:

* Redistributions of source code must retain the above copyright notice, 
this list of conditions and the following disclaimer. 

* Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution. 

* Neither the name of Jaroslaw Kowalski nor the names of its 
contributors may be used to endorse or promote products derived from this
software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
THE POSSIBILITY OF SUCH DAMAGE.

*/
using System;
using System.IO;
using System.Xml;

class Monodoc2NDoc
{
	static bool wroteAssemblyInfo = false;
	static int Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine("Monodoc2NDoc - converts documentation from monodoc to ndoc format");
			Console.WriteLine();
			Console.WriteLine("Usage: monodoc2ndoc outputfile.xml dir1 dir2 .. dirN"); 
			Console.WriteLine("");
			Console.WriteLine("\toutputfile.xml is the name of file to create.");
			Console.WriteLine("\tdir1..N are directory names to scan for documentation XML files");
			return 1;
		}

		XmlTextWriter xw = new XmlTextWriter(args[0], System.Text.Encoding.UTF8);
		xw.Formatting = Formatting.Indented;

		for (int i = 1; i < args.Length; ++i)
		{
			ProcessDirectory(xw, new DirectoryInfo(args[i]));
		}
		xw.Close();
		return 0;
	}

	private static void ProcessDirectory(XmlWriter xw, DirectoryInfo dirInfo)
	{
		Console.WriteLine("Processing directory {0}", dirInfo.FullName);

		foreach (DirectoryInfo subdir in dirInfo.GetDirectories())
		{
			ProcessDirectory(xw, subdir);
		}

		foreach (FileInfo fi in dirInfo.GetFiles())
		{
			if (fi.Extension != ".xml")
			{
				Console.WriteLine("Ignoring file: " + fi.FullName);
				continue;
			}
			XmlDocument doc = new XmlDocument();
			Console.WriteLine("Loading {0}", fi.FullName);
			doc.Load(fi.FullName);

			if (!wroteAssemblyInfo)
			{
				xw.WriteStartDocument();
				xw.WriteStartElement("doc");
				xw.WriteStartElement("assembly");
				xw.WriteElementString("name", doc.SelectSingleNode("Type/AssemblyInfo/AssemblyName").InnerText);
				xw.WriteEndElement();
				xw.WriteStartElement("members");
				wroteAssemblyInfo = true;
			}

			if (doc.DocumentElement.LocalName != "Type")
			{
				Console.WriteLine("Ignoring file: " + fi.FullName);
				continue;
			}

			string typeName = doc.SelectSingleNode("Type/@FullName").InnerText;

			xw.WriteStartElement("member");
			xw.WriteAttributeString("name", "T:" + typeName);
			foreach (XmlElement el in doc.SelectNodes("Type/Docs/*"))
			{
				el.WriteTo(xw);
			};
			xw.WriteEndElement();

			foreach (XmlElement member in doc.SelectNodes("Type/Members/Member"))
			{
				xw.WriteStartElement("member");
				xw.WriteAttributeString("name", GetMemberNameString(member, typeName));
				foreach (XmlElement el in member.SelectNodes("Docs/*"))
				{
					el.WriteTo(xw);
				};
				xw.WriteEndElement();
			}
		}
	}

	private static string GetParametersString(XmlElement member)
	{
		string par = "";
		XmlNodeList nodes = member.SelectNodes("Parameters/Parameter");
		if (nodes.Count != 0)
		{
			par += "(";
			bool first = true;
			foreach (XmlElement el in nodes)
			{
				if (!first)
					par += ",";
				first = false;
				par += el.GetAttribute("Type");
			}
			par += ")";
		}
		return par;
	}

	private static string GetMemberNameString(XmlElement member, string typeName)
	{
		switch (member.SelectSingleNode("MemberType").InnerText)
		{
			case "Event":
				return "E:" + typeName + "." + member.GetAttribute("MemberName") + GetParametersString(member);

			case "Method":
				return "M:" + typeName + "." + member.GetAttribute("MemberName") + GetParametersString(member);

			case "Field":
				return "F:" + typeName + "." + member.GetAttribute("MemberName") + GetParametersString(member);

			case "Property":
				return "P:" + typeName + "." + member.GetAttribute("MemberName") + GetParametersString(member);

			case "Constructor":
				return "M:" + typeName + ".#ctor" + GetParametersString(member);

			default:
				throw new NotSupportedException("Unsupported MemberType: " + member.SelectSingleNode("MemberType").InnerText);
		}
	}
}
