//
// Copyright (c) 2012 Piotr Fusik <piotr@fusik.info>
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

enum SoodaLinqMethod
{
	Unknown,
	Where,
	OrderBy,
	OrderByDescending,
	ThenBy,
	ThenByDescending,
	Take,
	Select,
	SelectIndexed,
	Reverse,
	All,
	Any,
	AnyFiltered,
	Count,
	CountFiltered,
	First,
	FirstFiltered,
	FirstOrDefault,
	FirstOrDefaultFiltered,
	Last,
	LastFiltered,
	LastOrDefault,
	LastOrDefaultFiltered,
	Single,
	SingleFiltered,
	SingleOrDefault,
	SingleOrDefaultFiltered,
	GetType,
	String_Like,
	String_Replace,
	String_ToLower,
	String_ToUpper,
	Math_Abs,
	Math_Acos,
	Math_Asin,
	Math_Atan,
	Math_Ceiling,
	Math_Cos,
	Math_Exp,
	Math_Floor,
	Math_Log,
	Math_Log10,
	Math_Pow,
	Math_Sign,
	Math_Sin,
	Math_Sqrt,
	Math_Tan,
	Random_NextDouble,
}

static class SoodaLinqMethodUtil
{
	static Dictionary<MethodInfo, SoodaLinqMethod> _dict;

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
		Dictionary<MethodInfo, SoodaLinqMethod> dict = _dict;
		if (dict == null)
		{
			dict = new Dictionary<MethodInfo, SoodaLinqMethod>();
			Expression<Func<object, bool>> predicate = o => true;
			dict.Add(MethodOf(() => Queryable.Where(null, predicate)), SoodaLinqMethod.Where);
			Expression<Func<object, int>> selector = o => 0;
			dict.Add(MethodOf(() => Queryable.OrderBy(null, selector)), SoodaLinqMethod.OrderBy);
			dict.Add(MethodOf(() => Queryable.OrderByDescending(null, selector)), SoodaLinqMethod.OrderByDescending);
			dict.Add(MethodOf(() => Queryable.ThenBy(null, selector)), SoodaLinqMethod.ThenBy);
			dict.Add(MethodOf(() => Queryable.ThenByDescending(null, selector)), SoodaLinqMethod.ThenByDescending);
			dict.Add(MethodOf(() => Queryable.Take<object>(null, 0)), SoodaLinqMethod.Take);
			dict.Add(MethodOf(() => Queryable.Select(null, selector)), SoodaLinqMethod.Select);
			dict.Add(MethodOf(() => Queryable.Select(null, (object o, int i) => i)), SoodaLinqMethod.SelectIndexed);
			dict.Add(MethodOf(() => Queryable.Reverse<object>(null)), SoodaLinqMethod.Reverse);
			dict.Add(MethodOf(() => Queryable.All(null, predicate)), SoodaLinqMethod.All);
			dict.Add(MethodOf(() => Queryable.Any<object>(null)), SoodaLinqMethod.Any);
			dict.Add(MethodOf(() => Queryable.Any(null, predicate)), SoodaLinqMethod.AnyFiltered);
			dict.Add(MethodOf(() => Queryable.Count<object>(null)), SoodaLinqMethod.Count);
			dict.Add(MethodOf(() => Queryable.Count(null, predicate)), SoodaLinqMethod.CountFiltered);
			dict.Add(MethodOf(() => Queryable.First<object>(null)), SoodaLinqMethod.First);
			dict.Add(MethodOf(() => Queryable.First(null, predicate)), SoodaLinqMethod.FirstFiltered);
			dict.Add(MethodOf(() => Queryable.FirstOrDefault<object>(null)), SoodaLinqMethod.FirstOrDefault);
			dict.Add(MethodOf(() => Queryable.FirstOrDefault(null, predicate)), SoodaLinqMethod.FirstOrDefaultFiltered);
			dict.Add(MethodOf(() => Queryable.Last<object>(null)), SoodaLinqMethod.Last);
			dict.Add(MethodOf(() => Queryable.Last(null, predicate)), SoodaLinqMethod.LastFiltered);
			dict.Add(MethodOf(() => Queryable.LastOrDefault<object>(null)), SoodaLinqMethod.LastOrDefault);
			dict.Add(MethodOf(() => Queryable.LastOrDefault(null, predicate)), SoodaLinqMethod.LastOrDefaultFiltered);
			dict.Add(MethodOf(() => Queryable.Single<object>(null)), SoodaLinqMethod.Single);
			dict.Add(MethodOf(() => Queryable.Single(null, predicate)), SoodaLinqMethod.SingleFiltered);
			dict.Add(MethodOf(() => Queryable.SingleOrDefault<object>(null)), SoodaLinqMethod.SingleOrDefault);
			dict.Add(MethodOf(() => Queryable.SingleOrDefault(null, predicate)), SoodaLinqMethod.SingleOrDefaultFiltered);
			dict.Add(MethodOf(() => string.Empty.GetType()), SoodaLinqMethod.GetType);
			dict.Add(MethodOf(() => LinqUtils.Like(string.Empty, string.Empty)), SoodaLinqMethod.String_Like);
			dict.Add(MethodOf(() => string.Empty.Replace(string.Empty, string.Empty)), SoodaLinqMethod.String_Replace);
			dict.Add(MethodOf(() => string.Empty.ToLower()), SoodaLinqMethod.String_ToLower);
			dict.Add(MethodOf(() => string.Empty.ToUpper()), SoodaLinqMethod.String_ToUpper);
			dict.Add(MethodOf(() => Math.Abs(0M)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Abs(0D)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Abs((short) 0)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Abs(0)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Abs(0L)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Abs((sbyte) 0)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Abs(0F)), SoodaLinqMethod.Math_Abs);
			dict.Add(MethodOf(() => Math.Acos(0)), SoodaLinqMethod.Math_Acos);
			dict.Add(MethodOf(() => Math.Asin(0)), SoodaLinqMethod.Math_Asin);
			dict.Add(MethodOf(() => Math.Atan(0)), SoodaLinqMethod.Math_Atan);
			dict.Add(MethodOf(() => Math.Ceiling(0M)), SoodaLinqMethod.Math_Ceiling);
			dict.Add(MethodOf(() => Math.Ceiling(0D)), SoodaLinqMethod.Math_Ceiling);
			dict.Add(MethodOf(() => Math.Cos(0)), SoodaLinqMethod.Math_Cos);
			dict.Add(MethodOf(() => Math.Exp(0)), SoodaLinqMethod.Math_Exp);
			dict.Add(MethodOf(() => Math.Floor(0M)), SoodaLinqMethod.Math_Floor);
			dict.Add(MethodOf(() => Math.Floor(0D)), SoodaLinqMethod.Math_Floor);
			dict.Add(MethodOf(() => Math.Log(1)), SoodaLinqMethod.Math_Log);
			dict.Add(MethodOf(() => Math.Log10(1)), SoodaLinqMethod.Math_Log10);
			dict.Add(MethodOf(() => Math.Pow(1, 1)), SoodaLinqMethod.Math_Pow);
			dict.Add(MethodOf(() => Math.Sign(0M)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sign(0D)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sign((short) 0)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sign(0)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sign(0L)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sign((sbyte) 0)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sign(0F)), SoodaLinqMethod.Math_Sign);
			dict.Add(MethodOf(() => Math.Sin(0)), SoodaLinqMethod.Math_Sin);
			dict.Add(MethodOf(() => Math.Sqrt(0)), SoodaLinqMethod.Math_Sqrt);
			dict.Add(MethodOf(() => Math.Tan(0)), SoodaLinqMethod.Math_Tan);
			dict.Add(MethodOf(() => new Random().NextDouble()), SoodaLinqMethod.Random_NextDouble);
			_dict = dict;
		}
		SoodaLinqMethod result;
		dict.TryGetValue(Ungeneric(method), out result);
		return result;
	}
}

#endif
