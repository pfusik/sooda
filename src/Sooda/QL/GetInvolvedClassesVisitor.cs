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

using Sooda.QL.TypedWrappers;
using Sooda.Schema;
using System;
using System.Collections.Generic;

namespace Sooda.QL
{
    /// <summary>
    /// Summary description for GetInvolvedClassesVisitor.
    /// </summary>
    public class GetInvolvedClassesVisitor : ISoqlVisitor
    {
        private ClassInfo _rootClass;
        private readonly List<ClassInfo> _result = new List<ClassInfo>();

        public GetInvolvedClassesVisitor(ClassInfo rootClass)
        {
            _rootClass = rootClass;
        }

        public void GetInvolvedClasses(SoqlExpression expr)
        {
            expr.Accept(this);
        }

        public void GetInvolvedClasses(SoqlQueryExpression query)
        {
            foreach (SoqlExpression expr in query.SelectExpressions)
                expr.Accept(this);
            if (query.WhereClause != null)
                query.WhereClause.Accept(this);
            if (query.Having != null)
                query.Having.Accept(this);
            foreach (SoqlExpression expr in query.GroupByExpressions)
                expr.Accept(this);
            foreach (SoqlExpression expr in query.OrderByExpressions)
                expr.Accept(this);
        }

        public string[] ClassNames
        {
            get
            {
                string[] names = new string[_result.Count];
                for (int i = 0; i < names.Length; i++)
                    names[i] = _result[i].Name;
                return names;
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlTypedWrapperExpression v)
        {
            v.InnerExpression.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanWrapperExpression v)
        {
            v.InnerExpression.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBinaryExpression v)
        {
            v.par1.Accept(this);
            v.par2.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanAndExpression v)
        {
            v.Left.Accept(this);
            v.Right.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanInExpression v)
        {
            v.Left.Accept(this);
            foreach (SoqlExpression expr in v.Right)
            {
                expr.Accept(this);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanIsNullExpression v)
        {
            v.Expr.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanLiteralExpression v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanNegationExpression v)
        {
            v.par.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanOrExpression v)
        {
            v.par1.Accept(this);
            v.par2.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanRelationalExpression v)
        {
            v.par1.Accept(this);
            v.par2.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlExistsExpression v)
        {
            v.Query.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlFunctionCallExpression v)
        {
            if (v.Parameters != null)
            {
                foreach (SoqlExpression e in v.Parameters)
                {
                    e.Accept(this);
                }
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlContainsExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
            v.Expr.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlSoodaClassExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlCountExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlAsteriskExpression v)
        {
            if (v.Left != null)
            {
                v.Left.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlLiteralExpression v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlNullLiteral v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlParameterLiteralExpression v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlPathExpression v)
        {
            v.GetAndAddClassInfo(_rootClass, _result);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlQueryExpression v)
        {
            throw new NotImplementedException();
            // TODO:  Add GetInvolvedClassesVisitor.Sooda.QL.ISoqlVisitor.Visit implementation
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlUnaryNegationExpression v)
        {
            v.par.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlRawExpression v)
        {
            throw new NotSupportedException("RAW queries not supported.");
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlConditionalExpression v)
        {
            v.condition.Accept(this);
            v.ifTrue.Accept(this);
            v.ifFalse.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlStringContainsExpression v)
        {
            v.haystack.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlCastExpression v)
        {
            v.source.Accept(this);
        }
    }
}
