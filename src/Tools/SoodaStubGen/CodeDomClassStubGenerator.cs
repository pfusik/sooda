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

// #define EMPTYACCESSORS

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;
using Sooda.StubGen.CDIL;

namespace Sooda.StubGen 
{
    public class CodeDomClassStubGenerator : CodeDomHelpers 
    {
        private ClassInfo classInfo;
        public readonly string KeyGen;

        public CodeDomClassStubGenerator(ClassInfo ci) 
        {
            this.classInfo = ci;
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
                        new CodeTypeReferenceExpression("_DatabaseSchema"), "GetSchema"),
                        "GetDataSourceInfo",
                        new CodePrimitiveExpression("default")));
                    break;
                case "long":
                    field.InitExpression = new CodeObjectCreateExpression("Sooda.ObjectMapper.KeyGenerators.TableBasedGeneratorBigint",
                        new CodePrimitiveExpression(GetRootClass(classInfo).Name),
                        new CodeMethodInvokeExpression(
                        new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("_DatabaseSchema"), "GetSchema"),
                        "GetDataSourceInfo",
                        new CodePrimitiveExpression("default")));
                    break;

                default:
                    field.InitExpression = new CodeObjectCreateExpression(KeyGen);
                    break;
            }
            return field;
        }

        public CodeMemberMethod Method_BeforeCollectionUpdate(FieldInfo fi, string OutNamespace) 
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "BeforeCollectionUpdate_" + fi.Name;
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaObject", "oldValue"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaObject", "newValue"));
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            if (fi.BackRefCollections != null) 
            {
                foreach (string s in fi.BackRefCollections) 
                {
                    method.Statements.Add(new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(Arg("oldValue"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                        new CodeStatement[] {
                                                new CodeExpressionStatement(
                                                new CodeMethodInvokeExpression(
                                                new CodeCastExpression(typeof(Sooda.ObjectMapper.ISoodaObjectListInternal),
                                                new CodePropertyReferenceExpression(new CodeCastExpression(fi.References, Arg("oldValue")), s)), "InternalRemove", new CodeCastExpression(OutNamespace + "." + classInfo.Name, new CodeThisReferenceExpression()))
                                                )
                                            },
                        new CodeStatement[] {
                                            }
                        ));
                };
            }

            return method;
        }

        public CodeMemberMethod Method_AfterCollectionUpdate(FieldInfo fi, string OutNamespace) 
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "AfterCollectionUpdate_" + fi.Name;
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaObject", "oldValue"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaObject", "newValue"));
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            if (fi.BackRefCollections != null) 
            {
                foreach (string s in fi.BackRefCollections) 
                {
                    method.Statements.Add(new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(Arg("newValue"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                        new CodeStatement[] {
                                                new CodeExpressionStatement(
                                                new CodeMethodInvokeExpression(
                                                new CodeCastExpression(typeof(Sooda.ObjectMapper.ISoodaObjectListInternal),
                                                new CodePropertyReferenceExpression(new CodeCastExpression(fi.References, Arg("newValue")), s)), "InternalAdd", new CodeCastExpression(OutNamespace + "." + classInfo.Name, new CodeThisReferenceExpression()))
                                                )
                                            },
                        new CodeStatement[] {
                                            }
                        ));
                }
            }

            return method;
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
                new CodeTypeReferenceExpression(classInfo.Name + "_Stub"), "GetRef", new CodePrimitiveExpression(val))));

            return prop;
        }

        public CodeTypeReference GetReturnType(PrimitiveRepresentation rep, FieldInfo fi) 
        {
            switch (rep) 
            {
                case PrimitiveRepresentation.Boxed:
                    return new CodeTypeReference(typeof(object));

                case PrimitiveRepresentation.SqlType:
                    Type t = FieldDataTypeHelper.GetSqlType(fi.DataType);
                    if (t == null)
                        return new CodeTypeReference(FieldDataTypeHelper.GetClrType(fi.DataType));
                    else
                        return new CodeTypeReference(t);

                case PrimitiveRepresentation.Raw:
                    return new CodeTypeReference(FieldDataTypeHelper.GetClrType(fi.DataType));

                default:
                    throw new NotImplementedException("Unknown PrimitiveRepresentation: " + rep);
            }
        }

        private CodeExpression SetterPrimitiveValue(PrimitiveRepresentation rep, CodeTypeReference returnType) 
        {
            switch (rep) 
            {
                case PrimitiveRepresentation.SqlType:
                    return new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), "Value");

                case PrimitiveRepresentation.Boxed:
                    return new CodeCastExpression(returnType, new CodePropertySetValueReferenceExpression());

                default:
                    return new CodePropertySetValueReferenceExpression();
            }
        }

        private CodeExpression IsSetterValueNull(PrimitiveRepresentation rep) 
        {
            switch (rep) 
            {
                case PrimitiveRepresentation.SqlType:
                    return new CodePropertyReferenceExpression(new CodePropertySetValueReferenceExpression(), "IsNull");

                case PrimitiveRepresentation.Boxed:
                    return new CodeBinaryOperatorExpression(
                        new CodePropertySetValueReferenceExpression(),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null));

                case PrimitiveRepresentation.Raw:
                    return new CodePrimitiveExpression(false);

                default:
                    throw new NotSupportedException();
            }
        }

        private CodeExpression GetFieldValueExpression(FieldInfo fi) 
        {
            return new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(),
                "_fieldValues"),
                "GetBoxedFieldValue",
                new CodePrimitiveExpression(fi.ClassUnifiedOrdinal));
        }

        private CodeExpression GetFieldIsDirtyExpression(FieldInfo fi) 
        {
            return new CodePropertyReferenceExpression(GetFieldDataExpression(fi), "IsDirty");
        }

        private CodeExpression GetFieldDataExpression(FieldInfo fi) 
        {
            return new CodeArrayIndexerExpression(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(),
                "_fieldData"),
                new CodePrimitiveExpression(fi.ClassUnifiedOrdinal));
        }

        private CodeExpression GetFieldIsNullExpression(FieldInfo fi) 
        {
            return new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(),
                "_fieldValues"),
                "IsNull",
                new CodePrimitiveExpression(fi.ClassUnifiedOrdinal));
        }

        private CodeMemberProperty _IsNull(FieldInfo fi) 
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = "_IsNull_" + fi.Name;
            prop.Attributes = MemberAttributes.Final | MemberAttributes.Public;
            prop.Type = new CodeTypeReference(typeof(bool));

            prop.GetStatements.Add(
                new CodeMethodReturnStatement(
                GetFieldIsNullExpression(fi)));

            return prop;
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
        public void GenerateProperties(CodeTypeDeclaration ctd, ClassInfo ci, string OutNamespace, StubGenOptions options) 
        {
            CodeMemberProperty prop;

            foreach (FieldInfo fi in classInfo.LocalFields) 
            {
                if (fi.References != null)
                    continue;

                if (fi.IsNullable) 
                {
                    if (options.NullableRepresentation == PrimitiveRepresentation.Raw) 
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
                    if (options.NotNullRepresentation == PrimitiveRepresentation.Raw) 
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

                if (FieldDataTypeHelper.GetSqlType(fi.DataType) == null) 
                {
                    if (actualNotNullRepresentation == PrimitiveRepresentation.SqlType)
                        actualNotNullRepresentation = PrimitiveRepresentation.Raw;

                    if (actualNullableRepresentation == PrimitiveRepresentation.SqlType)
                        actualNullableRepresentation = PrimitiveRepresentation.Raw;
                }

                CodeTypeReference returnType;
                CodeTypeReference rawReturnType;

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
                rawReturnType = GetReturnType(PrimitiveRepresentation.Raw, fi);

                prop = new CodeMemberProperty();
                prop.Name = fi.Name;
                prop.Attributes = MemberAttributes.Final | MemberAttributes.Public;
                prop.Type = returnType;
                //prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "_FieldNames")));
                ctd.Members.Add(prop);

                if (fi.IsPrimaryKey)
                {
                    CodeExpression getPrimaryKeyValue = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetPrimaryKeyValue");

                    if (classInfo.GetPrimaryKeyFields().Length > 1)
                    {
                        getPrimaryKeyValue = new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(typeof(SoodaTuple)), "GetValue", getPrimaryKeyValue, new CodePrimitiveExpression(primaryKeyComponentNumber));
                        primaryKeyComponentNumber++;
                    }

                    prop.GetStatements.Add(
                        new CodeMethodReturnStatement(
                        new CodeCastExpression(
                        prop.Type,
                        getPrimaryKeyValue
                        )));

                    if (!classInfo.ReadOnly) 
                    {
                        prop.SetStatements.Add(
                            new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "SetPrimaryKeyValue",
                            new CodePropertySetValueReferenceExpression())));
                    }
                    continue;
                }

                if (options.NullPropagation && (fi.References != null || fi.IsNullable) && (actualNullableRepresentation != PrimitiveRepresentation.Raw)) 
                {
                    CodeExpression retVal = new CodePrimitiveExpression(null);

                    if (fi.References == null && actualNullableRepresentation == PrimitiveRepresentation.SqlType) 
                    {
                        retVal = new CodePropertyReferenceExpression(
                            new CodeTypeReferenceExpression(FieldDataTypeHelper.GetSqlType(fi.DataType)), "Null");
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

                int tableNumber = fi.Table.OrdinalInClass;

                prop.GetStatements.Add(
                    new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(), "EnsureDataLoaded", new CodePrimitiveExpression(tableNumber))));

                if (fi.References != null) 
                {
                    prop.GetStatements.Add(
                        new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Sooda.ObjectMapper.RefCache"), "GetOrCreateObject",
                        new CodeDirectionExpression(FieldDirection.Ref, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_refCache_" + fi.Name)),
                        new CodeFieldReferenceExpression(This, "_fieldValues"),
                        new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetTransaction"),
                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(fi.References + "_Factory"), "TheFactory")
                        )));

                    prop.GetStatements.Add(
                        new CodeMethodReturnStatement(
                        new CodeCastExpression(fi.References, new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name))));

                    if (!classInfo.ReadOnly) 
                    {
                        prop.SetStatements.Add(
                            new CodeExpressionStatement(

                            new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "SetRefFieldValue",

                            // parameters
                            new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                            new CodePrimitiveExpression(fi.Name),
                            new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                            new CodePropertySetValueReferenceExpression(),
                            new CodeDirectionExpression(FieldDirection.Ref, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_refCache_" + fi.Name)),
                            new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(returnType.BaseType + "_Factory"), "TheFactory"
                            ))));
                    }
                } 
                else if (fi.IsNullable) 
                {
                    switch (actualNullableRepresentation) 
                    {
                        case PrimitiveRepresentation.Boxed:
                            prop.GetStatements.Add(
                                new CodeMethodReturnStatement(
                                GetFieldValueExpression(fi)));
                            break;

                        case PrimitiveRepresentation.SqlType:
                            prop.GetStatements.Add(
                                new CodeMethodReturnStatement(
                                new CodeMethodInvokeExpression(
                                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"), "_fieldhandler_" + fi.Name), "GetSqlNullableValue", GetFieldValueExpression(fi))));
                            break;

                        case PrimitiveRepresentation.Raw:
                            prop.GetStatements.Add(
                                new CodeMethodReturnStatement(
                                new CodeMethodInvokeExpression(
                                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"), "_fieldhandler_" + fi.Name), "GetNotNullValue", GetFieldValueExpression(fi)
                                )));
                            break;

                        default:
                            Console.WriteLine("Nullable representation " + actualNullableRepresentation + " not supported");
                            break;
                    }
                    if (!classInfo.ReadOnly) 
                    {
                        CodeStatement whenNull =
                            new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "SetPlainFieldValue",

                            // parameters
                            new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                            new CodePrimitiveExpression(fi.Name),
                            new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                            new CodePrimitiveExpression(null)
                            ));

                        CodeStatement whenNotNull =
                            new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "SetPlainFieldValue",

                            // parameters
                            new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                            new CodePrimitiveExpression(fi.Name),
                            new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                            SetterPrimitiveValue(actualNullableRepresentation, rawReturnType)
                            ));

                        if (actualNullableRepresentation != PrimitiveRepresentation.Raw) 
                        {
                            prop.SetStatements.Add(
                                new CodeConditionStatement(
                                IsSetterValueNull(actualNullableRepresentation),
                                new CodeStatement[] { whenNull },
                                new CodeStatement[] { whenNotNull }
                                ));
                        } 
                        else 
                        {
                            prop.SetStatements.Add(whenNotNull);
                        }
                    }
                } 
                else 
                {
                    prop.GetStatements.Add(
                        new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"), "_fieldhandler_" + fi.Name), "GetNotNullValue",
                        GetFieldValueExpression(fi))));
                    if (!classInfo.ReadOnly) 
                    {
                        // SetPlainFieldValue("FIELD_NAME", _db_fieldhandler_NAME, value); // box here

                        CodeStatement setStat =
                            new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                            null, "SetPlainFieldValue",

                            // parameters
                            new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                            new CodePrimitiveExpression(fi.Name),
                            new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                            SetterPrimitiveValue(actualNotNullRepresentation, rawReturnType)
                            ));

                        prop.SetStatements.Add(setStat);
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
                                new CodeObjectCreateExpression(new CodeTypeReference(OutNamespace + "." + coli.ClassName + "List"),
                                new CodeObjectCreateExpression(new CodeTypeReference(typeof(Sooda.ObjectMapper.SoodaObjectOneToManyCollection)),
                                new CodeExpression[] {
                                                         new CodeMethodInvokeExpression(This, "GetTransaction"),
                                                         new CodeTypeOfExpression(new CodeTypeReference(coli.ClassName)),
                                                         new CodeThisReferenceExpression(),
                                                         new CodePrimitiveExpression(coli.ForeignFieldName),
                                                         new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(coli.ClassName + "_Factory"), "TheClassInfo"),
                                                         new CodeFieldReferenceExpression(null, "_collectionWhere_" + coli.Name),
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
                    FieldInfo masterField = null;
                    FieldInfo slaveField = null;
                    string collectionClassName = null;

                    if (coli.MasterField == 1) 
                    {
                        masterField = relationInfo.Table.Fields[0];
                        slaveField = relationInfo.Table.Fields[1];
                        collectionClassName = relationInfo.Name + "_R_List";
                    } 
                    else 
                    {
                        slaveField = relationInfo.Table.Fields[0];
                        masterField = relationInfo.Table.Fields[1];
                        collectionClassName = relationInfo.Name + "_L_List";
                    };

                    string relationTargetClass = slaveField.References;
                    string relationHelperClass = OutNamespace + ".Stubs." + collectionClassName;

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

        public void GenerateFields(CodeTypeDeclaration ctd, ClassInfo ci, string OutNamespace) 
        {
            CodeMemberField field;
            CodeTypeReference fieldArrayType = new CodeTypeReference(
                "Sooda.ObjectMapper.SoodaFieldHandler", 1);

            foreach (FieldInfo fi in classInfo.LocalFields) 
            {
                if (fi.References != null) 
                {
                    field = new CodeMemberField("SoodaObject", "_refCache_" + fi.Name);
                    field.Attributes = MemberAttributes.Private;
                    ctd.Members.Add(field);
                }
            }

            if (classInfo.Collections1toN != null) 
            {
                foreach (CollectionOnetoManyInfo coli in classInfo.Collections1toN) 
                {
                    field = new CodeMemberField(OutNamespace + "." + coli.ClassName + "List", "_collectionCache_" + coli.Name);
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
                    string collectionClassName = null;

                    if (coli.MasterField == 1) 
                    {
                        collectionClassName = relationInfo.Name + "_R_List";
                    } 
                    else 
                    {
                        collectionClassName = relationInfo.Name + "_L_List";
                    };

                    field = new CodeMemberField(OutNamespace + "." + relationInfo.Table.Fields[coli.MasterField].ReferencedClass.Name + "List", "_collectionCache_" + coli.Name);
                    field.Attributes = MemberAttributes.Private;
                    field.InitExpression = new CodePrimitiveExpression(null);
                    ctd.Members.Add(field);
                }
            }
        }

        public CodeMemberMethod Method_IterateOuterReferences() 
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "IterateOuterReferences";
            method.Parameters.Add(new CodeParameterDeclarationExpression("Sooda.ObjectMapper.SoodaObjectRefFieldIterator", "iter"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "context"));
            method.Attributes = MemberAttributes.Override | MemberAttributes.Family;

            method.Statements.Add(
                new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_fieldData"),
                CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(null)), new CodeMethodReturnStatement()));

            method.Statements.Add(
                new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                new CodeBaseReferenceExpression(),
                "IterateOuterReferences",
                Arg("iter"),
                Arg("context"))));

            bool any = false;
            foreach (FieldInfo fi in classInfo.LocalFields) 
            {
                if (fi.References == null)
                    continue;

                method.Statements.Add(
                    new CodeExpressionStatement(
                    new CodeDelegateInvokeExpression(
                    new CodeArgumentReferenceExpression("iter"),
                    new CodeThisReferenceExpression(),
                    new CodePrimitiveExpression(fi.Name),
                    GetFieldValueExpression(fi),
                    GetFieldIsDirtyExpression(fi),
                    new CodeDirectionExpression(FieldDirection.Ref, new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_refCache_" + fi.Name)),
                    new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(fi.References + "_Factory"), "TheFactory"),
                    new CodeArgumentReferenceExpression("context")
                    )));
                any = true;
            }

            if (any)
                return method;
            else
                return null;
        }
    }
}
