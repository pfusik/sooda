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
using System.IO;

using System.Drawing;

namespace Sooda.ObjectMapper.FieldHandlers {
    public class ImageFieldHandler : SoodaFieldHandler {
        public ImageFieldHandler(bool nullable) : base(nullable) {}

        protected override string TypeName
        {
            get {
                return "image";
            }
        }

        public Image GetNotNullValue(object val) {
            // TEGO testu nie ma!!!!
            // if (val == null)
            //    throw new InvalidOperationException("Attempt to read a non-null value that isn't set yet");
            return (Image) val;
        }

        public override object RawRead(IDataRecord record, int pos) {
            return GetFromReader(record, pos);
        }

        public static Image GetFromReader(IDataRecord record, int pos) {
            long n = record.GetBytes(pos, 0, null, 0, 0);
            if (n <= 0)
                return null;

            byte[] buf = new byte[n];
            record.GetBytes(pos, 0, buf, 0, buf.Length);

            // tu nie mo¿e byæ using() - http://support.microsoft.com/?id=814675

            MemoryStream ms = new MemoryStream(buf);
            return Image.FromStream(ms);
        }

        public static object GetBoxedFromReader(IDataRecord record, int pos) {
            return GetFromReader(record, pos);
        }

        public override string RawSerialize(object val) {
            return SerializeToString(val);
        }

        public override object RawDeserialize(string s) {
            return DeserializeFromString(s);
        }

        public static string SerializeToString(object obj) {
            Image img = (Image)obj;
            // tu nie mo¿e byæ using() - http://support.microsoft.com/?id=814675
            MemoryStream ms = new MemoryStream();
            img.Save(ms, img.RawFormat);

            byte[] d = ms.GetBuffer();
            return System.Convert.ToBase64String(d);
        }

        public static object DeserializeFromString(string s) {
            byte[] data = Convert.FromBase64String(s);
            MemoryStream ms = new MemoryStream(data);
            return Image.FromStream(ms);
        }

        private static object _zeroValue = null;
        public override object ZeroValue() {
            return _zeroValue;
        }

        public override Type GetFieldType() {
            return typeof(Image);
        }
    }
}
