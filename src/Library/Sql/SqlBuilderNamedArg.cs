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
using System.IO;
using System.Data;
using System.Collections;

namespace Sooda.Sql {
    public abstract class SqlBuilderNamedArg : SqlBuilderBase {
        public override void BuildCommandWithParameters(System.Data.IDbCommand command, string query, object[] par) {
            command.Parameters.Clear();
            if (par == null || par.Length == 0) {
                command.CommandText = query;
                return ;
            };

            System.Text.StringBuilder sb = new System.Text.StringBuilder(query.Length * 2);

            for (int i = 0; i < query.Length; ++i) {
                char c = query[i];

                if (c == '\'')  // we leave strings untouched
                {
                    int stringStartPos = i;
                    int stringEndPos = -1;

                    for (int j = i + 1; j < query.Length; ++j) {
                        if (query[j] == '\'') {
                            // possible end of string, need to check for double apostrophes,
                            // which don't mean EOS

                            if (j + 1 < query.Length && query[j + 1] == '\'') {
                                j++;
                                continue;
                            }
                            stringEndPos = j;
                            break;
                        }
                    }

                    if (stringEndPos == -1) {
                        throw new ArgumentException("Query has unbalanced quotes");
                    }

                    sb.Append(query, stringStartPos, stringEndPos - stringStartPos + 1);

                    i = stringEndPos;
                    // string starts from i and ends at j INCLUSIVE
                } else if (c == '{') {
                    char c1 = query[i + 1];
                    char c2 = query[i + 2];
                    int paramNumber;

                    if (c2 == '}' || c2 == ':') {
                        paramNumber = c1 - '0';
                        i += 2;
                    } else {
                        paramNumber = (c1 - '0') * 10 + (c2 - '0');
                        if (query[i + 3] != '}' && query[i + 3] != ':')
                            throw new NotSupportedException("Max 99 positional parameters are supported");
                        i += 3;
                    };
                    bool bIn = true, bOut = false;
                    if (query[i] == ':') {
                        bIn = false;
                        i++;
                        while (query[i] != '}') {
                            if (Char.ToUpper(query[i]) == 'I') {
                                bIn = true;
                                i++;
                            } else if (Char.ToUpper(query[i]) == 'O') {
                                bOut = true;
                                i++;
                            } else
                                throw new ArgumentException("Unknown modifier for parameter " + paramNumber);
                        }
                    }

                    object v = par[paramNumber];
                    if (v == null)
                        sb.Append("null");
                    else {
                        IDbDataParameter p = command.CreateParameter();
                        if (bIn) {
                            p.Direction = ParameterDirection.Input;
                            if (bOut)
                                p.Direction = ParameterDirection.InputOutput;
                        } else if (bOut)
                            p.Direction = ParameterDirection.Output;
                        else
                            throw new ArgumentException("Direction not specified for parameter " + paramNumber);

                        string parName = GetNameForParameter(paramNumber);
						if (!command.Parameters.Contains(parName)) 
						{
							p.ParameterName = parName;
							if (v is Type) 
							{
								SetDbTypeFromClrType(p, (Type)v);
							} 
							else 
							{
								SetDbTypeFromClrType(p, v.GetType());

								// HACK
								if (v is System.Drawing.Image) 
								{
									System.Drawing.Image img = (System.Drawing.Image)v;

									MemoryStream ms = new MemoryStream();
									img.Save(ms, img.RawFormat);

									p.Value = ms.GetBuffer();
								} 
								else 
								{
									p.Value = v;
								}
							}
							command.Parameters.Add(p);
						}
                        sb.Append(parName);
                    }
                } else {
                    sb.Append(c);
                }
            }
            command.CommandText = sb.ToString();
        }
        protected abstract string GetNameForParameter(int pos);
    }
}
