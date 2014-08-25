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
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

using Sooda.QL;
using Sooda.Utils;

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

        protected override Expression VisitMember(MemberExpression node)
        {
            NewExpression ne = _executor.SubstituteGroupingKey(node);
            if (ne != null)
                return VisitNew(ne);
            return base.VisitMember(node);
        }

        public override Expression Visit(Expression node)
        {
            if (SoodaQueryExecutor.IsConstant(node))
                return node;

            if (node.NodeType == ExpressionType.Convert)
                return base.Visit(node);

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

        internal void Process(LambdaExpression lambda)
        {
            _body = Visit(lambda.Body);
            if (lambda.Parameters.Count == 2)
                _parameters.Add(lambda.Parameters[1]);
        }

        internal SoqlExpression GetSingleColumnExpression(out Type type)
        {
            if (_soqls.Count != 1 || _body != _parameters[0])
                throw new NotSupportedException("Select() is not scalar");
            type = _body.Type;
            return _soqls[0];
        }

        Func<object, object> GetHandler(Type type)
        {
            if (type == typeof(int))
                return value => Convert.ToInt32(value);
            if (type == typeof(int?))
                return value => value == DBNull.Value ? null : (object) Convert.ToInt32(value);
            if (type == typeof(bool))
                return value => Convert.ToBoolean(value);
            if (type == typeof(bool?))
                return value => value == DBNull.Value ? null : (object) Convert.ToBoolean(value);
            if (type.IsSubclassOf(typeof(SoodaObject)))
                return value => value == DBNull.Value ? null : _executor.GetRef(type, value);
            if (typeof(INullable).IsAssignableFrom(type))
                return value => SqlTypesUtil.Wrap(type, value == DBNull.Value ? null : value);
            if (type == typeof(TimeSpan))
                return value => TimeSpan.FromSeconds(Convert.ToInt32(value));
            if (type == typeof(TimeSpan?))
                return value => value == DBNull.Value ? null : (object) TimeSpan.FromSeconds(Convert.ToInt32(value));
            return value => value == DBNull.Value ? null : value;
        }

        IList GetGenericList<T>()
        {
            if (_soqls.Count == 1 && _body == _parameters[0])
            {
                List<T> list = new List<T>();
                Func<object, object> handler = GetHandler(typeof(T));
                using (IDataReader r = _executor.ExecuteQuery(_soqls))
                {
                    while (r.Read())
                    {
                        object value = r.GetValue(0);
                        value = handler(value); // FIXME: can fail if T is SoodaObject with subclasses - concurrent query
                        list.Add((T) value);
                    }
                }
                return list;
            }

            // First fetch all rows, call projection later.
            // The projection possibly executes database queries
            // and concurrent queries on the same connection
            // are unsupported in SQL server by default.
            List<object[]> rows = new List<object[]>();
            int columnCount = _soqls.Count;
            using (IDataReader r = _executor.ExecuteQuery(_soqls))
            {
                while (r.Read())
                {
                    object[] row = new object[columnCount];
                    for (int i = 0; i < columnCount; i++)
                        row[i] = r.GetValue(i);
                    rows.Add(row);
                }
            }

            Func<object, object>[] handlers = new Func<object, object>[columnCount];
            for (int i = 0; i < columnCount; i++)
                handlers[i] = GetHandler(_parameters[i].Type);
            object[] args = new object[_parameters.Count];
            Delegate d = Expression.Lambda(_body, _parameters).Compile();
            T[] array = new T[rows.Count];
            for (int rowNum = 0; rowNum < rows.Count; rowNum++)
            {
                object[] row = rows[rowNum];
                for (int i = 0; i < columnCount; i++)
                {
                    object value = row[i];
                    args[i] = handlers[i](value);
                }
                if (columnCount < args.Length)
                    args[columnCount] = rowNum;
                array[rowNum] = (T) d.DynamicInvoke(args);
            }
            return array;
        }

        internal IList GetList()
        {
            MethodInfo method = typeof(SelectExecutor).GetMethod("GetGenericList", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IList) method.MakeGenericMethod(_body.Type).Invoke(this, null);
        }
    }
}

#endif
