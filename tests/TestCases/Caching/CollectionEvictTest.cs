//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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
    public class CollectionEvictTest
    {
        private static SoodaCacheEntry DummyEntry(int a)
        {
            return new SoodaCacheEntry(a, new SoodaObjectArrayFieldValues(3));
        }

        [Test]
        public void Test5()
        {
            ISoodaCache c = new SoodaInProcessCache();
            string ck = "Contact where 1=2";
            c.Add("Contact", 1, DummyEntry(1), TimeSpan.FromHours(1), false);
            c.Add("Contact", 2, DummyEntry(2), TimeSpan.FromHours(1), false);
            c.Add("Contact", 3, DummyEntry(3), TimeSpan.FromHours(1), false);
            c.StoreCollection(ck, "Contact", new int[] { 1, 2, 3 }, new string[0], true, TimeSpan.FromHours(1), false);
            Assert.IsNotNull(c.LoadCollection(ck));
            Assert.AreEqual(3, c.LoadCollection(ck).Count);
            c.Evict("Contact", 3);
            Assert.IsNull(c.LoadCollection(ck));
        }

        [Test]
        public void Test6()
        {
            ISoodaCache c = new SoodaInProcessCache();
            c.Add("Contact", 1, DummyEntry(1), TimeSpan.FromHours(1), false);
            c.Add("Contact", 2, DummyEntry(2), TimeSpan.FromHours(1), false);
            c.Add("Contact", 3, DummyEntry(3), TimeSpan.FromHours(1), false);
            string ck = "Contact where 1=2";
            c.StoreCollection(ck, "Contact", new int[] { 1, 2, 3 }, new string[] { "Contact", "Group", "Role" }, true, TimeSpan.FromHours(1), false);
            Assert.IsNotNull(c.LoadCollection(ck));
            Assert.AreEqual(3, c.LoadCollection(ck).Count);
            c.Invalidate("Group", 3, SoodaCacheInvalidateReason.Updated);
            Assert.IsNull(c.LoadCollection(ck));
        }

        [Test]
        public void Test7()
        {
            SoodaInProcessCache c = new SoodaInProcessCache();
            c.Add("Contact", 3, DummyEntry(3), TimeSpan.FromMilliseconds(50), false);
            Assert.IsNotNull(c.Find("Contact", 3));
            System.Threading.Thread.Sleep(100);
            c.Sweep();
            Assert.IsNull(c.Find("Contact", 3));
        }

        [Test]
        public void Test8()
        {
            SoodaInProcessCache c = new SoodaInProcessCache();
            c.Add("Contact", 1, DummyEntry(1), TimeSpan.FromMilliseconds(150), false);
            c.Add("Contact", 2, DummyEntry(2), TimeSpan.FromMilliseconds(150), false);
            c.Add("Contact", 3, DummyEntry(3), TimeSpan.FromMilliseconds(150), false);
            string ck = "Contact where 1=2";
            c.StoreCollection(ck, "Contact", new int[] { 1, 2, 3 }, new string[] { "Contact", "Group", "Role" }, true, TimeSpan.FromMilliseconds(150), false);
            Assert.IsNotNull(c.Find("Contact", 3));
            System.Threading.Thread.Sleep(200);
            Console.WriteLine("*** Sweeping ***");
            c.Sweep();
            Console.WriteLine("*** Finished ***");
            Assert.IsNull(c.Find("Contact", 3));
        }

        [Test]
        public void Test9()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                SoodaInProcessCache myCache = new SoodaInProcessCache();
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    string ck1 = SoodaCache.GetCollectionKey("Vehicle", new SoodaWhereClause(true));
                    VehicleList c1 = Vehicle.GetList(true);

                    string ck2 = SoodaCache.GetCollectionKey("Bike", new SoodaWhereClause(true));
                    BikeList c2 = Bike.GetList(true);

                    string ck3 = SoodaCache.GetCollectionKey("Bike", new SoodaWhereClause(false));
                    BikeList c3 = Bike.GetList(false);

                    Assert.IsNotNull(myCache.LoadCollection(ck1));
                    Assert.IsNotNull(myCache.LoadCollection(ck2));
                    Assert.IsNotNull(myCache.LoadCollection(ck3));

                    myCache.Evict("Vehicle", 3);

                    // ck3 is not evicted because it contains zero objects

                    Assert.IsNull(myCache.LoadCollection(ck1));
                    Assert.IsNull(myCache.LoadCollection(ck2));
                    Assert.IsNotNull(myCache.LoadCollection(ck3));
                }
            }
        }

        [Test]
        public void Test10()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                SoodaInProcessCache myCache = new SoodaInProcessCache();
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    string ck1 = SoodaCache.GetCollectionKey("Vehicle", new SoodaWhereClause(true));
                    VehicleList c1 = Vehicle.GetList(true);

                    string ck2 = SoodaCache.GetCollectionKey("Bike", new SoodaWhereClause(true));
                    BikeList c2 = Bike.GetList(true);

                    string ck3 = SoodaCache.GetCollectionKey("Bike", new SoodaWhereClause(false));
                    BikeList c3 = Bike.GetList(false);

                    Assert.IsNotNull(myCache.LoadCollection(ck1));
                    Assert.IsNotNull(myCache.LoadCollection(ck2));
                    Assert.IsNotNull(myCache.LoadCollection(ck3));

                    myCache.Evict("Vehicle", 1);

                    // ck2 is not evicted because vehicle[1] is not a bike
                    // ck3 is not evicted because it contains zero objects

                    Assert.IsNull(myCache.LoadCollection(ck1));
                    Assert.IsNotNull(myCache.LoadCollection(ck2));
                    Assert.IsNotNull(myCache.LoadCollection(ck3));
                }
            }
        }

        [Test]
        public void Test11()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                SoodaInProcessCache myCache = new SoodaInProcessCache();
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Cache = myCache;
                    tran.CachingPolicy = new SoodaCacheAllPolicy();

                    string ck1 = SoodaCache.GetCollectionKey("Vehicle", new SoodaWhereClause(true));
                    VehicleList c1 = Vehicle.GetList(true, SoodaSnapshotOptions.KeysOnly);

                    string ck2 = SoodaCache.GetCollectionKey("Bike", new SoodaWhereClause(true));
                    BikeList c2 = Bike.GetList(true, SoodaSnapshotOptions.KeysOnly);

                    string ck3 = SoodaCache.GetCollectionKey("Bike", new SoodaWhereClause(false));
                    BikeList c3 = Bike.GetList(false, SoodaSnapshotOptions.KeysOnly);

                    Assert.IsNotNull(myCache.LoadCollection(ck1));
                    Assert.IsNotNull(myCache.LoadCollection(ck2));
                    Assert.IsNotNull(myCache.LoadCollection(ck3));

                    myCache.Evict("Vehicle", 1);

                    // nothing is evicted from collection cache because we requested KeysOnly

                    Assert.IsNotNull(myCache.LoadCollection(ck1));
                    Assert.IsNotNull(myCache.LoadCollection(ck2));
                    Assert.IsNotNull(myCache.LoadCollection(ck3));
                }
            }
        }
    }
}
