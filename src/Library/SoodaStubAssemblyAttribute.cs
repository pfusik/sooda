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
using System.Reflection;

namespace Sooda
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SoodaStubAssemblyAttribute : Attribute
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SoodaStubAssemblyAttribute));
        private Assembly _assembly;

        public SoodaStubAssemblyAttribute()
        {
            _assembly = null;
        }

        public SoodaStubAssemblyAttribute(string assemblyName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            _assembly = null;

            foreach (Assembly a in assemblies)
            {
                if (a.GetName().Name == assemblyName)
                {
                    this._assembly = a;
                }
            }

            logger.Debug(String.Format("{0} resolved to {1}", assemblyName, _assembly.FullName));
        }

        public SoodaStubAssemblyAttribute(Type sampleObject)
        {
            _assembly = sampleObject.Assembly;
        }

        public Assembly Assembly
        {
            get { return _assembly; }
        }
    }
}
