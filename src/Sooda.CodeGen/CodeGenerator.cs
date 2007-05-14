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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;
using Sooda.CodeGen.CDIL;

namespace Sooda.CodeGen
{
    public class CodeGenerator
    {
        private SoodaProject _project;
        private ICodeGeneratorOutput _output;
        private CodeDomProvider _codeProvider;
        private SchemaInfo _schema;
        private CodeGeneratorOptions _codeGeneratorOptions;

#if DOTNET2
        private CodeDomProvider _codeGenerator;
        private CodeDomProvider _csharpCodeGenerator;
#else
        private ICodeGenerator _codeGenerator;
        private ICodeGenerator _csharpCodeGenerator;
#endif

        private string _fileExtensionWithoutPeriod;

        private bool _rebuildIfChanged = true;
        private bool _rewriteSkeletons = false;
        private bool _rewriteProjects = false;

        public bool RebuildIfChanged
        {
            get { return _rebuildIfChanged; }
            set { _rebuildIfChanged = value; }
        }

        public bool RewriteSkeletons
        {
            get { return _rewriteSkeletons; }
            set { _rewriteSkeletons = value; }
        }

        public bool RewriteProjects
        {
            get { return _rewriteProjects; }
            set { _rewriteProjects = value; }
        }

        public SoodaProject Project
        {
            get { return _project; }
            set { _project = value; }
        }

        public ICodeGeneratorOutput Output
        {
            get { return _output; }
            set { _output = value; }
        }

        public CodeGenerator(SoodaProject Project, ICodeGeneratorOutput Output)
        {
            this.Project = Project;
            this.Output = Output;
        }

        private void GenerateConditionalSets(CodeStatementCollection stats, int min, int max, ClassInfo ci)
        {
            if (min >= max)
            {
                stats.Add(new CodeCommentStatement("ordinal: " + min));
                return;
            }
            // stats.Add(new CodeCommentStatement("range: [" + min + ".." + max + "]"));

            int mid = (min + max) / 2;

            CodeConditionStatement ifStatement = new CodeConditionStatement();
            ifStatement.Condition = new CodeBinaryOperatorExpression(
                            new CodeArgumentReferenceExpression("fieldOrdinal"),
                            CodeBinaryOperatorType.LessThanOrEqual,
                            new CodePrimitiveExpression(mid));

            stats.Add(ifStatement);
            GenerateConditionalSets(ifStatement.TrueStatements, min, mid, ci);
            GenerateConditionalSets(ifStatement.FalseStatements, mid + 1, max, ci);
        }

        public void GenerateClassValues(CodeNamespace nspace, ClassInfo ci, bool miniStub)
        {
            CodeDomClassStubGenerator gen = new CodeDomClassStubGenerator(ci, Project);

            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ci.Name + "_Values");
            if (ci.InheritFrom != null)
                ctd.BaseTypes.Add(ci.InheritFrom + "_Values");
            else
                ctd.BaseTypes.Add(typeof(SoodaObjectReflectionBasedFieldValues));
            ctd.Attributes = MemberAttributes.Assembly;

            CodeMemberField field;

            foreach (FieldInfo fi in ci.LocalFields)
            {
                CodeTypeReference fieldType;
                if (fi.References != null)
                {
                    fieldType = gen.GetReturnType(PrimitiveRepresentation.SqlType, fi);
                }
                else if (fi.IsNullable)
                {
                    fieldType = gen.GetReturnType(Project.NullableRepresentation, fi);
                }
                else
                {
                    fieldType = gen.GetReturnType(Project.NotNullRepresentation, fi);
                }

                field = new CodeMemberField(fieldType, fi.Name);
                field.Attributes = MemberAttributes.Public;
                ctd.Members.Add(field);
            }

            CodeConstructor constructor2 = new CodeConstructor();
            constructor2.Attributes = MemberAttributes.Public;
            constructor2.Parameters.Add(new CodeParameterDeclarationExpression(typeof(SoodaObjectReflectionBasedFieldValues), "other"));
            constructor2.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("other"));
            ctd.Members.Add(constructor2);

