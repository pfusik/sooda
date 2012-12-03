// 
// Copyright (c) 2012 Piotr Fusik <piotr@fusik.info>
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Sooda.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class CollectionTest
    {
        [Test]
        public void In0()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => new int[0].Contains(c.ContactId));
                Assert.AreEqual(0, ce.Count());
                ce = Contact.Linq().Where(c => new Contact[0].Contains(c));
                Assert.AreEqual(0, ce.Count());
                ce = Contact.Linq().Where(c => new ArrayList().Contains(c.Manager));
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void In1()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => new int[] { 1 }.Contains(c.ContactId));
                Assert.AreEqual(1, ce.Count());
            }
        }

        [Test]
        public void In2()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => new Contact[] { Contact.Mary }.Contains(c.Manager));
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void In3()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => new int[] { 1, 2, 3 }.Contains(c.ContactId));
                Assert.AreEqual(3, ce.Count());
            }
        }

        [Test]
        public void In4()
        {
            using (new SoodaTransaction())
            {
                ArrayList managers = new ArrayList();
                managers.Add(Contact.Mary);
                IEnumerable<Contact> ce = Contact.Linq().Where(c => managers.Contains(c.Manager));
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void In5()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => new Contact[] { c.Manager }.Contains(c.Manager));
                // "in" doesn't match nulls, at least in SQL Server
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void In6()
        {
            using (new SoodaTransaction())
            {
                Contact c0 = null;
                IEnumerable<Contact> ce = Contact.Linq().Where(c => new Contact[] { c0 }.Contains(c.Manager));
                // "in" doesn't match nulls, at least in SQL Server
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void In7()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Role.Manager.Members.Contains(c.Manager));
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void CountOnSelfReferencing()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Subordinates.Count == 2);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);
            }
        }

        [Test]
        public void CountOnFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Group> ge = Group.Linq().Where(g => g.Members.Count == 4);
                Assert.AreEqual(1, ge.Count());
                Assert.AreEqual(10, ge.First().Id);
                ge = Group.Linq().Where(g => g.Managers.Count() == 1);
                Assert.AreEqual(2, ge.Count());
                ge = Group.Linq().Where(g => g.Managers.Count != 1);
                Assert.AreEqual(0, ge.Count());
            }
        }

        [Test]
        public void CountOnSubclassTPT()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Bikes1.Count == 1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Ed Employee", ce.First().Name);
            }
        }

        [Test]
        public void ContainsOnSelfReferencing()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Subordinates.Contains(Contact.Ed));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);
            }
        }

        [Test]
        public void ContainsOnFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Group> ge = Group.Linq().Where(g => g.Members.Contains(Contact.Mary));
                Assert.AreEqual(1, ge.Count());
                ge = Group.Linq().Where(g => g.Managers.Contains(Contact.Mary));
                Assert.AreEqual(0, ge.Count());
            }
        }

        [Test]
        public void ContainsOnSubclassTPT()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Bikes1.Contains(Bike.GetRef(3)));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Ed Employee", ce.First().Name);
            }
        }

        [Test]
        public void ContainsOnSubclassTPTWhere()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Bikes1.Any(b => b.TwoWheels == 1));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Ed Employee", ce.First().Name);
            }
        }

        [Test]
        public void NestedContains()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.PrimaryGroup.Members.Contains(Contact.GetRef(3)));
                Assert.AreEqual(1, ce.Count());
            }
        }

        [Test]
        public void ComplexTest5()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Group> ge = Group.Linq().Where(g =>
                    g.Manager.Name == "Mary Manager"
                    && g.Members.Count > 3
                    && g.Members.Any(c => c.Name == "ZZZ" && c.PrimaryGroup.Members.Contains(Contact.GetRef(3)))
                    && g.Manager.Roles.Any(r => r.Name.Value == "Customer"));
                Assert.AreEqual(0, ge.Count());
            }
        }

        [Test]
        public void OneToManyAll()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Subordinates.All(s => s.Name.Like("E% Employee")));
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void OneToManyAny()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Subordinates.Any());
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void OneToManyAnyFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Subordinates.Any(s => s.Name == "Ed Employee"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void AnyWithOuterRange()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Subordinates.Any(s => s == c));
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void AnyWithRangeVariableComparison()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Group> ge = Group.Linq().Where(g => g.Members.Any(c => c == Contact.Mary));
                Assert.AreEqual(1, ge.Count());
            }
        }

        [Test]
        public void AnyWithSoodaClass()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Group> ge = Group.Linq().Where(g => g.Members.Any(c => g.GetType().Name == "Group" && c.GetType().Name == "Contact"));
                Assert.AreEqual(2, ge.Count());
            }
        }
    }
}
