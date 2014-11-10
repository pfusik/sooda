using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using SoodaAddin.UI;

namespace ConfigureSoodaProject
{
    class ProjectFileConfigurationStrategy : ISoodaConfigurationStrategy
    {
        string _projectFile;
        XmlDocument _projectXml;
        XmlNamespaceManager _namespaceManager;
        bool _modified = false;
        const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public ProjectFileConfigurationStrategy(string projectFile)
        {
            if (projectFile != null)
                ProjectFile = projectFile;
        }

        XmlElement SelectElement(XmlNode parent, string xpath)
        {
            return (XmlElement) parent.SelectSingleNode(xpath, _namespaceManager);
        }

        XmlElement CreateElement(string name)
        {
            return _projectXml.CreateElement("", name, msbuildNamespace);
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
                _namespaceManager.AddNamespace("msbuild", msbuildNamespace);
            }
        }

        public string AssemblyName
        {
            get
            {
                return SelectElement(_projectXml, "//msbuild:AssemblyName").InnerText;
            }
        }

        XmlElement AddItem(string whichHas, string type, string filename)
        {
            XmlElement itemGroup = SelectElement(_projectXml, "msbuild:Project/msbuild:ItemGroup[msbuild:" + whichHas + "]");
            if (itemGroup == null)
            {
                itemGroup = CreateElement("ItemGroup");
                _projectXml.DocumentElement.AppendChild(itemGroup);
            }
            else if (SelectElement(itemGroup, "msbuild:" + type + "[@Include='" + filename + "']") != null)
                return null;

            XmlElement el = CreateElement(type);
            el.SetAttribute("Include", filename);
            itemGroup.AppendChild(el);
            _modified = true;
            return el;
        }

        public void AddCompileUnit(string relativeFileName)
        {
            AddItem("Compile", "Compile", relativeFileName);
        }

        public void AddReference(string reference)
        {
            AddItem("Reference", "Reference", reference);
        }

        public void AddReferenceWithHintPath(string reference, string hintPath)
        {
            XmlElement el = AddItem("Reference", "Reference", reference);
            if (el != null)
            {
                XmlElement specificVersion = CreateElement("SpecificVersion");
                specificVersion.InnerText = "false";
                el.AppendChild(specificVersion);

                XmlElement hintPathElement = CreateElement("HintPath");
                hintPathElement.InnerText = hintPath;
                el.AppendChild(hintPathElement);
            }
        }

        public void AddEmbeddedResource(string relativeFileName)
        {
            AddItem("Compile", "EmbeddedResource", relativeFileName);
        }

        public void AddContent(string relativeFileName)
        {
            AddItem("Compile", "Content", relativeFileName);
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
            get { return "vs2005"; }
        }

        public string DefaultNamespace
        {
            get
            {
                XmlElement el = SelectElement(_projectXml, "//msbuild:RootNamespace");
                return el != null ? el.InnerText : null;
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
                XmlElement preBuildEvent = SelectElement(_projectXml, "//msbuild:PreBuildEvent");
                if (preBuildEvent == null)
                    return "";
                return preBuildEvent.InnerText;
            }
            set
            {
                XmlElement preBuildEvent = SelectElement(_projectXml, "//msbuild:PreBuildEvent");
                if (preBuildEvent == null)
                {
                    preBuildEvent = CreateElement("PreBuildEvent");
                    XmlElement propertyGroup = CreateElement("PropertyGroup");
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
