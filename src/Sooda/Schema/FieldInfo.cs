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

using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Collections.Specialized;

using Sooda.ObjectMapper;
using Sooda.ObjectMapper.FieldHandlers;

namespace Sooda.Schema
{

    [XmlTypeAttribute(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    [Serializable]
    public class FieldInfo : ICloneable
    {
        [XmlAttribute("name")]
        public string Name;

        private string dbcolumn;

        [XmlAttribute("type")]
        public FieldDataType DataType;

        [XmlElement("description")]
        public string Description;

        [XmlAttribute("size")]
        [System.ComponentModel.DefaultValueAttribute(-1)]
        public int Size = -1;

        [XmlAttribute("precision")]
        [System.ComponentModel.DefaultValueAttribute(-1)]
        public int Precision = -1;

        [XmlAttribute("references")]
        public string References;

        [XmlAttribute("precommitValue")]
        public string PrecommitValue;

        [XmlIgnore]
        [NonSerialized]
        public object PrecommitTypedValue;

        [XmlAttribute("primaryKey")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsPrimaryKey = false;

        [XmlAttribute("nullable")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsNullable = false;

        [XmlAttribute("forceTrigger")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ForceTrigger = false;

        [XmlAttribute("onDelete")]
        [System.ComponentModel.DefaultValueAttribute(DeleteAction.Nothing)]
        public DeleteAction DeleteAction = DeleteAction.Nothing;

        [XmlAnyAttribute()]
        [NonSerialized]
        public System.Xml.XmlAttribute[] Extensions;

        [XmlAttribute("label")]
        [DefaultValue(false)]
        public bool IsLabel = false;

        [XmlAttribute("prefetch")]
        [DefaultValue(0)]
        public int PrefetchLevel = 0;

        [XmlAttribute("find")]
        [DefaultValue(false)]
        public bool FindMethod = false;

        [XmlAttribute("findList")]
        [DefaultValue(false)]
        public bool FindListMethod = false;

        [XmlAttribute("dbcolumn")]
        public string DBColumnName
        {
            get
            {
                if (dbcolumn != null)
                    return dbcolumn;
                else
                    return Name;
            }
            set
            {
                dbcolumn = value;
            }
        }

        public FieldInfo Clone()
        {
            return DoClone();
        }

        object ICloneable.Clone()
        {
            return DoClone();
        }

        [XmlIgnore()]
        [NonSerialized]
        public int OrdinalInTable;

        [XmlIgnore()]
        [NonSerialized]
        public int ClassLocalOrdinal;

        [XmlIgnore()]
        [NonSerialized]
        public int ClassUnifiedOrdinal;

        [XmlIgnore]
        [NonSerialized]
        public TableInfo Table;

        [XmlIgnore]
        [NonSerialized]
        public ClassInfo ReferencedClass;

        [XmlIgnore]
        [NonSerialized]
        public ClassInfo ParentClass;

        [XmlIgnore]
        [NonSerialized]
        internal string NameTag;

        [XmlIgnore]
        [NonSerialized]
        public RelationInfo ParentRelation;

        public FieldInfo DoClone()
        {
            FieldInfo fi = new FieldInfo();

            fi.Name = this.Name;
            fi.dbcolumn = this.dbcolumn;
            fi.IsNullable = this.IsNullable;
            fi.DataType = this.DataType;
            fi.Size = this.Size;
            fi.ForceTrigger = this.ForceTrigger;

            return fi;
        }

        public StringCollection GetBackRefCollections(SchemaInfo schema)
        {
            return schema.GetBackRefCollections(this);
        }

        public SoodaFieldHandler GetFieldHandler()
        {
            return FieldHandlerFactory.GetFieldHandler(DataType);
        }

        internal void Resolve(TableInfo parentTable, string parentName, int ordinal)
        {
            this.Table = parentTable;
            this.OrdinalInTable = ordinal;
            this.NameTag = parentTable.NameToken + "/" + ordinal;
        }

        internal void ResolvePrecommitValues()
        {
        }

        public override string ToString()
        {
            return String.Format("{0}.{1} ({2} ref {3})", ParentClass != null ? "class " + ParentClass.Name : "???", Name, DataType, References);
        }

    }
}
