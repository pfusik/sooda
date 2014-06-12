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

using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class LabelTest
    {
        [Test]
        public void Basic()
        {
            using (new SoodaTransaction())
            {
                ContactList contacts = Contact.GetList(true, SoodaOrderBy.Ascending("ContactId"));
                Assert.AreEqual(7, contacts.Count);
                Assert.AreEqual("Mary Manager", contacts[0].GetLabel(false));
                Assert.AreEqual("Ed Employee", contacts[1].GetLabel(false));
                Assert.AreEqual("Eva Employee", contacts[2].GetLabel(false));
                Assert.AreEqual("Catie Customer", contacts[3].GetLabel(false));
                Assert.AreEqual("Caroline Customer", contacts[4].GetLabel(false));
                Assert.AreEqual("Chris Customer", contacts[5].GetLabel(false));
                Assert.AreEqual("Chuck Customer", contacts[6].GetLabel(false));
            }
        }

        [Test]
        public void NoLabel()
        {
            using (new SoodaTransaction())
            {
                ContactTypeList types = ContactType.GetList(true);
                Assert.AreEqual(3, types.Count);
                Assert.IsNull(types[0].GetLabel(false));
                Assert.IsNull(types[1].GetLabel(false));
                Assert.IsNull(types[2].GetLabel(false));
            }
        }

        [Test]
        public void ReferencedObject()
        {
            using (new SoodaTransaction())
            {
                VehicleList vehicles = Vehicle.GetList(true, SoodaOrderBy.Ascending("Id"));
                Assert.AreEqual(9, vehicles.Count);
                Assert.AreEqual(string.Empty, vehicles[0].GetLabel(false));
                Assert.AreEqual("Mary Manager", vehicles[1].GetLabel(false));
                Assert.AreEqual("Ed Employee", vehicles[2].GetLabel(false));
                Assert.AreEqual(string.Empty, vehicles[3].GetLabel(false));
                Assert.AreEqual(string.Empty, vehicles[4].GetLabel(false));
                Assert.AreEqual(string.Empty, vehicles[5].GetLabel(false));
                Assert.AreEqual(string.Empty, vehicles[6].GetLabel(false));
                Assert.AreEqual(string.Empty, vehicles[7].GetLabel(false));
                Assert.AreEqual(string.Empty, vehicles[8].GetLabel(false));
            }
        }

        [Test]
        public void Int()
        {
            using (new SoodaTransaction())
            {
                RoleList roles = Role.GetList(true, SoodaOrderBy.Ascending("Id"));
                Assert.AreEqual(3, roles.Count);
                Assert.AreEqual("1", roles[0].GetLabel(false));
                Assert.AreEqual("2", roles[1].GetLabel(false));
                Assert.AreEqual("3", roles[2].GetLabel(false));
            }
        }

        [Test]
        public void Count()
        {
            using (new SoodaTransaction())
            {
                GroupList groups = Group.GetList(true, SoodaOrderBy.Ascending("Id"));
                Assert.AreEqual(2, groups.Count);
                Assert.AreEqual("Sooda.UnitTests.BaseObjects.ContactList", groups[0].GetLabel(false));
                Assert.AreEqual("Sooda.UnitTests.BaseObjects.ContactList", groups[1].GetLabel(false));
            }
        }
    }
}