            CodeConstructor constructor3 = new CodeConstructor();
            constructor3.Attributes = MemberAttributes.Public;
            constructor3.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string[]), "fieldNames"));
            constructor3.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("fieldNames"));
            ctd.Members.Add(constructor3);

            CodeMemberMethod cloneMethod = new CodeMemberMethod();
            cloneMethod.Name = "Clone";
            cloneMethod.ReturnType = new CodeTypeReference(typeof(SoodaObjectFieldValues));
            cloneMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            cloneMethod.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(ci.Name + "_Values",
                            new CodeThisReferenceExpression())));
            ctd.Members.Add(cloneMethod);

            nspace.Types.Add(ctd);
        }

        public void CDILParserTest(CodeTypeDeclaration ctd)
        {
#if SKIPPED
            using (StringWriter sw = new StringWriter())
            {
                CDILPrettyPrinter.PrintType(sw, ctd);
                using (StreamWriter fsw = File.CreateText(ctd.Name + "_1.txt"))
                {
                    fsw.Write(sw.ToString());
                }
                CodeTypeDeclaration ctd2 = CDILParser.ParseClass(sw.ToString(), new CDILContext());
                StringWriter sw2 = new StringWriter();
                CDILPrettyPrinter.PrintType(sw2, ctd2);
                using (StreamWriter fsw = File.CreateText(ctd.Name + "_2.txt"))
                {
                    fsw.Write(sw2.ToString());
                }
                if (sw2.ToString() != sw.ToString())
                {
                    throw new InvalidOperationException("DIFFERENT!");
                }
            }
#endif
        }

        public string MakeCamelCase(string s)
        {
            return Char.ToLower(s[0]) + s.Substring(1);
        }

        public void GenerateClassStub(CodeNamespace nspace, ClassInfo ci, bool miniStub)
        {
            if (!miniStub)
                GenerateClassValues(nspace, ci, miniStub);

            CodeDomClassStubGenerator gen = new CodeDomClassStubGenerator(ci, Project);
            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;
            context["HasBaseClass"] = ci.InheritsFromClass != null;
            context["MiniStub"] = miniStub;
            context["HasKeyGen"] = gen.KeyGen != "none";
            if (ci.ExtBaseClassName != null && !miniStub)
            {
                context["BaseClassName"] = ci.ExtBaseClassName;
            }
            else if (ci.InheritFrom != null && !miniStub)
            {
                context["BaseClassName"] = ci.InheritFrom;
            }
            else if (Project.BaseClassName != null && !miniStub)
            {
                context["BaseClassName"] = Project.BaseClassName;
            }
            else
            {
                context["BaseClassName"] = "SoodaObject";
            }

            CodeTypeDeclaration ctd = CDILParser.ParseClass(CDILTemplate.Get("Stub.cdil"), context);
            if (ci.Description != null)
            {
                ctd.Comments.Add(new CodeCommentStatement("<summary>", true));
                ctd.Comments.Add(new CodeCommentStatement(ci.Description, true));
                ctd.Comments.Add(new CodeCommentStatement("</summary>", true));
            }
            nspace.Types.Add(ctd);

            if (miniStub)
                return;

            CodeTypeDeclaration ctdLoader = GetLoaderClass(ci);

            if (!Project.LoaderClass)
            {
                foreach (CodeTypeMember m in ctdLoader.Members)
                {
                    ctd.Members.Add(m);
                }
            }

            // class constructor

            if (gen.KeyGen != "none")
            {
                ctd.Members.Add(gen.Field_keyGenerator());
            }

            gen.GenerateFields(ctd, ci);
            gen.GenerateProperties(ctd, ci);


            // literals
            if (ci.Constants != null && ci.GetPrimaryKeyFields().Length == 1)
            {
                foreach (ConstantInfo constInfo in ci.Constants)
                {
                    if (ci.GetFirstPrimaryKeyField().DataType == FieldDataType.Integer)
                    {
                        ctd.Members.Add(gen.Prop_LiteralValue(constInfo.Name, Int32.Parse(constInfo.Key)));
                    }
                    else if (ci.GetFirstPrimaryKeyField().DataType == FieldDataType.String)
                    {
                        ctd.Members.Add(gen.Prop_LiteralValue(constInfo.Name, constInfo.Key));
                    }
                    else if (ci.GetFirstPrimaryKeyField().DataType == FieldDataType.AnsiString)
                    {
                        ctd.Members.Add(gen.Prop_LiteralValue(constInfo.Name, constInfo.Key));
                    }
                    else
                        throw new NotSupportedException("Primary key type " + ci.GetFirstPrimaryKeyField().DataType + " is not supported");
                }
            }

            foreach (FieldInfo fi in ci.LocalFields)
            {
                if (fi.IsPrimaryKey)
                    continue;

                if (ci.Triggers || fi.ForceTrigger)
                {
                    ctd.Members.Add(gen.Method_TriggerFieldUpdate(fi, "BeforeFieldUpdate"));
                    ctd.Members.Add(gen.Method_TriggerFieldUpdate(fi, "AfterFieldUpdate"));
                }
            }
        }

        public void GenerateClassFactory(CodeNamespace nspace, ClassInfo ci)
        {
            FieldInfo fi = ci.GetFirstPrimaryKeyField();
            string pkClrTypeName = fi.GetFieldHandler().GetFieldType().FullName;
            string pkFieldHandlerTypeName = fi.GetFieldHandler().GetType().FullName;

            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;
            context["OutNamespace"] = Project.OutputNamespace;
            if (ci.GetPrimaryKeyFields().Length == 1)
            {
                context["GetRefArgumentType"] = pkClrTypeName;
                context["MultiColumnPrimaryKey"] = false;
            }
            else
            {
                context["GetRefArgumentType"] = "SoodaTuple";
                context["MultiColumnPrimaryKey"] = true;
            }
            context["PrimaryKeyHandlerType"] = pkFieldHandlerTypeName;
            context["IsAbstract"] = ci.IsAbstractClass();
            if (Project.LoaderClass)
                context["LoaderClass"] = Project.OutputNamespace + "." + ci.Name + "Loader";
            else
                context["LoaderClass"] = Project.OutputNamespace + ".Stubs." + ci.Name + "_Stub";

            CodeTypeDeclaration factoryClass = CDILParser.ParseClass(CDILTemplate.Get("Factory.cdil"), context);

            factoryClass.CustomAttributes.Add(new CodeAttributeDeclaration("SoodaObjectFactoryAttribute",
                new CodeAttributeArgument(new CodePrimitiveExpression(ci.Name)),
                new CodeAttributeArgument(new CodeTypeOfExpression(ci.Name))
                ));

            CodeTypeConstructor cctor = new CodeTypeConstructor();
            cctor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, "_fieldHandlers"),
                new CodeArrayCreateExpression("SoodaFieldHandler", new CodePrimitiveExpression(ci.UnifiedFields.Count))));

            foreach (FieldInfo fi2 in ci.UnifiedFields)
            {
                string typeWrapper = fi2.GetFieldHandler().GetType().FullName;
                bool isNullable = fi2.IsNullable;

                cctor.Statements.Add(
                    new CodeAssignStatement(
                    new CodeArrayIndexerExpression(
                    new CodeFieldReferenceExpression(null, "_fieldHandlers"),
                    new CodePrimitiveExpression(fi2.ClassUnifiedOrdinal)
                    ),
                    new CodeObjectCreateExpression(typeWrapper, new CodePrimitiveExpression(isNullable))));
            }

            factoryClass.Members.Add(cctor);
            nspace.Types.Add(factoryClass);
        }

        public void GenerateClassSkeleton(CodeNamespace nspace, ClassInfo ci, bool useChainedConstructorCall, bool fakeSkeleton)
        {
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ci.Name);
            if (ci.Description != null)
            {
                ctd.Comments.Add(new CodeCommentStatement("<summary>", true));
                ctd.Comments.Add(new CodeCommentStatement(ci.Description, true));
                ctd.Comments.Add(new CodeCommentStatement("</summary>", true));
            }
            ctd.BaseTypes.Add(Project.OutputNamespace + ".Stubs." + ci.Name + "_Stub");
            if (ci.IsAbstractClass())
                ctd.TypeAttributes |= System.Reflection.TypeAttributes.Abstract;
            nspace.Types.Add(ctd);

            CodeDomClassSkeletonGenerator gen = new CodeDomClassSkeletonGenerator();

            ctd.Members.Add(gen.Constructor_Raw());
            ctd.Members.Add(gen.Constructor_Inserting(useChainedConstructorCall));
            ctd.Members.Add(gen.Constructor_Inserting2(useChainedConstructorCall));

            if (!useChainedConstructorCall)
            {
                ctd.Members.Add(gen.Method_InitObject());
            }
        }

        private void OutputFactories(CodeArrayCreateExpression cace, string ns, SchemaInfo schema)
        {
            foreach (IncludeInfo ii in schema.Includes)
            {
                OutputFactories(cace, ii.Namespace, ii.Schema);
            }

            foreach (ClassInfo ci in schema.LocalClasses)
            {
                cace.Initializers.Add(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(ns + ".Stubs." + ci.Name + "_Factory"), "TheFactory"));
            }
        }

        public void GenerateDatabaseSchema(CodeNamespace nspace, SchemaInfo schema)
        {
            CDILContext context = new CDILContext();
            context["OutNamespace"] = Project.OutputNamespace;

            CodeTypeDeclaration databaseSchemaClass = CDILParser.ParseClass(CDILTemplate.Get("DatabaseSchema.cdil"), context);

            CodeArrayCreateExpression cace = new CodeArrayCreateExpression("ISoodaObjectFactory");
            OutputFactories(cace, Project.OutputNamespace, schema);

            CodeTypeConstructor ctc = new CodeTypeConstructor();
            ctc.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(null, "_theSchema"),
                        new CodeMethodInvokeExpression(null,"LoadSchema")));

            databaseSchemaClass.Members.Add(ctc);

            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_factories"), cace));

            databaseSchemaClass.Members.Add(ctor);

            nspace.Types.Add(databaseSchemaClass);
        }

        public void GenerateListWrapper(CodeNamespace nspace, ClassInfo ci)
        {
            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;
            context["OptionalNewAttribute"] = (_codeProvider is Microsoft.VisualBasic.VBCodeProvider) ? "" : ",New";
            context["WithIndexers"] = Project.WithIndexers;

#if DOTNET2
            if (!_codeGenerator.Supports(GeneratorSupport.DeclareIndexerProperties))
                context["WithIndexers"] = false;
#endif

            CodeTypeDeclaration listWrapperClass = CDILParser.ParseClass(CDILTemplate.Get("ListWrapper.cdil"), context);
#if DOTNET2
            listWrapperClass.BaseTypes.Clear();
            listWrapperClass.BaseTypes.Add(
                new CodeTypeReference("Sooda.ObjectMapper.SoodaObjectCollectionWrapperGeneric", 
                new CodeTypeReference(Project.OutputNamespace + "." + ci.Name)));
#endif
            nspace.Types.Add(listWrapperClass);
        }

        public CodeTypeDeclaration GetLoaderClass(ClassInfo ci)
        {
            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;
            context["HasBaseClass"] = ci.InheritsFromClass != null;

            string formalParameters = "";
            string actualParameters = "";

            foreach (FieldInfo fi in ci.GetPrimaryKeyFields())
            {
                if (formalParameters != "")
                {
                    formalParameters += ", ";
                    actualParameters += ", ";
                }
                string pkClrTypeName = fi.GetFieldHandler().GetFieldType().FullName;
                formalParameters += pkClrTypeName + " " + MakeCamelCase(fi.Name);
                actualParameters += "arg(" + MakeCamelCase(fi.Name) + ")";
            }

            context["PrimaryKeyFormalParameters"] = formalParameters;
            context["PrimaryKeyActualParameters"] = actualParameters;
            if (ci.GetPrimaryKeyFields().Length == 1)
            {
                context["PrimaryKeyActualParametersTuple"] = actualParameters;
                context["PrimaryKeyIsTuple"] = false;
            }
            else
            {
                context["PrimaryKeyIsTuple"] = true;
                context["PrimaryKeyActualParametersTuple"] = "new SoodaTuple(" + actualParameters + ")";
            }

            context["ClassUnifiedFieldCount"] = ci.UnifiedFields.Count;
            context["PrimaryKeyFieldHandler"] = ci.GetFirstPrimaryKeyField().GetFieldHandler().GetType().FullName;
            context["OptionalNewAttribute"] = (ci.InheritsFromClass != null) ? ",New" : "";
            if (_codeProvider is Microsoft.VisualBasic.VBCodeProvider)
            {
                context["OptionalNewAttribute"] = "";
            }
            if (Project.LoaderClass)
            {
                context["LoaderClass"] = Project.OutputNamespace + "." + ci.Name + "Loader";
                context["OptionalNewAttribute"] = "";
            }
            else
                context["LoaderClass"] = Project.OutputNamespace + ".Stubs." + ci.Name + "_Stub";
            CodeTypeDeclaration ctd = CDILParser.ParseClass(CDILTemplate.Get("Loader.cdil"), context);
            foreach (FieldInfo fi in ci.LocalFields)
            {
                for (int withTransaction = 0; withTransaction <= 1; ++withTransaction)
                {
                    for (int list = 0; list <= 1; list++)
                    {
                        for (int reference = 0; reference <= 1; reference++)
                        {
                            if (reference == 1 && fi.ReferencedClass == null)
                                continue;

                            if (list == 0 && !fi.FindMethod)
                                continue;

                            if (list == 1 && !fi.FindListMethod)
                                continue;

                            CodeMemberMethod findMethod = new CodeMemberMethod();
                            findMethod.Name = "Find" + ((list == 1) ? "List" : "") + "By" + fi.Name;
                            findMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                            findMethod.ReturnType = new CodeTypeReference(Project.OutputNamespace + "." + ci.Name + ((list == 1) ? "List" : ""));

                            if (withTransaction == 1)
                            {
                                findMethod.Parameters.Add(
                                    new CodeParameterDeclarationExpression(
                                    new CodeTypeReference(typeof(SoodaTransaction)), "transaction")
                                    );
                            }

                            if (reference == 1)
                            {
                                findMethod.Parameters.Add(
                                    new CodeParameterDeclarationExpression(
                                        fi.ReferencedClass.Name, MakeCamelCase(fi.Name))
                                        );
                            }
                            else
                            {
                                findMethod.Parameters.Add(
                                    new CodeParameterDeclarationExpression(
                                        fi.GetFieldHandler().GetFieldType(), MakeCamelCase(fi.Name))
                                        );
                            }

                            CodeExpression whereClause =
                                new CodeObjectCreateExpression(
                                new CodeTypeReference(typeof(SoodaWhereClause)),
                                new CodePrimitiveExpression(fi.Name + " = {0}"),
                                new CodeArrayCreateExpression(
                                   typeof(object),
                                   new CodeExpression[] { new CodeArgumentReferenceExpression(MakeCamelCase(fi.Name)) })
                                   );

                            CodeExpression transaction = new CodeArgumentReferenceExpression("transaction");
                            if (withTransaction == 0)
                            {
                                transaction = new CodePropertyReferenceExpression(
                                    new CodeTypeReferenceExpression(typeof(SoodaTransaction)),
                                    "ActiveTransaction");
                            }

                            findMethod.Statements.Add(
                                new CodeMethodReturnStatement(
                                    new CodeMethodInvokeExpression(
                                    null, ((list == 1) ? "GetList" : "LoadSingleObject"),
                                    transaction,
                                    whereClause)));
                            ctd.Members.Add(findMethod);
                        }
                    }
                }
            }
            return ctd;
        }

        private void GenerateLoaderClass(CodeNamespace nspace, ClassInfo ci)
        {
            CodeTypeDeclaration ctd = GetLoaderClass(ci);
            nspace.Types.Add(ctd);
        }

        public void GenerateRelationStub(CodeNamespace nspace, RelationInfo ri)
        {
            CodeDomListRelationTableGenerator gen = new CodeDomListRelationTableGenerator(ri);

            // public class RELATION_NAME_RelationTable : SoodaRelationTable
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ri.Name + "_RelationTable");
            ctd.BaseTypes.Add("SoodaRelationTable");
            nspace.Types.Add(ctd);

            // public RELATION_NAME_RelationTable() : base("RELATION_TABLE_NAME","LEFT_COLUMN_NAME","RIGHT_COLUMN_NAME") { }
            ctd.Members.Add(gen.Constructor_1());
            ctd.Members.Add(gen.Method_DeserializeTupleLeft());
            ctd.Members.Add(gen.Method_DeserializeTupleRight());

            CodeMemberField field;

            field = new CodeMemberField("Sooda.Schema.RelationInfo", "theRelationInfo");
            field.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            field.InitExpression =
                new CodeMethodInvokeExpression(
                new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(Project.OutputNamespace + "." + "_DatabaseSchema"), "GetSchema"), "FindRelationByName",
                new CodePrimitiveExpression(ri.Name));

            ctd.Members.Add(field);

            //public class RELATION_NAME_L_List : RELATION_NAME_Rel_List, LEFT_COLUMN_REF_TYPEList, ISoodaObjectList

            //OutputRelationHalfTable(nspace, "L", relationName, leftColumnName, leftColumnType, ref1ClassInfo, Project);
            //OutputRelationHalfTable(nspace, "R", relationName, rightColumnName, rightColumnType, ref2ClassInfo, Project);
        }

        private Hashtable generatedMiniBaseClasses = new Hashtable();

        private void GenerateMiniBaseClass(CodeCompileUnit ccu, string className)
        {
            if (!generatedMiniBaseClasses.Contains(className))
            {
                generatedMiniBaseClasses.Add(className, className);

                int lastPeriod = className.LastIndexOf('.');
                string namespaceName = Project.OutputNamespace;
                if (lastPeriod != -1)
                {
                    namespaceName = className.Substring(0, lastPeriod);
                    className = className.Substring(lastPeriod + 1);
                }

                CodeNamespace ns = new CodeNamespace(namespaceName);
                ns.Imports.Add(new CodeNamespaceImport("Sooda"));
                ccu.Namespaces.Add(ns);

                CodeTypeDeclaration ctd = new CodeTypeDeclaration(className);
                ctd.BaseTypes.Add(typeof(SoodaObject));
                ns.Types.Add(ctd);

                CodeConstructor ctor = new CodeConstructor();

                ctor.Attributes = MemberAttributes.Family;
                ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaConstructor", "c"));
                ctor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("c"));

                ctd.Members.Add(ctor);

                ctor = new CodeConstructor();

                ctor.Attributes = MemberAttributes.Family;
                ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
                ctor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("tran"));
                ctd.Members.Add(ctor);

                Output.Verbose("Generating mini base class {0}", className);
            }
        }

        private void GenerateTypedPublicQueryWrappers(CodeNamespace ns, ClassInfo classInfo)
        {
            CDILContext context = new CDILContext();
            context["ClassName"] = classInfo.Name;

            CodeTypeDeclaration ctd = CDILParser.ParseClass(CDILTemplate.Get("ClassField.cdil"), context);
            ns.Types.Add(ctd);

            foreach (CollectionBaseInfo coll in classInfo.UnifiedCollections)
            {
                CodeMemberProperty prop = new CodeMemberProperty();

                prop.Name = coll.Name;
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                prop.Type = new CodeTypeReference(coll.GetItemClass().Name + "CollectionExpression");
                prop.GetStatements.Add(
                    new CodeMethodReturnStatement(
                    new CodeObjectCreateExpression(prop.Type, new CodePrimitiveExpression(null), new CodePrimitiveExpression(coll.Name))
                    ));

                ctd.Members.Add(prop);
            }

            foreach (FieldInfo fi in classInfo.UnifiedFields)
            {
                CodeMemberProperty prop = new CodeMemberProperty();

                prop.Name = fi.Name;
                prop.Attributes = MemberAttributes.Public | MemberAttributes.Static;
                string fullWrapperTypeName;
                string optionalNullable = "";
                if (fi.IsNullable)
                    optionalNullable = "Nullable";

                if (fi.ReferencedClass == null)
                {
                    fullWrapperTypeName = fi.GetFieldHandler().GetTypedWrapperClass(fi.IsNullable);
                    if (fullWrapperTypeName == null)
                        continue;

                    prop.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(fullWrapperTypeName,
                        new CodeObjectCreateExpression("Sooda.QL.SoqlPathExpression", new CodePrimitiveExpression(fi.Name)))));
                }
                else
                {
                    fullWrapperTypeName = fi.ReferencedClass.Name + optionalNullable + "WrapperExpression";
                    prop.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(fullWrapperTypeName,
                        new CodePrimitiveExpression(null), new CodePrimitiveExpression(fi.Name))));
                }

                prop.Type = new CodeTypeReference(fullWrapperTypeName);
                ctd.Members.Add(prop);
            }
        }

        private void GenerateTypedInternalQueryWrappers(CodeNamespace ns, ClassInfo classInfo)
        {
            CDILContext context = new CDILContext();
            context["ClassName"] = classInfo.Name;
            context["PrimaryKeyType"] = classInfo.GetFirstPrimaryKeyField().GetFieldHandler().GetFieldType().FullName;
            context["CSharp"] = _codeProvider is Microsoft.CSharp.CSharpCodeProvider;

            CodeTypeDeclaration ctd = CDILParser.ParseClass(CDILTemplate.Get("TypedCollectionWrapper.cdil"), context);
            ns.Types.Add(ctd);

            context = new CDILContext();
            context["ClassName"] = classInfo.Name;
            context["PrimaryKeyType"] = classInfo.GetFirstPrimaryKeyField().GetFieldHandler().GetFieldType().FullName;
            context["CSharp"] = _codeProvider is Microsoft.CSharp.CSharpCodeProvider;
            context["ParameterAttributes"] = _codeGenerator.Supports(GeneratorSupport.ParameterAttributes);

            ctd = CDILParser.ParseClass(CDILTemplate.Get("TypedWrapper.cdil"), context);
            ns.Types.Add(ctd);

            foreach (CollectionBaseInfo coll in classInfo.UnifiedCollections)
            {
                CodeMemberProperty prop = new CodeMemberProperty();

                prop.Name = coll.Name;
                prop.Attributes = MemberAttributes.Public;
                prop.Type = new CodeTypeReference(coll.GetItemClass().Name + "CollectionExpression");
                prop.GetStatements.Add(
                    new CodeMethodReturnStatement(
                    new CodeObjectCreateExpression(prop.Type, new CodeThisReferenceExpression(), new CodePrimitiveExpression(coll.Name))
                    ));

                ctd.Members.Add(prop);
            }

            foreach (FieldInfo fi in classInfo.UnifiedFields)
            {
                CodeMemberProperty prop = new CodeMemberProperty();

                prop.Name = fi.Name;
                prop.Attributes = MemberAttributes.Public;
                string fullWrapperTypeName;
                string optionalNullable = "";
                if (fi.IsNullable)
                    optionalNullable = "Nullable";

                if (fi.ReferencedClass == null)
                {
                    fullWrapperTypeName = fi.GetFieldHandler().GetTypedWrapperClass(fi.IsNullable);
                    if (fullWrapperTypeName == null)
                        continue;

                    prop.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(fullWrapperTypeName,
                        new CodeObjectCreateExpression("Sooda.QL.SoqlPathExpression", new CodeThisReferenceExpression(), new CodePrimitiveExpression(fi.Name)))));
                }
                else
                {
                    fullWrapperTypeName = fi.ReferencedClass.Name + optionalNullable + "WrapperExpression";
                    prop.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(fullWrapperTypeName,
                        new CodeThisReferenceExpression(), new CodePrimitiveExpression(fi.Name))));
                }

                prop.Type = new CodeTypeReference(fullWrapperTypeName);
                ctd.Members.Add(prop);
            }

            CodeTypeDeclaration nullablectd = CDILParser.ParseClass(CDILTemplate.Get("NullableTypedWrapper.cdil"), context);
            ns.Types.Add(nullablectd);
        }

        private string GetEmbeddedSchemaFileName()
        {
            string ext = "bin";
            if (Project.EmbedSchema == EmbedSchema.Xml)
                ext = "xml";

            if (Project.SeparateStubs)
                return Path.Combine(Project.OutputPath, "Stubs/_DBSchema." + ext);
            else
                return Path.Combine(Project.OutputPath, "_DBSchema." + ext);
        }

        private string GetStubsFile()
        {
            if (Project.SeparateStubs)
                return Path.Combine(Project.OutputPath, "Stubs/_Stubs.csx");
            else
                return Path.Combine(Project.OutputPath, "_Stubs." + _fileExtensionWithoutPeriod);
        }

        private void GetInputAndOutputFiles(StringCollection inputFiles, StringCollection rewrittenOutputFiles, StringCollection shouldBePresentOutputFiles, StringCollection includedFiles)
        {
            // input 
            inputFiles.Add(Path.GetFullPath(this.GetType().Assembly.Location)); // Sooda.CodeGen.dll
            inputFiles.Add(Path.GetFullPath(Project.SchemaFile));

            // includes
            foreach (IncludeInfo ii in _schema.Includes)
            {
                includedFiles.Add(Path.GetFullPath(ii.SchemaFile));
            }

            // output
            rewrittenOutputFiles.Add(Path.GetFullPath(GetEmbeddedSchemaFileName()));
            rewrittenOutputFiles.Add(Path.GetFullPath(GetStubsFile()));

            if (Project.FilePerNamespace)
            {
                rewrittenOutputFiles.Add(Path.GetFullPath(Path.Combine(Project.OutputPath, "_Stubs." + Project.OutputNamespace + "." + _fileExtensionWithoutPeriod)));
                rewrittenOutputFiles.Add(Path.GetFullPath(Path.Combine(Project.OutputPath, "_Stubs." + Project.OutputNamespace + ".Stubs." + _fileExtensionWithoutPeriod)));
                if (Project.WithTypedQueryWrappers)
                    rewrittenOutputFiles.Add(Path.GetFullPath(Path.Combine(Project.OutputPath, "_Stubs." + Project.OutputNamespace + ".TypedQueries." + _fileExtensionWithoutPeriod)));
            }
            foreach (ClassInfo ci in _schema.LocalClasses)
            {
                shouldBePresentOutputFiles.Add(Path.GetFullPath(Path.Combine(Project.OutputPath, ci.Name + "." + _fileExtensionWithoutPeriod)));
            }
        }

        private DateTime MaxDate(StringCollection files)
        {
            DateTime max = DateTime.MinValue;

            foreach (string s in files)
            {
                DateTime dt;
               
                if (File.Exists(s))
                    dt = File.GetLastWriteTime(s);
                else
                    dt = DateTime.MaxValue;

                if (dt > max)
                    max = dt;
            }
            // Console.WriteLine("maxDate: {0}", max);
            return max;
        }

        private DateTime MinDate(StringCollection files)
        {
            DateTime min = DateTime.MaxValue;

            foreach (string s in files)
            {
                DateTime dt;
               
                if (File.Exists(s))
                    dt = File.GetLastWriteTime(s);
                else
                {
                    Output.Info("{0} not found", s);
                    dt = DateTime.MinValue;
                }

                if (dt < min)
                    min = dt;
            }
            // Console.WriteLine("minDate: {0}", min);
            return min;
        }

        private void SaveExternalProjects()
        {
            foreach (ExternalProjectInfo epi in Project.ExternalProjects)
            {
                Output.Verbose("Saving Project '{0}'...", epi.ActualProjectFile);
                epi.ProjectProvider.SaveTo(epi.ActualProjectFile);
            }
            Output.Verbose("Saved.");
        }

        private void LoadExternalProjects()
        {
            foreach (ExternalProjectInfo epi in Project.ExternalProjects)
            {
                IProjectFile projectProvider = GetProjectProvider(epi.ProjectType, _codeProvider);
                if (epi.ProjectFile != null)
                {
                    epi.ActualProjectFile = Path.Combine(Project.OutputPath, epi.ProjectFile);
                }
                else
                {
                    epi.ActualProjectFile = Path.Combine(Project.OutputPath, projectProvider.GetProjectFileName(Project.OutputNamespace));
                }
                epi.ProjectProvider = projectProvider;

                if (!File.Exists(epi.ActualProjectFile) || RewriteProjects)
                {
                    Output.Info("Creating Project file '{0}'.", epi.ActualProjectFile);
                    projectProvider.CreateNew(Project.OutputNamespace, Project.AssemblyName);
                }
                else
                {
                    Output.Verbose("Opening Project file '{0}'...", epi.ActualProjectFile);
                    projectProvider.LoadFrom(epi.ActualProjectFile);
                };
            }
        }

        private void CreateOutputDirectories(StringCollection outputFiles)
        {
            foreach (string s in outputFiles)
            {
                string d = Path.GetDirectoryName(s);
                if (!Directory.Exists(d))
                {
                    Output.Verbose("Creating directory {0}", d);
                    Directory.CreateDirectory(d);
                }
            }
        }

        private void WriteMiniStubs()
        {
            string fname = Path.Combine(Project.OutputPath, "Stubs/_MiniStubs.csx");
            Output.Verbose("Generating code for {0}...", fname);

            CodeCompileUnit ccu = new CodeCompileUnit();

            // stubs namespace
            CodeNamespace nspace = CreateStubsNamespace(_schema);
            ccu.Namespaces.Add(nspace);

            Output.Verbose("    * class stubs");
            foreach (ClassInfo ci in _schema.LocalClasses)
            {
                GenerateClassStub(nspace, ci, true);
            }
            using (StreamWriter sw = new StreamWriter(fname))
            {
                _csharpCodeGenerator.GenerateCodeFromCompileUnit(ccu, sw, _codeGeneratorOptions);
            }
        }

        private void WriteMiniSkeleton()
        {
            string fname = Path.Combine(Project.OutputPath, "Stubs/_MiniSkeleton.csx");
            Output.Verbose("Generating code for {0}...", fname);
            // fake skeletons for first compilation only

            CodeCompileUnit ccu = new CodeCompileUnit();
            CodeNamespace nspace = CreateBaseNamespace(_schema);
            ccu.Namespaces.Add(nspace);

            foreach (ClassInfo ci in _schema.LocalClasses)
            {
                GenerateClassSkeleton(nspace, ci, _codeGenerator.Supports(GeneratorSupport.ChainedConstructorArguments) ? true : false, true);
            }

            foreach (ClassInfo ci in _schema.LocalClasses)
            {
                if (ci.ExtBaseClassName != null)
                {
                    GenerateMiniBaseClass(ccu, ci.ExtBaseClassName);
                }
            }

            if (Project.BaseClassName != null)
            {
                GenerateMiniBaseClass(ccu, Project.BaseClassName);
            }

            using (StreamWriter sw = new StreamWriter(fname))
            {
                _csharpCodeGenerator.GenerateCodeFromCompileUnit(ccu, sw, _codeGeneratorOptions);
            }

        }

        private void WriteSkeletonClasses()
        {
            foreach (ClassInfo ci in _schema.LocalClasses)
            {
                string fname = ci.Name + "." + _fileExtensionWithoutPeriod;
                Output.Verbose("    {0}", fname);
                foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                {
                    epi.ProjectProvider.AddCompileUnit(fname);
                }

                string outFile = Path.Combine(Project.OutputPath, fname);

                if (!File.Exists(outFile) || RewriteSkeletons)
                {
                    using (TextWriter tw = new StreamWriter(outFile))
                    {
                        CodeNamespace nspace = CreateBaseNamespace(_schema);
                        GenerateClassSkeleton(nspace, ci, _codeGenerator.Supports(GeneratorSupport.ChainedConstructorArguments) ? true : false, false);
                        _codeGenerator.GenerateCodeFromNamespace(nspace, tw, _codeGeneratorOptions);
                    }
                }
            }
        }

        private void SerializeSchema()
        {
            string embedBaseDir = Project.OutputPath;
            if (Project.SeparateStubs)
                embedBaseDir = Path.Combine(embedBaseDir, "Stubs");

            if (Project.EmbedSchema == EmbedSchema.Xml)
            {
                Output.Verbose("Copying schema to {0}...", Path.Combine(embedBaseDir, "_DBSchema.xml"));
                File.Copy(Project.SchemaFile, Path.Combine(embedBaseDir, "_DBSchema.xml"), true);
                if (!Project.SeparateStubs)
                {
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddResource("_DBSchema.xml");
                    }
                }
            }
            else if (Project.EmbedSchema == EmbedSchema.Binary)
            {
                string binFileName = Path.Combine(embedBaseDir, "_DBSchema.bin");
                Output.Verbose("Serializing schema to {0}...", binFileName);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (FileStream fileStream = File.OpenWrite(binFileName))
                {
                    bf.Serialize(fileStream, _schema);
                }
                if (!Project.SeparateStubs)
                {
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddResource("_DBSchema.bin");
                    }
                }
            }
        }
                

        public void Run()
        {
            CodeCompileUnit ccu;
            CodeNamespace nspace;

            try
            {
                if (Project.SchemaFile == null)
                    throw new SoodaCodeGenException("No schema file specified.");

                if (Project.OutputPath == null)
                    throw new SoodaCodeGenException("No Output path specified.");

                if (Project.OutputNamespace == null)
                    throw new SoodaCodeGenException("No Output namespace specified.");

                CodeDomProvider codeProvider = GetCodeProvider(Project.Language);
                _codeProvider = codeProvider;

#if DOTNET2
                CodeDomProvider codeGenerator = codeProvider;
                CodeDomProvider csharpCodeGenerator = GetCodeProvider("c#");
#else                
                ICodeGenerator codeGenerator = codeProvider.CreateGenerator();
                ICodeGenerator csharpCodeGenerator = GetCodeProvider("c#").CreateGenerator();
#endif
                _codeGenerator = codeGenerator;
                _csharpCodeGenerator = csharpCodeGenerator;

                _fileExtensionWithoutPeriod = _codeProvider.FileExtension;
                if (_fileExtensionWithoutPeriod.StartsWith("."))
                    _fileExtensionWithoutPeriod = _fileExtensionWithoutPeriod.Substring(1);            

                Output.Verbose("Loading schema file {0}...", Project.SchemaFile);
                _schema = SchemaManager.ReadAndValidateSchema(
                    new XmlTextReader(Project.SchemaFile),
                    Path.GetDirectoryName(Project.SchemaFile)
                    );

                StringCollection inputFiles = new StringCollection();
                StringCollection rewrittenOutputFiles = new StringCollection();
                StringCollection shouldBePresentOutputFiles = new StringCollection();
                StringCollection includedFiles = new StringCollection();

                GetInputAndOutputFiles(inputFiles, rewrittenOutputFiles, shouldBePresentOutputFiles, includedFiles);

                bool doRebuild = false;

                if (MinDate(shouldBePresentOutputFiles) == DateTime.MaxValue)
                    doRebuild = true;

                if (MaxDate(inputFiles) > MinDate(rewrittenOutputFiles))
                    doRebuild = true;

                if (MaxDate(includedFiles) > MaxDate(inputFiles))
                {
                    doRebuild = true;
                }

                if (!RebuildIfChanged)
                    doRebuild = true;

                if (RewriteProjects)
                    doRebuild = true;

                if (!doRebuild)
                {
                    Output.Info("Not rebuilding.");
                    return;
                }
                /*
                foreach (string s in inputFiles)
                {
                    Output.Verbose("IN: {0}", s);
                }
                foreach (string s in outputFiles)
                {
                    Output.Verbose("OUT: {0}", s);
                }
                */

                if (Project.AssemblyName == null)
                    Project.AssemblyName = Project.OutputNamespace;

                Output.Verbose("Loaded {0} classes, {1} relations...", _schema.LocalClasses.Count, _schema.Relations.Count);
                LoadExternalProjects();
                CreateOutputDirectories(rewrittenOutputFiles);
                CreateOutputDirectories(shouldBePresentOutputFiles);

                string stubsFileName;

                Output.Verbose("CodeProvider:      {0}", codeProvider.GetType().FullName);
                Output.Verbose("Source extension:  {0}", codeProvider.FileExtension);
                foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                {
                    Output.Verbose("Project:           {0} ({1})", epi.ProjectType, epi.ActualProjectFile);
                }
                Output.Verbose("Output Path:       {0}", Project.OutputPath);
                Output.Verbose("Namespace:         {0}", Project.OutputNamespace);
                _codeGeneratorOptions = new CodeGeneratorOptions();
                _codeGeneratorOptions.IndentString = "  ";
                _codeGeneratorOptions.ElseOnClosing = false;

                // write skeleton files

                WriteSkeletonClasses();
                _codeGeneratorOptions.BracingStyle = "Block";
                _codeGeneratorOptions.BlankLinesBetweenMembers = false;

                if (Project.SeparateStubs)
                {
                    WriteMiniSkeleton();
                    WriteMiniStubs();
                }

                SerializeSchema();

                // codeGenerator = csharpCodeGenerator;

                if (Project.SeparateStubs)
                {
                    stubsFileName = Path.Combine(Project.OutputPath, "Stubs/_Stubs.csx");
                }
                else if (Project.FilePerNamespace)
                {
                    string fname = "_Stubs." + _fileExtensionWithoutPeriod;
                    stubsFileName = Path.Combine(Project.OutputPath, fname);

                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddCompileUnit(fname);
                    }

                    fname = "_Stubs." + Project.OutputNamespace + ".TypedQueries." + _fileExtensionWithoutPeriod;
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddCompileUnit(fname);
                    }

                    fname = "_Stubs." + Project.OutputNamespace + ".Stubs." + _fileExtensionWithoutPeriod;
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddCompileUnit(fname);
                    }
                }
                else
                {
                    string fname = "_Stubs." + _fileExtensionWithoutPeriod;
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddCompileUnit(fname);
                    }
                    stubsFileName = Path.Combine(Project.OutputPath, fname);
                }

                ccu = new CodeCompileUnit();
                CodeAttributeDeclaration cad = new CodeAttributeDeclaration("Sooda.SoodaObjectsAssembly");
                cad.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(Project.OutputNamespace + "._DatabaseSchema")));
                ccu.AssemblyCustomAttributes.Add(cad);

                nspace = CreateBaseNamespace(_schema);
                ccu.Namespaces.Add(nspace);

                Output.Verbose("    * list wrappers");
                foreach (ClassInfo ci in _schema.LocalClasses)
                {
                    GenerateListWrapper(nspace, ci);
                }
                if (Project.LoaderClass)
                {
                    Output.Verbose("    * loader class");
                    foreach (ClassInfo ci in _schema.LocalClasses)
                    {
                        GenerateLoaderClass(nspace, ci);
                    }
                }
                Output.Verbose("    * database schema");

                // stubs namespace
                nspace = CreateStubsNamespace(_schema);
                ccu.Namespaces.Add(nspace);

                Output.Verbose("    * class stubs");
                foreach (ClassInfo ci in _schema.LocalClasses)
                {
                    GenerateClassStub(nspace, ci, false);
                }
                Output.Verbose("    * class factories");
                foreach (ClassInfo ci in _schema.LocalClasses)
                {
                    GenerateClassFactory(nspace, ci);
                }
                Output.Verbose("    * N-N relation stubs");
                foreach (RelationInfo ri in _schema.LocalRelations)
                {
                    GenerateRelationStub(nspace, ri);
                }

                if (Project.WithTypedQueryWrappers)
                {
                    Output.Verbose("    * typed query wrappers (internal)");
                    foreach (ClassInfo ci in _schema.LocalClasses)
                    {
                        GenerateTypedInternalQueryWrappers(nspace, ci);
                    }

                    nspace = CreateTypedQueriesNamespace(_schema);
                    ccu.Namespaces.Add(nspace);

                    Output.Verbose("    * typed query wrappers");
                    foreach (ClassInfo ci in _schema.LocalClasses)
                    {
                        GenerateTypedPublicQueryWrappers(nspace, ci);
                    }
                }

                if (Project.FilePerNamespace)
                {
                    foreach (CodeNamespace ns in ccu.Namespaces)
                    {
                        using (StringWriter sw = new StringWriter())
                        {
                            Output.Verbose("Writing code...");
                            codeGenerator.GenerateCodeFromNamespace(ns, sw, _codeGeneratorOptions);
                            Output.Verbose("Done.");

                            string resultString = sw.ToString();
                            resultString = resultString.Replace("[System.ParamArrayAttribute()] ", "params ");

                            string fileName = "_Stubs." + ns.Name + "." + _fileExtensionWithoutPeriod;
                            foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                            {
                                epi.ProjectProvider.AddCompileUnit(fileName);
                            }

                            using (TextWriter tw = new StreamWriter(Path.Combine(Project.OutputPath, fileName)))
                            {
                                tw.Write(resultString);
                            }
                        }
                    }
                    ccu.Namespaces.Clear();
                }

                nspace = CreateBaseNamespace(_schema);
                ccu.Namespaces.Add(nspace);

                GenerateDatabaseSchema(nspace, _schema);

                using (StringWriter sw = new StringWriter())
                {
                    Output.Verbose("Writing code...");
                    codeGenerator.GenerateCodeFromCompileUnit(ccu, sw, _codeGeneratorOptions);
                    Output.Verbose("Done.");

                    string resultString = sw.ToString();
                    resultString = resultString.Replace("[System.ParamArrayAttribute()] ", "params ");

                    using (TextWriter tw = new StreamWriter(stubsFileName))
                    {
                        tw.Write(resultString);
                    }
                }

                SaveExternalProjects();

                return;
            }
            catch (SoodaCodeGenException e)
            {
                throw e;
            }
            catch (SoodaSchemaException e)
            {
                throw new SoodaCodeGenException("Schema validation error.", e);
            }
            catch (ApplicationException e)
            {
                throw new SoodaCodeGenException("Error generating code.", e);
            }
            catch (Exception e)
            {
                throw new SoodaCodeGenException("Unexpected error.", e);
            }
        }

        CodeDomProvider GetCodeProvider(string lang)
        {
            if (lang == null)
                return new Microsoft.CSharp.CSharpCodeProvider();

            switch (lang.ToLower())
            {
                case "cs":
                case "c#":
                case "csharp":
                    return new Microsoft.CSharp.CSharpCodeProvider();
#if !NO_VB

                case "vb":
                    return new Microsoft.VisualBasic.VBCodeProvider();
#endif

#if DOTNET2
                case "c++/cli":
                    return GetCodeProvider("Microsoft.VisualC.CppCodeProvider, CppCodeProvider, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL");
#endif

#if !DOTNET2
                case "mc++":
                    return GetCodeProvider("Microsoft.MCpp.MCppCodeProvider, MCppCodeDomProvider, Version=7.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
#endif

                default:
                    {
                        CodeDomProvider cdp = Activator.CreateInstance(Type.GetType(lang, true)) as CodeDomProvider;
                        if (cdp == null)
                            throw new SoodaCodeGenException("Cannot instantiate type " + lang);
                        return cdp;
                    }
            }
        }

        IProjectFile GetProjectProvider(string projectType, CodeDomProvider codeProvider)
        {
            if (projectType == "vs2003" || projectType == "vs")
            {
                switch (codeProvider.FileExtension)
                {
                    case "cs":
                        return new VScsprojProjectFile("Sooda.CodeGen.Template.Template71.csproj");

                    case "vb":
                        return new VSvbprojProjectFile("Sooda.CodeGen.Template.Template71.vbproj");

                    default:
                        throw new Exception("Visual Studio Project not supported for '" + codeProvider.FileExtension + "' files");
                }
            }
            if (projectType == "vs2005")
            {
                switch (codeProvider.FileExtension)
                {
                    case "cs":
                        return new VS2005csprojProjectFile();

                    case "vb":
                        return new VS2005vbprojProjectFile();

                    default:
                        throw new Exception("Visual Studio 2005 Project not supported for '" + codeProvider.FileExtension + "' files");
                }
            }
            if (projectType == "null")
            {
                return new NullProjectFile();
            }
            return Activator.CreateInstance(Type.GetType(projectType, true)) as IProjectFile;
        }

        void AddImportsFromIncludedSchema(CodeNamespace nspace, IncludeInfoCollection includes, bool stubsSubnamespace)
        {
            if (includes == null)
                return;

            foreach (IncludeInfo ii in includes)
            {
                nspace.Imports.Add(new CodeNamespaceImport(ii.Namespace + (stubsSubnamespace ? ".Stubs" : "")));
                AddImportsFromIncludedSchema(nspace, ii.Schema.Includes, stubsSubnamespace);
            }
        }

        void AddTypedQueryImportsFromIncludedSchema(CodeNamespace nspace, IncludeInfoCollection includes)
        {
            if (includes == null)
                return;

            foreach (IncludeInfo ii in includes)
            {
                nspace.Imports.Add(new CodeNamespaceImport(ii.Namespace + ".TypedQueries"));
                AddTypedQueryImportsFromIncludedSchema(nspace, ii.Schema.Includes);
            }
        }

        CodeNamespace CreateTypedQueriesNamespace(SchemaInfo schema)
        {
            CodeNamespace nspace = new CodeNamespace(Project.OutputNamespace + ".TypedQueries");
            nspace.Imports.Add(new CodeNamespaceImport("System"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Data"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda"));
            nspace.Imports.Add(new CodeNamespaceImport(Project.OutputNamespace + ".Stubs"));
            AddImportsFromIncludedSchema(nspace, schema.Includes, false);
            AddImportsFromIncludedSchema(nspace, schema.Includes, true);
            AddTypedQueryImportsFromIncludedSchema(nspace, schema.Includes);
            return nspace;
        }

        CodeNamespace CreateBaseNamespace(SchemaInfo schema)
        {
            CodeNamespace nspace = new CodeNamespace(Project.OutputNamespace);
            nspace.Imports.Add(new CodeNamespaceImport("System"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Data"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda"));
            nspace.Imports.Add(new CodeNamespaceImport(Project.OutputNamespace + ".Stubs"));
            AddImportsFromIncludedSchema(nspace, schema.Includes, false);
            return nspace;
        }

        CodeNamespace CreateStubsNamespace(SchemaInfo schema)
        {
            CodeNamespace nspace = new CodeNamespace(Project.OutputNamespace + ".Stubs");
            nspace.Imports.Add(new CodeNamespaceImport("System"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Data"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda.ObjectMapper"));
            nspace.Imports.Add(new CodeNamespaceImport(Project.OutputNamespace));
            AddImportsFromIncludedSchema(nspace, schema.Includes, false);
            AddImportsFromIncludedSchema(nspace, schema.Includes, true);
            return nspace;
        }
    }
}
