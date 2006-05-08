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
using System.Data;

using Sooda.ObjectMapper;
using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class RetrievalTest
    {
        [Test]
        public void Test1()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                Assert.AreEqual("Mary Manager", Contact.Mary.Name);
                Assert.AreEqual("Ed Employee", Contact.Ed.Name);
                Assert.AreEqual("Group1", (string)Contact.Mary.PrimaryGroup.Name);

                Assert.AreEqual(ContactType.Manager, Contact.Mary.Type);
                Assert.AreEqual(ContactType.Employee, Contact.Ed.Type);
                Assert.AreEqual(ContactType.Employee, Contact.Eva.Type);

                Assert.IsTrue(Contact.Mary.PrimaryGroup.Members.Contains(Contact.Mary));
                Assert.IsTrue(Contact.Ed.PrimaryGroup.Members.Contains(Contact.Ed));
                Assert.IsTrue(Contact.Eva.PrimaryGroup.Members.Contains(Contact.Eva));
            }
        }
    }
}
