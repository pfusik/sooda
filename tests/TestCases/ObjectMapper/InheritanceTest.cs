// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class InheritanceTest
    {
        [Test]
        public void InsertTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Vehicle v = new Car();
                Assert.AreEqual(v.Type, 1);
                Car c = Car.GetRef(v.Id);

                Assert.AreSame(v, c);
                Assert.IsTrue(v is Car);
            }
        }

        [Test]
        public void LoadTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Assert.AreEqual(Vehicle.Load(4).GetType(), typeof(SuperBike));
                Assert.AreEqual(Vehicle.Load(6).GetType(), typeof(MegaSuperBike));
                Assert.AreEqual(Vehicle.Load(3).GetType(), typeof(Bike));
                Assert.AreEqual(Vehicle.Load(1).GetType(), typeof(Car));
                Assert.AreEqual(Vehicle.Load(2).GetType(), typeof(Car));
            }
        }

        [Test]
        public void LoadTest2()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Assert.AreEqual(Bike.Load(3).GetType(), typeof(Bike));
                Assert.AreEqual(Bike.Load(4).GetType(), typeof(SuperBike));
                Assert.AreEqual(Bike.Load(6).GetType(), typeof(MegaSuperBike));
            }
        }

        [Test]
        [ExpectedException(typeof(SoodaObjectNotFoundException))]
        public void LoadTest3()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Bike b = Bike.Load(1);
                Console.WriteLine("t: {0}", b.GetType());
            }
        }

        [Test]
        public void LoadTest4()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                VehicleList list = Vehicle.GetList(tran, SoodaWhereClause.Unrestricted);
                foreach (Vehicle v in list)
                {
                    Console.WriteLine("v: " + v.GetType());
                    // shouldn't throw
                }
                Assert.AreEqual(9, list.Count);
            }
        }

        [Test]
        public void LoadTest5()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                BikeList list = Bike.GetList(tran, SoodaWhereClause.Unrestricted);
                foreach (Vehicle v in list)
                {
                    if (v != null)
                    {
                    }
                    // shouldn't throw
                }
                foreach (Bike v in list)
                {
                    if (v != null)
                    {
                    }
                    // shouldn't throw
                }

                //
                // there are 7 bikes out of 9 vehicles and Bike.GetList 
                // should filter them properly
                //
                Assert.AreEqual(7, list.Count);
            }
        }

        [Test]
        public void LoadTest6()
        {
            Console.WriteLine("Test6a");
            try
            {
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    ExtendedBike ebk = (ExtendedBike)Vehicle.Load(10);

                    Console.WriteLine(ebk.ExtendedBikeInfo);
                }
            }
            finally
            {
                Console.WriteLine("Test6b");
            }
        }

        [Test]
        public void LongTest()
        {
            // run multiple Sooda transactions in a single SQL transaction
            // this is achieved by a hacked SqlDataSource which ignores
            // Close(), Commit() requests

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                int ebKey;
                int bikeKey;
                int gvKey;

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    Vehicle v = new Bike();
                    Assert.AreEqual(v.Type, 2);
                    ((Bike)v).TwoWheels = 1;

                    ExtendedBike eb = new ExtendedBike();
                    Assert.AreEqual(eb.Type, 7);
                    eb.TwoWheels = 1;
                    eb.ExtendedBikeInfo = "some info here";
                    ebKey = eb.Id;
                    bikeKey = v.Id;

                    Contact.Mary.Bikes.Add(v);
                    Contact.Mary.Bikes.Add(eb);

                    Contact.Mary.Vehicles.Add(v);
                    Contact.Mary.Vehicles.Add(eb);

                    v = new Bike();
                    gvKey = v.Id;
                    Contact.Mary.Vehicles.Add(v);

                    tran.Commit();
                }

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    Vehicle v1 = Vehicle.GetRef(ebKey);
                    Assert.IsTrue(v1 is ExtendedBike);

                    Vehicle v2 = Vehicle.GetRef(bikeKey);
                    Assert.IsTrue(v2.GetType() == typeof(Bike));

                    Assert.IsTrue(Contact.Mary.Vehicles.Contains(Vehicle.GetRef(gvKey)));
                    Assert.IsTrue(Contact.Mary.Vehicles.Contains(v1));
                    Assert.IsTrue(Contact.Mary.Vehicles.Contains(v2));

                    Contact.Mary.Vehicles.Remove(v1);
                    Contact.Mary.Vehicles.Remove(v2);

                    Assert.IsTrue(!Contact.Mary.Vehicles.Contains(v1));
                    Assert.IsTrue(!Contact.Mary.Vehicles.Contains(v2));

                    Assert.IsTrue(Contact.Mary.Bikes.Contains(v1));
                    Assert.IsTrue(Contact.Mary.Bikes.Contains(v2));

                    Contact.Mary.Bikes.Remove(v1);

                    try
                    {
                        Vehicle v3 = ExtendedBike.GetRef(bikeKey);
                        Assert.Fail("Vehicle v3 = ExtendedBike.GetRef(bikeKey); should fail here.");
                    }
                    catch (SoodaObjectNotFoundException)
                    {
                        Console.WriteLine("Got exception as expected");
                    }

                    tran.Commit();
                }
                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    Vehicle v1 = Vehicle.GetRef(ebKey);
                    Assert.IsTrue(v1 is ExtendedBike);

                    Vehicle v2 = Vehicle.GetRef(bikeKey);
                    Assert.IsTrue(v2.GetType() == typeof(Bike));

                    Assert.IsTrue(!Contact.Mary.Vehicles.Contains(v1));
                    Assert.IsTrue(!Contact.Mary.Vehicles.Contains(v2));

                    Assert.IsTrue(!Contact.Mary.Bikes.Contains(v1));
                    Assert.IsTrue(Contact.Mary.Bikes.Contains(v2));

                    tran.Commit();
                }
            }
        }
    }
}
