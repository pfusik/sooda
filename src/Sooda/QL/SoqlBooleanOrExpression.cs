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


namespace Sooda.QL
{
    public class SoqlBooleanOrExpression : SoqlBooleanExpression
    {
        public SoqlBooleanExpression par1;
        public SoqlBooleanExpression par2;

        public SoqlBooleanOrExpression() { }

        public SoqlBooleanOrExpression(SoqlBooleanExpression par1, SoqlBooleanExpression par2)
        {
            this.par1 = par1;
            this.par2 = par2;
        }

        // visitor pattern
        public override void Accept(ISoqlVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override SoqlExpression Simplify()
        {
            par1 = (SoqlBooleanExpression)par1.Simplify();
            par2 = (SoqlBooleanExpression)par2.Simplify();

            ISoqlConstantExpression cp1 = par1 as ISoqlConstantExpression;
            ISoqlConstantExpression cp2 = par2 as ISoqlConstantExpression;

            // left subexpression is true - our node is true

            if (cp1 != null && (bool)cp1.GetConstantValue() == true)
                return new SoqlBooleanLiteralExpression(true);

            // right subexpression is true - our node is true

            if (cp2 != null && (bool)cp2.GetConstantValue() == true)
                return new SoqlBooleanLiteralExpression(true);

            // both are constant and false - our node is false

            if (cp1 != null && cp2 != null)
                return new SoqlBooleanLiteralExpression(false);

            if (cp1 != null && (bool)cp1.GetConstantValue() == false)
                return par2;

            if (cp2 != null && (bool)cp2.GetConstantValue() == false)
                return par1;

            return this;
        }

        public override object Evaluate(ISoqlEvaluateContext context)
        {
            object val1 = par1.Evaluate(context);
            if (val1 == null)
                return null;

            bool bval1 = (bool)val1;
            if (bval1)
                return true;

            object val2 = par2.Evaluate(context);
            if (val2 == null)
                return null;

            bool bval2 = (bool)val2;
            if (bval2)
                return true;

            return false;
        }
    }
}
