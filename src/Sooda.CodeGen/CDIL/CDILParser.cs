//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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
using System.CodeDom;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Sooda.CodeGen.CDIL
{
    public class CDILParser : CDILTokenizer
    {
        public CDILParser(string txt, CDILContext context)
            : base(Preprocess(txt, context))
        {
        }

        private static string Preprocess(string txt, CDILContext context)
        {
            StringReader sr = new StringReader(txt);
            StringWriter sw = new StringWriter();

            string line;
            bool skip = false;
            Stack skipStack = new Stack();

            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("#ifnot "))
                {
                    string conditionalName = line.Substring(7);
                    object o = context[conditionalName];
                    if (o == null)
                        throw new ArgumentException("No such conditional: " + conditionalName);
                    bool v = Convert.ToBoolean(o) || skip;
                    skipStack.Push(skip);
                    skip = v;
                    continue;
                }
                if (line.StartsWith("#if "))
                {
                    string conditionalName = line.Substring(4);
                    object o = context[conditionalName];
                    if (o == null)
                        throw new ArgumentException("No such conditional: " + conditionalName);
                    bool v = !Convert.ToBoolean(o) || skip;
                    skipStack.Push(skip);
                    skip = v;
                    continue;
                }
                if (line.StartsWith("#endif"))
                {
                    skip = (bool)skipStack.Pop();
                    continue;
                }
                if (line.StartsWith("#else"))
                {
                    skip = !skip;
                    continue;
                }
                if (!skip)
                    sw.WriteLine(line);
            }

            // Console.WriteLine("Preprocessed as {0}", sw);

            return sw.ToString();
        }

        public CodeExpression ParseBaseExpression()
        {
            if (TokenType == CDILToken.Integer)
            {
                CodePrimitiveExpression expr = new CodePrimitiveExpression(TokenValue);
                GetNextToken();
                return expr;
            }
            if (TokenType == CDILToken.String)
            {
                CodePrimitiveExpression expr = new CodePrimitiveExpression(TokenValue);
                GetNextToken();
                return expr;
            }
            if (IsKeyword("base"))
            {
                GetNextToken();
                return new CodeBaseReferenceExpression();
            }

            if (IsKeyword("null"))
            {
                GetNextToken();
                return new CodePrimitiveExpression(null);
            }

            if (IsKeyword("false"))
            {
                GetNextToken();
                return new CodePrimitiveExpression(false);
            }

            if (IsKeyword("true"))
            {
                GetNextToken();
                return new CodePrimitiveExpression(true);
            }

            if (IsKeyword("this"))
            {
                GetNextToken();
                return new CodeThisReferenceExpression();
            }

            if (IsKeyword("setvalue"))
            {
                GetNextToken();
                return new CodePropertySetValueReferenceExpression();
            }

            if (IsKeyword("arg"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                string name = EatKeyword();
                Expect(CDILToken.RightParen);
                return new CodeArgumentReferenceExpression(name);
            }

            if (IsKeyword("delegatecall"))
            {
                CodeDelegateInvokeExpression retval = new CodeDelegateInvokeExpression();
                GetNextToken();
                Expect(CDILToken.LeftParen);
                retval.TargetObject = ParseExpression();
                Expect(CDILToken.RightParen);
                Expect(CDILToken.LeftParen);
                while (TokenType != CDILToken.RightParen && TokenType != CDILToken.EOF)
                {
                    CodeExpression expr = ParseExpression();
                    retval.Parameters.Add(expr);
                    if (TokenType == CDILToken.Comma)
                    {
                        GetNextToken();
                    }
                }
                Expect(CDILToken.RightParen);
                return retval;
            }

            if (IsKeyword("typeref"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeTypeReference typeRef = ParseType();
                Expect(CDILToken.RightParen);
                return new CodeTypeReferenceExpression(typeRef);
            }

            if (IsKeyword("typeof"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeTypeReference typeRef = ParseType();
                Expect(CDILToken.RightParen);
                return new CodeTypeOfExpression(typeRef);
            }

            if (IsKeyword("add"))
            {
                CodeBinaryOperatorExpression cboe = new CodeBinaryOperatorExpression();
                cboe.Operator = CodeBinaryOperatorType.Add;
                GetNextToken();
                Expect(CDILToken.LeftParen);
                cboe.Left = ParseExpression();
                Expect(CDILToken.Comma);
                cboe.Right = ParseExpression();
                Expect(CDILToken.RightParen);
                return cboe;
            }

            if (IsKeyword("equal"))
            {
                CodeBinaryOperatorExpression cboe = new CodeBinaryOperatorExpression();
                cboe.Operator = CodeBinaryOperatorType.ValueEquality;
                GetNextToken();
                Expect(CDILToken.LeftParen);
                cboe.Left = ParseExpression();
                Expect(CDILToken.Comma);
                cboe.Right = ParseExpression();
                Expect(CDILToken.RightParen);
                return cboe;
            }

            if (IsKeyword("refequal"))
            {
                CodeBinaryOperatorExpression cboe = new CodeBinaryOperatorExpression();
                cboe.Operator = CodeBinaryOperatorType.IdentityEquality;
                GetNextToken();
                Expect(CDILToken.LeftParen);
                cboe.Left = ParseExpression();
                Expect(CDILToken.Comma);
                cboe.Right = ParseExpression();
                Expect(CDILToken.RightParen);
                return cboe;
            }

            if (IsKeyword("refnotequal"))
            {
                CodeBinaryOperatorExpression cboe = new CodeBinaryOperatorExpression();
                cboe.Operator = CodeBinaryOperatorType.IdentityInequality;
                GetNextToken();
                Expect(CDILToken.LeftParen);
                cboe.Left = ParseExpression();
                Expect(CDILToken.Comma);
                cboe.Right = ParseExpression();
                Expect(CDILToken.RightParen);
                return cboe;
            }

            if (IsKeyword("arrayitem"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeArrayIndexerExpression caie = new CodeArrayIndexerExpression();
                caie.TargetObject = ParseExpression();
                while (TokenType == CDILToken.Comma)
                {
                    Expect(CDILToken.Comma);
                    caie.Indices.Add(ParseExpression());
                }
                Expect(CDILToken.RightParen);
                return caie;
            }

            if (IsKeyword("index"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeIndexerExpression cie = new CodeIndexerExpression();
                cie.TargetObject = ParseExpression();
                while (TokenType == CDILToken.Comma)
                {
                    Expect(CDILToken.Comma);
                    cie.Indices.Add(ParseExpression());
                }
                Expect(CDILToken.RightParen);
                return cie;
            }

            if (IsKeyword("var"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                string name = EatKeyword();
                Expect(CDILToken.RightParen);
                return new CodeVariableReferenceExpression(name);
            }

            if (IsKeyword("defaultscope"))
            {
                GetNextToken();
                return null;
            }

            if (IsKeyword("ref"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeExpression expr = ParseExpression();
                Expect(CDILToken.RightParen);
                return new CodeDirectionExpression(FieldDirection.Ref, expr);
            }

            if (IsKeyword("out"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeExpression expr = ParseExpression();
                Expect(CDILToken.RightParen);
                return new CodeDirectionExpression(FieldDirection.Out, expr);
            }

            if (IsKeyword("cast"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeTypeReference type = ParseType();
                Expect(CDILToken.Comma);
                CodeExpression expr = ParseExpression();
                Expect(CDILToken.RightParen);
                return new CodeCastExpression(type, expr);
            }
            if (IsKeyword("new"))
            {
                GetNextToken();
                CodeTypeReference type = ParseType();
                CodeObjectCreateExpression retval = new CodeObjectCreateExpression(type);
                Expect(CDILToken.LeftParen);
                while (TokenType != CDILToken.RightParen && TokenType != CDILToken.EOF)
                {
                    CodeExpression expr = ParseExpression();
                    retval.Parameters.Add(expr);
                    if (TokenType == CDILToken.Comma)
                    {
                        GetNextToken();
                    }
                }
                Expect(CDILToken.RightParen);
                return retval;
            }

            if (IsKeyword("newarray"))
            {
                GetNextToken();
                Expect(CDILToken.LeftParen);
                CodeArrayCreateExpression retval = new CodeArrayCreateExpression();
                retval.CreateType = ParseType();
                Expect(CDILToken.Comma);
                retval.SizeExpression = ParseExpression();
                Expect(CDILToken.RightParen);
                return retval;
            }

            throw BuildException("Unexpected token '" + TokenType + "'");
        }

        public CodeExpression ParseExpression()
        {
            CodeExpression currentValue = ParseBaseExpression();
            while (TokenType == CDILToken.Dot)
            {
                GetNextToken();
                string name = EatKeyword();
                if (TokenType == CDILToken.LeftParen)
                {
                    CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(currentValue, name);
                    GetNextToken();
                    while (TokenType != CDILToken.RightParen && TokenType != CDILToken.EOF)
                    {
                        CodeExpression expr = ParseExpression();
                        methodInvoke.Parameters.Add(expr);
                        if (TokenType == CDILToken.Comma)
                        {
                            GetNextToken();
                        }
                    }
                    Expect(CDILToken.RightParen);
                    currentValue = methodInvoke;
                    continue;
                }
                else if (TokenType == CDILToken.Dollar)
                {
                    GetNextToken();
                    currentValue = new CodeFieldReferenceExpression(currentValue, name);
                    continue;
                }
                else
                {
                    currentValue = new CodePropertyReferenceExpression(currentValue, name);
                }
            }
            return currentValue;
        }

        public CodeTypeReference ParseType()
        {
            string name = this.EatKeyword();
            if (name == "arrayof")
            {
                Expect(CDILToken.LeftParen);
                CodeTypeReference arrayItemType = ParseType();
                Expect(CDILToken.RightParen);
                return new CodeTypeReference(arrayItemType, 1);
            }
            if (name == "generic")
            {
                Expect(CDILToken.LeftParen);
                CodeTypeReference genericType = ParseType();
                System.Collections.Generic.List<CodeTypeReference> typeArguments = new System.Collections.Generic.List<CodeTypeReference>();
                while (TokenType == CDILToken.Comma)
                {
                    GetNextToken();
                    typeArguments.Add(ParseType());
                }
                Expect(CDILToken.RightParen);
                return new CodeTypeReference(genericType.BaseType, typeArguments.ToArray());
            }
            while (TokenType == CDILToken.Dot)
            {
                GetNextToken();
                name += "." + this.EatKeyword();
            }

            return new CodeTypeReference(name);
        }

        public TypeAttributes ParseTypeAttributes()
        {
            TypeAttributes retval = (TypeAttributes) 0;
            for (;;)
            {
                string keyword = EatKeyword();
                retval |= (TypeAttributes) Enum.Parse(typeof(TypeAttributes), keyword, true);
                if (TokenType != CDILToken.Comma)
                    return retval;
                GetNextToken();
            }
        }

        public MemberAttributes ParseMemberAttributes()
        {
            MemberAttributes retval = (MemberAttributes)0;

            while (true)
            {
                string keyword = EatKeyword();
                MemberAttributes v = (MemberAttributes)Enum.Parse(typeof(MemberAttributes), keyword, true);
                retval |= v;
                if (TokenType == CDILToken.Comma)
                {
                    GetNextToken();
                    continue;
                }
                else
                {
                    break;
                }
            }

            return retval;
        }

        public CodeTypeMember ParseMember()
        {
            if (IsKeyword("constructor"))
                return ParseConstructor();
            if (IsKeyword("method"))
                return ParseMethod();
            if (IsKeyword("field"))
                return ParseField();
            if (IsKeyword("property"))
                return ParseProperty();
            if (IsKeyword("typeconstructor"))
                return ParseTypeConstructor();
            throw BuildException("Unknown member: " + TokenType + "(" + TokenValue + ")");
        }

        public CodeMemberField ParseField()
        {
            CodeMemberField field = new CodeMemberField();
            ExpectKeyword("field");
            field.Type = ParseType();
            field.Name = EatKeyword();
            if (IsKeyword("attributes"))
            {
                GetNextToken();
                field.Attributes = ParseMemberAttributes();
            }
            if (IsKeyword("value"))
            {
                GetNextToken();
                field.InitExpression = ParseExpression();
            }
            ExpectKeyword("end");
            return field;
        }

        public CodeMemberProperty ParseProperty()
        {
            CodeMemberProperty property = new CodeMemberProperty();
            ExpectKeyword("property");
            property.Type = ParseType();
            property.Name = EatKeyword();
            if (TokenType == CDILToken.LeftParen)
            {
                Expect(CDILToken.LeftParen);
                while (TokenType != CDILToken.RightParen && TokenType != CDILToken.EOF)
                {
                    CodeTypeReference typeName = ParseType();
                    string varName = EatKeyword();
                    property.Parameters.Add(new CodeParameterDeclarationExpression(typeName, varName));
                    if (TokenType == CDILToken.Comma)
                        GetNextToken();
                }
                Expect(CDILToken.RightParen);
            }
            if (IsKeyword("attributes"))
            {
                GetNextToken();
                property.Attributes = ParseMemberAttributes();
            }
            if (IsKeyword("implements"))
            {
                GetNextToken();
                property.ImplementationTypes.Add(ParseType());
            }
            if (IsKeyword("get"))
            {
                GetNextToken();
                property.HasGet = true;
                while (!IsKeyword("end") && !IsKeyword("set") && TokenType != CDILToken.EOF)
                {
                    property.GetStatements.Add(ParseStatement());
                    if (TokenType == CDILToken.Semicolon)
                        Expect(CDILToken.Semicolon);
                    else
                        break;
                }
            }
            if (IsKeyword("set"))
            {
                GetNextToken();
                property.HasSet = true;
                while (!IsKeyword("end") && TokenType != CDILToken.EOF)
                {
                    property.SetStatements.Add(ParseStatement());
                    if (TokenType == CDILToken.Semicolon)
                        Expect(CDILToken.Semicolon);
                    else
                        break;
                }
            }
            ExpectKeyword("end");
            return property;
        }

        public CodeAttributeDeclaration ParseCustomAttribute()
        {
            CodeAttributeDeclaration decl = new CodeAttributeDeclaration();
            decl.Name = ParseType().BaseType;
            // TODO: add support for parameters
            return decl;
        }

        public CodeMemberMethod ParseMethod()
        {
            CodeMemberMethod method = new CodeMemberMethod();

            ExpectKeyword("method");
            string methodName = EatKeyword();
            switch (methodName)
            {
                case "op_Equality":
                    method.Name = "operator ==";
                    break;

                case "op_Inequality":
                    method.Name = "operator !=";
                    break;

                default:
                    method.Name = methodName;
                    break;
            }
            Expect(CDILToken.LeftParen);
            while (TokenType != CDILToken.RightParen && TokenType != CDILToken.EOF)
            {
                bool varargs = false;

                if (IsKeyword("params"))
                {
                    varargs = true;
                    GetNextToken();
                }

                CodeTypeReference typeName = ParseType();
                string varName = EatKeyword();
                CodeParameterDeclarationExpression cpd = new CodeParameterDeclarationExpression(typeName, varName);
                if (varargs)
                    cpd.CustomAttributes.Add(new CodeAttributeDeclaration("System.ParamArrayAttribute"));
                method.Parameters.Add(cpd);

                if (TokenType == CDILToken.Comma)
                    GetNextToken();
            }
            Expect(CDILToken.RightParen);
            while (!IsKeyword("begin") && TokenType != CDILToken.EOF)
            {
                if (IsKeyword("returns"))
                {
                    GetNextToken();
                    method.ReturnType = ParseType();
                    continue;
                }
                if (IsKeyword("implements"))
                {
                    GetNextToken();
                    method.ImplementationTypes.Add(ParseType());
                    continue;
                }
                if (IsKeyword("customattribute"))
                {
                    GetNextToken();
                    method.CustomAttributes.Add(ParseCustomAttribute());
                    continue;
                }
                if (IsKeyword("implementsprivate"))
                {
                    GetNextToken();
                    method.PrivateImplementationType = ParseType();
                    continue;
                }
                if (IsKeyword("attributes"))
                {
                    GetNextToken();
                    method.Attributes = ParseMemberAttributes();
                    continue;
                }
                throw BuildException("Unknown keyword: " + TokenValue);
            }

            ExpectKeyword("begin");
            while (!IsKeyword("end") && TokenType != CDILToken.EOF)
            {
                method.Statements.Add(ParseStatement());
                if (TokenType == CDILToken.Semicolon)
                    Expect(CDILToken.Semicolon);
                else
                    break;
            }
            ExpectKeyword("end");

            return method;
        }

        public CodeConstructor ParseConstructor()
        {
            CodeConstructor ctor = new CodeConstructor();

            ExpectKeyword("constructor");
            Expect(CDILToken.LeftParen);
            while (TokenType != CDILToken.RightParen && TokenType != CDILToken.EOF)
            {
                CodeTypeReference typeName = ParseType();
                string varName = EatKeyword();
                ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeName, varName));
                if (TokenType == CDILToken.Comma)
                    GetNextToken();
            }
            Expect(CDILToken.RightParen);
            while (!IsKeyword("begin") && TokenType != CDILToken.EOF)
            {
                if (IsKeyword("attributes"))
                {
                    GetNextToken();
                    ctor.Attributes = ParseMemberAttributes();
                    continue;
                }
                if (IsKeyword("baseArg"))
                {
                    GetNextToken();
                    Expect(CDILToken.LeftParen);
                    ctor.BaseConstructorArgs.Add(ParseExpression());
                    Expect(CDILToken.RightParen);
                    continue;
                }
                if (IsKeyword("chainedArg"))
                {
                    GetNextToken();
                    Expect(CDILToken.LeftParen);
                    ctor.ChainedConstructorArgs.Add(ParseExpression());
                    Expect(CDILToken.RightParen);
                }
                throw BuildException("Unknown keyword: " + TokenValue);
            }

            ExpectKeyword("begin");
            while (!IsKeyword("end") && TokenType != CDILToken.EOF)
            {
                ctor.Statements.Add(ParseStatement());
                if (TokenType == CDILToken.Semicolon)
                    Expect(CDILToken.Semicolon);
                else
                    break;
            }
            ExpectKeyword("end");

            return ctor;
        }

        public CodeTypeConstructor ParseTypeConstructor()
        {
            CodeTypeConstructor ctor = new CodeTypeConstructor();

            ExpectKeyword("typeconstructor");
            ExpectKeyword("begin");
            while (!IsKeyword("end") && TokenType != CDILToken.EOF)
            {
                ctor.Statements.Add(ParseStatement());
                if (TokenType == CDILToken.Semicolon)
                    Expect(CDILToken.Semicolon);
                else
                    break;
            }
            ExpectKeyword("end");
            return ctor;
        }

        public CodeTypeDeclaration ParseClass()
        {
            ExpectKeyword("class");
            CodeTypeReference className = ParseType();
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(className.BaseType);
            if (IsKeyword("attributes"))
            {
                GetNextToken();
                ctd.TypeAttributes = ParseTypeAttributes();
            }
            if (IsKeyword("extends"))
            {
                GetNextToken();
                ctd.BaseTypes.Add(ParseType());
            }
            else
            {
                ctd.BaseTypes.Add(typeof(object));
            }
            while (IsKeyword("implements"))
            {
                GetNextToken();
                ctd.BaseTypes.Add(ParseType());
            }

            ctd.Members.AddRange(ParseMembers());

            return ctd;
        }

        public CodeTypeMemberCollection ParseMembers()
        {
            CodeTypeMemberCollection mc = new CodeTypeMemberCollection();

            while (TokenType != CDILToken.EOF && !IsKeyword("end"))
            {
                CodeTypeMember member = ParseMember();
                mc.Add(member);
            }
            return mc;
        }

        public CodeStatement ParseStatement()
        {
            if (IsKeyword("var"))
            {
                GetNextToken();
                CodeTypeReference type = ParseType();
                string name = EatKeyword();
                if (TokenType == CDILToken.Assign)
                {
                    Expect(CDILToken.Assign);
                    CodeExpression expr = ParseExpression();
                    return new CodeVariableDeclarationStatement(type, name, expr);
                }
                else
                {
                    return new CodeVariableDeclarationStatement(type, name);
                }
            }
            if (IsKeyword("call"))
            {
                GetNextToken();
                return new CodeExpressionStatement(ParseExpression());
            }

            if (IsKeyword("return"))
            {
                CodeMethodReturnStatement retVal;

                GetNextToken();
                retVal = new CodeMethodReturnStatement();
                if (TokenType != CDILToken.Semicolon && TokenType != CDILToken.EOF)
                    retVal.Expression = ParseExpression();
                return retVal;
            }
            if (IsKeyword("throw"))
            {
                CodeThrowExceptionStatement retVal;

                GetNextToken();
                retVal = new CodeThrowExceptionStatement();
                if (TokenType != CDILToken.Semicolon && TokenType != CDILToken.EOF)
                    retVal.ToThrow = ParseExpression();
                return retVal;
            }
            if (IsKeyword("if"))
            {
                CodeConditionStatement retVal = new CodeConditionStatement();
                GetNextToken();
                retVal.Condition = ParseExpression();
                ExpectKeyword("then");
                while (TokenType != CDILToken.EOF && !IsKeyword("else") && !IsKeyword("endif"))
                {
                    retVal.TrueStatements.Add(ParseStatement());
                    if (TokenType == CDILToken.Semicolon)
                        GetNextToken();
                }
                if (IsKeyword("else"))
                {
                    ExpectKeyword("else");
                    while (TokenType != CDILToken.EOF && !IsKeyword("endif"))
                    {
                        retVal.FalseStatements.Add(ParseStatement());
                        if (TokenType == CDILToken.Semicolon)
                            GetNextToken();
                    }
                }
                ExpectKeyword("endif");
                return retVal;
            }
            if (IsKeyword("let"))
            {
                CodeAssignStatement retVal = new CodeAssignStatement();
                GetNextToken();
                retVal.Left = ParseExpression();
                Expect(CDILToken.Assign);
                retVal.Right = ParseExpression();
                return retVal;
            }
            if (IsKeyword("comment"))
            {
                GetNextToken();
                string s = TokenValue.ToString();
                GetNextToken();
                return new CodeCommentStatement(s);
            }
            throw BuildException("Invalid token: '" + TokenType + "': " + TokenValue);
        }

        public static CodeStatement ParseStatement(string s, CDILContext context)
        {
            CDILParser parser = new CDILParser(context.Format(s), context);
            return parser.ParseStatement();
        }

        public static CodeTypeMember ParseMember(string s, CDILContext context)
        {
            CDILParser parser = new CDILParser(context.Format(s), context);
            return parser.ParseMember();
        }

        public static CodeTypeMemberCollection ParseMembers(string s, CDILContext context)
        {
            CDILParser parser = new CDILParser(context.Format(s), context);
            return parser.ParseMembers();
        }

        public static CodeTypeDeclaration ParseClass(string s, CDILContext context)
        {
            // Console.WriteLine(s, par);
            CDILParser parser = new CDILParser(context.Format(s), context);
            return parser.ParseClass();
        }
    }
}
