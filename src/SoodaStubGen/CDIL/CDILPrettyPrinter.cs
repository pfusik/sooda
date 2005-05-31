using System;
using System.CodeDom;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace Sooda.StubGen.CDIL
{
	public class CDILPrettyPrinter
	{
        private static void PrintCustomAttributeDeclaration(TextWriter output, CodeAttributeDeclaration cad)
        {
            output.Write(cad.Name);
            // TODO - parameters
        }

        private static void PrintMemberAttributes(TextWriter output, MemberAttributes attr)
        {
            bool first = true;

            if ((attr & MemberAttributes.Abstract) == MemberAttributes.Abstract)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Abstract");
            }
            if ((attr & MemberAttributes.Assembly) == MemberAttributes.Assembly)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Assembly");
            }
            if ((attr & MemberAttributes.Const) == MemberAttributes.Const)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Const");
            }
            if ((attr & MemberAttributes.Public) == MemberAttributes.Public)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Public");
            }
            if ((attr & MemberAttributes.Static) == MemberAttributes.Static)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Static");
            }
            if ((attr & MemberAttributes.Private) == MemberAttributes.Private)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Private");
            }
            if ((attr & MemberAttributes.Family) == MemberAttributes.Family)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Family");
            }
            if ((attr & MemberAttributes.Override) == MemberAttributes.Override)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Override");
            }
            if ((attr & MemberAttributes.Overloaded) == MemberAttributes.Overloaded)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Overloaded");
            }
            if ((attr & MemberAttributes.New) == MemberAttributes.New)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("New");
            }
            if ((attr & MemberAttributes.Final) == MemberAttributes.Final)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("Final");
            }
#if A
            if ((attr & MemberAttributes.FamilyOrAssembly) == MemberAttributes.FamilyOrAssembly)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("FamilyOrAssembly");
            }
            if ((attr & MemberAttributes.FamilyAndAssembly) == MemberAttributes.FamilyAndAssembly)
            {
                if (!first)
                    output.Write(",");
                first = false;
                output.Write("FamilyAndAssembly");
            }
