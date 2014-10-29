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
using Sooda.QL;
using System.IO;
using Sooda.Schema;
using Sooda.UnitTests.Objects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Soql
{
    [TestFixture]
    public class GetInvolvedClassTest
    {
        private void AssertInvolved(string rootClassName, string expression, params string[] expectedClassNames)
        {
            SoqlExpression expr = SoqlParser.ParseExpression(expression);
            ClassInfo rootClass = _DatabaseSchema.GetSchema().FindClassByName(rootClassName);
            GetInvolvedClassesVisitor visitor = new GetInvolvedClassesVisitor(rootClass);
            expr.Accept(visitor);

            foreach (string s in expectedClassNames)
            {
                bool found = false;
                foreach (ClassInfo ci in visitor.Results)
                {
                    if (ci.Name == s)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, "Class " + s + " was not returned by GetInvolvedClasses on " + expression);
            }
            Assert.AreEqual(expectedClassNames.Length, visitor.Results.Count, "Extra involved classes returned by GetInvolvedClasses on " + expression);
        }

        [Test]
        public void Test1()
        {
            AssertInvolved("Contact", "Name", "Contact");
            AssertInvolved("Contact", "PrimaryGroup.Name", "Contact", "Group");
            AssertInvolved("Contact", "Name like PrimaryGroup.Name", "Contact", "Group");

            AssertInvolved("Contact", "1");
            AssertInvolved("Contact", "{0}");
        }
    }
}
