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
using System.Collections;

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

        public void GenerateClassValues(CodeNamespace nspace, ClassInfo ci, bool miniStub)
        {
            return;
            CodeDomClassStubGenerator gen = new CodeDomClassStubGenerator(ci, Project);

            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ci.Name + "_Values");
            if (ci.InheritFrom != null)
                ctd.BaseTypes.Add(ci.InheritFrom + "_Values");
            else
                ctd.BaseTypes.Add(typeof(SoodaObjectFieldValues));
            ctd.Attributes = MemberAttributes.Assembly;

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

                CodeMemberField field = new CodeMemberField(fieldType, fi.Name);
                field.Attributes = MemberAttributes.Public;
                ctd.Members.Add(field);
            }

            CodeMemberMethod cloneMethod = new CodeMemberMethod();
            cloneMethod.Name = "Clone";
            cloneMethod.ReturnType = new CodeTypeReference(typeof(SoodaObjectFieldValues));
            cloneMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            cloneMethod.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotImplementedException))));
            ctd.Members.Add(cloneMethod);

            CodeMemberMethod getBoxedFieldValueMethod = new CodeMemberMethod();
            getBoxedFieldValueMethod.Name = "GetBoxedFieldValue";
            getBoxedFieldValueMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "fieldOrdinal"));
            getBoxedFieldValueMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            getBoxedFieldValueMethod.ReturnType = new CodeTypeReference(typeof(object));
            getBoxedFieldValueMethod.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotImplementedException))));
            ctd.Members.Add(getBoxedFieldValueMethod);

            CodeMemberMethod setFieldValueMethod = new CodeMemberMethod();
            setFieldValueMethod.Name = "SetFieldValue";
            setFieldValueMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "fieldOrdinal"));
            setFieldValueMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "fieldValue"));
            setFieldValueMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            setFieldValueMethod.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(NotImplementedException))));
            ctd.Members.Add(setFieldValueMethod);

            CodeMemberMethod isNullMethod = new CodeMemberMethod();
            isNullMethod.Name = "IsNull";
            isNullMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "fieldOrdinal"));
            isNullMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;

            foreach (FieldInfo fi in ci.LocalFields)
            {
                if (fi.IsNullable)
                {
                    isNullMethod.Statements.Add(
                        new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                        new CodeArgumentReferenceExpression("fieldOrdinal"),CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(fi.ClassUnifiedOrdinal)),
                        new CodeMethodReturnStatement(
                        new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fi.Name), "IsNull")
                        )
                        ));
                }
            }
            if (ci.InheritFrom != null)
            {
                isNullMethod.Statements.Add(
                    new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                    new CodeBaseReferenceExpression(),"IsNull",
                    new CodeArgumentReferenceExpression("fieldOrdinal"))));
            }
            else
            {
                isNullMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
            }
            isNullMethod.ReturnType = new CodeTypeReference(typeof(bool));
            ctd.Members.Add(isNullMethod);

            CodeMemberProperty lengthProperty = new CodeMemberProperty();
            lengthProperty.Name = "Length";
            lengthProperty.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            lengthProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(ci.UnifiedFields.Count)));
            lengthProperty.Type = new CodeTypeReference(typeof(int));
            ctd.Members.Add(lengthProperty);

            nspace.Types.Add(ctd);
        }

        private void CDILParserTest(CodeTypeDeclaration ctd)
        {
            return;
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

            string formalParameters = "";
            string actualParameters = "";
            
            foreach (FieldInfo fi in ci.GetPrimaryKeyFields())
            {
                if (formalParameters != "")
                {
                    formalParameters += ", ";
                    actualParameters += ", ";
                }
                string pkClrTypeName = FieldDataTypeHelper.GetClrType(fi.DataType).FullName;
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
            context["PrimaryKeyFieldHandler"] = ci.GetFirstPrimaryKeyField().GetWrapperTypeName();
            context["OptionalNewAttribute"] = (ci.InheritsFromClass != null) ? ",New" : "";

            if (ci.ExtBaseClassName != null) 
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
            nspace.Types.Add(ctd);

            if (miniStub) 
                return ;

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
                    else
                        throw new NotSupportedException("Primary key type " + ci.GetFirstPrimaryKeyField().DataType + " is not supported");
                }
            }

            foreach (FieldInfo fi in ci.LocalFields) 
            {
                if (fi.IsPrimaryKey)
                    continue;

                if (fi.Name == ci.LastModifiedFieldName && !fi.ForceTrigger)
                    continue;

                if (!(ci.Triggers || fi.ForceTrigger))
                    continue;

                ctd.Members.Add(gen.Method_TriggerFieldUpdate(fi, "BeforeFieldUpdate"));
                ctd.Members.Add(gen.Method_TriggerFieldUpdate(fi, "AfterFieldUpdate"));

                if (fi.References != null) 
                {
                    ctd.Members.Add(gen.Method_BeforeCollectionUpdate(fi));
                    ctd.Members.Add(gen.Method_AfterCollectionUpdate(fi));
                }
            }

            CodeMemberMethod m = gen.Method_IterateOuterReferences();
            if (m != null)
                ctd.Members.Add(m);
        }

        public void GenerateClassFactory(CodeNamespace nspace, ClassInfo ci) 
        {
#warning ADD SUPPORT FOR MULTIPLE-COLUMN PRIMARY KEYS
            FieldInfo fi = ci.GetFirstPrimaryKeyField();
            string pkClrTypeName = FieldDataTypeHelper.GetClrType(fi.DataType).Name;
            string pkFieldHandlerTypeName = FieldDataTypeHelper.GetDefaultWrapperTypeName(fi.DataType);

            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;
            context["OutNamespace"] = Project.OutputNamespace;
            if (ci.GetPrimaryKeyFields().Length == 1)
            {
                context["GetRefArgumentType"] = pkClrTypeName;
            }
            else
            {
                context["GetRefArgumentType"] = "SoodaTuple";
            }
            context["PrimaryKeyHandlerType"] = pkFieldHandlerTypeName;
            context["IsAbstract"] = ci.IsAbstractClass();

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
                string typeWrapper = fi2.GetWrapperTypeName();
                bool isNullable = fi2.IsNullable;

                CodeMemberField field = new CodeMemberField(typeWrapper, "_fieldhandler_" + fi2.Name);
                field.Attributes = MemberAttributes.Assembly | MemberAttributes.Static;
                field.InitExpression = new CodeObjectCreateExpression(typeWrapper, new CodePrimitiveExpression(isNullable));
                factoryClass.Members.Add(field);

                cctor.Statements.Add(
                    new CodeAssignStatement(
                    new CodeArrayIndexerExpression(
                    new CodeFieldReferenceExpression(null, "_fieldHandlers"),
                    new CodePrimitiveExpression(fi2.ClassUnifiedOrdinal)
                    ),
                    new CodeFieldReferenceExpression(null, "_fieldhandler_" + fi2.Name)));
            }



            factoryClass.Members.Add(cctor);

            nspace.Types.Add(factoryClass);
        }

        public void GenerateClassSkeleton(CodeNamespace nspace, ClassInfo ci, bool useChainedConstructorCall, bool fakeSkeleton) 
        {
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ci.Name);
            ctd.BaseTypes.Add(Project.OutputNamespace + ".Stubs." + ci.Name + "_Stub");
            if (ci.IsAbstractClass())
                ctd.TypeAttributes |= System.Reflection.TypeAttributes.Abstract;
            nspace.Types.Add(ctd);

            CodeDomClassSkeletonGenerator gen = new CodeDomClassSkeletonGenerator(ci, Project);

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
            foreach (CodeTypeMember ctm in databaseSchemaClass.Members)
            {
                if (ctm.Name == "_factories")
                {
                    CodeMemberField fld = ctm as CodeMemberField;

                    CodeArrayCreateExpression cace = new CodeArrayCreateExpression("ISoodaObjectFactory");
                    OutputFactories(cace, Project.OutputNamespace, schema);
                    fld.InitExpression = cace;
                }
            }

            nspace.Types.Add(databaseSchemaClass);
        }

        public void GenerateListWrapper(CodeNamespace nspace, ClassInfo ci) 
        {
            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;

            CodeTypeDeclaration listWrapperClass = CDILParser.ParseClass(CDILTemplate.Get("ListWrapper.cdil"), context);
            nspace.Types.Add(listWrapperClass);
        }

        public void GenerateRelationStub(CodeNamespace nspace, RelationInfo ri) 
        {
            string relationName = ri.Name;
            string leftColumnName = ri.Table.Fields[0].DBColumnName;
            string rightColumnName = ri.Table.Fields[1].DBColumnName;
            string leftColumnType = FieldDataTypeHelper.GetClrType(ri.Table.Fields[0].DataType).Name;
            string rightColumnType = FieldDataTypeHelper.GetClrType(ri.Table.Fields[1].DataType).Name;
            string leftColumnRefType = ri.Table.Fields[0].References;
            string rightColumnRefType = ri.Table.Fields[1].References;
            string SoodaRelationTable = relationName + "_RelationTable";

            ClassInfo ref1ClassInfo = ri.GetRef1ClassInfo();
            ClassInfo ref2ClassInfo = ri.GetRef2ClassInfo();

            CodeDomListRelationTableGenerator gen = new CodeDomListRelationTableGenerator(ri, Project);

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
                string namespaceName = className.Substring(0, lastPeriod);
                className = className.Substring(lastPeriod + 1);

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
                    string rawTypeName = Sooda.Schema.FieldDataTypeHelper.GetClrType(fi.DataType).Name;
                    fullWrapperTypeName = "Sooda.QL.TypedWrappers.Soql" + optionalNullable + rawTypeName + "WrapperExpression";
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
            context["PrimaryKeyType"] = FieldDataTypeHelper.GetClrType(classInfo.GetFirstPrimaryKeyField().DataType).FullName;

            CodeTypeDeclaration ctd = CDILParser.ParseClass(CDILTemplate.Get("TypedCollectionWrapper.cdil"), context);
            ns.Types.Add(ctd);

            context = new CDILContext();
            context["ClassName"] = classInfo.Name;
            context["PrimaryKeyType"] = FieldDataTypeHelper.GetClrType(classInfo.GetFirstPrimaryKeyField().DataType).FullName;

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
                    string rawTypeName = Sooda.Schema.FieldDataTypeHelper.GetClrType(fi.DataType).Name;
                    fullWrapperTypeName = "Sooda.QL.TypedWrappers.Soql" + optionalNullable + rawTypeName + "WrapperExpression";
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

        public void Run()
        {
            CodeCompileUnit ccu;
            CodeNamespace nspace;

            try 
            {
                if (Project.SchemaFile == null)
                {
                    throw new SoodaCodeGenException("No schema file specified.");
                }

                if (Project.OutputPath == null) 
                {
                    throw new SoodaCodeGenException("No Output path specified.");
                }

                if (Project.OutputNamespace == null) 
                {
                    throw new SoodaCodeGenException("No Output namespace specified.");
                }

                CodeDomProvider codeProvider = GetCodeProvider(Project.Language);


                if (RebuildIfChanged) 
                {
                    string stubgenFile = System.Reflection.Assembly.GetEntryAssembly().Location;
                    string outputFile;

                    if (Project.SeparateStubs) 
                    {
                        outputFile = Path.Combine(Project.OutputPath, "Stubs/_Stubs.csx");
                    } 
                    else 
                    {
                        outputFile = Path.Combine(Project.OutputPath, "_Stubs." + codeProvider.FileExtension);
                    }

                    if (File.Exists(outputFile)) 
                    {
                        DateTime stubgenDateTime = File.GetLastWriteTime(stubgenFile);
                        DateTime schemaDateTime = File.GetLastWriteTime(Project.SchemaFile);
                        DateTime outputDateTime = File.GetLastWriteTime(outputFile);

                        DateTime maxInputDate = schemaDateTime;
                        if (stubgenDateTime > maxInputDate) 
                        {
                            maxInputDate = stubgenDateTime;
                        }

                        if (maxInputDate < outputDateTime) 
                        {
                            Output.Info("{0} not changed. Not rebuilding.", Project.SchemaFile);
                            return;
                        }
                    } 
                    else 
                    {
                        Output.Info("{0} does not exist. Rebuilding.", outputFile);
                    }
                }

                if (Project.AssemblyName == null)
                    Project.AssemblyName = Project.OutputNamespace;

                foreach (ExternalProjectInfo epi in Project.ExternalProjects) 
                {
                    IProjectFile projectProvider = GetProjectProvider(epi.ProjectType, codeProvider);
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

                Output.Verbose("CodeProvider:      {0}", codeProvider.GetType().FullName);
                Output.Verbose("Source extension:  {0}", codeProvider.FileExtension);
                foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                {
                    Output.Verbose("Project:           {0} ({1})", epi.ProjectType, epi.ActualProjectFile);
                }
                Output.Verbose("Output Path:       {0}", Project.OutputPath);
                Output.Verbose("Namespace:         {0}", Project.OutputNamespace);
                ICodeGenerator codeGenerator = codeProvider.CreateGenerator();
                ICodeGenerator csharpCodeGenerator = GetCodeProvider("c#").CreateGenerator();

                CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
                codeGeneratorOptions.BracingStyle = "C";
                codeGeneratorOptions.IndentString = "    ";
                codeGeneratorOptions.BlankLinesBetweenMembers = false;
                codeGeneratorOptions.ElseOnClosing = false;

                Output.Verbose("Loading schema file {0}...", Project.SchemaFile);
                SchemaInfo schema = SchemaManager.ReadAndValidateSchema(
                    new XmlTextReader(Project.SchemaFile), 
                    Path.GetDirectoryName(Project.SchemaFile)
                    );

                Output.Verbose("Loaded {0} classes, {1} relations...", schema.LocalClasses.Count, schema.Relations.Count);

                string dir = Project.OutputPath;
                string fname;

                if (!Directory.Exists(dir)) 
                {
                    Output.Verbose("Creating directory {0}", dir);
                    Directory.CreateDirectory(dir);
                }
                dir = Path.Combine(Project.OutputPath, "Stubs");
                if (!Directory.Exists(dir)) 
                {
                    Output.Verbose("Creating directory {0}", dir);
                    Directory.CreateDirectory(dir);
                }
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    fname = ci.Name + "." + codeProvider.FileExtension;
                    Output.Verbose("    {0}", fname);
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddCompileUnit(fname);
                    }

                    string outFile = Path.Combine(Project.OutputPath, fname);

                    if (!File.Exists(outFile) || RewriteSkeletons) 
                    {
                        string code;

                        using (TextWriter tw = new StringWriter()) 
                        {
                            ccu = new CodeCompileUnit();

                            nspace = CreateBaseNamespace(schema);
                            ccu.Namespaces.Add(nspace);

                            GenerateClassSkeleton(nspace, ci, codeGenerator.Supports(GeneratorSupport.ChainedConstructorArguments) ? true : false, false);
                            codeGenerator.GenerateCodeFromCompileUnit(ccu, tw, codeGeneratorOptions);
                            code = tw.ToString();
                        }

                        bool skip = false;
                        skip = code.IndexOf("<autogenerated>") != -1;

                        using (TextReader tr = new StringReader(code)) 
                        {
                            string line;
                            if (skip) 
                            {
                                while ((line = tr.ReadLine()) != null) 
                                {
                                    if (line.IndexOf("</autogenerated>") != -1) 
                                    {
                                        // skip two more lines and break
                                        tr.ReadLine();
                                        tr.ReadLine();
                                        break;
                                    }
                                }
                            }
                            using (TextWriter tw = new StreamWriter(outFile)) 
                            {
                                while ((line = tr.ReadLine()) != null) 
                                {
                                    tw.WriteLine(line);
                                };
                            }
                        }
                    }
                }

                if (Project.SeparateStubs) 
                {
                    fname = Path.Combine(Project.OutputPath, "Stubs/_MiniSkeleton.csx");
                    Output.Verbose("Generating code for {0}...", fname);
                    // fake skeletons for first compilation only

                    ccu = new CodeCompileUnit();

                    nspace = CreateBaseNamespace(schema);
                    ccu.Namespaces.Add(nspace);

                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateClassSkeleton(nspace, ci, codeGenerator.Supports(GeneratorSupport.ChainedConstructorArguments) ? true : false, true);
                    }

                    foreach (ClassInfo ci in schema.LocalClasses)
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
                        csharpCodeGenerator.GenerateCodeFromCompileUnit(ccu, sw, codeGeneratorOptions);
                    }

                    fname = Path.Combine(Project.OutputPath, "Stubs/_MiniStubs.csx");
                    Output.Verbose("Generating code for {0}...", fname);

                    ccu = new CodeCompileUnit();

                    // stubs namespace
                    nspace = CreateStubsNamespace(schema);
                    ccu.Namespaces.Add(nspace);

                    Output.Verbose("    * class stubs");
                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateClassStub(nspace, ci, true);
                    }
                    using (StreamWriter sw = new StreamWriter(fname)) 
                    {
                        csharpCodeGenerator.GenerateCodeFromCompileUnit(ccu, sw, codeGeneratorOptions);
                    }
                }

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
                        bf.Serialize(fileStream, schema);
                    }
                    if (!Project.SeparateStubs) 
                    {
                        foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                        {
                            epi.ProjectProvider.AddResource("_DBSchema.bin");
                        }
                    }
                }

                // codeGenerator = csharpCodeGenerator;

                if (Project.SeparateStubs) 
                {
                    fname = Path.Combine(Project.OutputPath, "Stubs/_Stubs.csx");
                } 
                else 
                {
                    fname = "_Stubs." + codeProvider.FileExtension;
                    foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                    {
                        epi.ProjectProvider.AddCompileUnit(fname);
                    }
                    fname = Path.Combine(Project.OutputPath, fname);
                }

                Output.Verbose("Generating code...");
                Output.Verbose("{0}", fname);
                ccu = new CodeCompileUnit();
                CodeAttributeDeclaration cad = new CodeAttributeDeclaration("Sooda.SoodaObjectsAssembly");
                cad.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(Project.OutputNamespace +  "._DatabaseSchema")));
                ccu.AssemblyCustomAttributes.Add(cad);

                nspace = CreateBaseNamespace(schema);
                ccu.Namespaces.Add(nspace);

                Output.Verbose("    * list wrappers");
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    GenerateListWrapper(nspace, ci);
                }
                Output.Verbose("    * database schema");
                GenerateDatabaseSchema(nspace, schema);

                // stubs namespace
                nspace = CreateStubsNamespace(schema);
                ccu.Namespaces.Add(nspace);

                Output.Verbose("    * class stubs");
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    GenerateClassStub(nspace, ci, false);
                }
                Output.Verbose("    * class factories");
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    GenerateClassFactory(nspace, ci);
                }
                Output.Verbose("    * N-N relation stubs");
                foreach (RelationInfo ri in schema.LocalRelations) 
                {
                    GenerateRelationStub(nspace, ri);
                }

                Output.Verbose("    * typed query wrappers (internal)");
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    GenerateTypedInternalQueryWrappers(nspace, ci);
                }

                nspace = CreateTypedQueriesNamespace(schema);
                ccu.Namespaces.Add(nspace);

                Output.Verbose("    * typed query wrappers");
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    GenerateTypedPublicQueryWrappers(nspace, ci);
                }

                using (StringWriter sw = new StringWriter())
                {
                    Output.Verbose("Writing code...");
                    codeGenerator.GenerateCodeFromCompileUnit(ccu, sw, codeGeneratorOptions);
                    Output.Verbose("Done.");

                    string resultString = sw.ToString();
                    resultString = resultString.Replace("[System.ParamArrayAttribute()] ", "params ");

                    using (TextWriter tw = new StreamWriter(fname)) 
                    {
                        tw.Write(resultString);
                    }
                }

                fname = "_FakeSkeleton." + codeProvider.FileExtension;

                foreach (ExternalProjectInfo epi in Project.ExternalProjects)
                {
                    Output.Verbose("Saving Project '{0}'...", epi.ActualProjectFile);
                    epi.ProjectProvider.SaveTo(epi.ActualProjectFile);
                }
                Output.Verbose("Saved.");
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
