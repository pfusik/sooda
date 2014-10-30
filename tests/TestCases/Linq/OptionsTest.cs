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

using Sooda.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class OptionsTest
    {
        [Test]
        public void Default()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq();
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void NoWriteObjects()
        {
            using (new SoodaTransaction())
            {
                Contact c50 = Contact.Load(50);
                c50.Name = "Barbra Streisland";
                IEnumerable<Contact> ce = Contact.Linq(SoodaSnapshotOptions.NoWriteObjects).Where(c => c.Name == "Barbara Streisland");
                Assert.AreEqual(0, ce.Count());
            }
        }

        [Test]
        public void Transaction()
        {
            using (SoodaTransaction trans1 = new SoodaTransaction())
            {
                using (new SoodaTransaction())
                {
                    Contact c50 = Contact.Load(50);
                    c50.Name = "Barbra Streisland";

                    IEnumerable<Contact> ce = Contact.Linq(trans1, SoodaSnapshotOptions.Default).Where(c => c.Name == "Barbara Streisland");
                    Assert.AreEqual(0, ce.Count());
                }
            }
        }
    }
}

#endif
