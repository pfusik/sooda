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
#if DOTNET35
using System.Linq;
#endif

using Sooda;
using Sooda.Schema;

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

#if DOTNET35
        [Test]
        public void Where()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => (string) c["Name"] == "Mary Manager");
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Mary }, ce);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void WhereNonExisting()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Any(c => (string) c["NoSuchField"] == "Mary Manager");
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

        [Test]
        public void AddRemove()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                ClassInfo ci = Sooda.UnitTests.BaseObjects.Stubs.Contact_Factory.TheClassInfo; // doesn't work: tran.Schema.FindClassByName("Contact")
                const string fieldName = "FirstDynamicField";
                DynamicFieldManager.Add(new FieldInfo {
                        ParentClass = ci,
                        Name = fieldName,
                        TypeName = "Integer",
                        IsNullable = false
                    }, tran);
                try
                {
                    object value = Contact.Mary[fieldName];
                    Assert.IsNull(value);
                }
                finally
                {
                    FieldInfo fi = ci.FindFieldByName(fieldName);
                    DynamicFieldManager.Remove(fi, tran);
                }
            }
        }
#endif

#if DOTNET4
        [Test]
        public void DynamicGet()
        {
            using (new SoodaTransaction())
            {
                dynamic d = Contact.Mary;
                object result = d.Name;
                Assert.AreEqual("Mary Manager", result);

                result = d.Active;
                Assert.AreEqual(true, result);
            }
        }

        [Test]
        public void DynamicGetRef()
        {
            using (new SoodaTransaction())
            {
                dynamic d = Contact.Ed;
                object result = d.Manager;
                Assert.AreEqual(Contact.Mary, result);

                d = Contact.Mary;
                result = d.Manager;
                Assert.IsNull(result);
            }
        }

        [Test]
        public void DynamicGetPrimaryKey()
        {
            using (new SoodaTransaction())
            {
                dynamic d = Contact.Ed;
                object result = d.ContactId;
                Assert.AreEqual(2, result);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void DynamicGetNonExisting()
        {
            using (new SoodaTransaction())
            {
                dynamic d = Contact.Mary;
                object result = d.NoSuchField;
            }
        }

        [Test]
        public void DynamicGetLabel()
        {
            using (new SoodaTransaction())
            {
                dynamic d = Contact.Mary;
                object result = d.GetLabel(false);
                Assert.AreEqual("Mary Manager", result);
            }
        }

        [Test]
        public void DynamicGetCodeProperty()
        {
            using (new SoodaTransaction())
            {
                dynamic d = Contact.Mary;
                object result = d.NameAndType;
                Assert.AreEqual("Mary Manager (Manager)", result);
            }
        }

        [Test]
        public void DynamicAddRemove()
        {
            using (SoodaTransaction tran = new SoodaTransaction())
            {
                ClassInfo ci = Sooda.UnitTests.BaseObjects.Stubs.Contact_Factory.TheClassInfo; // doesn't work: tran.Schema.FindClassByName("Contact")
                const string fieldName = "FirstDynamicField";
                DynamicFieldManager.Add(new FieldInfo {
                        ParentClass = ci,
                        Name = fieldName,
                        TypeName = "Integer",
                        IsNullable = false
                    }, tran);

                dynamic d = Contact.Mary;
                try
                {
                    object value = d.FirstDynamicField;
                    Assert.IsNull(value);
                }
                finally
                {
                    FieldInfo fi = ci.FindFieldByName(fieldName);
                    DynamicFieldManager.Remove(fi, tran);
                }
            }
        }
#endif
    }
}
