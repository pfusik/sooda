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

namespace Sooda.QL
{
    public class SoqlBooleanAndExpression : SoqlBooleanExpression
    {
        public SoqlBooleanExpression Left;
        public SoqlBooleanExpression Right;

        public SoqlBooleanAndExpression()
        {
        }

        public SoqlBooleanAndExpression(SoqlBooleanExpression Left, SoqlBooleanExpression Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        // visitor pattern
        public override void Accept(ISoqlVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override SoqlExpression Simplify()
        {
            Left = (SoqlBooleanExpression)Left.Simplify();
            Right = (SoqlBooleanExpression)Right.Simplify();

            ISoqlConstantExpression cp1 = Left as ISoqlConstantExpression;
            ISoqlConstantExpression cp2 = Right as ISoqlConstantExpression;

            // left subexpression is false - our node is false
            
            if (cp1 != null && (bool)cp1.GetConstantValue() == false)
                return new SoqlBooleanLiteralExpression(false);

            // right subexpression is false - our node is false

            if (cp2 != null && (bool)cp2.GetConstantValue() == false)
                return new SoqlBooleanLiteralExpression(false);

            // both are constant and not false - our node is true

            if (cp1 != null && cp2 != null)
                return new SoqlBooleanLiteralExpression(true);

            // left subexpression is true - we return the right node

            if (cp1 != null && (bool)cp1.GetConstantValue() == true)
                return Right;

            // right subexpression is true - we return the left node

            if (cp2 != null && (bool)cp2.GetConstantValue() ==  true)
                return Left;

            return this;
        }

        public override SoqlExpressionType GetExpressionType() 
        {
            return new SoqlExpressionType(typeof(bool));
        }
    }
}
