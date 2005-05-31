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

namespace Sooda.ObjectMapper.FieldHandlers {
    public class StringFieldHandler : SoodaFieldHandler {
        public StringFieldHandler(bool nullable) : base(nullable) {}

        protected override string TypeName
        {
            get {
                return "string";
            }
        }

        public System.Data.SqlTypes.SqlString GetSqlNullableValue(object fieldValue) {
            if (fieldValue == null)
                return System.Data.SqlTypes.SqlString.Null;
            else
                return new System.Data.SqlTypes.SqlString((String)fieldValue);
        }

        public string GetNotNullValue(object val) {
            if (val == null)
                throw new InvalidOperationException("Attempt to read a non-null value that isn't set yes");
            return (string)val;
        }

        public override object RawRead(IDataRecord record, int pos) {
            return GetFromReader(record, pos);
        }

        public override string RawSerialize(object val) {
            return SerializeToString(val);
        }

        public override object RawDeserialize(string s) {
            return DeserializeFromString(s);
        }

        public static String GetFromReader(IDataRecord record, int pos) {
            return record.GetString(pos);
        }

        public static object GetBoxedFromReader(IDataRecord record, int pos) {
            object v = record.GetValue(pos);
            if (!(v is string))
                throw new SoodaDatabaseException();
            return v;
        }

        public static string SerializeToString(object o) {
            return (string)o;
        }

        public static object DeserializeFromString(string s) {
            return s;
        }

        private static object _zeroValue = String.Empty;
        public override object ZeroValue() {
            return _zeroValue;
        }

        public override Type GetFieldType() {
            return typeof(String);
        }
    }
}