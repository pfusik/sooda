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

using System;



namespace Sooda
{
    public class SoodaObjectArrayFieldValues : SoodaObjectFieldValues
    {
        readonly object[] _values;

        public SoodaObjectArrayFieldValues(int count)
        {
            _values = new object[count];
        }

        protected SoodaObjectArrayFieldValues(SoodaObjectArrayFieldValues other)
        {
            _values = (object[]) other._values.Clone();
        }

        public override SoodaObjectFieldValues Clone()
        {
            return new SoodaObjectArrayFieldValues(this);
        }

        public override void SetFieldValue(int fieldOrdinal, object val)
        {
            _values[fieldOrdinal] = val;
        }

        public override object GetBoxedFieldValue(int fieldOrdinal)
        {
            return _values[fieldOrdinal];
        }

        public override int Length
        {
            get
            {
                return _values.Length;
            }
        }

        public override bool IsNull(int fieldOrdinal)
        {
            return _values[fieldOrdinal] == null;
        }
    }
}
