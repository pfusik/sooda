// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Threading;

using Sooda;
using Sooda.Schema;
using Sooda.QL;
using Sooda.ObjectMapper;

using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Sooda.Caching;

using System.Security.Principal;
using System.Security.Permissions;

using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.BaseObjects.TypedQueries;

[assembly: SoodaStubAssembly(typeof(Sooda.UnitTests.BaseObjects._DatabaseSchema))]
[assembly: SoodaConfig(XmlConfigFileName = "sooda.config.xml")]

namespace ConsoleTest
{
    class Class1
    {
        static void Removed(object sender, LruCacheEventArgs args)
        {
            //Console.WriteLine("removed: {0}", args.Key);
        }

        static void Main(string[] args)
        {
            using (SoodaTransaction t = new SoodaTransaction())
            {
                Console.WriteLine(SoqlParser.ParseExpression("3 - 2 - 1").Evaluate(null));
                Console.WriteLine(SoqlParser.ParseExpression("6 / 3 * 2").Evaluate(null));

                Console.WriteLine(SoqlParser.ParseExpression("1 + 2 * 3").Evaluate(null));
                Console.WriteLine(SoqlParser.ParseExpression("1 + 2 * 3 - 4 * 5").Evaluate(null));
                Console.WriteLine(SoqlParser.ParseExpression("1 - 4 / 2 - 4 % 2").Evaluate(null));
                //Contact c = Contact.Eva;
                //Console.WriteLine(c.Name);
            }
        }
    }
}

