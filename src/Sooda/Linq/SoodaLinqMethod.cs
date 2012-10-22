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
	GetType,
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
			dict.Add(MethodOf(() => Queryable.Where(null, (object o) => true)), SoodaLinqMethod.Where);
			Expression<Func<object, int>> selector = o => 0;
			dict.Add(MethodOf(() => Queryable.OrderBy(null, selector)), SoodaLinqMethod.OrderBy);
			dict.Add(MethodOf(() => Queryable.OrderByDescending(null, selector)), SoodaLinqMethod.OrderByDescending);
			dict.Add(MethodOf(() => Queryable.ThenBy(null, selector)), SoodaLinqMethod.ThenBy);
			dict.Add(MethodOf(() => Queryable.ThenByDescending(null, selector)), SoodaLinqMethod.ThenByDescending);
			dict.Add(MethodOf(() => Queryable.Take<object>(null, 0)), SoodaLinqMethod.Take);
			dict.Add(MethodOf(() => Queryable.Select(null, selector)), SoodaLinqMethod.Select);
			dict.Add(MethodOf(() => Queryable.Select(null, (object o, int i) => i)), SoodaLinqMethod.SelectIndexed);
			dict.Add(MethodOf(() => string.Empty.GetType()), SoodaLinqMethod.GetType);
			_dict = dict;
		}
		SoodaLinqMethod result;
		dict.TryGetValue(Ungeneric(method), out result);
		return result;
	}
}

#endif
