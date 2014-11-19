//
// Copyright (c) 2014 Piotr Fusik <piotr@fusik.info>
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

#if DOTNET4

using NUnit.Framework;
using Sooda.Linq;
using Sooda.UnitTests.BaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sooda.UnitTests.TestCases.Linq
{
    public class DoubleRewriter : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(RewriterTest) && node.Method.Name == "Double")
                return Expression.Add(node.Arguments[0], node.Arguments[0], typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
            return base.VisitMethodCall(node);
        }
    }

    [TestFixture]
    public class RewriterTest
    {
        [SoodaExpressionRewrite(typeof(DoubleRewriter))]
        static string Double(string s)
        {
            return s + s;
        }

        [Test]
        public void Double()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Double(c.Name) == "Mary ManagerMary Manager");
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary }, ce);

                ce = Contact.Linq().Where(c => Double(c.Name) == "Mary Manager");
                CollectionAssert.IsEmpty(ce);
            }
        }

        [Test]
        public void DoubleNested()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Double(Double(c.Name)) == "Mary ManagerMary ManagerMary ManagerMary Manager");
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary }, ce);
            }
        }
    }
}

#endif
