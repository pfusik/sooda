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
using System.Data;
using System.Globalization;

namespace Sooda.ObjectMapper.FieldHandlers
{
    public class DecimalFieldHandler : SoodaFieldHandler
    {
        public DecimalFieldHandler(bool nullable) : base(nullable) { }

        protected override string TypeName
        {
            get
            {
                return "decimal";
            }
        }

        public override object RawRead(IDataRecord record, int pos)
        {
            return GetFromReader(record, pos);
        }

        public static Decimal GetFromReader(IDataRecord record, int pos)
        {
            return record.GetDecimal(pos);
        }

        public override string RawSerialize(object val)
        {
            return SerializeToString(val);
        }

        public override object RawDeserialize(string s)
        {
            return DeserializeFromString(s);
        }

        public static string SerializeToString(object obj)
        {
            return Convert.ToDecimal(obj).ToString(CultureInfo.InvariantCulture);
        }

        public static object DeserializeFromString(string s)
        {
            return Decimal.Parse(s, CultureInfo.InvariantCulture);
        }

        private static readonly object _zeroValue = (decimal)0.0m;
        public override object ZeroValue()
        {
            return _zeroValue;
        }

        public override Type GetFieldType()
        {
            return typeof(Decimal);
        }

        public override Type GetSqlType()
        {
            return typeof(System.Data.SqlTypes.SqlDecimal);
        }

        public override void SetupDBParameter(IDbDataParameter parameter, object value)
        {
            parameter.DbType = DbType.Decimal;
            parameter.Value = value;
        }

        // type conversions - used in generated stub code

        public static System.Data.SqlTypes.SqlDecimal GetSqlNullableValue(object fieldValue)
        {
            if (fieldValue == null)
                return System.Data.SqlTypes.SqlDecimal.Null;
            else
                return new System.Data.SqlTypes.SqlDecimal((Decimal)fieldValue);
        }

        public static decimal GetNotNullValue(object val)
        {
            if (val == null)
                throw new InvalidOperationException("Attempt to read a non-null value that isn't set yet");
            return (decimal)val;
        }

        public static decimal? GetNullableValue(object fieldValue)
        {
            if (fieldValue == null)
                return null;
            else
                return (decimal)fieldValue;
        }

        public override Type GetNullableType()
        {
            return typeof(decimal?);
        }
    }
}
