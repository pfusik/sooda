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
using System.Collections;

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;
using Sooda.StubGen.CDIL;

namespace Sooda.StubGen 
{
    public class StubGen 
    {
        public static void GenerateClassValues(CodeNamespace nspace, ClassInfo ci, string outNamespace, StubGenOptions options, bool miniStub)
        {
            return;
            CodeDomClassStubGenerator gen = new CodeDomClassStubGenerator(ci, outNamespace);

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
                    fieldType = gen.GetReturnType(options.NullableRepresentation, fi);
                }
                else
                {
                    fieldType = gen.GetReturnType(options.NotNullRepresentation, fi);
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

        private static void CDILParserTest(CodeTypeDeclaration ctd)
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

        public static string MakeCamelCase(string s)
        {
            return Char.ToLower(s[0]) + s.Substring(1);
        }

        public static void GenerateClassStub(CodeNamespace nspace, ClassInfo ci, string outNamespace, StubGenOptions options, bool miniStub) 
        {
            if (!miniStub)
                GenerateClassValues(nspace, ci, outNamespace, options, miniStub);

            CodeDomClassStubGenerator gen = new CodeDomClassStubGenerator(ci, outNamespace);

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
            else if (options.BaseClassName != null && !miniStub) 
            {
                context["BaseClassName"] = options.BaseClassName;
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

            gen.GenerateFields(ctd, ci, outNamespace);
            gen.GenerateProperties(ctd, ci, outNamespace, options);

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
                    ctd.Members.Add(gen.Method_BeforeCollectionUpdate(fi, outNamespace));
                    ctd.Members.Add(gen.Method_AfterCollectionUpdate(fi, outNamespace));
                }
            }

#if A
            ctd.Members.Add(gen.Method_InitFieldValues());
            ctd.Members.Add(gen.Method_GetFieldHandler());
            if (gen.KeyGen != "none") 
            {
                if (ci.InheritsFromClass == null) 
                {
                    ctd.Members.Add(gen.Method_GetKeyGenerator());
                }
                ctd.Members.Add(gen.Method_InitNewObject());
            }

            bool[] boolTable = { false, true };

            for (int withTransaction = 0; withTransaction <= 1; withTransaction++) 
            {
                for (int withOrderBy = 0; withOrderBy <= 1; withOrderBy++) 
                {
                    for (int withOptions = 0; withOptions <= 1; withOptions++) 
                    {
                        ctd.Members.Add(gen.Method_GetList(
                            boolTable[withTransaction],
                            boolTable[withOrderBy],
                            boolTable[withOptions]));
                    }
                }
            }

            ctd.Members.Add(gen.Method_DoGetList());
            ctd.Members.Add(gen.Method_Load1());
            ctd.Members.Add(gen.Method_Load2());
            ctd.Members.Add(gen.Method_GetRefFromRecord());
            ctd.Members.Add(gen.Method_Get1());
            ctd.Members.Add(gen.Method_TryGet1());

            ctd.Members.Add(gen.Method_NormalGet());
            ctd.Members.Add(gen.Method_NormalTryGet());
            //ctd.Members.Add(gen.Method_SetPrimaryKeyValue());
#endif
            CodeMemberMethod m = gen.Method_IterateOuterReferences();
            if (m != null)
                ctd.Members.Add(m);

            CDILParserTest(ctd);
        }

        public static void GenerateClassFactory(CodeNamespace nspace, ClassInfo ci, string outNamespace) 
        {
#warning ADD SUPPORT FOR MULTIPLE-COLUMN PRIMARY KEYS
            FieldInfo fi = ci.GetFirstPrimaryKeyField();
            string pkClrTypeName = FieldDataTypeHelper.GetClrType(fi.DataType).Name;
            string pkFieldHandlerTypeName = FieldDataTypeHelper.GetDefaultWrapperTypeName(fi.DataType);

            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;
            context["OutNamespace"] = outNamespace;
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
            CDILParserTest(factoryClass);
        }

