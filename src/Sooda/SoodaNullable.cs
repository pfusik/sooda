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

using System.Data.SqlTypes;

namespace Sooda
{
    public class SoodaNullable
    {
        // non-generic methods to extract the value of a nullable type and box it
        public static object Box(SqlInt16 a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlInt32 a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlInt64 a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlString a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlSingle a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlDouble a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlDateTime a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlGuid a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlDecimal a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlBoolean a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }
        public static object Box(SqlBinary a)
        {
            if (a.IsNull)
                return null;
            else
                return a.Value;
        }

        // this catches all of the remaining values
        public static object Box(object a) { return a; }
    }
}
