// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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
using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class CollectionTest
    {
        bool predicate1(SoodaObject c0)
        {
            Contact c = (Contact)c0;
            return (string)c.Name != "Mary Manager";
        }

        [Test]
        public void GetListTest()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    Contact mary = Contact.Load(1);
                    //mary.Name = "Ala ma kota";

                    Contact ed = Contact.Load(2);
                    ed.PrimaryGroup = null;

                    ContactList list = Contact.GetList(tran, new SoodaWhereClause("PrimaryGroup.Manager.Name = 'Mary Manager'"), SoodaSnapshotOptions.Default);
                    Console.WriteLine("Len: {0}", list.Count);

                    SoodaObjectMultiFieldComparer comparer = new SoodaObjectMultiFieldComparer();

                    comparer.AddField(new string[] { "PrimaryGroup", "Name" }, SortOrder.Ascending);
                    comparer.AddField("LastSalary", SortOrder.Ascending);
                    comparer.AddField("Name", SortOrder.Descending);

                    foreach (Contact c in list.Sort(comparer).Filter(new SoodaObjectFilter(predicate1)))
                    {
                        Console.WriteLine("Member: {0} - {2} - {1}", c.Evaluate("PrimaryGroup.Name"), c.Name, c.LastSalary);
                    }
                    tran.Commit();
                }
            }
        }

        [Test]
        public void Collection1toNTest()
        {
            Collection1toNTest(false);
        }

        [Test]
        [Ignore("not implemented yet")]
        public void SharedCollection1ToNTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Group g1 = Group.GetRef(10);

                Contact newManager = new Contact();
                newManager.Type = ContactType.Manager;

                Assert.AreEqual(g1.Managers.Count, 1);
                Assert.IsTrue(g1.Members.Contains(Contact.Mary));
                Assert.IsTrue(g1.Managers.Contains(Contact.Mary));
                g1.Managers.Remove(Contact.Mary);
                Assert.AreEqual(g1.Managers.Count, 0);
                Assert.IsTrue(!g1.Members.Contains(Contact.Mary));
                Assert.IsTrue(!g1.Managers.Contains(Contact.Mary));
                g1.Managers.Add(Contact.Mary);
                Assert.AreEqual(g1.Managers.Count, 1);
                Assert.IsTrue(g1.Members.Contains(Contact.Mary));
                Assert.IsTrue(g1.Managers.Contains(Contact.Mary));
                g1.Members.Add(newManager);
                Assert.AreEqual(g1.Managers.Count, 2);
            }
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

                    Assert.AreEqual((string)g.Manager.Name, "Mary Manager");
                    Assert.AreEqual(g.Members.Count, 4);
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(53)));
                    g.Members.Remove(Contact.GetRef(53));
                    Assert.AreEqual(g.Members.Count, 3);
                    Assert.IsTrue(!g.Members.Contains(Contact.GetRef(53)));

                    g.Members.Add(c1 = new Contact());
                    c1.Name = "Nancy Newcomer";
                    c1.Active = true;
                    Assert.AreEqual(g.Members.Count, 4);
                    c1.Type = ContactType.Employee;

                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(51)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(1)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(2)));
                    int times = 0;
                    foreach (Contact c in g.Members)
                    {
                        if (!quiet)
                            Console.WriteLine("Got {0} [{1}]", c.Name, c.ContactId);
                        times++;
                        Assert.IsTrue(
                            c == Contact.GetRef(51) ||
                            c == Contact.GetRef(1) ||
                            c == c1 ||
                            c == Contact.GetRef(2));
                    };
                    Assert.AreEqual(times, 4, "foreach() loop gets called 4 times");
                    Assert.IsTrue(!g.Members.Contains(Contact.GetRef(53)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(51)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(1)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(2)));
                    Assert.IsTrue(g.Members.Contains(c1));
                    Assert.AreEqual(g.Members.Count, 4);

                    foreach (Contact c in g.Members)
                    {
                        if (!quiet)
                            Console.WriteLine("before serialization, member: {0}", c.Name);
                    }
                    serialized = tran.Serialize(SoodaSerializeOptions.IncludeNonDirtyFields | SoodaSerializeOptions.IncludeNonDirtyObjects | SoodaSerializeOptions.Canonical);
                    //serialized = tran.Serialize();
                    if (!quiet)
                        Console.WriteLine("Serialized as\n{0}", serialized);
                }

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Deserialize(serialized);
                    string serialized2 = tran.Serialize(SoodaSerializeOptions.IncludeNonDirtyFields | SoodaSerializeOptions.IncludeNonDirtyObjects | SoodaSerializeOptions.Canonical);
                    //string serialized2 = tran.Serialize();
                    if (serialized == serialized2)
                    {
                        if (!quiet)
                            Console.WriteLine("Serialization is stable");
                    }
                    else
                    {
                        if (!quiet)
                            Console.WriteLine("Serialized again as\n{0}", serialized2);
                    }
                    Assert.AreEqual(serialized, serialized2, "Serialization preserves state");

                    Group g = Group.Load(10);

                    foreach (Contact c in g.Members)
                    {
                        //if (!quiet)
                        Console.WriteLine("after deserialization, member: {0}", c.Name);
                    }
                    Assert.AreEqual("Mary Manager", g.Manager.Name);
                    Assert.AreEqual(4, g.Members.Count);
                    Assert.IsTrue(!g.Members.Contains(Contact.GetRef(53)));

                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(51)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(1)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(2)));
                    int times = 0;
                    foreach (Contact c in g.Members)
                    {
                        times++;
                        Assert.IsTrue(
                            c == Contact.GetRef(51) ||
                            c == Contact.GetRef(1) ||
                            (string)c.Name == "Nancy Newcomer" ||
                            c == Contact.GetRef(2));
                    };
                    Assert.AreEqual(times, 4, "foreach() loop gets called 4 times");
                    Assert.IsTrue(!g.Members.Contains(Contact.GetRef(53)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(51)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(1)));
                    Assert.IsTrue(g.Members.Contains(Contact.GetRef(2)));
                    Assert.AreEqual(g.Members.Count, 4);
                    tran.Commit();
                }
            }
        }
    }
}
