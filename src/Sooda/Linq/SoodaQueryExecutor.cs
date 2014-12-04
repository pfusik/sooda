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

using Sooda.ObjectMapper;
using Sooda.ObjectMapper.FieldHandlers;
using Sooda.QL;
using Sooda.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sooda.Linq
{
    public class SoodaQueryExecutor
    {
        // Caution: this must match first result of SoqlToSqlConverter.GetNextTablePrefix()
        const string DefaultAlias = "t0";

        SoodaQueryExecutor _parent;
        ParameterExpression _parameter;
        string _alias = DefaultAlias;

        SoodaTransaction _transaction;
        ClassInfo _classInfo;
        SoodaSnapshotOptions _options;
#if DOTNET4
        SelectExecutor _select = null;
#endif
        bool _distinct = false;
        SoqlBooleanExpression _where = null;
        SoodaOrderBy _orderBy = null;
        int _startIdx = 0;
        int _topCount = -1;
        readonly SoqlExpressionCollection _groupBy = new SoqlExpressionCollection();
        readonly List<string> _groupByFields = new List<string>();
        NewExpression _groupByNew = null;
        SoqlBooleanExpression _having = null;

        internal SoodaObject GetRef(Type type, object keyValue)
        {
            //if (keyValue == null)
            //    return null;
            ISoodaObjectFactory factory = _transaction.GetFactory(type);
            if (!(keyValue is int) && factory.GetPrimaryKeyFieldHandler() is Int32FieldHandler)
                keyValue = Convert.ToInt32(keyValue);
            return factory.GetRef(_transaction, keyValue);
        }

        ClassInfo FindClassInfo(Type type)
        {
            string className = type.Name;
            if (!type.IsSubclassOf(typeof(SoodaObject)))
                throw new NotSupportedException(className + " is not a Sooda class");
            ClassInfo classInfo = _transaction.Schema.FindClassByName(className);
            if (classInfo == null)
                throw new NotSupportedException("Class " + className + " not found in database schema");
            return classInfo;
        }

        ClassInfo FindClassInfo(Expression expr)
        {
            return FindClassInfo(expr.Type);
        }

        internal bool IsPolymorphicSoodaObject(Type type)
        {
            return type.IsSubclassOf(typeof(SoodaObject))
                && FindClassInfo(type).GetSubclassesForSchema(_transaction.Schema).Count > 0;
        }

        static bool IsConstant(IEnumerable<ElementInit> initializers)
        {
            return initializers.All(ei => ei.Arguments.All(IsConstant));
        }

        static bool IsConstant(IEnumerable<MemberBinding> bindings)
        {
            return bindings.All(binding =>
                {
                    switch (binding.BindingType)
                    {
                    case MemberBindingType.Assignment:
                        return IsConstant(((MemberAssignment) binding).Expression);
                    case MemberBindingType.ListBinding:
                        return IsConstant(((MemberListBinding) binding).Initializers);
                    case MemberBindingType.MemberBinding:
                        return IsConstant(((MemberMemberBinding) binding).Bindings);
                    default:
                        throw new NotImplementedException(binding.BindingType.ToString());
                    }
                });
        }

        internal static bool IsConstant(Expression expr)
        {
            if (expr == null)
                return true;
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
#if DOTNET4
                case ExpressionType.DebugInfo:
                case ExpressionType.Default:
                case ExpressionType.Label:
#endif
                    return true;
                case ExpressionType.Parameter:
                    // FIXME: local variables are fine
#if DOTNET4
                case ExpressionType.Throw:
#endif
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
#if DOTNET4
                case ExpressionType.Increment:
                case ExpressionType.Decrement:
                case ExpressionType.Unbox:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.OnesComplement:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
#endif
                    return IsConstant(((UnaryExpression) expr).Operand);
                case ExpressionType.TypeIs:
#if DOTNET4
                case ExpressionType.TypeEqual:
#endif
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
#if DOTNET4
                case ExpressionType.Assign:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.DivideAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.PowerAssign:
#endif
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
                case ExpressionType.ListInit:
                    ListInitExpression lie = (ListInitExpression) expr;
                    return IsConstant(lie.NewExpression) && IsConstant(lie.Initializers);
                case ExpressionType.MemberInit:
                    MemberInitExpression mie = (MemberInitExpression) expr;
                    return IsConstant(mie.NewExpression) && IsConstant(mie.Bindings);
#if DOTNET4
                case ExpressionType.Block:
                    return ((BlockExpression) expr).Expressions.All(IsConstant);
                case ExpressionType.Index:
                    IndexExpression ide = (IndexExpression) expr;
                    return IsConstant(ide.Object) && ide.Arguments.All(IsConstant);
                case ExpressionType.Loop:
                    return IsConstant(((LoopExpression) expr).Body);
                case ExpressionType.Switch:
                    SwitchExpression se = (SwitchExpression) expr;
                    return IsConstant(se.SwitchValue) && se.Cases.All(kase => IsConstant(kase.Body)) && IsConstant(se.DefaultBody);
                case ExpressionType.Try:
                    TryExpression te = (TryExpression) expr;
                    return IsConstant(te.Body) && IsConstant(te.Fault) && IsConstant(te.Finally) && te.Handlers.All(katch => IsConstant(katch.Body) && IsConstant(katch.Filter));
                case ExpressionType.Extension:
                case ExpressionType.Goto:
                case ExpressionType.RuntimeVariables:
#endif
                default:
                    throw new NotSupportedException(expr.NodeType.ToString());
            }
        }

        static object GetConstant(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Constant)
                return ((ConstantExpression) expr).Value;
            if (IsConstant(expr))
                return Expression.Lambda(expr).Compile().DynamicInvoke(null);
            return null;
        }

        static SoqlExpression FoldConstant(Expression expr)
        {
            object value;
            if (expr.NodeType == ExpressionType.Constant)
                value = ((ConstantExpression) expr).Value;
            else if (IsConstant(expr))
                value = Expression.Lambda(expr).Compile().DynamicInvoke(null);
            else
                return null;

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
            if (expr.Type == typeof(object)
             || expr.Operand.Type == typeof(object)
             || Nullable.GetUnderlyingType(expr.Type) == expr.Operand.Type // T -> Nullable<T>
             || (expr.Type == typeof(long) && expr.Operand.Type == typeof(int)))
                return TranslateExpression(expr.Operand);
            if (expr.Type == typeof(double))
                return new SoqlCastExpression(TranslateExpression(expr.Operand), "float");
            if (expr.Type == typeof(decimal))
                return new SoqlCastExpression(TranslateExpression(expr.Operand), "decimal"); // FIXME: SQL Serverism
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

        SoqlBooleanExpression TranslateEquals(SoqlExpression left, SoqlExpression right, bool notEqual)
        {
            return TranslateEqualsLiteral(left, right, notEqual)
                ?? TranslateEqualsLiteral(right, left, notEqual)
                ?? new SoqlBooleanRelationalExpression(left, right, notEqual ? SoqlRelationalOperator.NotEqual : SoqlRelationalOperator.Equal);
        }

        SoqlBooleanExpression TranslateEquals(Expression left, Expression right, bool notEqual)
        {
            return TranslateEquals(TranslateExpression(left), TranslateExpression(right), notEqual);
        }

        SoqlBooleanExpression TranslateEquals(BinaryExpression expr, bool notEqual)
        {
            return TranslateEquals(expr.Left, expr.Right, notEqual);
        }

        SoqlBooleanExpression TranslateRelational(BinaryExpression expr, SoqlRelationalOperator op)
        {
            return new SoqlBooleanRelationalExpression(TranslateExpression(expr.Left), TranslateExpression(expr.Right), op);
        }

        SoqlExpression TranslateConditional(ConditionalExpression expr)
        {
            SoqlBooleanExpression condition = TranslateBoolean(expr.Test);
            SoqlBooleanLiteralExpression constCondition = condition as SoqlBooleanLiteralExpression;
            if (constCondition != null)
                return TranslateExpression(constCondition.Value ? expr.IfTrue : expr.IfFalse);
            return new SoqlConditionalExpression(condition, TranslateExpression(expr.IfTrue), TranslateExpression(expr.IfFalse));
        }

        int GetNestingLevel()
        {
            return _parent != null ? _parent.GetNestingLevel() + 1 : 0;
        }

        SoodaQueryExecutor CreateSubqueryTranslator()
        {
            SoodaQueryExecutor child = new SoodaQueryExecutor();
            child._transaction = _transaction;
            child._parent = this;
            return child;
        }

        SoqlPathExpression TranslateParameter(ParameterExpression pe)
        {
            for (SoodaQueryExecutor p = _parent; p != null; p = p._parent)
            {
                if (pe == p._parameter)
                {
                    if (p._alias == null)
                        p._alias = "p" + p.GetNestingLevel();
                    return new SoqlPathExpression(p._alias);
                }
            }
#if DOTNET4
            if (_select != null)
            {
                Type type;
                SoqlPathExpression path = _select.GetSingleColumnExpression(out type) as SoqlPathExpression;
                if (path == null)
                    throw new NotSupportedException("Intermediate Select() is not a path");
                return path;
            }
#endif
            return null;
        }

        SoqlPathExpression TranslateToPathExpression(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Parameter)
                return TranslateParameter((ParameterExpression) expr);
            return (SoqlPathExpression) TranslateExpression(expr);
        }

        SoqlPathExpression TranslatePrimaryKey()
        {
            return new SoqlPathExpression(_classInfo.GetPrimaryKeyFields().Single().Name);
        }

        SoqlPathExpression TranslatePrimaryKey(Expression expr)
        {
            return new SoqlPathExpression(TranslateToPathExpression(expr), FindClassInfo(expr).GetPrimaryKeyFields().Single().Name);
        }

        SoqlPathExpression TryTranslatePath(SoqlPathExpression parent, MemberExpression expr)
        {
            string name = expr.Member.Name;
            if (expr.Member.MemberType == MemberTypes.Property
             && expr.Member.DeclaringType.IsSubclassOf(typeof(SoodaObject))
             && FindClassInfo(expr.Expression).ContainsField(name))
                return new SoqlPathExpression(parent, name);
            return null;
        }

        string GetCollectionName(Expression expr)
        {
            MemberExpression me = expr as MemberExpression;
            if (me == null)
                return null;
            string name = me.Member.Name;
            if (name.EndsWith("Query"))
                name = name.Remove(name.Length - 5);
            if (FindClassInfo(me.Expression).ContainsCollection(name) == 0)
                return null;
            return name;
        }

        T TranslateCollectionOp<T>(Expression expr, Func<SoqlPathExpression, string, T> constructor)
        {
            string name = GetCollectionName(expr);
            if (name == null)
                throw new NotSupportedException();
            return constructor(TranslateToPathExpression(((MemberExpression) expr).Expression), name);
        }

        SoqlCountExpression TranslateCollectionCount(Expression expr)
        {
            return TranslateCollectionOp(expr, (parent, name) => new SoqlCountExpression(parent, name));
        }

        SoqlContainsExpression TranslateCollectionContains(Expression haystack, SoqlExpression needle)
        {
            return TranslateCollectionOp(haystack, (parent, name) => new SoqlContainsExpression(parent, name, needle));
        }

        static bool IsGroupingKey(MemberExpression expr)
        {
            if (expr.Expression == null || expr.Expression.NodeType != ExpressionType.Parameter)
                return false;
            Type t = expr.Member.DeclaringType;
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IGrouping<,>) && expr.Member.Name == "Key";
        }

        internal NewExpression SubstituteGroupingKey(MemberExpression expr)
        {
            if (IsGroupingKey(expr))
                return _groupByNew;
            return null;
        }

        SoqlExpression TranslateCustomExpression(string exprName, object replacement, IList<Expression> arguments)
        {
            LambdaExpression lambda = replacement as LambdaExpression;
            if (lambda == null)
                throw new SoodaException(string.Format("{0} did not return a LambdaExpression", exprName));
            if (lambda.Parameters.Count != arguments.Count)
                throw new SoodaException(string.Format("{0} returned a lambda with {1} parameters, expected {2}", exprName, lambda.Parameters.Count, arguments.Count));
            Expression expr;
            if (arguments.Count == 0)
                expr = lambda.Body;
            else
            {
#if DOTNET4
                expr = new ParameterBinder(exprName, lambda.Parameters, arguments).Visit(lambda.Body);
#else
                throw new NotSupportedException(string.Format("{0} requires .NET 4", exprName));
#endif
            }
            return TranslateExpression(expr);
        }

        SoqlExpression TranslateMember(MemberExpression expr)
        {
            string name = expr.Member.Name;
            Type t = expr.Member.DeclaringType;

            if (expr.Expression != null)
            {
                // non-static members

                switch (expr.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        if (_groupBy.Count == 1 && IsGroupingKey(expr))
                        {
                            // g.Key
                            return _groupBy[0];
                        }
                        SoqlPathExpression path = TryTranslatePath(TranslateParameter((ParameterExpression) expr.Expression), expr);
                        if (path != null)
                        {
                            // x.SoodaField -> SoqlPathExpression
                            return path;
                        }
                        break;

                    case ExpressionType.MemberAccess:
                        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(SoodaObjectCollectionWrapperGeneric<>) && name == "Count")
                        {
                            // x.SoodaCollection.Count -> SoqlCountExpression
                            return TranslateCollectionCount(expr.Expression);
                        }
                        if (_groupByFields.Count > 0 && IsGroupingKey((MemberExpression) expr.Expression))
                        {
                            // g.Key.Field
                            return _groupBy[_groupByFields.IndexOf(name)];
                        }

                        break;

                    case ExpressionType.Call:
                        if (t == typeof(MemberInfo) && name == "Name")
                        {
                            // x.GetType().Name -> SoqlSoodaClassExpression
                            MethodCallExpression mc = (MethodCallExpression) expr.Expression;
                            if (SoodaLinqMethodDictionary.Get(mc.Method) == SoodaLinqMethod.Object_GetType)
                                return new SoqlSoodaClassExpression(TranslateToPathExpression(mc.Object));
                        }
                        break;

                    default:
                        break;
                }

                SoqlExpression parent = TranslateExpression(expr.Expression);

                if (typeof(INullable).IsAssignableFrom(t))
                {
                    switch (name)
                    {
                        case "Value":
                            return parent;
                        case "IsNull":
                            return new SoqlBooleanIsNullExpression(parent, false);
                        case "IsTrue":
                            return new SoqlFunctionCallExpression("coalesce", parent, new SoqlLiteralExpression(false));
                        default:
                            break;
                    }
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

                        // HACK: SQL Server only
                        case "DayOfYear":
                            return new SoqlFunctionCallExpression("datepart", new SoqlRawExpression("dy"), parent);
                        case "Hour":
                            return new SoqlFunctionCallExpression("datepart", new SoqlRawExpression("hh"), parent);
                        case "Minute":
                            return new SoqlFunctionCallExpression("datepart", new SoqlRawExpression("mi"), parent);
                        case "Second":
                            return new SoqlFunctionCallExpression("datepart", new SoqlRawExpression("s"), parent);
                        case "Millisecond":
                            return new SoqlFunctionCallExpression("datepart", new SoqlRawExpression("ms"), parent);

                        default:
                            break;
                    }
                }

                SoqlPathExpression parentPath = parent as SoqlPathExpression;
                if (parentPath != null)
                {
                    SoqlPathExpression path = TryTranslatePath(parentPath, expr);
                    if (path != null)
                    {
                        // x.SoodaField1.SoodaField2 -> SoqlPathExpression
                        return path;
                    }
                }

                string exprName = name + "Expression";
                PropertyInfo pi = t.GetProperty(exprName, BindingFlags.Public | BindingFlags.Static);
                if (pi != null)
                {
                    // It's an instance property because static properties are constant-folded.
                    return TranslateCustomExpression(exprName, pi.GetValue(null, null), new Expression[] { expr.Expression });
                }
            }

            throw new NotSupportedException(t.FullName + "." + name);
        }

        SoqlBooleanExpression TranslateCollectionAny(MethodCallExpression mc)
        {
            string className = mc.Method.GetGenericArguments()[0].Name;
            string alias;
            SoqlBooleanExpression where;
            if (mc.Arguments.Count == 1)
            {
                alias = string.Empty;
                where = null;
            }
            else
            {
                SoodaQueryExecutor subquery = CreateSubqueryTranslator();
                subquery._alias = null;
                where = subquery.TranslateBoolean(subquery.GetLambda(mc).Body);
                if (mc.Method.Name == "All")
                    where = new SoqlBooleanNegationExpression(where);
                alias = subquery._alias ?? string.Empty;
            }

            SoqlQueryExpression query = new SoqlQueryExpression();
            query.From.Add(className);
            query.FromAliases.Add(alias);
            query.WhereClause = where;
            return TranslateCollectionContains(mc.Arguments[0], query);
        }

        SoqlBooleanInExpression TranslateIn(Expression haystack, Expression needle)
        {
            IEnumerable haystack2;
            object literal = GetConstant(haystack);
            if (literal != null)
                haystack2 = (IEnumerable) literal;
            else if (haystack.NodeType == ExpressionType.NewArrayInit)
                haystack2 = ((NewArrayExpression) haystack).Expressions.Select(e => TranslateExpression(e));
            else
            {
                SoodaQueryExecutor subquery = CreateSubqueryTranslator();
                subquery.TranslateQuery(haystack);
                haystack2 = new SoqlExpressionCollection { subquery.CreateSoqlQuery() };
            }

            if (needle.NodeType == ExpressionType.Convert && needle.Type == typeof(object)) // IList.Contains(object)
                needle = ((UnaryExpression) needle).Operand;

            return new SoqlBooleanInExpression(TranslateExpression(needle), haystack2);
        }

        SoqlExpression TranslateContains(Expression haystack, Expression needle)
        {
            if (IsConstant(haystack) || GetCollectionName(haystack) == null)
            {
                // ConstSoodaCollection.Contains(expr) -> SoqlBooleanInExpression
                // SoodaQueryable.Contains(expr) -> SoqlBooleanInExpression
                return TranslateIn(haystack, needle);
            }
            // x.SoodaCollection.Contains(expr) -> SoqlContainsExpression
            return TranslateCollectionContains(haystack, TranslateExpression(needle));
        }

        string GetStringArgument(MethodCallExpression mc)
        {
            string result = GetConstant(mc.Arguments[0]) as string;
            if (result == null)
                throw new NotSupportedException(mc.Method.Name + " must be given a constant");
            return result;
        }

        SoqlStringContainsExpression TranslateStringContains(MethodCallExpression mc, SoqlStringContainsPosition position)
        {
            return new SoqlStringContainsExpression(TranslateExpression(mc.Object), position, GetStringArgument(mc));
        }

        SoqlExpression TranslateGetLabel(Expression expr)
        {
            ClassInfo classInfo = FindClassInfo(expr);
            string labelPath = classInfo.GetLabel();
            if (labelPath == null)
                return new SoqlNullLiteral();

            SoqlPathExpression path = TranslateToPathExpression(expr);
            bool nullable = false;
            Sooda.Schema.FieldInfo field = null;
            foreach (string part in labelPath.Split('.'))
            {
                if (classInfo == null)
                    throw new InvalidOperationException("Invalid label for class " + expr.Type.Name + " - " + part + " is not a Sooda field");
                field = classInfo.FindFieldByName(part);
                if (field == null)
                    throw new InvalidOperationException("Invalid label for class " + expr.Type.Name + " - " + part + " is not a Sooda field");
                path = new SoqlPathExpression(path, part);
                if (field.IsNullable)
                    nullable = true;
                classInfo = field.ReferencedClass;
            }
            if (field.DataType != FieldDataType.String && field.DataType != FieldDataType.AnsiString)
                throw new NotSupportedException("Class " + expr.Type.Name + " label is not a string");
            if (nullable)
                return new SoqlFunctionCallExpression("coalesce", path, new SoqlLiteralExpression(string.Empty));
            return path;
        }

        bool IsGroupAggregate(MethodCallExpression mc)
        {
            return _groupBy.Count > 0 && mc.Arguments[0].NodeType == ExpressionType.Parameter;
        }

        SoqlBooleanExpression TranslateEnumerableFilter(MethodCallExpression mc)
        {
            return TranslateBoolean(((LambdaExpression) mc.Arguments[1]).Body);
        }

        static SoqlExpression TranslateCountFiltered(SoqlBooleanExpression filter)
        {
            // count(case when ... then 1 end)
            return new SoqlFunctionCallExpression("count", new SoqlConditionalExpression(filter, new SoqlLiteralExpression(1), null));
        }

        static SoqlExpression TranslateGroupAny(SoqlBooleanExpression filter, SoqlRelationalOperator op)
        {
            return new SoqlBooleanRelationalExpression(TranslateCountFiltered(filter), new SoqlLiteralExpression(0), op);
        }

        static SoqlExpression TranslateFunction(SoqlExpression expr, Type type, string function)
        {
            // needed by SQL Server, not needed by Oracle
            if (function == "avg" && (type == typeof(int) || type == typeof(long)))
                expr = new SoqlCastExpression(expr, "float");
            return new SoqlFunctionCallExpression(function, expr);
        }

        SoqlExpression TranslateFunction(MethodCallExpression mc, string function)
        {
            SoqlExpression expr;
            Type type;
#if DOTNET4
            if (mc.Arguments.Count == 1)
            {
                if (_select == null)
                    throw new NotSupportedException("Cannot aggregate SoodaObjects");
                expr = _select.GetSingleColumnExpression(out type);
            }
            else
#endif
            {
                Expression arg = GetLambda(mc).Body;
                type = arg.Type;
                expr = TranslateExpression(arg);
            }

            return TranslateFunction(expr, type, function);
        }

        SoqlQueryExpression TranslateSubqueryAggregate(MethodCallExpression mc, bool where, bool reverse, int topCount, Func<SoodaQueryExecutor, SoqlExpression> selector)
        {
            SoodaQueryExecutor subquery = CreateSubqueryTranslator();
            subquery.TranslateQuery(mc.Arguments[0]);
            if (where)
                subquery.Where(mc, mc.Method.Name == "All");
            if (reverse)
                subquery.Reverse();
            if (topCount > 0)
                subquery.Take(topCount);
            SoqlQueryExpression query = subquery.CreateSoqlQuery();
            if (selector != null)
            {
                query.SelectExpressions.Add(selector(subquery));
                query.SelectAliases.Add(string.Empty);
            }
#if DOTNET4
            else if (subquery._select != null)
            {
                Type type;
                query.SelectExpressions.Add(subquery._select.GetSingleColumnExpression(out type));
                query.SelectAliases.Add(string.Empty);
            }
#endif
            return query;
        }

        SoqlBooleanExpression TranslateSubqueryAny(MethodCallExpression mc)
        {
            SoqlQueryExpression query = TranslateSubqueryAggregate(mc, mc.Arguments.Count > 1, false, -1, subquery => new SoqlAsteriskExpression());
            return new SoqlExistsExpression(query);
        }

        SoqlExpression TranslateSubqueryCount(MethodCallExpression mc)
        {
            return TranslateSubqueryAggregate(mc, mc.Arguments.Count > 1, false, -1, subquery => new SoqlFunctionCallExpression("count", new SoqlAsteriskExpression()));
        }

        SoqlExpression TranslateAggregate(MethodCallExpression mc, string function)
        {
            if (IsGroupAggregate(mc))
                return TranslateFunction(mc, function);

            return TranslateSubqueryAggregate(mc, false, false, -1, subquery => subquery.TranslateFunction(mc, function));
        }

        SoqlExpression TranslateSubquerySingleOrDefault(MethodCallExpression mc, bool reverse, int topCount)
        {
            SoqlExpression result = TranslateSubqueryAggregate(mc, mc.Arguments.Count > 1, reverse, topCount, null);
            if (mc.Type.IsValueType && !typeof(INullable).IsAssignableFrom(mc.Type) && Nullable.GetUnderlyingType(mc.Type) == null)
                result = new SoqlFunctionCallExpression("coalesce", result, new SoqlLiteralExpression(Activator.CreateInstance(mc.Type)));
            return result;
        }

        SoqlExpression TranslateToString(Expression expr)
        {
            if (expr.Type == typeof(int))
                return new SoqlCastExpression(TranslateExpression(expr), "varchar(11)");
            if (expr.Type == typeof(double))
                return new SoqlCastExpression(TranslateExpression(expr), "varchar(24)"); // -2.2250738585072020E-308
            if (expr.Type == typeof(decimal))
                return new SoqlCastExpression(TranslateExpression(expr), "varchar(42)");
            if (expr.Type == typeof(long))
                return new SoqlCastExpression(TranslateExpression(expr), "varchar(20)");
            if (expr.Type == typeof(bool))
                return new SoqlConditionalExpression(TranslateBoolean(expr), new SoqlLiteralExpression("True"), new SoqlLiteralExpression("False"));
            throw new NotSupportedException(expr.Type.ToString());
        }

        SoqlExpression TranslateUnknownMethod(MethodCallExpression mc)
        {
            // First look for an XXXExpression method whose signature matches the unknown method.
            // This enables translation of overloaded methods.
            string exprName = mc.Method.Name + "Expression";
            Type[] parameterTypes = mc.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            MethodInfo exprMethod = mc.Method.DeclaringType.GetMethod(exprName, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);
            if (exprMethod == null)
            {
                // If not found, try an XXXExpression method with no parameters.
                exprMethod = mc.Method.DeclaringType.GetMethod(exprName, BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (exprMethod == null)
                    throw new NotSupportedException(mc.Method.DeclaringType.FullName + "." + mc.Method.Name);
            }
            // Invoke XXXExpression with default argument values.
            int dummyParameterCount = exprMethod.GetParameters().Length;
            object replacement = exprMethod.Invoke(null, dummyParameterCount == 0 ? null : new object[dummyParameterCount]);

            IList<Expression> arguments = mc.Arguments;
            if (mc.Object != null)
            {
                // The unknown method is instance method.
                // Add the object for lambda parameter replacements.
                Expression[] argArray = new Expression[1 + arguments.Count];
                argArray[0] = mc.Object;
                arguments.CopyTo(argArray, 1);
                arguments = argArray;
            }
            return TranslateCustomExpression(exprName, replacement, arguments);
        }

        SoqlExpression TranslateCall(MethodCallExpression mc)
        {
            switch (SoodaLinqMethodDictionary.Get(mc.Method))
            {
                case SoodaLinqMethod.Enumerable_All:
                    if (IsGroupAggregate(mc))
                    {
                        // count(case when ... then 1 end) = 0
                        return TranslateGroupAny(new SoqlBooleanNegationExpression(TranslateEnumerableFilter(mc)), SoqlRelationalOperator.Equal);
                    }
                    if (GetCollectionName(mc.Arguments[0]) != null)
                        return new SoqlBooleanNegationExpression(TranslateCollectionAny(mc));
                    return new SoqlBooleanNegationExpression(TranslateSubqueryAny(mc));
                case SoodaLinqMethod.Enumerable_Any:
                    if (IsGroupAggregate(mc))
                        return SoqlBooleanLiteralExpression.True;
                    if (GetCollectionName(mc.Arguments[0]) != null)
                        return TranslateCollectionAny(mc);
                    return TranslateSubqueryAny(mc);
                case SoodaLinqMethod.Enumerable_AnyFiltered:
                    if (IsGroupAggregate(mc))
                    {
                        // count(case when ... then 1 end) > 0
                        return TranslateGroupAny(TranslateEnumerableFilter(mc), SoqlRelationalOperator.Greater);
                    }
                    if (GetCollectionName(mc.Arguments[0]) != null)
                        return TranslateCollectionAny(mc);
                    return TranslateSubqueryAny(mc);
                case SoodaLinqMethod.Enumerable_Contains:
                    return TranslateContains(mc.Arguments[0], mc.Arguments[1]);
                case SoodaLinqMethod.Enumerable_Count:
                    if (IsGroupAggregate(mc))
                        return new SoqlFunctionCallExpression("count", new SoqlAsteriskExpression());
                    if (GetCollectionName(mc.Arguments[0]) != null)
                        return TranslateCollectionCount(mc.Arguments[0]);
                    return TranslateSubqueryCount(mc);
                case SoodaLinqMethod.Enumerable_CountFiltered:
                    if (IsGroupAggregate(mc))
                        return TranslateCountFiltered(TranslateEnumerableFilter(mc));
                    return TranslateSubqueryCount(mc);
                case SoodaLinqMethod.Enumerable_Average:
                    return TranslateAggregate(mc, "avg");
                case SoodaLinqMethod.Enumerable_Max:
                    return TranslateAggregate(mc, "max");
                case SoodaLinqMethod.Enumerable_Min:
                    return TranslateAggregate(mc, "min");
                case SoodaLinqMethod.Enumerable_Sum:
                    return new SoqlFunctionCallExpression("coalesce",
                        TranslateAggregate(mc, "sum"),
                        new SoqlLiteralExpression(Activator.CreateInstance(mc.Type))); // 0, 0L, 0D or 0M
                case SoodaLinqMethod.ICollection_Contains:
                    return TranslateContains(mc.Object, mc.Arguments[0]);
                case SoodaLinqMethod.Queryable_FirstOrDefault:
                case SoodaLinqMethod.Queryable_FirstOrDefaultFiltered:
                    return TranslateSubquerySingleOrDefault(mc, false, 1);
                case SoodaLinqMethod.Queryable_LastOrDefault:
                case SoodaLinqMethod.Queryable_LastOrDefaultFiltered:
                    return TranslateSubquerySingleOrDefault(mc, true, 1);
                case SoodaLinqMethod.Queryable_SingleOrDefault:
                case SoodaLinqMethod.Queryable_SingleOrDefaultFiltered:
                    return TranslateSubquerySingleOrDefault(mc, false, -1);
                case SoodaLinqMethod.Object_InstanceEquals:
                    return TranslateEquals(mc.Object, mc.Arguments[0], false);
                case SoodaLinqMethod.Object_StaticEquals:
                    return TranslateEquals(mc.Arguments[0], mc.Arguments[1], false);
                case SoodaLinqMethod.String_Concat:
                    return new SoqlFunctionCallExpression("concat", TranslateExpression(mc.Arguments[0]), TranslateExpression(mc.Arguments[1]));
                case SoodaLinqMethod.String_Like:
                    return new SoqlBooleanRelationalExpression(
                        TranslateExpression(mc.Arguments[0]),
                        TranslateExpression(mc.Arguments[1]),
                        SoqlRelationalOperator.Like);
                case SoodaLinqMethod.String_Remove:
                    return new SoqlFunctionCallExpression("left", TranslateExpression(mc.Object), TranslateExpression(mc.Arguments[0]));
                case SoodaLinqMethod.String_Substring:
                    {
                        SoqlExpressionCollection parameters = new SoqlExpressionCollection {
                            TranslateExpression(mc.Object),
                            new SoqlBinaryExpression(new SoqlLiteralExpression(1), TranslateExpression(mc.Arguments[0]), SoqlBinaryOperator.Add),
                            TranslateExpression(mc.Arguments[1])
                        };
                        return new SoqlFunctionCallExpression("substring", parameters);
                    }
                case SoodaLinqMethod.String_Replace:
                    {
                        SoqlExpressionCollection parameters = new SoqlExpressionCollection {
                            TranslateExpression(mc.Object),
                            TranslateExpression(mc.Arguments[0]),
                            TranslateExpression(mc.Arguments[1])
                        };
                        return new SoqlFunctionCallExpression("replace", parameters);
                    }
                case SoodaLinqMethod.String_ToLower:
                    return new SoqlFunctionCallExpression("lower", TranslateExpression(mc.Object));
                case SoodaLinqMethod.String_ToUpper:
                    return new SoqlFunctionCallExpression("upper", TranslateExpression(mc.Object));
                case SoodaLinqMethod.String_StartsWith:
                    return TranslateStringContains(mc, SoqlStringContainsPosition.Start);
                case SoodaLinqMethod.String_EndsWith:
                    return TranslateStringContains(mc, SoqlStringContainsPosition.End);
                case SoodaLinqMethod.String_Contains:
                    return TranslateStringContains(mc, SoqlStringContainsPosition.Any);
                case SoodaLinqMethod.String_IsNullOrEmpty:
                    SoqlExpression s = TranslateExpression(mc.Arguments[0]);
                    return new SoqlBooleanOrExpression(
                        new SoqlBooleanIsNullExpression(s, false),
                        new SoqlBooleanRelationalExpression(s, new SoqlLiteralExpression(string.Empty), SoqlRelationalOperator.Equal));
                case SoodaLinqMethod.Int_ToString:
                case SoodaLinqMethod.Long_ToString:
                case SoodaLinqMethod.Double_ToString:
                case SoodaLinqMethod.Decimal_ToString:
                case SoodaLinqMethod.Bool_ToString:
                    return TranslateToString(mc.Object);
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
                case SoodaLinqMethod.SoodaObject_GetPrimaryKeyValue:
                    return TranslatePrimaryKey(mc.Object);
                case SoodaLinqMethod.SoodaObject_GetLabel:
                    return TranslateGetLabel(mc.Object);
                case SoodaLinqMethod.SoodaObject_GetItem:
                    SoqlPathExpression parent = TranslateToPathExpression(mc.Object);
                    string name = GetStringArgument(mc);
                    if (!FindClassInfo(mc.Object).ContainsField(name))
                        throw new Exception(name + " is not a Sooda field");
                    return new SoqlPathExpression(parent, name);
                default:
                    return TranslateUnknownMethod(mc);
            }
        }

        SoqlExpression TranslateToFunction(string function, BinaryExpression expr)
        {
            return new SoqlFunctionCallExpression(function, TranslateExpression(expr.Left), TranslateExpression(expr.Right));
        }

        SoqlBooleanExpression TranslateTypeIs(TypeBinaryExpression expr)
        {
            SoqlPathExpression path = TranslateToPathExpression(expr.Expression);
            Type type = expr.TypeOperand;
            if (type != typeof(object) && type != typeof(SoodaObject))  // x is object, x is SoodaObject -> x IS NOT NULL
            {
                SoqlBooleanExpression result = Soql.ClassRestriction(path, _transaction.Schema, FindClassInfo(type));
                if (result != null)
                    return result;
            }

            if (expr.Expression.NodeType == ExpressionType.Parameter)
            {
                // path is probably not valid SoqlPathExpression.
                // Fortunately, primary keys should be non-null.
                return SoqlBooleanLiteralExpression.True;
            }

            // path IS NOT NULL
            return new SoqlBooleanIsNullExpression(path, true);
        }

        SoqlExpression StringConcatArg(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Convert)
                return TranslateToString(((UnaryExpression) expr).Operand);
            return TranslateExpression(expr);
        }

        internal SoqlExpression TranslateExpression(Expression expr)
        {
            SoqlExpression literal = FoldConstant(expr);
            if (literal != null)
                return literal;

            switch (expr.NodeType)
            {
                case ExpressionType.Parameter:
                    return TranslatePrimaryKey(expr);
                case ExpressionType.MemberAccess:
                    return TranslateMember((MemberExpression) expr);
                case ExpressionType.Add:
                    if (expr.Type == typeof(string) || expr.Type == typeof(SqlString))
                    {
                        BinaryExpression be = (BinaryExpression) expr;
                        return new SoqlBinaryExpression(StringConcatArg(be.Left), StringConcatArg(be.Right), SoqlBinaryOperator.Concat);
                    }
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
                    return TranslateEquals((BinaryExpression) expr, false);
                case ExpressionType.NotEqual:
                    return TranslateEquals((BinaryExpression) expr, true);
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
            Expression expr = mc.Arguments[1];
            if (expr.NodeType == ExpressionType.Quote)
                expr = ((UnaryExpression) expr).Operand;
            LambdaExpression lambda = expr as LambdaExpression;
            if (lambda == null)
                lambda = (LambdaExpression) GetConstant(expr);
            _parameter = lambda.Parameters.First();
            return lambda;
        }

        SoqlQueryExpression CreateSoqlQuery()
        {
            SoqlQueryExpression query = new SoqlQueryExpression();
            query.StartIdx = _startIdx;
            query.PageCount = _topCount;
            query.From.Add(_classInfo.Name);
            // Must replace DefaultAlias with string.Empty, otherwise GetNextTablePrefix() could assign DefaultAlias to other tables.
            query.FromAliases.Add(_alias == null || _alias == DefaultAlias ? string.Empty : _alias);
            query.WhereClause = _where;
            if (_orderBy != null)
                query.SetOrderBy(_orderBy);
            query.GroupByExpressions.AddRange(_groupBy);
            query.Having = _having;
            return query;
        }

        void SkipTakeNotSupported()
        {
            if (_startIdx > 0 || _topCount >= 0)
            {
                SoqlPathExpression needle = TranslatePrimaryKey();
                SoqlExpressionCollection haystack = new SoqlExpressionCollection();
                haystack.Add(CreateSoqlQuery());
                _where = new SoqlBooleanInExpression(needle, haystack);
                _startIdx = 0;
                _topCount = -1;
                // _orderBy must be in both the subquery and the outer query
            }
        }

        void Select(MethodCallExpression mc)
        {
#if DOTNET4
            SelectExecutor select = new SelectExecutor(this);
            select.Process(GetLambda(mc));
            _select = select;
#else
            throw new NotImplementedException("Select() requires Sooda for .NET 4, this is .NET 3.5");
#endif
        }

        void GroupBy(MethodCallExpression mc)
        {
            if (_groupBy.Count > 0)
                throw new NotSupportedException("Chaining GroupBy()s not supported");
            _orderBy = null;
            SkipTakeNotSupported();
            Expression expr = GetLambda(mc).Body;
            if (expr.NodeType == ExpressionType.New)
            {
                NewExpression ne = (NewExpression) expr;
                if (ne.Members.Count == 0)
                    throw new NotSupportedException("GroupBy(x => new...)");
                Debug.Assert(ne.Arguments.Count == ne.Members.Count);
                foreach (Expression arg in ne.Arguments)
                    _groupBy.Add(TranslateExpression(arg));
                foreach (MemberInfo mi in ne.Members)
                    _groupByFields.Add(mi.Name);
                _groupByNew = ne;
            }
            else
                _groupBy.Add(TranslateExpression(expr));
        }

        void Where(SoqlBooleanExpression where)
        {
            if (_groupBy.Count > 0)
                _having = _having == null ? where : _having.And(where);
            else
                _where = _where == null ? where : _where.And(where);
        }

        void Where(MethodCallExpression mc, bool not)
        {
            SkipTakeNotSupported();
            SoqlBooleanExpression filter = TranslateBoolean(GetLambda(mc).Body);
            if (not)
                filter = new SoqlBooleanNegationExpression(filter);
            Where(filter);
        }

        void Where(MethodCallExpression mc)
        {
            Where(mc, false);
        }

        void Reverse()
        {
            SkipTakeNotSupported();
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
                ClassInfo classInfo = FindClassInfo(type);

                if (IsSameOrSubclassOf(_classInfo, classInfo))
                {
                    // x.OfType<X>() -> x
                    // x.OfType<BaseClass>() -> x
                }
                else if (IsSameOrSubclassOf(classInfo, _classInfo))
                {
                    // x.OfType<SubClass>() -> from SubClass
                    _classInfo = classInfo;
                }
                else
                    _where = SoqlBooleanLiteralExpression.False;
            }
        }

        SoqlBooleanExpression TranslateSubqueryWhere(Expression expr)
        {
            SkipTakeNotSupported();

            if (expr.NodeType == ExpressionType.Constant)
            {
                // e.g. Contact.Linq().Union(new Contact[] { Contact.Mary })
                SoqlPathExpression needle = TranslatePrimaryKey();
                object haystack = ((ConstantExpression) expr).Value;
                if (!(haystack is ISoodaQuerySource))
                    return new SoqlBooleanInExpression(needle, (IEnumerable) haystack);
            }

            // TODO: compare _transaction, _classInfo, _options, _groupBy
            // TODO: OfType?
            SoodaQueryExecutor subquery = CreateSubqueryTranslator();
            subquery.TranslateQuery(expr);
            subquery.SkipTakeNotSupported();
            return subquery._where;
        }

        void TranslateQuery(Expression expr)
        {
            ISoodaQuerySource source = GetConstant(expr) as ISoodaQuerySource;
            if (source != null)
            {
                _transaction = source.Transaction;
                _classInfo = source.ClassInfo;
                _options = source.Options;
                _where = source.Where;
                return;
            }

            if (expr.NodeType != ExpressionType.Call)
                throw new NotSupportedException(expr.NodeType.ToString());
            MethodCallExpression mc = (MethodCallExpression) expr;
            SoodaLinqMethod method = SoodaLinqMethodDictionary.Get(mc.Method);
            TranslateQuery(mc.Arguments[0]);
            SoqlBooleanExpression thatWhere;
            switch (method)
            {
                case SoodaLinqMethod.Queryable_Select:
                case SoodaLinqMethod.Queryable_SelectIndexed:
                    Select(mc);
                    break;

                case SoodaLinqMethod.Queryable_GroupBy:
                    GroupBy(mc);
                    break;

                case SoodaLinqMethod.Queryable_Where:
                    Where(mc);
                    break;

                case SoodaLinqMethod.Queryable_OrderBy:
                case SoodaLinqMethod.Queryable_OrderByDescending:
                case SoodaLinqMethod.Queryable_ThenBy:
                case SoodaLinqMethod.Queryable_ThenByDescending:
                    SoqlExpression orderBy = TranslateExpression(GetLambda(mc).Body);
                    if (orderBy is ISoqlConstantExpression)
                        break;
                    SkipTakeNotSupported();
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

                case SoodaLinqMethod.Queryable_Skip:
                    {
                        int count = (int) ((ConstantExpression) mc.Arguments[1]).Value;
                        if (count > 0)
                        {
                            if (_startIdx + count < 0) // int overflow
                                SkipTakeNotSupported();
                            _startIdx += count;
                            if (_topCount > 0)
                                _topCount = Math.Max(_topCount - count, 0);
                        }
                    }
                    break;

                case SoodaLinqMethod.Queryable_Take:
                    {
                        int count = (int) ((ConstantExpression) mc.Arguments[1]).Value;
                        if (count < 0)
                            count = 0;
                        Take(count);
                    }
                    break;

                case SoodaLinqMethod.Queryable_Distinct:
#if DOTNET4
                    if (_select != null)
                    {
                        SkipTakeNotSupported();
                        _distinct = true;
                    }
#endif
                    break;

                case SoodaLinqMethod.Queryable_OfType:
                    OfType(mc.Method.GetGenericArguments()[0]);
                    break;

                case SoodaLinqMethod.Queryable_Except:
                    thatWhere = TranslateSubqueryWhere(mc.Arguments[1]);
                    thatWhere = thatWhere == null ? (SoqlBooleanExpression) SoqlBooleanLiteralExpression.False : new SoqlBooleanNegationExpression(thatWhere);
                    Where(thatWhere);
                    break;
                case SoodaLinqMethod.Queryable_Intersect:
                    thatWhere = TranslateSubqueryWhere(mc.Arguments[1]);
                    if (thatWhere != null)
                        Where(thatWhere);
                    break;
                case SoodaLinqMethod.Queryable_Union:
                    thatWhere = TranslateSubqueryWhere(mc.Arguments[1]);
                    if (_where != null)
                        _where = thatWhere == null ? null : _where.Or(thatWhere);
                    break;

                default:
                    throw new NotSupportedException(mc.Method.Name);
            }
        }

        internal IDataReader ExecuteQuery(IEnumerable<SoqlExpression> columns)
        {
            SoqlQueryExpression query = CreateSoqlQuery();
            foreach (SoqlExpression column in columns)
            {
                query.SelectExpressions.Add(column);
                query.SelectAliases.Add(string.Empty);
            }
            query.Distinct = _distinct;

            if ((_options & SoodaSnapshotOptions.NoWriteObjects) == 0)
            {
                string[] involvedClasses = null;
                try
                {
                    GetInvolvedClassesVisitor gic = new GetInvolvedClassesVisitor(_classInfo);
                    gic.GetInvolvedClasses(query);
                    involvedClasses = gic.ClassNames;
                }
                catch
                {
                    // cannot detect involved classes - precommit all objects
                    // if we get here, involvedClasses remains set to null
                }
                _transaction.PrecommitClasses(involvedClasses);
            }

            SoodaDataSource ds = _transaction.OpenDataSource(_classInfo.GetDataSource());
            return ds.ExecuteQuery(query, _transaction.Schema);
        }

        IList GetList()
        {
#if DOTNET4
            if (_select != null)
                return _select.GetList();
#endif
            return new SoodaObjectListSnapshot(_transaction, new SoodaWhereClause(_where), _orderBy, _startIdx, _topCount, _options, _classInfo);
        }

        object Single(int topCount, MethodCallExpression orDefault)
        {
            Take(topCount);
            IList list = GetList();
            if (list.Count == 1)
                return list[0];
            if (list.Count == 0 && orDefault != null)
            {
                Type t = orDefault.Type;
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            }
            throw new InvalidOperationException("Found " + list.Count + " matches");
        }

        object ExecuteScalar(SoqlExpression expr, Type type)
        {
            SkipTakeNotSupported();
            _orderBy = null;

            using (IDataReader r = ExecuteQuery(new SoqlExpression[] { expr }))
            {
                if (!r.Read())
                    throw new SoodaObjectNotFoundException();
                object result = r.GetValue(0);
                if (result == DBNull.Value)
                    return null;
                if (type == typeof(int) || type == typeof(int?))
                    return Convert.ToInt32(result);
                if (type == typeof(double) || type == typeof(double?))
                    return Convert.ToDouble(result);
                if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
                    return TimeSpan.FromSeconds(Convert.ToInt32(result));
                return result;
            }
        }

        object ExecuteScalar(MethodCallExpression mc, string function)
        {
            TranslateQuery(mc.Arguments[0]);
            return ExecuteScalar(TranslateFunction(mc, function), mc.Type);
        }

        int Count()
        {
#if CACHE_LINQ_COUNT
            return GetList().Count;
#else
            return (int) ExecuteScalar(new SoqlFunctionCallExpression("count", new SoqlAsteriskExpression()), typeof(int));
#endif
        }

        bool Contains(MethodCallExpression mc)
        {
            TranslateQuery(mc.Arguments[0]);
            SkipTakeNotSupported();
            SoqlExpression haystack;
#if DOTNET4
            if (_select != null)
            {
                Type type;
                haystack = _select.GetSingleColumnExpression(out type);
            }
            else
#endif
            {
                haystack = TranslatePrimaryKey();
            }
            Where(TranslateEquals(haystack, FoldConstant(mc.Arguments[1]), false));
            Take(1);
            return Count() > 0;
        }

        static object ThrowEmptyAggregate()
        {
            throw new InvalidOperationException("Aggregate on an empty collection");
        }

        internal object Execute(Expression expr)
        {
            MethodCallExpression mc = expr as MethodCallExpression;
            if (mc != null)
            {
                switch (SoodaLinqMethodDictionary.Get(mc.Method))
                {
                    case SoodaLinqMethod.Enumerable_All:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc, true);
                        Take(1);
                        return Count() == 0;
                    case SoodaLinqMethod.Enumerable_Any:
                        TranslateQuery(mc.Arguments[0]);
                        Take(1);
                        return Count() > 0;
                    case SoodaLinqMethod.Enumerable_AnyFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        Take(1);
                        return Count() > 0;
                    case SoodaLinqMethod.Enumerable_Contains:
                        return Contains(mc);
                    case SoodaLinqMethod.Enumerable_Count:
                        TranslateQuery(mc.Arguments[0]);
                        return Count();
                    case SoodaLinqMethod.Enumerable_CountFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Count();

                    case SoodaLinqMethod.Queryable_First:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(1, null);
                    case SoodaLinqMethod.Queryable_FirstFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(1, null);
                    case SoodaLinqMethod.Queryable_FirstOrDefault:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(1, mc);
                    case SoodaLinqMethod.Queryable_FirstOrDefaultFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(1, mc);
                    case SoodaLinqMethod.Queryable_Last:
                        TranslateQuery(mc.Arguments[0]);
                        Reverse();
                        return Single(1, null);
                    case SoodaLinqMethod.Queryable_LastFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        Reverse();
                        return Single(1, null);
                    case SoodaLinqMethod.Queryable_LastOrDefault:
                        TranslateQuery(mc.Arguments[0]);
                        Reverse();
                        return Single(1, mc);
                    case SoodaLinqMethod.Queryable_LastOrDefaultFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        Reverse();
                        return Single(1, mc);
                    case SoodaLinqMethod.Queryable_Single:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(2, null);
                    case SoodaLinqMethod.Queryable_SingleFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(2, null);
                    case SoodaLinqMethod.Queryable_SingleOrDefault:
                        TranslateQuery(mc.Arguments[0]);
                        return Single(2, mc);
                    case SoodaLinqMethod.Queryable_SingleOrDefaultFiltered:
                        TranslateQuery(mc.Arguments[0]);
                        Where(mc);
                        return Single(2, mc);

                    case SoodaLinqMethod.Enumerable_Average:
                        return ExecuteScalar(mc, "avg") ?? ThrowEmptyAggregate();
                    case SoodaLinqMethod.Queryable_AverageNullable:
                        return ExecuteScalar(mc, "avg");
                    case SoodaLinqMethod.Enumerable_Max:
                        return ExecuteScalar(mc, "max") ?? ThrowEmptyAggregate();
                    case SoodaLinqMethod.Enumerable_Min:
                        return ExecuteScalar(mc, "min") ?? ThrowEmptyAggregate();
                    case SoodaLinqMethod.Enumerable_Sum:
                        return ExecuteScalar(mc, "sum")
                            ?? Activator.CreateInstance(mc.Type); // 0, 0L, 0D or 0M

                    default:
                        break;
                }
            }
            TranslateQuery(expr);
            return GetList();
        }
    }
}

#endif