        public static void GenerateClassSkeleton(CodeNamespace nspace, ClassInfo ci, string outNamespace, bool useChainedConstructorCall, bool fakeSkeleton) 
        {
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(ci.Name);
            ctd.BaseTypes.Add(outNamespace + ".Stubs." + ci.Name + "_Stub");
            if (ci.IsAbstractClass())
                ctd.TypeAttributes |= System.Reflection.TypeAttributes.Abstract;
            nspace.Types.Add(ctd);

            CodeDomClassSkeletonGenerator gen = new CodeDomClassSkeletonGenerator(ci);

            ctd.Members.Add(gen.Constructor_Raw());
            ctd.Members.Add(gen.Constructor_Inserting(useChainedConstructorCall));
            ctd.Members.Add(gen.Constructor_Inserting2(useChainedConstructorCall));

            if (!useChainedConstructorCall) 
            {
                ctd.Members.Add(gen.Method_InitObject());
            }
            CDILParserTest(ctd);
        }

        private static void OutputFactories(CodeArrayCreateExpression cace, string ns, SchemaInfo schema)
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

        public static void GenerateDatabaseSchema(CodeNamespace nspace, string outNamespace, SchemaInfo schema) 
        {
            CDILContext context = new CDILContext();
            context["OutNamespace"] = outNamespace;

            CodeTypeDeclaration databaseSchemaClass = CDILParser.ParseClass(CDILTemplate.Get("DatabaseSchema.cdil"), context);
            foreach (CodeTypeMember ctm in databaseSchemaClass.Members)
            {
                if (ctm.Name == "_factories")
                {
                    CodeMemberField fld = ctm as CodeMemberField;

                    CodeArrayCreateExpression cace = new CodeArrayCreateExpression("ISoodaObjectFactory");
                    OutputFactories(cace, outNamespace, schema);
                    fld.InitExpression = cace;
                }
            }

            nspace.Types.Add(databaseSchemaClass);
        }

        public static void GenerateListWrapper(CodeNamespace nspace, ClassInfo ci, string outNamespace, StubGenOptions options) 
        {
            CDILContext context = new CDILContext();
            context["ClassName"] = ci.Name;

            CodeTypeDeclaration listWrapperClass = CDILParser.ParseClass(CDILTemplate.Get("ListWrapper.cdil"), context);
            nspace.Types.Add(listWrapperClass);
            CDILParserTest(listWrapperClass);
        }

        public static void GenerateRelationStub(CodeNamespace nspace, RelationInfo ri, string outNamespace, StubGenOptions options) 
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

