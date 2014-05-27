// 
// Copyright (c) 2012-2014 Piotr Fusik <piotr@fusik.info>
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
using System.Data.SqlTypes;
using System.Linq;

using Sooda.Linq;

using NUnit.Framework;
using Sooda.UnitTests.Objects;
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

        [Test]
        public void ToSoodaObjectList()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = new ContactList(Contact.Linq().ToSoodaObjectList());
                Assert.AreEqual(7, cl.Count);
                CollectionAssert.Contains(cl, Contact.Mary);
            }
        }

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
        public void SelectNew()
        {
            using (new SoodaTransaction())
            {
                var a = (from c in Contact.Linq() orderby c.ContactId select new { c.Name, c.Active }).ToArray();
                Assert.AreEqual("Mary Manager", a.First().Name);
                Assert.IsTrue(a.All(o => o.Active));
            }
        }

        [Test]
        public void SelectNewSkipTake()
        {
            using (new SoodaTransaction())
            {
                var a = (from c in Contact.Linq() orderby c.ContactId select new { c.Name, c.Active }).Skip(2).Take(1).ToArray();
                Assert.AreEqual("Eva Employee", a.Single().Name);
                Assert.AreEqual(true, a.Single().Active);
            }
        }

        [Test]
        public void SelectSoodaObject()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> oe = from c in Contact.Linq() orderby c.ContactId select c.Manager;
                CollectionAssert.AreEqual(new Contact[] { null, Contact.Mary, Contact.Mary, null, null, null, null }, oe);
            }
        }

        [Test]
        public void SelectPath()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() orderby c.ContactId select c.Manager.Name;
                CollectionAssert.AreEqual(new string[] { null, "Mary Manager", "Mary Manager", null, null, null, null }, se);
            }
        }

        [Test]
        public void SelectNewSoodaObject()
        {
            using (new SoodaTransaction())
            {
                var oa = (from c in Contact.Linq() orderby c.ContactId select new { c.Manager }).ToArray();
                CollectionAssert.AreEqual(new SoodaObject[] { null, Contact.Mary, Contact.Mary, null, null, null, null }, oa.Select(o => o.Manager));
            }
        }

        [Test]
        public void SelectNewPath()
        {
            using (new SoodaTransaction())
            {
                var oa = (from c in Contact.Linq() orderby c.ContactId select new { c.Manager.Name }).ToArray();
                CollectionAssert.AreEqual(new string[] { null, "Mary Manager", "Mary Manager", null, null, null, null }, oa.Select(o => o.Name));
            }
        }

        [Test]
        public void SelectAggregate()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() orderby c.ContactId select c.Subordinates.Count;
                CollectionAssert.AreEqual(new int[] { 2, 0, 0, 0, 0, 0, 0 }, ie);

                ie = from c in Contact.Linq() orderby c.ContactId select c.Subordinates.Count();
                CollectionAssert.AreEqual(new int[] { 2, 0, 0, 0, 0, 0, 0 }, ie);

                IEnumerable<bool> be = from c in Contact.Linq() orderby c.ContactId select c.Subordinates.Any();
                CollectionAssert.AreEqual(new bool[] { true, false, false, false, false, false, false }, be);
            }
        }

        [Test]
        public void SelectNewAggregate()
        {
            using (new SoodaTransaction())
            {
                var l = (from c in Contact.Linq() orderby c.ContactId select new { c.Name, SubordinatesCount = c.Subordinates.Count, SubordinatesCount2 = c.Subordinates.Count(), HasSubordinates = c.Subordinates.Any() }).ToList();
                CollectionAssert.AreEqual(new int[] { 2, 0, 0, 0, 0, 0, 0 }, l.Select(o => o.SubordinatesCount));
                CollectionAssert.AreEqual(new int[] { 2, 0, 0, 0, 0, 0, 0 }, l.Select(o => o.SubordinatesCount2));
                CollectionAssert.AreEqual(new bool[] { true, false, false, false, false, false, false }, l.Select(o => o.HasSubordinates));
            }
        }

        [Test]
        public void SelectConst()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => 42);
                CollectionAssert.AreEqual(Enumerable.Repeat(42, 7), ie);
            }
        }

        [Test]
        public void SelectNewObject()
        {
            using (new SoodaTransaction())
            {
                object[] oa = Contact.Linq().Select(c => new object()).ToArray();
                Assert.AreEqual(7, oa.Length);
                CollectionAssert.AllItemsAreUnique(oa);
            }
        }

        /* error CS0834: A lambda expression with a statement body cannot be converted to an expression tree
        [Test]
        public void SelectDictionary()
        {
            using (new SoodaTransaction())
            {
                Dictionary<string, object>[] da = Contact.Linq().OrderBy(c => c.ContactId).Take(2).Select(c => {
                        Dictionary<string, object> d = new Dictionary<string, object>();
                        d.Add("Id", c.ContactId);
                        d.Add("Name", c.Name);
                        d.Add("SubordinatesCount", c.Subordinates.Count);
                        return d;
                    }).ToArray();
                Assert.AreEqual(2, da.Length);

                Assert.AreEqual(1, da[0]["Id"]);
                Assert.AreEqual("Mary Manager", da[0]["Name"]);
                Assert.AreEqual(2, da[0]["SubordinatesCount"]);

                Assert.AreEqual(2, da[1]["Id"]);
                Assert.AreEqual("Ed Employee", da[1]["Name"]);
                Assert.AreEqual(0, da[1]["SubordinatesCount"]);
            }
        }
        */

        [Test]
        public void SelectArray()
        {
            using (new SoodaTransaction())
            {
                object[][] da = Contact.Linq().OrderBy(c => c.ContactId).Take(2).Select(c =>
                        new object[] { c.ContactId, c.Name, c.Subordinates.Count }
                    ).ToArray();
                Assert.AreEqual(2, da.Length);

                Assert.AreEqual(1, da[0][0]);
                Assert.AreEqual("Mary Manager", da[0][1]);
                Assert.AreEqual(2, da[0][2]);

                Assert.AreEqual(2, da[1][0]);
                Assert.AreEqual("Ed Employee", da[1][1]);
                Assert.AreEqual(0, da[1][2]);
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
        public void SelectIndexedIndex()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select((c, i) => i);
                CollectionAssert.AreEqual(Enumerable.Range(0, 7), ie);
            }
        }

        [Test]
        public void SelectNullable()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<SqlString> se = from v in Vehicle.Linq() orderby v.Name select v.Name;
                CollectionAssert.AreEqual(new SqlString[] { "a bike", "a car", "a super-bike", "an extended bike", "another super-bike", "concrete bike 1", "concrete bike 2", "mega super-bike", "some vehicle" }, se);
            }
        }

        [Test]
        public void SelectTimeSpan()
        {
            using (new SoodaTransaction())
            {
                TimeSpan t = (from e in EightFields.Linq() where e.Id == 1 select e.TimeSpan).Single();
                Assert.AreEqual(TimeSpan.FromHours(1), t);
            }
        }

        [Test]
        public void SelectGuid()
        {
            using (new SoodaTransaction())
            {
                Guid g= (from e in EightFields.Linq() where e.Id == 1 select e.Guid).Single();
                Assert.AreEqual(new Guid("757A29AF-2BB2-4974-829A-A944CF741265"), g);
            }
        }

        [Test]
        public void SelectBlob()
        {
            using (new SoodaTransaction())
            {
                byte[] b = (from e in EightFields.Linq() where e.Id == 1 select e.Blob).Single();
                CollectionAssert.AreEqual(new byte[] { 0xf }, b);
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
        public void Skip()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Skip(3);
                Assert.AreEqual(4, ce.Count());

                ce = Contact.Linq().Skip(0);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Skip(-5);
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Skip(1);
                Assert.AreEqual(1, ce.Count());

                ce = Contact.Linq().Where(c => c.Manager == Contact.Mary).Skip(4);
                Assert.AreEqual(0, ce.Count());

                ce = Contact.Linq().Where(c => c.Type == ContactType.Customer).OrderBy(c => c.Name).Skip(2);
                Assert.AreEqual(2, ce.Count());
                Assert.AreEqual("Chris Customer", ce.First().Name);
                Assert.AreEqual("Chuck Customer", ce.Last().Name);

                ce = Contact.Linq().Skip(1).Skip(2);
                Assert.AreEqual(4, ce.Count());

                ce = Contact.Linq().Skip(2).Skip(-2);
                Assert.AreEqual(5, ce.Count());

                ce = Contact.Linq().Skip(-2).Skip(2);
                Assert.AreEqual(5, ce.Count());

                ce = Contact.Linq().Skip(0x7fffffff).Skip(0x7fffffff).Skip(5);
                Assert.AreEqual(0, ce.Count());
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

                ce = Contact.Linq().Take(3).Take(5);
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().Take(5).Take(3);
                Assert.AreEqual(3, ce.Count());
            }
        }

        [Test]
        public void SkipTake()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Skip(0).Take(3);
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().Skip(3).Take(3);
                Assert.AreEqual(3, ce.Count());

                ce = Contact.Linq().Skip(6).Take(3);
                Assert.AreEqual(1, ce.Count());

                ce = Contact.Linq().Where(c => c.Type == ContactType.Customer).Skip(2).Take(1);
                Assert.AreEqual(1, ce.Count());

                ce = Contact.Linq().Where(c => c.Type == ContactType.Customer).OrderBy(c => c.Name).Skip(2).Take(1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Chris Customer", ce.First().Name);
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
        public void SkipWhere()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Skip(4).Where(c => c.ContactId > 0);
                Assert.AreEqual(3, ce.Count());
            }
        }

        [Test]
        public void SkipScalar()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().OrderBy(c => c.ContactId).Skip(2).Min(c => c.ContactId);
                Assert.AreEqual(3, result);
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
        [ExpectedException(typeof(NotSupportedException))]
        public void SelectSelect()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => c.ContactId).Select(i => 2 * i);
                CollectionAssert.AreEquivalent(new int[] { 2 * 1, 2 * 2, 2 * 3, 2 * 50, 2 * 51, 2 * 52, 2 * 53 }, ie);
            }
        }

        [Test]
        public void Distinct()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => c.ContactId / 10).Distinct();
                CollectionAssert.AreEquivalent(new int[] { 0, 5 }, ie);
            }
        }

        [Test]
        public void DistinctDistinct()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => c.ContactId / 10).Distinct().Distinct();
                CollectionAssert.AreEquivalent(new int[] { 0, 5 }, ie);
            }
        }

        [Test]
        public void DistinctSelect()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Distinct().Select(c => c.ContactId / 10 + 1);
                CollectionAssert.AreEquivalent(new int[] { 1, 1, 1, 6, 6, 6, 6 }, ie);
            }
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void SelectDistinctSelect()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => c.ContactId / 10).Distinct().Select(i => i + 1);
                CollectionAssert.AreEquivalent(new int[] { 1, 6 }, ie);
            }
        }

        [Test]
        public void ManyQueries()
        {
            using (new SoodaTransaction())
            {
                IQueryable<Contact> cq = Contact.Linq();
                Assert.AreEqual(1, cq.Where(c => c.ContactId == 3).Count());
                Assert.AreEqual(2, cq.Where(c => c.ContactId < 3).Count());
                Assert.AreEqual(7, cq.Count());
            }
        }

        [Test]
        public void OfType()
        {
            using (new SoodaTransaction())
            {
                IQueryable<Vehicle> vq = Vehicle.Linq();
                Assert.AreEqual(9, vq.OfType<object>().Count());
                Assert.AreEqual(9, vq.OfType<SoodaObject>().Count());
                Assert.AreEqual(9, vq.OfType<Vehicle>().Count());
                Assert.AreEqual(2, vq.OfType<Car>().Count());
                Assert.AreEqual(7, vq.OfType<Bike>().Count());
                Assert.AreEqual(3, vq.OfType<MegaSuperBike>().Count());
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

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void OnServer()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                IQueryable<Contact> ce = Contact.Linq();
                // just call any function that has no SQL translation
                Assert.IsTrue(ce.All(c => c.Name.GetEnumerator() != null));
            }
        }

        [Test]
        public void OnClient()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq();
                // just call any function that has no SQL translation
                Assert.IsTrue(ce.All(c => c.Name.GetEnumerator() != null));
            }
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void OnServer2()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                var ce = Contact.Linq();
                // just call any function that has no SQL translation
                Assert.IsTrue(ce.All(c => c.Name.GetEnumerator() != null));
            }
        }

        [Test]
        public void OnClient2()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                var ce = Contact.Linq().AsEnumerable();
                // just call any function that has no SQL translation
                Assert.IsTrue(ce.All(c => c.Name.GetEnumerator() != null));
            }
        }

        [Test]
        public void Deferred()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                // must not be executed
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.GetEnumerator() != null);
                Assert.IsTrue(true);
            }
        }

        [Test]
        public void Deferred2()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                string name = "Unused";
                var ce = Contact.Linq().Where(c => c.Name == name);
                name = "Mary Manager";
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);
            }
        }

        bool Deferred3HelperCalled;

        string Deferred3Helper()
        {
            Deferred3HelperCalled = true;
            return "Mary Manager";
        }

        [Test]
        public void Deferred3()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Deferred3HelperCalled = false;
                var ce = Contact.Linq().Where(c => c.Name == Deferred3Helper());
                Assert.IsFalse(Deferred3HelperCalled);
                Assert.AreEqual(1, ce.Count());
                Assert.IsTrue(Deferred3HelperCalled);
            }
        }

        [Test]
        public void BikeOwners()
        {
            using (new SoodaTransaction())
            {
                var bikeOwners = Bike.Linq().Select(b => new {
                        Label = b.Owner != null ? b.Owner.NameAndType : "nobody",
                        Id = (b.Owner != null ? b.Owner.ContactId : (int?) null)
                    }).ToList();

                Assert.AreEqual(7, bikeOwners.Count);
                Assert.AreEqual("Ed Employee (Employee)", bikeOwners[0].Label);
                Assert.AreEqual(2, bikeOwners[0].Id);
                for (int i = 1; i < 7; i++)
                {
                    Assert.AreEqual("nobody", bikeOwners[i].Label);
                    Assert.IsNull(bikeOwners[i].Id);
                }
            }
        }

        [Test]
        public void BikeOwnerTypes()
        {
            using (new SoodaTransaction())
            {
                List<ContactType> tl = (from b in Bike.Linq() where b.Owner != null orderby b.Owner.Type.Code select b.Owner.Type).ToList();
                CollectionAssert.AreEqual(new ContactType[] { ContactType.Employee }, tl);
            }
        }

    }
}

#endif
