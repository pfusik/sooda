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
using System.Xml.Serialization;
using System.IO;

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;

namespace Sooda.StubGen
{
    public class CodeDomHelpers
    {
        // protec

        //protected public void Disable CodeAttributeDeclarationCollection 
        //    NoStepThrough

        protected void NoStepThrough(CodeAttributeDeclarationCollection attrs)
        {
            attrs.Add(new CodeAttributeDeclaration("System.Diagnostics.DebuggerStepThroughAttribute"));
        }


        protected static CodeFieldReferenceExpression Field(CodeExpression targetObject, string fieldName)
        {
            return new CodeFieldReferenceExpression(targetObject, fieldName);
        }

        protected static CodeThisReferenceExpression This
        {
            get
            {
                return new CodeThisReferenceExpression();
            }
        }

        protected static CodePropertyReferenceExpression ThisProperty(string name)
        {
            return new CodePropertyReferenceExpression(This, name);
        }

        protected static CodeArgumentReferenceExpression Arg(string name)
        {
            return new CodeArgumentReferenceExpression(name);
        }

        protected static CodeMethodReturnStatement Return(CodeExpression ex)
        {
            return new CodeMethodReturnStatement(ex);
        }

        protected static MemberAttributes ParseMemberAttributes(string[] parts, int from, int to)
        {
            MemberAttributes attr = 0;
            
            for (int i = 0; i < parts.Length - 2; ++i)
            {
                switch (parts[i])
                {
                    case "override": 
                        attr = attr | MemberAttributes.Override;
                        break;

                    case "public":
                        attr = attr | MemberAttributes.Public;
                        break;

                    case "internal":
                        attr = attr | MemberAttributes.Assembly;
                        break;

                    case "protected":
                        attr = attr | MemberAttributes.Family;
                        break;

                    case "protected_internal":
                        attr = attr | MemberAttributes.FamilyOrAssembly;
                        break;

                    case "private":
                        attr = attr | MemberAttributes.Private;
                        break;
                };
            }

            return attr;
        }
        public static void SetPropertySignature(CodeMemberProperty prop, string txt)
        {
            string[] parts = txt.Split(' ');

            prop.Attributes = ParseMemberAttributes(parts, 0, parts.Length - 2);
            prop.Type = new CodeTypeReference(parts[parts.Length - 2]);
            prop.Name = parts[parts.Length - 1];
        }
    }
}
