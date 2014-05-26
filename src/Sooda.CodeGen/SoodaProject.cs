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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;

namespace Sooda.CodeGen
{
    public enum PrimitiveRepresentation
    {
        Boxed,
        SqlType,
        Raw,
        Nullable,
        RawWithIsNull
    }

    public class ExternalProjectInfo
    {
        public ExternalProjectInfo()
        {
        }

        public ExternalProjectInfo(string projectType)
        {
            this.ProjectType = projectType;
        }

        public ExternalProjectInfo(string projectType, string projectFile)
        {
            this.ProjectType = projectType;
            this.ProjectFile = projectFile;
        }

        [XmlIgnore]
        public IProjectFile ProjectProvider;

        [XmlIgnore]
        public string ActualProjectFile;

        [XmlAttribute("type")]
        public string ProjectType;

        [XmlAttribute("file")]
        public string ProjectFile;
    }

    [XmlRoot("sooda-project", Namespace = "http://www.sooda.org/schemas/SoodaProject.xsd")]
    public class SoodaProject
    {
        public static string NamespaceURI = "http://www.sooda.org/schemas/SoodaProject.xsd";

        public static Stream GetSoodaProjectXsdStream()
        {
            Assembly ass = typeof(SoodaProject).Assembly;
            foreach (string name in ass.GetManifestResourceNames())
            {
                if (name.EndsWith(".SoodaProject.xsd"))
                {
                    return ass.GetManifestResourceStream(name);
                };
            }
            throw new SoodaSchemaException("SoodaProject.xsd not embedded in Sooda.CodeGen assembly");
        }

        public static XmlReader GetSoodaProjectXsdStreamXmlReader()
        {
            return new XmlTextReader(GetSoodaProjectXsdStream());
        }

        [XmlElement("schema-file")]
        public string SchemaFile;

        [XmlElement("language")]
        public string Language = "c#";

        [XmlElement("output-assembly")]
        public string AssemblyName;

        [XmlElement("output-namespace")]
        public string OutputNamespace;

        [XmlElement("output-path")]
        public string OutputPath;

        [XmlElement("output-partial-path")]
        public string OutputPartialPath;

        [XmlElement("nullable-representation")]
        public PrimitiveRepresentation NullableRepresentation = PrimitiveRepresentation.SqlType;

        [XmlElement("not-null-representation")]
        public PrimitiveRepresentation NotNullRepresentation = PrimitiveRepresentation.Raw;

        [XmlElement("with-indexers")]
        [System.ComponentModel.DefaultValue(true)]
        public bool WithIndexers;

        [XmlElement("null-propagation")]
        [System.ComponentModel.DefaultValue(false)]
        public bool NullPropagation = false;

        [XmlElement("base-class-name")]
        public string BaseClassName = null;

        [XmlElement("with-typed-queries")]
        [System.ComponentModel.DefaultValue(true)]
        public bool WithTypedQueryWrappers = true;

        [XmlElement("file-per-namespace")]
        [System.ComponentModel.DefaultValue(false)]
        public bool FilePerNamespace = false;

        [XmlElement("loader-class")]
        [System.ComponentModel.DefaultValue(false)]
        public bool LoaderClass = false;

        [XmlElement("stubs-compiled-separately")]
        [System.ComponentModel.DefaultValue(false)]
        public bool SeparateStubs = false;

        [XmlElement("embedded-schema-type")]
        public EmbedSchema EmbedSchema = EmbedSchema.Binary;

        [XmlArray("external-projects")]
        [XmlArrayItem("project")]
        public List<ExternalProjectInfo> ExternalProjects = new List<ExternalProjectInfo>();

        [XmlElement("use-partial")]
        [System.ComponentModel.DefaultValue(false)]
        public bool UsePartial = false;

        [XmlElement("partial-suffix")]
        public string PartialSuffix = "";

        public void WriteTo(string fileName)
        {
            using (FileStream fs = File.Create(fileName))
            {
                WriteTo(fs);
            }
        }

        public void WriteTo(Stream stream)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                WriteTo(sw);
            }
        }
        public void WriteTo(TextWriter tw)
        {
            XmlTextWriter xtw = new XmlTextWriter(tw);
            xtw.Indentation = 4;
            xtw.Formatting = Formatting.Indented;
            WriteTo(xtw);
        }
        public void WriteTo(XmlTextWriter xtw)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SoodaProject));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(String.Empty, "http://www.sooda.org/schemas/SoodaProject.xsd");
            ser.Serialize(xtw, this, ns);
            xtw.Flush();
        }

        public static SoodaProject LoadFrom(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                return LoadFrom(fs);
            }
        }

        public static SoodaProject LoadFrom(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                return LoadFrom(sr);
            }
        }

        public static SoodaProject LoadFrom(TextReader reader)
        {
            XmlTextReader xmlreader = new XmlTextReader(reader);
            return LoadFrom(xmlreader);
        }

        public static SoodaProject LoadFrom(XmlTextReader reader)
        {
#if SOODA_NO_VALIDATING_READER
            XmlSerializer ser = new XmlSerializer(typeof(SoodaProject));
            SoodaProject project = (SoodaProject)ser.Deserialize(reader);
#else

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.Schemas.Add(NamespaceURI, GetSoodaProjectXsdStreamXmlReader());
            XmlReader validatingReader = XmlReader.Create(reader, readerSettings);

            XmlSerializer ser = new XmlSerializer(typeof(SoodaProject));
            SoodaProject project = (SoodaProject)ser.Deserialize(validatingReader);
#endif
            return project;
        }
    }
}
