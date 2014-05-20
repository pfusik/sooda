// 
// Copyright (c) 2014 Piotr Fusik <piotr@fusik.info>
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
using System.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class CollectionQueryTest
    {
        [Test]
        public void OneToMany()
        {
            using (new SoodaTransaction())
            {
                Contact[] subordinates = Contact.Mary.SubordinatesQuery.ToArray();
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Ed, Contact.Eva }, subordinates);

                Bike[] bikes = Contact.Ed.Bikes1Query.ToArray();
                CollectionAssert.AreEquivalent(new Bike[] { Bike.GetRef(3) }, bikes);
            }
        }

        [Test]
        public void OneToManyWithWhere()
        {
            using (new SoodaTransaction())
            {
                Contact[] managers = Group.GetRef(10).ManagersQuery.ToArray();
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Ed }, managers);
            }
        }

        [Test]
        public void ManyToMany()
        {
            using (new SoodaTransaction())
            {
                Role[] roles = Contact.Mary.RolesQuery.ToArray();
                CollectionAssert.AreEquivalent(new Role[] { Role.Employee, Role.Manager }, roles);
            }
        }

        [Test]
        public void OneToManyWhere()
        {
            using (new SoodaTransaction())
            {
                Contact[] subordinates = Contact.Mary.SubordinatesQuery.Where(c => c.ContactId >= 3).ToArray();
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Eva }, subordinates);

                Bike[] bikes = Contact.Ed.Bikes1Query.Where(b => b.Id == 3).ToArray();
                CollectionAssert.AreEquivalent(new Bike[] { Bike.GetRef(3) }, bikes);

                bikes = Contact.Ed.Bikes1Query.Where(b => b.Id == 2).ToArray();
                CollectionAssert.AreEquivalent(new Bike[0], bikes);
            }
        }

        [Test]
        public void ManyToManyWhere()
        {
            using (new SoodaTransaction())
            {
                Role[] roles = Contact.Mary.RolesQuery.Where(r => r.Name.Value == "Manager").ToArray();
                CollectionAssert.AreEquivalent(new Role[] { Role.Manager }, roles);
            }
        }

        [Test]
        public void OneToManyAny()
        {
            using (new SoodaTransaction())
            {
                bool a = Contact.Mary.SubordinatesQuery.Where(c => c.ContactId >= 3).Any();
                Assert.IsTrue(a);

                a = Contact.Ed.Bikes1Query.Where(b => b.Id == 3).Any();
                Assert.IsTrue(a);

                a = Contact.Ed.Bikes1Query.Where(b => b.Id == 2).Any();
                Assert.IsFalse(a);
            }
        }

        [Test]
        public void ManyToManyAny()
        {
            using (new SoodaTransaction())
            {
                bool a = Contact.Mary.RolesQuery.Where(r => r.Name.Value == "Manager").Any();
                Assert.IsTrue(a);

                a = Contact.Mary.RolesQuery.Where(r => r.Name.Value == "Foo Bar").Any();
                Assert.IsFalse(a);
            }
        }
    }
}
