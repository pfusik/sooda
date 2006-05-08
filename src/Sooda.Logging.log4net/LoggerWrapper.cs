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

using log4net;

namespace log4netwrapper
{
    internal sealed class LoggerWrapper : Sooda.Logging.Logger
    {
        private ILog _logger;

        public LoggerWrapper(ILog logger)
        {
            _logger = logger;
        }

        // log4net has no trace so we map Trace to Debug

        #region Trace

        public override bool IsTraceEnabled 
        {
            get { return _logger.IsDebugEnabled; }
        }

        public override void Trace(IFormatProvider fp, string format, params object[] par)
        {
            _logger.DebugFormat(fp, format, par);
        }

        public override void Trace(string format, params object[] par)
        {
            _logger.DebugFormat(format, par);
        }

        public override void Trace(string message)
        {
            _logger.Debug(message);
        }

        #endregion

        #region Debug

        public override bool IsDebugEnabled 
        {
            get { return _logger.IsDebugEnabled; }
        }

        public override void Debug(IFormatProvider fp, string format, params object[] par)
        {
            _logger.DebugFormat(fp, format, par);
        }

        public override void Debug(string format, params object[] par)
        {
            _logger.DebugFormat(format, par);
        }

        public override void Debug(string message)
        {
            _logger.Debug(message);
        }

        #endregion

        #region Info

        public override bool IsInfoEnabled 
        {
            get { return _logger.IsInfoEnabled; }
        }

        public override void Info(IFormatProvider fp, string format, params object[] par)
        {
            _logger.InfoFormat(fp, format, par);
        }

        public override void Info(string format, params object[] par)
        {
            _logger.InfoFormat(format, par);
        }

        public override void Info(string message)
        {
            _logger.Info(message);
        }

        #endregion

        #region Warn

        public override bool IsWarnEnabled 
        {
            get { return _logger.IsWarnEnabled; }
        }

        public override void Warn(IFormatProvider fp, string format, params object[] par)
        {
            _logger.WarnFormat(fp, format, par);
        }

        public override void Warn(string format, params object[] par)
        {
            _logger.WarnFormat(format, par);
        }

        public override void Warn(string message)
        {
            _logger.Warn(message);
        }

        #endregion

        #region Error

        public override bool IsErrorEnabled 
        {
            get { return _logger.IsErrorEnabled; }
        }

        public override void Error(IFormatProvider fp, string format, params object[] par)
        {
            _logger.ErrorFormat(fp, format, par);
        }

        public override void Error(string format, params object[] par)
        {
            _logger.ErrorFormat(format, par);
        }

        public override void Error(string message)
        {
            _logger.Error(message);
        }

        #endregion

        #region Fatal

        public override bool IsFatalEnabled 
        {
            get { return _logger.IsFatalEnabled; }
        }

        public override void Fatal(IFormatProvider fp, string format, params object[] par)
        {
            _logger.FatalFormat(fp, format, par);
        }

        public override void Fatal(string format, params object[] par)
        {
            _logger.FatalFormat(format, par);
        }

        public override void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        #endregion
    }
}
