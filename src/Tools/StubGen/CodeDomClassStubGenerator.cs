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

            if (!ci.ReadOnly)
            {
                switch (ci.GetPrimaryKeyField().DataType)
                {
                    case FieldDataType.Integer:
                        keyGen = "integer";
                        break;

                    case FieldDataType.Guid:
                        keyGen = "guid";
                        break;
                }
            }

            if (ci.KeyGenName != null)
                keyGen = ci.KeyGenName;

            this.KeyGen = keyGen;
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
                        new CodePrimitiveExpression(classInfo.Name), 
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

        public CodeMemberField Field_precacheHash()
        {
            CodeMemberField field;

            field = new CodeMemberField("Hashtable", "precacheHash");
            field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
            field.InitExpression = new CodeObjectCreateExpression("Hashtable");
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
												new CodePropertyReferenceExpression(new CodeCastExpression(fi.References, Arg("oldValue")),s)),"InternalRemove",new CodeCastExpression(OutNamespace + "." + classInfo.Name,new CodeThisReferenceExpression()))
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
												new CodePropertyReferenceExpression(new CodeCastExpression(fi.References, Arg("newValue")),s)),"InternalAdd",new CodeCastExpression(OutNamespace + "." + classInfo.Name,new CodeThisReferenceExpression()))
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

        public CodeMemberMethod Method_GetPrimaryKeyValue(StubGenOptions options)
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetPrimaryKeyValue";
            method.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            method.ReturnType = new CodeTypeReference(typeof(object));
            if (options.NotNullRepresentation == PrimitiveRepresentation.SqlType)
            {
                method.Statements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(This, classInfo.GetPrimaryKeyField().Name), "Value")));
            }
            else
            {
                method.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(This, classInfo.GetPrimaryKeyField().Name)));
            }

            return method;
        }
        public CodeMemberMethod Method_GetKeyGenerator()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetKeyGenerator";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference("IPrimaryKeyGenerator");
            method.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "keyGenerator")));

            return method;
        }
        public CodeMemberMethod Method_InitNewObject()
        {
            CodeMemberMethod method;

            // protected override void InitNewObject()
            method = new CodeMemberMethod();
            method.Name = "InitNewObject";
            method.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            method.Statements.Add(
                new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(),
                "SetPrimaryKeyValue", 
                new CodeMethodInvokeExpression(new CodeMethodInvokeExpression(null, "GetKeyGenerator"), "GetNextKeyValue"))));

            return method;
        }
		public CodeMemberMethod Method_GetFieldHandler()
		{
			CodeMemberMethod method;

			method = new CodeMemberMethod();
			method.Name = "GetFieldHandler";
			method.Attributes = MemberAttributes.Family | MemberAttributes.Override;
			method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "ordinal"));
			method.ReturnType = new CodeTypeReference("Sooda.ObjectMapper.SoodaFieldHandler");

            if (classInfo.InheritsFromClass == null)
            {
                method.Statements.Add(                                        
                    new CodeMethodReturnStatement(
                    new CodeArrayIndexerExpression(
                    new CodeFieldReferenceExpression(null, "_fieldHandlers"),
                    new CodeArgumentReferenceExpression("ordinal")))
                    );
            }
            else
            {
                if (classInfo.LocalFields.Count > 0)
                {
                    int firstFieldOrdinal = classInfo.LocalFields[0].ClassUnifiedOrdinal;
                
                    method.Statements.Add(
                        new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                        Arg("ordinal"),
                        CodeBinaryOperatorType.GreaterThanOrEqual,
                        new CodePrimitiveExpression(firstFieldOrdinal)),
                        new CodeStatement[] {
                                                new CodeMethodReturnStatement(
                                                new CodeArrayIndexerExpression(
                                                new CodeFieldReferenceExpression(null, "_fieldHandlers"),
                                                new CodeBinaryOperatorExpression(new CodeArgumentReferenceExpression("ordinal"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression(firstFieldOrdinal))))
                                            },
                        new CodeStatement[] {
                                                new CodeMethodReturnStatement(
                                                new CodeMethodInvokeExpression(
                                                new CodeBaseReferenceExpression(),
                                                "GetFieldHandler",
                                                Arg("ordinal")))
                                            }));
                }
                else
                {
                    method.Statements.Add(new CodeMethodReturnStatement(
                                                new CodeMethodInvokeExpression(
                                                new CodeBaseReferenceExpression(),
                                                "GetFieldHandler",
                                                Arg("ordinal"))));

                }
            }

			return method;
		}
		public CodeMemberMethod Method_InitFields()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "InitFields";
            method.Attributes = MemberAttributes.Private | MemberAttributes.Static;

            method.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, "_fieldHandlers"), 
                new CodeArrayCreateExpression("SoodaFieldHandler", new CodePrimitiveExpression(classInfo.LocalFields.Count))));

            foreach (FieldInfo fi in classInfo.LocalFields)
            {
                method.Statements.Add(
                    new CodeAssignStatement(
                    new CodeArrayIndexerExpression(
                    new CodeFieldReferenceExpression(null, "_fieldHandlers"), 
                    new CodePrimitiveExpression(fi.ClassLocalOrdinal)
                    ), 
                    new CodeFieldReferenceExpression(null, "_fieldhandler_" + fi.Name)));
            }

            foreach (FieldInfo fi in classInfo.LocalFields)
            {
                method.Statements.Add(
                    new CodeAssignStatement(
                    new CodeArrayIndexerExpression(
                    new CodeFieldReferenceExpression(null, "_fieldHandlers"), 
                    new CodePrimitiveExpression(fi.ClassLocalOrdinal)
                    ), 
                    new CodeFieldReferenceExpression(null, "_fieldhandler_" + fi.Name)));
            }

            return method;
        }
        public CodeTypeConstructor Constructor_Class()
        {
            CodeTypeConstructor ctor = new CodeTypeConstructor();

            if (classInfo.PrecacheAll && classInfo.ReadOnly)
            {
                ctor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "PrecacheAll")));
            }
			ctor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "InitFields")));

            return ctor;
        }
        public CodeConstructor Constructor_Inserting()
        {
            CodeConstructor ctor = new CodeConstructor();

            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            ctor.BaseConstructorArgs.Add(Arg("tran"));

            if (classInfo.InheritsFromClass == null)
            {
                ctor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(This, "InitNewObject")));
            }

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
        public CodeMemberMethod Method_GetClassInfo()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetClassInfo";
            method.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            method.ReturnType = new CodeTypeReference("Sooda.Schema.ClassInfo");
            NoStepThrough(method.CustomAttributes);
            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"), "TheClassInfo")));

            return method;
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
                new CodeTypeReferenceExpression(classInfo.Name+"_Stub"), "GetRef", new CodePrimitiveExpression(val))));

            return prop;
        }

        private CodeTypeReference GetReturnType(PrimitiveRepresentation rep, FieldInfo fi)
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
            return new CodeArrayIndexerExpression(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(),
                "_fieldValues"),
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
            return 
                new CodeBinaryOperatorExpression(
                new CodeArrayIndexerExpression(
                new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(),
                "_fieldValues"),
                new CodePrimitiveExpression(fi.ClassUnifiedOrdinal)
                ),CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(null));
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
                string _db_FieldName = "_fieldhandler_" + fi.Name;

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

                if (classInfo.GetPrimaryKeyField() == fi)
                {
                    prop.GetStatements.Add(
                        new CodeMethodReturnStatement(
                        new CodeCastExpression(
                        prop.Type, 
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetPrimaryKeyValue"))));
                        
                    if (!classInfo.ReadOnly)
                    {
                        prop.SetStatements.Add(
                            new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "SetInitialPrimaryKeyValue", 
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
                        new CodeAssignStatement(
                        new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name),
                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Sooda.ObjectMapper.RefCache"), "GetOrCreateObject", 
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),"_refCache_" + fi.Name),
                            GetFieldValueExpression(fi),
                            new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetTransaction"),
                            new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(fi.References + "_Factory"), "TheFactory")
                        )));

                    prop.GetStatements.Add(
                        new CodeMethodReturnStatement(
                        new CodeCastExpression(fi.References, new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name))));

                    if (!classInfo.ReadOnly)
                    {
                        prop.SetStatements.Add(
                            new CodeAssignStatement(
                            new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name),

                            new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "SetRefFieldValue", 
                            
                            // parameters
                            new CodePrimitiveExpression(fi.Table.OrdinalInClass),
                            new CodePrimitiveExpression(fi.Name),
                            new CodePrimitiveExpression(fi.ClassUnifiedOrdinal),
                            new CodePropertySetValueReferenceExpression(), 
                            new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name),
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
                                new CodeFieldReferenceExpression(null, "_fieldhandler_" + fi.Name), "GetSqlNullableValue", GetFieldValueExpression(fi))));
                            break;

                        case PrimitiveRepresentation.Raw:
                            prop.GetStatements.Add(
                                new CodeMethodReturnStatement(
                                new CodeMethodInvokeExpression(
                                new CodeFieldReferenceExpression(null, "_fieldhandler_" + fi.Name), "GetNotNullValue", GetFieldValueExpression(fi)
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
                        new CodeFieldReferenceExpression(null, "_fieldhandler_" + fi.Name), "GetNotNullValue",
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
													 new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(coli.ClassName + "_Factory"),"TheClassInfo"),
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
                        new CodePrimitiveExpression(null)), new CodeStatement[] 
                        { 
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
                    }, new CodeStatement[] { }));

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

			field = new CodeMemberField(fieldArrayType , "_fieldHandlers");
			field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			field.InitExpression = new CodeArrayCreateExpression("Sooda.ObjectMapper.SoodaFieldHandler", new CodePrimitiveExpression(ci.LocalFields.Count));
			ctd.Members.Add(field);

            foreach (FieldInfo fi in classInfo.LocalFields)
            {
                string typeWrapper = fi.GetWrapperTypeName();
                bool isNullable = fi.IsNullable;

                field = new CodeMemberField(typeWrapper, "_fieldhandler_" + fi.Name);
                field.Attributes = MemberAttributes.Private | MemberAttributes.Static;
                field.InitExpression = new CodeObjectCreateExpression(typeWrapper, new CodePrimitiveExpression(isNullable));
                ctd.Members.Add(field);
            }

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

        private void MakeParameters(CodeMemberMethod method, int paramCount)
        {
            if (paramCount == -1)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object[]), "par"));
            }
            else
            {
                CodeExpression initexpr;

                if (paramCount == 0)
                {
                    initexpr = new CodePrimitiveExpression(null);
                }
                else
                {
                    initexpr = new CodeArrayCreateExpression(typeof(object), new CodePrimitiveExpression(paramCount));
                }
                method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                    new CodeTypeReference(new CodeTypeReference(typeof(object)),1),"par",
                    initexpr));

                for (int i = 0; i < paramCount; ++i)
                {
                    string varname = "p" + i.ToString();
                    method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), varname));
                    method.Statements.Add(new CodeAssignStatement(
                        new CodeArrayIndexerExpression(
                        new CodeVariableReferenceExpression("par"),
                        new CodePrimitiveExpression(i)),
                        new CodeArgumentReferenceExpression(varname)));
                }
            }
        }

        public CodeMemberMethod Method_GetList(bool withTransaction, bool withOrderBy, bool withOptions)
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetList";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            if (withTransaction)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            }
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaWhereClause", "where"));
            if (withOrderBy)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaOrderBy", "orderBy"));
            }
            if (withOptions)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaSnapshotOptions", "options"));
            }
            
            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(null, "DoGetList", 
                withTransaction ? (CodeExpression)Arg("tran") : (CodeExpression)new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("SoodaTransaction"), "ActiveTransaction"),
                Arg("where"),
                withOrderBy 
                    ? (CodeExpression)Arg("orderBy") 
                    : (CodeExpression)new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("SoodaOrderBy"),"Unsorted"),
                withOptions 
                    ? (CodeExpression)Arg("options") 
                    : (CodeExpression)new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("SoodaSnapshotOptions"),"Default"))));

            return method;
        }
        public CodeMemberMethod Method_DoGetList()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "DoGetList";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Private | MemberAttributes.Overloaded;
            method.ReturnType = new CodeTypeReference(classInfo.Name + "List");
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaWhereClause", "whereClause"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaOrderBy", "orderByClause"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaSnapshotOptions", "options"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeObjectCreateExpression(classInfo.Name + "List",  
				new CodeObjectCreateExpression(typeof(Sooda.ObjectMapper.SoodaObjectListSnapshot),
                Arg("tran"),
                Arg("whereClause"),
                Arg("orderByClause"),
                Arg("options"),
				new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"),"TheClassInfo")
				))));

            return method;
        }
        public CodeMemberMethod Method_Load1()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "Load";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(null, "Load", 
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("SoodaTransaction"), "ActiveTransaction"),
                Arg("val")
                )));

            return method;
        }
        public CodeMemberMethod Method_Load2()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "Load";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(new CodeVariableDeclarationStatement(method.ReturnType, "retVal", 
                new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(classInfo.Name + "_Stub"), "GetRef", 
                new CodeArgumentReferenceExpression("tran"),
                new CodeArgumentReferenceExpression("val"))));

            // TODO - this just makes sure that the first table is loaded
            // maybe we should support loading ALL tables here?

            method.Statements.Add(new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression("retVal"),
                "LoadAllData"
                ));

            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retVal")));

            return method;
        }
        public CodeMemberMethod Method_GetRefFromRecord()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetRefFromRecord";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression("System.Data.IDataRecord", "record"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "firstColumn"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(new CodeTypeReference(typeof(TableInfo)),1), "loadedTables"));

            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(object), "val",
            new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(classInfo.GetPrimaryKeyField().GetWrapperTypeName()),
                "GetBoxedFromReader",
                Arg("record"),
                new CodeBinaryOperatorExpression(
                    new CodeArgumentReferenceExpression("firstColumn"),
                    CodeBinaryOperatorType.Add,
                    new CodePrimitiveExpression(classInfo.GetPrimaryKeyField().ClassUnifiedOrdinal)
                        )
                )
                ));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeCastExpression(classInfo.Name,
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression(typeof(SoodaObject)),
                        "GetRefFromRecordHelper",
                        Arg("tran"),
                        new CodePropertyReferenceExpression(
                new CodeTypeReferenceExpression(classInfo.Name + "_Factory"), "TheFactory"),
                        new CodeVariableReferenceExpression("val"),
                Arg("record"),
                Arg("firstColumn"),
                Arg("loadedTables")))));
            return method;
        }
        public CodeMemberMethod Method_Get1()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetRef";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(null, "GetRef", 
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("SoodaTransaction"), "ActiveTransaction"),
                Arg("val")
                )));

            return method;
        }
        public CodeMemberMethod Method_TryGet1()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "TryGet";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            NoStepThrough(method.CustomAttributes);
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeMethodInvokeExpression(null, "TryGet", 
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("SoodaTransaction"), "ActiveTransaction"),
                Arg("val")
                )));

            return method;
        }
        public CodeMemberMethod Method_PrecacheAll()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "PrecacheAll";
            method.Attributes = MemberAttributes.Private | MemberAttributes.Static;

            method.Statements.Add(
                new CodeExpressionStatement(
                new CodeMethodInvokeExpression(null, "PrecacheHelper",
                new CodeFieldReferenceExpression(null, "precacheHash"),
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"),"TheFactory"),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(classInfo.Name + "_Factory"),"TheClassInfo")
                )));

            return method;
        }
        public CodeMemberMethod Method_PrecachedGet()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetRef";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeCastExpression(classInfo.Name,
                new CodeIndexerExpression(
                new CodeFieldReferenceExpression(null, "precacheHash"), Arg("val")))));
            return method;
        }
        public CodeMemberMethod Method_PrecachedTryGet()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "TryGet";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeCastExpression(classInfo.Name,
                new CodeIndexerExpression(
                new CodeFieldReferenceExpression(null, "precacheHash"), Arg("val")))));

            return method;
        }
        public CodeMemberMethod Method_NormalGet()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "GetRef";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(
                new CodeMethodReturnStatement(
                new CodeCastExpression(classInfo.Name,
                new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(typeof(SoodaObject)),
                "GetRefHelper",
                Arg("tran"),
                new CodePropertyReferenceExpression(
                new CodeTypeReferenceExpression(classInfo.Name + "_Factory"), "TheFactory"),
                new CodeVariableReferenceExpression("val")
                ))));

            return method;
        }
        public CodeMemberMethod Method_NormalTryGet()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "TryGet";
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public | MemberAttributes.Overloaded;
            NoStepThrough(method.CustomAttributes);
            if (classInfo.InheritFrom != null)
            {
                method.Attributes |= MemberAttributes.New;
            }
            method.ReturnType = new CodeTypeReference(classInfo.Name);

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "tran"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(ctr, "val"));

            method.Statements.Add(new CodeMethodReturnStatement(new CodeCastExpression(classInfo.Name,
                new CodeMethodInvokeExpression(
                Arg("tran"),
                "FindObjectWithKey",
                new CodePrimitiveExpression(classInfo.GetRootClass().Name),
                Arg("val"),
                new CodeTypeOfExpression(classInfo.Name)
                ))));

            return method;
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
                new CodePrimitiveExpression(null)),new CodeMethodReturnStatement()));

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
                    new CodeAssignStatement(
                    new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name),
                    new CodeDelegateInvokeExpression(
                    new CodeArgumentReferenceExpression("iter"),
                    new CodeThisReferenceExpression(),
                    new CodePrimitiveExpression(fi.Name),
                    GetFieldValueExpression(fi),
                    GetFieldIsDirtyExpression(fi),
                    new CodeFieldReferenceExpression(This, "_refCache_" + fi.Name),
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

        public CodeMemberMethod Method_SetPrimaryKeyValue()
        {
            CodeMemberMethod method;

            method = new CodeMemberMethod();
            method.Name = "SetPrimaryKeyValue";
            method.Attributes = MemberAttributes.Override | MemberAttributes.Family;

            CodeTypeReference ctr = new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType).FullName);
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "o"));

            if (classInfo.ReadOnly)
            {
                method.Statements.Add(new CodeThrowExceptionStatement(
                    new CodeObjectCreateExpression("NotSupportedException", 
                    new CodePrimitiveExpression("SetPrimaryKeyValue() is not supported on read-only objects of type " + classInfo.Name))));
            }
            else
            {
                method.Statements.Add(
                    new CodeAssignStatement(
                    new CodePropertyReferenceExpression(This, classInfo.GetPrimaryKeyField().Name),
                    new CodeCastExpression(
                    new CodeTypeReference(FieldDataTypeHelper.GetClrType(classInfo.GetPrimaryKeyField().DataType)),
                    new CodeVariableReferenceExpression("o"))));
            };

            return method;
        }
    }
}
