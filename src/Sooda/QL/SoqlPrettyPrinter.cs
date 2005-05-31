// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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
using System.IO;
using System.Collections.Specialized;

using Sooda.Schema;
using Sooda.QL;

namespace Sooda.QL {
    /// <summary>
    /// Summary description for SoqlPrettyPrinter.
    /// </summary>
    public class SoqlPrettyPrinter : ISoqlVisitor {
        public SoqlPrettyPrinter(TextWriter output) {
            Output = output;
        }

        public virtual void Visit(SoqlBinaryExpression v) {
            Output.Write("(");
            v.par1.Accept(this);
            Output.Write(" ");
            switch (v.op) {
            case SoqlBinaryOperator.Add:
                Output.Write("+");
                break;

            case SoqlBinaryOperator.Sub:
                Output.Write("-");
                break;

            case SoqlBinaryOperator.Div:
                Output.Write("/");
                break;

            case SoqlBinaryOperator.Mul:
                Output.Write("*");
                break;

            case SoqlBinaryOperator.Mod:
                Output.Write("%");
                break;
            }
            Output.Write(" ");
            v.par2.Accept(this);
            Output.Write(")");
        }

        public virtual void Visit(SoqlBooleanAndExpression v) {
            Output.Write("(");
            v.Left.Accept(this);
            Output.Write(" and ");
            v.Right.Accept(this);
            Output.Write(")");

        }

        public virtual void Visit(SoqlBooleanInExpression v) {
            v.Left.Accept(this);
            Output.Write(" in (");
            bool first = true;

            for (int i = 0; i < v.Right.Count; ++i) {
                if (!first)
                    Output.Write(",");
                first = false;
                ((SoqlExpression)v.Right[i]).Accept(this);
            }
            Output.Write(")");
        }

        public virtual void Visit(SoqlBooleanIsNullExpression v) {
            v.Expr.Accept(this);
            Output.Write(" is ");
            if (v.NotNull)
                Output.Write("not ");
            Output.Write("null");
        }

        public virtual void Visit(SoqlBooleanLiteralExpression v) {
            Output.Write(v.Value);
        }

        public virtual void Visit(SoqlBooleanNegationExpression v) {
            Output.Write("not (");
            v.par.Accept(this);
            Output.Write(")");
        }

        public virtual void Visit(SoqlUnaryNegationExpression v) {
            Output.Write("-(");
            v.par.Accept(this);
            Output.Write(")");
        }

        public virtual void Visit(SoqlBooleanOrExpression v) {
            Output.Write("(");
            v.par1.Accept(this);
            Output.Write(" OR ");
            v.par2.Accept(this);
            Output.Write(")");
        }

        public virtual void Visit(SoqlBooleanRelationalExpression v) {
            Output.Write("(");
            v.par1.Accept(this);
            Output.Write(" ");
            switch (v.op) {
            case SoqlRelationalOperator.Greater:
                Output.Write(">");
                break;

            case SoqlRelationalOperator.Less:
                Output.Write("<");
                break;

            case SoqlRelationalOperator.LessOrEqual:
                Output.Write("<=");
                break;

            case SoqlRelationalOperator.GreaterOrEqual:
                Output.Write(">=");
                break;

            case SoqlRelationalOperator.Equal:
                Output.Write("=");
                break;

            case SoqlRelationalOperator.NotEqual:
                Output.Write("<>");
                break;

            case SoqlRelationalOperator.Like:
                Output.Write("like");
                break;

            default:
                throw new NotImplementedException(v.op.ToString());
            }
            Output.Write(" ");
            v.par2.Accept(this);
            Output.Write(")");
        }

        public virtual void Visit(SoqlDecimalLiteralExpression v) {
            Output.Write(v.val);
        }

        public virtual void Visit(SoqlExistsExpression v) {
            Output.Write("exists (");
            Output.WriteLine();
            v.Query.Accept(this);
            Output.WriteLine();
            Output.Write(")");
        }

        public virtual void Visit(SoqlFunctionCallExpression v) {
            Output.Write(v.FunctionName);
            Output.Write("(");
            if (v.Parameters.Count == 1 && v.Parameters[0] is SoqlAsteriskExpression) {
                // special case for count(*) - temporary hack
                Output.Write("*");
            } else {
                for (int i = 0; i < v.Parameters.Count; ++i) {
                    if (i != 0)
                        Output.Write(", ");
                    v.Parameters[i].Accept(this);
                }
            }
            Output.Write(")");
        }

        public virtual void Visit(SoqlLiteralExpression v) {
            if (v.literalValue is String)
            {
                Output.Write("'");
                Output.Write(v.literalValue);
                Output.Write("'");
            }
            else
            {
                Output.Write(v.literalValue);
            }
        }

