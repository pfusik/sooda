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

using Sooda.ObjectMapper;
using System.IO;
using Sooda.UnitTests.Objects;
using Sooda.QL;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Soql
{
    [TestFixture]
    public class EvaluateTest
    {
        private void AssertExpression(string s, object expectedValue)
        {
            SoqlExpression expr = SoqlParser.ParseExpression(s);
            SoqlPrettyPrinter pp = new SoqlPrettyPrinter(Console.Out);
            pp.PrintExpression(expr);
            object val = expr.Evaluate(null);
            Console.WriteLine(" = {0} (expected {1})", val, expectedValue);
            Assert.AreEqual(expectedValue, val);
        }

        [Test]
        public void TestRelationOperators()
        {
            // string & string
            AssertExpression("'a' == 'a'", true);
            AssertExpression("'a' == 'b'", false);
            AssertExpression("'a' != 'b'", true);
            AssertExpression("'a' != 'a'", false);
            AssertExpression("'a' > 'b'", false);
            AssertExpression("'a' > 'a'", false);
            AssertExpression("'b' > 'a'", true);
            AssertExpression("'a' >= 'b'", false);
            AssertExpression("'a' >= 'a'", true);
            AssertExpression("'b' >= 'a'", true);
            AssertExpression("'a' < 'b'", true);
            AssertExpression("'a' < 'a'", false);
            AssertExpression("'b' < 'a'", false);
            AssertExpression("'a' <= 'b'", true);
            AssertExpression("'a' <= 'a'", true);
            AssertExpression("'b' <= 'a'", false);

            // bool & bool
            AssertExpression("false == false", true);
            AssertExpression("false == true", false);
            AssertExpression("false != true", true);
            AssertExpression("false != false", false);
            AssertExpression("false > true", false);
            AssertExpression("false > false", false);
            AssertExpression("true > false", true);
            AssertExpression("false >= true", false);
            AssertExpression("false >= false", true);
            AssertExpression("true >= false", true);
            AssertExpression("false < true", true);
            AssertExpression("false < false", false);
            AssertExpression("true < false", false);
            AssertExpression("false <= true", true);
            AssertExpression("false <= false", true);
            AssertExpression("true <= false", false);

            // int & int
            AssertExpression("1 == 1", true);
            AssertExpression("1 == 2", false);
            AssertExpression("1 != 2", true);
            AssertExpression("1 != 1", false);
            AssertExpression("1 > 2", false);
            AssertExpression("1 > 1", false);
            AssertExpression("2 > 1", true);
            AssertExpression("1 >= 2", false);
            AssertExpression("1 >= 1", true);
            AssertExpression("2 >= 1", true);
            AssertExpression("1 < 2", true);
            AssertExpression("1 < 1", false);
            AssertExpression("2 < 1", false);
            AssertExpression("1 <= 2", true);
            AssertExpression("1 <= 1", true);
            AssertExpression("2 <= 1", false);

            // int & double
            AssertExpression("1 == 1.0", true);
            AssertExpression("1 == 1.5", false);
            AssertExpression("1 != 1.5", true);
            AssertExpression("1 != 1.0", false);
            AssertExpression("1 > 1.5", false);
            AssertExpression("2 > 1.5", true);
            AssertExpression("1 > 0.0", true);
            AssertExpression("1 >= 1.5", false);
            AssertExpression("1 >= 1.0", true);
            AssertExpression("2 >= 1.5", true);
            AssertExpression("1 < 1.5", true);
            AssertExpression("1 < 1.0", false);
            AssertExpression("2 < 1", false);
            AssertExpression("1 <= 1.5", true);
            AssertExpression("1 <= 1.0", true);
            AssertExpression("2 <= 1.5", false);

            // double & double
            AssertExpression("1.0 == 1.0", true);
            AssertExpression("1.0 == 2.0", false);
            AssertExpression("1.0 != 2.0", true);
            AssertExpression("1.0 != 1.0", false);
            AssertExpression("1.0 > 2.0", false);
            AssertExpression("1.0 > 1.0", false);
            AssertExpression("2.0 > 1.0", true);
            AssertExpression("1.0 >= 2.0", false);
            AssertExpression("1.0 >= 1.0", true);
            AssertExpression("2.0 >= 1.0", true);
            AssertExpression("1.0 < 2.0", true);
            AssertExpression("1.0 < 1.0", false);
            AssertExpression("2.0 < 1.0", false);
            AssertExpression("1.0 <= 2.0", true);
            AssertExpression("1.0 <= 1.0", true);
            AssertExpression("2.0 <= 1.0", false);

            // double & int
            AssertExpression("1.0 == 1", true);
            AssertExpression("1.0 == 2", false);
            AssertExpression("1.0 != 2", true);
            AssertExpression("1.0 != 1", false);
            AssertExpression("1.0 > 2", false);
            AssertExpression("1.0 > 1", false);
            AssertExpression("2.0 > 1", true);
            AssertExpression("1.0 >= 2", false);
            AssertExpression("1.0 >= 1", true);
            AssertExpression("2.0 >= 1", true);
            AssertExpression("1.0 < 2", true);
            AssertExpression("1.0 < 1", false);
            AssertExpression("2.0 < 1", false);
            AssertExpression("1.0 <= 2", true);
            AssertExpression("1.0 <= 1", true);
            AssertExpression("2.0 <= 1", false);
        }

        [Test]
        public void TestCoreOperations()
        {
            AssertExpression("1 + 2", 3);
            AssertExpression("1 + 2 + 3", 6);
            AssertExpression("1 + 2 * 3", 7);
            AssertExpression("2 * 1 * 3", 6);
            AssertExpression("1 / 2 + 3", 3);
            AssertExpression("5.0 / (2 + 8)", 0.5);
            AssertExpression("((((1))))", 1);
            AssertExpression("((((1 + 2))))", 3);
            AssertExpression("((((1 + 2)+(2 + 1))))", 6);
            AssertExpression("((((1 + 2)/(2 + 1))))", 1);
            AssertExpression("-1", -1);
            AssertExpression("--1", 1);
            AssertExpression("10 % 3", 1);
            AssertExpression("10 % 3 % 5", 1);
            AssertExpression("-1 == 1 - 2", true);
            AssertExpression("--1.0 == 1.0", true);
            AssertExpression("1 != 1", false);
            AssertExpression("1 == 2", false);
            AssertExpression("10.0 - 1.0 >= 8.9", true);
            AssertExpression("10.0 + 1 <= 11.1", true);
            AssertExpression("1 * 1.0 == 1.0", true);
        }

        [Test]
        public void TestRelationalOperators()
        {
            AssertExpression("'a' + 'b' == 'ab'", true);
            AssertExpression("true", true);
            AssertExpression("false", false);
        }

        [Test]
        public void TestLogicalOperators()
        {
            AssertExpression("true or false or false", true);
            AssertExpression("false or false or false", false);
            AssertExpression("false or true", true);
            AssertExpression("true and false", false);
            AssertExpression("true and true and false", false);
            AssertExpression("true and true and true", true);
            AssertExpression("false and true and true", false);
            AssertExpression("not true", false);
            AssertExpression("not false", true);
            AssertExpression("not (1==1)", false);
            AssertExpression("true or not (1 == 1)", true);
            AssertExpression("true or not (--1 == 1)", true);
            AssertExpression("not true or false", false);
            AssertExpression("not false and false", false);
            AssertExpression("not false or false", true);
            AssertExpression("not true and true", false);
            AssertExpression("not true or true", true);
            AssertExpression("not (true and false)", true);
        }

        [Test]
        public void TestAssociativity()
        {
            AssertExpression("3 - 2 - 1", 0);
            AssertExpression("6 / 3 * 2", 4);
            AssertExpression("1 + 2 * 3", 7);
            AssertExpression("1 + 2 * 3 - 4 * 5", -13);
            AssertExpression("1 - 4 / 2 - 4 % 2", -1);
        }

        [Test]
        public void TestIn()
        {
            AssertExpression("42 in (1, 5, 42)", true);
            AssertExpression("13 in (1, 5, 42)", false);
            AssertExpression("42 in (42)", true);
            AssertExpression("13 in ()", false);
        }
    }
}
