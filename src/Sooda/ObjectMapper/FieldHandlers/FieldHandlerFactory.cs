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

using System.Collections;

using Sooda.Schema;

namespace Sooda.ObjectMapper.FieldHandlers
{
	public class FieldHandlerFactory
	{
		private static Hashtable _nullableHandlers = new Hashtable();
		private static Hashtable _notNullHandlers = new Hashtable();

		static FieldHandlerFactory()
		{
			_notNullHandlers[FieldDataType.Blob] = new Sooda.ObjectMapper.FieldHandlers.BlobFieldHandler(false);
			_notNullHandlers[FieldDataType.Boolean] = new Sooda.ObjectMapper.FieldHandlers.BooleanFieldHandler(false);
			_notNullHandlers[FieldDataType.BooleanAsInteger] = new Sooda.ObjectMapper.FieldHandlers.BooleanAsIntegerFieldHandler(false);
			_notNullHandlers[FieldDataType.Integer] = new Sooda.ObjectMapper.FieldHandlers.Int32FieldHandler(false);
			_notNullHandlers[FieldDataType.Long] = new Sooda.ObjectMapper.FieldHandlers.Int64FieldHandler(false);
			_notNullHandlers[FieldDataType.DateTime] = new Sooda.ObjectMapper.FieldHandlers.DateTimeFieldHandler(false);
			_notNullHandlers[FieldDataType.Decimal] = new Sooda.ObjectMapper.FieldHandlers.DecimalFieldHandler(false);
			_notNullHandlers[FieldDataType.Double] = new Sooda.ObjectMapper.FieldHandlers.DoubleFieldHandler(false);
			_notNullHandlers[FieldDataType.Float] = new Sooda.ObjectMapper.FieldHandlers.FloatFieldHandler(false);
			_notNullHandlers[FieldDataType.String] = new Sooda.ObjectMapper.FieldHandlers.StringFieldHandler(false);
			_notNullHandlers[FieldDataType.Guid] = new Sooda.ObjectMapper.FieldHandlers.GuidFieldHandler(false);
			_notNullHandlers[FieldDataType.Image] = new Sooda.ObjectMapper.FieldHandlers.ImageFieldHandler(false);
			_notNullHandlers[FieldDataType.TimeSpan] = new Sooda.ObjectMapper.FieldHandlers.TimeSpanFieldHandler(false);
			_notNullHandlers[FieldDataType.AnsiString] = new Sooda.ObjectMapper.FieldHandlers.AnsiStringFieldHandler(false);

			_nullableHandlers[FieldDataType.Blob] = new Sooda.ObjectMapper.FieldHandlers.BlobFieldHandler(true);
			_nullableHandlers[FieldDataType.Boolean] = new Sooda.ObjectMapper.FieldHandlers.BooleanFieldHandler(true);
			_nullableHandlers[FieldDataType.BooleanAsInteger] = new Sooda.ObjectMapper.FieldHandlers.BooleanAsIntegerFieldHandler(true);
			_nullableHandlers[FieldDataType.Integer] = new Sooda.ObjectMapper.FieldHandlers.Int32FieldHandler(true);
			_nullableHandlers[FieldDataType.Long] = new Sooda.ObjectMapper.FieldHandlers.Int64FieldHandler(true);
			_nullableHandlers[FieldDataType.DateTime] = new Sooda.ObjectMapper.FieldHandlers.DateTimeFieldHandler(true);
			_nullableHandlers[FieldDataType.Decimal] = new Sooda.ObjectMapper.FieldHandlers.DecimalFieldHandler(true);
			_nullableHandlers[FieldDataType.Double] = new Sooda.ObjectMapper.FieldHandlers.DoubleFieldHandler(true);
			_nullableHandlers[FieldDataType.Float] = new Sooda.ObjectMapper.FieldHandlers.FloatFieldHandler(true);
			_nullableHandlers[FieldDataType.String] = new Sooda.ObjectMapper.FieldHandlers.StringFieldHandler(true);
			_nullableHandlers[FieldDataType.Guid] = new Sooda.ObjectMapper.FieldHandlers.GuidFieldHandler(true);
			_nullableHandlers[FieldDataType.Image] = new Sooda.ObjectMapper.FieldHandlers.ImageFieldHandler(true);
			_nullableHandlers[FieldDataType.TimeSpan] = new Sooda.ObjectMapper.FieldHandlers.TimeSpanFieldHandler(true);
			_nullableHandlers[FieldDataType.AnsiString] = new Sooda.ObjectMapper.FieldHandlers.AnsiStringFieldHandler(true);
		}
 
		public static SoodaFieldHandler GetFieldHandler(FieldDataType type)
		{
			return GetFieldHandler(type, true);
		}

		public static SoodaFieldHandler GetFieldHandler(FieldDataType type, bool nullable)
		{
			object value;

			if (nullable)
				value = _nullableHandlers[type];
			else
				value = _notNullHandlers[type];

			if (value == null)
				throw new SoodaSchemaException("Field handler for type '" + type + "' not supported.");

			return (SoodaFieldHandler)value;
		}
	}
}
