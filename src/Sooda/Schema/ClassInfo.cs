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

namespace Sooda.Schema 
{
    using System;
    using System.Xml.Serialization;
    using System.Data;
    using System.Collections;

    /// <summary>
    /// Stores database table schema information
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://sooda.sourceforge.net/schemas/DBSchema.xsd")]
    [Serializable]
    public class ClassInfo : IFieldContainer 
    {
        [XmlAttribute("datasource")]
        public string DataSourceName;

        [System.Xml.Serialization.XmlElementAttribute("table")]
        public TableInfoCollection LocalTables;

        [System.Xml.Serialization.XmlElementAttribute("index")]
        [CLSCompliant(false)]
        public IndexInfoCollection Indexes = new IndexInfoCollection();

        [System.Xml.Serialization.XmlElementAttribute("collectionOneToMany")]
        public CollectionOnetoManyInfo[] Collections1toN;

        [System.Xml.Serialization.XmlElementAttribute("collectionManyToMany")]
        public CollectionManyToManyInfo[] CollectionsNtoN;

        [XmlIgnore]
        [NonSerialized]
        public CollectionBaseInfoCollection UnifiedCollections = new CollectionBaseInfoCollection();

        [XmlIgnore]
        [NonSerialized]
        public CollectionBaseInfoCollection LocalCollections = new CollectionBaseInfoCollection();

        [System.Xml.Serialization.XmlElementAttribute("const")]
        public ConstantInfo[] Constants;

        private string _name = null;

        [System.Xml.Serialization.XmlAnyAttribute()]
        [NonSerialized]
        public System.Xml.XmlAttribute[] Extensions;

