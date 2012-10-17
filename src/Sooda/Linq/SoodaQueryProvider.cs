//
// Copyright (c) 2010-2012 Piotr Fusik <piotr@fusik.info>
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
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Sooda;
using Sooda.ObjectMapper;
using Sooda.QL;

namespace Sooda.Linq
{
    public class SoodaQueryProvider : IQueryProvider
    {
        readonly SoodaTransaction _transaction;
        readonly Sooda.Schema.ClassInfo _classInfo;
        readonly SoodaSnapshotOptions _options;
        SoqlBooleanExpression _where;
        SoodaOrderBy _orderBy;
        int _topCount;

        public SoodaQueryProvider(SoodaTransaction transaction, Sooda.Schema.ClassInfo classInfo, SoodaSnapshotOptions options)
        {
            _transaction = transaction;
            _classInfo = classInfo;
            _options = options;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expr)
        {
            throw new NotImplementedException(); // TODO
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expr)
        {
            return new SoodaQueryable<TElement>(this, expr);
        }

        static bool IsConstant(Expression expr)
        {
            if (expr == null)
                return true;
            switch (expr.NodeType)
            {
            case ExpressionType.Constant:
                return true;
            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
                BinaryExpression be = (BinaryExpression) expr;
                return IsConstant(be.Left) && IsConstant(be.Right);
            case ExpressionType.MemberAccess:
                return IsConstant(((MemberExpression) expr).Expression);
            case ExpressionType.Call:
                MethodCallExpression mc = (MethodCallExpression) expr;
                return IsConstant(mc.Object) && mc.Arguments.All(arg => IsConstant(arg));
            default:
                throw new NotSupportedException(expr.NodeType.ToString());
            }
        }

        static SoqlLiteralExpression FoldConstant(Expression expr, Func<string> error)
        {
            if (IsConstant(expr))
                return new SoqlLiteralExpression(Expression.Lambda(expr).Compile().DynamicInvoke(null));
            throw new NotSupportedException(error());
        }

        SoqlBooleanExpression TranslateAnd(BinaryExpression expr)
        {
            return TranslateBoolean(expr.Left).And(TranslateBoolean(expr.Right));
        }

        SoqlBooleanExpression TranslateOr(BinaryExpression expr)
        {
            return TranslateBoolean(expr.Left).Or(TranslateBoolean(expr.Right));
        }

        SoqlBinaryExpression TranslateBinary(BinaryExpression expr, SoqlBinaryOperator op)
        {
            return new SoqlBinaryExpression(TranslateExpression(expr.Left), TranslateExpression(expr.Right), op);
        }

        SoqlBooleanExpression TranslateRelational(BinaryExpression expr, SoqlRelationalOperator op)
        {
            SoqlExpression left = TranslateExpression(expr.Left).Simplify();
            SoqlExpression right = TranslateExpression(expr.Right).Simplify();
            SoqlLiteralExpression rightConst = right as SoqlLiteralExpression;
            if (rightConst != null && rightConst.GetConstantValue() == null
                && !(left is SoqlLiteralExpression))
            {
                switch (op)
                {
                case SoqlRelationalOperator.Equal:
                    return new SoqlBooleanIsNullExpression(left, false);
                case SoqlRelationalOperator.NotEqual:
                    return new SoqlBooleanIsNullExpression(left, true);
                default:
                    throw new NotSupportedException(op + " NULL");
                }
            }
            return new SoqlBooleanRelationalExpression(left, right, op);
        }

