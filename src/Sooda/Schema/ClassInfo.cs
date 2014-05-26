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
using System.Collections;
using System.Collections.Generic;

using Sooda.ObjectMapper.FieldHandlers;

namespace Sooda.Schema
{
    /// <summary>
    /// Stores database table schema information
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    [Serializable]
    public class ClassInfo : IFieldContainer
    {
        [XmlAttribute("datasource")]
        public string DataSourceName;

        [XmlElement("description")]
        public string Description;

        [System.Xml.Serialization.XmlElementAttribute("table")]
        public List<TableInfo> LocalTables;

        [System.Xml.Serialization.XmlElementAttribute("collectionOneToMany")]
        public CollectionOnetoManyInfo[] Collections1toN;

        [System.Xml.Serialization.XmlElementAttribute("collectionManyToMany")]
        public CollectionManyToManyInfo[] CollectionsNtoN;

        [XmlIgnore]
        [NonSerialized]
        public List<CollectionBaseInfo> UnifiedCollections = new List<CollectionBaseInfo>();

        [XmlIgnore]
        [NonSerialized]
        public List<CollectionBaseInfo> LocalCollections = new List<CollectionBaseInfo>();

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

        private string[] _orderedFieldNames;

        [System.Xml.Serialization.XmlAttributeAttribute("cached")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Cached = false;

        [System.Xml.Serialization.XmlAttributeAttribute("cacheCollections")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool CacheCollections = false;

        [System.Xml.Serialization.XmlAttributeAttribute("cardinality")]
        [System.ComponentModel.DefaultValueAttribute(ClassCardinality.Medium)]
        public ClassCardinality Cardinality = ClassCardinality.Medium;

        [System.Xml.Serialization.XmlAttributeAttribute("triggers")]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Triggers = true;

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

        [System.Xml.Serialization.XmlAttributeAttribute("keygen")]
        public string KeyGenName;

        [System.Xml.Serialization.XmlAttributeAttribute("ignorePartial")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IgnorePartial = false;

        // array of FieldInfo's that point to this class
        [XmlIgnore()]
        [NonSerialized]
        public List<FieldInfo> OuterReferences;

        [XmlIgnore()]
        [NonSerialized]
        public FieldInfo SubclassSelectorField;

        [XmlIgnore()]
        [NonSerialized]
        public object SubclassSelectorValue;

        [System.Xml.Serialization.XmlAttributeAttribute("disableTypeCache")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool DisableTypeCache = false;

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
        [XmlIgnore]
        public List<FieldInfo> LocalFields;

        [NonSerialized]
        [XmlIgnore]
        public List<FieldInfo> UnifiedFields;

        [NonSerialized]
        [XmlIgnore]
        public List<TableInfo> DatabaseTables;

        [NonSerialized]
        [XmlIgnore]
        public ClassInfo InheritsFromClass;

        [NonSerialized]
        [XmlIgnore]
        public List<TableInfo> UnifiedTables;

        public List<ClassInfo> GetSubclassesForSchema(SchemaInfo schema)
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
                LocalTables = new List<TableInfo>();

            if (InheritsFromClass != null)
            {
                if (InheritsFromClass.UnifiedTables == null || InheritsFromClass.UnifiedTables.Count == 0)
                {
                    InheritsFromClass.FlattenTables();
                }
                UnifiedTables = new List<TableInfo>();
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
                t.Resolve(this.Name, false);
            }

            if (UnifiedTables.Count > 30)
            {
                throw new SoodaSchemaException("Class " + Name + " is invalid, because it's base on more than 30 tables. ");
            }
            // Console.WriteLine("<<< End of FlattenTables for {0}", Name);
        }

        internal void Resolve(SchemaInfo schema)
        {
            if (parentSchema == null)
                parentSchema = schema;

            OuterReferences = new List<FieldInfo>();

            // local fields - a sum of all tables local to the class

            LocalFields = new List<FieldInfo>();
            int localOrdinal = 0;
            int count = 0;
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

            UnifiedCollections = new List<CollectionBaseInfo>();
            LocalCollections = new List<CollectionBaseInfo>();
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

            UnifiedFields = new List<FieldInfo>();

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

            _orderedFieldNames = new string[UnifiedFields.Count];
            for (int i = 0; i < UnifiedFields.Count; ++i)
            {
                _orderedFieldNames[i] = UnifiedFields[i].Name;
            }

            if (SubclassSelectorFieldName != null)
            {
                SubclassSelectorField = FindFieldByName(SubclassSelectorFieldName);
                if (SubclassSelectorField == null)
                    throw new SoodaSchemaException("subclassSelectorField points to invalid field name " + SubclassSelectorFieldName + " in " + Name);
            }
            else if (InheritFrom != null)
            {
                throw new SoodaSchemaException(String.Format("Must use subclassSelectorFieldName when defining inherited class '{0}'", this.Name));
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

            List<FieldInfo> pkFields = new List<FieldInfo>();
            foreach (FieldInfo fi in UnifiedFields)
            {
                if (fi.IsPrimaryKey)
                    pkFields.Add(fi);
            }
            _primaryKeyFields = pkFields.ToArray();
        }

        internal Array MergeArray(Array oldArray, Array merge)
        {
            int oldSize = oldArray.Length;
            int mergeSize = merge.Length;
            int newSize = oldSize + mergeSize;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            System.Array.Copy(oldArray, 0, newArray, 0, oldSize);
            System.Array.Copy(merge, 0, newArray, oldSize, mergeSize);
            return newArray;
        }

        internal void Merge(ClassInfo merge)
        {
            Hashtable mergeNames = new Hashtable();
            foreach (TableInfo mti in this.LocalTables)
                mergeNames.Add(mti.DBTableName, mti);
            foreach (TableInfo ti in merge.LocalTables)
            {
                if (mergeNames.ContainsKey(ti.DBTableName))
                    ((TableInfo)mergeNames[ti.DBTableName]).Merge(ti);
                else
                    LocalTables.Add(ti);
            }
            mergeNames.Clear();

            if (this.Collections1toN != null)
            {
                foreach (CollectionOnetoManyInfo ci in Collections1toN)
                    mergeNames.Add(ci.Name, ci);
            }
            if (this.Collections1toN == null)
                this.Collections1toN = merge.Collections1toN;
            else
            {
                if (merge.Collections1toN != null)
                {
                    foreach (CollectionOnetoManyInfo mci in merge.Collections1toN)
                        if (mergeNames.ContainsKey(mci.Name))
                            throw new SoodaSchemaException(String.Format("Duplicate collection 1:N '{0}' found!", mci.Name));
                    this.Collections1toN = (CollectionOnetoManyInfo[])MergeArray(this.Collections1toN, merge.Collections1toN);
                }
            }
            mergeNames.Clear();

            if (this.CollectionsNtoN != null)
            {
                foreach (CollectionManyToManyInfo ci in CollectionsNtoN)
                    mergeNames.Add(ci.Name, ci);
            }
            if (this.CollectionsNtoN == null)
                this.CollectionsNtoN = merge.CollectionsNtoN;
            else
            {
                if (merge.CollectionsNtoN != null)
                {
                    foreach (CollectionManyToManyInfo mci in merge.CollectionsNtoN)
                        if (mergeNames.ContainsKey(mci.Name))
                            throw new SoodaSchemaException(String.Format("Duplicate collection N:N '{0}' found!", mci.Name));
                    this.CollectionsNtoN = (CollectionManyToManyInfo[])MergeArray(this.CollectionsNtoN, merge.CollectionsNtoN);
                }
            }
            mergeNames.Clear();

            if (this.Constants != null)
            {
                foreach (ConstantInfo ci in Constants)
                    mergeNames.Add(ci.Name, ci);
            }
            if (this.Constants == null)
                this.Constants = merge.Constants;
            else
            {
                if (merge.Constants != null)
                {
                    foreach (ConstantInfo mci in merge.Constants)
                        if (mergeNames.ContainsKey(mci.Name))
                            throw new SoodaSchemaException(String.Format("Duplicate constant name '{0}' found!", mci.Name));
                    this.Constants = (ConstantInfo[])MergeArray(this.Constants, merge.Constants);
                }
            }
        }

        internal void MergeTables()
        {
            DatabaseTables = new List<TableInfo>();
            Dictionary<string, TableInfo> mergedTables = new Dictionary<string, TableInfo>();

            foreach (TableInfo table in UnifiedTables)
            {
                TableInfo mt;
                if (!mergedTables.TryGetValue(table.DBTableName, out mt))
                {
                    mt = new TableInfo();
                    mt.DBTableName = table.DBTableName;
                    mt.OrdinalInClass = -1;
                    mt.Rehash();
                    mergedTables[table.DBTableName] = mt;
                    DatabaseTables.Add(mt);
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

                    schema.AddBackRefCollection(fi, cinfo.Name);
                    cinfo.ForeignField2 = fi;
                    cinfo.Class = ci;
                }
            }
        }

        internal void ResolveReferences(SchemaInfo schema)
        {
            // logger.Debug("ResolveReferences({0})", this.Name);
            foreach (FieldInfo fi in UnifiedFields)
            {
                // logger.Debug("unifiedField: {0}", fi.Name);
                if (fi.References != null)
                {
                    ClassInfo ci = schema.FindClassByName(fi.References);
                    if (ci == null)
                        throw new SoodaSchemaException("Class " + fi.References + " not found.");
                    fi.ReferencedClass = ci;
                }
            }

            foreach (FieldInfo fi in LocalFields)
            {
                fi.ParentClass = this;
                // logger.Debug("localField: {0}", fi.Name);
                if (fi.ReferencedClass != null)
                {
                    // logger.Debug("Is a reference to {0} with ondelete = {1}", fi.ReferencedClass.Name, fi.DeleteAction);
                    fi.ReferencedClass.OuterReferences.Add(fi);
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
                    fi.PrecommitTypedValue = FieldHandlerFactory.GetFieldHandler(fi.DataType).RawDeserialize(pcv);
                }
            }
        }

        public DataSourceInfo GetDataSource()
        {
            return parentSchema.GetDataSourceInfo(DataSourceName);
        }

        [XmlIgnore()]
        public SchemaInfo Schema
        {
            get
            {
                return parentSchema;
            }
            set
            {
                parentSchema = value;
            }
        }

        public List<FieldInfo> GetAllFields()
        {
            return UnifiedFields;
        }

        public string[] OrderedFieldNames
        {
            get { return _orderedFieldNames; }
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
