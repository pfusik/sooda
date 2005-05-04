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
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;

namespace Sooda.ObjectMapper {
    public class SoodaCache {
        private static Hashtable _hashtable = new Hashtable();
        private static NLog.Logger logger = NLog.LogManager.GetLogger("Sooda.Cache");

        public static TimeSpan ExpirationTimeout = TimeSpan.FromMinutes(1);
        public static bool Enabled = false;

        public static SoodaCacheEntry FindObjectData(string className, object primaryKeyValue) {
            if (!Enabled)
                return null;

            Hashtable ht = (Hashtable)_hashtable[className];
            SoodaCacheEntry retVal = null;

            if (ht != null) {
                retVal = (SoodaCacheEntry)ht[primaryKeyValue];
                if (retVal != null) {
                    if (retVal.Age > ExpirationTimeout) {
                        ht.Remove(primaryKeyValue);
                        retVal = null;
                    }
                }
            }

            logger.Debug("SoodaCache.FindObjectData('{0}',{1}) {2}", className, primaryKeyValue, (retVal != null) ? "FOUND" : "NOT FOUND");
            return retVal;
        }

        public static void AddObject(string className, object primaryKeyValue, SoodaCacheEntry entry) {
            if (!Enabled)
                return ;

            Hashtable ht = (Hashtable)_hashtable[className];
            if (ht == null) {
                ht = new Hashtable();
                _hashtable[className] = ht;
            }

            if (logger.IsDebugEnabled) {
                logger.Debug("Add {0}({1}): {2}", className, primaryKeyValue, entry.ToString());
            }

            ht[primaryKeyValue] = entry;
        }

        public static void Clear() {
            logger.Debug("Clear");
            _hashtable.Clear();
        }

        public static void Dump(TextWriter output) {
            output.WriteLine("CACHE DUMP:");
            foreach (string className in _hashtable.Keys) {
                output.WriteLine(className);

                foreach (DictionaryEntry de in (Hashtable)_hashtable[className]) {
                    SoodaCacheEntry entry = (SoodaCacheEntry)de.Value;

                    output.Write("{0,8} [", de.Key);
                    bool first = true;
                    for (int i = 0; i < entry.Data.Length; ++i)
                    {
                        object fd = entry.Data.GetBoxedFieldValue(i);
                        if (!first)
                            output.Write("|");
                        output.Write(fd);
                        first = false;
                    }
                    output.Write("]");
                    output.WriteLine();
                }
                output.WriteLine();
            }
            output.WriteLine();
        }
    }
}
