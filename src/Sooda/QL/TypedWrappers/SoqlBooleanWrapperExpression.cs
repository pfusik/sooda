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
using System.Xml;
using System.Xml.Serialization;
using Sooda.Schema;

namespace Sooda.QL.TypedWrappers 
{
    public class SoqlBooleanWrapperExpression : SoqlBooleanExpression 
    {
        private SoqlExpression _innerExpression;

        public SoqlBooleanWrapperExpression()
        {
        }

        public SoqlBooleanWrapperExpression(SoqlExpression innerExpression)
        {
            _innerExpression = innerExpression;
        }

        public SoqlExpression InnerExpression
        {
            get { return _innerExpression; }
            set { _innerExpression = value; }
        }

        public virtual SoqlExpression Simplify() 
        {
            return this;
        }

        public override object Evaluate(ISoqlEvaluateContext context)
        {
            return InnerExpression.Evaluate(context);
        }

        public override void Accept(ISoqlVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static implicit operator SoqlBooleanWrapperExpression(bool v)
        {
            return new SoqlBooleanWrapperExpression(new SoqlLiteralExpression(v));
        }

        public static implicit operator SoqlBooleanWrapperExpression(SoqlParameterLiteralExpression v)
        {
            return new SoqlBooleanWrapperExpression(v);
        }

        public static SoqlBooleanExpression operator ==(SoqlBooleanWrapperExpression left, SoqlBooleanWrapperExpression right) { return new Sooda.QL.SoqlBooleanRelationalExpression(left, right, Sooda.QL.SoqlRelationalOperator.Equal); }
        public static SoqlBooleanExpression operator !=(SoqlBooleanWrapperExpression left, SoqlBooleanWrapperExpression right) { return new Sooda.QL.SoqlBooleanRelationalExpression(left, right, Sooda.QL.SoqlRelationalOperator.NotEqual); }

        public override bool Equals(object o) { return Object.ReferenceEquals(this, o); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
