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

using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;

using Sooda.Schema;

namespace Sooda.StubGen
{
    public class CodeDomDatabaseSchemaSourceGenerator : CodeDomHelpers
    {
        private static string AllocID(Hashtable idTable, Type t)
        {
            if (!idTable.Contains(t))
            {
                idTable[t] = (int)0;
            }

            int v = (int)idTable[t] + 1;
            idTable[t] = v;

            if (v == 1)
                return "_cur" + t.Name;
            else
                return "_cur" + t.Name + v.ToString();
        }

        private static void FreeID(Hashtable idTable, Type t)
        {
            int v = (int)idTable[t] - 1;
            idTable[t] = v;
        }

        private static void CopyPublic(CodeTypeConstructor ctc, string varname, object o, int pass, Hashtable idTable)
        {
            Type t = o.GetType();
            object[] noIndexArray = new object[0];

            foreach (MemberInfo mi in t.GetMembers())
            {
                object val;
                Type mt;

                if (mi is PropertyInfo)
                {
                    PropertyInfo pi = (PropertyInfo)mi;
                    if (pi.GetGetMethod().GetParameters().Length == 0)
                    {
                        val = pi.GetValue(o, noIndexArray);
                    }
                    else
                    {
                        Console.WriteLine("WARNING: " + pi.DeclaringType + " " + pi.Name + " not supported");
                        continue;
                    }
                    mt = pi.PropertyType;
                    if (!pi.CanWrite)
                        continue;
                }
                else if (mi is System.Reflection.FieldInfo)
                {
                    System.Reflection.FieldInfo fi = (System.Reflection.FieldInfo)mi;

                    val = fi.GetValue(o);
                    mt = fi.FieldType;
                }
                else
                    continue;

                if (mi.IsDefined(typeof(XmlIgnoreAttribute), false))
                    continue;

                if (mt.IsValueType || mt == typeof(string) || val == null)
                {
                    if (pass == 1)
                    {
                        if (mt.IsEnum)
                        {
                            ctc.Statements.Add(
                                new CodeAssignStatement(
                                new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                                new CodeSnippetExpression(mt.FullName + "." + val.ToString())));
                        }
                        else
                        {
                            ctc.Statements.Add(
                                new CodeAssignStatement(
                                new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                                new CodePrimitiveExpression(val)));
                        }
                    }
                }
                else if (mt.IsArray)
                {
                    if (pass == 1)
                    {
                        Array a = (Array)val;
                        int pos = 0;

                        ctc.Statements.Add(
                            new CodeAssignStatement(
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                            new CodeArrayCreateExpression(mt.GetElementType(), new CodePrimitiveExpression(a.Length))));

                        ctc.Statements.Add(new CodeCommentStatement(""));

                        foreach (object item in a)
                        {
                            string tmpvar = AllocID(idTable, item.GetType());

                            ctc.Statements.Add(
                                new CodeAssignStatement(
                                new CodeVariableReferenceExpression(tmpvar),
                                new CodeObjectCreateExpression(item.GetType())));

                            CopyPublic(ctc, tmpvar, item, 1, idTable);
                            ctc.Statements.Add(
                                new CodeAssignStatement(
                                new CodeArrayIndexerExpression(
                                    new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                                    new CodePrimitiveExpression(pos)
                                ),
                                new CodeVariableReferenceExpression(tmpvar)
                                ));
                            CopyPublic(ctc, tmpvar, item, 2, idTable);
                            ctc.Statements.Add(new CodeCommentStatement("---"));
                            FreeID(idTable, item.GetType());
                            pos++;
                        }
                    }
                }
                else if (mt.GetInterface("System.Collections.IList") != null)
                {
                    if (pass == 1)
                    {
                        ctc.Statements.Add(
                            new CodeAssignStatement(
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                            new CodeObjectCreateExpression(mt)));
                    }
                    else
                    {
                        IList alist = (IList)val;

                        foreach (object item in alist)
                        {
                            string tmpvar = AllocID(idTable, item.GetType());

                            ctc.Statements.Add(
                                new CodeAssignStatement(
                                new CodeVariableReferenceExpression(tmpvar),
                                new CodeObjectCreateExpression(item.GetType())));

                            CopyPublic(ctc, tmpvar, item, 1, idTable);
                            ctc.Statements.Add(
                                new CodeMethodInvokeExpression(
                                new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                                "Add",
                                new CodeVariableReferenceExpression(tmpvar)));
                            CopyPublic(ctc, tmpvar, item, 2, idTable);
                            ctc.Statements.Add(new CodeCommentStatement("---"));
                            FreeID(idTable, item.GetType());
                        }
                    }
                }
                else
                {
                    string tmpvar = AllocID(idTable, mt);

                    ctc.Statements.Add(
                        new CodeAssignStatement(
                        new CodeVariableReferenceExpression(tmpvar),
                        new CodeObjectCreateExpression(mt)));

                    CopyPublic(ctc, tmpvar, val, 1, idTable);
                    ctc.Statements.Add(
                        new CodeAssignStatement(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(varname), mi.Name),
                        new CodeVariableReferenceExpression(tmpvar)));
                    CopyPublic(ctc, tmpvar, val, 2, idTable);
                    ctc.Statements.Add(new CodeCommentStatement("---"));
                    FreeID(idTable, mt);

                    // ctc.Statements.Add(new CodeCommentStatement("*** " + mi.Name + " - " + mt));
                }
            }
        }
        public static void GenerateTypeCreator(CodeTypeDeclaration ctd, Type t)
        {
            CodeMemberMethod method = new CodeMemberMethod();

            method.Name = "_create" + t.Name;
            method.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            method.ReturnType = new CodeTypeReference(t);
            method.Statements.Add(
                new CodeVariableDeclarationStatement(t, "retVal", new CodeObjectCreateExpression(t)));
            foreach (MemberInfo mi in t.GetMembers())
            {
                Type mt;

                if (mi is PropertyInfo)
                {
                    PropertyInfo pi = (PropertyInfo)mi;
                    mt = pi.PropertyType;
                    if (!pi.CanWrite)
                        continue;
                }
                else if (mi is System.Reflection.FieldInfo)
                {
                    System.Reflection.FieldInfo fi = (System.Reflection.FieldInfo)mi;

                    mt = fi.FieldType;
                }
                else
                    continue;

                if (mi.IsDefined(typeof(XmlIgnoreAttribute), false))
                    continue;

                method.Parameters.Add(
                    new CodeParameterDeclarationExpression(
                    mt, "_" + mi.Name));
                method.Statements.Add(
                    new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeVariableReferenceExpression("retVal"), 
                        mi.Name),
                    new CodeArgumentReferenceExpression("_" + mi.Name)));
            }
            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeVariableReferenceExpression("retVal")));
            ctd.Members.Add(method);
        }

        public static void GenHelpers(CodeTypeDeclaration ctd, SchemaInfo schema)
        {
            Hashtable generatedTypes = new Hashtable();

            GenerateTypeCreator(ctd, typeof(Sooda.Schema.SchemaInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.ClassInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.RelationInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.FieldInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.DataSourceInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.ConstantInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.CollectionOnetoManyInfo));
            GenerateTypeCreator(ctd, typeof(Sooda.Schema.CollectionManyToManyInfo));
        }

        public static CodeTypeConstructor Constructor_Class(SchemaInfo schema)
        {
            CodeTypeConstructor ctc = new CodeTypeConstructor();

            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.SchemaInfo", "_curSchemaInfo", new CodeFieldReferenceExpression(null, "theSchema")));

            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.ClassInfo", "_curClassInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.RelationInfo", "_curRelationInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.FieldInfo", "_curFieldInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.DataSourceInfo", "_curDataSourceInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.ConstantInfo", "_curConstantInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.CollectionOnetoManyInfo", "_curCollectionOnetoManyInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("Sooda.Schema.CollectionManyToManyInfo", "_curCollectionManyToManyInfo"));
            ctc.Statements.Add(new CodeVariableDeclarationStatement("System.Xml.XmlElement", "_curXmlElement"));

            Hashtable idTable = new Hashtable();

            CopyPublic(ctc, "_curSchemaInfo", schema, 1, idTable);
            CopyPublic(ctc, "_curSchemaInfo", schema, 2, idTable);

            ctc.Statements.Add(
                new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression("_curSchemaInfo"),
                "Resolve"));

            return ctc;
        }

        public static CodeMemberField Field_theSchema()
        {
            CodeMemberField field;

            field = new CodeMemberField("Sooda.Schema.SchemaInfo", "theSchema");
            field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            field.InitExpression = new CodeObjectCreateExpression("Sooda.Schema.SchemaInfo");
            return field;
        }
        
        public static CodeMemberMethod Method_GetSchema()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.Name = "GetSchema";
            method.ReturnType = new CodeTypeReference("Sooda.Schema.SchemaInfo");
            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(null, "theSchema")));
            return method;
        }
    }
}
