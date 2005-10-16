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
        [TaskAttribute("project",Required=true)]
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
