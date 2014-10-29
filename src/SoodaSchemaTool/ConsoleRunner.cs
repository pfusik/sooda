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
using System.IO;
using System.Xml;

using Sooda.Schema;
using Sooda.Sql;
using System.Reflection;
using System.Collections;
using System.Globalization;

namespace SoodaSchemaTool
{
    public class ConsoleRunner
    {
        private static void Usage()
        {
            CommandFactory.CreateCommand("help").Run(new string[] { });
        }

        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return 1;
            }

            try
            {
                Command cmd = CommandFactory.CreateCommand(args[0]);
                ArrayList actualCommandLine = new ArrayList();

                for (int i = 1; i < args.Length; ++i)
                {
                    if (args[i].StartsWith("-"))
                    {
                        string name = args[i].Substring(1);
                        PropertyInfo pi = cmd.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
                        if (pi == null)
                            throw new Exception(name + " not found in " + cmd.GetType().Name);

                        if (pi.PropertyType == typeof(bool))
                        {
                            pi.SetValue(cmd, true, null);
                        }
                        else
                        {
                            string value = args[++i];
                            object typedValue = Convert.ChangeType(value, pi.PropertyType, CultureInfo.InvariantCulture);
                            pi.SetValue(cmd, typedValue, null);
                        }
                        continue;
                    }
                    else
                    {
                        actualCommandLine.Add(args[i]);
                    }
                }

                return cmd.Run((string[])actualCommandLine.ToArray(typeof(string)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }
    }
}