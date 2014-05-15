//
// Copyright (c) 2012-2014 Piotr Fusik <piotr@fusik.info>
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sooda.Linq
{
    enum SoodaLinqMethod
    {
        Unknown,
        Queryable_Where,
        Queryable_OrderBy,
        Queryable_OrderByDescending,
        Queryable_ThenBy,
        Queryable_ThenByDescending,
        Queryable_Skip,
        Queryable_Take,
        Queryable_Select,
        Queryable_SelectIndexed,
        Queryable_Reverse,
        Queryable_Distinct,
        Queryable_OfType,
        Queryable_Except,
        Queryable_Intersect,
        Queryable_Union,
        Queryable_All,
        Queryable_Any,
        Queryable_AnyFiltered,
        Queryable_Contains,
        Queryable_Count,
        Queryable_CountFiltered,
        Queryable_First,
        Queryable_FirstFiltered,
        Queryable_FirstOrDefault,
        Queryable_FirstOrDefaultFiltered,
        Queryable_Last,
        Queryable_LastFiltered,
        Queryable_LastOrDefault,
        Queryable_LastOrDefaultFiltered,
        Queryable_Single,
        Queryable_SingleFiltered,
        Queryable_SingleOrDefault,
        Queryable_SingleOrDefaultFiltered,
        Queryable_Average,
        Queryable_AverageNullable,
        Queryable_Max,
        Queryable_Min,
        Queryable_Sum,
        Enumerable_All,
        Enumerable_Any,
        Enumerable_AnyFiltered,
        Enumerable_Contains,
        Enumerable_Count,
        ICollection_Contains,
        Object_GetType,
        String_Concat,
        String_Like,
        String_Remove,
        String_Replace,
        String_ToLower,
        String_ToUpper,
        Math_Abs,
        Math_Acos,
        Math_Asin,
        Math_Atan,
        Math_Cos,
        Math_Exp,
        Math_Floor,
        Math_Pow,
        Math_Round,
        Math_Sign,
        Math_Sin,
        Math_Sqrt,
        Math_Tan,
        SoodaObject_GetPrimaryKeyValue,
        SoodaObject_GetLabel,
    }

    static class SoodaLinqMethodDictionary
    {
        static Dictionary<MethodInfo, SoodaLinqMethod> _method2id;

        static MethodInfo Ungeneric(MethodInfo method)
        {
            return method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;
        }

        static MethodInfo MethodOf(Expression<Action> lambda)
        {
            return Ungeneric(((MethodCallExpression) lambda.Body).Method);
        }

        public static SoodaLinqMethod Get(MethodInfo method)
        {
            Dictionary<MethodInfo, SoodaLinqMethod> method2id = _method2id;
            if (method2id == null)
            {
                method2id = new Dictionary<MethodInfo, SoodaLinqMethod>();
                Expression<Func<object, bool>> predicate = o => true;
                Expression<Func<object, int>> selector = o => 0;
                Expression<Func<object, decimal>> selectorM = o => 0;
                Expression<Func<object, double>> selectorD = o => 0;
                Expression<Func<object, long>> selectorL = o => 0;
                Expression<Func<object, int?>> selectorN = o => 0;
                Expression<Func<object, decimal?>> selectorNM = o => 0;
                Expression<Func<object, double?>> selectorND = o => 0;
                Expression<Func<object, long?>> selectorNL = o => 0;
                method2id.Add(MethodOf(() => Queryable.Where(null, predicate)), SoodaLinqMethod.Queryable_Where);
                method2id.Add(MethodOf(() => Queryable.OrderBy(null, selector)), SoodaLinqMethod.Queryable_OrderBy);
                method2id.Add(MethodOf(() => Queryable.OrderByDescending(null, selector)), SoodaLinqMethod.Queryable_OrderByDescending);
                method2id.Add(MethodOf(() => Queryable.ThenBy(null, selector)), SoodaLinqMethod.Queryable_ThenBy);
                method2id.Add(MethodOf(() => Queryable.ThenByDescending(null, selector)), SoodaLinqMethod.Queryable_ThenByDescending);
                method2id.Add(MethodOf(() => Queryable.Skip<object>(null, 0)), SoodaLinqMethod.Queryable_Skip);
                method2id.Add(MethodOf(() => Queryable.Take<object>(null, 0)), SoodaLinqMethod.Queryable_Take);
                method2id.Add(MethodOf(() => Queryable.Select(null, selector)), SoodaLinqMethod.Queryable_Select);
                method2id.Add(MethodOf(() => Queryable.Select(null, (object o, int i) => i)), SoodaLinqMethod.Queryable_SelectIndexed);
                method2id.Add(MethodOf(() => Queryable.Reverse<object>(null)), SoodaLinqMethod.Queryable_Reverse);
                method2id.Add(MethodOf(() => Queryable.Distinct<object>(null)), SoodaLinqMethod.Queryable_Distinct);
                method2id.Add(MethodOf(() => Queryable.OfType<object>(null)), SoodaLinqMethod.Queryable_OfType);
                method2id.Add(MethodOf(() => Queryable.Except<object>(null, null)), SoodaLinqMethod.Queryable_Except);
                method2id.Add(MethodOf(() => Queryable.Intersect<object>(null, null)), SoodaLinqMethod.Queryable_Intersect);
                method2id.Add(MethodOf(() => Queryable.Union<object>(null, null)), SoodaLinqMethod.Queryable_Union);
                method2id.Add(MethodOf(() => Queryable.All(null, predicate)), SoodaLinqMethod.Queryable_All);
                method2id.Add(MethodOf(() => Queryable.Any<object>(null)), SoodaLinqMethod.Queryable_Any);
                method2id.Add(MethodOf(() => Queryable.Any(null, predicate)), SoodaLinqMethod.Queryable_AnyFiltered);
                method2id.Add(MethodOf(() => Queryable.Contains<object>(null, null)), SoodaLinqMethod.Queryable_Contains);
                method2id.Add(MethodOf(() => Queryable.Count<object>(null)), SoodaLinqMethod.Queryable_Count);
                method2id.Add(MethodOf(() => Queryable.Count(null, predicate)), SoodaLinqMethod.Queryable_CountFiltered);
                method2id.Add(MethodOf(() => Queryable.First<object>(null)), SoodaLinqMethod.Queryable_First);
                method2id.Add(MethodOf(() => Queryable.First(null, predicate)), SoodaLinqMethod.Queryable_FirstFiltered);
                method2id.Add(MethodOf(() => Queryable.FirstOrDefault<object>(null)), SoodaLinqMethod.Queryable_FirstOrDefault);
                method2id.Add(MethodOf(() => Queryable.FirstOrDefault(null, predicate)), SoodaLinqMethod.Queryable_FirstOrDefaultFiltered);
                method2id.Add(MethodOf(() => Queryable.Last<object>(null)), SoodaLinqMethod.Queryable_Last);
                method2id.Add(MethodOf(() => Queryable.Last(null, predicate)), SoodaLinqMethod.Queryable_LastFiltered);
                method2id.Add(MethodOf(() => Queryable.LastOrDefault<object>(null)), SoodaLinqMethod.Queryable_LastOrDefault);
                method2id.Add(MethodOf(() => Queryable.LastOrDefault(null, predicate)), SoodaLinqMethod.Queryable_LastOrDefaultFiltered);
                method2id.Add(MethodOf(() => Queryable.Single<object>(null)), SoodaLinqMethod.Queryable_Single);
                method2id.Add(MethodOf(() => Queryable.Single(null, predicate)), SoodaLinqMethod.Queryable_SingleFiltered);
                method2id.Add(MethodOf(() => Queryable.SingleOrDefault<object>(null)), SoodaLinqMethod.Queryable_SingleOrDefault);
                method2id.Add(MethodOf(() => Queryable.SingleOrDefault(null, predicate)), SoodaLinqMethod.Queryable_SingleOrDefaultFiltered);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<decimal>) null)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<double>) null)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<int>) null)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<long>) null)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<decimal?>) null)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<double?>) null)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<int?>) null)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Average((IQueryable<long?>) null)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Max<int>(null)), SoodaLinqMethod.Queryable_Max);
                method2id.Add(MethodOf(() => Queryable.Min<int>(null)), SoodaLinqMethod.Queryable_Min);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<decimal>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<double>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<int>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<long>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<decimal?>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<double?>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<int?>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum((IQueryable<long?>) null)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorM)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorD)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average(null, selector)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorL)), SoodaLinqMethod.Queryable_Average);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorNM)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorND)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorN)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Average(null, selectorNL)), SoodaLinqMethod.Queryable_AverageNullable);
                method2id.Add(MethodOf(() => Queryable.Max(null, selector)), SoodaLinqMethod.Queryable_Max);
                method2id.Add(MethodOf(() => Queryable.Min(null, selector)), SoodaLinqMethod.Queryable_Min);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorM)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorD)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selector)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorL)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorNM)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorND)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorN)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Queryable.Sum(null, selectorNL)), SoodaLinqMethod.Queryable_Sum);
                method2id.Add(MethodOf(() => Enumerable.All(null, (object o) => true)), SoodaLinqMethod.Enumerable_All);
                method2id.Add(MethodOf(() => Enumerable.Any<object>(null)), SoodaLinqMethod.Enumerable_Any);
                method2id.Add(MethodOf(() => Enumerable.Any(null, (object o) => true)), SoodaLinqMethod.Enumerable_AnyFiltered);
                method2id.Add(MethodOf(() => Enumerable.Contains<object>(null, null)), SoodaLinqMethod.Enumerable_Contains);
                method2id.Add(MethodOf(() => Enumerable.Count<object>(null)), SoodaLinqMethod.Enumerable_Count);
                method2id.Add(MethodOf(() => ((ICollection<object>) null).Contains(null)), SoodaLinqMethod.ICollection_Contains); // FIXME: Ungeneric doesn't handle methods in generic classes, so this will only work on ICollection<object>
                method2id.Add(MethodOf(() => ((System.Collections.ArrayList) null).Contains(null)), SoodaLinqMethod.ICollection_Contains);
                method2id.Add(MethodOf(() => ((System.Collections.IList) null).Contains(null)), SoodaLinqMethod.ICollection_Contains);
                method2id.Add(MethodOf(() => string.Empty.GetType()), SoodaLinqMethod.Object_GetType);
                method2id.Add(MethodOf(() => string.Concat(string.Empty, string.Empty)), SoodaLinqMethod.String_Concat);
                method2id.Add(MethodOf(() => LinqUtils.Like(string.Empty, string.Empty)), SoodaLinqMethod.String_Like);
                method2id.Add(MethodOf(() => string.Empty.Remove(0)), SoodaLinqMethod.String_Remove);
                method2id.Add(MethodOf(() => string.Empty.Replace(string.Empty, string.Empty)), SoodaLinqMethod.String_Replace);
                method2id.Add(MethodOf(() => string.Empty.ToLower()), SoodaLinqMethod.String_ToLower);
                method2id.Add(MethodOf(() => string.Empty.ToUpper()), SoodaLinqMethod.String_ToUpper);
                method2id.Add(MethodOf(() => Math.Abs(0M)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Abs(0D)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Abs((short) 0)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Abs(0)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Abs(0L)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Abs((sbyte) 0)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Abs(0F)), SoodaLinqMethod.Math_Abs);
                method2id.Add(MethodOf(() => Math.Acos(0)), SoodaLinqMethod.Math_Acos);
                method2id.Add(MethodOf(() => Math.Asin(0)), SoodaLinqMethod.Math_Asin);
                method2id.Add(MethodOf(() => Math.Atan(0)), SoodaLinqMethod.Math_Atan);
                method2id.Add(MethodOf(() => Math.Cos(0)), SoodaLinqMethod.Math_Cos);
                method2id.Add(MethodOf(() => Math.Exp(0)), SoodaLinqMethod.Math_Exp);
                method2id.Add(MethodOf(() => Math.Floor(0M)), SoodaLinqMethod.Math_Floor);
                method2id.Add(MethodOf(() => Math.Floor(0D)), SoodaLinqMethod.Math_Floor);
                method2id.Add(MethodOf(() => Math.Pow(1, 1)), SoodaLinqMethod.Math_Pow);
                method2id.Add(MethodOf(() => Math.Round(0M, 0)), SoodaLinqMethod.Math_Round);
                method2id.Add(MethodOf(() => Math.Round(0D, 0)), SoodaLinqMethod.Math_Round);
                method2id.Add(MethodOf(() => Math.Sign(0M)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sign(0D)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sign((short) 0)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sign(0)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sign(0L)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sign((sbyte) 0)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sign(0F)), SoodaLinqMethod.Math_Sign);
                method2id.Add(MethodOf(() => Math.Sin(0)), SoodaLinqMethod.Math_Sin);
                method2id.Add(MethodOf(() => Math.Sqrt(0)), SoodaLinqMethod.Math_Sqrt);
                method2id.Add(MethodOf(() => Math.Tan(0)), SoodaLinqMethod.Math_Tan);
                method2id.Add(MethodOf(() => ((SoodaObject) null).GetPrimaryKeyValue()), SoodaLinqMethod.SoodaObject_GetPrimaryKeyValue);
                method2id.Add(MethodOf(() => ((SoodaObject) null).GetLabel(false)), SoodaLinqMethod.SoodaObject_GetLabel);
                _method2id = method2id;
            }
            SoodaLinqMethod id;
            method2id.TryGetValue(Ungeneric(method), out id);
            return id;
        }
    }
}

#endif
