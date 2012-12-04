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

using Sooda.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class QueryTest
    {
        [Test]
        public void Linq()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq();
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void ToArray()
        {
            using (new SoodaTransaction())
            {
                Contact[] ca = Contact.Linq().ToArray();
                Assert.AreEqual(7, ca.Length);
            }
        }

        [Test]
        public void ToList()
        {
            using (new SoodaTransaction())
            {
                List<Contact> cl = Contact.Linq().ToList();
                Assert.AreEqual(7, cl.Count);
            }
        }

        // not implemented yet
        //[Test]
        //public void ToSoodaList()
        //{
        //    using (new SoodaTransaction())
        //    {
        //        ContactList cl = new ContactList(Contact.Linq());
        //        or? ContactList cl = Contact.Linq().ToSoodaList();
        //        Assert.AreEqual(7, cl.Count);
        //    }
        //}

        [Test]
        public void Where()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name == "Mary Manager");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);

                ce = from c in Contact.Linq() where c.Name == "Mary Manager" select c;
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);
            }
        }

        [Test]
        public void WhereBool()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => true);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => false);
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void Select()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => c.ContactId);
                Assert.AreEqual(7, ie.Count());
                CollectionAssert.AreEquivalent(new int[] { 1, 2, 3, 50, 51, 52, 53 }, ie);

                IEnumerable<bool> be = Contact.Linq().Select(c => c.Active);
                Assert.AreEqual(7, be.Count());
                Assert.IsTrue(be.All(b => b));
            }
        }

        [Test]
        public void SelectIndexed()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().OrderBy(c => c.ContactId).Select((c, i) => c.ContactId + i);
                CollectionAssert.AreEqual(new int[] { 1 + 0, 2 + 1, 3 + 2, 50 + 3, 51 + 4, 52 + 5, 53 + 6 }, ie);
            }
        }

        [Test]
        public void OrderBy()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().OrderBy(c => c.ContactId);
                CollectionAssert.AreEqual(new int[] { 1, 2, 3, 50, 51, 52, 53 }, ce.Select(c => c.ContactId));

                ce = Contact.Linq().OrderByDescending(c => c.ContactId);
                CollectionAssert.AreEqual(new int[] { 53, 52, 51, 50, 3, 2, 1 }, ce.Select(c => c.ContactId));

                ce = Contact.Linq().OrderBy(c => c.Name);
                CollectionAssert.AreEqual(new string[] { "Caroline Customer", "Catie Customer", "Chris Customer", "Chuck Customer", "Ed Employee", "Eva Employee", "Mary Manager" }, ce.Select(c => c.Name));

                ce = Contact.Linq().OrderByDescending(c => c.Name);
                CollectionAssert.AreEqual(new string[] { "Mary Manager", "Eva Employee", "Ed Employee", "Chuck Customer", "Chris Customer", "Catie Customer", "Caroline Customer" }, ce.Select(c => c.Name));
            }
        }

        [Test]
        public void ThenBy()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = from c in Contact.Linq() orderby c.PrimaryGroup.Id descending, c.Name select c;
                CollectionAssert.AreEqual(new int[] { 3, 51, 53, 2, 1, 50, 52 }, ce.Select(c => c.ContactId));

                ce = from c in Contact.Linq() orderby c.PrimaryGroup.Id descending, c.Name descending select c;
                CollectionAssert.AreEqual(new int[] { 3, 1, 2, 53, 51, 52, 50 }, ce.Select(c => c.ContactId));
            }
        }

        [Test]
        public void Reverse()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = (from c in Contact.Linq() orderby c.PrimaryGroup.Id descending, c.Name select c).Reverse();
                CollectionAssert.AreEqual(new int[] { 52, 50, 1, 2, 53, 51, 3 }, ce.Select(c => c.ContactId));
            }
        }

        [Test]
        public void Take()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Take(3);
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().Take(0);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Take(-5);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Take(1);
                Assert.AreEqual(1, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Take(4);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void Except()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Except(new Contact[] { Contact.Ed });
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Eva, ce.First());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Except(Contact.Linq().Where(c => c.ContactId == 3));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Ed, ce.First());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Except(Contact.Linq());
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void Intersect()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Intersect(new Contact[] { Contact.Ed });
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Ed, ce.First());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Intersect(Contact.Linq().Where(c => c.ContactId == 50));
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Intersect(Contact.Linq());
                Assert.AreEqual(2, ce.Count());
            }
        }


        [Test]
        public void Union()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Union(new Contact[] { Contact.Ed });
                Assert.AreEqual(2, ce.Count());
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Ed, Contact.Eva }, ce);

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Union(Contact.Linq().Where(c => c.ContactId == 50));
                Assert.AreEqual(3, ce.Count());
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Ed, Contact.Eva, Contact.GetRef(50) }, ce);

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Union(Contact.Linq());
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void TakeWhere()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Take(2).Where(c => c.ContactId > 0);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void TakeOrderBy()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().OrderBy(c => c.ContactId).Take(2).OrderBy(c => c.Name).ToArray();
                CollectionAssert.AreEqual(new Contact[] { Contact.Ed, Contact.Mary }, ce);
            }
        }

        [Test]
        public void TakeReverse()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Take(2).Reverse();
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void TakeUnion()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().OrderBy(c => c.ContactId).Take(2).Union(new Contact[] { Contact.Eva });
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().OrderBy(c => c.ContactId).Take(2).Union(Contact.Linq().Where(c => c.ContactId == 3));
                Assert.AreEqual(3, ce.Count());
            }
        }

        [Test]
        public void UnionTake()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.ContactId == 3).Union(Contact.Linq().OrderBy(c => c.ContactId).Take(2));
                Assert.AreEqual(3, ce.Count());
            }
        }

        [Test]
        public void TakeAll()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().Take(4).All(c => c.ContactId < 1000);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void TakeAnyFiltered()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().Take(7).Any(c => c.ContactId == 51);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void TakeScalar()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().OrderBy(c => c.ContactId).Take(2).Sum(c => c.ContactId);
                Assert.AreEqual(3, result);
            }
        }

        [Test]
        public void Let()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce =
                    from c in Contact.Linq()
                    let initial = c.Name.Remove(1)
                    where initial == "C"
                    select c;
                Assert.AreEqual(4, ce.Count());
            }
        }

        [Test]
        public void ComplexQuery()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie =
                    from c in Contact.Linq()
                    where c.Name.Like("C%") && c.Active
                    where c != Contact.Mary
                    orderby c.LastSalary.Value descending
                    select c.ContactId;
                CollectionAssert.AreEqual(new int[] { 52, 50, 51, 53 }, ie);
            }
        }
    }
}

#endif
