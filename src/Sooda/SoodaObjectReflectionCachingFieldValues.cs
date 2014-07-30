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

using System;
using System.Reflection;
using System.Collections;

namespace Sooda
{
    ///Reflection based implementation, with caching
    public abstract class SoodaObjectReflectionCachingFieldValues : SoodaObjectFieldValues
    {
        private readonly string[] _orderedFieldNames;
        private static readonly Hashtable _fieldCache = new Hashtable();
        
        private static FieldInfo GetField(Type t, string name)
        {
            string key = string.Format("{0}.{1}", t.FullName, name);
            FieldInfo fi = (FieldInfo) _fieldCache[key];
            if (fi != null) return fi;
            lock (_fieldCache)
            {
                fi = (FieldInfo) _fieldCache[key];
                if (fi != null) return fi;
                fi = t.GetField(name);
                if (fi != null)
                {
                    _fieldCache[key] = fi;
                }
                return fi;
            }
        }
        
        protected SoodaObjectReflectionCachingFieldValues(string[] orderedFieldNames)
        {
            _orderedFieldNames = orderedFieldNames;
        }

        protected SoodaObjectReflectionCachingFieldValues(SoodaObjectReflectionCachingFieldValues other)
        {
            _orderedFieldNames = other.GetFieldNames();
            for (int i = 0; i < _orderedFieldNames.Length; ++i)
            {
                FieldInfo fi = GetField(this.GetType(), _orderedFieldNames[i]);
                fi.SetValue(this, fi.GetValue(other));
            }
        }

        public override void SetFieldValue(int fieldOrdinal, object val)
        {
            System.Reflection.FieldInfo fi = GetField(this.GetType(), _orderedFieldNames[fieldOrdinal]);
            Sooda.Utils.SqlTypesUtil.SetValue(fi, this, val);
        }

        public override object GetBoxedFieldValue(int fieldOrdinal)
        {
            System.Reflection.FieldInfo fi = GetField(this.GetType(), _orderedFieldNames[fieldOrdinal]);
            object rawValue = fi.GetValue(this);

            // we got raw value, it's possible that it's a sqltype, nullables are already boxed here
            return Sooda.Utils.SqlTypesUtil.Unwrap(rawValue);
        }

        public override int Length
        {
            get { return _orderedFieldNames.Length; }
        }

        public override bool IsNull(int fieldOrdinal)
        {
            return GetBoxedFieldValue(fieldOrdinal) == null;
        }

        protected string[] GetFieldNames()
        {
            return _orderedFieldNames;
        }
    } // class SoodaObjectFieldValues
} // namespace
