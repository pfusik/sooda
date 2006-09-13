using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using SoodaAddin.UI;

namespace ConfigureSoodaProject
{
    class ProjectFileConfigurationStrategy : ISoodaConfigurationStrategy
    {
        private string _projectFile;
        private XmlDocument _projectXml;
        private XmlNamespaceManager _namespaceManager;
        private bool _modified = false;
        private bool _isVS2003;

        public ProjectFileConfigurationStrategy(string projectFile)
        {
            if (projectFile != null)
                ProjectFile = projectFile;
        }

        public string ProjectFile
        {
            get { return _projectFile; }
            set 
            {
                _modified = false;
                _projectFile = value; 
                _projectXml = new XmlDocument();
                _projectXml.Load(_projectFile);
                _namespaceManager = new XmlNamespaceManager(_projectXml.NameTable);
                _namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

                _isVS2003 = _projectXml.DocumentElement.LocalName != "Project";
            }
        }

        public string AssemblyName
        {
            get { 
                if (_isVS2003)
                {
                    return null;
                }
                else
                {
                    return _projectXml.SelectSingleNode("//msbuild:AssemblyName", _namespaceManager).InnerText;
                }
            }
        }

        private XmlElement GetItemGroup(string whichHas)
        {
            XmlElement itemGroup = (XmlElement)_projectXml.SelectSingleNode("msbuild:Project/msbuild:ItemGroup[msbuild:" + whichHas + "]", _namespaceManager);
            if (itemGroup == null)
            {
                itemGroup = _projectXml.CreateElement("", "ItemGroup", "http://schemas.microsoft.com/developer/msbuild/2003");
                _projectXml.DocumentElement.AppendChild(itemGroup);
            }
            return itemGroup;
        }

        public void AddCompileUnit(string relativeFileName)
        {
            XmlElement compileItemGroup = GetItemGroup("Compile");

            XmlElement file = (XmlElement)compileItemGroup.SelectSingleNode("msbuild:Compile[@Include='" + relativeFileName + "']", _namespaceManager);
            if (file == null)
            {
                XmlElement el = _projectXml.CreateElement("", "Compile", "http://schemas.microsoft.com/developer/msbuild/2003");
                el.SetAttribute("Include", relativeFileName);
                compileItemGroup.AppendChild(el);
                _modified = true;
            }
        }

        public void AddReference(string reference)
        {
            XmlElement compileItemGroup = GetItemGroup("Reference");

            XmlElement file = (XmlElement)compileItemGroup.SelectSingleNode("msbuild:Reference[@Include='" + reference + "']", _namespaceManager);
            if (file == null)
            {
                XmlElement el = _projectXml.CreateElement("", "Reference", "http://schemas.microsoft.com/developer/msbuild/2003");
                el.SetAttribute("Include", reference);
                compileItemGroup.AppendChild(el);
                _modified = true;
            }
        }

        public void AddReferenceWithHintPath(string reference, string hintPath)
        {
            XmlElement compileItemGroup = GetItemGroup("Reference");

            XmlElement file = (XmlElement)compileItemGroup.SelectSingleNode("msbuild:Reference[@Include='" + reference + "']", _namespaceManager);
            if (file == null)
            {
                XmlElement el = _projectXml.CreateElement("", "Reference", "http://schemas.microsoft.com/developer/msbuild/2003");
                el.SetAttribute("Include", reference);
                compileItemGroup.AppendChild(el);
                XmlElement specificVersion = _projectXml.CreateElement("", "SpecificVersion", "http://schemas.microsoft.com/developer/msbuild/2003");
                specificVersion.InnerText = "false";

                XmlElement hintPathElement = _projectXml.CreateElement("", "HintPath", "http://schemas.microsoft.com/developer/msbuild/2003");
                hintPathElement.InnerText = hintPath;
                el.AppendChild(specificVersion);
                el.AppendChild(hintPathElement);
                _modified = true;
            }
        }

        public void AddEmbeddedResource(string relativeFileName)
        {
            XmlElement compileItemGroup = GetItemGroup("Compile");

            XmlElement file = (XmlElement)compileItemGroup.SelectSingleNode("msbuild:EmbeddedResource[@Include='" + relativeFileName + "']", _namespaceManager);
            if (file == null)
            {
                XmlElement el = _projectXml.CreateElement("", "EmbeddedResource", "http://schemas.microsoft.com/developer/msbuild/2003");
                el.SetAttribute("Include", relativeFileName);
                compileItemGroup.AppendChild(el);
                _modified = true;
            }
        }

        public void AddContent(string relativeFileName)
        {
            XmlElement compileItemGroup = GetItemGroup("Compile");

            XmlElement file = (XmlElement)compileItemGroup.SelectSingleNode("msbuild:Content[@Include='" + relativeFileName + "']", _namespaceManager);
            if (file == null)
            {
                XmlElement el = _projectXml.CreateElement("", "Content", "http://schemas.microsoft.com/developer/msbuild/2003");
                el.SetAttribute("Include", relativeFileName);
                compileItemGroup.AppendChild(el);
                _modified = true;
            }
        }

        public void SaveProject()
        {
            if (_modified)
            {
                _projectXml.Save(_projectFile);
            }
        }

        public string ProjectType
        {
            get { return _isVS2003 ? "vs2003" : "vs2005"; }
        }

        public string DefaultNamespace
        {
            get 
            { 
                if (_isVS2003)
                {
                    return ((XmlElement)_projectXml.SelectSingleNode("//Settings")).GetAttribute("RootNamespace");
                }
                else
                {
                    return _projectXml.SelectSingleNode("//msbuild:RootNamespace", _namespaceManager).InnerText;
                }
            }
        }

        public string FileExtension
        {
            get { return ".cs"; }
        }

        public string PreBuildEvent
        {
            get 
            { 
                XmlElement preBuildEvent = (XmlElement)_projectXml.SelectSingleNode("//msbuild:PreBuildEvent", _namespaceManager);
                if (preBuildEvent == null)
                    return "";
                return preBuildEvent.InnerText;
            }
            set
            {
                XmlElement preBuildEvent = (XmlElement)_projectXml.SelectSingleNode("//msbuild:PreBuildEvent", _namespaceManager);
                if (preBuildEvent == null)
                {
                    preBuildEvent = _projectXml.CreateElement("", "PreBuildEvent", "http://schemas.microsoft.com/developer/msbuild/2003"); 
                    XmlElement propertyGroup = _projectXml.CreateElement("PropertyGroup", "http://schemas.microsoft.com/developer/msbuild/2003");
                    _projectXml.DocumentElement.AppendChild(propertyGroup);
                    propertyGroup.AppendChild(preBuildEvent);
                }
                preBuildEvent.InnerText = value;
                _modified = true;
            }
        }
    }

	public class Program
	{
        public static void Main(string[] args)
        {
            string projectFile;

            if (args.Length == 0)
            {
                projectFile = null;
            }
            else
            {
                projectFile = args[0];
            }

            ProjectFileConfigurationStrategy strategy = new ProjectFileConfigurationStrategy(projectFile);

            Application.EnableVisualStyles();
            Application.Run(new SoodaConfigurationWizard(strategy));
#if A
            using (SoodaConfigurationWizard wizard = new SoodaConfigurationWizard())
            {
                if (DialogResult.OK == wizard.ShowDialog())
                {
                    WizardResult result = wizard.Result;

                    using (ConfiguringSoodaForm csf = new ConfiguringSoodaForm())
                    {
                        csf.ShowDialog();
                    }
                }
            }
#endif
        }
	}
}
