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
using System.Collections;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Text;

using Sooda.ObjectMapper.FieldHandlers;

[assembly: System.CLSCompliant(true)]

namespace Sooda.Schema
{
    [XmlType(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    [XmlRoot("schema", Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd", IsNullable = false)]
    [Serializable]
    public class SchemaInfo
    {
        public static readonly string XmlNamespace = "http://www.sooda.org/schemas/SoodaSchema.xsd";

        [XmlElement("namespace")]
        public string Namespace;

        [XmlElement("assembly")]
        public string AssemblyName;

        [XmlElement("include", typeof(IncludeInfo))]
        public IncludeInfoCollection Includes = new IncludeInfoCollection();

        [XmlElement("datasource", typeof(DataSourceInfo))]
        public DataSourceInfoCollection DataSources = new DataSourceInfoCollection();

        [XmlElement("class", typeof(ClassInfo))]
        public ClassInfoCollection Classes = new ClassInfoCollection();

        [XmlElement("defaultPrecommitValues")]
        [System.ComponentModel.DefaultValue(true)]
        public bool DefaultPrecommitValues = true;

        [XmlElement("precommitValue", typeof(PrecommitValueInfo))]
        public PrecommitValueInfoCollection PrecommitValues = new PrecommitValueInfoCollection();

        [XmlIgnore]
        public ClassInfoCollection LocalClasses;

        [XmlIgnore]
        public RelationInfoCollection LocalRelations;

        [System.Xml.Serialization.XmlAnyAttribute()]
        [NonSerialized]
        public System.Xml.XmlAttribute[] Extensions;

        [XmlElement("relation", typeof(RelationInfo))]
        public RelationInfoCollection Relations = new RelationInfoCollection();

        [XmlIgnore]
        [NonSerialized]
        private Hashtable _subclasses;

        [XmlIgnore]
        [NonSerialized]
        private Hashtable _backRefCollections;

        public bool Contains(string className)
        {
            return FindClassByName(className) != null;
        }

        public ClassInfo FindClassByName(string className)
        {
            if (Classes == null)
                return null;

            return (ClassInfo)classNameHash[className];
        }

        public RelationInfo FindRelationByName(string relationName)
        {
            if (Relations == null)
                return null;

            return (RelationInfo)relationNameHash[relationName];
        }

        public void AddClass(ClassInfo ci)
        {
            Classes.Add(ci);
            Rehash();
        }

        [NonSerialized]
        private Hashtable classNameHash;
        [NonSerialized]
        private Hashtable relationNameHash;

        public void Include(SchemaInfo schema)
        {
        }

        public void Resolve()
        {
            if (Includes == null)
                Includes = new IncludeInfoCollection();

#if DOTNET2
            classNameHash = new Hashtable(StringComparer.OrdinalIgnoreCase);
            relationNameHash = new Hashtable(StringComparer.OrdinalIgnoreCase);
#else
            classNameHash = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());
            relationNameHash = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());
#endif
            Rehash();

            _backRefCollections = new Hashtable();

            foreach (ClassInfo ci in Classes)
            {
                ci.ResolveInheritance(this);
            }
            foreach (ClassInfo ci in Classes)
            {
                ci.FlattenTables();
            }
            foreach (ClassInfo ci in Classes)
            {
                ci.Resolve(this);
            }
            foreach (ClassInfo ci in Classes)
            {
                ci.MergeTables();
            }
            foreach (ClassInfo ci in Classes)
            {
                ci.ResolveReferences(this);
            }
            foreach (ClassInfo ci in Classes)
            {
                ci.ResolveCollections(this);
            }

            foreach (ClassInfo ci in Classes)
            {
                ci.ResolvePrecommitValues();
            }

            if (Relations != null)
            {
                foreach (RelationInfo ri in Relations)
                {
                    ri.Resolve(this);
                }
            }

            foreach (DataSourceInfo dsi in DataSources)
            {
                dsi.Resolve();
            };


            LocalClasses = new ClassInfoCollection();
            foreach (ClassInfo ci in Classes)
            {
                if (ci.Schema == this)
                    LocalClasses.Add(ci);
            }
            LocalRelations = new RelationInfoCollection();
            foreach (RelationInfo ri in Relations)
            {
                if (ri.Schema == this)
                    LocalRelations.Add(ri);
            }

            _subclasses = new Hashtable();

            foreach (ClassInfo ci in Classes)
            {
                _subclasses[ci.Name] = new ClassInfoCollection();
            }

            foreach (ClassInfo ci0 in Classes)
            {
                for (ClassInfo ci = ci0.InheritsFromClass; ci != null; ci = ci.InheritsFromClass)
                {
                    ((ClassInfoCollection)_subclasses[ci.Name]).Add(ci0);
                }
            }
        }

        internal ClassInfoCollection GetSubclasses(ClassInfo ci)
        {
            return (ClassInfoCollection)_subclasses[ci.Name];
        }

        private void Rehash()
        {
            classNameHash.Clear();
            relationNameHash.Clear();
            if (Classes != null)
            {
                foreach (ClassInfo ci in Classes)
                {
                    if (ci.Name != null)
                        classNameHash[ci.Name] = ci;
                }
            }
            if (Relations != null)
            {
                foreach (RelationInfo ri in Relations)
                {
                    relationNameHash[ri.Name] = ri;
                }
            }
        }

        public DataSourceInfo GetDataSourceInfo(string name)
        {
            if (name == null)
                name = "default";

            foreach (DataSourceInfo dsi in DataSources)
            {
                if (dsi.Name == name)
                    return dsi;
            }
            throw new SoodaSchemaException("Data source " + name + " not found. Available data sources: " + GetAvailableDataSources());
        }

        private string GetAvailableDataSources()
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataSourceInfo dsi in DataSources)
            {
                if (sb.Length > 0)
                    sb.Append(',');
                sb.Append(dsi.Name + ": " + dsi.DataSourceType);
            }

            return sb.ToString();
        }

        internal void MergeIncludedSchema(SchemaInfo includedSchema)
        {
            // merge classes, relations and datasources

            if (includedSchema.Classes != null)
            {
                Hashtable classNames = new Hashtable();
                foreach (ClassInfo nci in includedSchema.Classes)
                    classNames.Add(nci.Name, nci);

                ClassInfoCollection newClasses = new ClassInfoCollection();
                if (this.Classes != null)
                {
                    foreach (ClassInfo ci in this.Classes)
                    {
                        newClasses.Add(ci);
                        if (classNames.ContainsKey(ci.Name))
                        {
                            ci.Merge((ClassInfo)classNames[ci.Name]);
                            classNames.Remove(ci.Name);
                        }
                    }
                }

                foreach (ClassInfo ci in classNames.Values)
                {
                    newClasses.Add(ci);
                }

                this.Classes = newClasses;
            }

            if (includedSchema.Relations != null)
            {
                RelationInfoCollection newRelations = new RelationInfoCollection();

                foreach (RelationInfo ci in includedSchema.Relations)
                {
                    newRelations.Add(ci);
                }

                if (this.Relations != null)
                {
                    foreach (RelationInfo ci in this.Relations)
                    {
                        newRelations.Add(ci);
                    }
                }

                this.Relations = newRelations;
            }

            if (includedSchema.DataSources != null)
            {
                DataSourceInfoCollection newDataSources = new DataSourceInfoCollection();

                foreach (DataSourceInfo ci in includedSchema.DataSources)
                {
                    newDataSources.Add(ci);
                }

                if (this.DataSources != null)
                {
                    foreach (DataSourceInfo ci in this.DataSources)
                    {
                        newDataSources.Add(ci);
                    }
                }

                this.DataSources = newDataSources;
            }
        }

        public object GetDefaultPrecommitValueForDataType(FieldDataType dataType)
        {
            if (PrecommitValues != null)
            {
                for (int i = 0; i < PrecommitValues.Count; ++i)
                    if (dataType == PrecommitValues[i].DataType)
                        return PrecommitValues[i].Value;
            }
            if (DefaultPrecommitValues)
            {
                return FieldHandlerFactory.GetFieldHandler(dataType).DefaultPrecommitValue();
            }
            return null;
        }

        public StringCollection GetBackRefCollections(FieldInfo fi)
        {
            return (StringCollection)_backRefCollections[fi.NameTag];
        }

        internal void AddBackRefCollection(FieldInfo fi, string name)
        {
            StringCollection sc = GetBackRefCollections(fi);
            if (sc == null)
            {
                sc = new StringCollection();
                _backRefCollections[fi.NameTag] = sc;
            }
            sc.Add(name);
        }
    }
}
