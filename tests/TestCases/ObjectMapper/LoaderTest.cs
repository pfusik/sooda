// 
// Copyright (c) 2015 Piotr Fusik <piotr@fusik.info>
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
using Sooda.UnitTests.BaseObjects;
using System;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class LoaderTest
    {
        [Test]
        public void FindByName()
        {
            using (new SoodaTransaction())
            {
                Contact c = Contact.FindByName("Mary Manager");
                Assert.AreEqual(Contact.Mary, c);
            }
        }

        [Test]
        public void FindByNameNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact c = Contact.FindByName("Jarek Kowalski");
                Assert.IsNull(c);
            }
        }

        [Test]
        public void FindByPrimaryGroupReference()
        {
            using (new SoodaTransaction())
            {
                Contact c = Contact.FindByPrimaryGroup(Group.GetRef(11));
                Assert.AreEqual(Contact.Eva, c);
            }
        }

        [Test]
        public void FindByPrimaryGroupId()
        {
            using (new SoodaTransaction())
            {
                Contact c = Contact.FindByPrimaryGroup(11);
                Assert.AreEqual(Contact.Eva, c);
            }
        }

        [Test]
        public void FindByPrimaryGroupIdNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact c = Contact.FindByPrimaryGroup(42);
                Assert.IsNull(c);
            }
        }

        [Test]
        public void FindListByTypeReference()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.FindListByType(ContactType.Employee);
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Ed, Contact.Eva }, cl);
            }
        }

        [Test]
        public void FindListByTypeId()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.FindListByType("Employee");
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Ed, Contact.Eva }, cl);
            }
        }

        [Test]
        public void FindListByTypeNotFound()
        {
            using (new SoodaTransaction())
            {
                ContactList cl = Contact.FindListByType("Mega Boss");
                CollectionAssert.IsEmpty(cl);
            }
        }
    }
}
