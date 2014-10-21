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

#if DOTNET35

using System.Linq;

using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class PrecommitTest
    {
        [Test]
        public void NewCount()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Count();
                Assert.AreEqual(7, n);

                new Contact();
                n = Contact.Linq().Count();
                Assert.AreEqual(8, n);
            }
        }

        [Test]
        public void UpdateCount()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Count(c => c.Active);
                Assert.AreEqual(7, n);

                Contact.Mary.Active = false;
                n = Contact.Linq().Count(c => c.Active);
                Assert.AreEqual(6, n);
                n = Contact.Linq().Count(c => !c.Active);
                Assert.AreEqual(1, n);
            }
        }

        [Test]
        public void DeleteCount()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Count();
                Assert.AreEqual(7, n);

                Contact.GetRef(5).MarkForDelete();
                n = Contact.Linq().Count();
                Assert.AreEqual(6, n);
            }
        }

        [Test]
        public void NewToList()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().ToList().Count;
                Assert.AreEqual(7, n);

                new Contact();
                n = Contact.Linq().ToList().Count;
                Assert.AreEqual(8, n);
            }
        }

        [Test]
        public void UpdateToList()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Where(c => c.Active).ToList().Count;
                Assert.AreEqual(7, n);

                Contact.Mary.Active = false;
                n = Contact.Linq().Where(c => c.Active).ToList().Count;
                Assert.AreEqual(6, n);
                n = Contact.Linq().Where(c => !c.Active).ToList().Count;
                Assert.AreEqual(1, n);
            }
        }

        [Test]
        public void DeleteToList()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().ToList().Count;
                Assert.AreEqual(7, n);

                Contact.GetRef(5).MarkForDelete();
                n = Contact.Linq().ToList().Count;
                Assert.AreEqual(6, n);
            }
        }

        [Test]
        public void NewSelect()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Select(c => c.Name).ToList().Count;
                Assert.AreEqual(7, n);

                new Contact();
                n = Contact.Linq().Select(c => c.Name).ToList().Count;
                Assert.AreEqual(8, n);
            }
        }

        [Test]
        public void UpdateSelect()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Where(c => c.Active).Select(c => c.Name).ToList().Count;
                Assert.AreEqual(7, n);

                Contact.Mary.Active = false;
                n = Contact.Linq().Where(c => c.Active).Select(c => c.Name).ToList().Count;
                Assert.AreEqual(6, n);
                n = Contact.Linq().Where(c => !c.Active).Select(c => c.Name).ToList().Count;
                Assert.AreEqual(1, n);
            }
        }

        [Test]
        public void DeleteSelect()
        {
            using (new SoodaTransaction())
            {
                int n = Contact.Linq().Select(c => c.Name).ToList().Count;
                Assert.AreEqual(7, n);

                Contact.GetRef(5).MarkForDelete();
                n = Contact.Linq().Select(c => c.Name).ToList().Count;
                Assert.AreEqual(6, n);
            }
        }
    }
}

#endif
