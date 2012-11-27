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
using System.Diagnostics;

using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.BaseObjects.TypedQueries;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Soql
{
    [TestFixture]
    public class CollectionTest
    {
        [Test]
        public void In0()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.ContactId.In(new int[0]));
                Assert.AreEqual(0, cl.Count);
                cl = Contact.GetList(ContactField.Manager.In(new Contact[0]));
                Assert.AreEqual(0, cl.Count);
                cl = Contact.GetList(ContactField.Manager.In(new ArrayList()));
                Assert.AreEqual(0, cl.Count);
            }
        }

        [Test]
        public void In1()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.ContactId.In(1));
                Assert.AreEqual(1, cl.Count);
            }
        }

        [Test]
        public void In2()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Manager.In(Contact.Mary));
                Assert.AreEqual(2, cl.Count);
            }
        }

        [Test]
        public void In3()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.ContactId.In(1, 2, 3));
                Assert.AreEqual(3, cl.Count);
            }
        }

        [Test]
        public void In4()
        {
            using (new SoodaTransaction())
            {
                ArrayList managers = new ArrayList();
                managers.Add(Contact.Mary);
                ContactList cl = Contact.GetList(ContactField.Manager.In(managers));
                Assert.AreEqual(2, cl.Count);
            }
        }

        [Test]
        public void In5()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Manager.In(ContactField.Manager));
                // "in" doesn't match nulls, at least in SQL Server
                Assert.AreEqual(2, cl.Count);
            }
        }

        [Test]
        public void In6()
        {
            using (new SoodaTransaction())
            {
                Contact c = null;
                ContactList cl = Contact.GetList(ContactField.Manager.In(c));
                // "in" doesn't match nulls, at least in SQL Server
                Assert.AreEqual(0, cl.Count);
            }
        }

        [Test]
        public void In7()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Manager.In(Role.Manager.Members));
                Assert.AreEqual(2, cl.Count);
            }
        }

        [Test]
        public void CountOnSelfReferencing()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Subordinates.Count == 2);
                Assert.AreEqual(1, cl.Count);
                Assert.AreEqual(cl[0].Name, "Mary Manager");
            }
        }

        [Test]
        public void CountOnFiltered()
        {
            using (new SoodaTransaction())
            {
                GroupList gl = Group.GetList(GroupField.Members.Count == 4);
                Assert.AreEqual(1, gl.Count);
                Assert.AreEqual(10, gl[0].Id);
                gl = Group.GetList(GroupField.Managers.Count == 1);
                Assert.AreEqual(2, gl.Count);
                gl = Group.GetList(GroupField.Managers.Count != 1);
                Assert.AreEqual(0, gl.Count);
            }
        }

        [Test]
        public void CountOnSubclassTPT()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Bikes1.Count == 1);
                Assert.AreEqual(1, cl.Count);
                Assert.AreEqual(cl[0].Name, "Ed Employee");
            }
        }

        [Test]
        public void ContainsOnSelfReferencing()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Subordinates.Contains(Contact.Ed));
                Assert.AreEqual(1, cl.Count);
                Assert.AreEqual(cl[0].Name, "Mary Manager");
            }
        }

        [Test]
        public void ContainsOnFiltered()
        {
            using (new SoodaTransaction())
            {
                GroupList gl = Group.GetList(GroupField.Members.Contains(Contact.Mary));
                Assert.AreEqual(1, gl.Count);
                gl = Group.GetList(GroupField.Managers.Contains(Contact.Mary));
                Assert.AreEqual(0, gl.Count);
            }
        }

        [Test]
        public void ContainsOnSubclassTPT()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Bikes1.Contains(Bike.GetRef(3)));
                Assert.AreEqual(1, cl.Count);
                Assert.AreEqual(cl[0].Name, "Ed Employee");
            }
        }

        [Test]
        public void ContainsOnSubclassTPTWhere()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.Bikes1.ContainsBikeWhere(BikeField.TwoWheels == 1));
                Assert.AreEqual(1, cl.Count);
                Assert.AreEqual(cl[0].Name, "Ed Employee");
            }
        }

        [Test]
        public void NestedContains()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.GetList(ContactField.PrimaryGroup.Members.Contains(3));
                Assert.AreEqual(1, cl.Count);
            }
        }

        [Test]
        public void ComplexTest5()
        {
            using (new SoodaTransaction())
            {
                GroupList gl = Group.GetList(GroupField.Manager.Name == "Mary Manager"
                    && GroupField.Members.Count > 3
                    && GroupField.Members.ContainsContactWhere(ContactField.Name == "ZZZ" && ContactField.PrimaryGroup.Members.Contains(3))
                    && GroupField.Manager.Roles.ContainsRoleWhere(RoleField.Name == "Customer"));
                Assert.AreEqual(0, gl.Count);
            }
        }
    }
}