        SoqlExpression TranslateMember(MemberExpression expr)
        {
            string name = expr.Member.Name;
            // x.SoodaField -> SoqlPathExpression
            if (expr.Expression.NodeType == ExpressionType.Parameter)
                return new SoqlPathExpression(name); // FIXME: different parameters
            Type t = expr.Member.DeclaringType;

            // x.GetType().Name -> SoqlSoodaClassExpression
            if (t == typeof(MemberInfo) && name == "Name" && expr.Expression.NodeType == ExpressionType.Call)
            {
                MethodCallExpression mc = (MethodCallExpression) expr.Expression;
                if (mc.Method.DeclaringType == typeof(object) && mc.Method.Name == "GetType")
                {
                    if (mc.Object.NodeType == ExpressionType.Parameter)
                        return new SoqlSoodaClassExpression();
                    return new SoqlSoodaClassExpression((SoqlPathExpression) TranslateExpression(mc.Object));
                }
            }

            SoqlExpression parent = TranslateExpression(expr.Expression);
            SoqlPathExpression parentPath = parent as SoqlPathExpression;
            if (parentPath != null)
            {
                if (expr.Member.MemberType == MemberTypes.Property)
                {
                    // x.SoodaField1.SoodaField2 -> SoqlPathExpression
                    if (t.IsSubclassOf(typeof(SoodaObject)))
                        return new SoqlPathExpression(parentPath, name);

                    if (typeof(INullable).IsAssignableFrom(t))
                    {
                        // x.SoodaField1.Value -> x.SoodaField1
                        if (name == "Value")
                            return parent;
                        // x.SoodaField1.IsNull -> SoqlBooleanIsNullExpression
                        if (name == "IsNull")
                            return new SoqlBooleanIsNullExpression(parent, false);
                    }

                    // x.SoodaCollection.Count -> SoqlCountExpression
                    if (t == typeof(SoodaObjectCollectionWrapper) && name == "Count")
                        return new SoqlCountExpression(parentPath.Left, parentPath.PropertyName);
                }
                throw new NotSupportedException(t.FullName + "." + name);
            }

            return FoldConstant(expr, () => t.FullName + "." + name);
        }

        SoqlExpression TranslateCall(MethodCallExpression mc)
        {
            // x.SoodaCollection.Contains(expr) -> SoqlContainsExpression
            Type cwg = mc.Method.DeclaringType.BaseType;
            if (cwg != null && cwg.IsGenericType && cwg.GetGenericTypeDefinition() == typeof(SoodaObjectCollectionWrapperGeneric<>) && mc.Method.Name == "Contains")
            {
                SoqlPathExpression haystack = (SoqlPathExpression) TranslateExpression(mc.Object);
                SoqlExpression needle = TranslateExpression(mc.Arguments[0]);
                return new SoqlContainsExpression(haystack.Left, haystack.PropertyName, needle);
            }

            return FoldConstant(mc, () => mc.Method.DeclaringType.FullName + "." + mc.Method.Name);
        }

        SoqlExpression TranslateExpression(Expression expr)
        {
            switch (expr.NodeType)
            {
            case ExpressionType.Constant:
                return new SoqlLiteralExpression(((ConstantExpression) expr).Value);
            case ExpressionType.Parameter:
                Sooda.Schema.FieldInfo[] pks = _classInfo.GetPrimaryKeyFields();
                if (pks.Length != 1)
                    throw new NotSupportedException("composite primary key");
                return new SoqlPathExpression(pks[0].Name);
            case ExpressionType.MemberAccess:
                return TranslateMember((MemberExpression) expr);
            case ExpressionType.Add:
                return TranslateBinary((BinaryExpression) expr, SoqlBinaryOperator.Add);
            case ExpressionType.Subtract:
                return TranslateBinary((BinaryExpression) expr, SoqlBinaryOperator.Sub);
            case ExpressionType.Multiply:
                return TranslateBinary((BinaryExpression) expr, SoqlBinaryOperator.Mul);
            case ExpressionType.Divide:
                return TranslateBinary((BinaryExpression) expr, SoqlBinaryOperator.Div);
            case ExpressionType.Modulo:
                return TranslateBinary((BinaryExpression) expr, SoqlBinaryOperator.Mod);
            case ExpressionType.Negate:
                return new SoqlUnaryNegationExpression(TranslateExpression(((UnaryExpression) expr).Operand));
            case ExpressionType.And:
            case ExpressionType.AndAlso:
                return TranslateAnd((BinaryExpression) expr);
            case ExpressionType.Or:
            case ExpressionType.OrElse:
                return TranslateOr((BinaryExpression) expr);
            case ExpressionType.Not:
                return new SoqlBooleanNegationExpression(TranslateBoolean(((UnaryExpression) expr).Operand));
            case ExpressionType.Equal:
                return TranslateRelational((BinaryExpression) expr, SoqlRelationalOperator.Equal);
            case ExpressionType.NotEqual:
                return TranslateRelational((BinaryExpression) expr, SoqlRelationalOperator.NotEqual);
            case ExpressionType.LessThan:
                return TranslateRelational((BinaryExpression) expr, SoqlRelationalOperator.Less);
            case ExpressionType.LessThanOrEqual:
                return TranslateRelational((BinaryExpression) expr, SoqlRelationalOperator.LessOrEqual);
            case ExpressionType.GreaterThan:
                return TranslateRelational((BinaryExpression) expr, SoqlRelationalOperator.Greater);
            case ExpressionType.GreaterThanOrEqual:
                return TranslateRelational((BinaryExpression) expr, SoqlRelationalOperator.GreaterOrEqual);
            case ExpressionType.Call:
                return TranslateCall((MethodCallExpression) expr);
            default:
                throw new NotSupportedException(expr.NodeType.ToString());
            }
        }

