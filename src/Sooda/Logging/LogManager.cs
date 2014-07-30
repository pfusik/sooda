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

namespace Sooda.Logging
{
    public static class LogManager
    {
        private static ILoggingImplementation _implementation;

        static void Create()
        {
            // set up null implementation first so that any logging statements
            // from SoodaConfig will not result in NullReferenceException
            _implementation = new NullLoggingImplementation();
            string name = SoodaConfig.GetString("sooda.logging", "null");
            switch (name)
            {
                case "console":
                    _implementation = new ConsoleLoggingImplementation();
                    return;

                case "null":
                    return;

                case "nlog":
                    name = "Sooda.Logging.NLog.LoggingImplementation, Sooda.Logging.NLog";
                    break;

                case "log4net":
                    name = "Sooda.Logging.log4net.LoggingImplementation, Sooda.Logging.log4net";
                    break;

                default:
                    break;
            }
            try
            {
                ILoggingImplementation implementation = Activator.CreateInstance(Type.GetType(name)) as ILoggingImplementation;
                if (implementation != null)
                    _implementation = implementation;
            }
            catch (Exception)
            {
            }
        }

        public static ILoggingImplementation Implementation
        {
            get
            {
                ILoggingImplementation implementation = _implementation;
                if (implementation == null)
                {
                    lock (typeof (LogManager))
                    {
                        implementation = _implementation;
                        if (implementation == null)
                        {
                            Create();
                            implementation = _implementation;
                        }
                    }
                }
                return implementation;
            }
            set { _implementation = value; }
        }

        public static Logger GetLogger(string name)
        {
            return Implementation.GetLogger(name);
        }
    }
}
