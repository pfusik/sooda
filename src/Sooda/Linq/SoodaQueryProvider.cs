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
using System.Data;
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
                case ExpressionType.Parameter:
                    return false;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return IsConstant(((UnaryExpression) expr).Operand);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    BinaryExpression be = (BinaryExpression) expr;
                    return IsConstant(be.Left) && IsConstant(be.Right);
                case ExpressionType.Conditional:
                    ConditionalExpression ce = (ConditionalExpression) expr;
                    return IsConstant(ce.Test) && IsConstant(ce.IfTrue) && IsConstant(ce.IfFalse);
                case ExpressionType.MemberAccess:
                    return IsConstant(((MemberExpression) expr).Expression);
                case ExpressionType.Call:
                    MethodCallExpression mc = (MethodCallExpression) expr;
                    return IsConstant(mc.Object) && mc.Arguments.All(IsConstant);
                case ExpressionType.Lambda:
                    return IsConstant(((LambdaExpression) expr).Body);
                case ExpressionType.New:
                    return ((NewExpression) expr).Arguments.All(IsConstant);
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                    return ((NewArrayExpression) expr).Expressions.All(IsConstant);
                case ExpressionType.Invoke:
                    InvocationExpression ie = (InvocationExpression) expr;
                    return IsConstant(ie.Expression) && ie.Arguments.All(IsConstant);
                case ExpressionType.MemberInit:
                case ExpressionType.ListInit:
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
                if (SoodaLinqMethodUtil.Get(mc.Method) == SoodaLinqMethod.Object_GetType)
                {
                    if (mc.Object.NodeType == ExpressionType.Parameter)
                        return new SoqlSoodaClassExpression();
                    return new SoqlSoodaClassExpression((SoqlPathExpression) TranslateExpression(mc.Object));
                }
            }

            SoqlExpression parent = TranslateExpression(expr.Expression);

            if (typeof(INullable).IsAssignableFrom(t))
            {
                if (name == "Value")
                    return parent;
                if (name == "IsNull")
                    return new SoqlBooleanIsNullExpression(parent, false);
            }

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (name == "Value")
                    return parent;
                if (name == "HasValue")
                    return new SoqlBooleanIsNullExpression(parent, true);
            }

            if (t == typeof(DateTime))
            {
                switch (name)
                {
                    case "Day":
                        return new SoqlFunctionCallExpression("day", parent);
                    case "Month":
                        return new SoqlFunctionCallExpression("month", parent);
                    case "Year":
                        return new SoqlFunctionCallExpression("year", parent);
                    default:
                        break;
                }
            }

            SoqlPathExpression parentPath = parent as SoqlPathExpression;
            if (parentPath != null)
            {
                if (expr.Member.MemberType == MemberTypes.Property)
                {
                    // x.SoodaField1.SoodaField2 -> SoqlPathExpression
                    if (t.IsSubclassOf(typeof(SoodaObject)))
                        return new SoqlPathExpression(parentPath, name);

                    // x.SoodaCollection.Count -> SoqlCountExpression
                    if (t == typeof(SoodaObjectCollectionWrapper) && name == "Count")
                        return new SoqlCountExpression(parentPath.Left, parentPath.PropertyName);
                }
                throw new NotSupportedException(t.FullName + "." + name);
            }

            return FoldConstant(expr, () => t.FullName + "." + name);
        }

        SoqlBooleanExpression TranslateCollectionAny(MethodCallExpression mc, SoqlBooleanExpression where)
        {
            SoqlPathExpression parentPath = (SoqlPathExpression) TranslateExpression(mc.Arguments[0]);
            SoqlQueryExpression query = new SoqlQueryExpression();
            query.From.Add(mc.Method.GetGenericArguments()[0].Name);
            query.FromAliases.Add(string.Empty);
            query.WhereClause = where;
            return new SoqlContainsExpression(parentPath.Left, parentPath.PropertyName, query);
        }

        SoqlExpression TranslateCall(MethodCallExpression mc)
        {
            LambdaExpression lambda;
            switch (SoodaLinqMethodUtil.Get(mc.Method))
            {
                case SoodaLinqMethod.Enumerable_All:
                    lambda = (LambdaExpression) mc.Arguments[1];
                    return new SoqlBooleanNegationExpression(TranslateCollectionAny(mc, new SoqlBooleanNegationExpression(TranslateBoolean(lambda.Body))));
                case SoodaLinqMethod.Enumerable_Any:
                    return TranslateCollectionAny(mc, SoqlBooleanLiteralExpression.True);
                case SoodaLinqMethod.Enumerable_AnyFiltered:
                    lambda = (LambdaExpression) mc.Arguments[1];
                    return TranslateCollectionAny(mc, TranslateBoolean(lambda.Body));
                case SoodaLinqMethod.Enumerable_Count:
                    SoqlPathExpression parentPath = (SoqlPathExpression) TranslateExpression(mc.Arguments[0]);
                    return new SoqlCountExpression(parentPath.Left, parentPath.PropertyName);
                case SoodaLinqMethod.String_Concat:
                    return new SoqlFunctionCallExpression("concat", TranslateExpression(mc.Arguments[0]), TranslateExpression(mc.Arguments[1]));
                case SoodaLinqMethod.String_Like:
                    return new SoqlBooleanRelationalExpression(
                        TranslateExpression(mc.Arguments[0]),
                        TranslateExpression(mc.Arguments[1]),
                        SoqlRelationalOperator.Like);
                case SoodaLinqMethod.String_Remove:
                    return new SoqlFunctionCallExpression("left", TranslateExpression(mc.Object), TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.String_Replace:
                    SoqlExpressionCollection parameters = new SoqlExpressionCollection {
                        TranslateExpression(mc.Object),
                        TranslateExpression(mc.Arguments[0]),
                        TranslateExpression(mc.Arguments[1])
                    };
                    return new SoqlFunctionCallExpression("replace", parameters);
                case SoodaLinqMethod.String_ToLower:
                    return new SoqlFunctionCallExpression("lower", TranslateExpression(mc.Object));
                case SoodaLinqMethod.String_ToUpper:
                    return new SoqlFunctionCallExpression("upper", TranslateExpression(mc.Object));
                case SoodaLinqMethod.Math_Abs:
                    return new SoqlFunctionCallExpression("abs", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Acos:
                    return new SoqlFunctionCallExpression("acos", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Asin:
                    return new SoqlFunctionCallExpression("asin", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Atan:
                    return new SoqlFunctionCallExpression("atan", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Cos:
                    return new SoqlFunctionCallExpression("cos", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Exp:
                    return new SoqlFunctionCallExpression("exp", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Floor:
                    return new SoqlFunctionCallExpression("floor", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Pow:
                    return new SoqlFunctionCallExpression("power", TranslateExpression(mc.Arguments[0]), TranslateExpression(mc.Arguments[1]));
                case SoodaLinqMethod.Math_Round:
                    return new SoqlFunctionCallExpression("round", TranslateExpression(mc.Arguments[0]), TranslateExpression(mc.Arguments[1]));
                case SoodaLinqMethod.Math_Sign:
                    return new SoqlFunctionCallExpression("sign", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Sin:
                    return new SoqlFunctionCallExpression("sin", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Sqrt:
                    return new SoqlFunctionCallExpression("sqrt", TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.Math_Tan:
                    return new SoqlFunctionCallExpression("tan", TranslateExpression(mc.Arguments[0]));
                default:
                    break;
            }

            Type t = mc.Method.DeclaringType;

            // x.SoodaCollection.Contains(expr) -> SoqlContainsExpression
            Type cwg = t.BaseType;
            if (cwg != null && cwg.IsGenericType && cwg.GetGenericTypeDefinition() == typeof(SoodaObjectCollectionWrapperGeneric<>) && mc.Method.Name == "Contains")
            {
                SoqlPathExpression haystack = (SoqlPathExpression) TranslateExpression(mc.Object);
                SoqlExpression needle;
                if (mc.Arguments[0].NodeType == ExpressionType.Parameter)
                {
                    Sooda.Schema.FieldInfo[] pks = _classInfo.GetPrimaryKeyFields();
                    if (pks.Length != 1)
                        throw new NotSupportedException(t.FullName + ".Contains(composite_primary_key)");
                    needle = new SoqlPathExpression(pks[0].Name);
                }
                else
                    needle = TranslateExpression(mc.Arguments[0]);
                return new SoqlContainsExpression(haystack.Left, haystack.PropertyName, needle);
            }

            return FoldConstant(mc, () => t.FullName + "." + mc.Method.Name);
        }

        SoqlExpression TranslateCoalesce(BinaryExpression expr)
        {
            return new SoqlFunctionCallExpression("coalesce", TranslateExpression(expr.Left), TranslateExpression(expr.Right));
        }

        SoqlExpression TranslateExpression(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                    return new SoqlLiteralExpression(((ConstantExpression) expr).Value);
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
                case ExpressionType.Coalesce:
                    return TranslateCoalesce((BinaryExpression) expr);
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
            return (LambdaExpression) ((UnaryExpression) mc.Arguments[1]).Operand;
        }

        void Where(MethodCallExpression mc)
        {
            if (_topCount >= 0)
                throw new NotSupportedException("Take().Where() not supported");
            LambdaExpression lambda = GetLambda(mc);
            SoqlBooleanExpression where = (SoqlBooleanExpression) TranslateBoolean(lambda.Body).Simplify();
            _where = _where == null ? where : _where.And(where);
        }

        void Reverse()
        {
            if (_topCount >= 0)
                throw new NotSupportedException("Take().Reverse() not supported");
            if (_orderBy != null)
            {
                SortOrder[] sortOrders = _orderBy.SortOrders;
                for (int i = 0; i < sortOrders.Length; i++)
                    sortOrders[i] = sortOrders[i] == SortOrder.Descending ? SortOrder.Ascending : SortOrder.Descending;
                _orderBy = new SoodaOrderBy(_orderBy.OrderByExpressions, sortOrders);
            }
            else
            {
                // There was no order - order by primary keys descending.
                // This should do the trick for SQL Server if the primary keys are clustered.
                Sooda.Schema.FieldInfo[] pks = _classInfo.GetPrimaryKeyFields();
                string[] columnNames = new string[pks.Length];
                SortOrder[] sortOrders = new SortOrder[pks.Length];
                for (int i = 0; i < pks.Length; i++)
                {
                    columnNames[i] = pks[i].Name;
                    sortOrders[i] = SortOrder.Descending;
                }
                _orderBy = new SoodaOrderBy(columnNames, sortOrders);
            }
        }

        void Take(int count)
        {
            if (_topCount < 0 || _topCount > count)
                _topCount = count;
        }

        void TranslateQuery(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                    break;
                case ExpressionType.Call:
                    MethodCallExpression mc = (MethodCallExpression) expr;
                    SoodaLinqMethod method = SoodaLinqMethodUtil.Get(mc.Method);
                    TranslateQuery(mc.Arguments[0]);
                    LambdaExpression lambda;
                    switch (method)
                    {
                        case SoodaLinqMethod.Queryable_Where:
                            Where(mc);
                            break;

                        case SoodaLinqMethod.Queryable_OrderBy:
                        case SoodaLinqMethod.Queryable_OrderByDescending:
                        case SoodaLinqMethod.Queryable_ThenBy:
                        case SoodaLinqMethod.Queryable_ThenByDescending:
                            lambda = GetLambda(mc);
                            SoqlExpression orderBy = TranslateExpression(lambda.Body);
                            if (_topCount >= 0)
                                throw new NotSupportedException("Take().OrderBy() not supported");
                            switch (method)
                            {
                                case SoodaLinqMethod.Queryable_OrderBy:
                                    _orderBy = new SoodaOrderBy(orderBy, SortOrder.Ascending, _orderBy);
                                    break;
                                case SoodaLinqMethod.Queryable_OrderByDescending:
                                    _orderBy = new SoodaOrderBy(orderBy, SortOrder.Descending, _orderBy);
                                    break;
                                case SoodaLinqMethod.Queryable_ThenBy:
                                    _orderBy = new SoodaOrderBy(_orderBy, orderBy, SortOrder.Ascending);
                                    break;
                                case SoodaLinqMethod.Queryable_ThenByDescending:
                                    _orderBy = new SoodaOrderBy(_orderBy, orderBy, SortOrder.Descending);
                                    break;
                            }
                            break;
                        case SoodaLinqMethod.Queryable_Reverse:
                            Reverse();
                            break;

                        case SoodaLinqMethod.Queryable_Take:
                            int count = (int) ((ConstantExpression) mc.Arguments[1]).Value;
                            if (count < 0)
                                count = 0;
                            Take(count);
                            break;

                        default:
                            throw new NotSupportedException(mc.Method.Name);
                    }
                    break;
                default:
                    throw new NotSupportedException(expr.NodeType.ToString());
            }
        }

        ISoodaObjectList GetList()
        {
            return new SoodaObjectListSnapshot(_transaction, new SoodaWhereClause(_where), _orderBy, _topCount, _options, _classInfo);
        }

        static IEnumerable Select(IEnumerable source, Delegate d)
        {
            foreach (object obj in source)
                yield return d.DynamicInvoke(obj);
        }

        static IEnumerable SelectIndexed(IEnumerable source, Delegate d)
        {
            int i = 0;
            foreach (object obj in source)
                yield return d.DynamicInvoke(obj, i++);
        }

        SoodaObject Single(int topCount, bool orDefault)
        {
            Take(topCount);
            ISoodaObjectList list = GetList();
            if (list.Count == 1)
                return list.GetItem(0);
            if (orDefault && list.Count == 0)
                return null;
            throw new InvalidOperationException("Found " + list.Count + " matches");
        }

        object ExecuteScalar(MethodCallExpression mc, string function, object onNull)
        {
            TranslateQuery(mc.Arguments[0]);
            if (_topCount >= 0)
                throw new NotSupportedException("Take().aggregate() not supported");

            SoqlExpression selector = TranslateExpression(GetLambda(mc).Body);
            SoqlQueryExpression query = new SoqlQueryExpression();
            query.SelectExpressions.Add(new SoqlFunctionCallExpression(function, selector));
            query.SelectAliases.Add("result");
            query.From.Add(_classInfo.Name);
            query.FromAliases.Add(string.Empty);
            query.WhereClause = _where;

            SoodaDataSource ds = _transaction.OpenDataSource(_classInfo.GetDataSource());
            using (IDataReader r = ds.ExecuteQuery(query, _classInfo.Schema))
            {
                if (!r.Read())
                    throw new SoodaObjectNotFoundException();
                object result = r.GetValue(0);
                if (result != DBNull.Value)
                    return result;
                if (onNull != this)
                    return onNull;
                throw new InvalidOperationException("Aggregate on an empty collection");
            }
        }

        object ExecuteAvg(MethodCallExpression mc, object onNull)
        {
            object result = ExecuteScalar(mc, "avg", onNull);
            if (result is int || result is long)
                return Convert.ToDouble(result);
            return result;
        }

        public object Execute(Expression expr)
        {
            _where = null;
            _orderBy = null;
            _topCount = -1;
            MethodCallExpression mc = expr as MethodCallExpression;
            if (mc != null)
            {
                switch (SoodaLinqMethodUtil.Get(mc.Method))
                {
                    case SoodaLinqMethod.Queryable_Select:
                        TranslateQuery(mc.Arguments[0]);
                        return Select(GetList(), GetLambda(mc).Compile());
                    case SoodaLinqMethod.Queryable_SelectIndexed:
                        TranslateQuery(mc.Arguments[0]);
                        return SelectIndexed(GetList(), GetLambda(mc).Compile());

                    case SoodaLinqMethod.Queryable_All:
                        TranslateQuery(mc.Arguments[0]);
                        if (_topCount >= 0)
                            throw new NotSupportedException("Take().All() not supported");
                        LambdaExpression lambda = GetLambda(mc);
                        SoqlBooleanExpression where = new SoqlBooleanNegationExpression((SoqlBooleanExpression) TranslateBoolean(lambda.Body).Simplify());
                        _where = _where == null ? where : _where.And(where);
                        Take(1);
                        return GetList().Count == 0;
                    case SoodaLinqMethod.Queryable_Any:
                        TranslateQuery(mc.Arguments[0]);
                        Take(1);
                        return GetList().Count > 0;
                    case SoodaLinqMethod.Queryable_AnyFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        Take(1);
                        return GetList().Count > 0;
                    case SoodaLinqMethod.Queryable_Count:
                        TranslateQuery(mc.Arguments[0]);
                        return GetList().Count;
                    case SoodaLinqMethod.Queryable_CountFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return GetList().Count;

                    case SoodaLinqMethod.Queryable_First:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(1, false);
                    case SoodaLinqMethod.Queryable_FirstFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(1, false);
                    case SoodaLinqMethod.Queryable_FirstOrDefault:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(1, true);
                    case SoodaLinqMethod.Queryable_FirstOrDefaultFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(1, true);
                    case SoodaLinqMethod.Queryable_Last:
                        TranslateQuery(mc.Arguments[0]);
                        Reverse();
                        return Single(1, false);
                    case SoodaLinqMethod.Queryable_LastFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        Reverse();
                        return Single(1, false);
                    case SoodaLinqMethod.Queryable_LastOrDefault:
                        TranslateQuery(mc.Arguments[0]);
                        Reverse();
                        return Single(1, true);
                    case SoodaLinqMethod.Queryable_LastOrDefaultFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        Reverse();
                        return Single(1, true);
                    case SoodaLinqMethod.Queryable_Single:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(2, false);
                    case SoodaLinqMethod.Queryable_SingleFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(2, false);
                    case SoodaLinqMethod.Queryable_SingleOrDefault:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(2, true);
                    case SoodaLinqMethod.Queryable_SingleOrDefaultFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(2, true);

                    case SoodaLinqMethod.Queryable_Average:
                        return ExecuteAvg(mc, this);
                    case SoodaLinqMethod.Queryable_AverageNullable:
                        return ExecuteAvg(mc, null);
                    case SoodaLinqMethod.Queryable_Max:
                        return ExecuteScalar(mc, "max", this);
                    case SoodaLinqMethod.Queryable_Min:
                        return ExecuteScalar(mc, "min", this);
                    case SoodaLinqMethod.Queryable_SumDecimal:
                        return ExecuteScalar(mc, "sum", 0M);
                    case SoodaLinqMethod.Queryable_SumDouble:
                        return ExecuteScalar(mc, "sum", 0D);
                    case SoodaLinqMethod.Queryable_SumInt:
                        return ExecuteScalar(mc, "sum", 0);
                    case SoodaLinqMethod.Queryable_SumLong:
                        return ExecuteScalar(mc, "sum", 0L);

                    default:
                        break;
                }
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
