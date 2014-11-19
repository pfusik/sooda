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

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;
using System;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class NtoNCollectionTest
    {
        [Test]
        public void CollectionNtoNTest()
        {
            string serialized;

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    Assert.IsTrue(!Contact.Mary.Roles.Contains(Role.Customer));

                    foreach (SoodaObject r in Contact.Mary.Roles)
                    {
                        Console.WriteLine("00 serialization mary has role: {0}", r.GetObjectKeyString());
                    }
                    Contact.Mary.Roles.Add(Role.Customer);
                    Assert.IsTrue(Contact.Mary.Roles.Contains(Role.Customer));
                    Contact.Mary.Roles.Remove(Role.Customer);
                    Assert.IsTrue(!Contact.Mary.Roles.Contains(Role.Customer));
                    Contact.Mary.Roles.Add(Role.Customer);
                    Assert.IsTrue(Contact.Mary.Roles.Contains(Role.Customer));

                    foreach (SoodaObject r in Contact.Mary.Roles)
                    {
                        Console.WriteLine("Before serialization mary has role: {0}", r.GetObjectKeyString());
                    }
                    serialized = tran.Serialize(SoodaSerializeOptions.IncludeNonDirtyFields | SoodaSerializeOptions.IncludeNonDirtyObjects);
                }
                Console.WriteLine("serialized: {0}", serialized);

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Deserialize(serialized);
                    string serialized2 = tran.Serialize(SoodaSerializeOptions.IncludeNonDirtyFields | SoodaSerializeOptions.IncludeNonDirtyObjects);
                    if (serialized == serialized2)
                    {
                        Console.WriteLine("Serialization is stable");
                    }
                    else
                    {
                        Console.WriteLine("Serialized again as\n{0}", serialized2);
                    }
                    Assert.AreEqual(serialized, serialized2, "Serialization preserves state");

                    foreach (SoodaObject r in Contact.Mary.Roles)
                    {
                        Console.WriteLine("After deserialization mary has role: {0}", r.GetObjectKeyString());
                    }
                    Assert.IsTrue(Contact.Mary.Roles.Contains(Role.Customer), "Deserialization preserves N-N relation membersips");
                    tran.Commit();
                }
            }
        }

        [Test]
        public void CollectionNtoNSharedTest()
        {
            string serialized;

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    Contact.Mary.Roles.Add(Role.Customer);
                    Assert.IsTrue(Role.Customer.Members.Contains(Contact.Mary));
                    Contact.Mary.Roles.Remove(Role.Customer);
                    Assert.IsTrue(!Role.Customer.Members.Contains(Contact.Mary));

                    serialized = tran.Serialize(SoodaSerializeOptions.IncludeNonDirtyFields | SoodaSerializeOptions.IncludeNonDirtyObjects);
                }
                Console.WriteLine("serialized: {0}", serialized);

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    tran.Deserialize(serialized);
                    string serialized2 = tran.Serialize(SoodaSerializeOptions.IncludeNonDirtyFields | SoodaSerializeOptions.IncludeNonDirtyObjects);
                    if (serialized == serialized2)
                    {
                        Console.WriteLine("Serialization is stable");
                    }
                    else
                    {
                        Console.WriteLine("Serialized again as\n{0}", serialized2);
                    }
                    Assert.AreEqual(serialized, serialized2, "Serialization preserves state");

                    tran.Commit();
                }
            }
        }
    }
}
