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

using System;
using System.Xml;
using System.IO;

namespace Sooda.StubGen
{
    public class NantProjectFileBase : IProjectFile
    {
        protected XmlDocument doc = new XmlDocument();
        protected string templateName;
        protected string elementName;
        protected bool modified = false;
        
        protected NantProjectFileBase(string templateName, string elementName)
        {
            this.templateName = templateName;
            this.elementName = elementName;
        }

        public virtual void CreateNew(string outputNamespace, string assemblyName)
        {
            doc = new XmlDocument();
            using (Stream ins = typeof(StubGen).Assembly.GetManifestResourceStream(templateName))
            {
                doc.Load(ins);
            }
            doc.DocumentElement.SetAttribute("name", outputNamespace);
            doc.DocumentElement.SetAttribute("default", outputNamespace);

            XmlElement el;
            
            el = (XmlElement)doc.SelectSingleNode("project/target[@name='*']");
            el.SetAttribute("name", outputNamespace);

            el = (XmlElement)doc.SelectSingleNode("project/property[@name='.path']");
            el.SetAttribute("name", outputNamespace + ".path");

            el = (XmlElement)doc.SelectSingleNode("project/target/" + elementName);
            el.SetAttribute("output", "${" + outputNamespace + ".path}\\" + assemblyName + ".dll");

            modified = true;
        }
        void IProjectFile.LoadFrom(string fileName)
        {
            doc = new XmlDocument();
            doc.Load(fileName);
            modified = false;
        }
        void IProjectFile.SaveTo(string fileName)
        {
            if (modified)
            {
                doc.Save(fileName);
            }
        }
        void IProjectFile.AddCompileUnit(string relativeFileName)
        {
        }
        void IProjectFile.AddResource(string relativeFileName)
        {
        }
        string IProjectFile.GetProjectFileName(string outNamespace)
        {
            return outNamespace + ".build";
        }
    }
}
