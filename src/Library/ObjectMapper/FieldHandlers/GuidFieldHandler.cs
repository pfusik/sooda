// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

namespace Sooda.ObjectMapper.FieldHandlers
{
    public class GuidFieldHandler : SoodaFieldHandler
    {
        public GuidFieldHandler(bool nullable) : base(nullable)
        {
        }

        protected override string TypeName
        {
            get { return "guid"; } 
        }

        public System.Data.SqlTypes.SqlGuid GetSqlNullableValue(object fieldValue)
        {
            if (fieldValue == null)
                return System.Data.SqlTypes.SqlGuid.Null;
            else
                return new System.Data.SqlTypes.SqlGuid((Guid)fieldValue);
        }

        public Guid GetNotNullValue(object val)
        {
            if (val == null)
                throw new InvalidOperationException("Attempt to read a non-null value that isn't set yes");
            return (Guid)val;
        }

        public override object RawRead(IDataRecord record, int pos)
        {
            return GetFromReader(record, pos);
        }

		public override string RawSerialize(object val)
		{
			return SerializeToString(val);
		}

        public override object RawDeserialize(string s)
        {
            return DeserializeFromString(s);
        }

        public static Guid GetFromReader(IDataRecord record, int pos)
        {
            return record.GetGuid(pos);
        }

        public static object GetBoxedFromReader(IDataRecord record, int pos)
        {
            object v = record.GetValue(pos);
            if (!(v is Guid))
                throw new SoodaDatabaseException();
            return v;
        }

        public static string SerializeToString(object o)
        {
            return o.ToString();
        }

        public static object DeserializeFromString(string s)
        {
            return new Guid(s);
        }

        private static object _zeroValue = Guid.Empty;
        public override object ZeroValue()
        {
            return _zeroValue;
        }

        public override Type GetFieldType()
        {
            return typeof(Guid);
        }
    }
}
