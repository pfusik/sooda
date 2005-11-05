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
using System.Data;
using System.Globalization;
using System.Collections;

namespace Sooda.Sql 
{
    public abstract class SqlBuilderNamedArg : SqlBuilderBase 
    {
        private void SetParameterFromValue(IDbCommand command, IDbDataParameter p, object v)
        {
            p.Direction = ParameterDirection.Input;

            string parName = GetNameForParameter(command.Parameters.Count);
            p.ParameterName = parName;
            SetDbTypeFromValue(p, v);

            // HACK
            if (v is System.Drawing.Image) 
            {
                System.Drawing.Image img = (System.Drawing.Image)v;

                MemoryStream ms = new MemoryStream();
                img.Save(ms, img.RawFormat);

                p.Value = ms.GetBuffer();
            } 
            else if (v is TimeSpan)
            {
                p.Value = (int)(((TimeSpan)v).TotalSeconds);
            }
            else
            {
                p.Value = v;
            }
        }
        
        public override void BuildCommandWithParameters(System.Data.IDbCommand command, bool append, string query, object[] par) 
        {
            if (append)
            {
                if (command.CommandText == null)
                    command.CommandText = "";
                if (command.CommandText != "")
                    command.CommandText += ";";
            }
            else
            {
                command.CommandText = "";
                command.Parameters.Clear();
            }

            int startingParamNumber = command.Parameters.Count;

            System.Text.StringBuilder sb = new System.Text.StringBuilder(query.Length * 2);
            ArrayList parameterObjects = new ArrayList();

            for (int i = 0; i < query.Length; ++i) 
            {
                char c = query[i];

                if (c == '\'')  // locate the string
                {
                    int stringStartPos = i;
                    int stringEndPos = -1;

                    for (int j = i + 1; j < query.Length; ++j) 
                    {
                        if (query[j] == '\'') 
                        {
                            // possible end of string, need to check for double apostrophes,
                            // which don't mean EOS

                            if (j + 1 < query.Length && query[j + 1] == '\'') 
                            {
                                j++;
                                continue;
                            }
                            stringEndPos = j;
                            break;
                        }
                    }

                    if (stringEndPos == -1) 
                    {
                        throw new ArgumentException("Query has unbalanced quotes");
                    }

                    string stringValue = query.Substring(stringStartPos + 1, stringEndPos - stringStartPos - 1);
                    // replace double quotes with single quotes
                    stringValue = stringValue.Replace("''", "'");
                    IDbDataParameter p = command.CreateParameter();
                    
                    if (stringEndPos + 1 < query.Length && query[stringEndPos + 1] == 'D')
                    {
                        // datetime literal
                        SetParameterFromValue(command, p, DateTime.ParseExact(stringValue, "yyyyMMddHH:mm:ss", CultureInfo.InvariantCulture));
                        stringEndPos++;
                    }
                    else
                    {
                        SetParameterFromValue(command, p, stringValue);
                    }
                    command.Parameters.Add(p);
                    sb.Append(p.ParameterName);
                    
                    i = stringEndPos;
                } 
                else if (c == '{') 
                {
                    char c1 = query[i + 1];
                    char c2 = query[i + 2];
                    int paramNumber;

                    if (c2 == '}' || c2 == ':') 
                    {
                        paramNumber = c1 - '0';
                        i += 2;
                    } 
                    else 
                    {
                        paramNumber = (c1 - '0') * 10 + (c2 - '0');
                        if (query[i + 3] != '}' && query[i + 3] != ':')
                            throw new NotSupportedException("Max 99 positional parameters are supported");
                        i += 3;
                    };
                    bool bIn = true, bOut = false;

                    object v = par[paramNumber];

                    if (v is SoodaObject)
                    {
                        v = ((SoodaObject)v).GetPrimaryKeyValue();
                    }

                    if (v == null)
                    {
                        sb.Append("null");
                    }
                    //else if (v is int)
                    //{
                        //sb.Append(v.ToString());
                    //}
                    else
                    {
                        while (parameterObjects.Count <= paramNumber)
                            parameterObjects.Add(null);

                        if (parameterObjects[paramNumber] == null)
                        {
                            IDbDataParameter p = command.CreateParameter();
                            SetParameterFromValue(command, p, v);
                            parameterObjects[paramNumber] = p;
                            command.Parameters.Add(p);
                        }
                        sb.Append(((IDbDataParameter)parameterObjects[paramNumber]).ParameterName);
                    }
                } 
                else 
                {
                    sb.Append(c);
                }
            }
            command.CommandText += sb.ToString();
        }
        protected abstract string GetNameForParameter(int pos);
    }
}
