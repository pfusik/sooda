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

using System.Xml.Serialization;

namespace Sooda.QL {
    public class SoqlBinaryExpression : SoqlExpression {
        [XmlElement("Left")]
        public SoqlExpression par1;

        [XmlElement("Right")]
        public SoqlExpression par2;

        [XmlAttribute("operator")]
        public SoqlBinaryOperator op;

        public SoqlBinaryExpression() {}

        public SoqlBinaryExpression(SoqlExpression par1, SoqlExpression par2, SoqlBinaryOperator op) {
            this.par1 = par1;
            this.par2 = par2;
            this.op = op;
        }

        // visitor pattern
        public override void Accept(ISoqlVisitor visitor) {
            visitor.Visit(this);
        }

        private static object CalcValue(SoqlBinaryOperator op, object val1, object val2)
        {
            if (val1 == null || val2 == null)
                return null;

            //
            // the type precedence is based on SQL2000 "Data Type Precedence"
            // help topic
            //

            switch (op)
            {
                case SoqlBinaryOperator.Add:
                    if (val1 is double || val2 is double)
                        return Convert.ToDouble(val1) + Convert.ToDouble(val2);
                    if (val1 is float || val2 is float)
                        return Convert.ToSingle(val1) + Convert.ToSingle(val2);
                    if (val1 is decimal || val2 is decimal)
                        return Convert.ToDecimal(val1) + Convert.ToDecimal(val2);
                    if (val1 is long || val2 is long)
                        return Convert.ToInt64(val1) + Convert.ToInt64(val2);
                    if (val1 is int || val2 is int)
                        return Convert.ToInt32(val1) + Convert.ToInt32(val2);
                    if (val1 is short || val2 is short)
                        return Convert.ToInt16(val1) + Convert.ToInt16(val2);
                    if (val1 is sbyte || val2 is sbyte)
                        return Convert.ToSByte(val1) + Convert.ToSByte(val2);
                    if (val1 is string || val2 is string)
                        return Convert.ToString(val1) + Convert.ToString(val2);
                    throw new NotSupportedException("Addition not supported for arguments of type " + val1.GetType().Name + " and " + val2.GetType().Name);

                case SoqlBinaryOperator.Sub:
                    if (val1 is double || val2 is double)
                        return Convert.ToDouble(val1) - Convert.ToDouble(val2);
                    if (val1 is float || val2 is float)
                        return Convert.ToSingle(val1) - Convert.ToSingle(val2);
                    if (val1 is decimal || val2 is decimal)
                        return Convert.ToDecimal(val1) - Convert.ToDecimal(val2);
                    if (val1 is long || val2 is long)
                        return Convert.ToInt64(val1) - Convert.ToInt64(val2);
                    if (val1 is int || val2 is int)
                        return Convert.ToInt32(val1) - Convert.ToInt32(val2);
                    if (val1 is short || val2 is short)
                        return Convert.ToInt16(val1) - Convert.ToInt16(val2);
                    if (val1 is sbyte || val2 is sbyte)
                        return Convert.ToSByte(val1) - Convert.ToSByte(val2);
                    throw new NotSupportedException("Subtraction not supported for arguments of type " + val1.GetType().Name + " and " + val2.GetType().Name);

                case SoqlBinaryOperator.Mul:
                    if (val1 is double || val2 is double)
                        return Convert.ToDouble(val1) * Convert.ToDouble(val2);
                    if (val1 is float || val2 is float)
                        return Convert.ToSingle(val1) * Convert.ToSingle(val2);
                    if (val1 is decimal || val2 is decimal)
                        return Convert.ToDecimal(val1) * Convert.ToDecimal(val2);
                    if (val1 is long || val2 is long)
                        return Convert.ToInt64(val1) * Convert.ToInt64(val2);
                    if (val1 is int || val2 is int)
                        return Convert.ToInt32(val1) * Convert.ToInt32(val2);
                    if (val1 is short || val2 is short)
                        return Convert.ToInt16(val1) * Convert.ToInt16(val2);
                    if (val1 is sbyte || val2 is sbyte)
                        return Convert.ToSByte(val1) * Convert.ToSByte(val2);
                    throw new NotSupportedException("Multiplication not supported for arguments of type " + val1.GetType().Name + " and " + val2.GetType().Name);

                case SoqlBinaryOperator.Div:
                    if (val1 is double || val2 is double)
                        return Convert.ToDouble(val1) / Convert.ToDouble(val2);
                    if (val1 is float || val2 is float)
                        return Convert.ToSingle(val1) / Convert.ToSingle(val2);
                    if (val1 is decimal || val2 is decimal)
                        return Convert.ToDecimal(val1) / Convert.ToDecimal(val2);
                    if (val1 is long || val2 is long)
                        return Convert.ToInt64(val1) / Convert.ToInt64(val2);
                    if (val1 is int || val2 is int)
                        return Convert.ToInt32(val1) / Convert.ToInt32(val2);
                    if (val1 is short || val2 is short)
                        return Convert.ToInt16(val1) / Convert.ToInt16(val2);
                    if (val1 is sbyte || val2 is sbyte)
                        return Convert.ToSByte(val1) / Convert.ToSByte(val2);
                    throw new NotSupportedException("Division not supported for arguments of type " + val1.GetType().Name + " and " + val2.GetType().Name);

                case SoqlBinaryOperator.Mod:
                    if (val1 is double || val2 is double)
                        return Convert.ToDouble(val1) % Convert.ToDouble(val2);
                    if (val1 is float || val2 is float)
                        return Convert.ToSingle(val1) % Convert.ToSingle(val2);
                    if (val1 is decimal || val2 is decimal)
                        return Convert.ToDecimal(val1) % Convert.ToDecimal(val2);
                    if (val1 is long || val2 is long)
                        return Convert.ToInt64(val1) % Convert.ToInt64(val2);
                    if (val1 is int || val2 is int)
                        return Convert.ToInt32(val1) % Convert.ToInt32(val2);
                    if (val1 is short || val2 is short)
                        return Convert.ToInt16(val1) % Convert.ToInt16(val2);
                    if (val1 is sbyte || val2 is sbyte)
                        return Convert.ToSByte(val1) % Convert.ToSByte(val2);
                    throw new NotSupportedException("Modulus not supported for arguments of type " + val1.GetType().Name + " and " + val2.GetType().Name);

                default:
                    throw new NotSupportedException("Binary operator " + op + " is not supported.");
            }
        }

        public override object Evaluate(ISoqlEvaluateContext context)
        {
            object val1 = par1.Evaluate(context);
            object val2 = par2.Evaluate(context);

            return CalcValue(op, val1, val2);
        }

        public override SoqlExpression Simplify() 
        {
            par1 = par1.Simplify();
            par2 = par2.Simplify();

            ISoqlConstantExpression cp1 = par1 as ISoqlConstantExpression;
            ISoqlConstantExpression cp2 = par2 as ISoqlConstantExpression;
            if (cp1 != null && cp2 != null) {
                object v1 = cp1.GetConstantValue();
                object v2 = cp2.GetConstantValue();

                object newValue = CalcValue(op, v1, v2);
                if (newValue != null)
                    return new SoqlLiteralExpression(newValue);
                else
                    return new SoqlNullLiteral();
            }

            return this;
        }
    }
}
