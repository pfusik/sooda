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

using Sooda.ObjectMapper;
using Sooda.QL;
using System.IO;
using Sooda.UnitTests.Objects;

using NUnit.Framework;

#if A

namespace Sooda.UnitTests.TestCases.Soql {
    [TestFixture]
    public class ConstantFoldingTest {
        static void AssertSimplifies(string input, string expectedResult) {
            StringWriter sw0 = new StringWriter();
            StringWriter sw1 = new StringWriter();

            SoqlPrettyPrinter pp0 = new SoqlPrettyPrinter(sw0);
            SoqlPrettyPrinter pp1 = new SoqlPrettyPrinter(sw1);

            pp0.PrintExpression(Sooda.QL.SoqlParser.ParseExpression(input).Simplify());
            pp1.PrintExpression(Sooda.QL.SoqlParser.ParseExpression(expectedResult));

            Console.WriteLine("Test: |{0}| => |{1}| => |{2}|", input, sw0.ToString(), sw1.ToString());
            Assertion.AssertEquals(input + " simplifies to " + expectedResult, sw1.ToString(), sw0.ToString());
        }

        [Test]
        public void Test1() {
            AssertSimplifies("true || false || true", "True");
        }
        [Test]
        public void Test2() {
            AssertSimplifies("true && false && true", "False");
        }
        [Test]
        public void Test3() {
            AssertSimplifies("true && (a = b)", "(a = b)");
        }
        [Test]
        public void Test4() {
            AssertSimplifies("true || (a = b)", "True");
        }
        [Test]
        public void Test5() {
            AssertSimplifies("(a = b) || (1 = 1)", "True");
        }
        [Test]
        public void Test6() {
            AssertSimplifies("(a = b) || (1 < 3)", "True");
        }
        [Test]
        public void Test7() {
            AssertSimplifies("(a = b) && (1 < 3)", "(a = b)");
        }
        [Test]
        public void Test8() {
            AssertSimplifies("(a = b) || (1 = 0)", "(a = b)");
        }
        [Test]
        public void Test9() {
            AssertSimplifies("false || (a = b)", "(a = b)");
        }

        [Test]
        public void Test10() {
            AssertSimplifies("1 + a", "(1 + a)");
        }
        [Test]
        public void Test11() {
            AssertSimplifies("1 + 1", "2");
        }
        [Test]
        public void Test12() {
            AssertSimplifies("1 + 2 * 3", "7");
        }
        [Test]
        public void Test13() {
            AssertSimplifies("1 + 2 % 3", "3");
        }
        [Test]
        public void Test14() {
            AssertSimplifies("(1 + 2) % 3", "0");
        }
        [Test]
        public void Test15() {
            AssertSimplifies("1 * 2 + 3", "5");
        }
        [Test]
        public void Test16() {
            AssertSimplifies("(1 + 2) * 3", "9");
        }
        [Test]
        public void Test17() {
            AssertSimplifies("1 - 4 / 2", "-(1)");
        }
        [Test]
        public void Test18() {
            AssertSimplifies("(a + 1) * (a - 1)", "((a + 1) * (a - 1))");
        }
        [Test]
        public void Test19() {
            AssertSimplifies("1 - 4 % 2", "1");
        }
    }
}

#endif
