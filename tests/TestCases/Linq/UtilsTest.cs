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

using System.Collections.Generic;

using Sooda.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class UtilsTest
    {
        [Test]
        public void Like()
        {
            Assert.IsTrue("foo".Like("foo"));
            Assert.IsFalse("foo".Like("bar"));
            Assert.IsFalse("f".Like("foo"));
            Assert.IsFalse("o".Like("foo"));
            Assert.IsFalse("foo".Like("f"));
            Assert.IsFalse("foo".Like("o"));
        }

        [Test]
        public void LikeIgnoreCase()
        {
            Assert.IsTrue("Foo".Like("foO"));
            Assert.IsTrue("Foo".Like("f%"));
        }

        [Test]
        public void LikeWildcard()
        {
            Assert.IsTrue("foo".Like("f%"));
            Assert.IsTrue("foo".Like("%o"));
            Assert.IsTrue("foo".Like("f%o"));
            Assert.IsTrue("foo".Like("%foo"));
            Assert.IsTrue("foo".Like("%"));
            Assert.IsTrue("foo".Like("%%"));
            Assert.IsTrue("foo".Like("%%%%"));
        }

        [Test]
        public void LikeAnyChar()
        {
            Assert.IsTrue("foo".Like("f_o"));
            Assert.IsTrue("foo".Like("_oo"));
            Assert.IsTrue("foo".Like("___"));
            Assert.IsFalse("foo".Like("____"));
            Assert.IsFalse("_".Like("f"));

            Assert.IsTrue("foo".Like("_o%"));
            Assert.IsTrue("foo".Like("_%__"));
        }

        [Test]
        public void LikeEmpty()
        {
            Assert.IsTrue("".Like(""));
            Assert.IsTrue("".Like("%"));
            Assert.IsTrue("".Like("%%"));
            Assert.IsTrue("".Like("%%%"));
            Assert.IsFalse("".Like("_"));
            Assert.IsFalse("foo".Like(""));
        }

        [Test]
        public void ToSoodaObjectListArray()
        {
            ContactList cl = new ContactList(new Contact[0].ToSoodaObjectList());
            Assert.AreEqual(0, cl.Count);

            using (new SoodaTransaction())
            {
                Contact[] a = new Contact[] { Contact.Mary, Contact.Ed };
                cl = new ContactList(a.ToSoodaObjectList());
                Assert.AreEqual(2, cl.Count);
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Ed }, cl);

                // array should have been copied
                a[1] = Contact.Mary;
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Ed }, cl);

                // should not affect the array
                cl.Add(Contact.Eva);
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Ed, Contact.Eva }, cl);
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Mary }, a);
            }
        }

        [Test]
        public void ToSoodaObjectListGenericList()
        {
            List<Contact> gl = new List<Contact>();
            ContactList cl = new ContactList(gl.ToSoodaObjectList());
            Assert.AreEqual(0, cl.Count);

            using (new SoodaTransaction())
            {
                gl.Add(Contact.Mary);
                gl.Add(Contact.Ed);
                Assert.AreEqual(0, cl.Count);
                cl = new ContactList(gl.ToSoodaObjectList());
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Ed }, cl);

                // ArrayList should have been copied
                gl[1] = Contact.Mary;
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Ed }, cl);

                // should not affect the array
                cl.Add(Contact.Eva);
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Ed, Contact.Eva }, cl);
                CollectionAssert.AreEqual(new Contact[] { Contact.Mary, Contact.Mary }, gl);
            }
        }
    }
}

#endif