        SoqlBooleanExpression TranslateBoolean(Expression expr)
        {
            SoqlExpression ql = TranslateExpression(expr);
            return ql as SoqlBooleanExpression ?? new SoqlBooleanRelationalExpression(ql, SoqlBooleanLiteralExpression.True, SoqlRelationalOperator.Equal);
        }

        LambdaExpression GetLambda(MethodCallExpression mc)
        {
            if (mc.Arguments.Count == 2)
            {
                LambdaExpression lambda = (LambdaExpression) ((UnaryExpression) mc.Arguments[1]).Operand;
                if (lambda.Parameters.Count == 1)
                    return lambda;
            }
            throw new NotSupportedException("Unsupported overload of " + mc.Method.Name);
        }

        void TranslateQuery(Expression expr)
        {
            switch (expr.NodeType)
            {
            case ExpressionType.Constant:
                break;
            case ExpressionType.Call:
                MethodCallExpression mc = (MethodCallExpression) expr;
                if (mc.Method.DeclaringType != typeof(Queryable))
                    throw new NotSupportedException(mc.Method.DeclaringType.FullName);
                TranslateQuery(mc.Arguments[0]);
                LambdaExpression lambda;
                switch (mc.Method.Name)
                {
                case "Where":
                    lambda = GetLambda(mc);
                    if (_topCount >= 0)
                        throw new NotSupportedException("Take().Where() not supported");
                    SoqlBooleanExpression where = (SoqlBooleanExpression) TranslateBoolean(lambda.Body).Simplify();
                    _where = _where == null ? where : _where.And(where);
                    break;

                case "OrderBy":
                case "OrderByDescending":
                case "ThenBy":
                case "ThenByDescending":
                    lambda = GetLambda(mc);
                    SoqlExpression orderBy = TranslateExpression(lambda.Body);
                    if (_topCount >= 0)
                        throw new NotSupportedException("Take().OrderBy() not supported");
                    switch (mc.Method.Name)
                    {
                    case "OrderBy":
                        _orderBy = new SoodaOrderBy(orderBy, SortOrder.Ascending, _orderBy);
                        break;
                    case "OrderByDescending":
                        _orderBy = new SoodaOrderBy(orderBy, SortOrder.Descending, _orderBy);
                        break;
                    case "ThenBy":
                        _orderBy = new SoodaOrderBy(_orderBy, orderBy, SortOrder.Ascending);
                        break;
                    case "ThenByDescending":
                        _orderBy = new SoodaOrderBy(_orderBy, orderBy, SortOrder.Descending);
                        break;
                    }
                    break;

                case "Take":
                    int count = (int) ((ConstantExpression) mc.Arguments[1]).Value;
                    if (count < 0)
                        count = 0;
                    if (_topCount < 0 || _topCount > count)
                        _topCount = count;
                    break;

                default:
                    throw new NotSupportedException(mc.Method.Name);
                }
                break;
            default:
                throw new NotSupportedException(expr.NodeType.ToString());
            }
        }

        IEnumerable GetList()
        {
            return new SoodaObjectListSnapshot(_transaction, new SoodaWhereClause(_where), _orderBy, _topCount, _options, _classInfo);
        }

        static IEnumerable Select(IEnumerable source, Delegate d)
        {
            foreach (object obj in source)
                yield return d.DynamicInvoke(obj);
        }

        public object Execute(Expression expr)
        {
            _where = null;
            _orderBy = null;
            _topCount = -1;
            MethodCallExpression mc = expr as MethodCallExpression;
            if (mc != null && mc.Method.DeclaringType == typeof(Queryable) && mc.Method.Name == "Select")
            {
                TranslateQuery(mc.Arguments[0]);
                return Select(GetList(), GetLambda(mc).Compile());
            }
            TranslateQuery(expr);
            return GetList();
        }

        public TResult Execute<TResult>(Expression expr)
        {
            return (TResult) Execute(expr);
        }
    }
}

#endif
