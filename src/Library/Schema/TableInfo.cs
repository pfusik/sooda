// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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

namespace Sooda.Schema {
    using System;
    using System.Xml.Serialization;
    using System.Data;
    using System.Collections;

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://sooda.sourceforge.net/schemas/DBSchema.xsd")]
    [Serializable]
    public class TableInfo {
        [System.Xml.Serialization.XmlElementAttribute("field")]
        public FieldInfoCollection Fields = new FieldInfoCollection();

        private string _dbTableName = null;

        [NonSerialized]
        [XmlIgnore]
        public ClassInfo OwnerClass = null;
        public int OrdinalInClass = 0;

        [System.Xml.Serialization.XmlAnyAttribute()]
        [NonSerialized]
        public System.Xml.XmlAttribute[] Extensions;

        [NonSerialized]
        [XmlIgnore]
        public string NameToken;

        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string DBTableName
        {
            get {
                return _dbTableName;
            }
            set {
                _dbTableName = value;
            }
        }

        public FieldInfo FindFieldByName(string fieldName) {
            if (fieldName == null)
                return null;

            return (FieldInfo)fieldsNameHash[fieldName];
        }

        public FieldInfo FindFieldByDBName(string fieldName) {
            if (fieldName == null)
                return null;

            return (FieldInfo)fieldsDBNameHash[fieldName];
        }

        public void AddField(FieldInfo fi) {
            if (ContainsField(fi.Name))
                throw new Exception("Cannot add FieldInfo twice");

            if (Fields == null)
                Fields = new FieldInfoCollection();

            Fields.Add(fi);
            Rehash();
        }

        public void AddReference(string fieldName, string refTable) {
            // only for building

            FieldInfo fi = FindFieldByDBName(fieldName);
            fi.References = refTable;
        }

        public bool ContainsField(string fieldName) {
            return FindFieldByName(fieldName) != null;
        }

        [NonSerialized]
        private Hashtable fieldsNameHash;

        [NonSerialized]
        private Hashtable fieldsDBNameHash;

        [NonSerialized]
        public TableInfo[] ArraySingleton;

        internal void Rehash() {
            ArraySingleton = new TableInfo[] { this };

            fieldsNameHash = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();
            fieldsDBNameHash = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

            foreach (FieldInfo fi in Fields) {
                if (fi.Name != null)
                    fieldsNameHash[fi.Name] = fi;
                if (fi.DBColumnName != null)
                    fieldsDBNameHash[fi.DBColumnName] = fi;
            };
        }

        internal void Resolve(string name) {
            int ordinal = 0;
            foreach (FieldInfo fi in Fields) {
                fi.Resolve(this, name, ordinal++);
            }
        }

        public TableInfo Clone(ClassInfo newParent) {
            TableInfo tableInfo = (TableInfo)this.MemberwiseClone();

            tableInfo.OwnerClass = newParent;
            return tableInfo;
        }
    }
}
