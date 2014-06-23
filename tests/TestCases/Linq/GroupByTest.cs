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
                var oe = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select new KeyValuePair<string, int>(g.Key, g.Count());
                CollectionAssert.AreEqual(new KeyValuePair<string, int>[] {
                    new KeyValuePair<string, int>("Customer", 4),
                    new KeyValuePair<string, int>("Employee", 2),
                    new KeyValuePair<string, int>("Manager", 1) }, oe);
            }
        }

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
        public void Max()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Type.Code into g orderby g.Key select g.Max(c => c.ContactId);
                CollectionAssert.AreEqual(new int[] { 53, 3, 1 }, ie);
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
        public void Bool()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() group c by c.Name.StartsWith("C") into g orderby g.Key select g.Count();
                CollectionAssert.AreEqual(new int[] { 3, 4 }, ie);
            }
        }
    }
}

#endif
