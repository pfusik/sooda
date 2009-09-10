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
    using System;
    using System.Xml.Serialization;
    using System.Data;
    using System.Collections;

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    public enum TableUsageType
    {
        Normal,
        Dictionary,
        OccasionallyModificated
    }

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    [Serializable]
    public class TableInfo
    {
        [System.Xml.Serialization.XmlElementAttribute("field")]
        public FieldInfoCollection Fields = new FieldInfoCollection();

        private string _dbTableName = null;
        private TableUsageType _usageType = TableUsageType.Normal;

        [NonSerialized]
        [XmlIgnore]
        public ClassInfo OwnerClass = null;

        [NonSerialized]
        [XmlIgnore]
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
            get
            {
                return _dbTableName;
            }
            set
            {
                _dbTableName = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute("usage")]
        [System.ComponentModel.DefaultValueAttribute(TableUsageType.Normal)]
        public TableUsageType TableUsageType
        {
            get
            {
                return _usageType;
            }
            set
            {
                _usageType = value;
            }
        }

        public FieldInfo FindFieldByName(string fieldName)
        {
            if (fieldName == null)
                return null;

            if (fieldsNameHash == null)
                this.Rehash();

            return (FieldInfo)fieldsNameHash[fieldName];
        }

        public FieldInfo FindFieldByDBName(string fieldName)
        {
            if (fieldName == null)
                return null;

            return (FieldInfo)fieldsDBNameHash[fieldName];
        }

        public void AddField(FieldInfo fi)
        {
            if (ContainsField(fi.Name))
                throw new SoodaSchemaException(String.Format("Duplicate field '{0}' found!", fi.Name));

            if (Fields == null)
                Fields = new FieldInfoCollection();

            Fields.Add(fi);
            Rehash();
        }

        public void AddReference(string fieldName, string refTable)
        {
            // only for building

            FieldInfo fi = FindFieldByDBName(fieldName);
            fi.References = refTable;
        }

        public bool ContainsField(string fieldName)
        {
            return FindFieldByName(fieldName) != null;
        }

        [NonSerialized]
        private Hashtable fieldsNameHash;

        [NonSerialized]
        private Hashtable fieldsDBNameHash;

        [NonSerialized]
        [XmlIgnore]
        public TableInfo[] ArraySingleton;

        internal void Rehash()
        {
            ArraySingleton = new TableInfo[] { this };

            fieldsNameHash = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();
            fieldsDBNameHash = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

            foreach (FieldInfo fi in Fields)
            {
                if (fi.Name != null)
                    fieldsNameHash[fi.Name] = fi;
                if (fi.DBColumnName != null)
                    fieldsDBNameHash[fi.DBColumnName] = fi;
            };
        }

        internal void Resolve(string name, bool isInRelation)
        {
            int ordinal = 0;
            int pkCount = 0;
            foreach (FieldInfo fi in Fields)
            {
                fi.Resolve(this, name, ordinal++);
                if (fi.IsPrimaryKey)
                    pkCount++;
            }
            if (pkCount == 0 && !isInRelation)
            {
                throw new SoodaSchemaException("Table '" + NameToken + "' doesn't have a primary key. If you declare a <class> that's based on more than one <table>, you need to have a primary key in each <table>. The primary key can be shared.");
            }
        }

        public TableInfo Clone(ClassInfo newParent)
        {
            TableInfo tableInfo = (TableInfo)this.MemberwiseClone();

            tableInfo.OwnerClass = newParent;
            return tableInfo;
        }

        internal void Merge(TableInfo merge)
        {
            //            Hashtable mergeNames = new Hashtable();
            //            foreach (FieldInfo fi in this.Fields)
            //                mergeNames.Add(fi.Name, fi);
            //            foreach (FieldInfo mfi in merge.Fields)
            //                if (!mergeNames.ContainsKey(mfi.Name))
            //                    this.AddField(mfi);
            //                else
            //                    throw new SoodaSchemaException(String.Format("Duplicate field '{0}' found!", fi.Name));
            foreach (FieldInfo mfi in merge.Fields)
                this.AddField(mfi);
        }
    }
}