#endif
        }

        private static void PrintTypeReference(TextWriter output, CodeTypeReference ctr)
        {
            if (ctr.ArrayRank == 1)
            {
                output.Write("arrayof(");
                output.Write(ctr.BaseType);
                output.Write(")");
            }
            else
            {
                output.Write(ctr.BaseType);
            }
        }

        private static void PrintExpression(TextWriter output, CodeExpression expression)
        {
            if (expression is CodeMethodInvokeExpression)
            {
                CodeMethodInvokeExpression cmi = (CodeMethodInvokeExpression)expression;
                if (cmi.Method.TargetObject == null)
                    output.Write("defaultscope");
                else
                    PrintExpression(output, cmi.Method.TargetObject);
                output.Write(".");
                output.Write(cmi.Method.MethodName);
                output.Write("(");
                for (int i = 0; i < cmi.Parameters.Count; ++i)
                {
                    if (i > 0)
                        output.Write(", ");
                    PrintExpression(output, cmi.Parameters[i]);
                }
                output.Write(")");
                return;
            }

            if (expression is CodeDelegateInvokeExpression)
            {
                CodeDelegateInvokeExpression die = (CodeDelegateInvokeExpression)expression;
                output.Write("delegatecall(");
                PrintExpression(output, die.TargetObject);
                output.Write(")");
                output.Write("(");
                for (int i = 0; i < die.Parameters.Count; ++i)
                {
                    if (i > 0)
                        output.Write(", ");
                    PrintExpression(output, die.Parameters[i]);
                }
                output.Write(")");
                return;
            }

            if (expression is CodePropertyReferenceExpression)
            {
                CodePropertyReferenceExpression cpre = (CodePropertyReferenceExpression)expression;
                if (cpre.TargetObject == null)
                    output.Write("defaultscope");
                else
                    PrintExpression(output, cpre.TargetObject);
                output.Write(".");
                output.Write(cpre.PropertyName);
                return;
            }

            if (expression is CodeFieldReferenceExpression)
            {
                CodeFieldReferenceExpression cpre = (CodeFieldReferenceExpression)expression;
                if (cpre.TargetObject == null)
                    output.Write("defaultscope");
                else
                    PrintExpression(output, cpre.TargetObject);
                output.Write(".");
                output.Write(cpre.FieldName);
                output.Write("$");
                return;
            }

            if (expression is CodePrimitiveExpression)
            {
                object value = ((CodePrimitiveExpression)expression).Value;
                if (value == null)
                    output.Write("null");
                else if (value.Equals(true))
                    output.Write("true");
                else if (value.Equals(false))
                    output.Write("false");
                else if (value is string)
                    output.Write("'{0}'", value.ToString().Replace("'","\\'"));
                else
                    output.Write("{0}", value);
                return;
            }

            if (expression is CodeThisReferenceExpression)
            {
                output.Write("this");
                return;
            }

            if (expression is CodeBaseReferenceExpression)
            {
                output.Write("base");
                return;
            }

            if (expression is CodeArgumentReferenceExpression)
            {
                output.Write("arg({0})", ((CodeArgumentReferenceExpression)expression).ParameterName);
                return;
            }

            if (expression is CodeVariableReferenceExpression)
            {
                output.Write("var({0})", ((CodeVariableReferenceExpression)expression).VariableName);
                return;
            }

            if (expression is CodeTypeReferenceExpression)
            {
                output.Write("typeref(");
                CodeTypeReferenceExpression ctr = (CodeTypeReferenceExpression)expression;
                PrintTypeReference(output, ctr.Type);
                output.Write(")");
                return;
            }
            if (expression is CodeCastExpression)
            {
                CodeCastExpression cce = expression as CodeCastExpression;

                output.Write("cast(");
                PrintTypeReference(output, cce.TargetType);
                output.Write(", ");
                PrintExpression(output, cce.Expression);
                output.Write(")");
                return;
            }

            if (expression is CodeTypeOfExpression)
            {
                CodeTypeOfExpression ctoe = (CodeTypeOfExpression)expression;
                output.Write("typeof(");
                PrintTypeReference(output, ctoe.Type);
                output.Write(")");
                return;
            }

            if (expression is CodeObjectCreateExpression)
            {
                CodeObjectCreateExpression coce = (CodeObjectCreateExpression)expression;
                output.Write("new ");
                PrintTypeReference(output, coce.CreateType);
                output.Write("(");
                for (int i = 0; i < coce.Parameters.Count; ++i)
                {
                    if (i > 0)
                        output.Write(", ");
                    PrintExpression(output, coce.Parameters[i]);

                }

                output.Write(")");
                return;
            }

            if (expression is CodeArrayIndexerExpression)
            {
                CodeArrayIndexerExpression caie = (CodeArrayIndexerExpression)expression;
                output.Write("arrayitem(");
                PrintExpression(output, caie.TargetObject);
                for (int i = 0; i < caie.Indices.Count; ++i)
                {
                    output.Write(", ");
                    PrintExpression(output, caie.Indices[i]);
                }
                output.Write(")");
                return;
            }

            if (expression is CodeArrayCreateExpression)
            {
                CodeArrayCreateExpression cace = (CodeArrayCreateExpression)expression;
                output.Write("newarray(");
                PrintTypeReference(output, cace.CreateType);
                output.Write(",");
                PrintExpression(output, cace.SizeExpression);
                output.Write(")");
                return;
            }

            if (expression is CodeBinaryOperatorExpression)
            {
                CodeBinaryOperatorExpression cboe = (CodeBinaryOperatorExpression)expression;
                switch (cboe.Operator)
                {
                    case CodeBinaryOperatorType.ValueEquality:
                        output.Write("equals");
                        break;

                    case CodeBinaryOperatorType.IdentityEquality:
                        output.Write("refequal");
                        break;

                    case CodeBinaryOperatorType.IdentityInequality:
                        output.Write("refnotequal");
                        break;

                    case CodeBinaryOperatorType.Add:
                        output.Write("add");
                        break;

                    default:
                        output.Write("UNKNOWN CBOE: {0}", cboe.Operator);
                        break;
                }
                output.Write("(");
                PrintExpression(output, cboe.Left);
                output.Write(", ");
                PrintExpression(output, cboe.Right);
                output.Write(")");
                return;
            }

            if (expression is CodePropertySetValueReferenceExpression)
            {
                output.Write("setvalue");
                return;
            }

            if (expression is CodeDirectionExpression)
            {
                switch (((CodeDirectionExpression)expression).Direction)
                {
                    case FieldDirection.In:
                        PrintExpression(output, ((CodeDirectionExpression)expression).Expression);
                        break;

                    case FieldDirection.Out:
                        output.Write("out(");
                        PrintExpression(output, ((CodeDirectionExpression)expression).Expression);
                        output.Write(")");
                        break;

                    case FieldDirection.Ref:
                        output.Write("ref(");
                        PrintExpression(output, ((CodeDirectionExpression)expression).Expression);
                        output.Write(")");
                        break;
                }
                return;
            }

            output.Write("*** UNKNOWN EXPRESSION:" + expression.GetType() + " ***");
        }

        private static void PrintConditionStatement(TextWriter output, CodeConditionStatement ccs)
        {
            output.Write("if ");
            PrintExpression(output, ccs.Condition);
            output.WriteLine();
            output.WriteLine("    then");
            for (int i = 0; i < ccs.TrueStatements.Count; ++i)
            {
                output.Write("        ");
                PrintStatement(output, ccs.TrueStatements[i]);
                if (i + 1 < ccs.TrueStatements.Count)
                    output.WriteLine(";");
                else
                    output.WriteLine();
            }
            if (ccs.FalseStatements.Count > 0)
            {
                output.WriteLine("    else");
                for (int i = 0; i < ccs.FalseStatements.Count; ++i)
                {
                    output.Write("        ");
                    PrintStatement(output, ccs.FalseStatements[i]);
                    if (i + 1 < ccs.FalseStatements.Count)
                        output.WriteLine(";");
                    else
                        output.WriteLine();
                }
            }
            output.WriteLine("    endif");
        }

        private static void PrintVariableDeclarationStatement(TextWriter output, CodeVariableDeclarationStatement statement)
        {
            output.Write("var ");
            PrintTypeReference(output, statement.Type);
            output.Write(" {0}", statement.Name);
            if (statement.InitExpression != null)
            {
                output.Write(" = ");
                PrintExpression(output, statement.InitExpression);
            }
        }

        private static void PrintExpressionStatement(TextWriter output, CodeExpressionStatement statement)
        {
            if (statement.Expression is CodeMethodInvokeExpression || statement.Expression is CodeDelegateInvokeExpression)
            {
                output.Write("call ");
                PrintExpression(output, statement.Expression);
            }
            else
                throw new ArgumentException("statement.Expression is not a CodeMethodInvokeExpression. Was: " + statement.Expression.GetType());
        }

        private static void PrintMethodReturnStatement(TextWriter output, CodeMethodReturnStatement statement)
        {
            output.Write("return ");
            if (statement.Expression != null)
            {
                PrintExpression(output, statement.Expression);
            }
            else
            {
                output.Write("nothing");
            }
        }

        private static void PrintAssignStatement(TextWriter output, CodeAssignStatement statement)
        {
            output.Write("let ");

            PrintExpression(output, statement.Left);
            output.Write(" = ");
            PrintExpression(output, statement.Right);
        }

        private static void PrintCommentStatement(TextWriter output, CodeCommentStatement statement)
        {
            output.Write("comment '{0}'", statement.Comment.Text.Replace("'", "\\'"));
        }

        private static void PrintThrowExceptionStatement(TextWriter output, CodeThrowExceptionStatement statement)
        {
            output.Write("throw ");
            if (statement.ToThrow != null)
            {
                PrintExpression(output, statement.ToThrow);
            }
        }

        private static void PrintStatement(TextWriter output, CodeStatement statement)
        {
            if (statement is CodeVariableDeclarationStatement)
            {
                PrintVariableDeclarationStatement(output, (CodeVariableDeclarationStatement)statement);
                return;
            }
            if (statement is CodeExpressionStatement)
            {
                PrintExpressionStatement(output, (CodeExpressionStatement)statement);
                return;
            }
            if (statement is CodeMethodReturnStatement)
            {
                PrintMethodReturnStatement(output, (CodeMethodReturnStatement)statement);
                return;
            }
            if (statement is CodeAssignStatement)
            {
                PrintAssignStatement(output, (CodeAssignStatement)statement);
                return;
            }
            if (statement is CodeThrowExceptionStatement)
            {
                PrintThrowExceptionStatement(output, (CodeThrowExceptionStatement)statement);
                return;
            }
            if (statement is CodeConditionStatement)
            {
                PrintConditionStatement(output, (CodeConditionStatement)statement);
                return;
            }
            if (statement is CodeCommentStatement)
            {
                PrintCommentStatement(output, (CodeCommentStatement)statement);
                return;
            }
            output.WriteLine("*** UNKNOWN STATEMENT: " + statement.GetType() + " ***");
        }

        public static void PrintMethod(TextWriter output, CodeMemberMethod method)
        {
            output.Write("method {0}(", method.Name);
            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                if (i > 0)
                    output.Write(", ");
                PrintTypeReference(output, method.Parameters[i].Type);
                output.Write(" {0}", method.Parameters[i].Name);
            }
            output.WriteLine(")");
            output.Write("    attributes ");
            PrintMemberAttributes(output, method.Attributes);
            output.WriteLine();
            
            if (method.ReturnType.BaseType != "System.Void")
            {
                output.Write("    returns ");
                PrintTypeReference(output, method.ReturnType);
                output.WriteLine();
            }
            foreach (CodeTypeReference ctr in method.ImplementationTypes)
            {
                output.Write("    implements ");
                PrintTypeReference(output, ctr);
                output.WriteLine();
            }
            if (method.PrivateImplementationType != null)
            {
                output.Write("    implementsprivate ");
                PrintTypeReference(output, method.PrivateImplementationType);
                output.WriteLine();
            }
            foreach (CodeAttributeDeclaration cad in method.CustomAttributes)
            {
                output.Write("    customattribute ");
                PrintCustomAttributeDeclaration(output, cad);
                output.WriteLine();
            }
            output.WriteLine("begin");
            for (int i = 0; i < method.Statements.Count; ++i)
            {
                output.Write("    ");
                PrintStatement(output, method.Statements[i]);
                if (i + 1 < method.Statements.Count)
                    output.WriteLine(";");
                else
                    output.WriteLine();
            }
            output.WriteLine("end");
        }

        private static void PrintTypeConstructor(TextWriter output, CodeTypeConstructor typeConstructor)
        {
            output.WriteLine("typeconstructor");
            foreach (CodeAttributeDeclaration cad in typeConstructor.CustomAttributes)
            {
                output.Write("    customattribute ");
                PrintCustomAttributeDeclaration(output, cad);
                output.WriteLine();
            }
            output.WriteLine("begin");
            for (int i = 0; i < typeConstructor.Statements.Count; ++i)
            {
                output.Write("    ");
                PrintStatement(output, typeConstructor.Statements[i]);
                if (i + 1 < typeConstructor.Statements.Count)
                    output.WriteLine(";");
                else
                    output.WriteLine();
            }
            output.WriteLine("end");
        }

        private static void PrintField(TextWriter output, CodeMemberField field)
        {
            output.Write("field ");
            PrintTypeReference(output, field.Type);
            output.Write(" ");
            output.WriteLine(field.Name);
            output.Write("    attributes ");
            PrintMemberAttributes(output, field.Attributes);
            output.WriteLine();
            if (field.InitExpression != null)
            {
                output.Write("    value ");
                PrintExpression(output, field.InitExpression);
                output.WriteLine();
            }

            output.WriteLine("end");
        }

        private static void PrintProperty(TextWriter output, CodeMemberProperty property)
        {
            output.Write("property ");
            PrintTypeReference(output, property.Type);
            output.WriteLine(" {0}", property.Name);
            output.Write("    attributes ");
            PrintMemberAttributes(output, property.Attributes);
            output.WriteLine();
            if (property.HasGet || property.GetStatements.Count > 0)
            {
                output.WriteLine("get");
                for (int i = 0; i < property.GetStatements.Count; ++i)
                {
                    output.Write("    ");
                    PrintStatement(output, property.GetStatements[i]);
                    if (i + 1 < property.GetStatements.Count)
                        output.WriteLine(";");
                    else
                        output.WriteLine();
                }
            }
            if (property.HasSet || property.SetStatements.Count > 0)
            {
                output.WriteLine("set");
                for (int i = 0; i < property.SetStatements.Count; ++i)
                {
                    output.Write("    ");
                    PrintStatement(output, property.SetStatements[i]);
                    if (i + 1 < property.SetStatements.Count)
                        output.WriteLine(";");
                    else
                        output.WriteLine();
                }
            }
            output.WriteLine("end");
        }

        private static void PrintConstructor(TextWriter output, CodeConstructor constructor)
        {
            output.Write("constructor(", constructor);
            for (int i = 0; i < constructor.Parameters.Count; ++i)
            {
                if (i > 0)
                    output.Write(", ");
                PrintTypeReference(output, constructor.Parameters[i].Type);
                output.Write(" {0}", constructor.Parameters[i].Name);
            }
            output.WriteLine(")");
            output.Write("    attributes ");
            PrintMemberAttributes(output, constructor.Attributes);
            output.WriteLine();
            
            foreach (CodeAttributeDeclaration cad in constructor.CustomAttributes)
            {
                output.Write("    customattribute ");
                PrintCustomAttributeDeclaration(output, cad);
                output.WriteLine();
            }
            output.WriteLine("begin");
            for (int i = 0; i < constructor.Statements.Count; ++i)
            {
                output.Write("    ");
                PrintStatement(output, constructor.Statements[i]);
                if (i + 1 < constructor.Statements.Count)
                    output.WriteLine(";");
                else
                    output.WriteLine();
            }
            output.WriteLine("end");
        }

        public static void PrintMember(TextWriter output, CodeTypeMember member)
        {
            if (member is CodeConstructor)
            {
                PrintConstructor(output, (CodeConstructor)member);
                return;
            }
            if (member is CodeTypeConstructor)
            {
                PrintTypeConstructor(output, (CodeTypeConstructor)member);
                return;
            }
            if (member is CodeMemberField)
            {
                PrintField(output, (CodeMemberField)member);
                return;
            }
            if (member is CodeMemberProperty)
            {
                PrintProperty(output, (CodeMemberProperty)member);
                return;
            }
            if (member is CodeMemberMethod)
            {
                PrintMethod(output, (CodeMemberMethod)member);
            }

        }
        public static void PrintType(TextWriter output, CodeTypeDeclaration ctd)
        {
            output.WriteLine("class {0}", ctd.Name);
            if (ctd.BaseTypes.Count > 0)
                output.WriteLine("    extends {0}", ctd.BaseTypes[0].BaseType);
            for (int i = 1; i < ctd.BaseTypes.Count; ++i)
            {
                output.WriteLine("    implements {0}", ctd.BaseTypes[i].BaseType);
            }

            foreach (CodeTypeMember member in ctd.Members)
            {
                output.WriteLine();
                PrintMember(output, member);
            }
            
            output.WriteLine();
            output.WriteLine("end");
        }
    }
}
