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
using System.Data;
using System.Xml;

using Sooda.QL;

namespace Sooda 
{
    public class SoodaWhereClause
    {
        private SoqlBooleanExpression whereExpression;
        private object[] parameters = null;

        public SoodaWhereClause() : this((string)null, null) {}
        public SoodaWhereClause(string whereText) : this(whereText, null) {}
        public SoodaWhereClause(string whereText, params object[] par) 
        {
            this.Parameters = par;
            if (whereText != null) 
            {
                this.WhereExpression = SoqlParser.ParseWhereClause(whereText);
            } 
            else 
            {
                this.WhereExpression = null;
            }
        }

        public SoodaWhereClause(SoqlBooleanExpression whereExpression) 
        {
            this.WhereExpression = whereExpression;
        }

        public SoodaWhereClause(SoqlBooleanExpression whereExpression, params object[] par) 
        {
            this.Parameters = par;
            this.WhereExpression = whereExpression;
        }

        public SoqlBooleanExpression WhereExpression
        {
            get 
            {
                return whereExpression;
            }
            set 
            {
                whereExpression = value;
            }
        }

        public object[] Parameters
        {
            get 
            {
                return this.parameters;
            }
            set 
            {
                if (value != null && value.Length != 0) 
                {
                    this.parameters = value;
                } 
                else 
                {
                    this.parameters = null;
                }
            }
        }


        public SoodaWhereClause Append(SoodaWhereClause other) 
        {
            if (other.WhereExpression == null)
                return this;
            if (this.WhereExpression == null)
                return other;

            object[] newParams = this.Parameters;

            if (this.Parameters == null)
                newParams = other.Parameters;
            else if (other.Parameters != null)
                throw new SoodaException("You cannot merge two where clauses when they both have parameters");

            return new SoodaWhereClause(new SoqlBooleanAndExpression(
                this.WhereExpression, other.WhereExpression), newParams);
        }

        public bool Matches(SoodaObject obj, bool throwOnUnknown)
        {
            if (this.WhereExpression == null)
                return true;

            EvaluateContext context = new EvaluateContext(this, obj);
            object val = this.WhereExpression.Evaluate(context);
            if (val == null && throwOnUnknown)
                throw new SoqlException("Cannot evaluate expression '" + this.whereExpression.ToString() + " ' in memory.");
                
            if (val is bool)
                return (bool)val;
            else
                return false;
        }

        public override string ToString()
        {
            return (this.WhereExpression != null) ? this.WhereExpression.ToString() : "";
        }

        public static readonly SoodaWhereClause Unrestricted = new SoodaWhereClause((string)null);

        class EvaluateContext : ISoqlEvaluateContext
        {
            private SoodaWhereClause _whereClause;
            private SoodaObject _rootObject;

            public EvaluateContext(SoodaWhereClause whereClause, SoodaObject rootObject)
            {
                _whereClause = whereClause;
                _rootObject = rootObject;
            }

            public object GetRootObject()
            {
                return _rootObject;
            }

            public object GetParameter(int position)
            {
                return _whereClause.Parameters[position];
            }
        }
    }
}

