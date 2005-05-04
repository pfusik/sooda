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

        public override SoqlExpression Simplify() {
            par1 = par1.Simplify();
            par2 = par2.Simplify();

            ISoqlConstantExpression cp1 = par1 as ISoqlConstantExpression;
            ISoqlConstantExpression cp2 = par2 as ISoqlConstantExpression;
            if (cp1 != null && cp2 != null) {
                object v1 = cp1.GetConstantValue();
                object v2 = cp2.GetConstantValue();

                if (v1.GetType() == v2.GetType()) {
                    if (v1.GetType() == typeof(int)) {
                        switch (op) {
                        case SoqlBinaryOperator.Add:
                            return new SoqlLiteralExpression((int)v1 + (int)v2);

                        case SoqlBinaryOperator.Sub:
                            return new SoqlLiteralExpression((int)v1 - (int)v2);

                        case SoqlBinaryOperator.Mul:
                            return new SoqlLiteralExpression((int)v1 * (int)v2);

                        case SoqlBinaryOperator.Div:
                            return new SoqlLiteralExpression((int)v1 / (int)v2);

                        case SoqlBinaryOperator.Mod:
                            return new SoqlLiteralExpression((int)v1 % (int)v2);

                        default:
                            throw new NotImplementedException();
                        };
                    }

                    if (v1.GetType() == typeof(string)) {
                        switch (op) {
                        case SoqlBinaryOperator.Add:
                            return new SoqlStringLiteralExpression((string)v1 + (string)v2);

                        default:
                            throw new NotImplementedException();
                        }
                    }

                    // TODO - add more constant folding
                }
            };

            return this;
        }

        public override SoqlExpressionType GetExpressionType() {
            throw new NotImplementedException();
        }
    }
}
