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
    public class CodeDomListWrapperGenerator : CodeDomHelpers
    {
        private ClassInfo classInfo;

        public CodeDomListWrapperGenerator(ClassInfo ci)
        {
            this.classInfo = ci;
        }

        public CodeConstructor Constructor()
        {
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Sooda.ObjectMapper.ISoodaObjectList)), "list"));
            //ctor.Statements.Add(
			//	new CodeAssignStatement(TheList(), Arg("list")));
			ctor.BaseConstructorArgs.Add(Arg("list"));
			return ctor;
        }
        public CodeMemberMethod Method_GetSnapshot()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetSnapshot";
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(
                classInfo.Name + "List",
                new CodeMethodInvokeExpression(
                TheList(),
                "GetSnapshot"
                ))));
            return method;
        }
        public CodeMemberProperty Property_Item()
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = "Item";
            prop.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            prop.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "pos"));
            prop.Type = new CodeTypeReference(classInfo.Name);
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            prop.GetStatements.Add(
                new CodeMethodReturnStatement(
                new CodeCastExpression(classInfo.Name, 
                new CodeMethodInvokeExpression(
                TheList(), 
                "GetItem",
                new CodeExpression[] { Arg("pos") }))));
            return prop;
        }

        public CodeMemberMethod Method_Add()
        {
            // public virtual void Add(CLASS_NAME obj) { base.Add(obj); }
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "Add";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Overloaded;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(classInfo.Name, "obj"));
            method.ReturnType = new CodeTypeReference(typeof(int));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(
                TheList(),
                "Add",
                new CodeExpression[] { 
                                         Arg("obj")
                                     }
                )));
            return method;
        }
        public CodeMemberMethod Method_Remove()
        {
            // public virtual void Add(CLASS_NAME obj) { base.Add(obj); }
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "Remove";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Overloaded;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(classInfo.Name, "obj"));
            //method.ReturnType = new CodeTypeReference(typeof(int));
            method.Statements.Add(new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                TheList(),
                "Remove",
                new CodeExpression[] { 
                                         Arg("obj")
                                     }
                )));
            return method;
        }

        public CodeMemberMethod Method_Contains()
        {
            // public virtual void Contains(CLASS_NAME obj) { base.Contains(obj); }
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "Contains";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Overloaded;
            method.ImplementationTypes.Contains(new CodeTypeReference(classInfo + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(classInfo.Name, "obj"));
            method.ReturnType = new CodeTypeReference(typeof(bool));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(
                TheList(),
                "Contains",
                new CodeExpression[] { 
                                         Arg("obj")
                                     }
                )));
            return method;
        }
        public CodeMemberMethod Method_Sort()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "Sort";
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("IComparer", "comp"));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(
                classInfo.Name + "List",
                new CodeMethodInvokeExpression(
                    TheList(),
                    "Sort",
                    Arg("comp")))));
            return method;
        }

        public CodeMemberMethod Method_SelectFirst()
        {
            CodeMemberMethod method;

            // public virtual public CLASS_NAMEList SelectFirst(int count) { return new CLASS_NAMEListSnapshot(this, 0, count); }
            method = new CodeMemberMethod();
            method.Name = "SelectFirst";
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "count"));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(
                classInfo.Name + "List",
                new CodeMethodInvokeExpression(
                TheList(),
                "SelectFirst",
                Arg("count")))));
            return method;
        }

        public CodeMemberMethod Method_SelectLast()
        {
            CodeMemberMethod method;

            // public virtual public CLASS_NAMEList SelectLast(int count) { return new CLASS_NAMEListSnapshot(this, this.Length - count, count); }
            method = new CodeMemberMethod();
            method.Name = "SelectLast";
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "count"));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(
                classInfo.Name + "List",
                new CodeMethodInvokeExpression(
                TheList(),
                "SelectLast",
                Arg("count")))));
            return method;
        }

        public CodeMemberMethod Method_SelectRange()
        {
            CodeMemberMethod method;

            // public virtual public CLASS_NAMEList SelectRange(int from, int to) { return new CLASS_NAMEListSnapshot(this, from, to - from); }
            method = new CodeMemberMethod();
            method.Name = "SelectRange";
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "_from"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "_to"));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(
                classInfo.Name + "List",
                new CodeMethodInvokeExpression(
                TheList(),
                "SelectRange",
                Arg("_from"),
                Arg("_to")
                ))));
            return method;
        }

        public CodeMemberMethod Method_Filter()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "Filter";
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
            method.ImplementationTypes.Add(new CodeTypeReference(classInfo.Name + "List"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaObjectFilter", "f"));
            method.Statements.Add(new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(
                classInfo.Name + "List",
                new CodeMethodInvokeExpression(
                TheList(),
                "Filter",
                Arg("f")
                ))));
            return method;
        }

		public CodeMemberMethod Method_IndexOf()
		{
			CodeMemberMethod method;

			method = new CodeMemberMethod();
			method.Name = "IndexOf";
			method.ReturnType = new CodeTypeReference(typeof(int));
			method.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.New;
			method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaObject", "o"));
			method.Statements.Add(new CodeMethodReturnStatement(
				new CodeObjectCreateExpression(
				classInfo.Name + "List",
				new CodeMethodInvokeExpression(
				TheList(),
				"IndexOf",
				Arg("o")
				))));
			return method;
		}

		public CodeMemberField Field__theList()
		{
			CodeMemberField field = new CodeMemberField(typeof(Sooda.ObjectMapper.ISoodaObjectList), "_theList");
			return field;
		}

		public CodeExpression TheList()
		{
			// return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_theList");
			return new CodeBaseReferenceExpression();
		}
    }
}
