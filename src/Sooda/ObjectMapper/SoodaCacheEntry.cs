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
using System.IO;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Globalization;

namespace Sooda.ObjectMapper {
    public class SoodaCacheEntry {
        private int _dataLoadedMask;
        private SoodaObjectFieldValues _data;
        private DateTime _timestamp;

        public SoodaCacheEntry(int dataLoadedMask, SoodaObjectFieldValues data) {
            _dataLoadedMask = dataLoadedMask;
            _data = data;
            _timestamp = DateTime.Now;
        }

        public SoodaObjectFieldValues Data
        {
            get {
                return _data;
            }
        }

        public int DataLoadedMask
        {
            get {
                return _dataLoadedMask;
            }
        }

        public DateTime TimeStamp
        {
            get {
                return _timestamp;
            }
        }

        public TimeSpan Age
        {
            get {
                return DateTime.Now - TimeStamp;
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Data.Length; ++i)
            {
                object o = Data.GetBoxedFieldValue(i);
                if (sb.Length != 0)
                    sb.Append(",");

                if (o == null)
                    sb.Append("null");
                else if (o is string) {
                    sb.Append("'");
                    sb.Append((string)o);
                    sb.Append("'");
                } else {
                    sb.Append(Convert.ToString(o, CultureInfo.InvariantCulture));
                }
            }
            return String.Format("Mask: [{0}] Data: [{1}]", DataLoadedMask, sb.ToString());
        }
    }
}
