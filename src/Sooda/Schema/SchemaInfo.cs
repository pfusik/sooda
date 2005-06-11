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

[assembly: System.CLSCompliant(true)]

namespace Sooda.Schema {
    using System;
    using System.Collections;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using System.Text;

    [XmlType(Namespace = "http://sooda.sourceforge.net/schemas/DBSchema.xsd")]
    [XmlRoot("schema", Namespace = "http://sooda.sourceforge.net/schemas/DBSchema.xsd", IsNullable = false)]
    [Serializable]
    public class SchemaInfo {
        [XmlElement("datasource", typeof(DataSourceInfo))]
        public DataSourceInfoCollection DataSources = new DataSourceInfoCollection();

        [XmlElement("class", typeof(ClassInfo))]
        public ClassInfoCollection Classes = new ClassInfoCollection();

        [System.Xml.Serialization.XmlAnyAttribute()]
        [NonSerialized]
        public System.Xml.XmlAttribute[] Extensions;

        [XmlElement("relation", typeof(RelationInfo))]
        public RelationInfoCollection Relations = new RelationInfoCollection();

        public bool Contains(string className) {
            return FindClassByName(className) != null;
        }

        public ClassInfo FindClassByName(string className) {
            if (Classes == null)
                return null;

            return (ClassInfo)classNameHash[className];
        }

        public RelationInfo FindRelationByName(string relationName) {
            if (Relations == null)
                return null;

            return (RelationInfo)relationNameHash[relationName];
        }

        public void AddClass(ClassInfo ci) {
            Classes.Add(ci);
            Rehash();
        }

        [NonSerialized]
        private Hashtable classNameHash;
        [NonSerialized]
        private Hashtable relationNameHash;

        public void Resolve() {
            classNameHash = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());
            relationNameHash = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());

            Rehash();
            foreach (ClassInfo ci in Classes) {
                ci.Subclasses = new ClassInfoCollection();
            }
            foreach (ClassInfo ci in Classes) {
                ci.ResolveInheritance(this);
            }
            foreach (ClassInfo ci in Classes) {
                ci.CalculateSubclasses();
            }
            foreach (ClassInfo ci in Classes) {
                ci.FlattenTables();
            }
            foreach (ClassInfo ci in Classes) {
                ci.Resolve(this);
            }
            foreach (ClassInfo ci in Classes) {
                ci.MergeTables();
            }
            foreach (ClassInfo ci in Classes) {
                ci.ResolveReferences(this);
            }
            foreach (ClassInfo ci in Classes) {
                ci.ResolveCollections(this);
            }

            if (Relations != null) {
                foreach (RelationInfo ri in Relations) {
                    ri.Resolve(this);
                }
            }

            foreach (DataSourceInfo dsi in DataSources) {
                dsi.Resolve();
            };
        }

        private void Rehash() {
            classNameHash.Clear();
            relationNameHash.Clear();
            if (Classes != null) {
                foreach (ClassInfo ci in Classes) {
                    if (ci.Name != null)
                        classNameHash[ci.Name] = ci;
                }
            }
            if (Relations != null) {
                foreach (RelationInfo ri in Relations) {
                    relationNameHash[ri.Name] = ri;
                }
            }
        }

        public DataSourceInfo GetDataSourceInfo(string name) {
            if (name == null)
                name = "default";

            foreach (DataSourceInfo dsi in DataSources) {
                if (dsi.Name == name)
                    return dsi;
            }
            throw new Exception("Data source " + name + " not found. Available data sources: " + GetAvailableDataSources());
        }

        private string GetAvailableDataSources()
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataSourceInfo dsi in DataSources)
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(dsi.Name + ": " + dsi.DataSourceType);
            }   

            return sb.ToString();
        }
    }
}
