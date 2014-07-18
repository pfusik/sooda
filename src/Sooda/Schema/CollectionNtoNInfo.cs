// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace Sooda.Schema
{
    using System.Xml.Serialization;
    using System;
    using System.ComponentModel;

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    [Serializable]
    public class CollectionManyToManyInfo : CollectionBaseInfo
    {

        [System.Xml.Serialization.XmlAttributeAttribute("relation")]
        public string Relation;

        [System.Xml.Serialization.XmlAttributeAttribute("masterField")]
        [DefaultValue(-1)]
        public int MasterField = -1;

        [System.Xml.Serialization.XmlAttributeAttribute("foreignField")]
        public string ForeignField;

        private RelationInfo relationInfo = null;

        public RelationInfo GetRelationInfo()
        {
            return relationInfo;
        }

        internal void Resolve(SchemaInfo schemaInfo)
        {
            relationInfo = schemaInfo.FindRelationByName(Relation);
            if (relationInfo == null)
                throw new SoodaSchemaException("Relation " + this.Name + " not found.");

            if (ForeignField != null)
            {
                if (relationInfo.Table.Fields[0].Name == ForeignField)
                    MasterField = 1;
                else
                    MasterField = 0;
            }
            if (MasterField == -1)
                throw new SoodaConfigException("You need to set either masterField or foreignField in <collectionManyToMany name='" + this.Name + "' />");
        }

        public override ClassInfo GetItemClass()
        {
            return relationInfo.Table.Fields[MasterField].ReferencedClass;
        }
    }
}
