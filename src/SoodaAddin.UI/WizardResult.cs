using System;

namespace SoodaAddin.UI
{
	public class WizardResult
	{
        public string ConnectionString;
        public string SelectedDatabase;
        public bool ReverseEngineerDatabase;
        public bool CreateAppConfigFile;
        public bool CreateSoodaXmlConfigFile;
        public bool ModifyAssemblyInfo;
        public bool ModifyBuildEvent;
        public bool CreateKeyGenTable;
        public bool DisableIdentityColumns;
        public bool SeparateStubs;
    }
}
