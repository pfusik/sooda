// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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
using System.Diagnostics;
using System.Data;

using Sooda.ObjectMapper;
using Sooda.UnitTests.Objects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class SerializationTest
    {
        [Test]
        public void Bug1()
        {   
            string ser1, ser2;

            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Group g1 = new Group();
                Group g2 = new Group();
                Contact c1 = new Contact();
                Contact c2 = new Contact();
                Contact c3 = new Contact();
                g1.Members.Add(c1);
                g1.Members.Add(c2);
                g1.Members.Add(c3);
                g1.Manager = c1;
                g2.Manager = c2;

                ser1 = tran.Serialize(SerializeOptions.Canonical);
            }
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                tran.Deserialize(ser1);

                ser2 = tran.Serialize(SerializeOptions.Canonical);
                Console.WriteLine("ser1 {0}", ser1);
                Console.WriteLine("ser2 {0}", ser2);
                Assertion.AssertEquals("Serialization is stable", ser1, ser2);
            }
        }

        [Test]
        public void Collection1toNTest()
        {
            Collection1toNTest(false);
        }

        public void Collection1toNTest(bool quiet)
        {
            string serialized;

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    Contact c1;
                    Group g = Group.Load(10);

                    Assertion.AssertEquals((string)g.Manager.Name, "Mary Manager");
                    Assertion.AssertEquals(g.Members.Count, 4);
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(53)));
                    g.Members.Remove(Contact.GetRef(53));
                    Assertion.AssertEquals(g.Members.Count, 3);
                    Assertion.Assert(!g.Members.Contains(Contact.GetRef(53)));

                    g.Members.Add(c1 = new Contact());
                    c1.Name = "Nancy Newcomer";
                    c1.Active = 1;
                    c1.Type = ContactType.Employee;
                    Assertion.AssertEquals(g.Members.Count, 4);

                    //Console.WriteLine(ContactType.Employee.GetPrimaryKeyValue());
                    //Console.WriteLine(ContactType.Employee.Code);
                    //Console.WriteLine(ContactType.Employee.Description);

                    Assertion.Assert(g.Members.Contains(Contact.GetRef(51)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(1)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(2)));
                    int times = 0;
                    foreach (Contact c in g.Members)
                    {
                        if (!quiet)
                            Console.WriteLine("Got {0} [{1}]", c.Name, c.ContactId);
                        times++;
                        Assertion.Assert(
                            c == Contact.GetRef(51) ||
                            c == Contact.GetRef(1) ||
                            c == c1 ||
                            c == Contact.GetRef(2));
                    };
                    Assertion.AssertEquals("foreach() loop gets called 4 times", times, 4);
                    Assertion.Assert(!g.Members.Contains(Contact.GetRef(53)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(51)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(1)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(2)));
                    Assertion.Assert(g.Members.Contains(c1));
                    Assertion.AssertEquals(g.Members.Count, 4);

                    foreach (Contact c in g.Members)
                    {
                        if (!quiet)
                            Console.WriteLine("before serialization, member: {0}", c.Name);
                    }
                    serialized = tran.Serialize(SerializeOptions.IncludeNonDirtyFields | SerializeOptions.IncludeNonDirtyObjects);
                    //serialized = tran.Serialize();
                    if (!quiet)
                        Console.WriteLine("Serialized as\n{0}", serialized);
                }

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Deserialize(serialized);

                    Console.WriteLine(ContactType.Employee.Description);
                    string serialized2 = tran.Serialize(SerializeOptions.IncludeNonDirtyFields | SerializeOptions.IncludeNonDirtyObjects);
                    //string serialized2 = tran.Serialize();
                    if (serialized == serialized2)
                    {
                        if (!quiet)
                            Console.WriteLine("Serialization is stable\n{0}", serialized);
                    }
                    else
                    {
                        if (!quiet)
                            Console.WriteLine("Serialized again as\n{0}", serialized2);
                    }
                    Assertion.AssertEquals("Serialization preserves state", serialized, serialized2);

                    Group g = Group.Load(10);

                    foreach (Contact c in g.Members)
                    {
                        //if (!quiet)
                        Console.WriteLine("after deserialization, member: {0}", c.Name);
                    }
                    Assertion.AssertEquals("Mary Manager", g.Manager.Name);
                    Assertion.AssertEquals(4, g.Members.Count);
                    Assertion.Assert(!g.Members.Contains(Contact.GetRef(53)));

                    Assertion.Assert(g.Members.Contains(Contact.GetRef(51)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(1)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(2)));
                    int times = 0;
                    foreach (Contact c in g.Members)
                    {
                        times++;
                        Assertion.Assert(
                            c == Contact.GetRef(51) ||
                            c == Contact.GetRef(1) ||
                            (string)c.Name == "Nancy Newcomer" ||
                            c == Contact.GetRef(2));
                    };
                    Assertion.AssertEquals("foreach() loop gets called 4 times", times, 4);
                    Assertion.Assert(!g.Members.Contains(Contact.GetRef(53)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(51)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(1)));
                    Assertion.Assert(g.Members.Contains(Contact.GetRef(2)));
                    Assertion.AssertEquals(g.Members.Count, 4);
                    tran.Commit();
                }
            }
        }
    }
}