        public virtual void Visit(SoqlNullLiteral v) {
            Output.Write("null");
        }

        public virtual void Visit(SoqlParameterLiteralExpression v) {
            Output.Write("{");
            Output.Write(v.ParameterPosition);
            Output.Write("}");
        }

        public virtual void Visit(SoqlPathExpression v) {
            if (v.Left != null) {
                v.Left.Accept(this);
                Output.Write(".");
            }

            Output.Write(v.PropertyName);
        }

        public virtual void Visit(SoqlAsteriskExpression v) {
            if (v.Left != null) {
                v.Left.Accept(this);
                Output.Write(".");
            }

            Output.Write("*");
        }

        public virtual void Visit(SoqlCountExpression v) {
            if (v.Path != null) {
                v.Path.Accept(this);
                Output.Write(".");
            }

            Output.Write(v.CollectionName);
            Output.Write(".");
            Output.Write("Count");
        }

        public virtual void Visit(SoqlSoodaClassExpression v) {
            if (v.Path != null) {
                v.Path.Accept(this);
                Output.Write(".");
            }

            Output.Write("SoodaClass");
        }

        public virtual void Visit(SoqlContainsExpression v) {
            if (v.Path != null) {
                v.Path.Accept(this);
                Output.Write(".");
            }

            Output.Write(v.CollectionName);
            Output.Write(".");
            Output.Write("Contains(");
            if (v.Expr is SoqlQueryExpression)
                Output.WriteLine();
            v.Expr.Accept(this);
            if (v.Expr is SoqlQueryExpression)
                Output.WriteLine();
            Output.Write(")");
        }

        public virtual void Visit(SoqlQueryExpression v) {
            IndentLevel++;
            try {
                if (v.SelectExpressions.Count > 0) {
                    WriteIndentString();
                    Output.Write("select   ");
                    for (int i = 0; i < v.SelectExpressions.Count; ++i) {
                        if (i > 0) {
                            Output.WriteLine(",");
                            WriteIndentString();
                            Output.Write("         ");
                        }
                        v.SelectExpressions[i].Accept(this);
                        if (v.SelectAliases[i].Length > 0) {
                            Output.Write(" as ");
                            Output.Write(v.SelectAliases[i]);
                        }
                    }
                    Output.WriteLine();
                    WriteIndentString();
                    Output.Write("from     ");
                } else {
                    WriteIndentString();
                }
                for (int i = 0; i < v.From.Count; ++i) {
                    if (i > 0) {
                        Output.WriteLine(",");
                        WriteIndentString();
                        Output.Write("         ");
                    }

                    Output.Write(v.From[i]);
                    if (v.FromAliases[i].Length > 0) {
                        Output.Write(" as ");
                        Output.Write(v.FromAliases[i]);
                    }
                }

                if (v.WhereClause != null) {
                    Output.WriteLine();
                    WriteIndentString();
                    Output.Write("where    ");
                    v.WhereClause.Accept(this);
                }
                if (v.GroupByExpressions != null && v.GroupByExpressions.Count > 0) {
                    Output.WriteLine();
                    WriteIndentString();
                    Output.Write("group by ");
                    for (int i = 0; i < v.GroupByExpressions.Count; ++i) {
                        if (i > 0)
                            Output.Write(", ");
                        v.GroupByExpressions[i].Accept(this);
                    }
                }
                if (v.Having != null) {
                    Output.WriteLine();
                    WriteIndentString();
                    Output.Write("having   ");
                    v.Having.Accept(this);
                }
                if (v.OrderByExpressions != null && v.OrderByExpressions.Count > 0) {
                    Output.WriteLine();
                    WriteIndentString();
                    Output.Write("order by ");
                    for (int i = 0; i < v.OrderByExpressions.Count; ++i) {
                        if (i > 0)
                            Output.Write(", ");
                        v.OrderByExpressions[i].Accept(this);
                        Output.Write(" ");
                        Output.Write(v.OrderByOrder[i]);
                    }
                }
            } finally {
                IndentLevel--;
            }
        }

        public virtual void Visit(SoqlRawExpression v) {
            Output.Write("RAWQUERY(" + v.Text + ")");
        }

        public TextWriter Output;
        public int IndentLevel = -1;
        public int IndentStep = 4;

        public void WriteIndentString() {
            for (int i = 0; i < IndentLevel * IndentStep; ++i)
                Output.Write(' ');
        }

        public void PrintQuery(SoqlQueryExpression expr) {
            expr.Accept(this);
        }

        public void PrintExpression(SoqlExpression expr) {
            expr.Accept(this);
        }
    }
}
