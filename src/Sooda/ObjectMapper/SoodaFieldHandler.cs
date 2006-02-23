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
using System.Data;
using System.Xml;

namespace Sooda.ObjectMapper {
    public abstract class SoodaFieldHandler {
        private bool _isNullable;

        protected SoodaFieldHandler(bool nullable) {
            this._isNullable = nullable;
        }

        public bool IsNullable
        {
            get {
                return _isNullable;
            }
        }

        public object Deserialize(XmlReader xr) {
            if (xr.GetAttribute("type") != TypeName)
                throw new ArgumentException("Invalid field type on deserialize");
            if (xr.GetAttribute("null") != null) {
                return null;
            } else {
                return RawDeserialize(xr.GetAttribute("value"));
            }
        }

        protected abstract string TypeName { get ; }

        public void Serialize(object fieldValue, XmlWriter xw) {
            xw.WriteAttributeString("type", TypeName);
            if (fieldValue == null)
                xw.WriteAttributeString("null", "true");
            else
                xw.WriteAttributeString("value", RawSerialize(fieldValue));
        }

        public abstract string RawSerialize(object val);
        public abstract object RawDeserialize(string s);
        public abstract object RawRead(IDataRecord record, int pos);
        public abstract object ZeroValue();

		public virtual object DefaultPrecommitValue()
		{
			return ZeroValue();
		}

        public abstract Type GetFieldType();
		public abstract Type GetSqlType();
        public virtual Type GetNullableType()
        {
            return null;
        }

		public abstract void SetupDBParameter(IDbDataParameter parameter, object value);

        public virtual string GetTypedWrapperClass(bool nullable)
        {
            return "Sooda.QL.TypedWrappers.Soql" + (nullable ? "Nullable" : "") + GetFieldType().Name + "WrapperExpression";
        }

    }
}

