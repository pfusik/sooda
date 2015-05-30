//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2015 Piotr Fusik <piotr@fusik.info>
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
using System.Collections.Generic;

namespace Sooda.Schema
{
    using System.Xml.Serialization;

    [XmlType(Namespace = "http://www.sooda.org/schemas/SoodaSchema.xsd")]
    [Serializable]
    public class RelationInfo : IFieldContainer
    {
        [XmlAttribute("datasource")]
        public string DataSourceName = null;

        private string _name;

        [XmlAttribute("name")]
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

        [XmlElement("table")]
        public TableInfo Table = new TableInfo();

        public RelationInfo() { }

        public ClassInfo GetRef1ClassInfo()
        {
            return Table.Fields[0].ReferencedClass;
        }

        public ClassInfo GetRef2ClassInfo()
        {
            return Table.Fields[1].ReferencedClass;
        }

        [NonSerialized]
        private SchemaInfo parentSchema;

        [XmlIgnore]
        public SchemaInfo Schema
        {
            get
            {
                return parentSchema;
            }
        }

        internal void Resolve(SchemaInfo schemaInfo)
        {
            if (parentSchema != null)
            {
                // already resolved, probably as part of included schema Resolve() process
                return;
            }

            parentSchema = schemaInfo;

            Table.Resolve(this.Name, true);
            Table.Rehash();

            Table.Fields[0].ReferencedClass = schemaInfo.FindClassByName(Table.Fields[0].References);
            if (Table.Fields[0].ReferencedClass == null)
                throw new SoodaSchemaException("Class " + Table.Fields[0].References + " not found in " + this.Name + "." + Table.Fields[0].Name);
            Table.Fields[1].ReferencedClass = schemaInfo.FindClassByName(Table.Fields[1].References);
            if (Table.Fields[1].ReferencedClass == null)
                throw new SoodaSchemaException("Class " + Table.Fields[1].References + " not found in " + this.Name + "." + Table.Fields[1].Name);
            foreach (FieldInfo fi in Table.Fields)
                fi.ParentRelation = this;
        }

        public DataSourceInfo GetDataSource()
        {
            return parentSchema.GetDataSourceInfo(DataSourceName);
        }

        public int ContainsCollection(string name)
        {
            return 0;
        }

        public bool ContainsField(string name)
        {
            return Table.ContainsField(name);
        }

        public FieldInfo FindFieldByName(string name)
        {
            return Table.FindFieldByName(name);
        }

        public List<FieldInfo> GetAllFields()
        {
            return Table.Fields;
        }

    }
}
