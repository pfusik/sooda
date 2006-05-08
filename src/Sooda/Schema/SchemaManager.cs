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

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace Sooda.Schema
{
    public sealed class SchemaManager
    {
        private SchemaManager() { }

        public static string NamespaceURI
        {
            get
            {
                return "http://www.sooda.org/schemas/SoodaSchema.xsd";
            }
        }

        public static Stream GetSchemaXsdStream()
        {
            Assembly ass = typeof(SchemaManager).Assembly;
            foreach (string name in ass.GetManifestResourceNames())
            {
                if (name.EndsWith(".SoodaSchema.xsd"))
                {
                    return ass.GetManifestResourceStream(name);
                };
            }
            throw new SoodaSchemaException("SoodaSchema not embedded in Sooda assembly");
        }

        public static XmlReader GetSchemaXsdStreamXmlReader()
        {
            return new XmlTextReader(GetSchemaXsdStream());
        }

        public static void ProcessIncludes(SchemaInfo resultSchema, IncludeInfoCollection includes, string baseDirectoryForIncludes)
        {
            if (includes == null)
                return;

            if (includes.Count == 0)
                return;

            foreach (IncludeInfo ii in includes)
            {
                string includeFileName = Path.Combine(baseDirectoryForIncludes, ii.SchemaFile);
                SchemaInfo includedSchema = ReadAndValidateSchema(new XmlTextReader(includeFileName), Path.GetDirectoryName(includeFileName));

                resultSchema.MergeIncludedSchema(includedSchema);
                ii.Schema = includedSchema;
            }
        }

        public static SchemaInfo ReadAndValidateSchema(XmlReader reader, string baseDirectoryForIncludes)
        {
#if SOODA_NO_VALIDATING_READER
            XmlSerializer ser = new XmlSerializer(typeof(Sooda.Schema.SchemaInfo));
            SchemaInfo schemaInfo = (SchemaInfo)ser.Deserialize(reader);
#else

#if DOTNET2
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.Schemas.Add(NamespaceURI, GetSchemaXsdStreamXmlReader());

            XmlReader validatingReader = XmlReader.Create(reader, readerSettings);
#else
            XmlValidatingReader validatingReader = new XmlValidatingReader(reader);
            validatingReader.Schemas.Add(NamespaceURI, GetSchemaXsdStreamXmlReader());
#endif

            XmlSerializer ser = new XmlSerializer(typeof(Sooda.Schema.SchemaInfo));
            SchemaInfo schemaInfo = (SchemaInfo)ser.Deserialize(validatingReader);
#endif

            if (baseDirectoryForIncludes != null)
            {
                ProcessIncludes(schemaInfo, schemaInfo.Includes, baseDirectoryForIncludes);
            }

            schemaInfo.Resolve();
            return schemaInfo;
        }
    }
}
