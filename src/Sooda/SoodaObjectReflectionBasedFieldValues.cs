// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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

namespace Sooda 
{
    public abstract class SoodaObjectReflectionBasedFieldValues : SoodaObjectFieldValues
    {
        private string[] _orderedFieldNames;

        protected SoodaObjectReflectionBasedFieldValues(string[] orderedFieldNames)
        {
            _orderedFieldNames = orderedFieldNames;
        }

        protected SoodaObjectReflectionBasedFieldValues(SoodaObjectReflectionBasedFieldValues other, string[] orderedFieldNames)
        {
            _orderedFieldNames = orderedFieldNames;
            for (int i = 0; i < orderedFieldNames.Length; ++i)
            {
                FieldInfo fi = this.GetType().GetField(orderedFieldNames[i]);
                fi.SetValue(this, fi.GetValue(other));
            }
        }

        public override void SetFieldValue(int fieldOrdinal, object val)
        {
            System.Reflection.FieldInfo fi = this.GetType().GetField(_orderedFieldNames[fieldOrdinal]);
            if (typeof(System.Data.SqlTypes.INullable).IsAssignableFrom(fi.FieldType))
            {
                if (val == null)
                {
                    FieldInfo nullProperty = fi.FieldType.GetField("Null", BindingFlags.Static | BindingFlags.Public);
                    object sqlNullValue = nullProperty.GetValue(null);
                    fi.SetValue(this, sqlNullValue);
                }
                else
                {
                    Type[] constructorParameterTypes = new Type[] { val.GetType() };
                    ConstructorInfo constructorInfo = fi.FieldType.GetConstructor(constructorParameterTypes);
                    object sqlValue = constructorInfo.Invoke(new object[] { val });
                    fi.SetValue(this, sqlValue);
                }
            }
            else
            {
                fi.SetValue(this, val);
            }
        }

        public override object GetBoxedFieldValue(int fieldOrdinal)
        {
            System.Reflection.FieldInfo fi = this.GetType().GetField(_orderedFieldNames[fieldOrdinal]);
            object rawValue = fi.GetValue(this);

            if (rawValue == null)
                return null;
            
            // we got raw value, it's possible that it's a sqltype, nullables are already boxed here

            System.Data.SqlTypes.INullable sqlType = rawValue as System.Data.SqlTypes.INullable;
            if (sqlType != null)
            {
                if (sqlType.IsNull)
                    return null;

                return rawValue.GetType().GetProperty("Value").GetValue(rawValue, null);
            }

            return rawValue;
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
