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
using System.IO;
using System.Collections;

using Sooda.QL.TypedWrappers;

namespace Sooda.QL
{
    /// <summary>
    /// Summary description for SoqlPrettyPrinter.
    /// </summary>
    public class SoqlPrettyPrinter : ISoqlVisitor
    {
        public bool IndentOutput = true;
        public IList ParameterValues;

        public SoqlPrettyPrinter(TextWriter output)
        {
            Output = output;
            ParameterValues = null;
        }

        public SoqlPrettyPrinter(TextWriter output, IList parameterValues)
        {
            Output = output;
            ParameterValues = parameterValues;
        }

        public virtual void Visit(SoqlTypedWrapperExpression v)
        {
            v.InnerExpression.Accept(this);
        }

        public virtual void Visit(SoqlBooleanWrapperExpression v)
        {
            v.InnerExpression.Accept(this);
        }

        protected virtual void Write(SoqlBinaryOperator op)
        {
            switch (op)
            {
                case SoqlBinaryOperator.Add:
                    Output.Write('+');
                    break;

                case SoqlBinaryOperator.Sub:
                    Output.Write('-');
                    break;

                case SoqlBinaryOperator.Div:
                    Output.Write('/');
                    break;

                case SoqlBinaryOperator.Mul:
                    Output.Write('*');
                    break;

                case SoqlBinaryOperator.Mod:
                    Output.Write('%');
                    break;

                case SoqlBinaryOperator.Concat:
                    Output.Write("||");
                    break;
            }
        }

        public virtual void Visit(SoqlBinaryExpression v)
        {
            Output.Write('(');
            v.par1.Accept(this);
            Output.Write(' ');
            Write(v.op);
            Output.Write(' ');
            v.par2.Accept(this);
            Output.Write(')');
        }

        public virtual void Visit(SoqlBooleanAndExpression v)
        {
            Output.Write('(');
            v.Left.Accept(this);
            Output.Write(" and ");
            v.Right.Accept(this);
            Output.Write(')');
        }

        public virtual void Visit(SoqlBooleanInExpression v)
        {
            if (v.Right.Count == 0)
            {
                Output.Write("0=1");
                return;
            }

            v.Left.Accept(this);
            Output.Write(" in (");

            for (int i = 0; i < v.Right.Count; ++i)
            {
                if (i > 0)
                    Output.Write(',');
                v.Right[i].Accept(this);
            }
            Output.Write(')');
        }

        public virtual void Visit(SoqlBooleanIsNullExpression v)
        {
            v.Expr.Accept(this);
            Output.Write(" is ");
            if (v.NotNull)
                Output.Write("not ");
            Output.Write("null");
        }

        public virtual void Visit(SoqlBooleanLiteralExpression v)
        {
            Output.Write(v.Value);
        }

        public virtual void Visit(SoqlBooleanNegationExpression v)
        {
            Output.Write("(not (");
            v.par.Accept(this);
            Output.Write("))");
        }

        public virtual void Visit(SoqlUnaryNegationExpression v)
        {
            Output.Write("(-(");
            v.par.Accept(this);
            Output.Write("))");
        }

        public virtual void Visit(SoqlBooleanOrExpression v)
        {
            Output.Write('(');
            v.par1.Accept(this);
            Output.Write(" OR ");
            v.par2.Accept(this);
            Output.Write(')');
        }

        protected void OutputRelationalOperator(SoqlRelationalOperator op)
        {
            Output.Write(' ');
            switch (op)
            {
                case SoqlRelationalOperator.Greater:
                    Output.Write('>');
                    break;

                case SoqlRelationalOperator.Less:
                    Output.Write('<');
                    break;

                case SoqlRelationalOperator.LessOrEqual:
                    Output.Write("<=");
                    break;

                case SoqlRelationalOperator.GreaterOrEqual:
                    Output.Write(">=");
                    break;

                case SoqlRelationalOperator.Equal:
                    Output.Write('=');
                    break;

                case SoqlRelationalOperator.NotEqual:
                    Output.Write("<>");
                    break;

                case SoqlRelationalOperator.Like:
                    Output.Write("like");
                    break;

                default:
                    throw new NotImplementedException(op.ToString());
            }
            Output.Write(' ');
        }

        public virtual void Visit(SoqlBooleanRelationalExpression v)
        {
            Output.Write('(');
            v.par1.Accept(this);
            OutputRelationalOperator(v.op);
            v.par2.Accept(this);
            Output.Write(')');
        }

