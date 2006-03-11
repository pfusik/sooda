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
using Sooda.UnitTests.BaseObjects.TypedQueries;
using Sooda.Caching;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Caching
{
    [TestFixture]
    public class CollectionTest
    {
        [Test]
        public void SimpleWhere()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();
                SoodaCache.Clear();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    int ch0 = tran.Statistics.CollectionCacheHits;
                    int cm0 = tran.Statistics.CollectionCacheMisses;

                    ContactList cl = Contact.GetList(ContactField.Active == true, SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(7, cl.Count);

                    // we've had a miss
                    int ch1 = tran.Statistics.CollectionCacheHits;
                    int cm1 = tran.Statistics.CollectionCacheMisses;
                    Assert.AreEqual(ch0, ch1);
                    // Assert.AreNotEqual(cm0, cm1);

                    ContactList cl2 = Contact.GetList(ContactField.Active == true, SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(7, cl2.Count);

                    // we've had a hit
                    int ch2 = tran.Statistics.CollectionCacheHits;
                    int cm2 = tran.Statistics.CollectionCacheMisses;
                    // Assert.AreNotEqual(ch1, ch2);
                    Assert.AreEqual(cm1, cm2);
                }
            }
        }

        [Test]
        public void UnrestrictedWhere()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();
                SoodaCache.Clear();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    int ch0 = tran.Statistics.CollectionCacheHits;
                    int cm0 = tran.Statistics.CollectionCacheMisses;

                    ContactList cl = Contact.GetList(SoodaWhereClause.Unrestricted, SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(7, cl.Count);

                    // we've had a miss
                    int ch1 = tran.Statistics.CollectionCacheHits;
                    int cm1 = tran.Statistics.CollectionCacheMisses;
                    Assert.AreEqual(ch0, ch1);
                    // Assert.AreNotEqual(cm0, cm1);

                    ContactList cl2 = Contact.GetList(SoodaWhereClause.Unrestricted, SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(7, cl2.Count);

                    // we've had a hit
                    int ch2 = tran.Statistics.CollectionCacheHits;
                    int cm2 = tran.Statistics.CollectionCacheMisses;
                    // Assert.AreNotEqual(ch1, ch2);
                    Assert.AreEqual(cm1, cm2);

                    ContactList cl3 = Contact.GetList(SoodaWhereClause.Unrestricted, 3, SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(3, cl3.Count);

                    // Dump("after top no sort", cl3);

                    // we've had a hit
                    int ch3 = tran.Statistics.CollectionCacheHits;
                    int cm3 = tran.Statistics.CollectionCacheMisses;
                    // Assert.AreNotEqual(ch2, ch3);
                    Assert.AreEqual(cm1, cm3);

                    ContactList cl4 = Contact.GetList(SoodaWhereClause.Unrestricted, SoodaOrderBy.Descending("Name"), SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(7, cl4.Count);

                    Assert.AreSame(cl4[0], Contact.Mary);
                    // Dump("after sort, no top", cl4);

                    // we've had a hit
                    int ch4 = tran.Statistics.CollectionCacheHits;
                    int cm4 = tran.Statistics.CollectionCacheMisses;
                    // Assert.AreNotEqual(ch3, ch4);
                    Assert.AreEqual(cm1, cm4);

                    ContactList cl5 = Contact.GetList(SoodaWhereClause.Unrestricted, SoodaOrderBy.Descending("LastSalary"), 4, SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(4, cl5.Count);

                    Assert.AreSame(cl5[0], Contact.Eva);
                    // Dump("after sort by salary, with top", cl5);

                    // we've had a hit
                    int ch5 = tran.Statistics.CollectionCacheHits;
                    int cm5 = tran.Statistics.CollectionCacheMisses;
                    // Assert.AreNotEqual(ch4, ch5);
                    Assert.AreEqual(cm1, cm5);
                }
            }
        }


        [Test]
        public void InvalidateTest()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();
                SoodaCache.Clear();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    ContactList cl = Contact.GetList(ContactField.Name == "Mary Manager", SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(1, cl.Count);

                    cl[0].Name = "modified";
                    tran.Commit();
                }

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    ContactList cl = Contact.GetList(ContactField.Name == "Mary Manager", SoodaSnapshotOptions.Cache);
                    Assert.AreEqual(0, cl.Count);
                }
            }
        }

        private static void Dump(string desc, ContactList cl)
        {
            Console.WriteLine("{0} count={1}", desc, cl.Count);
            int p = 0;
            foreach (Contact c in cl)
            {
                Console.WriteLine("{0}. {1} {2}", ++p, c.Name, c.LastSalary);
            }
        }
    }
}
