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

using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;
using NAnt.Core;

using Sooda.CodeGen;

namespace Sooda.NAnt.Tasks
{
    /// <summary>
    /// Generates source code 
    /// </summary>
    [TaskName("sooda-generate-code")]
    public class SoodaCodeGenTask : Task, ICodeGeneratorOutput
    {
        #region Private Instance Fields

        private FileInfo _projectFile;
        private bool _rebuild = false;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The name of the file which will have its attributes set. This is 
        /// provided as an alternate to using the task's fileset.
        /// </summary>
        [TaskAttribute("project", Required = true)]
        public FileInfo ProjectFile
        {
            get { return _projectFile; }
            set { _projectFile = value; }
        }

        [TaskAttribute("rebuild")]
        public bool Rebuild
        {
            get { return _rebuild; }
            set { _rebuild = value; }
        }

        #endregion

        #region Override implementation of Task

        protected override void ExecuteTask()
        {
            Log(Level.Verbose, "Loading Sooda project from '{0}'", _projectFile.FullName);
            SoodaProject project = SoodaProject.LoadFrom(_projectFile.FullName);
            CodeGenerator generator = new CodeGenerator(project, this);
            if (Rebuild)
                generator.RebuildIfChanged = false;

            string oldDirectory = Environment.CurrentDirectory;
            try
            {
                // temporarily change to the directory which contains the project file
                Environment.CurrentDirectory = _projectFile.DirectoryName;
                generator.Run();
            }
            finally
            {
                Environment.CurrentDirectory = oldDirectory;
            }
        }

        #endregion Override implementation of Task

        #region ICodeGeneratorOutput Members

        void ICodeGeneratorOutput.Info(string s, params object[] p)
        {
            Log(Level.Info, s, p);
        }

        void ICodeGeneratorOutput.Warning(string s, params object[] p)
        {
            Log(Level.Warning, s, p);
        }

        void ICodeGeneratorOutput.Verbose(string s, params object[] p)
        {
            Log(Level.Verbose, s, p);
        }

        #endregion
    }
}
