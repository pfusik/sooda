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
using Sooda.UnitTests.Objects;
using System;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class RefTest
    {
        [Test]
        public void Int32Test()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                PKInt32 test2 = PKInt32.Load(7777778);

                Assert.AreEqual(test2.Parent.Id, 7777777);
                Assert.AreEqual((string)test2.Parent.Data, "test data");
                Assert.AreEqual((string)test2.Data, "test data 2");
            }
        }

        [Test]
        public void Int64Test()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                PKInt64 test2 = PKInt64.Load(77777777777778);

                Assert.AreEqual(test2.Parent.Id, 77777777777777);
                Assert.AreEqual((string)test2.Parent.Data, "test data");
                Assert.AreEqual((string)test2.Data, "test data 2");
            }
        }

        [Test]
        public void StringTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                PKString test2 = PKString.Load("xxxxxxx");

                Assert.AreEqual(test2.Parent.Id, "zzzzzzz");
                Assert.AreEqual((string)test2.Parent.Data, "test data");
                Assert.AreEqual((string)test2.Data, "test data 2");
            }
        }

        [Test]
        public void DateTimeTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                PKDateTime test2 = PKDateTime.Load(new DateTime(2000, 1, 1, 1, 0, 0));

                Assert.AreEqual(test2.Parent.Id, new DateTime(2000, 1, 1, 0, 0, 0));
                Assert.AreEqual((string)test2.Parent.Data, "test data");
                Assert.AreEqual((string)test2.Data, "test data 2");
            }
        }

        [Test]
        public void BoolTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                PKBool test2 = PKBool.Load(false);

                Assert.AreEqual(test2.Parent.Id, true);
                Assert.AreEqual((string)test2.Parent.Data, "test data");
                Assert.AreEqual((string)test2.Data, "test data 2");
            }
        }
    }
}
