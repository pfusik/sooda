//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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
    public abstract class BasicLogger : Logger
    {
        readonly string Prefix;

        public BasicLogger(string name)
        {
            Prefix = name + ": ";
        }

        public override bool IsTraceEnabled { get { return true; } }
        public override void Trace(IFormatProvider fp, string format, params object[] par) { OutputText(Prefix + String.Format(fp, format, par)); }
        public override void Trace(string format, params object[] par) { OutputText(Prefix + format, par); }
        public override void Trace(string message) { OutputText(Prefix + message); }

        public override bool IsDebugEnabled { get { return true; } }
        public override void Debug(IFormatProvider fp, string format, params object[] par) { OutputText(Prefix + String.Format(fp, format, par)); }
        public override void Debug(string format, params object[] par) { OutputText(Prefix + format, par); }
        public override void Debug(string message) { OutputText(Prefix + message); }

        public override bool IsInfoEnabled { get { return true; } }
        public override void Info(IFormatProvider fp, string format, params object[] par) { OutputText(Prefix + String.Format(fp, format, par)); }
        public override void Info(string format, params object[] par) { OutputText(Prefix + format, par); }
        public override void Info(string message) { OutputText(Prefix + message); }

        public override bool IsWarnEnabled { get { return true; } }
        public override void Warn(IFormatProvider fp, string format, params object[] par) { OutputText(Prefix + String.Format(fp, format, par)); }
        public override void Warn(string format, params object[] par) { OutputText(Prefix + format, par); }
        public override void Warn(string message) { OutputText(Prefix + message); }

        public override bool IsErrorEnabled { get { return true; } }
        public override void Error(IFormatProvider fp, string format, params object[] par) { OutputText(Prefix + String.Format(fp, format, par)); }
        public override void Error(string format, params object[] par) { OutputText(Prefix + format, par); }
        public override void Error(string message) { OutputText(Prefix + message); }

        public override bool IsFatalEnabled { get { return true; } }
        public override void Fatal(IFormatProvider fp, string format, params object[] par) { OutputText(Prefix + String.Format(fp, format, par)); }
        public override void Fatal(string format, params object[] par) { OutputText(Prefix + format, par); }
        public override void Fatal(string message) { OutputText(Prefix + message); }

        protected virtual void OutputText(string message, object[] parameters)
        {
            OutputText(String.Format(message, parameters));
        }

        protected abstract void OutputText(string message);
    }
}
