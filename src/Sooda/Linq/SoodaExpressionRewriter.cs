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
using System.Linq.Expressions;
using System.Reflection;

namespace Sooda.Linq
{
    class SoodaExpressionRewriter : ExpressionVisitor
    {
        Expression Rewrite(Expression node, MemberInfo member)
        {
            Attribute attribute = Attribute.GetCustomAttribute(member, typeof(SoodaExpressionRewriteAttribute));
            if (attribute == null)
                return null;
            ExpressionVisitor rewriter = (ExpressionVisitor) Activator.CreateInstance(((SoodaExpressionRewriteAttribute) attribute).RewriterType);
            Expression output = rewriter.Visit(node);
            if (output == node) // unmodified
                return null;
            return Visit(output); // visit the rewritten expression
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return Rewrite(node, node.Method) ?? base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return Rewrite(node, node.Member) ?? base.VisitMember(node);
        }
    }
}

#endif
