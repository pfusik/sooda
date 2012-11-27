// 
// Copyright (c) 2012 Piotr Fusik <piotr@fusik.info>
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

#if DOTNET35

using System;
using System.Collections.Generic;
using System.Linq;

using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.BaseObjects.TypedQueries;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class ClauseTest
    {
        [Test]
        public void Linq()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq();
                Assert.AreEqual(7, ce.Count());
                Contact[] ca = Contact.Linq().ToArray();
                Assert.AreEqual(7, ca.Length);
            }
        }

        [Test]
        public void Where()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name == "Mary Manager");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);

                ce = from c in Contact.Linq() where c.Name == "Mary Manager" select c;
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);
            }
        }

        [Test]
        public void Select()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = Contact.Linq().Select(c => c.ContactId);
                Assert.AreEqual(7, ie.Count());
                Assert.AreEqual(1 + 2 + 3 + 50 + 51 + 52 + 53, ie.Sum());

                IEnumerable<bool> be = Contact.Linq().Select(c => c.Active);
                Assert.AreEqual(7, be.Count());
                Assert.IsTrue(be.All(b => b));
            }
        }
    }
}

#endif
