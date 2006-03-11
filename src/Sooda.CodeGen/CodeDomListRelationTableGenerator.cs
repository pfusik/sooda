//
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace Sooda.CodeGen
{
    public class CodeDomListRelationTableGenerator : CodeDomHelpers
    {
        private RelationInfo relationInfo;
        private SoodaProject options;

        public CodeDomListRelationTableGenerator(RelationInfo ri, SoodaProject options)
        {
            this.relationInfo = ri;
            this.options = options;
        }

        public CodeConstructor Constructor_1()
        {
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.BaseConstructorArgs.Add(new CodePrimitiveExpression(relationInfo.Table.DBTableName));
            ctor.BaseConstructorArgs.Add(new CodePrimitiveExpression(relationInfo.Table.Fields[0].DBColumnName));
            ctor.BaseConstructorArgs.Add(new CodePrimitiveExpression(relationInfo.Table.Fields[1].DBColumnName));
            ctor.BaseConstructorArgs.Add(new CodeMethodInvokeExpression(
                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(options.OutputNamespace + "." + "_DatabaseSchema"), "GetSchema"),
                    "FindClassByName", new CodePrimitiveExpression(relationInfo.Table.Fields[0].References)));

            return ctor;
        }
        public CodeMemberMethod Method_DeserializeTupleLeft()
        {
            // public virtual public CLASS_NAMEList GetSnapshot() { return new CLASS_NAMEListSnapshot(this, 0, Length); }
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "DeserializeTupleLeft";
            method.ReturnType = new CodeTypeReference(typeof(Object));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(XmlReader), "reader"));
            method.Attributes = MemberAttributes.Family | MemberAttributes.Override;

            string typeWrapper = relationInfo.Table.Fields[0].GetFieldHandler().GetType().FullName;

            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(typeWrapper),
                        "DeserializeFromString",
                        new CodeExpression[]
                        {
                            new CodeMethodInvokeExpression(
                                new CodeArgumentReferenceExpression("reader"),
                                "GetAttribute",
                                new CodePrimitiveExpression("r1"))
                        }
                    )));
            return method;
        }
        public CodeMemberMethod Method_DeserializeTupleRight()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "DeserializeTupleRight";
            method.ReturnType = new CodeTypeReference(typeof(Object));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(XmlReader), "reader"));
            method.Attributes = MemberAttributes.Family | MemberAttributes.Override;

            string typeWrapper = relationInfo.Table.Fields[1].GetFieldHandler().GetType().FullName;

            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(typeWrapper),
                        "DeserializeFromString",
                        new CodeExpression[]
                        {
                            new CodeMethodInvokeExpression(
                                new CodeArgumentReferenceExpression("reader"),
                                "GetAttribute",
                                new CodePrimitiveExpression("r2"))
                        }
                    )));
            return method;
        }
    }
}
