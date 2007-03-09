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

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;
using Sooda.CodeGen.CDIL;

namespace Sooda.CodeGen
{
    public class CodeDomClassStubGenerator : CodeDomHelpers
    {
        private ClassInfo classInfo;
        private SoodaProject options;
        public readonly string KeyGen;

        public CodeDomClassStubGenerator(ClassInfo ci, SoodaProject options)
        {
            this.classInfo = ci;
            this.options = options;
            string keyGen = "none";

            if (!ci.ReadOnly && ci.GetPrimaryKeyFields().Length == 1)
            {
                switch (ci.GetFirstPrimaryKeyField().DataType)
                {
                    case FieldDataType.Integer:
                        keyGen = "integer";
                        break;

                    case FieldDataType.Guid:
                        keyGen = "guid";
                        break;

                    case FieldDataType.Long:
                        keyGen = "long";
                        break;
                }
            }

            if (ci.KeyGenName != null)
                keyGen = ci.KeyGenName;

            this.KeyGen = keyGen;
        }

        private ClassInfo GetRootClass(ClassInfo ci)
        {
            if (ci.InheritsFromClass != null)
                return GetRootClass(ci.InheritsFromClass);
            else
                return ci;
        }

        public CodeMemberField Field_keyGenerator()
        {
            CodeMemberField field = new CodeMemberField("IPrimaryKeyGenerator", "keyGenerator");
            field.Attributes = MemberAttributes.Private | MemberAttributes.Static;

            switch (KeyGen)
            {
                case "guid":
                    field.InitExpression = new CodeObjectCreateExpression("Sooda.ObjectMapper.KeyGenerators.GuidGenerator");
                    break;

                case "integer":
                    field.InitExpression = new CodeObjectCreateExpression("Sooda.ObjectMapper.KeyGenerators.TableBasedGenerator",
                        new CodePrimitiveExpression(GetRootClass(classInfo).Name),
                        new CodeMethodInvokeExpression(
                        new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(options.OutputNamespace + "." + "_DatabaseSchema"), "GetSchema"),
                        "GetDataSourceInfo",
                        new CodePrimitiveExpression(classInfo.GetSafeDataSourceName())));
                    break;
                case "long":
                    field.InitExpression = new CodeObjectCreateExpression("Sooda.ObjectMapper.KeyGenerators.TableBasedGeneratorBigint",
                        new CodePrimitiveExpression(GetRootClass(classInfo).Name),
                        new CodeMethodInvokeExpression(
                        new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(options.OutputNamespace + "." + "_DatabaseSchema"), "GetSchema"),
                        "GetDataSourceInfo",
                        new CodePrimitiveExpression(classInfo.GetSafeDataSourceName())));
                    break;