        [System.Xml.Serialization.XmlAttributeAttribute("defaultPrecommitValue")]
        public string DefaultPrecommitValue;

        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name
        {
            get 
            {
                return _name;
            }
            set 
            {
                _name = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute("extBaseClassName")]
        public string ExtBaseClassName;

        bool _loadOnDemand = true;

        [System.Xml.Serialization.XmlAttributeAttribute("cached")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Cached = false;

        [System.Xml.Serialization.XmlAttributeAttribute("cardinality")]
        [System.ComponentModel.DefaultValueAttribute(ClassCardinality.Medium)]
        public ClassCardinality Cardinality = ClassCardinality.Medium;

        [System.Xml.Serialization.XmlAttributeAttribute("triggers")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Triggers = true;

        [System.Xml.Serialization.XmlAttributeAttribute("canDelete")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool CanDelete = false;

        [System.Xml.Serialization.XmlAttributeAttribute("readOnly")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ReadOnly = false;

        [System.Xml.Serialization.XmlAttributeAttribute("label")]
        public string LabelField = null;

        [System.Xml.Serialization.XmlAttributeAttribute("subclassSelectorField")]
        public string SubclassSelectorFieldName = null;

        [System.Xml.Serialization.XmlAttributeAttribute("subclassSelectorValue")]
        public string SubclassSelectorStringValue = null;

        [System.Xml.Serialization.XmlAttributeAttribute("inheritFrom")]
        public string InheritFrom = null;

        [System.Xml.Serialization.XmlAttributeAttribute("lastModifiedField")]
        public string LastModifiedFieldName;

        [System.Xml.Serialization.XmlAttributeAttribute("keygen")]
        public string KeyGenName;

        // array of FieldInfo's that point to this class
        [XmlIgnore()]
        [NonSerialized]
        public FieldInfoCollection OuterReferences;

        [XmlIgnore()]
        [NonSerialized]
        public FieldInfo SubclassSelectorField;

        [XmlIgnore()]
        [NonSerialized]
        public object SubclassSelectorValue;

        [System.Xml.Serialization.XmlAttributeAttribute("disableTypeCache")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool DisableTypeCache = false;

        [System.Xml.Serialization.XmlAttributeAttribute("loadOnDemand")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool LoadOnDemand
        {
            get 
            {
                if (ReadOnly)
                    return false;
                return _loadOnDemand;
            }
            set 
            {
                _loadOnDemand = value;
            }
        }
        public CollectionOnetoManyInfo FindCollectionOneToMany(string collectionName) 
        {
            if (Collections1toN != null) 
            {
                foreach (CollectionOnetoManyInfo i in Collections1toN) 
                {
                    if (collectionName == i.Name)
                        return i;
                }
            }
            if (InheritsFromClass != null)
                return InheritsFromClass.FindCollectionOneToMany(collectionName);
            return null;
        }

        public CollectionManyToManyInfo FindCollectionManyToMany(string collectionName) 
        {
            if (CollectionsNtoN != null) 
            {
                foreach (CollectionManyToManyInfo i in CollectionsNtoN) 
                {
                    if (collectionName == i.Name)
                        return i;
                }
            }
            if (InheritsFromClass != null)
                return InheritsFromClass.FindCollectionManyToMany(collectionName);
            return null;
        }

        public int ContainsCollection(string collectionName) 
        {
            if (FindCollectionOneToMany(collectionName) != null)
                return 1;
            if (FindCollectionManyToMany(collectionName) != null)
                return 2;
            return 0;
        }

        FieldInfo[] _primaryKeyFields = null;

        public FieldInfo[] GetPrimaryKeyFields() 
        {
            return _primaryKeyFields;
        }

        public FieldInfo GetFirstPrimaryKeyField() 
        {
            return _primaryKeyFields[0];
        }

        [NonSerialized]
        private SchemaInfo parentSchema;

        [NonSerialized]
        public FieldInfoCollection LocalFields;

        [NonSerialized]
        public FieldInfoCollection UnifiedFields;

        [NonSerialized]
        public TableInfoCollection MergedTables;

        [NonSerialized]
        public ClassInfo InheritsFromClass;

        [NonSerialized]
        public TableInfoCollection UnifiedTables;

        public ClassInfoCollection GetSubclassesForSchema(SchemaInfo schema)
        {
            return schema.GetSubclasses(this);
        }

        internal void ResolveInheritance(SchemaInfo schema) 
        {
            if (InheritFrom != null) 
            {
                InheritsFromClass = schema.FindClassByName(InheritFrom);
            } 
            else 
            {
                InheritsFromClass = null;
            }
        }

        internal void FlattenTables() 
        {
            // Console.WriteLine(">>> FlattenTables for {0}", Name);
            if (LocalTables == null)
                LocalTables = new TableInfoCollection();

            if (InheritsFromClass != null) 
            {
                if (InheritsFromClass.UnifiedTables == null || InheritsFromClass.UnifiedTables.Count == 0)
                {
                    InheritsFromClass.FlattenTables();
                }
                UnifiedTables = new TableInfoCollection();
                foreach (TableInfo ti in InheritsFromClass.UnifiedTables) 
                {
                    UnifiedTables.Add(ti.Clone(this));
                }
                foreach (TableInfo ti in LocalTables) 
                {
                    UnifiedTables.Add(ti);
                }
            } 
            else 
            {
                UnifiedTables = LocalTables;
            }

            int ordinalInClass = 0;

            foreach (TableInfo t in UnifiedTables) 
            {
                // Console.WriteLine("Setting OrdinalInClass for {0}.{1} to {2}", Name, t.DBTableName, ordinalInClass);
                t.OrdinalInClass = ordinalInClass++;
                t.NameToken = this.Name + "#" + t.OrdinalInClass;
                t.Rehash();
                t.OwnerClass = this;
                t.Resolve(this.Name);
            }

            if (UnifiedTables.Count > 30) 
            {
                throw new SoodaSchemaException("Class " + Name + " is invalid, because it's base on more than 30 tables. ");
            }
            // Console.WriteLine("<<< End of FlattenTables for {0}", Name);
        }

        internal void Resolve(SchemaInfo schema) 
        {
            if (parentSchema != null)
            {
                // already resolved, probably as part of included schema Resolve() process
                return;
            }

            OuterReferences = new FieldInfoCollection();
            parentSchema = schema;

            // local fields - a sum of all tables local to the class

            LocalFields = new FieldInfoCollection();
            int localOrdinal = 0;
            int count = 0;
            foreach (TableInfo table in UnifiedTables) 
            {
                foreach (FieldInfo fi in table.Fields) 
                {
                }
            }
            foreach (TableInfo table in LocalTables) 
            {
                foreach (FieldInfo fi in table.Fields) 
                {
                    // add all fields from the root table + all non-key fields
                    // from other tables

                    if (table.OrdinalInClass == 0 || !fi.IsPrimaryKey) 
                    {
                        // Console.WriteLine("Adding local field {0} to class {1}", fi.Name, Name);
                        LocalFields.Add(fi);
                        fi.ClassLocalOrdinal = localOrdinal++;
                    }
                }
                count++;
            }

            if (SubclassSelectorFieldName == null && InheritsFromClass != null) 
            {
                for (ClassInfo ci = this; ci != null; ci = ci.InheritsFromClass) 
                {
                    if (ci.SubclassSelectorFieldName != null) 
                    {
                        SubclassSelectorFieldName = ci.SubclassSelectorFieldName;
                        break;
                    }
                }
            }

            UnifiedCollections = new CollectionBaseInfoCollection();
            LocalCollections = new CollectionBaseInfoCollection();
            for (ClassInfo ci = this; ci != null; ci = ci.InheritsFromClass)
            {
                if (ci.Collections1toN != null)
                {
                    foreach (CollectionOnetoManyInfo c in ci.Collections1toN)
                    {
                        UnifiedCollections.Add(c);
                        if (ci == this)
                            LocalCollections.Add(c);
                    }
                }

                if (ci.CollectionsNtoN != null)
                {
                    foreach (CollectionManyToManyInfo c in ci.CollectionsNtoN)
                    {
                        UnifiedCollections.Add(c);
                        if (ci == this)
                            LocalCollections.Add(c);
                    }
                }
            }

            // all inherited fields + local fields

            UnifiedFields = new FieldInfoCollection();

            int unifiedOrdinal = 0;
            foreach (TableInfo ti in UnifiedTables) 
            {
                foreach (FieldInfo fi in ti.Fields) 
                {
                    if (ti.OrdinalInClass == 0 || !fi.IsPrimaryKey) 
                    {
                        UnifiedFields.Add(fi);
                        fi.ClassUnifiedOrdinal = unifiedOrdinal++;
                    }
                }
            }

            if (SubclassSelectorFieldName != null) 
            {
                SubclassSelectorField = FindFieldByName(SubclassSelectorFieldName);
                if (SubclassSelectorField == null)
                    throw new SoodaSchemaException("subclassSelectorField points to invalid field name " + SubclassSelectorFieldName + " in " + Name);
            } 
            else if (InheritFrom != null) 
            {
                throw new SoodaSchemaException("Must use subclassSelectorFieldName when defining inherited class");
            }
            if (SubclassSelectorStringValue != null) 
            {
                // TODO - allow other types based on the field type
                //
                if (SubclassSelectorField == null)
                    throw new SoodaSchemaException("subclassSelectorField is invalid");
                switch (SubclassSelectorField.DataType) 
                {
                    case FieldDataType.Integer:
                        SubclassSelectorValue = Convert.ToInt32(SubclassSelectorStringValue);
                        break;

                    case FieldDataType.String:
                        SubclassSelectorValue = SubclassSelectorStringValue;
                        break;

                    default:
                        throw new SoodaSchemaException("Field data type not supported for subclassSelectorValue: " + SubclassSelectorField.DataType);
                }
            }

            ArrayList pkFields = new ArrayList();

            foreach (FieldInfo fi in UnifiedFields) 
            {
                if (fi.IsPrimaryKey) 
                {
                    pkFields.Add(fi);
                }
            }
            _primaryKeyFields = (FieldInfo[])pkFields.ToArray(typeof(FieldInfo));
        }

        internal void MergeTables() 
        {
            MergedTables = new TableInfoCollection();
            Hashtable mergedTables = new Hashtable();

            foreach (TableInfo table in UnifiedTables) 
            {
                TableInfo mt = (TableInfo)mergedTables[table.DBTableName];
                if (mt == null) 
                {
                    mt = new TableInfo();
                    mt.DBTableName = table.DBTableName;
                    mt.OrdinalInClass = -1;
                    mt.Rehash();
                    mergedTables[table.DBTableName] = mt;
                    MergedTables.Add(mt);
                }

                foreach (FieldInfo fi in table.Fields) 
                {
                    if (mt.ContainsField(fi.Name)) 
                    {
                        if (!fi.IsPrimaryKey)
                            throw new SoodaSchemaException("Duplicate field found for one table!");
                        continue;
                    }

                    mt.Fields.Add(fi);
                }
                mt.Rehash();
            }
        }

        internal void ResolveCollections(SchemaInfo schema) 
        {
            if (CollectionsNtoN != null) 
            {
                foreach (CollectionManyToManyInfo cinfo in CollectionsNtoN) 
                {
                    cinfo.Resolve(schema);
                }
            }

            if (Collections1toN != null) 
            {
                foreach (CollectionOnetoManyInfo cinfo in Collections1toN) 
                {
                    ClassInfo ci = schema.FindClassByName(cinfo.ClassName);
                    if (ci == null)
                        throw new SoodaSchemaException("Collection " + Name + "." + cinfo.Name + " cannot find class " + cinfo.ClassName);

                    FieldInfo fi = ci.FindFieldByName(cinfo.ForeignFieldName);

                    if (fi == null)
                        throw new SoodaSchemaException("Collection " + Name + "." + cinfo.Name + " cannot find field " + cinfo.ClassName + "." + cinfo.ForeignFieldName);

                    fi.AddBackRefCollection(cinfo.Name);
                    cinfo.ForeignField2 = fi;
                    cinfo.Class = ci;
                }
            }
        }

        internal void ResolveReferences(SchemaInfo schema) 
        {
            foreach (FieldInfo fi in UnifiedFields) 
            {
                if (fi.References != null) 
                {
                    ClassInfo ci = schema.FindClassByName(fi.References);
                    fi.ReferencedClass = ci;

                    if (ci != null) 
                    {
                        ci.OuterReferences.Add(fi);
                    } 
                    else 
                    {
                        throw new SoodaSchemaException("Class " + Name + " refers to nonexisting class " + fi.References);
                    }
                }
            }
        }

        internal void ResolvePrecommitValues() 
        {
            foreach (FieldInfo fi in UnifiedFields) 
            {
                string pcv = fi.PrecommitValue;
                if (pcv == null && fi.ReferencedClass != null)
                    pcv = fi.ReferencedClass.DefaultPrecommitValue;

                if (pcv == null)
                {
                     fi.PrecommitTypedValue = Schema.GetDefaultPrecommitValueForDataType(fi.DataType);
                }
                else
                {
                    fi.PrecommitTypedValue = Convert.ChangeType(pcv, FieldDataTypeHelper.GetClrType(fi.DataType));
                }
            }
        }

        public DataSourceInfo GetDataSource() 
        {
            return parentSchema.GetDataSourceInfo(DataSourceName);
        }

        public SchemaInfo Schema
        {
            get 
            {
                return parentSchema;
            }
        }

        public FieldInfoCollection GetAllFields() 
        {
            return UnifiedFields;
        }

        public FieldInfo FindFieldByName(string fieldName) 
        {
            foreach (TableInfo ti in UnifiedTables) 
            {
                FieldInfo fi = ti.FindFieldByName(fieldName);
                if (fi != null)
                    return fi;
            }
            return null;
        }

        public FieldInfo FindFieldByDBName(string fieldName) 
        {
            foreach (TableInfo ti in UnifiedTables) 
            {
                FieldInfo fi = ti.FindFieldByDBName(fieldName);
                if (fi != null)
                    return fi;
            }
            return null;
        }

        public bool ContainsField(string fieldName) 
        {
            return FindFieldByName(fieldName) != null;
        }

        public ClassInfo GetRootClass() 
        {
            if (InheritsFromClass != null)
                return InheritsFromClass.GetRootClass();
            else
                return this;
        }

        public bool IsAbstractClass() 
        {
            return (SubclassSelectorFieldName != null) && (SubclassSelectorValue == null);
        }

        public string GetLabel() 
        {
            if (LabelField != null)
                return LabelField;
            if (InheritsFromClass != null)
                return InheritsFromClass.GetLabel();
            return null;
        }

        public string GetSafeDataSourceName()
        {
            if (DataSourceName == null) return "default";
            return DataSourceName;
        }
    }
}
