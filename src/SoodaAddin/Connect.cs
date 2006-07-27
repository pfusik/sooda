using System;
using Microsoft.Office.Core;
using Extensibility;
using System.Runtime.InteropServices;
using EnvDTE;
using System.Windows.Forms;

using NLog;

namespace SoodaAddIn
{
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("C4399862-9E3C-415D-A3C5-33E940E1E202"), ProgId("SoodaAddIn.Connect")]
	public class Connect : Object, Extensibility.IDTExtensibility2, IDTCommandTarget
	{
        private static Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		///		Implements the constructor for the Add-in object.
		///		Place your initialization code within this method.
		/// </summary>
		public Connect()
		{
            NLog.Config.SimpleConfigurator.ConfigureForFileLogging("c:\\log.txt", NLog.LogLevel.Debug);
		}

		/// <summary>
		///      Implements the OnConnection method of the IDTExtensibility2 interface.
		///      Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param term='application'>
		///      Root object of the host application.
		/// </param>
		/// <param term='connectMode'>
		///      Describes how the Add-in is being loaded.
		/// </param>
		/// <param term='addInInst'>
		///      Object representing this Add-in.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom)
		{
			applicationObject = (_DTE)application;
			addInInstance = (AddIn)addInInst;

            object []contextGUIDS = new object[] { };
            Commands commands = applicationObject.Commands;
            _CommandBars commandBars = applicationObject.CommandBars;

            try
            {
                CommandBar commandBar = (CommandBar)commandBars["Project"];
                Command command = null;

                foreach (Command c in commands)
                {
                    if (c.Name == "SoodaAddIn.Connect.ConfigureSooda")
                    {
                        c.Delete();
                    }
                }

                if (command == null)
                {
                    command = commands.AddNamedCommand(addInInstance, 
                        "ConfigureSooda", 
                        "Configure Sooda...", 
                        "Add/Configure Sooda support for the current project", 
                        true, 0, 
                        ref contextGUIDS, 
                        (int)vsCommandStatus.vsCommandStatusSupported + 
                        (int)vsCommandStatus.vsCommandStatusEnabled);

                    //applicationObject.Commands.DTE.Dele
                }

                bool found = false;

                foreach (Command c in command.Collection)
                {
                    if (c.Name == "SoodaAddIn.Connect.ConfigureSooda")
                    {
                        found = true;
                        break;
                    }
                }
                //if (!found)
                    command.AddControl(commandBar, 3);

                // commandBars["
            }
            catch(System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

		/// <summary>
		///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///     Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///      Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///      Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///      Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///      Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom)
		{
		}
		
		private _DTE applicationObject;
        private AddIn addInInstance;

        public void Exec(string CmdName, EnvDTE.vsCommandExecOption executeOption, ref object VariantIn, ref object VariantOut, ref bool handled)
        {
            if (CmdName == "SoodaAddIn.Connect.ConfigureSooda")
            {
                using (AddSoodaSupportForm form = new AddSoodaSupportForm())
                {
                    form.Project = GetActiveProject();
                    if (form.Project != null)
                    {
                        form.ShowDialog(Form.ActiveForm);
                    }
                }
                handled = true;
            }            
        }

        private Project GetActiveProject()
        {
            if (applicationObject.Solution == null)
                return null;

            if (applicationObject.Solution.FullName == "")
                return null;

            Array projects = (Array)applicationObject.ActiveSolutionProjects;
            if (projects.Length != 1)
                return null;

            Project activeProject = projects.GetValue(0) as Project;
            if (activeProject == null)
                return null;

            return activeProject;
        }

        public void QueryStatus(string CmdName, EnvDTE.vsCommandStatusTextWanted NeededText, ref EnvDTE.vsCommandStatus status, ref object CommandText)
        {
            if (CmdName == "SoodaAddIn.Connect.ConfigureSooda")
            {
                bool enabled = false;

                try
                {
                    Project p = GetActiveProject();
                    if (p == null)
                        return;
                    
                    if (p.Kind != csharpProjectGuid && p.Kind != vbProjectGuid)
                        return;

                    enabled = true;
                }
                finally
                {
                    // logger.Debug("enabled: {0}", enabled);
                    if (enabled)
                        status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    else
                        status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported;
                }
            }
        }

        const string csharpProjectGuid = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
        const string vbProjectGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    }
}