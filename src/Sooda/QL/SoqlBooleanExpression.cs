//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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

namespace Sooda.QL
{
    public abstract class SoqlBooleanExpression : SoqlExpression
    {
        public SoqlBooleanExpression And(SoqlBooleanExpression expr)
        {
            return new SoqlBooleanAndExpression(this, expr);
        }

        public SoqlBooleanExpression Or(SoqlBooleanExpression expr)
        {
            return new SoqlBooleanOrExpression(this, expr);
        }

        public static bool operator true(SoqlBooleanExpression expr)
        {
            return false; // never true to disable short-circuit evaluation of a || b
        }

        public static bool operator false(SoqlBooleanExpression expr)
        {
            return false; // never false to disable short-circuit evaluation of a && b
        }

        public static implicit operator SoqlBooleanExpression(bool v)
        {
            return new SoqlBooleanLiteralExpression(v);
        }

        public static SoqlBooleanExpression operator &(SoqlBooleanExpression left, SoqlBooleanExpression right)
        {
            return left.And(right);
        }

        public static SoqlBooleanExpression operator |(SoqlBooleanExpression left, SoqlBooleanExpression right)
        {
            return left.Or(right);
        }

        public static SoqlBooleanExpression operator &(SoqlBooleanExpression left, bool right)
        {
            if (right)
                return left;
            else
                return SoqlBooleanLiteralExpression.False;
        }

        public static SoqlBooleanExpression operator |(SoqlBooleanExpression left, bool right)
        {
            if (right)
                return SoqlBooleanLiteralExpression.True;
            else
                return left;
        }

        public static SoqlBooleanExpression operator &(bool left, SoqlBooleanExpression right)
        {
            if (left)
                return right;
            else
                return SoqlBooleanLiteralExpression.False;
        }

        public static SoqlBooleanExpression operator |(bool left, SoqlBooleanExpression right)
        {
            if (left)
                return SoqlBooleanLiteralExpression.True;
            else
                return right;
        }

        public static SoqlBooleanExpression operator !(SoqlBooleanExpression expr)
        {
            return new SoqlBooleanNegationExpression(expr);
        }
    }
}
