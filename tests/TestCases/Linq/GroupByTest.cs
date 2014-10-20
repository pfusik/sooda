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

#if DOTNET35

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.Objects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class GroupByTest
    {
        [Test]
        public void Key()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Key;
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee", "Manager" }, se);
            }
        }

        [Test]
        public void KeyReference()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() group c by c.Type into g orderby g.Key.Description select g.Key.Code;
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee", "Manager" }, se);
            }
        }

        [Test]
        public void Multi()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se =
                    from c in Contact.Linq()
                    group c by new { c.Type.Code, c.Type.Description } into g
                    orderby g.Key.Code, g.Key.Description
                    select g.Key.Code;
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee", "Manager" }, se);
            }
        }

        [Test]
        public void MultiSelect()
        {
            using (new SoodaTransaction())
            {
                var ol =
                    (from c in Contact.Linq()
                    group c by new { c.Type.Code, c.Type.Description } into g
                    orderby g.Key.Code
                    select g.Key).ToList();
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee", "Manager" }, ol.Select(o => o.Code));
                CollectionAssert.AreEqual(new string[] { "External Contact", "Internal Employee", "Internal Manager" }, ol.Select(o => o.Description.Value));
            }
        }

        [Test]
        public void Count()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Count();
                CollectionAssert.AreEqual(new int[] { 4, 2, 1 }, ie);
            }
        }

        [Test]
        public void CountFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Count(c => c.LastSalary.Value >= 100);
                CollectionAssert.AreEqual(new int[] { 1, 2, 1 }, ie);
            }
        }

        [Test]
        public void KeyCount()
        {
            using (new SoodaTransaction())
            {
                var ol = (from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select new { Type = g.Key, Count = g.Count() }).ToList();
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee", "Manager" }, ol.Select(o => o.Type));
                CollectionAssert.AreEqual(new int[] { 4, 2, 1}, ol.Select(o => o.Count));
            }
        }

        [Test]
        public void KeyCountKeyValuePair()
        {
            using (new SoodaTransaction())
            {
                var pe = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select new KeyValuePair<string, int>(g.Key, g.Count());
                CollectionAssert.AreEqual(new KeyValuePair<string, int>[] {
                    new KeyValuePair<string, int>("Customer", 4),
                    new KeyValuePair<string, int>("Employee", 2),
                    new KeyValuePair<string, int>("Manager", 1) }, pe);
            }
        }

#if DOTNET4 // Tuple
        [Test]
        public void MultiCount()
        {
            using (new SoodaTransaction())
            {
                var tq =
                    from c in Contact.Linq()
                    group c by new { c.Type.Code, c.Type.Description } into g
                    orderby g.Key.Code, g.Key.Description
                    select new Tuple<string, string, int>(g.Key.Code, g.Key.Description.Value, g.Count());
                CollectionAssert.AreEqual(new Tuple<string, string, int>[] {
                    new Tuple<string, string, int>("Customer", "External Contact", 4),
                    new Tuple<string, string, int>("Employee", "Internal Employee", 2),
                    new Tuple<string, string, int>("Manager", "Internal Manager", 1) }, tq);
            }
        }
#endif

        [Test]
        public void Min()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Min(c => c.ContactId);
                CollectionAssert.AreEqual(new int[] { 50, 2, 1 }, ie);
            }
        }

        [Test]
        public void MinDateTime()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<DateTime> de = from d in PKDateTime.Linq() group d by d.Id.Year into g select g.Min(d => d.Id);
                CollectionAssert.AreEqual(new DateTime[] { new DateTime(2000, 1, 1) }, de);
            }
        }

        [Test]
        public void Max()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Max(c => c.ContactId);
                CollectionAssert.AreEqual(new int[] { 53, 3, 1 }, ie);
            }
        }

        [Test]
        public void MaxDateTime()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<DateTime> de = from d in PKDateTime.Linq() group d by d.Id.Year into g select g.Max(d => d.Id);
                CollectionAssert.AreEqual(new DateTime[] { new DateTime(2000, 1, 1, 2, 0, 0) }, de);
            }
        }

        [Test]
        public void Sum()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Sum(c => c.ContactId);
                CollectionAssert.AreEqual(new int[] { 50 + 51 + 52 + 53, 2 + 3, 1 }, ie);
            }
        }

        [Test]
        public void Average()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<double> de = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Average(c => c.ContactId);
                CollectionAssert.AreEqual(new double[] { (50 + 51 + 52 + 53) / 4.0, (2 + 3) / 2.0, 1 }, de);
            }
        }

        [Test]
        public void Having()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() group c by c.Type.Code into g where g.Count() > 1 orderby g.Key select g.Key;
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee" }, se);
            }
        }

        [Test]
        public void All()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key where g.All(c => c.LastSalary.Value >= 100) select g.Key;
                CollectionAssert.AreEqual(new string[] { "Employee", "Manager" }, se);
            }
        }

        [Test]
        public void Any()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key where g.Any() select g.Key;
                CollectionAssert.AreEqual(new string[] { "Customer", "Employee", "Manager" }, se);
            }
        }

        [Test]
        public void AnyFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key where g.Any(c => c.LastSalary.Value < 100) select g.Key;
                CollectionAssert.AreEqual(new string[] { "Customer" }, se);
            }
        }

        [Test]
        public void Bool()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Name.StartsWith("C") into g orderby g.Key select g.Count();
                CollectionAssert.AreEqual(new int[] { 3, 4 }, ie);
            }
        }

        [Test]
        public void Take()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().GroupBy(c => c.Type.Code).OrderBy(g => g.Key).Take(2).Select(g => g.Count());
                CollectionAssert.AreEqual(new int[] { 4, 2 }, ie);
            }
        }

        [Test]
        public void Skip()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().GroupBy(c => c.Type.Code).OrderBy(g => g.Key).Skip(1).Select(g => g.Count());
                CollectionAssert.AreEqual(new int[] { 2, 1 }, ie);
            }
        }

        [Test]
        public void SkipTake()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().GroupBy(c => c.Type.Code).OrderBy(g => g.Key).Skip(1).Take(1).Select(g => g.Count());
                CollectionAssert.AreEqual(new int[] { 2 }, ie);
            }
        }

        [Test]
        public void TakeSkip()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().GroupBy(c => c.Type.Code).OrderBy(g => g.Key).Take(2).Skip(1).Select(g => g.Count());
                CollectionAssert.AreEqual(new int[] { 2 }, ie);
            }
        }

        [Test]
        public void First()
        {
            using (new SoodaTransaction())
            {
                int i = Contact.Linq().GroupBy(c => c.Type.Code).OrderBy(g => g.Key).Select(g => g.Count()).First();
                Assert.AreEqual(4, i);
            }
        }

        [Test]
        public void Distinct()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = (from c in Contact.Linq() group c by c.ContactId % 10 into g orderby g.Count() descending select g.Count()).Distinct();
                CollectionAssert.AreEqual(new int[] { 2, 1 }, ie);
            }
        }

        [Test]
        public void NoSelect()
        {
            using (new SoodaTransaction())
            {
                var q = Contact.Linq().GroupBy(c => c.Type.Code);
                Assert.AreEqual(3, q.Count());
            }
        }
    }
}

#endif
