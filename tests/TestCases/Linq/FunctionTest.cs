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
using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.BaseObjects.TypedQueries;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class FunctionTest
    {
        [Test]
        public void StringLike()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.Like("Mar%"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);

                ce = Contact.Linq().Where(c => c.Name.Like("Mary Manager"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Mary Manager", ce.First().Name);

                ce = Contact.Linq().Where(c => c.Name.Like("% Customer"));
                Assert.AreEqual(4, ce.Count());

                ce = Contact.Linq().Where(c => c.Name.Like("%e %"));
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => c.Name.Like("%"));
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => c.Name.Like("%%%"));
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void SoodaClass()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.GetType().Name == "Contact");
                Assert.AreEqual(7, ce.Count());

                IEnumerable<Vehicle> ve = Vehicle.Linq().Where(c => c.GetType().Name == "Car");
                Assert.AreEqual(2, ve.Count());
            }
        }
    }
}

#endif
