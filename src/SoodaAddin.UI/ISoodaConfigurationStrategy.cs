using System;

namespace SoodaAddin.UI
{
	public interface ISoodaConfigurationStrategy
	{
        string ProjectFile { get; set; }
        string AssemblyName { get; }
        void AddCompileUnit(string file);
        void AddContent(string file);
        void AddEmbeddedResource(string file);
        void AddReference(string reference);
        void AddReferenceWithHintPath(string reference, string hintPath);
        void SaveProject();
        string PreBuildEvent { get; set; }
        string ProjectType { get; }
        string DefaultNamespace { get; }
        string FileExtension { get; }
    }
}
