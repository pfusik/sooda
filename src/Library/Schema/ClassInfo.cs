//
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its
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

    /// <summary>
    /// Stores database table schema information
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://sooda.sourceforge.net/schemas/DBSchema.xsd")]
    [Serializable]
    public class ClassInfo : IFieldContainer {
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

        [System.Xml.Serialization.XmlElementAttribute("const")]
        public ConstantInfo[] Constants;

        private string _name = null;

        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name
        {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute("extBaseClassName")]
        public string ExtBaseClassName;

        bool _loadOnDemand = true;

        [System.Xml.Serialization.XmlAttributeAttribute("cached")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Cached = false;

        [System.Xml.Serialization.XmlAttributeAttribute("objectCount")]
        [System.ComponentModel.DefaultValueAttribute(ObjectCount.Medium)]
        public ObjectCount ObjectCount = ObjectCount.Medium;

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

        [System.Xml.Serialization.XmlAttributeAttribute("precacheAll")]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool PrecacheAll = false;

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
            get {
                if (ReadOnly)
                    return false;
                return _loadOnDemand;
            }
            set {
                _loadOnDemand = value;
            }
        }
        public CollectionOnetoManyInfo FindCollectionOneToMany(string collectionName) {
            if (Collections1toN != null) {
                foreach (CollectionOnetoManyInfo i in Collections1toN) {
                    if (collectionName == i.Name)
                        return i;
                }
            }
            return null;
        }

        public CollectionManyToManyInfo FindCollectionManyToMany(string collectionName) {
            if (CollectionsNtoN != null) {
                foreach (CollectionManyToManyInfo i in CollectionsNtoN) {
                    if (collectionName == i.Name)
                        return i;
                }
            }
            return null;
        }

        public int ContainsCollection(string collectionName) {
            if (FindCollectionOneToMany(collectionName) != null)
                return 1;
            if (FindCollectionManyToMany(collectionName) != null)
                return 2;
            return 0;
        }

        FieldInfo _primaryKeyField = null;

        public FieldInfo GetPrimaryKeyField() {
            return _primaryKeyField;
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

        [NonSerialized]
        public ClassInfoCollection Subclasses;

        internal void ResolveInheritance(SchemaInfo schema) {
            if (InheritFrom != null) {
                InheritsFromClass = schema.FindClassByName(InheritFrom);
            } else {
                InheritsFromClass = null;
            }
        }

        internal void CalculateSubclasses() {
            for (ClassInfo ci = InheritsFromClass; ci != null; ci = ci.InheritsFromClass) {
                ci.Subclasses.Add(this);
            }
        }

        internal void FlattenTables() {
            if (LocalTables == null)
                LocalTables = new TableInfoCollection();

            if (InheritsFromClass != null) {
                if (InheritsFromClass.UnifiedTables == null) {
                    InheritsFromClass.FlattenTables();
                }
                UnifiedTables = new TableInfoCollection();
                foreach (TableInfo ti in InheritsFromClass.UnifiedTables) {
                    UnifiedTables.Add(ti.Clone(this));
                }
                foreach (TableInfo ti in LocalTables) {
                    UnifiedTables.Add(ti);
                }
            } else {
                UnifiedTables = LocalTables;
            }

            int ordinalInClass = 0;

            foreach (TableInfo t in UnifiedTables) {
                // Console.WriteLine("Setting OrdinalInClass for {0}.{1} to {2}", Name, t.DBTableName, ordinalInClass);
                t.OrdinalInClass = ordinalInClass++;
                t.NameToken = this.Name + "_" + t.OrdinalInClass;
                t.Rehash();
                t.OwnerClass = this;
                t.Resolve(this.Name);
            }

            if (UnifiedTables.Count > 30) {
                throw new Exception("Class " + Name + " is invalid, because it's base on more than 30 tables. ");
            }
        }

        internal void Resolve(SchemaInfo schema) {
            OuterReferences = new FieldInfoCollection();
            parentSchema = schema;

            // local fields - a sum of all tables local to the class

            LocalFields = new FieldInfoCollection();
            int localOrdinal = 0;
            int count = 0;
            foreach (TableInfo table in UnifiedTables) {
                table.TablePrimaryKeyField = null;
                foreach (FieldInfo fi in table.Fields) {
                    if (fi.IsPrimaryKey) {
                        if (table.TablePrimaryKeyField != null)
                            throw new Exception("Multiple primary keys found in " + table.DBTableName);
                        table.TablePrimaryKeyField = fi;
                    }
                }
            }
            foreach (TableInfo table in LocalTables) {
                foreach (FieldInfo fi in table.Fields) {
                    // add all fields from the root table + all non-key fields
                    // from other tables

                    if (table.OrdinalInClass == 0 || !fi.IsPrimaryKey) {
                        // Console.WriteLine("Adding local field {0} to class {1}", fi.Name, Name);
                        LocalFields.Add(fi);
                        fi.ClassLocalOrdinal = localOrdinal++;
                    }
                }
                count++;
            }

            if (SubclassSelectorFieldName == null && InheritsFromClass != null) {
                for (ClassInfo ci = this; ci != null; ci = ci.InheritsFromClass) {
                    if (ci.SubclassSelectorFieldName != null) {
                        SubclassSelectorFieldName = ci.SubclassSelectorFieldName;
                        break;
                    }
                }
            }

            if (SubclassSelectorFieldName != null) {
                SubclassSelectorField = FindFieldByName(SubclassSelectorFieldName);
            } else if (InheritFrom != null) {
                throw new Exception("Must use subclassSelectorFieldName when defining inherited class");
            }
            if (SubclassSelectorStringValue != null) {
                // TODO - allow other types based on the field type
                //
                switch (SubclassSelectorField.DataType) {
                case FieldDataType.Integer:
                    SubclassSelectorValue = Convert.ToInt32(SubclassSelectorStringValue);
                    break;

                case FieldDataType.String:
                    SubclassSelectorValue = SubclassSelectorStringValue;
                    break;

                default:
                    throw new NotSupportedException("Field data type not supported for subclassSelectorValue: " + SubclassSelectorField.DataType);
                }
            }

            // all inherited fields + local fields

            UnifiedFields = new FieldInfoCollection();

            int unifiedOrdinal = 0;
            foreach (TableInfo ti in UnifiedTables) {
                foreach (FieldInfo fi in ti.Fields) {
                    if (ti.OrdinalInClass == 0 || !fi.IsPrimaryKey) {
                        UnifiedFields.Add(fi);
                        fi.ClassUnifiedOrdinal = unifiedOrdinal++;
                    }
                }
            }

            _primaryKeyField = null;

            foreach (FieldInfo fi in UnifiedFields) {
                if (fi.IsPrimaryKey) {
                    if (_primaryKeyField != null)
                        throw new Exception("Multiple primary keys found in " + Name);
                    _primaryKeyField = fi;
                }
            }
        }

        internal void MergeTables() {
            MergedTables = new TableInfoCollection();
            Hashtable mergedTables = new Hashtable();

            foreach (TableInfo table in UnifiedTables) {
                TableInfo mt = (TableInfo)mergedTables[table.DBTableName];
                if (mt == null) {
                    mt = new TableInfo();
                    mt.DBTableName = table.DBTableName;
                    mt.OrdinalInClass = -1;
                    mt.TablePrimaryKeyField = table.TablePrimaryKeyField;
                    mt.Rehash();
                    mergedTables[table.DBTableName] = mt;
                    MergedTables.Add(mt);
                }

                foreach (FieldInfo fi in table.Fields) {
                    if (mt.ContainsField(fi.Name)) {
                        if (!fi.IsPrimaryKey)
                            throw new Exception("Duplicate field found for one table!");
                        continue;
                    }

                    mt.Fields.Add(fi);
                }
                mt.Rehash();
            }
        }

        internal void ResolveCollections(SchemaInfo schema) {
            if (CollectionsNtoN != null) {
                foreach (CollectionManyToManyInfo cinfo in CollectionsNtoN) {
                    cinfo.Resolve(schema);
                }
            }

            if (Collections1toN != null) {
                foreach (CollectionOnetoManyInfo cinfo in Collections1toN) {
                    ClassInfo ci = schema.FindClassByName(cinfo.ClassName);
                    if (ci == null)
                        throw new Exception("Collection " + Name + "." + cinfo.Name + " cannot find class " + cinfo.ClassName);

                    FieldInfo fi = ci.FindFieldByName(cinfo.ForeignFieldName);

                    if (fi == null)
                        throw new Exception("Collection " + Name + "." + cinfo.Name + " cannot find field " + cinfo.ClassName + "." + cinfo.ForeignFieldName);

                    fi.AddBackRefCollection(cinfo.Name);
                    cinfo.ForeignField2 = fi;
                    cinfo.Class = ci;
                }
            }
        }

        internal void ResolveReferences(SchemaInfo schema) {
            foreach (FieldInfo fi in UnifiedFields) {
                if (fi.References != null) {
                    ClassInfo ci = schema.FindClassByName(fi.References);
                    fi.ReferencedClass = ci;

                    if (ci != null) {
                        if (PrecacheAll && !ci.PrecacheAll) {
                            throw new Exception("Precached class " + Name + " cannot have outer references to non-precached class " + ci.Name);
                        }

                        ci.OuterReferences.Add(fi);
                    } else {
                        throw new Exception("Class " + Name + " refers to nonexisting class " + fi.References);
                    }
                }
            }

            if (PrecacheAll) {
                if (CollectionsNtoN != null) {
                    if (CollectionsNtoN.Length > 0) {
                        throw new Exception("Precached class " + Name + " cannot have outer references via N-N relations.");
                    }
                }

                if (Collections1toN != null) {
                    if (Collections1toN.Length > 0) {
                        throw new Exception("Precached class " + Name + " cannot have outer references via 1-N relations.");
                    }
                }
            }
        }

        public DataSourceInfo GetDataSource() {
            return parentSchema.GetDataSourceInfo(DataSourceName);
        }

        public SchemaInfo Schema
        {
            get {
                return parentSchema;
            }
        }

        public FieldInfoCollection GetAllFields() {
            return UnifiedFields;
        }

        public FieldInfo FindFieldByName(string fieldName) {
            foreach (TableInfo ti in UnifiedTables) {
                FieldInfo fi = ti.FindFieldByName(fieldName);
                if (fi != null)
                    return fi;
            }
            return null;
        }

        public FieldInfo FindFieldByDBName(string fieldName) {
            foreach (TableInfo ti in UnifiedTables) {
                FieldInfo fi = ti.FindFieldByDBName(fieldName);
                if (fi != null)
                    return fi;
            }
            return null;
        }

        public bool ContainsField(string fieldName) {
            return FindFieldByName(fieldName) != null;
        }

        public ClassInfo GetRootClass() {
            if (InheritsFromClass != null)
                return InheritsFromClass.GetRootClass();
            else
                return this;
        }

        public bool IsAbstractClass() {
            return (SubclassSelectorFieldName != null) && (SubclassSelectorValue == null);
        }

        public string GetLabel() {
            if (LabelField != null)
                return LabelField;
            if (InheritsFromClass != null)
                return InheritsFromClass.GetLabel();
            return null;
        }
    }
}
