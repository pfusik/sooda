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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sooda.Linq
{
    class ParameterBinder : ExpressionVisitor
    {
        readonly Dictionary<ParameterExpression, Expression> parameter2arg = new Dictionary<ParameterExpression, Expression>();

        public ParameterBinder(string exprName, IList<ParameterExpression> parameters, IList<Expression> arguments)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                if (!parameters[i].Type.IsAssignableFrom(arguments[i].Type))
                    throw new SoodaException(string.Format("{0} returned a lambda with parameter {1} of type {2}, expected {3}", exprName, i + 1, parameters[i], arguments[i].Type));
                parameter2arg.Add(parameters[i], arguments[i]);
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression argument;
            if (parameter2arg.TryGetValue(node, out argument))
                return argument;
            return base.VisitParameter(node);
        }
    }
}

#endif
