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

namespace Sooda.UnitTests.TestCases.ObjectMapper {
    [TestFixture]
    public class PrimaryKeyTest {
        [Test]
        public void Int32Test() {
            string ser;

            using (SoodaTransaction tran = new SoodaTransaction()) {
                PKInt32 test = new PKInt32();
                PKInt32 test2 = PKInt32.Load(7777777);

                Assertion.AssertEquals((string)test2.Data, "test data");

                ser = tran.Serialize();
                tran.Deserialize(ser);
                Assertion.AssertEquals(ser, tran.Serialize());
            }
        }

        [Test]
        public void Int64Test() {
            string ser;

            using (SoodaTransaction tran = new SoodaTransaction()) {
                PKInt64 test = new PKInt64();
                PKInt64 test2 = PKInt64.Load(77777777777777);

                Assertion.AssertEquals((string)test2.Data, "test data");

                ser = tran.Serialize();
                tran.Deserialize(ser);
                Assertion.AssertEquals(ser, tran.Serialize());
            }
        }

        [Test]
        public void StringTest() {
            string ser;

            using (SoodaTransaction tran = new SoodaTransaction()) {
                PKString test = new PKString();
                PKString test2 = PKString.Load("zzzzzzz");

                Assertion.AssertEquals((string)test2.Data, "test data");

                ser = tran.Serialize();
                tran.Deserialize(ser);
                Assertion.AssertEquals(ser, tran.Serialize());
            }
        }

        [Test]
        public void DateTimeTest() {
            string ser;

            using (SoodaTransaction tran = new SoodaTransaction()) {
                PKDateTime test = new PKDateTime();
                PKDateTime test2 = PKDateTime.Load(new DateTime(2000, 1, 1, 0, 0, 0, 0));

                Assertion.AssertEquals((string)test2.Data, "test data");

                ser = tran.Serialize();
                tran.Deserialize(ser);
                Assertion.AssertEquals(ser, tran.Serialize());
            }
        }

        [Test]
        public void BooleanTest() {
            string ser;

            using (SoodaTransaction tran = new SoodaTransaction()) {
                PKBool test = new PKBool();
                PKBool test2 = PKBool.Load(true);

                Assertion.AssertEquals((string)test2.Data, "test data");

                ser = tran.Serialize();
                tran.Deserialize(ser);
                Assertion.AssertEquals(ser, tran.Serialize());
            }
        }
    }
}
