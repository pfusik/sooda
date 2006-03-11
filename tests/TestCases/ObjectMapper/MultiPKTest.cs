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

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class MultiPKTest
    {
        [Test]
        public void SimpleTest()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    Console.WriteLine(MultiKey.Load(1, 1).Value);
                    Console.WriteLine(MultiKey.Load(1, 1).Value);
                }
            }
        }
        [Test]
        public void GetListTest()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    foreach (MultiKey mk in MultiKey.GetList(SoodaWhereClause.Unrestricted))
                    {
                        Console.WriteLine("mk: {0},{1} = {2}", mk.Contact, mk.Group, mk.Value);
                    }
                }
            }
        }
        [Test]
        public void MultiTableGetListTest()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    foreach (MultiKey mk in MultiKey.GetList(SoodaWhereClause.Unrestricted))
                    {
                        Console.WriteLine("mk: {0},{1} = {2},{3},{4}", mk.Contact, mk.Group, mk.Value, mk.Value2, mk.Value3);
                    }
                }
            }
        }
        [Test]
        public void MultiTableGetListTestWithWhere()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    foreach (MultiKey mk in MultiKey.GetList(new SoodaWhereClause("Contact = {0}", 1)))
                    {
                        Console.WriteLine("mk: {0},{1} = {2},{3},{4}", mk.Contact, mk.Group, mk.Value, mk.Value2, mk.Value3);
                    }
                }
            }
        }
        [Test]
        public void InsertTest()
        {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default"))
            {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    MultiKey mk = new MultiKey();
                    mk.Contact = 99;
                    mk.Group = 123;
                    mk.Value = 44;
                    mk.Value2 = 55;
                    mk.Value3 = 66;
                    Console.WriteLine(tran.Serialize());
                    tran.Commit();
                }

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    MultiKey mk2 = MultiKey.Load(99, 123);
                    Assert.AreEqual(44, mk2.Value);
                    Assert.AreEqual(55, mk2.Value2);
                    Assert.AreEqual(66, mk2.Value3);
                    mk2.Value2 = 99;
                    tran.Commit();
                }

                using (SoodaTransaction tran = new SoodaTransaction())
                {
                    tran.RegisterDataSource(testDataSource);
                    MultiKey mk2 = MultiKey.Load(99, 123);
                    Assert.AreEqual(44, mk2.Value);
                    Assert.AreEqual(99, mk2.Value2);
                    Assert.AreEqual(66, mk2.Value3);
                    tran.Commit();
                }
            }
        }
    }
}