                default:
                    field.InitExpression = new CodeObjectCreateExpression(KeyGen);
                    break;
            }
            return field;
        }

        public CodeMemberMethod Method_TriggerFieldUpdate(FieldInfo fi, string methodPrefix)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodPrefix + "_" + fi.Name;
            if (fi.References != null)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(fi.References, "oldValue"));
                method.Parameters.Add(new CodeParameterDeclarationExpression(fi.References, "newValue"));
            }
            else
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "oldValue"));
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "newValue"));
            };
            method.Attributes = MemberAttributes.Family;
            method.Statements.Add(new CodeMethodInvokeExpression(This, methodPrefix, new CodePrimitiveExpression(fi.Name), Arg("oldValue"), Arg("newValue")));
            return method;
        }

        public CodeConstructor Constructor_Raw()
        {
            CodeConstructor ctor = new CodeConstructor();

            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaConstructor", "c"));
            ctor.BaseConstructorArgs.Add(Arg("c"));

            return ctor;
        }
        public CodeConstructor Constructor_Mini_Inserting()
        {
            CodeConstructor ctor = new CodeConstructor();

            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            ctor.BaseConstructorArgs.Add(Arg("tran"));

            return ctor;
        }
        public CodeMemberProperty Prop_LiteralValue(string name, object val)
        {
            CodeMemberProperty prop;

            prop = new CodeMemberProperty();
            prop.Name = name;
            prop.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            prop.Type = new CodeTypeReference(classInfo.Name);
            //prop.CustomAttributes.Add(NoStepThrough());
            prop.GetStatements.Add(
                new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(
                LoaderClass(classInfo), "GetRef", new CodePrimitiveExpression(val))));

            return prop;
        }

        private CodeTypeReferenceExpression LoaderClass(ClassInfo ci)
        {
            if (options.LoaderClass)
                return new CodeTypeReferenceExpression(ci.Name + "Loader");
            else
                return new CodeTypeReferenceExpression(ci.Name + "_Stub");
        }

        public CodeTypeReference GetReturnType(PrimitiveRepresentation rep, FieldInfo fi)
        {
            switch (rep)
            {
                case PrimitiveRepresentation.Boxed:
                    return new CodeTypeReference(typeof(object));

                case PrimitiveRepresentation.SqlType:
                    Type t = fi.GetFieldHandler().GetSqlType();
                    if (t == null)
                        return new CodeTypeReference(fi.GetFieldHandler().GetFieldType());
                    else
                        return new CodeTypeReference(t);

                case PrimitiveRepresentation.RawWithIsNull:
                case PrimitiveRepresentation.Raw:
                    return new CodeTypeReference(fi.GetFieldHandler().GetFieldType());

#if DOTNET2
                case PrimitiveRepresentation.Nullable:
                    return new CodeTypeReference(fi.GetFieldHandler().GetNullableType());
#endif

                default:
                    throw new NotImplementedException("Unknown PrimitiveRepresentation: " + rep);
            }
        }

        private CodeExpression GetFieldValueExpression(FieldInfo fi)
        {
            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(typeof(Sooda.ObjectMapper.SoodaObjectImpl)),
                "GetBoxedFieldValue",
                new CodeThisReferenceExpression(),
                new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                new CodePrimitiveExpression(fi.ClassUnifiedOrdinal));
        }

        private CodeExpression GetFieldIsNullExpression(FieldInfo fi)
        {
            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(typeof(Sooda.ObjectMapper.SoodaObjectImpl)),
                "IsFieldNull",
                new CodeThisReferenceExpression(),
                new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                new CodePrimitiveExpression(fi.ClassUnifiedOrdinal)
                );
        }

        private CodeMemberProperty _IsNull(FieldInfo fi)
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = fi.Name + "_IsNull";
            prop.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            prop.Type = new CodeTypeReference(typeof(bool));

            prop.GetStatements.Add(
                new CodeMethodReturnStatement(
                GetFieldIsNullExpression(fi)));

            return prop;
        }

        private CodeExpression Box(CodeExpression expr)
        {
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(SoodaNullable)), "Box", expr);
        }

        private CodeMemberMethod _SetNull(FieldInfo fi)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "_SetNull_" + fi.Name;
            method.Attributes = MemberAttributes.Final | MemberAttributes.Public;

            method.Statements.Add(
                new CodeAssignStatement(
                GetFieldValueExpression(fi), new CodePrimitiveExpression(null)));

            return method;
        }

        private CodeExpression IsFieldNotNull(FieldInfo fi)
        {
            if (fi.References != null)
            {
                return new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(GetFieldValueForRead(fi), "IsNull"),
                        CodeBinaryOperatorType.ValueEquality,
                        new CodePrimitiveExpression(false));
            }
            else if (fi.IsNullable)
            {
                return new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(GetFieldValueForRead(fi), "IsNull"),
                        CodeBinaryOperatorType.ValueEquality,
                        new CodePrimitiveExpression(false));
            }
            else
            {
                return new CodePrimitiveExpression(true);
            }
        }

        private CodeExpression GetTransaction()
        {
            return new CodeMethodInvokeExpression(This, "GetTransaction");
        }

        private CodeExpression GetFieldValueForRead(FieldInfo fi)
        {
            return new CodeFieldReferenceExpression(
                    new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "Get" + fi.ParentClass.Name + "FieldValuesForRead",
                        new CodePrimitiveExpression(fi.Table.OrdinalInClass)), fi.Name);
        }

        private CodeExpression GetNotNullFieldValue(FieldInfo fi)
        {
            if (fi.References != null)
            {
                return new CodePropertyReferenceExpression(GetFieldValueForRead(fi), "Value");
            }
            else if (fi.IsNullable)
            {
                return new CodePropertyReferenceExpression(GetFieldValueForRead(fi), "Value");
            }
            else
            {
                return GetFieldValueForRead(fi);
            }
        }

        private int GetFieldRefCacheIndex(ClassInfo ci, FieldInfo fi0)
        {
            int p = 0;

            foreach (FieldInfo fi in ci.LocalFields)
            {
                if (fi == fi0)
                    return p;
                if (fi.ReferencedClass != null)
                    p++;
            }

            return -1;
        }

        private int GetFieldRefCacheCount(ClassInfo ci)
        {
            int p = 0;

            foreach (FieldInfo fi in ci.LocalFields)
            {
                if (fi.ReferencedClass != null)
                    p++;
            }

            return p;
        }

        private CodeExpression RefCacheArray()
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_refcache");
        }

        private CodeExpression RefCacheExpression(ClassInfo ci, FieldInfo fi)
        {
            return new CodeArrayIndexerExpression(RefCacheArray(), 
                new CodePrimitiveExpression(GetFieldRefCacheIndex(ci, fi)));
        }

        public void GenerateProperties(CodeTypeDeclaration ctd, ClassInfo ci)
        {
            CodeMemberProperty prop;

            foreach (FieldInfo fi in classInfo.LocalFields)
            {
                if (fi.References != null)
                    continue;

                if (fi.IsNullable)
                {
                    if (options.NullableRepresentation == PrimitiveRepresentation.RawWithIsNull)
                    {
                        ctd.Members.Add(_IsNull(fi));
                        if (!ci.ReadOnly)
                        {
                            ctd.Members.Add(_SetNull(fi));
                        }
                    }
                }
                else
                {
                    if (options.NotNullRepresentation == PrimitiveRepresentation.RawWithIsNull)
                    {
                        if (!ci.ReadOnly)
                        {
                            // if it's read-only, not-null means not-null and there's no
                            // exception
                            ctd.Members.Add(_IsNull(fi));
                        }
                    }
                }
            }

            int primaryKeyComponentNumber = 0;

            foreach (FieldInfo fi in classInfo.LocalFields)
            {
                PrimitiveRepresentation actualNullableRepresentation = options.NullableRepresentation;
                PrimitiveRepresentation actualNotNullRepresentation = options.NotNullRepresentation;

                if (fi.GetFieldHandler().GetSqlType() == null)
                {
                    if (actualNotNullRepresentation == PrimitiveRepresentation.SqlType)
                        actualNotNullRepresentation = PrimitiveRepresentation.Raw;

                    if (actualNullableRepresentation == PrimitiveRepresentation.SqlType)
                        actualNullableRepresentation = PrimitiveRepresentation.Raw;
                }

                CodeTypeReference returnType;

                //if (fi.Name == classInfo.PrimaryKeyFieldName)
                //{
                //  returnType = GetReturnType(PrimitiveRepresentation.Raw, fi.DataType);
                //}
                //else
                if (fi.References != null)
                {
                    returnType = new CodeTypeReference(fi.References);
                }
                else if (fi.IsNullable)
                {
                    returnType = GetReturnType(actualNullableRepresentation, fi);
                }
                else
                {
                    returnType = GetReturnType(actualNotNullRepresentation, fi);
                }

                prop = new CodeMemberProperty();
                prop.Name = fi.Name;
                prop.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                prop.Type = returnType;
                //prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_FieldNames")));
                if (fi.Description != null)
                {
                    prop.Comments.Add(new CodeCommentStatement("<summary>", true));
                    prop.Comments.Add(new CodeCommentStatement(fi.Description, true));
                    prop.Comments.Add(new CodeCommentStatement("</summary>", true));
                }
                ctd.Members.Add(prop);

                if (fi.Size != -1)
                {
                    CodeAttributeDeclaration cad = new CodeAttributeDeclaration("Sooda.SoodaFieldSizeAttribute");
                    cad.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(fi.Size)));
                    prop.CustomAttributes.Add(cad);
                }

                if (fi.IsPrimaryKey)
                {
                    CodeExpression getPrimaryKeyValue = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetPrimaryKeyValue");

                    if (classInfo.GetPrimaryKeyFields().Length > 1)
                    {
                        getPrimaryKeyValue = new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(SoodaTuple)), "GetValue", getPrimaryKeyValue, new CodePrimitiveExpression(primaryKeyComponentNumber));
                    }

                    prop.GetStatements.Add(
                        new CodeMethodReturnStatement(
                        new CodeCastExpression(
                        prop.Type,
                        getPrimaryKeyValue
                        )));

                    if (!classInfo.ReadOnly)
                    {
                        if (classInfo.GetPrimaryKeyFields().Length == 1)
                        {
                            prop.SetStatements.Add(
                                new CodeExpressionStatement(
                                new CodeMethodInvokeExpression(
                                new CodeThisReferenceExpression(), "SetPrimaryKeyValue",
                                new CodePropertySetValueReferenceExpression())));
                        }
                        else
                        {
                            prop.SetStatements.Add(
                                new CodeExpressionStatement(
                                new CodeMethodInvokeExpression(
                                new CodeThisReferenceExpression(), "SetPrimaryKeySubValue",
                                new CodePropertySetValueReferenceExpression(),
                                new CodePrimitiveExpression(primaryKeyComponentNumber),
                                new CodePrimitiveExpression(classInfo.GetPrimaryKeyFields().Length))));
                        }
                    }
                    primaryKeyComponentNumber++;
                    continue;
                }

                if (options.NullPropagation && (fi.References != null || fi.IsNullable) && (actualNullableRepresentation != PrimitiveRepresentation.Raw))
                {
                    CodeExpression retVal = new CodePrimitiveExpression(null);

                    if (fi.References == null && actualNullableRepresentation == PrimitiveRepresentation.SqlType)
                    {
                        retVal = new CodePropertyReferenceExpression(
                            new CodeTypeReferenceExpression(fi.GetFieldHandler().GetSqlType()), "Null");
                    }

                    prop.GetStatements.Add(
                        new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                        new CodeThisReferenceExpression(),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                        new CodeStatement[]
                            {
                                new CodeMethodReturnStatement(retVal)
                            },
                        new CodeStatement[]
                            {
                            }));
                }

                if (fi.References != null)
                {
                    // reference field getter
                    //
                    prop.GetStatements.Add(
                            new CodeConditionStatement(
                                new CodeBinaryOperatorExpression(
                                    RefCacheExpression(ci, fi),
                                    CodeBinaryOperatorType.IdentityEquality,
                                    new CodePrimitiveExpression(null)),
                                new CodeStatement[]
                                {
                                new CodeConditionStatement(
                                    IsFieldNotNull(fi),
                                    new CodeStatement[]
                                    {
                                    new CodeAssignStatement(
                                        RefCacheExpression(ci, fi),
                                        new CodeMethodInvokeExpression(
                                            LoaderClass(fi.ReferencedClass),
                                            "GetRef",
                                            GetTransaction(),
                                            GetNotNullFieldValue(fi)
                                            )
                                        )

                                    })
                                }
                    ));


                    prop.GetStatements.Add(
                            new CodeMethodReturnStatement(
                                new CodeCastExpression(returnType,
                                    RefCacheExpression(ci, fi))));

                    // reference field setter
                    if (!classInfo.ReadOnly)
                    {
                        prop.SetStatements.Add(
                                new CodeExpressionStatement(

                                    new CodeMethodInvokeExpression(
                                        new CodeTypeReferenceExpression(typeof(Sooda.ObjectMapper.SoodaObjectImpl)), "SetRefFieldValue",

                                        // parameters
                                        new CodeThisReferenceExpression(),
                                        new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                                        new CodePrimitiveExpression(fi.Name),
                                        new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                                        new CodePropertySetValueReferenceExpression(),
                                        RefCacheArray(),
                                        new CodePrimitiveExpression(GetFieldRefCacheIndex(ci, fi)),
                                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(returnType.BaseType + "_Factory"), "TheFactory")
                                        )));
                    }
                }
                else
                {
                    // plain field getter

                    prop.GetStatements.Add(new CodeMethodReturnStatement(GetFieldValueForRead(fi)));
                    if (!classInfo.ReadOnly)
                    {
                        CodeExpression beforeDelegate = new CodePrimitiveExpression(null);
                        CodeExpression afterDelegate = new CodePrimitiveExpression(null);

                        if (classInfo.Triggers)
                        {
                            beforeDelegate = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(SoodaFieldUpdateDelegate)),
                                    new CodeThisReferenceExpression(), "BeforeFieldUpdate_" + fi.Name);
                            afterDelegate = new CodeDelegateCreateExpression(new CodeTypeReference(typeof(SoodaFieldUpdateDelegate)),
                                    new CodeThisReferenceExpression(), "AfterFieldUpdate_" + fi.Name);
                        }

                        prop.SetStatements.Add(
                                new CodeExpressionStatement(
                                    new CodeMethodInvokeExpression(
                                        new CodeTypeReferenceExpression(typeof(Sooda.ObjectMapper.SoodaObjectImpl)), "SetPlainFieldValue",

                                        // parameters
                                        new CodeThisReferenceExpression(),
                                        new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                                        new CodePrimitiveExpression(fi.Name),
                                        new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                                        Box(new CodePropertySetValueReferenceExpression()),
                                        beforeDelegate,
                                        afterDelegate
                                        )));
                    }
                }
            };


            if (classInfo.Collections1toN != null)
            {
                foreach (CollectionOnetoManyInfo coli in classInfo.Collections1toN)
                {
                    prop = new CodeMemberProperty();
                    prop.Name = coli.Name;
                    prop.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                    prop.Type = new CodeTypeReference(coli.ClassName + "List");

                    prop.GetStatements.Add(
                        new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(This, "_collectionCache_" + coli.Name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)), new CodeStatement[]
                            {
                                new CodeAssignStatement(
                                new CodeFieldReferenceExpression(This, "_collectionCache_" + coli.Name),
                                new CodeObjectCreateExpression(new CodeTypeReference(options.OutputNamespace + "." + coli.ClassName + "List"),
                                new CodeObjectCreateExpression(new CodeTypeReference(typeof(Sooda.ObjectMapper.SoodaObjectOneToManyCollection)),
                                new CodeExpression[] {
                                                         new CodeMethodInvokeExpression(This, "GetTransaction"),
                                                         new CodeTypeOfExpression(new CodeTypeReference(coli.ClassName)),
                                                         new CodeThisReferenceExpression(),
                                                         new CodePrimitiveExpression(coli.ForeignFieldName),
                                                         new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(coli.ClassName + "_Factory"), "TheClassInfo"),
                                                         new CodeFieldReferenceExpression(null, "_collectionWhere_" + coli.Name),
														 new CodePrimitiveExpression(coli.Cache)
                            }))
                                ),
                    }, new CodeStatement[] { }));

                    prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(This, "_collectionCache_" + coli.Name)));

                    ctd.Members.Add(prop);
                }
            }

            if (classInfo.CollectionsNtoN != null)
            {
                foreach (CollectionManyToManyInfo coli in classInfo.CollectionsNtoN)
                {
                    RelationInfo relationInfo = coli.GetRelationInfo();
                    // FieldInfo masterField = relationInfo.Table.Fields[1 - coli.MasterField];

                    string relationTargetClass = relationInfo.Table.Fields[coli.MasterField].References;

                    prop = new CodeMemberProperty();
                    prop.Name = coli.Name;
                    prop.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                    prop.Type = new CodeTypeReference(relationTargetClass + "List");

                    prop.GetStatements.Add(
                        new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(This, "_collectionCache_" + coli.Name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)), new CodeStatement[] {
                                                                                    new CodeAssignStatement(
                                                                                    new CodeFieldReferenceExpression(This, "_collectionCache_" + coli.Name),
                                                                                    new CodeObjectCreateExpression(new CodeTypeReference(relationTargetClass + "List"),
                                                                                    new CodeObjectCreateExpression(new CodeTypeReference(typeof(Sooda.ObjectMapper.SoodaObjectManyToManyCollection)),
                                                                                    new CodeExpression[] {
                                                                                                             new CodeMethodInvokeExpression(This, "GetTransaction"),
                                                                                                             new CodePrimitiveExpression(coli.MasterField),
                                                                                                             new CodeMethodInvokeExpression(This, "GetPrimaryKeyValue"),
                                                                                                             new CodeTypeOfExpression(relationInfo.Name + "_RelationTable"),
                                                                                                             new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(relationInfo.Name + "_RelationTable"), "theRelationInfo")

                                                                                                         }))
                                                                                    ),
                    }
                        , new CodeStatement[] { }));

                    prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(This, "_collectionCache_" + coli.Name)));

                    ctd.Members.Add(prop);
                }
            }
        }

        public void GenerateFields(CodeTypeDeclaration ctd, ClassInfo ci)
        {
            CodeMemberField field;

            if (GetFieldRefCacheCount(ci) > 0)
            {
                field = new CodeMemberField(new CodeTypeReference(new CodeTypeReference("SoodaObject"),1), "_refcache");
                field.Attributes = MemberAttributes.Private;
                field.InitExpression = new CodeArrayCreateExpression(
                    new CodeTypeReference(typeof(SoodaObject)), new CodePrimitiveExpression(GetFieldRefCacheCount(ci)));
                ctd.Members.Add(field);
            }

            foreach (FieldInfo fi in classInfo.LocalFields)
            {
                if (fi.References != null)
                {
                }
            }

            if (classInfo.Collections1toN != null)
            {
                foreach (CollectionOnetoManyInfo coli in classInfo.Collections1toN)
                {
                    field = new CodeMemberField(options.OutputNamespace + "." + coli.ClassName + "List", "_collectionCache_" + coli.Name);
                    field.Attributes = MemberAttributes.Assembly;
                    field.InitExpression = new CodePrimitiveExpression(null);
                    ctd.Members.Add(field);
                }
                foreach (CollectionOnetoManyInfo coli in classInfo.Collections1toN)
                {
                    field = new CodeMemberField("Sooda.SoodaWhereClause", "_collectionWhere_" + coli.Name);
                    field.Attributes = MemberAttributes.Static | MemberAttributes.Private;
                    if (coli.Where != null && coli.Where.Length > 0)
                    {
                        field.InitExpression = new CodeObjectCreateExpression(
                            "Sooda.SoodaWhereClause",
                            new CodePrimitiveExpression(coli.Where));
                    }
                    else
                    {
                        field.InitExpression = new CodePrimitiveExpression(null);
                    }
                    ctd.Members.Add(field);
                }
            }

            if (classInfo.CollectionsNtoN != null)
            {
                foreach (CollectionManyToManyInfo coli in classInfo.CollectionsNtoN)
                {
                    RelationInfo relationInfo = coli.GetRelationInfo();
                    field = new CodeMemberField(options.OutputNamespace + "." + relationInfo.Table.Fields[coli.MasterField].ReferencedClass.Name + "List", "_collectionCache_" + coli.Name);
                    field.Attributes = MemberAttributes.Private;
                    field.InitExpression = new CodePrimitiveExpression(null);
                    ctd.Members.Add(field);
                }
            }
        }
    }
}
