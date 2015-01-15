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

using NUnit.Framework;
using Sooda.UnitTests.Objects;
using System;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class CyclicReferenceTest
    {
        [Test]
        public void MissingReference()
        {
            int id1;
            int id2;
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                EightFields o1 = new EightFields();
                EightFields o2 = new EightFields();
                o1.TimeSpan = TimeSpan.FromSeconds(1);
                o2.TimeSpan = TimeSpan.FromSeconds(2);
                id1 = o1.Id;
                id2 = o2.Id;
                o1.Parent = o2;
                o2.Parent = o1;
                tran.Commit();
            }

            using (SoodaTransaction tran = new SoodaTransaction())
            {
                tran.CachingPolicy = new Sooda.Caching.SoodaCacheAllPolicy();
                EightFields o1 = EightFields.GetRef(id1);
                EightFields o2 = EightFields.GetRef(id2);
                Assert.AreEqual(o2, o1.Parent);
                Assert.AreEqual(o1, o2.Parent);
                tran.Commit();
            }
        }
    }
}
