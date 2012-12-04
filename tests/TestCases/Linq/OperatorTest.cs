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

#if DOTNET35

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class OperatorTest
    {
        [Test]
        public void StringEquals()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name == "Mary Manager");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);

                ce = Contact.Linq().Where(c => c.Name != "Mary Manager");
                Assert.AreEqual(6, ce.Count());
            }
        }

        [Test]
        public void BoolTest()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Active);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => !c.Active);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => c.Active && c.Name == "Ed Employee");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Ed, ce.First());

                ce = Contact.Linq().Where(c => c.Name == "Chuck Customer" || c.LastSalary.Value == 345);
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => c.PrimaryGroup == null & c.LastSalary.Value == 123);
                Assert.AreEqual(1, ce.Count());

                ce = Contact.Linq().Where(c => c.PrimaryGroup == null | c.LastSalary.Value == 123);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void BoolEquals()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Active == true);
                Assert.AreEqual(7, ce.Count());
                ce = Contact.Linq().Where(c => c.Active == false);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => c.Active != false);
                Assert.AreEqual(7, ce.Count());
                ce = Contact.Linq().Where(c => c.Active != true);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => true == c.Active);
                Assert.AreEqual(7, ce.Count());
                ce = Contact.Linq().Where(c => false == c.Active);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => false != c.Active);
                Assert.AreEqual(7, ce.Count());
                ce = Contact.Linq().Where(c => true != c.Active);
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void ChainedEquals()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Active == true == true);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => c.Active == true == true != false);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => true == (c.Active == true));
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => false != (true == (c.Active == true)));
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void Equivalence()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => (c.Active == true) == (c.Active == true));
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void DecimalCompare()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.LastSalary.Value == 234);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Ed, ce.First());

                ce = Contact.Linq().Where(c => c.LastSalary.Value == -1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(-1, ce.First().LastSalary.Value);

                ce = Contact.Linq().Where(c => c.LastSalary.Value == 3.14159M);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(3.14159M, ce.First().LastSalary.Value);

                ce = Contact.Linq().Where(c => c.LastSalary.Value == 123.123456789M);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(123.123456789M, ce.First().LastSalary.Value);

                ce = Contact.Linq().Where(c => c.LastSalary.Value < 123);
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().Where(c => c.LastSalary.Value <= 123);
                Assert.AreEqual(4, ce.Count());

                ce = Contact.Linq().Where(c => c.LastSalary.Value > 123);
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().Where(c => c.LastSalary.Value >= 123);
                Assert.AreEqual(4, ce.Count());
            }
        }

        // just to make sure it's not compile-time constant
        protected virtual Contact GetNullContact()
        {
            return null;
        }

        [Test]
        public void IsNull()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.LastSalary.IsNull);
                Assert.AreEqual(0, ce.Count());
                ce = Contact.Linq().Where(c => !c.LastSalary.IsNull);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == null);
                Assert.AreEqual(5, ce.Count());
                ce = Contact.Linq().Where(c => c.Manager != null);
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == GetNullContact());
                Assert.AreEqual(5, ce.Count());
                ce = Contact.Linq().Where(c => c.Manager != GetNullContact());
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => null == c.Manager);
                Assert.AreEqual(5, ce.Count());
                ce = Contact.Linq().Where(c => null != c.Manager);
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => GetNullContact() == c.Manager);
                Assert.AreEqual(5, ce.Count());
                ce = Contact.Linq().Where(c => GetNullContact() != c.Manager);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void IsNullComparison()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.LastSalary.IsNull == true);
                Assert.AreEqual(0, ce.Count());
                ce = Contact.Linq().Where(c => c.LastSalary.IsNull != true);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => (c.Manager == null) == true);
                Assert.AreEqual(5, ce.Count());
                ce = Contact.Linq().Where(c => (c.Manager != null) != false);
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => (c.Manager == GetNullContact()) != false);
                Assert.AreEqual(5, ce.Count());
                ce = Contact.Linq().Where(c => (c.Manager != GetNullContact()) == true);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void PathExpression()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.PrimaryGroup.Name.Value == "Group1");
                Assert.AreEqual(4, ce.Count());
                ce = Contact.Linq().Where(c => c.Manager.Name == "Mary Manager");
                Assert.AreEqual(2, ce.Count());
                ce = Contact.Linq().Where(c => c.Manager.PrimaryGroup.Manager.ContactId == 1);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void Add()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.ContactId + c.PrimaryGroup.Id == 61);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(51, ce.First().ContactId);
            }
        }

        [Test]
        public void Subtract()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.ContactId - c.PrimaryGroup.Id == 41);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(51, ce.First().ContactId);
            }
        }

        [Test]
        public void Multiply()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.ContactId * c.PrimaryGroup.Id == 33);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(3, ce.First().ContactId);
            }
        }

        [Test]
        public void Divide()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.PrimaryGroup.Id / c.ContactId == 10);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(1, ce.First().ContactId);
            }
        }

        [Test]
        public void Modulo()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.ContactId % 10 == 2);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void UnaryPlus()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => +c.ContactId == 50);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(50, ce.First().ContactId);
            }
        }

        [Test]
        public void Negate()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => -c.ContactId == -50);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(50, ce.First().ContactId);
            }
        }

        [Test]
        public void Coalesce()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => (c.Manager ?? c) == Contact.Mary);
                Assert.AreEqual(3, ce.Count());
            }
        }

        [Test]
        public void Conditional()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => (GetNullContact() == null ? c : c.Manager) == c);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => (GetNullContact() != null ? c : c.Manager) == c);
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void TypeIs()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c is Contact);
                Assert.AreEqual(7, ce.Count());

                IEnumerable<Vehicle> ve = Vehicle.Linq().Where(c => c is Car);
                Assert.AreEqual(2, ve.Count());
            }
        }

        [Test]
        public void TypeIs2()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Vehicle> ve = Vehicle.Linq().Where(v => v.Owner is Contact);
                Assert.AreEqual(2, ve.Count());
            }
        }

        [Test]
        public void TypeIs3()
        {
            using (new SoodaTransaction())
            {
                Contact.Mary.Vehicles.Add(Bike.GetRef(6));
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Vehicles.Any(v => v is Bike));
                Assert.AreEqual(1, ce.Count());
            }
        }

        [Test]
        public void TypeIsObject()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Manager is object);
                Assert.AreEqual(2, ce.Count());
            }
        }
    }
}

#endif
