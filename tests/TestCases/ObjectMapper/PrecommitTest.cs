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
using Sooda.Caching;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class PrecommitTest
    {
        [Test]
        public void Test1()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();
                SoodaCache.DefaultCache.Clear();;

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    // create new uncommitted object
                    Contact nc = new Contact();

                    Console.WriteLine("Precommitting...");

                    // force precommit
                    // the 'type' field precommits as 'Customer'
                    ContactList cl = Contact.GetList(SoodaWhereClause.Unrestricted);

                    // update name

                    Console.WriteLine("Precommitting again...");
                    nc.Name = "name1";

                    // force precommit again
                    cl = Contact.GetList(SoodaWhereClause.Unrestricted);

                    Console.WriteLine("Precommitting again (2)...");

                    nc.Type = ContactType.Customer;
                    // force precommit again
                    cl = Contact.GetList(SoodaWhereClause.Unrestricted);

                    // this should do nothing

                    Console.WriteLine("Comitting...");
                    tran.Commit();
                }
            }
        }

        [Test]
        public void Test2()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();
                SoodaCache.DefaultCache.Clear();;

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);

                    // create new uncommitted object
                    Contact nc = new Contact();

                    Console.WriteLine("Precommitting...");

                    // force precommit
                    // the 'type' field precommits as 'Customer'
                    ContactList cl = Contact.GetList(new SoodaWhereClause("Type={0} and ContactId={1}", "Customer", nc.ContactId));

                    // we get one record
                    Assert.AreEqual(1, cl.Count, "#1");

                    // force precommit
                    // the 'type' field precommits as 'Customer'
                    ContactList cl2 = Contact.GetList(new SoodaWhereClause("Type={0} and ContactId={1}", "Customer", nc.ContactId), SoodaSnapshotOptions.VerifyAfterLoad);

                    // we use SoodaSnapshotOptions.Verify to check in-memory
                    // values after the load
                    Assert.AreEqual(0, cl2.Count, "#2");

                    nc.Type = ContactType.Customer;

                    // force precommit
                    ContactList cl3 = Contact.GetList(new SoodaWhereClause("Type={0} and ContactId={1}", "Customer", nc.ContactId), SoodaSnapshotOptions.VerifyAfterLoad);

                    // we use SoodaSnapshotOptions.Verify to check in-memory
                    // values after the load
                    Assert.AreEqual(1, cl3.Count, "#3");

                    nc.Type = ContactType.Employee;

                    ContactList cl4 = Contact.GetList(new SoodaWhereClause("Type={0} and ContactId={1}", "Customer", nc.ContactId), SoodaSnapshotOptions.VerifyAfterLoad);

                    // we use SoodaSnapshotOptions.Verify to check in-memory
                    // values after the load
                    Assert.AreEqual(0, cl4.Count, "#4");

                    // tran.Commit();
                }
            }
        }

        [Test]
        public void EventsTest()
        {
            using (TestSqlDataSource ds = new TestSqlDataSource("default"))
            {
                ds.Open();
                int roleID;

                using (SoodaTransaction t = new SoodaTransaction())
                {
                    t.RegisterDataSource(ds);
                    Role r = new Role();
                    r.Name = "aaa";
                    Console.WriteLine("Precommitting...");
                    t.SaveObjectChanges();
                    Console.WriteLine("Comitting.");
                    t.Commit();
                    roleID = r.Id;
                    Assert.AreEqual(0, r.BeforeObjectInsertEventCounter);
                    Assert.AreEqual(2, r.AfterObjectInsertEventCounter);
                    Assert.AreEqual(-1, r.BeforeObjectUpdateEventCounter);
                    Assert.AreEqual(-1, r.AfterObjectUpdateEventCounter);

                    Assert.AreEqual(1, r.Second.BeforeObjectInsertEventCounter);
                    Assert.AreEqual(3, r.Second.AfterObjectInsertEventCounter);
                    Assert.AreEqual(-1, r.Second.BeforeObjectUpdateEventCounter);
                    Assert.AreEqual(-1, r.Second.AfterObjectUpdateEventCounter);
                }
            }
        }
    }
}
