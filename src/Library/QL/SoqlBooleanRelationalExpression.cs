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
using System.IO;
using System.Collections;
using System.ComponentModel;

using System.Globalization;

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

        public SoqlBooleanRelationalExpression()
        {
        }

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

                if (v1.GetType() == v2.GetType())
                {
                    if (v1 is System.Int32)
                    {
                        int i1 = (int)v1;
                        int i2 = (int)v2;

                        switch (op)
                        {
                            case SoqlRelationalOperator.Equal:
                                return new SoqlBooleanLiteralExpression(i1 == i2);

                            case SoqlRelationalOperator.NotEqual:
                                return new SoqlBooleanLiteralExpression(i1 != i2);

                            case SoqlRelationalOperator.Less:
                                return new SoqlBooleanLiteralExpression(i1 < i2);

                            case SoqlRelationalOperator.Greater:
                                return new SoqlBooleanLiteralExpression(i1 > i2);

                            case SoqlRelationalOperator.LessOrEqual:
                                return new SoqlBooleanLiteralExpression(i1 <= i2);

                            case SoqlRelationalOperator.GreaterOrEqual:
                                return new SoqlBooleanLiteralExpression(i1 >= i2);

                            default:
                                throw new NotImplementedException();
                        }
                    }
                    if (v1 is System.String)
                    {
                        string s1 = (string)v1;
                        string s2 = (string)v2;

                        switch (op)
                        {
                            case SoqlRelationalOperator.Equal:
                                return new SoqlBooleanLiteralExpression(s1 == s2);

                            case SoqlRelationalOperator.NotEqual:
                                return new SoqlBooleanLiteralExpression(s1 != s2);

                            case SoqlRelationalOperator.Less:
                                return new SoqlBooleanLiteralExpression(String.Compare(s1, s2) < 0);

                            case SoqlRelationalOperator.Greater:
                                return new SoqlBooleanLiteralExpression(String.Compare(s1, s2) > 0);

                            case SoqlRelationalOperator.LessOrEqual:
                                return new SoqlBooleanLiteralExpression(String.Compare(s1, s2) <= 0);

                            case SoqlRelationalOperator.GreaterOrEqual:
                                return new SoqlBooleanLiteralExpression(String.Compare(s1, s2) >= 0);

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }
            return this;
        }

        public static object Compare(object v1, object v2, SoqlRelationalOperator op)
        {
            if (v1 == null || v2 == null)
                return false;

            if (v1 is DateTime || v2 is DateTime)
            {
                DateTime d1 = Convert.ToDateTime(v1, CultureInfo.InvariantCulture);
                DateTime d2 = Convert.ToDateTime(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for datetime");

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is Single || v2 is Single || v1 is Double || v2 is Double)
            {
                Double d1 = Convert.ToDouble(v1, CultureInfo.InvariantCulture);
                Double d2 = Convert.ToDouble(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for floating point");

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is Decimal || v2 is Decimal)
            {
                Decimal d1 = Convert.ToDecimal(v1, CultureInfo.InvariantCulture);
                Decimal d2 = Convert.ToDecimal(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for Decimal");

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is Int32 || v2 is Int32 || v1 is Int16 || v2 is Int16 || v1 is SByte || v2 is SByte)
            {
                Int32 d1 = Convert.ToInt32(v1, CultureInfo.InvariantCulture);
                Int32 d2 = Convert.ToInt32(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for Int32");

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is Boolean || v2 is Boolean)
            {
                Boolean d1 = Convert.ToBoolean(v1, CultureInfo.InvariantCulture);
                Boolean d2 = Convert.ToBoolean(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    default:
                        throw new NotSupportedException("Operator " + op + " is not supported for Boolean");
                }
            }

            if (v1 is String || v2 is String)
            {
                String d1 = v1.ToString();
                String d2 = v2.ToString();

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return String.Compare(d1, d2, true) == 0;

                    case SoqlRelationalOperator.NotEqual:
                        return String.Compare(d1, d2, true) != 0;

                    case SoqlRelationalOperator.Less:
                        return String.Compare(d1, d2, true) < 0;

                    case SoqlRelationalOperator.Greater:
                        return String.Compare(d1, d2, true) > 0;

                    case SoqlRelationalOperator.LessOrEqual:
                        return String.Compare(d1, d2, true) <= 0;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return String.Compare(d1, d2, true) >= 0;

                    case SoqlRelationalOperator.Like:
                        return SoqlUtils.Like(d1, d2);

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is UInt32 || v2 is UInt32 || v1 is UInt16 || v2 is UInt16 || v1 is Byte || v2 is Byte)
            {
                UInt32 d1 = Convert.ToUInt32(v1, CultureInfo.InvariantCulture);
                UInt32 d2 = Convert.ToUInt32(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for UInt32");

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is Int64 || v2 is Int64)
            {
                Int64 d1 = Convert.ToInt64(v1, CultureInfo.InvariantCulture);
                Int64 d2 = Convert.ToInt64(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for int64");

                    default:
                        throw new NotImplementedException();
                }
            }

            if (v1 is UInt64 || v2 is UInt64)
            {
                UInt64 d1 = Convert.ToUInt64(v1, CultureInfo.InvariantCulture);
                UInt64 d2 = Convert.ToUInt64(v2, CultureInfo.InvariantCulture);

                switch (op)
                {
                    case SoqlRelationalOperator.Equal:
                        return d1 == d2;

                    case SoqlRelationalOperator.NotEqual:
                        return d1 != d2;

                    case SoqlRelationalOperator.Less:
                        return d1 < d2;

                    case SoqlRelationalOperator.Greater:
                        return d1 > d2;

                    case SoqlRelationalOperator.LessOrEqual:
                        return d1 <= d2;

                    case SoqlRelationalOperator.GreaterOrEqual:
                        return d1 >= d2;

                    case SoqlRelationalOperator.Like:
                        throw new NotSupportedException("like is not supported for uint64");

                    default:
                        throw new NotImplementedException();
                }
            }

            IComparable comparable = v1 as IComparable;

            if (v1 != null)
            {

            }

            throw new NotSupportedException("Comparing objects of types: " + v1.GetType() + " and " + v2.GetType() + " is not supported");
        }

        public override SoqlExpressionType GetExpressionType()
        {
            return new SoqlExpressionType (typeof(bool));
        }
    }
}
