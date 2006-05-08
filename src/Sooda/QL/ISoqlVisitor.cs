// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using Sooda.QL.TypedWrappers;

namespace Sooda.QL
{
    public interface ISoqlVisitor
    {
        void Visit(SoqlBinaryExpression v);
        void Visit(SoqlBooleanAndExpression v);
        void Visit(SoqlBooleanInExpression v);
        void Visit(SoqlBooleanIsNullExpression v);
        void Visit(SoqlBooleanLiteralExpression v);
        void Visit(SoqlBooleanNegationExpression v);
        void Visit(SoqlBooleanOrExpression v);
        void Visit(SoqlBooleanRelationalExpression v);
        void Visit(SoqlExistsExpression v);
        void Visit(SoqlFunctionCallExpression v);
        void Visit(SoqlContainsExpression v);
        void Visit(SoqlSoodaClassExpression v);
        void Visit(SoqlCountExpression v);
        void Visit(SoqlAsteriskExpression v);
        void Visit(SoqlLiteralExpression v);
        void Visit(SoqlNullLiteral v);
        void Visit(SoqlParameterLiteralExpression v);
        void Visit(SoqlPathExpression v);
        void Visit(SoqlQueryExpression v);
        void Visit(SoqlUnaryNegationExpression v);
        void Visit(SoqlRawExpression v);
        void Visit(SoqlTypedWrapperExpression v);
        void Visit(SoqlBooleanWrapperExpression v);
    }
}
