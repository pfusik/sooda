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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Sooda;
using Sooda.ObjectMapper;
using Sooda.QL;
using Sooda.Schema;

namespace Sooda.Linq
{
    public class SoodaQueryProvider : IQueryProvider
    {
        // Caution: this must match first result of SoqlToSqlConverter.GetNextTablePrefix()
        const string DefaultAlias = "t0";

        readonly SoodaTransaction _transaction;
        readonly ClassInfo _rootClassInfo;
        readonly SoodaSnapshotOptions _options;
        ClassInfo _classInfo;
        SoqlBooleanExpression _where;
        SoodaOrderBy _orderBy;
        int _topCount;
        readonly Dictionary<ParameterExpression, ClassInfo> _param2classInfo = new Dictionary<ParameterExpression, ClassInfo>();
        readonly Dictionary<ParameterExpression, string> _param2alias = new Dictionary<ParameterExpression, string>();
        int _currentPrefix = 0;

        public SoodaQueryProvider(SoodaTransaction transaction, ClassInfo classInfo, SoodaSnapshotOptions options)
        {
            _transaction = transaction;
            _rootClassInfo = classInfo;
            _options = options;
        }

        static Type GetElementType(Type seqType)
        {
            // array?
            Type elementType = seqType.GetElementType();
            if (elementType != null)
                return elementType;

            do
            {
                if (seqType.IsGenericType)
                {
                    // X<T1, T2, ...> -> try T1, T2, ...
                    foreach (Type type in seqType.GetGenericArguments())
                    {
                        Type enumerable = typeof(IEnumerable<>).MakeGenericType(new Type[1] { type });
                        if (enumerable.IsAssignableFrom(seqType))
                            return type;
                    }
                }

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

            return null;
        }

        IQueryable IQueryProvider.CreateQuery(Expression expr)
        {
            Type elementType = GetElementType(expr.Type);
            return (IQueryable) Activator.CreateInstance(typeof(SoodaQueryable<>).MakeGenericType(new Type[1] { elementType }),
                new object[] { this, expr });
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
                case ExpressionType.UnaryPlus:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return IsConstant(((UnaryExpression) expr).Operand);
                case ExpressionType.TypeIs:
                    return IsConstant(((TypeBinaryExpression) expr).Expression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Power:
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

        static ISoqlConstantExpression FoldConstant(Expression expr)
        {
            if (!IsConstant(expr))
                return null;

            object value;
            ConstantExpression constExpr = expr as ConstantExpression;
            if (constExpr != null)
                value = constExpr.Value;
            else
                value = Expression.Lambda(expr).Compile().DynamicInvoke(null);

            SoodaObject so = value as SoodaObject;
            if (so != null)
                value = so.GetPrimaryKeyValue();

            if (value == null)
                return new SoqlNullLiteral();
            if (value is bool)
                return (bool) value ? SoqlBooleanLiteralExpression.True : SoqlBooleanLiteralExpression.False;

            return new SoqlLiteralExpression(value);
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

        SoqlExpression TranslateConvert(UnaryExpression expr)
        {
            if (expr.Type == typeof(double) && expr.Operand.Type == typeof(int))
                return TranslateExpression(expr.Operand);
            throw new NotSupportedException("Convert " + expr.Operand.Type + " to " + expr.Type);
        }

        SoqlBooleanExpression TranslateEqualsLiteral(SoqlExpression left, SoqlExpression right, bool notEqual)
        {
            ISoqlConstantExpression rightConst = right as ISoqlConstantExpression;
            if (rightConst != null)
            {
                object value = rightConst.GetConstantValue();

                // left == null -> left IS NULL
                // left != null -> left IS NOT NULL
                if (value == null || value == DBNull.Value)
                    return new SoqlBooleanIsNullExpression(left, notEqual);

                if (value is bool)
                {
                    SoqlBooleanExpression leftBool = left as SoqlBooleanExpression;
                    if (leftBool != null)
                    {
                        // left == true, left != false -> left
                        if ((bool) value ^ notEqual)
                            return leftBool;
                        // left == false, left != true -> NOT(left)
                        return new SoqlBooleanNegationExpression(leftBool);
                    }
                }
            }
            return null;
        }

        SoqlBooleanExpression TranslateRelational(BinaryExpression expr, SoqlRelationalOperator op)
        {
            SoqlExpression left = TranslateExpression(expr.Left);
            SoqlExpression right = TranslateExpression(expr.Right);
            SoqlBooleanExpression result;

            switch (op)
            {
                case SoqlRelationalOperator.Equal:
                    result = TranslateEqualsLiteral(left, right, false) ?? TranslateEqualsLiteral(right, left, false);
                    break;
                case SoqlRelationalOperator.NotEqual:
                    result = TranslateEqualsLiteral(left, right, true) ?? TranslateEqualsLiteral(right, left, true);
                    break;
                default:
                    result = null;
                    break;
            }
            return result ?? new SoqlBooleanRelationalExpression(left, right, op);
        }

        SoqlExpression TranslateConditional(ConditionalExpression expr)
        {
            ISoqlConstantExpression test = FoldConstant(expr.Test);
            if (test == null)
                throw new NotSupportedException("?: operator condition is not constant");
            return TranslateExpression((bool) test.GetConstantValue() ? expr.IfTrue : expr.IfFalse);
        }

        SoqlPathExpression TranslateParameter(ParameterExpression pe)
        {
            // Don't use DefaultAlias if not needed.
            // This is necessary because OrderBy clauses don't handle table aliases.
            if (_param2alias.Count == 0)
                return null;

            string alias;
            if (_param2alias.TryGetValue(pe, out alias))
                return new SoqlPathExpression(alias);
            return new SoqlPathExpression(DefaultAlias);
        }

        SoqlPathExpression TranslateToPathExpression(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Parameter)
                return TranslateParameter((ParameterExpression) expr);
            return (SoqlPathExpression) TranslateExpression(expr);
        }

        SoqlExpression TranslateMember(MemberExpression expr)
        {
            string name = expr.Member.Name;
            Type t = expr.Member.DeclaringType;

            if (expr.Expression != null)
            {
                // non-static members

                // x.SoodaField -> SoqlPathExpression
                if (expr.Expression.NodeType == ExpressionType.Parameter)
                    return new SoqlPathExpression(TranslateParameter((ParameterExpression) expr.Expression), name);

                // x.GetType().Name -> SoqlSoodaClassExpression
                if (t == typeof(MemberInfo) && name == "Name" && expr.Expression.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression mc = (MethodCallExpression) expr.Expression;
                    if (SoodaLinqMethodDictionary.Get(mc.Method) == SoodaLinqMethod.Object_GetType)
                        return new SoqlSoodaClassExpression(TranslateToPathExpression(mc.Object));
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
                }
            }

            throw new NotSupportedException(t.FullName + "." + name);
        }

        SoqlBooleanExpression TranslateCollectionAny(MethodCallExpression mc, SoodaLinqMethod method)
        {
            string className = mc.Method.GetGenericArguments()[0].Name;
            string alias;
            SoqlBooleanExpression where;
            if (method == SoodaLinqMethod.Enumerable_Any)
            {
                alias = string.Empty;
                where = SoqlBooleanLiteralExpression.True;
            }
            else
            {
                alias = "any" + _currentPrefix++;
                LambdaExpression lambda = (LambdaExpression) mc.Arguments[1];
                ParameterExpression param = lambda.Parameters.Single();
                _param2classInfo[param] = _transaction.Schema.FindClassByName(className);
                _param2alias[param] = alias;
                where = TranslateBoolean(lambda.Body);
                _param2classInfo.Remove(param);
                _param2alias.Remove(param);
                if (method == SoodaLinqMethod.Enumerable_All)
                    where = new SoqlBooleanNegationExpression(where);
            }

            SoqlPathExpression parentPath = (SoqlPathExpression) TranslateExpression(mc.Arguments[0]);
            SoqlQueryExpression query = new SoqlQueryExpression();
            query.From.Add(className);
            query.FromAliases.Add(alias);
            query.WhereClause = where;
            return new SoqlContainsExpression(parentPath.Left, parentPath.PropertyName, query);
        }

        SoqlBooleanInExpression TranslateCollectionContains(Expression haystack, Expression needle)
        {
            IEnumerable haystack2;
            ISoqlConstantExpression literal = FoldConstant(haystack);
            if (literal != null)
                haystack2 = (IEnumerable) literal.GetConstantValue();
            else if (haystack.NodeType == ExpressionType.NewArrayInit)
                haystack2 = ((NewArrayExpression) haystack).Expressions.Select(e => TranslateExpression(e));
            else
                throw new NotSupportedException(haystack.NodeType.ToString());

            if (needle.NodeType == ExpressionType.Convert && needle.Type == typeof(object)) // IList.Contains(object)
                needle = ((UnaryExpression) needle).Operand;

            return new SoqlBooleanInExpression(TranslateExpression(needle), haystack2);
        }

        SoqlExpression TranslateCall(MethodCallExpression mc)
        {
            switch (SoodaLinqMethodDictionary.Get(mc.Method))
            {
                case SoodaLinqMethod.Enumerable_All:
                    return new SoqlBooleanNegationExpression(TranslateCollectionAny(mc, SoodaLinqMethod.Enumerable_All));
                case SoodaLinqMethod.Enumerable_Any:
                    return TranslateCollectionAny(mc, SoodaLinqMethod.Enumerable_Any);
                case SoodaLinqMethod.Enumerable_AnyFiltered:
                    return TranslateCollectionAny(mc, SoodaLinqMethod.Enumerable_AnyFiltered);
                case SoodaLinqMethod.Enumerable_Contains:
                    return TranslateCollectionContains(mc.Arguments[0], mc.Arguments[1]);
                case SoodaLinqMethod.Enumerable_Count:
                    SoqlPathExpression parentPath = (SoqlPathExpression) TranslateExpression(mc.Arguments[0]);
                    return new SoqlCountExpression(parentPath.Left, parentPath.PropertyName);
                case SoodaLinqMethod.ICollection_Contains:
                    return TranslateCollectionContains(mc.Object, mc.Arguments[0]);
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

            Type cwg = t.BaseType;
            if (cwg != null && cwg.IsGenericType && cwg.GetGenericTypeDefinition() == typeof(SoodaObjectCollectionWrapperGeneric<>) && mc.Method.Name == "Contains")
            {
                if (IsConstant(mc.Object))
                {
                    // ConstSoodaCollection.Contains(expr) -> SoqlBooleanInExpression
                    return TranslateCollectionContains(mc.Object, mc.Arguments[0]);
                }
                // x.SoodaCollection.Contains(expr) -> SoqlContainsExpression
                SoqlPathExpression haystack = (SoqlPathExpression) TranslateExpression(mc.Object);
                SoqlExpression needle = TranslateExpression(mc.Arguments[0]);
                return new SoqlContainsExpression(haystack.Left, haystack.PropertyName, needle);
            }

            throw new NotSupportedException(t.FullName + "." + mc.Method.Name);
        }

        SoqlExpression TranslateToFunction(string function, BinaryExpression expr)
        {
            return new SoqlFunctionCallExpression(function, TranslateExpression(expr.Left), TranslateExpression(expr.Right));
        }

        SoqlBooleanExpression TranslateTypeIs(TypeBinaryExpression expr)
        {
            SoqlPathExpression path = TranslateToPathExpression(expr.Expression);
            Type type = expr.TypeOperand;
            if (type != typeof(object)) // x is object -> x IS NOT NULL
            {
                if (!type.IsSubclassOf(typeof(SoodaObject)))
                    throw new NotSupportedException("'is' operator supported only for Sooda classes and object");
                SchemaInfo schema = _transaction.Schema;
                ClassInfo classInfo = schema.FindClassByName(type.Name);
                if (classInfo == null)
                    throw new NotSupportedException("is " + type.Name);

                SoqlBooleanExpression result = Soql.ClassRestriction(path, schema, classInfo);
                if (result != null)
                    return result;
            }

            if (expr.Expression.NodeType == ExpressionType.Parameter)
            {
                // path is probably not valid SoqlPathExpression.
                // Fortunately, primary keys should non-null.
                return SoqlBooleanLiteralExpression.True;
            }

            // path IS NOT NULL
            return new SoqlBooleanIsNullExpression(path, true);
        }

        SoqlExpression TranslateExpression(Expression expr)
        {
            ISoqlConstantExpression literal = FoldConstant(expr);
            if (literal != null)
                return (SoqlExpression) literal;

            switch (expr.NodeType)
            {
                case ExpressionType.Parameter:
                    ParameterExpression pe = (ParameterExpression) expr;
                    ClassInfo classInfo;
                    if (!_param2classInfo.TryGetValue(pe, out classInfo))
                        classInfo = _classInfo;
                    return new SoqlPathExpression(TranslateParameter(pe), classInfo.GetPrimaryKeyFields().Single().Name);
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
                case ExpressionType.Power:
                    return TranslateToFunction("power", (BinaryExpression) expr);
                case ExpressionType.Negate:
                    return new SoqlUnaryNegationExpression(TranslateExpression(((UnaryExpression) expr).Operand));
                case ExpressionType.Convert:
                    return TranslateConvert((UnaryExpression) expr);
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return TranslateAnd((BinaryExpression) expr);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return TranslateOr((BinaryExpression) expr);
                case ExpressionType.Not:
                    return new SoqlBooleanNegationExpression(TranslateBoolean(((UnaryExpression) expr).Operand));
                case ExpressionType.Coalesce:
                    return TranslateToFunction("coalesce", (BinaryExpression) expr);
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
                case ExpressionType.Conditional:
                    return TranslateConditional((ConditionalExpression) expr);
                case ExpressionType.Call:
                    return TranslateCall((MethodCallExpression) expr);
                case ExpressionType.TypeIs:
                    return TranslateTypeIs((TypeBinaryExpression) expr);
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

        void TakeNotSupported()
        {
            if (_topCount >= 0)
            {
                SoqlQueryExpression query = new SoqlQueryExpression();
                query.PageCount = _topCount;
                query.From.Add(_classInfo.Name);
                query.FromAliases.Add(string.Empty);
                query.WhereClause = _where;
                if (_orderBy != null)
                    query.SetOrderBy(_orderBy);
                SoqlPathExpression needle = new SoqlPathExpression(_classInfo.GetPrimaryKeyFields().Single().Name);
                SoqlExpressionCollection haystack = new SoqlExpressionCollection();
                haystack.Add(query);
                _where = new SoqlBooleanInExpression(needle, haystack);
                _topCount = -1;
                // _orderBy must be in both the subquery and the outer query
            }
        }

        void Where(SoqlBooleanExpression where)
        {
            _where = _where == null ? where : _where.And(where);
        }

        void Where(MethodCallExpression mc)
        {
            TakeNotSupported();
            Where(TranslateBoolean(GetLambda(mc).Body));
        }

        void Reverse()
        {
            TakeNotSupported();
            if (_orderBy != null)
            {
                SortOrder[] sortOrders = _orderBy.SortOrders;
                for (int i = 0; i < sortOrders.Length; i++)
                    sortOrders[i] = sortOrders[i] == SortOrder.Descending ? SortOrder.Ascending : SortOrder.Descending;
                _orderBy = new SoodaOrderBy(_orderBy.OrderByExpressions, sortOrders);
            }
            else
            {
                // There was no order, so order by primary keys descending.
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

        static bool IsSameOrSubclassOf(ClassInfo subClass, ClassInfo baseClass)
        {
            while (subClass.Name != baseClass.Name)
            {
                subClass = subClass.InheritsFromClass;
                if (subClass == null)
                    return false;
            }
            return true;
        }

        void OfType(Type type)
        {
            // x.OfType<object>() -> x
            // x.OfType<SoodaObject>() -> x
            if (type != typeof(object) && type != typeof(SoodaObject))
            {
                if (!type.IsSubclassOf(typeof(SoodaObject)))
                    throw new NotSupportedException("OfType() supported only for Sooda classes and object");
                ClassInfo classInfo = _transaction.Schema.FindClassByName(type.Name);
                if (classInfo == null)
                    throw new NotSupportedException("OfType() supported only for Sooda classes and object");

                if (IsSameOrSubclassOf(_classInfo, classInfo))
                {
                    // x.OfType<X>() -> x
                    // x.OfType<BaseClass>() -> x
                }
                else if (IsSameOrSubclassOf(classInfo, _classInfo))
                {
                    // x.OfType<SubClass>() -> from SubClass ...
                    _classInfo = classInfo;
                }
                else
                    _where = SoqlBooleanLiteralExpression.False;
            }
        }

        SoqlBooleanExpression TranslateSubquery(Expression expr)
        {
            TakeNotSupported();

            if (expr.NodeType == ExpressionType.Constant)
            {
                // e.g. Contact.Linq().Union(new Contact[] { Contact.Mary })
                // TODO: NOT if .Union(Contact.Linq())
                SoqlPathExpression needle = new SoqlPathExpression(_classInfo.GetPrimaryKeyFields().Single().Name);
                IEnumerable haystack = (IEnumerable) ((ConstantExpression) expr).Value;
                return new SoqlBooleanInExpression(needle, haystack);
            }

            // TODO: compare _transaction, _classInfo, _options
            // TODO: OfType?
            SoqlBooleanExpression thisWhere = _where;
            SoodaOrderBy thisOrderBy = _orderBy;
            _where = null;
            TranslateQuery(expr);
            TakeNotSupported();
            SoqlBooleanExpression thatWhere = _where;
            _where = thisWhere;
            _orderBy = thisOrderBy;
            return thatWhere;
        }

        void TranslateQuery(Expression expr)
        {
            SoqlBooleanExpression thatWhere;
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                    break;

                case ExpressionType.Call:
                    MethodCallExpression mc = (MethodCallExpression) expr;
                    SoodaLinqMethod method = SoodaLinqMethodDictionary.Get(mc.Method);
                    TranslateQuery(mc.Arguments[0]);
                    switch (method)
                    {
                        case SoodaLinqMethod.Queryable_Where:
                            Where(mc);
                            break;

                        case SoodaLinqMethod.Queryable_OrderBy:
                        case SoodaLinqMethod.Queryable_OrderByDescending:
                        case SoodaLinqMethod.Queryable_ThenBy:
                        case SoodaLinqMethod.Queryable_ThenByDescending:
                            SoqlExpression orderBy = TranslateExpression(GetLambda(mc).Body);
                            TakeNotSupported();
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

                        case SoodaLinqMethod.Queryable_OfType:
                            OfType(mc.Method.GetGenericArguments()[0]);
                            break;

                        case SoodaLinqMethod.Queryable_Except:
                            thatWhere = TranslateSubquery(mc.Arguments[1]);
                            thatWhere = thatWhere == null ? (SoqlBooleanExpression) SoqlBooleanLiteralExpression.False : new SoqlBooleanNegationExpression(thatWhere);
                            Where(thatWhere);
                            break;
                        case SoodaLinqMethod.Queryable_Intersect:
                            thatWhere = TranslateSubquery(mc.Arguments[1]);
                            if (thatWhere != null)
                                Where(thatWhere);
                            break;
                        case SoodaLinqMethod.Queryable_Union:
                            thatWhere = TranslateSubquery(mc.Arguments[1]);
                            if (_where != null)
                                _where = thatWhere == null ? null : _where.Or(thatWhere);
                            break;

                        default:
                            throw new NotSupportedException(mc.Method.Name);
                    }
                    break;

                default:
                    throw new NotSupportedException(expr.NodeType.ToString());
            }
        }

        // mc = "Queryable.Foo(source, extraParams)"
        // methodId = "Enumerable.Foo(source, extraParams)"
        object Invoke(MethodCallExpression mc, SoodaLinqMethod methodId, params object[] extraParams)
        {
            // TSource, ...
            Type[] ga = mc.Method.GetGenericArguments();

            // calculate source
            object source = Execute<IEnumerable>(mc.Arguments[0]);

            // source = source.Cast<TSource>();
            MethodInfo cast = SoodaLinqMethodDictionary.Get(SoodaLinqMethod.Enumerable_Cast);
            cast = cast.MakeGenericMethod(new Type[1] { ga[0] });
            source = cast.Invoke(null, new object[1] { source });

            // return Enumerable.Foo(source, extraParams);
            MethodInfo method = SoodaLinqMethodDictionary.Get(methodId);
            method = method.MakeGenericMethod(ga);
            object[] parameters = new object[1 + extraParams.Length];
            parameters[0] = source;
            extraParams.CopyTo(parameters, 1);
            return method.Invoke(null, parameters);
        }

        ISoodaObjectList GetList()
        {
            return new SoodaObjectListSnapshot(_transaction, new SoodaWhereClause(_where), _orderBy, 0, _topCount, _options, _classInfo);
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
            TakeNotSupported();

            SoqlExpression selector = TranslateExpression(GetLambda(mc).Body);
            SoqlQueryExpression query = new SoqlQueryExpression();
            query.SelectExpressions.Add(new SoqlFunctionCallExpression(function, selector));
            query.SelectAliases.Add("result");
            query.From.Add(_classInfo.Name);
            // string.Empty will result in DefaultAlias.
            // Do NOT specify DefaultAlias here, because GetNextTablePrefix() could assign DefaultAlias to other tables.
            query.FromAliases.Add(string.Empty);
            query.WhereClause = _where;

            SoodaDataSource ds = _transaction.OpenDataSource(_classInfo.GetDataSource());
            using (IDataReader r = ds.ExecuteQuery(query, _transaction.Schema))
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
            _classInfo = _rootClassInfo;
            _where = null;
            _orderBy = null;
            _topCount = -1;
            MethodCallExpression mc = expr as MethodCallExpression;
            if (mc != null)
            {
                switch (SoodaLinqMethodDictionary.Get(mc.Method))
                {
                    case SoodaLinqMethod.Queryable_Select:
                        return Invoke(mc, SoodaLinqMethod.Enumerable_Select, GetLambda(mc).Compile());
                    case SoodaLinqMethod.Queryable_SelectIndexed:
                        return Invoke(mc, SoodaLinqMethod.Enumerable_SelectIndexed, GetLambda(mc).Compile());
                    case SoodaLinqMethod.Queryable_Distinct:
                        return Invoke(mc, SoodaLinqMethod.Enumerable_Distinct);

                    case SoodaLinqMethod.Queryable_All:
                        TranslateQuery(mc.Arguments[0]);
                        TakeNotSupported();
                        Where(new SoqlBooleanNegationExpression(TranslateBoolean(GetLambda(mc).Body)));
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
                    case SoodaLinqMethod.Queryable_Contains:
                        TranslateQuery(mc.Arguments[0]);
                        TakeNotSupported();
                        SoqlBooleanExpression where = new SoqlBooleanRelationalExpression(
                            new SoqlPathExpression(_classInfo.GetPrimaryKeyFields().Single().Name),
                            (SoqlExpression) FoldConstant(mc.Arguments[1]),
                            SoqlRelationalOperator.Equal);
                        Where(where);
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

        /// <summary>
        /// creates SoqlQueryExpression with whereClause and orderBy.
        /// used for convert IQuerable&lt;T&gt; to SoqlBooleanExpression.
        /// based on object Execute(Expression expr) method.
        /// </summary>
        /// <returns></returns>
        internal SoqlQueryExpression GetSoqlQuery(Expression expr)
        {
            _classInfo = _rootClassInfo;
            _where = null;
            _orderBy = null;
            _topCount = -1;

            TranslateQuery(expr);

            // replacement: return GetList():
            SoqlQueryExpression query = new SoqlQueryExpression();

            query.From.Add(_classInfo.Name);
            query.FromAliases.Add(string.Empty);

            query.WhereClause = _where;
            if (_orderBy != null)
            {
                query.OrderByExpressions.AddRange(_orderBy.OrderByExpressions);
                query.OrderByOrder.AddRange(_orderBy.SortOrders.Select(it => it == SortOrder.Ascending ? "asc" : "desc").ToArray());
            }

            query.PageCount = _topCount;
            return query;
        }
    }
}

#endif