            CodeDomListRelationTableGenerator gen = new CodeDomListRelationTableGenerator(ri, outNamespace);

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
                new CodeTypeReferenceExpression(outNamespace + "." + "_DatabaseSchema"), "GetSchema"), "FindRelationByName",
                new CodePrimitiveExpression(ri.Name));

            ctd.Members.Add(field);
            CDILParserTest(ctd);

            //public class RELATION_NAME_L_List : RELATION_NAME_Rel_List, LEFT_COLUMN_REF_TYPEList, ISoodaObjectList

            //OutputRelationHalfTable(nspace, "L", relationName, leftColumnName, leftColumnType, ref1ClassInfo, options);
            //OutputRelationHalfTable(nspace, "R", relationName, rightColumnName, rightColumnType, ref2ClassInfo, options);
        }
        private static int Usage() 
        {
            Console.WriteLine("Usage: StubGen [options]");
            Console.WriteLine("SoodaWhereClause options can be (*) - required option:");
            Console.WriteLine();
            Console.WriteLine("        --lang csharp    - (default) generate C# code");
#if !NO_VB

            Console.WriteLine("        --lang vb        - generate VB.NET code");
#endif
#if !NO_JSCRIPT

            Console.WriteLine("        --lang js        - generate JS.NET code (broken)");
#endif

            Console.WriteLine("        --lang <type>    - generate code using the specified CodeDOM codeProvider");
            Console.WriteLine();
            Console.WriteLine("        --project vs2003 - (default) generate VS.NET 2003 project file (.??proj)");
            Console.WriteLine("        --project null   - generate no project file");
            Console.WriteLine("        --project <type> - generate project file using custom type");
            Console.WriteLine();
            Console.WriteLine("    (*) --schema filename.xml - generate code from the specified schema");
            Console.WriteLine("    (*) --namespace <ns>      - specify the namespace to use");
            Console.WriteLine("    (*) --output <path>       - specify the output directory for files");
            Console.WriteLine("        --assembly <name>     - specify the name of resulting assembly");
            Console.WriteLine("        --base-class <name>   - specify the name of stub base class (SoodaObject)");
            Console.WriteLine("        --projectfile <name>  - use <name> instead of default project file name");
            Console.WriteLine("        --rewrite-skeletons   - force overwrite of skeleton classes");
            Console.WriteLine("        --rewrite-project     - force overwrite of project file");
            Console.WriteLine("        --separate-stubs      - ");
            Console.WriteLine("        --schema-embed-xml    - embed schema as an XML file");
            Console.WriteLine("        --schema-embed-bin    - embed schema as an BIN file");
            Console.WriteLine("        --help                - display this help");
            Console.WriteLine();
            Console.WriteLine("        --null-progagation    - enable null propagation");
            Console.WriteLine("        --no-null-progagation - disable null propagation (default)");
            Console.WriteLine("        --nullable-as [boxed | sqltype | raw ] (default = boxed)");
            Console.WriteLine("        --not-null-as [boxed | sqltype | raw ] (default = raw)");
            Console.WriteLine("                         - specify the way primitive values are handled");
            Console.WriteLine("        --with-tostring       - generate ToString() that returns primary key value");
            Console.WriteLine("        --without-tostring    - don't generate ToString()");
            Console.WriteLine();
            return 1;
        }

        enum EmbedSchema
        {
            Xml,
            Binary
        }

        private static Hashtable generatedMiniBaseClasses = new Hashtable();

        private static void GenerateMiniBaseClass(CodeCompileUnit ccu, string className)
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

                Console.WriteLine("Generating mini base class {0}", className);
            }
        }

        public static int Main(string[] args) 
        {
            string lang = null;
            string outputNamespace = null;
            string outputPath = null;
            string schemaFile = null;
            string customProjectFile = null;
            string assemblyName = null;
            bool rewriteSkeletons = false;
            bool rewriteProject = false;
            bool noProject = false;
            bool separateStubs = false;
            bool rebuildIfChanged = false;

            EmbedSchema embedSchema = EmbedSchema.Binary;
            StubGenOptions options = new StubGenOptions();

            System.Collections.Specialized.StringCollection projectTypes = new System.Collections.Specialized.StringCollection();
            System.Collections.Specialized.StringCollection projectFiles = new System.Collections.Specialized.StringCollection();

            ArrayList projectProviders = new ArrayList();

            try 
            {
                for (int i = 0; i < args.Length; ++i) 
                {
                    switch (args[i]) 
                    {
                        case "/?":
                        case "-?":
                        case "--help":
                        case "-h":
                            return Usage();

                        case "-l":
                        case "--lang":
                            lang = args[++i];
                            break;

                        case "-p":
                        case "--project":
                            projectTypes.Add(args[++i]);
                            break;

                        case "-pf":
                        case "--projectfile":
                            customProjectFile = args[++i];
                            break;

                        case "-np":
                        case "--noproject":
                            noProject = true;
                            break;

                        case "--separate-stubs":
                            separateStubs = true;
                            break;

                        case "--rebuild-if-changed":
                            rebuildIfChanged = true;
                            break;

                        case "--merged-stubs":
                            separateStubs = false;
                            break;

                        case "--schema-embed-xml":
                            embedSchema = EmbedSchema.Xml;
                            break;

                        case "--schema-embed-bin":
                            embedSchema = EmbedSchema.Binary;
                            break;

                        case "--nullable-as":
                            options.NullableRepresentation = (PrimitiveRepresentation)Enum.Parse(typeof(PrimitiveRepresentation), args[++i], true);
                            break;

                        case "--not-null-as":
                            options.NotNullRepresentation = (PrimitiveRepresentation)Enum.Parse(typeof(PrimitiveRepresentation), args[++i], true);
                            break;

                        case "-s":
                        case "--schema":
                            schemaFile = args[++i];
                            break;

                        case "-a":
                        case "--assembly-name":
                            assemblyName = args[++i];
                            break;

                        case "-bc":
                        case "--base-class":
                            options.BaseClassName = args[++i];
                            break;

                        case "--null-propagation":
                            options.NullPropagation = true;
                            break;

                        case "--no-null-propagation":
                            options.NullPropagation = false;
                            break;

                        case "--rewrite-skeletons":
                            rewriteSkeletons = true;
                            break;

                        case "--rewrite-project":
                            rewriteProject = true;
                            break;

                        case "-n":
                        case "-ns":
                        case "--namespace":
                            outputNamespace = args[++i];
                            break;

                        case "-o":
                        case "-out":
                        case "--output":
                            outputPath = args[++i];
                            break;
                    }
                };

                if (projectTypes.Count == 0 && !noProject) 
                {
                    projectTypes.Add("vs2003");
                };

                if (schemaFile == null) 
                {
                    Console.WriteLine("ERROR: No schema filename specified.");
                    Console.WriteLine();
                    return Usage();
                }

                if (outputPath == null) 
                {
                    Console.WriteLine("ERROR: No output path specified.");
                    Console.WriteLine();
                    return Usage();
                }

                if (outputNamespace == null) 
                {
                    Console.WriteLine("ERROR: No output namespace specified.");
                    Console.WriteLine();
                    return Usage();
                }

                CodeDomProvider codeProvider = GetCodeProvider(lang);


                // Hacks:

#if !NO_JSCRIPT

                if (codeProvider is Microsoft.JScript.JScriptCodeProvider) 
                {
                    options.WithIndexers = false;
                }
#endif
#if !NO_VB
                if (codeProvider is Microsoft.VisualBasic.VBCodeProvider) 
                {
                    options.IsVB = true;
                }
#endif
                // End of hacks

                if (rebuildIfChanged) 
                {
                    string stubgenFile = System.Reflection.Assembly.GetEntryAssembly().Location;
                    string outputFile;

                    if (separateStubs) 
                    {
                        outputFile = Path.Combine(outputPath, "Stubs/_Stubs.csx");
                    } 
                    else 
                    {
                        outputFile = Path.Combine(outputPath, "_Stubs." + codeProvider.FileExtension);
                    }

                    if (File.Exists(outputFile)) 
                    {
                        DateTime stubgenDateTime = File.GetLastWriteTime(stubgenFile);
                        DateTime schemaDateTime = File.GetLastWriteTime(schemaFile);
                        DateTime outputDateTime = File.GetLastWriteTime(outputFile);

                        DateTime maxInputDate = schemaDateTime;
                        if (stubgenDateTime > maxInputDate) 
                        {
                            maxInputDate = stubgenDateTime;
                        }

                        if (maxInputDate < outputDateTime) 
                        {
                            Console.WriteLine("{0} not changed. Not rebuilding.", schemaFile);
                            return 0;
                        }
                    } 
                    else 
                    {
                        Console.WriteLine("{0} does not exist. Rebuilding.", outputFile);
                    }
                }

                if (assemblyName == null)
                    assemblyName = outputNamespace;

                foreach (string projectType in projectTypes) 
                {
                    IProjectFile projectProvider = GetProjectProvider(projectType, codeProvider);
                    string projectFile;

                    if (customProjectFile == null) 
                    {
                        projectFile = projectProvider.GetProjectFileName(outputNamespace);
                    } 
                    else 
                    {
                        projectFile = projectProvider.GetProjectFileName(customProjectFile);
                    }

                    projectFile = Path.Combine(outputPath, projectFile);

                    if (!File.Exists(projectFile) || rewriteProject) 
                    {
                        Console.WriteLine("Creating project file '{0}'.", projectFile);
                        projectProvider.CreateNew(outputNamespace, assemblyName);
                    } 
                    else 
                    {
                        Console.WriteLine("Opening project file '{0}'...", projectFile);
                        projectProvider.LoadFrom(projectFile);
                    };
                    projectFiles.Add(projectFile);
                    projectProviders.Add(projectProvider);
                }

                Console.WriteLine("CodeProvider:      {0}", codeProvider.GetType().FullName);
                Console.WriteLine("Source extension:  {0}", codeProvider.FileExtension);
                foreach (IProjectFile prov in projectProviders) 
                {
                    Console.WriteLine("ProjectProvider:   {0}", prov.GetType().FullName);
                }
                foreach (string s in projectFiles) 
                {
                    Console.WriteLine("Project File:      {0}", s);
                }
                Console.WriteLine("Output Path:       {0}", outputPath);
                Console.WriteLine("Namespace:         {0}", outputNamespace);
                Console.WriteLine();

                ICodeGenerator codeGenerator = codeProvider.CreateGenerator();
                ICodeGenerator csharpCodeGenerator = GetCodeProvider("c#").CreateGenerator();

                CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
                codeGeneratorOptions.BracingStyle = "C";
                codeGeneratorOptions.IndentString = "    ";
                codeGeneratorOptions.BlankLinesBetweenMembers = false;
                codeGeneratorOptions.ElseOnClosing = false;

                Console.WriteLine("Loading schema file {0}...", schemaFile);
                SchemaInfo schema = SchemaManager.ReadAndValidateSchema(new XmlTextReader(schemaFile), Path.GetDirectoryName(schemaFile));

                Console.WriteLine("Loaded {0} classes, {1} relations...", schema.LocalClasses.Count, schema.Relations.Count);

                string dir = outputPath;
                string fname;

                if (!Directory.Exists(dir)) 
                {
                    Console.WriteLine("Creating directory {0}", dir);
                    Directory.CreateDirectory(dir);
                }
                dir = Path.Combine(outputPath, "Stubs");
                if (!Directory.Exists(dir)) 
                {
                    Console.WriteLine("Creating directory {0}", dir);
                    Directory.CreateDirectory(dir);
                }
                foreach (ClassInfo ci in schema.LocalClasses) 
                {
                    fname = ci.Name + "." + codeProvider.FileExtension;
                    Console.WriteLine("    {0}", fname);
                    foreach (IProjectFile projectProvider in projectProviders) 
                    {
                        projectProvider.AddCompileUnit(fname);
                    }

                    string outFile = Path.Combine(outputPath, fname);

                    if (!File.Exists(outFile) || rewriteSkeletons) 
                    {
                        string code;

                        using (TextWriter tw = new StringWriter()) 
                        {
                            CodeCompileUnit ccu = new CodeCompileUnit();
                            CodeNamespace nspace;

                            nspace = CreateBaseNamespace(outputNamespace, schema);
                            ccu.Namespaces.Add(nspace);

                            GenerateClassSkeleton(nspace, ci, outputNamespace, codeGenerator.Supports(GeneratorSupport.ChainedConstructorArguments) ? true : false, false);
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

                if (separateStubs) 
                {
                    options.IsVB = false;

                    fname = Path.Combine(outputPath, "Stubs/_MiniSkeleton.csx");
                    Console.WriteLine("Generating code for {0}...", fname);
                    // fake skeletons for first compilation only

                    CodeCompileUnit ccu = new CodeCompileUnit();
                    CodeNamespace nspace;

                    nspace = CreateBaseNamespace(outputNamespace, schema);
                    ccu.Namespaces.Add(nspace);

                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateClassSkeleton(nspace, ci, outputNamespace, codeGenerator.Supports(GeneratorSupport.ChainedConstructorArguments) ? true : false, true);
                    }

                    foreach (ClassInfo ci in schema.LocalClasses)
                    {
                        if (ci.ExtBaseClassName != null)
                        {
                            GenerateMiniBaseClass(ccu, ci.ExtBaseClassName);
                        }
                    }

                    if (options.BaseClassName != null)
                    {
                        GenerateMiniBaseClass(ccu, options.BaseClassName);
                    }

                    using (StreamWriter sw = new StreamWriter(fname)) 
                    {
                        csharpCodeGenerator.GenerateCodeFromCompileUnit(ccu, sw, codeGeneratorOptions);
                    }

                    fname = Path.Combine(outputPath, "Stubs/_MiniStubs.csx");
                    Console.WriteLine("Generating code for {0}...", fname);

                    ccu = new CodeCompileUnit();

                    // stubs namespace
                    nspace = CreateStubsNamespace(outputNamespace, schema);
                    ccu.Namespaces.Add(nspace);

                    Console.WriteLine("    * class stubs");
                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateClassStub(nspace, ci, outputNamespace, options, true);
                    }
                    using (StreamWriter sw = new StreamWriter(fname)) 
                    {
                        csharpCodeGenerator.GenerateCodeFromCompileUnit(ccu, sw, codeGeneratorOptions);
                    }
                }

                string embedBaseDir = outputPath;
                if (separateStubs)
                    embedBaseDir = Path.Combine(embedBaseDir, "Stubs");

                if (embedSchema == EmbedSchema.Xml) 
                {
                    Console.WriteLine("Copying schema to {0}...", Path.Combine(embedBaseDir, "_DBSchema.xml"));
                    File.Copy(schemaFile, Path.Combine(embedBaseDir, "_DBSchema.xml"), true);
                    if (!separateStubs) 
                    {
                        foreach (IProjectFile projectProvider in projectProviders) 
                        {
                            projectProvider.AddResource("_DBSchema.xml");
                        }
                    }
                } 
                else if (embedSchema == EmbedSchema.Binary) 
                {
                    string binFileName = Path.Combine(embedBaseDir, "_DBSchema.bin");
                    Console.WriteLine("Serializing schema to {0}...", binFileName);
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (FileStream fileStream = File.OpenWrite(binFileName)) 
                    {
                        bf.Serialize(fileStream, schema);
                    }
                    if (!separateStubs) 
                    {
                        foreach (IProjectFile projectProvider in projectProviders) 
                        {
                            projectProvider.AddResource("_DBSchema.bin");
                        }
                    }
                }

                // codeGenerator = csharpCodeGenerator;

                if (separateStubs) 
                {
                    fname = Path.Combine(outputPath, "Stubs/_Stubs.csx");
                } 
                else 
                {
                    fname = "_Stubs." + codeProvider.FileExtension;
                    foreach (IProjectFile projectProvider in projectProviders) 
                    {
                        projectProvider.AddCompileUnit(fname);
                    }
                    fname = Path.Combine(outputPath, fname);
                }

                Console.WriteLine("Generating code...");
                Console.WriteLine("{0}", fname);
                using (TextWriter tw = new StreamWriter(fname)) 
                {
                    CodeCompileUnit ccu = new CodeCompileUnit();
                    CodeAttributeDeclaration cad = new CodeAttributeDeclaration("Sooda.SoodaObjectsAssembly");
                    cad.Arguments.Add(new CodeAttributeArgument(new CodeTypeOfExpression(outputNamespace +  "._DatabaseSchema")));
                    ccu.AssemblyCustomAttributes.Add(cad);

                    CodeNamespace nspace;

                    nspace = CreateBaseNamespace(outputNamespace, schema);
                    ccu.Namespaces.Add(nspace);

                    Console.WriteLine("    * list wrappers");
                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateListWrapper(nspace, ci, outputNamespace, options);
                    }
                    Console.WriteLine("    * database schema");
                    GenerateDatabaseSchema(nspace, outputNamespace, schema);

                    // stubs namespace
                    nspace = CreateStubsNamespace(outputNamespace, schema);
                    ccu.Namespaces.Add(nspace);

                    Console.WriteLine("    * class stubs");
                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateClassStub(nspace, ci, outputNamespace, options, false);
                    }
                    Console.WriteLine("    * class factories");
                    foreach (ClassInfo ci in schema.LocalClasses) 
                    {
                        GenerateClassFactory(nspace, ci, outputNamespace);
                    }
                    Console.WriteLine("    * N-N relation stubs");
                    foreach (RelationInfo ri in schema.LocalRelations) 
                    {
                        GenerateRelationStub(nspace, ri, outputNamespace, options);
                    }
                    Console.WriteLine("Writing code...");
                    codeGenerator.GenerateCodeFromCompileUnit(ccu, tw, codeGeneratorOptions);
                    Console.WriteLine("Done.");
                }

                fname = "_FakeSkeleton." + codeProvider.FileExtension;

                for (int i = 0; i < projectFiles.Count; ++i) 
                {
                    Console.WriteLine("Saving project '{0}'...", projectFiles[i]);
                    ((IProjectFile)projectProviders[i]).SaveTo(projectFiles[i]);
                }
                Console.WriteLine("Saved.");
                return 0;
            } 
            catch (ApplicationException e) 
            {
                Console.WriteLine("EXCEPTION: {0}", e);
                return 1;
            } 
            catch (Exception e) 
            {
                Console.WriteLine("EXCEPTION: {0}", e);
                return 1;
            }
        }

        static CodeDomProvider GetCodeProvider(string lang) 
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

