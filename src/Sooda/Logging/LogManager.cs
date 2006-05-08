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
    public sealed class LogManager
    {
        private static ILoggingImplementation _implementation;

        static LogManager()
        {
            // set up null implementation first so that any logging statements
            // from SoodaConfig will not result in NullReferenceException

            _implementation = new NullLoggingImplementation();
            try
            {
                string loggingImplementationName = SoodaConfig.GetString("sooda.logging", "null");
                string typeName;

                switch (loggingImplementationName)
                {
                    case "console":
                        _implementation = new ConsoleLoggingImplementation();
                        break;

                    case "null":
                        _implementation = new NullLoggingImplementation();
                        break;

                    case "nlog":
                        typeName = "Sooda.Logging.NLog.LoggingImplementation, Sooda.Logging.NLog";
                        _implementation = Activator.CreateInstance(Type.GetType(typeName)) as ILoggingImplementation;
                        break;

                    case "log4net":
                        typeName = "Sooda.Logging.log4net.LoggingImplementation, Sooda.Logging.log4net";
                        _implementation = Activator.CreateInstance(Type.GetType(typeName)) as ILoggingImplementation;
                        break;

                    default:
                        _implementation = Activator.CreateInstance(Type.GetType(loggingImplementationName)) as ILoggingImplementation;
                        break;
                }
            }
            catch (Exception)
            {
                _implementation = new NullLoggingImplementation();
            }
            finally
            {
                if (_implementation == null)
                    _implementation = new NullLoggingImplementation();
            }
        }

        public static ILoggingImplementation Implementation
        {
            get { return _implementation; }
            set { _implementation = value; }
        }

        public static Logger GetLogger(string name)
        {
            return _implementation.GetLogger(name);
        }
    }
}