        public virtual void Visit(SoqlExistsExpression v)
        {
            Output.Write("exists (");
            if (IndentOutput)
            {
                Output.WriteLine();
            }
            v.Query.Accept(this);
            if (IndentOutput)
            {
                Output.WriteLine();
            }
            Output.Write(')');
        }

        public virtual void Visit(SoqlFunctionCallExpression v)
        {
            Output.Write(v.FunctionName);
            Output.Write('(');
            if (v.Parameters.Count == 1 && v.Parameters[0] is SoqlAsteriskExpression)
            {
                // special case for count(*) - temporary hack
                Output.Write('*');
            }
            else
            {
                for (int i = 0; i < v.Parameters.Count; ++i)
                {
                    if (i != 0)
                        Output.Write(", ");
                    v.Parameters[i].Accept(this);
                }
            }
            Output.Write(')');
        }

        public virtual void Visit(SoqlLiteralExpression v)
        {
            if (v.LiteralValue is String)
            {
                Output.Write('\'');
                Output.Write(((string)v.LiteralValue).Replace("'", "''"));
                Output.Write('\'');
            }
            else if (v.LiteralValue is DateTime)
            {
                Output.Write('\'');
                Output.Write(((DateTime)v.LiteralValue).ToString("yyyyMMdd HH:mm:ss"));
                Output.Write('\'');
            }
            else if (v.LiteralValue == null)
            {
                Output.Write("null");
            }
            else
            {
                Output.Write(v.LiteralValue);
            }
        }

        public virtual void Visit(SoqlNullLiteral v)
        {
            Output.Write("null");
        }

        public virtual void Visit(SoqlParameterLiteralExpression v)
        {
            if (ParameterValues != null)
            {
                object parameterValue = ParameterValues[v.ParameterPosition];
                if (parameterValue is String)
                {
                    Output.Write('\'');
                    Output.Write(((string)parameterValue).Replace("'", "''"));
                    Output.Write('\'');
                }
                else if (parameterValue is DateTime)
                {
                    Output.Write('\'');
                    Output.Write(((DateTime)parameterValue).ToString("yyyyMMdd HH:mm:ss"));
                    Output.Write('\'');
                }
                else
                {
                    Output.Write(parameterValue);
                }
            }
            else
            {
                Output.Write('{');
                Output.Write(v.ParameterPosition);
                if (v.Modifiers != null)
                {
                    Output.Write(':');
                    Output.Write(v.Modifiers.ToString());
                }
                Output.Write('}');
            }
        }

        public virtual void Visit(SoqlPathExpression v)
        {
            if (v.Left != null)
            {
                v.Left.Accept(this);
                Output.Write('.');
            }

            Output.Write(v.PropertyName);
        }

        public virtual void Visit(SoqlAsteriskExpression v)
        {
            if (v.Left != null)
            {
                v.Left.Accept(this);
                Output.Write('.');
            }

            Output.Write('*');
        }

        public virtual void Visit(SoqlCountExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
                Output.Write('.');
            }

            Output.Write(v.CollectionName);
            Output.Write(".Count");
        }

        public virtual void Visit(SoqlSoodaClassExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
                Output.Write('.');
            }

