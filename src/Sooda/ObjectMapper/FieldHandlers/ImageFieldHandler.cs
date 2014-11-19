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
using System.Drawing;
using System.IO;

namespace Sooda.ObjectMapper.FieldHandlers
{
    public class ImageFieldHandler : SoodaFieldHandler
    {
        public ImageFieldHandler(bool nullable) : base(nullable) { }

        protected override string TypeName
        {
            get
            {
                return "image";
            }
        }

        public override object RawRead(IDataRecord record, int pos)
        {
            return GetFromReader(record, pos);
        }

        public static Image GetFromReader(IDataRecord record, int pos)
        {
            long n = record.GetBytes(pos, 0, null, 0, 0);
            if (n <= 0)
                return null;

            byte[] buf = new byte[n];
            record.GetBytes(pos, 0, buf, 0, buf.Length);

            return ImageFromBytes(buf);
        }

        private static Image ImageFromBytes(byte[] buf)
        {
            try
            {
                // must not do using() here - http://support.microsoft.com/?id=814675

                MemoryStream ms = new MemoryStream(buf);
                Image img = Image.FromStream(ms);
                return img;
            }
            catch (ArgumentException)
            {
                // it's possible that the image has been saved
                // with OLE header, strip it. It's the first 78 bytes.

                MemoryStream ms = new MemoryStream(buf, 78, buf.Length - 78);
                Image img = Image.FromStream(ms);
                return img;
            }
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
            Image img = (Image)obj;
            // must not do using() here - http://support.microsoft.com/?id=814675
            MemoryStream ms = new MemoryStream();
            img.Save(ms, img.RawFormat);

            byte[] d = ms.GetBuffer();
            return System.Convert.ToBase64String(d);
        }

        public static object DeserializeFromString(string s)
        {
            byte[] data = Convert.FromBase64String(s);
            MemoryStream ms = new MemoryStream(data);
            return Image.FromStream(ms);
        }

        private const object _zeroValue = null;
        public override object ZeroValue()
        {
            return _zeroValue;
        }

        public override Type GetFieldType()
        {
            return typeof(Image);
        }

        public override Type GetSqlType()
        {
            return null;
        }

        public override void SetupDBParameter(IDbDataParameter parameter, object value)
        {
            System.Drawing.Image img = (System.Drawing.Image)value;

            MemoryStream ms = new MemoryStream();
            img.Save(ms, img.RawFormat);

            parameter.DbType = DbType.Binary;
            parameter.Value = ms.GetBuffer();
        }

        public static Image GetNotNullValue(object val)
        {
            return (Image)val;
        }
    }
}
