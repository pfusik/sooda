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

using System;
using System.Collections.Generic;
using System.Linq;

using Sooda;

using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases
{
    [TestFixture]
    public class DynamicFieldsTest
    {
        [Test]
        public void IndexerGet()
        {
            using (new SoodaTransaction())
            {
                object result = Contact.Mary["Name"];
                Assert.AreEqual("Mary Manager", result);

                result = Contact.Mary["Active"];
                Assert.AreEqual(true, result);
            }
        }

        [Test]
        public void IndexerGetRef()
        {
            using (new SoodaTransaction())
            {
                object result = Contact.Ed["Manager"];
                Assert.AreEqual(Contact.Mary, result);

                result = Contact.Mary["Manager"];
                Assert.IsNull(result);
            }
        }

        [Test]
        public void IndexerGetPrimaryKey()
        {
            using (new SoodaTransaction())
            {
                object result = Contact.Ed["ContactId"];
                Assert.AreEqual(2, result);
            }
        }

        [Test]
        public void IndexerGetChanged()
        {
            using (new SoodaTransaction())
            {
                Contact.Mary.Name = "Mary Dismissed";
                Contact.Mary.Active = false;

                object result = Contact.Mary["Name"];
                Assert.AreEqual("Mary Dismissed", result);

                result = Contact.Mary["Active"];
                Assert.AreEqual(false, result);
            }
        }

        [Test]
        public void IndexerGetRefChanged()
        {
            using (new SoodaTransaction())
            {
                Contact.Ed.Manager = Contact.Eva;
                Contact.Eva.Manager = null;

                object result = Contact.Ed["Manager"];
                Assert.AreEqual(Contact.Eva, result);

                result = Contact.Eva["Manager"];
                Assert.IsNull(result);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void IndexerGetNonExisting()
        {
            using (new SoodaTransaction())
            {
                object result = Contact.Mary["NoSuchField"];
            }
        }

        [Test]
        public void Where()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c["Name"] == "Mary Manager");
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Mary }, ce);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void WhereNonExisting()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Any(c => c["NoSuchField"] == "Mary Manager");
            }
        }

        [Test]
        public void OrderBySelect()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from c in Contact.Linq() orderby c["Name"] select (string) c["Name"];
                CollectionAssert.AreEqual(new string[] { "Caroline Customer", "Catie Customer", "Chris Customer", "Chuck Customer", "Ed Employee", "Eva Employee", "Mary Manager" }, se);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SelectNonExisting()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Select(c => c["NoSuchField"]).ToList();
            }
        }
    }
}