            Output.Write("SoodaClass");
        }

        public virtual void Visit(SoqlContainsExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
                Output.Write('.');
            }

            Output.Write(v.CollectionName);
            Output.Write('.');
            Output.Write("Contains(");
            if (v.Expr is SoqlQueryExpression && IndentOutput)
                Output.WriteLine();
            v.Expr.Accept(this);
            if (v.Expr is SoqlQueryExpression && IndentOutput)
                Output.WriteLine();
            Output.Write(')');
        }

        public virtual void Visit(SoqlQueryExpression v)
        {
            IndentLevel++;
            try
            {
                if (v.SelectExpressions.Count > 0)
                {
                    WriteIndentString();
                    Output.Write("select   ");
                    if (v.Distinct)
                        Output.Write("distinct ");
                    for (int i = 0; i < v.SelectExpressions.Count; ++i)
                    {
                        if (i > 0)
                        {
                            if (IndentOutput)
                            {
                                Output.WriteLine(',');
                                WriteIndentString();
                                Output.Write("         ");
                            }
                            else
                            {
                                Output.Write(',');
                            }
                        }
                        if (v.SelectExpressions[i] is SoqlQueryExpression)
                            Output.Write('(');
                        v.SelectExpressions[i].Accept(this);
                        if (v.SelectExpressions[i] is SoqlQueryExpression)
                            Output.Write(')');
                        if (v.SelectAliases[i].Length > 0)
                        {
                            Output.Write(" as ");
                            Output.Write(v.SelectAliases[i]);
                        }
                    }
                    if (IndentOutput)
                    {
                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("from     ");
                    }
                    else
                    {
                        Output.Write(" from ");
                    }
                }
                else
                {
                    WriteIndentString();
                }
                for (int i = 0; i < v.From.Count; ++i)
                {
                    if (i > 0)
                    {
                        if (IndentOutput)
                        {
                            Output.WriteLine(',');
                            WriteIndentString();
                            Output.Write("         ");
                        }
                        else
                        {
                            Output.Write(',');
                        }
                    }

                    Output.Write(v.From[i]);
                    if (v.FromAliases[i].Length > 0)
                    {
                        Output.Write(" as ");
                        Output.Write(v.FromAliases[i]);
                    }
                }

                if (v.WhereClause != null)
                {
                    if (IndentOutput)
                    {
                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("where    ");
                    }
                    else
                    {
                        Output.Write(" where ");
                    }
                    v.WhereClause.Accept(this);
                }
                if (v.GroupByExpressions != null && v.GroupByExpressions.Count > 0)
                {
                    if (IndentOutput)
                    {
                        Output.WriteLine();
                        WriteIndentString();
                    }
                    else
                    {
                        Output.Write(' ');
                    }
                    Output.Write("group by ");
                    for (int i = 0; i < v.GroupByExpressions.Count; ++i)
                    {
                        if (i > 0)
                            Output.Write(", ");
                        v.GroupByExpressions[i].Accept(this);
                    }
                }
                if (v.Having != null)
                {
                    if (IndentOutput)
                    {
                        Output.WriteLine();
                        WriteIndentString();
                    }
                    else
                    {
                        Output.Write(' ');
                    }
                    Output.Write("having   ");
                    v.Having.Accept(this);
                }
                if (v.OrderByExpressions != null && v.OrderByExpressions.Count > 0)
                {
                    if (IndentOutput)
                    {
                        Output.WriteLine();
                        WriteIndentString();
                    }
                    else
                    {
                        Output.Write(' ');
                    }
                    Output.Write("order by ");
                    for (int i = 0; i < v.OrderByExpressions.Count; ++i)
                    {
                        if (i > 0)
                            Output.Write(", ");
                        v.OrderByExpressions[i].Accept(this);
                        Output.Write(' ');
                        Output.Write(v.OrderByOrder[i]);
                    }
                }
            }
            finally
            {
                IndentLevel--;
            }
        }

        public virtual void Visit(SoqlRawExpression v)
        {
            Output.Write("RAWQUERY(");
            Output.Write(v.Text);
            Output.Write(')');
        }

        public virtual void Visit(SoqlConditionalExpression v)
        {
            Output.Write("case when ");
            v.condition.Accept(this);
            Output.Write(" then ");
            v.ifTrue.Accept(this);
            Output.Write(" else ");
            v.ifFalse.Accept(this);
            Output.Write(" end");
        }

        static readonly char[] LikeMetacharacters = { '%', '_', '[' };

        public virtual void Visit(SoqlStringContainsExpression v)
        {
            Output.Write('(');
            v.haystack.Accept(this);
            Output.Write(" like '");
            if (v.position != SoqlStringContainsPosition.Start)
                Output.Write('%');
            string s = v.needle.Replace("'", "''");
            string suffix;
            if (s.IndexOfAny(LikeMetacharacters) >= 0)
            {
                s = s.Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "~[");
                suffix = "' escape '~')";
            }
            else
            {
                suffix = "')";
            }
            Output.Write(s);
            if (v.position != SoqlStringContainsPosition.End)
                Output.Write('%');
            Output.Write(suffix);
        }

        public TextWriter Output;
        public int IndentLevel = -1;
        public int IndentStep = 4;

        public void WriteIndentString()
        {
            if (IndentOutput)
            {
                for (int i = 0; i < IndentLevel * IndentStep; ++i)
                    Output.Write(' ');
            }
        }

        public void PrintQuery(SoqlQueryExpression expr)
        {
            expr.Accept(this);
        }

        public void PrintExpression(SoqlExpression expr)
        {
            expr.Accept(this);
        }
    }
}