#if !NO_JSCRIPT

                case "js":
                    return new Microsoft.JScript.JScriptCodeProvider();
#endif

                default: 
                {
                    CodeDomProvider cdp = Activator.CreateInstance(Type.GetType(lang, true)) as CodeDomProvider;
                    if (cdp == null)
                        Console.WriteLine("ERROR: Cannot instantiate type: " + lang);
                    return cdp;
                }
            }
        }

        static IProjectFile GetProjectProvider(string projectType, CodeDomProvider codeProvider) 
        {
            if (projectType == "vs2003" || projectType == "vs") 
            {
                switch (codeProvider.FileExtension) 
                {
                    case "cs":
                        return new VScsprojProjectFile("Sooda.StubGen.Template.Template71.csproj");

                    case "vb":
                        return new VSvbprojProjectFile("Sooda.StubGen.Template.Template71.vbproj");

                    default:
                        throw new Exception("Visual Studio project not supported for '" + codeProvider.FileExtension + "' files");
                }
            }
            if (projectType == "null") 
            {
                return new NullProjectFile();
            }
            return Activator.CreateInstance(Type.GetType(projectType, true)) as IProjectFile;
        }

        static void AddImportsFromIncludedSchema(CodeNamespace nspace, IncludeInfoCollection includes, bool stubsSubnamespace)
        {
            if (includes == null)
                return;

            foreach (IncludeInfo ii in includes)
            {
                nspace.Imports.Add(new CodeNamespaceImport(ii.Namespace + (stubsSubnamespace ? ".Stubs" : "")));
                AddImportsFromIncludedSchema(nspace, ii.Schema.Includes, stubsSubnamespace);
            }
        }

        static CodeNamespace CreateBaseNamespace(string outNamespace, SchemaInfo schema) 
        {
            CodeNamespace nspace = new CodeNamespace(outNamespace);
            nspace.Imports.Add(new CodeNamespaceImport("System"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Data"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda"));
            AddImportsFromIncludedSchema(nspace, schema.Includes, false);
            return nspace;
        }

        static CodeNamespace CreateStubsNamespace(string outNamespace, SchemaInfo schema) 
        {
            CodeNamespace nspace = new CodeNamespace(outNamespace + ".Stubs");
            nspace.Imports.Add(new CodeNamespaceImport("System"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            nspace.Imports.Add(new CodeNamespaceImport("System.Data"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda"));
            nspace.Imports.Add(new CodeNamespaceImport("Sooda.ObjectMapper"));
            nspace.Imports.Add(new CodeNamespaceImport(outNamespace));
            AddImportsFromIncludedSchema(nspace, schema.Includes, false);
            AddImportsFromIncludedSchema(nspace, schema.Includes, true);
            return nspace;
        }
    }
}