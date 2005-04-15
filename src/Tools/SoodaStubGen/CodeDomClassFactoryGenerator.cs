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

using Sooda.StubGen.CDIL;

namespace Sooda.StubGen {
    public class CodeDomClassFactoryGenerator : CodeDomHelpers {
        private ClassInfo classInfo;
        private string outNamespace;

        public CodeDomClassFactoryGenerator(ClassInfo ci, string outNamespace) {
            this.classInfo = ci;
            this.outNamespace = outNamespace;
        }
        public CodeMemberMethod Method_CreateNew() {
            CodeMemberMethod method = (CodeMemberMethod)CDILParser.ParseMember(@"
method CreateNew(SoodaTransaction tran)
returns SoodaObject
attributes Private
implementsprivate ISoodaObjectFactory
begin
end", classInfo.Name);

            if (classInfo.IsAbstractClass()) {
                method.Statements.Add(CDILParser.ParseStatement("throw new NotSupportedException('Cannot create instances of abstract class {0}')", classInfo.Name));
            } else {
                method.Statements.Add(CDILParser.ParseStatement("return new {0}(arg(tran))", outNamespace + "." + classInfo.Name));
            }
            return method;
        }

        public CodeMemberMethod Method_Get() {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "GetRef";
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "keyValue"));
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.ReturnType = new CodeTypeReference("SoodaObject");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(outNamespace + ".Stubs." + classInfo.Name + "_Stub"),
                        "GetRef",
                        new CodeExpression[] {
                            Arg("tran"),
                            new CodeCastExpression( FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).Name, Arg("keyValue"))
                        })));
            return method;
        }

        public CodeMemberMethod Method_TryGet() {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "TryGet";
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("System.Object", "keyValue"));
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.ReturnType = new CodeTypeReference("SoodaObject");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(outNamespace + ".Stubs." + classInfo.Name + "_Stub"),
                        "TryGet",
                        new CodeExpression[] {
                            Arg("tran"),
                            new CodeCastExpression( FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).Name, Arg("keyValue"))
                        })));
            return method;
        }

        public CodeMemberMethod Method_GetRefFromRecord() {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "GetRefFromRecord";
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("System.Data.IDataRecord", "record"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "firstColumnIndex"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(new CodeTypeReference(typeof(TableInfo)), 1), "loadedTables"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "tableIndex"));
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.ReturnType = new CodeTypeReference("SoodaObject");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(outNamespace + ".Stubs." + classInfo.Name + "_Stub"),
                        "GetRefFromRecord",
                        new CodeExpression[] {
                            Arg("tran"),
                            Arg("record"),
                            Arg("firstColumnIndex"),
                            Arg("loadedTables"),
                            Arg("tableIndex")
                        })));
            return method;
        }
        public CodeMemberMethod Method_GetList() {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "GetList";
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaWhereClause", "whereClause"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaOrderBy", "orderByClause"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaSnapshotOptions", "options"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name), "GetList"),
                                                   Arg("tran"),
                                                   Arg("whereClause"),
                                                   Arg("orderByClause"),
                                                   Arg("options"))));

            method.ReturnType = new CodeTypeReference(typeof(System.Collections.IList));
            return method;
        }
        public CodeMemberMethod Method_GetPrimaryKeyFieldHandler() {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "GetPrimaryKeyFieldHandler";
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.ReturnType = new CodeTypeReference("SoodaFieldHandler");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(null, "_primaryKeyFieldHandler")));
            return method;
        }
        public CodeMemberMethod Method_GetClassInfo() {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "GetClassInfo";
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.ReturnType = new CodeTypeReference("Sooda.Schema.ClassInfo");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodePropertyReferenceExpression(null, "TheClassInfo")));
            return method;
        }
        public CodeMemberMethod Method_GetRawObject() {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private;
            method.Name = "GetRawObject";
            method.ImplementationTypes.Add(new CodeTypeReference("ISoodaObjectFactory"));
            method.PrivateImplementationType = new CodeTypeReference("ISoodaObjectFactory");
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.ReturnType = new CodeTypeReference("SoodaObject");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("SoodaObject"),
                        "GetRawObjectHelper",
                        new CodeFieldReferenceExpression(null, "_theType"),
                        new CodeArgumentReferenceExpression("tran")
                    )));
            return method;
        }

        public CodeMemberField Field__theFactory() {
            CodeMemberField _theFactoryField = new CodeMemberField(classInfo.Name + "_Factory", "_theFactory");
            _theFactoryField.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            _theFactoryField.InitExpression = new CodeObjectCreateExpression(classInfo.Name + "_Factory");
            return _theFactoryField;
        }

        public CodeMemberField Field__theType() {
            CodeMemberField _theFactoryField = new CodeMemberField(typeof(Type), "_theType");
            _theFactoryField.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            _theFactoryField.InitExpression = new CodeTypeOfExpression(classInfo.Name);
            return _theFactoryField;
        }

        public CodeConstructor Constructor() {
            CodeConstructor cc = new CodeConstructor();

            return cc;
        }
        public CodeMemberProperty Property_TheFactory() {
            CodeMemberProperty method = new CodeMemberProperty();

            method.Name = "TheFactory";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.Type = new CodeTypeReference(classInfo.Name + "_Factory");

            method.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_theFactory")));
            return method;
        }
        public CodeMemberField Field__theClassInfo() {
            CodeMemberField field;

            field = new CodeMemberField("Sooda.Schema.ClassInfo", "_theClassInfo");
            field.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
            field.InitExpression =
                new CodeMethodInvokeExpression(
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("_DatabaseSchema"), "GetSchema"), "FindClassByName",
                    new CodePrimitiveExpression(classInfo.Name));
            return field;
        }
        public CodeMemberField Field__primaryKeyFieldHandler() {
            string typeWrapper = classInfo.GetPrimaryKeyField().GetWrapperTypeName();
            CodeMemberField field;

            field = new CodeMemberField("Sooda.ObjectMapper.SoodaFieldHandler", "_primaryKeyFieldHandler");
            field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            field.InitExpression =
                new CodeObjectCreateExpression(typeWrapper, new CodePrimitiveExpression(false));
            return field;
        }
        public CodeMemberProperty Property_TheClassInfo() {
            CodeMemberProperty method = new CodeMemberProperty();

            method.Name = "TheClassInfo";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.Type = new CodeTypeReference("Sooda.Schema.ClassInfo");

            method.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_theClassInfo")));
            return method;
        }

        public CodeMemberProperty Property_TheType() {
            CodeMemberProperty method = new CodeMemberProperty();

            method.Name = "TheType";
            method.Attributes = MemberAttributes.Public;
            method.Type = new CodeTypeReference(typeof(Type));

            method.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_theType")));
            return method;
        }


    }
}
