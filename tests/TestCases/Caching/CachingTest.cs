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
using Sooda.Schema;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Caching
{
    [TestFixture]
    public class CachingTest
    {
        private static SoodaCacheEntry DummyEntry(int a)
        {
            return new SoodaCacheEntry(a, new SoodaObjectArrayFieldValues(3));
        }

        [Test]
        public void Test1()
        {
            ISoodaCache c = new SoodaInProcessCache();
            Assert.IsNull(c.Find("Contact", 2));
        }

        [Test]
        public void Test2()
        {
            ISoodaCache c = new SoodaInProcessCache();
            c.Add("Contact", 2, DummyEntry(2));
            c.Invalidate("Contact", 2, SoodaCacheInvalidateReason.Updated);
            Assert.IsNull(c.Find("Contact", 2));
        }

        [Test]
        public void Test3()
        {
            ISoodaCache c = new SoodaInProcessCache();
            c.Add("Contact", 2, DummyEntry(2));
            //c.Invalidate("Contact", 2, SoodaCacheInvalidateReason.Updated);
            Assert.IsNotNull(c.Find("Contact", 2));
            Assert.AreEqual(2, c.Find("Contact", 2).DataLoadedMask);
        }

        [Test]
        public void Test4()
        {
            ISoodaCache c = new SoodaInProcessCache();
            c.Add("Contact", 2, DummyEntry(2));
            c.Add("Contact", 2, DummyEntry(3));
            Assert.AreEqual(3, c.Find("Contact", 2).DataLoadedMask);
        }

        [Test]
        public void Test5()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                SoodaInProcessCache myCache = new SoodaInProcessCache();
                myCache.ExpirationTimeout = TimeSpan.FromHours(1);
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    ContactList cl = Role.Employee.Members.GetSnapshot();
                }
                Assert.IsNotNull(myCache.LoadCollection("ContactToRole where Role = 1"));
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    Assert.AreEqual(0, tran.Statistics.CollectionCacheHits);
                    Assert.AreEqual(0, tran.Statistics.CollectionCacheMisses);

                    // make sure w have a cache hit
                    ContactList cl = Role.Employee.Members.GetSnapshot();
                    Assert.AreEqual(1, tran.Statistics.CollectionCacheHits);
                    Assert.AreEqual(0, tran.Statistics.CollectionCacheMisses);
                }
            }
        }

        [Test]
        public void Test6()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                SoodaInProcessCache myCache = new SoodaInProcessCache();
                myCache.ExpirationTimeout = TimeSpan.FromHours(1);
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    ContactList cl = Role.Employee.Members.GetSnapshot();
                }
                // make sure it's in cache
                Assert.IsNotNull(myCache.LoadCollection("ContactToRole where Role = 1"));
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    // add role
                    Contact.Eva.Roles.Add(Role.Customer);

                    // force precommit
                    tran.SaveObjectChanges();

                    Assert.AreEqual(0, tran.Statistics.CollectionCacheHits);
                    Assert.AreEqual(0, tran.Statistics.CollectionCacheMisses);

                    // make sure w have a cache miss
                    ContactList cl = Role.Employee.Members.GetSnapshot();
                    Assert.AreEqual(0, tran.Statistics.CollectionCacheHits);
                    Assert.AreEqual(1, tran.Statistics.CollectionCacheMisses);
                    tran.Commit();
                }
                // make sure it's NOT in cache
                Assert.IsNull(myCache.LoadCollection("ContactToRole where Role = 1"));
            }
        }
    }
}
