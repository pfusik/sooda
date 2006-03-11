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

namespace Sooda.Logging
{
    public abstract class Logger
    {
        public abstract bool IsTraceEnabled { get; }
        public abstract void Trace(IFormatProvider fp, string format, params object[] par);
        public abstract void Trace(string format, params object[] par);
        public abstract void Trace(string message);

        public abstract bool IsDebugEnabled { get; }
        public abstract void Debug(IFormatProvider fp, string format, params object[] par);
        public abstract void Debug(string format, params object[] par);
        public abstract void Debug(string message);

        public abstract bool IsInfoEnabled { get; }
        public abstract void Info(IFormatProvider fp, string format, params object[] par);
        public abstract void Info(string format, params object[] par);
        public abstract void Info(string message);

        public abstract bool IsErrorEnabled { get; }
        public abstract void Error(IFormatProvider fp, string format, params object[] par);
        public abstract void Error(string format, params object[] par);
        public abstract void Error(string message);

        public abstract bool IsWarnEnabled { get; }
        public abstract void Warn(IFormatProvider fp, string format, params object[] par);
        public abstract void Warn(string format, params object[] par);
        public abstract void Warn(string message);

        public abstract bool IsFatalEnabled { get; }
        public abstract void Fatal(IFormatProvider fp, string format, params object[] par);
        public abstract void Fatal(string format, params object[] par);
        public abstract void Fatal(string message);
    }
}
