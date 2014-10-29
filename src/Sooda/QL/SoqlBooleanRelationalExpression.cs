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
using System.Collections;


using System.Xml.Serialization;

namespace Sooda.QL
{
    public class SoqlBooleanRelationalExpression : SoqlBooleanExpression
    {
        [XmlElement("Left")]
        public SoqlExpression par1;

        [XmlElement("Right")]
        public SoqlExpression par2;

        [XmlAttribute("operator")]
        public SoqlRelationalOperator op;

        public SoqlBooleanRelationalExpression() { }

        public SoqlBooleanRelationalExpression(SoqlExpression par1, SoqlExpression par2, SoqlRelationalOperator op)
        {
            this.par1 = par1;
            this.par2 = par2;
            this.op = op;
        }

        // visitor pattern
        public override void Accept(ISoqlVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override SoqlExpression Simplify()
        {
            par1 = par1.Simplify();
            par2 = par2.Simplify();

            ISoqlConstantExpression cp1 = par1 as ISoqlConstantExpression;
            ISoqlConstantExpression cp2 = par2 as ISoqlConstantExpression;

            if (cp1 != null && cp2 != null)
            {
                object v1 = cp1.GetConstantValue();
                object v2 = cp2.GetConstantValue();

                object result = Compare(v1, v2, op);
                if (result == null)
                    return new SoqlNullLiteral();
                else
                    return new SoqlBooleanLiteralExpression((bool)result);
            }
            return this;
        }

        private static void PromoteTypes(ref object val1, ref object val2)
        {
            if (val1.GetType() == val2.GetType())
                return;

            if (val1 is SoodaObject)
            {
                val1 = ((SoodaObject)val1).GetPrimaryKeyValue();
            }

            if (val2 is SoodaObject)
            {
                val2 = ((SoodaObject)val2).GetPrimaryKeyValue();
            }

            if (val1 is DateTime || val2 is DateTime)
            {
                val1 = Convert.ToDateTime(val1);
                val2 = Convert.ToDateTime(val2);
                return;
            }

            if (val1 is string || val2 is string)
            {
                val1 = Convert.ToString(val1);
                val2 = Convert.ToString(val2);
                return;
            }
            if (val1 is double || val2 is double)
            {
                val1 = Convert.ToDouble(val1);
                val2 = Convert.ToDouble(val2);
                return;
            }

            if (val1 is float || val2 is float)
            {
                val1 = Convert.ToSingle(val1);
                val2 = Convert.ToSingle(val2);
                return;
            }
            if (val1 is decimal || val2 is decimal)
            {
                val1 = Convert.ToDecimal(val1);
                val2 = Convert.ToDecimal(val2);
                return;
            }
            if (val1 is long || val2 is long)
            {
                val1 = Convert.ToInt64(val1);
                val2 = Convert.ToInt64(val2);
                return;
            }
            if (val1 is int || val2 is int)
            {
                val1 = Convert.ToInt32(val1);
                val2 = Convert.ToInt32(val2);
                return;
            }

            if (val1 is short || val2 is short)
            {
                val1 = Convert.ToInt16(val1);
                val2 = Convert.ToInt16(val2);
                return;
            }
            if (val1 is sbyte || val2 is sbyte)
            {
                val1 = Convert.ToSByte(val1);
                val2 = Convert.ToSByte(val2);
                return;
            }
            if (val1 is bool || val2 is bool)
            {
                val1 = Convert.ToBoolean(val1);
                val2 = Convert.ToBoolean(val2);
                return;
            }
            throw new Exception("Cannot promote types " + val1.GetType().Name + " and " + val2.GetType().Name + " to one type.");
        }

        public static object Compare(object v1, object v2, SoqlRelationalOperator op)
        {
            v1 = Sooda.Utils.SqlTypesUtil.Unwrap(v1);
            v2 = Sooda.Utils.SqlTypesUtil.Unwrap(v2);

            if (v1 == null || v2 == null)
                return null;

            IComparer comparer = Comparer.Default;
            PromoteTypes(ref v1, ref v2);
            switch (op)
            {
                case SoqlRelationalOperator.Equal:
                    return comparer.Compare(v1, v2) == 0;

                case SoqlRelationalOperator.NotEqual:
                    return comparer.Compare(v1, v2) != 0;

                case SoqlRelationalOperator.Greater:
                    return comparer.Compare(v1, v2) > 0;

                case SoqlRelationalOperator.GreaterOrEqual:
                    return comparer.Compare(v1, v2) >= 0;

                case SoqlRelationalOperator.LessOrEqual:
                    return comparer.Compare(v1, v2) <= 0;

                case SoqlRelationalOperator.Less:
                    return comparer.Compare(v1, v2) < 0;

                case SoqlRelationalOperator.Like:
                    string s1 = Convert.ToString(v1);
                    string s2 = Convert.ToString(v2);

                    return SoqlUtils.Like(s1, s2);

                default:
                    throw new NotSupportedException("Relational operator " + op + " is not supported.");
            }
        }

        public override object Evaluate(ISoqlEvaluateContext context)
        {
            object v1 = par1.Evaluate(context);
            object v2 = par2.Evaluate(context);

            return Compare(v1, v2, op);
        }
    }
}
