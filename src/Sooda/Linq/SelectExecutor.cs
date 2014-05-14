//
// Copyright (c) 2014 Piotr Fusik <piotr@fusik.info>
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

#if DOTNET4

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

using Sooda.QL;

namespace Sooda.Linq
{
    class SelectExecutor : ExpressionVisitor
    {
        readonly SoodaQueryExecutor _executor;
        readonly List<ParameterExpression> _parameters = new List<ParameterExpression>();
        readonly List<SoqlExpression> _soqls = new List<SoqlExpression>();
        Expression _body;

        internal SelectExecutor(SoodaQueryExecutor executor)
        {
            _executor = executor;
        }

        public override Expression Visit(Expression node)
        {
            if (SoodaQueryExecutor.IsConstant(node))
                return node;

            SoqlExpression soql;
            try
            {
                soql = _executor.TranslateExpression(node);
            }
            catch (NotSupportedException)
            {
                return base.Visit(node);
            }

            ParameterExpression pe = Expression.Parameter(node.Type);
            _parameters.Add(pe);
            _soqls.Add(soql);
            return pe;
        }

        internal void Process(Expression body)
        {
            _body = Visit(body);
        }

        internal List<T> GetList<T>()
        {
            using (IDataReader r = _executor.ExecuteQuery(_soqls))
            {
                List<T> list = new List<T>();
                if (_parameters.Count == 1 && _body == _parameters[0])
                {
                    while (r.Read())
                    {
                        object value = r.GetValue(0);
                        if (value == DBNull.Value)
                            value = null;
                        else if (typeof(T) == typeof(bool) && value is int)
                            value = (int) value != 0;
                        list.Add((T) value);
                    }
                }
                else
                {
                    Delegate d = Expression.Lambda(_body, _parameters).Compile();
                    object[] columns = new object[_parameters.Count];
                    while (r.Read())
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            object value = r.GetValue(i);
                            if (value == DBNull.Value)
                                value = null;
                            else if (_parameters[i].Type == typeof(bool) && value is int)
                                value = (int) value != 0;
                            columns[i] = value;
                        }
                        list.Add((T) d.DynamicInvoke(columns));
                    }
                }
                return list;
            }
        }

        internal IList GetList()
        {
            return (IList) SoodaLinqMethodDictionary.Get(SoodaLinqMethod.SelectExecutor_GetList).MakeGenericMethod(_body.Type).Invoke(this, null);
        }
    }
}

#endif
