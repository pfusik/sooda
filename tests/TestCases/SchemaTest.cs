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
using Sooda.Schema;
using Sooda.UnitTests.BaseObjects;
using Sooda.UnitTests.BaseObjects.Stubs;

namespace Sooda.UnitTests.TestCases
{
    [TestFixture]
    public class SchemaTest
    {
        [Test]
        public void ParentClass()
        {
            ClassInfo classInfo = Contact_Factory.TheClassInfo;
            foreach (FieldInfo fi in classInfo.UnifiedFields)
            {
                Assert.AreEqual(classInfo, fi.ParentClass);
                Assert.IsNull(fi.ParentRelation);
            }
        }

        [Test]
        public void ParentRelation()
        {
            RelationInfo relationInfo = _DatabaseSchema.GetSchema().FindRelationByName("ContactToBike");
            foreach (FieldInfo fi in relationInfo.Table.Fields)
            {
                Assert.AreEqual(relationInfo, fi.ParentRelation);
                Assert.IsNull(fi.ParentClass);
            }
        }
    }
}
