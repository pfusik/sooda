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
using System.IO;

using Sooda.ObjectMapper;
using Sooda.QL;
using Sooda.UnitTests.Objects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Soql
{
    [TestFixture]
    public class SoodaObjectMatchingTest
    {
        [Test]
        public void MatchTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Contact c1 = Contact.GetRef(1);
                ISoqlEvaluateContext context = new SoodaObjectEvaluateContext(c1, new object[] {});

                c1.Name = null;

                //Console.WriteLine("name: {0}", c1.Name);
                SoqlExpression e = Sooda.QL.SoqlParser.ParseWhereClause("Name = 'Mary Manager'");
                Console.WriteLine("RESULT: {0}", e.Evaluate(context));

                e = Sooda.QL.SoqlParser.ParseWhereClause("'Group1' = PrimaryGroup.Name");
                Console.WriteLine("RESULT: {0}", e.Evaluate(context));

                e = Sooda.QL.SoqlParser.ParseWhereClause("PrimaryGroup.Name != ''");
                Console.WriteLine("RESULT: {0}", e.Evaluate(context));
            }
        }
        
        [Test]
        public void StringLikeMatchTest()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Contact c1 = Contact.GetRef(1);
                ISoqlEvaluateContext context = new SoodaObjectEvaluateContext(c1, new object[] {});

                SoqlExpression e;
                e = Sooda.QL.SoqlParser.ParseWhereClause("PrimaryGroup.Name like 'Group%'");
                Console.WriteLine("RESULT: {0}", e.Evaluate(context));
            }
        }
    }
}
