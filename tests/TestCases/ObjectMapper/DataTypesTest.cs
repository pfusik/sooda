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
using System.Data.SqlTypes;

using Sooda.ObjectMapper;
using Sooda.UnitTests.Objects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper {
    [TestFixture]
    public class DataTypesTest {
        [Test]
        public void DecimalTest() {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default")) {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction()) {
                    tran.RegisterDataSource(testDataSource);
                    Assert.AreEqual(123.1234567890m, (decimal)Contact.Mary.LastSalary);
                    Assert.AreEqual(234.0000000000m, (decimal)Contact.Ed.LastSalary);
                    Assert.AreEqual(345.0000000000m, (decimal)Contact.Eva.LastSalary);

                    Console.WriteLine("Type: {0}", Contact.Eva.Type);
                    Contact.Eva.LastSalary = (Decimal)Contact.Mary.LastSalary * 2;
                    tran.Commit();
                }
            }
        }

        [Test]
        public void AllDataTypesMaxTest() {
            string ser;

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default")) {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction()) {
                    tran.RegisterDataSource(testDataSource);
                    AllDataTypes adt = new AllDataTypes();

                    adt.DateVal = DateTime.Now;
                    adt.IntVal = Int32.MaxValue;
                    adt.Int64Val = Int64.MaxValue;
                    adt.DecimalVal = 1000000.12345m;
                    adt.DoubleVal = Double.MaxValue;
                    adt.FloatVal = Single.MaxValue;
                    adt.StringVal = "test 12345";
                    adt.BoolVal = true;

                    adt.NnDateVal = DateTime.Now;
                    adt.NnIntVal = Int32.MaxValue;
                    adt.NnInt64Val = Int64.MaxValue;
                    adt.NnDecimalVal = 1000000.12345m;
                    adt.NnDoubleVal = Double.MaxValue;
                    adt.NnFloatVal = Single.MaxValue;
                    adt.NnStringVal = "test 12345";
                    adt.NnBoolVal = true;

                    ser = tran.Serialize();

                    Console.WriteLine("ser: {0}", ser);

                    tran.Deserialize(ser);
                    Assert.AreEqual(ser, tran.Serialize());
                    Console.WriteLine("Serialization is stable...");
                    tran.Commit();
                }
            }
        }

        [Test]
        public void AllDataTypesMinTest() {
            string ser;

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default")) {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction()) {
                    tran.RegisterDataSource(testDataSource);
                    AllDataTypes adt = new AllDataTypes();

                    // the range for decimal doesn't match the range in SQL

                    adt.DateVal = DateTime.Now;
                    adt.IntVal = Int32.MinValue;
                    adt.Int64Val = Int64.MinValue;
                    adt.DecimalVal = -1000000.12345m;
                    adt.DoubleVal = Double.MinValue;
                    adt.FloatVal = Single.MinValue;
                    adt.StringVal = "test 12345";
                    adt.BoolVal = true;

                    adt.NnDateVal = DateTime.Now;
                    adt.NnIntVal = Int32.MinValue;
                    adt.NnInt64Val = Int64.MinValue;
                    adt.NnDecimalVal = -1000000.12345m;
                    adt.NnDoubleVal = Double.MinValue;
                    adt.NnFloatVal = Single.MinValue;
                    adt.NnStringVal = "test 12345";
                    adt.NnBoolVal = true;

                    ser = tran.Serialize();

                    Console.WriteLine("ser: {0}", ser);

                    tran.Deserialize(ser);
                    Assert.AreEqual(ser, tran.Serialize());
                    Console.WriteLine("Serialization is stable...");
                    tran.Commit();
                }
            }
        }

        [Test]
        public void AllDataTypesNullTest() {
            string ser;

            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default")) {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction()) {
                    tran.RegisterDataSource(testDataSource);
                    AllDataTypes adt = new AllDataTypes();

                    adt.DateVal = SqlDateTime.Null;
                    adt.IntVal = SqlInt32.Null;
                    adt.Int64Val = SqlInt64.Null;
                    adt.DecimalVal = SqlDecimal.Null;
                    adt.DoubleVal = SqlDouble.Null;
                    adt.FloatVal = SqlSingle.Null;
                    adt.StringVal = SqlString.Null;
                    adt.BoolVal = SqlBoolean.Null;

                    adt.NnDateVal = DateTime.Now;
                    adt.NnIntVal = Int32.MinValue;
                    adt.NnInt64Val = Int64.MinValue;
                    adt.NnDecimalVal = 0.0m;
                    adt.NnDoubleVal = Double.MinValue;
                    adt.NnFloatVal = Single.MinValue;
                    adt.NnStringVal = "test 12345";
                    adt.NnBoolVal = true;

                    ser = tran.Serialize();

                    Console.WriteLine("ser: {0}", ser);

                    tran.Deserialize(ser);
                    Assert.AreEqual(ser, tran.Serialize());
                    Console.WriteLine("Serialization is stable...");
                    tran.Commit();
                }
            }
        }

        //[Test]
        public void AllDataTypesNotNullDefaults() {
            using (TestSqlDataSource testDataSource = new TestSqlDataSource("default")) {
                testDataSource.Open();

                using (SoodaTransaction tran = new SoodaTransaction()) {
                    tran.RegisterDataSource(testDataSource);
                    AllDataTypes adt = new AllDataTypes();

                    Assert.AreEqual(adt.NnDateVal, DateTime.MinValue);
                    Assert.AreEqual(adt.NnIntVal, (int)0);
                    Assert.AreEqual(adt.NnInt64Val, (long)0);
                    Assert.AreEqual(adt.NnDecimalVal, (decimal)0);
                    Assert.AreEqual(adt.NnDoubleVal, (double)0);
                    Assert.AreEqual(adt.NnFloatVal, (float)0);
                    Assert.AreEqual(adt.NnStringVal, String.Empty);
                    Assert.AreEqual(adt.NnBoolVal, false);
                }
            }
        }
    }
}
