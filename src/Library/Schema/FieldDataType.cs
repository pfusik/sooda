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

namespace Sooda.Schema {
    using System;
    using System.Xml.Serialization;

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://sooda.sourceforge.net/schemas/DBSchema.xsd")]
    public enum FieldDataType
    {
        Integer,
        Long,
        Boolean,
        BooleanAsInteger,
        Decimal,
        Float,
        Double,
        DateTime,
        String,
        Blob,
        Guid,
        Image,
        TimeSpan,
    }

    internal class FieldDataLookup {
        public readonly FieldDataType fieldDataType;
        public readonly Type clrRawType;
        public readonly Type clrSqlType;
        public readonly string defaultWrapperTypeName;
        public readonly bool needsSize;
        public readonly bool needsPrecision;

        public FieldDataLookup(FieldDataType fieldDataType, Type clrRawType, Type clrSqlType, bool needsSize, bool needsPrecision, string defaultWrapperTypeName) {
            this.fieldDataType = fieldDataType;
            this.clrRawType = clrRawType;
            this.clrSqlType = clrSqlType;
            this.needsSize = needsSize;
            this.needsPrecision = needsPrecision;
            this.defaultWrapperTypeName = defaultWrapperTypeName;
        }
    }

    public class FieldDataTypeHelper {
        private static FieldDataLookup[] lookupTable = new FieldDataLookup[]
                {
                    new FieldDataLookup(FieldDataType.Blob, typeof(byte[]), typeof(System.Data.SqlTypes.SqlBinary), true, false, "Sooda.ObjectMapper.FieldHandlers.BlobFieldHandler"),
                    new FieldDataLookup(FieldDataType.Boolean, typeof(bool), typeof(System.Data.SqlTypes.SqlBoolean), false, false, "Sooda.ObjectMapper.FieldHandlers.BooleanFieldHandler"),
                    new FieldDataLookup(FieldDataType.BooleanAsInteger, typeof(bool), typeof(System.Data.SqlTypes.SqlBoolean), false, false, "Sooda.ObjectMapper.FieldHandlers.BooleanAsIntegerFieldHandler"),
                    new FieldDataLookup(FieldDataType.Integer, typeof(System.Int32), typeof(System.Data.SqlTypes.SqlInt32), false, false, "Sooda.ObjectMapper.FieldHandlers.Int32FieldHandler"),
                    new FieldDataLookup(FieldDataType.Long, typeof(System.Int64), typeof(System.Data.SqlTypes.SqlInt64), false, false, "Sooda.ObjectMapper.FieldHandlers.Int64FieldHandler"),
                    new FieldDataLookup(FieldDataType.DateTime, typeof(System.DateTime), typeof(System.Data.SqlTypes.SqlDateTime), false, false, "Sooda.ObjectMapper.FieldHandlers.DateTimeFieldHandler"),
                    new FieldDataLookup(FieldDataType.Decimal, typeof(System.Decimal), typeof(System.Data.SqlTypes.SqlDecimal), false, false, "Sooda.ObjectMapper.FieldHandlers.DecimalFieldHandler"),
                    new FieldDataLookup(FieldDataType.Double, typeof(System.Double), typeof(System.Data.SqlTypes.SqlDouble), true, true, "Sooda.ObjectMapper.FieldHandlers.DoubleFieldHandler"),
                    new FieldDataLookup(FieldDataType.Float, typeof(System.Single), typeof(System.Data.SqlTypes.SqlSingle), true, true, "Sooda.ObjectMapper.FieldHandlers.FloatFieldHandler"),
                    new FieldDataLookup(FieldDataType.String, typeof(System.String), typeof(System.Data.SqlTypes.SqlString), true, false, "Sooda.ObjectMapper.FieldHandlers.StringFieldHandler"),
                    new FieldDataLookup(FieldDataType.Guid, typeof(System.Guid), typeof(System.Data.SqlTypes.SqlGuid), true, false, "Sooda.ObjectMapper.FieldHandlers.GuidFieldHandler"),
                    new FieldDataLookup(FieldDataType.Image, typeof(System.Drawing.Image), null, false, false, "Sooda.ObjectMapper.FieldHandlers.ImageFieldHandler"),
                    new FieldDataLookup(FieldDataType.TimeSpan, typeof(System.TimeSpan), null, false, false, "Sooda.ObjectMapper.FieldHandlers.TimeSpanFieldHandler"),
        };

        public static Type GetClrType(FieldDataType t) {
            foreach (FieldDataLookup lookup in lookupTable)
            if (lookup.fieldDataType == t)
                return lookup.clrRawType;

            throw new NotSupportedException("Data type " + t + " not supported (yet!)");
        }

        public static string GetDefaultWrapperTypeName(FieldDataType t) {
            foreach (FieldDataLookup lookup in lookupTable)
            if (lookup.fieldDataType == t)
                return lookup.defaultWrapperTypeName;

            throw new NotSupportedException("Data type " + t + " not supported (yet!)");
        }

        public static Type GetSqlType(FieldDataType t) {
            foreach (FieldDataLookup lookup in lookupTable)
            if (lookup.fieldDataType == t)
                return lookup.clrSqlType;

            throw new NotSupportedException("Data type " + t + " not supported (yet!)");
        }

        public static FieldDataType FromCLRType(Type t) {
            foreach (FieldDataLookup lookup in lookupTable)
            if (lookup.clrRawType == t)
                return lookup.fieldDataType;

            throw new NotSupportedException("Data type " + t.FullName + " not supported (yet!)");
        }

        public static bool NeedsSize(FieldDataType t) {
            foreach (FieldDataLookup lookup in lookupTable)
            if (lookup.fieldDataType == t)
                return lookup.needsSize;

            throw new NotSupportedException("Data type " + t + " not supported (yet!)");
        }
    }

}
