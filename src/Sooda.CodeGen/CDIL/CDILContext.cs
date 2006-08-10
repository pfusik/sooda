// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Sooda.CodeGen.CDIL
{
    public class CDILContext
    {
        public static readonly CDILContext Null = new CDILContext();
        private Hashtable _params = new Hashtable();

        public string Format(string s)
        {
            StringBuilder result = new StringBuilder();

            int startingPos = 0;
            int pos = s.IndexOf("${", startingPos);

            while (pos >= 0)
            {
                if (pos != startingPos)
                {
                    result.Append(s, startingPos, pos - startingPos);
                }
                int pos2 = s.IndexOf("}", pos + 2);
                if (pos2 >= 0)
                {
                    startingPos = pos2 + 1;
                    string item = s.Substring(pos + 2, pos2 - pos - 2);
                    if (_params[item] == null)
                        throw new SoodaCodeGenException("parameter ${" + item + "} not defined in context.");
                    string replacement = Convert.ToString(_params[item]);

                    result.Append(replacement);
                    pos = s.IndexOf("${", startingPos);
                }
                else
                {
                    break;
                }
            }
            if (startingPos != s.Length)
            {
                result.Append(s, startingPos, s.Length - startingPos);
            }

            // Console.WriteLine("res: {0}", result);

            return result.ToString();
        }

        public object this[string name]
        {
            get
            {
                return _params[name];
            }
            set
            {
                _params[name] = value;
            }
        }
    }
}
