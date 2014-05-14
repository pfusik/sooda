//
// Copyright (c) 2010-2014 Piotr Fusik <piotr@fusik.info>
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

#if DOTNET35

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Sooda.QL;

namespace Sooda.Linq
{
    public class SoodaQueryable<T> : IOrderedQueryable<T>, IQueryProvider
    {
        readonly Expression _expression;

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return this;
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return _expression;
            }
        }

        Type IQueryable.ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        internal SoodaQueryable()
        {
            _expression = Expression.Constant(this);
        }

        public SoodaQueryable(Expression expression)
        {
            _expression = expression;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable ie = Execute<IEnumerable>();
            return ie.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerable ie = Execute<IEnumerable>();
            return ie.Cast<T>().GetEnumerator();
        }

        static Type GetElementType(Type seqType)
        {
            // array?
            Type elementType = seqType.GetElementType();
            if (elementType != null)
                return elementType;

            do
            {
                if (seqType.IsGenericType && seqType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return seqType.GetGenericArguments()[0];

                // : IX1, IX2, ... -> try GetElementType(IX1), ...
                foreach (Type iface in seqType.GetInterfaces())
                {
                    elementType = GetElementType(iface);
                    if (elementType != null)
                        return elementType;
                }

                // GetElementType(baseType)
                seqType = seqType.BaseType;
            } while (seqType != null && seqType != typeof(object));

            throw new ArgumentException("Type is not IEnumerable<T>");
        }

        IQueryable IQueryProvider.CreateQuery(Expression expr)
        {
            Type elementType = GetElementType(expr.Type);
            return (IQueryable) Activator.CreateInstance(typeof(SoodaQueryable<>).MakeGenericType(elementType), expr);
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expr)
        {
            return new SoodaQueryable<TElement>(expr);
        }

        object IQueryProvider.Execute(Expression expr)
        {
            return new SoodaQueryExecutor().Execute(expr);
        }

        TResult IQueryProvider.Execute<TResult>(Expression expr)
        {
            return (TResult) new SoodaQueryExecutor().Execute(expr);
        }

        internal TResult Execute<TResult>()
        {
            return (TResult) new SoodaQueryExecutor().Execute(_expression);
        }
    }
}

#endif
