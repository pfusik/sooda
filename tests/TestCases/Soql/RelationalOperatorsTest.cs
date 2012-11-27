// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using Sooda.ObjectMapper;
using Sooda.QL;
using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Soql
{
    [TestFixture]
    public class RelationalOperatorsTest
    {
        void AssumeException(object o1, SoqlRelationalOperator op, object o2)
        {
            try
            {
                // Console.WriteLine("Checking that {0} ({1}) {2} {3} ({4}) will throw... ", o1, o1.GetType(), op, o2, o2.GetType());
                SoqlBooleanRelationalExpression.Compare(o1, o2, op);
                // Assert.IsTrue("Failed. Exception was expected!", false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        void Assume(object o1, SoqlRelationalOperator op, object o2, bool value)
        {
            // Console.WriteLine("Checking that {0} ({1}) {2} {3} ({4}) = {5}...", o1, o1.GetType(), op, o2, o2.GetType(), value);
            Assert.AreEqual(value, (bool)SoqlBooleanRelationalExpression.Compare(o1, o2, op));
        }

        void AssumeSymmetric(object o1, SoqlRelationalOperator op, object o2, bool value)
        {
            Assume(o1, op, o2, value);
            Assume(o2, op, o1, value);
        }

        void AssumeAntiSymmetric(object o1, SoqlRelationalOperator op, object o2, bool value)
        {
            Assume(o1, op, o2, value);
            Assume(o2, op, o1, !value);
        }

        private void CheckNumberPair(object o1, object o2)
        {
            AssumeSymmetric(o1, SoqlRelationalOperator.Equal, o1, true);
            AssumeSymmetric(o1, SoqlRelationalOperator.NotEqual, o2, true);

            Assume(o1, SoqlRelationalOperator.LessOrEqual, o1, true);
            Assume(o1, SoqlRelationalOperator.GreaterOrEqual, o1, true);

            AssumeAntiSymmetric(o1, SoqlRelationalOperator.Less, o2, true);
            AssumeAntiSymmetric(o2, SoqlRelationalOperator.Greater, o1, true);

            AssumeException(o1, SoqlRelationalOperator.Like, o1);
        }

        [Test]
        public void TestInt8()
        {
            CheckNumberPair((sbyte)1, (sbyte)2);
        }

        [Test]
        public void TestInt16()
        {
            CheckNumberPair((short)1, (short)2);
        }

        [Test]
        public void TestInt32()
        {
            CheckNumberPair((int)1, (int)2);
        }

        [Test]
        public void TestInt64()
        {
            CheckNumberPair((long)1, (long)2);
        }

        [Test]
        public void TestDecimal()
        {
            CheckNumberPair((decimal)1.3, (decimal)1.4);
        }

        [Test]
        public void TestFloat()
        {
            CheckNumberPair((float)1.3, (float)1.4);
        }

        [Test]
        public void TestDouble()
        {
            CheckNumberPair((double)1.3, (double)1.4);
        }

        private void CheckNumberPair(object o1_1, object o1_2, object o2_1, object o2_2)
        {
            AssumeSymmetric(o1_1, SoqlRelationalOperator.Equal, o2_1, true);
            AssumeSymmetric(o1_1, SoqlRelationalOperator.NotEqual, o2_2, true);

            Assume(o1_1, SoqlRelationalOperator.LessOrEqual, o2_1, true);
            Assume(o1_1, SoqlRelationalOperator.GreaterOrEqual, o2_1, true);

            AssumeAntiSymmetric(o1_1, SoqlRelationalOperator.Less, o2_2, true);
            AssumeAntiSymmetric(o1_2, SoqlRelationalOperator.Greater, o2_1, true);

            AssumeException(o1_1, SoqlRelationalOperator.Like, "1");

            AssumeSymmetric(o2_1, SoqlRelationalOperator.Equal, o1_1, true);
            AssumeSymmetric(o2_1, SoqlRelationalOperator.NotEqual, o1_2, true);

            Assume(o2_1, SoqlRelationalOperator.LessOrEqual, o1_1, true);
            Assume(o2_1, SoqlRelationalOperator.GreaterOrEqual, o1_1, true);

            AssumeAntiSymmetric(o2_1, SoqlRelationalOperator.Less, o1_2, true);
            AssumeAntiSymmetric(o2_2, SoqlRelationalOperator.Greater, o1_1, true);

            AssumeException(o2_1, SoqlRelationalOperator.Like, 1);
        }

        [Test]
        public void TestInt32AndString()
        {
            CheckNumberPair(1, 2, "1", "2");
        }

        [Test]
        public void TestInt32AndInt16()
        {
            CheckNumberPair(1, 2, (short)1, (short)2);
        }

        [Test]
        public void TestInt32AndInt64()
        {
            CheckNumberPair(1, 2, (long)1, (long)2);
        }

        [Test]
        public void TestInt8AndInt64()
        {
            CheckNumberPair((sbyte)1, (sbyte)2, (long)1, (long)2);
        }

        [Test]
        public void TestDateTimeAndString()
        {
            CheckNumberPair(DateTime.Parse("2000-01-01"), DateTime.Parse("2001-01-01"), "2000-01-01", "2001-01-01");
        }

        [Test]
        public void TestString()
        {
            CheckNumberPair("aaa", "bbb");
            CheckNumberPair("AAA", "bbb");
            CheckNumberPair("aaa", "BBB");
            CheckNumberPair("AAA", "BBB");
        }

        private void CheckUnsupported(object o1, object o2)
        {
            AssumeException(o1, SoqlRelationalOperator.Equal, o2);
            AssumeException(o1, SoqlRelationalOperator.NotEqual, o2);
            AssumeException(o1, SoqlRelationalOperator.Less, o2);
            AssumeException(o1, SoqlRelationalOperator.Greater, o2);
            AssumeException(o1, SoqlRelationalOperator.LessOrEqual, o2);
            AssumeException(o1, SoqlRelationalOperator.GreaterOrEqual, o2);
            AssumeException(o1, SoqlRelationalOperator.Like, o2);

            AssumeException(o2, SoqlRelationalOperator.Equal, o1);
            AssumeException(o2, SoqlRelationalOperator.NotEqual, o1);
            AssumeException(o2, SoqlRelationalOperator.Less, o1);
            AssumeException(o2, SoqlRelationalOperator.Greater, o1);
            AssumeException(o2, SoqlRelationalOperator.LessOrEqual, o1);
            AssumeException(o2, SoqlRelationalOperator.GreaterOrEqual, o1);
            AssumeException(o2, SoqlRelationalOperator.Like, o1);
        }

        [Test]
        public void TestUnsupported()
        {
            CheckUnsupported(DateTime.Now, false);
            CheckUnsupported(DateTime.Now, (sbyte)1);
            CheckUnsupported(DateTime.Now, (short)1);
            CheckUnsupported(DateTime.Now, (int)1);
            CheckUnsupported(DateTime.Now, (long)1);
            CheckUnsupported(DateTime.Now, (decimal)1);

            CheckUnsupported(DateTime.Now, (byte)1);
            CheckUnsupported(DateTime.Now, (ushort)1);
            CheckUnsupported(DateTime.Now, (uint)1);
            CheckUnsupported(DateTime.Now, (ulong)1);
        }

        static void ChainedEquals1()
        {
            Contact.GetList(new SoodaWhereClause("Active == true == true != false"));
        }

        [Test]
        public void ChainedEquals()
        {
            using (new SoodaTransaction())
            {
                Assert.Throws(typeof(SoqlException), ChainedEquals1);
            }
        }
    }
}
