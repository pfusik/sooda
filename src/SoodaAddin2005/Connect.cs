using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Windows.Forms;
using SoodaAddIn;



namespace SoodaAddin2005
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
            NLog.Config.SimpleConfigurator.ConfigureForFileLogging("c:\\log.2005.txt", NLog.LogLevel.Debug);
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

            object[] contextGUIDS = new object[] { };
            Commands commands = _applicationObject.Commands;
            CommandBars commandBars = (CommandBars)_applicationObject.CommandBars;

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
                    command = commands.AddNamedCommand(_addInInstance,
                        "ConfigureSooda",
                        "Configure Sooda...",
                        "Add/Configure Sooda support for the current project",
                        true, 0,
                        ref contextGUIDS,
                        (int)vsCommandStatus.vsCommandStatusSupported +
                        (int)vsCommandStatus.vsCommandStatusEnabled);
                }

                command.AddControl(commandBar, 3);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		private DTE2 _applicationObject;
		private AddIn _addInInstance;

        public void Exec(string CmdName, EnvDTE.vsCommandExecOption executeOption, ref object VariantIn, ref object VariantOut, ref bool handled)
        {
            if (CmdName == this.GetType().FullName + ".ConfigureSooda")
            {
                try
                {
                    using (AddSoodaSupportForm form = new AddSoodaSupportForm())
                    {
                        form.Project = GetActiveProject();
                        if (form.Project != null)
                        {
                            form.ShowDialog(Form.ActiveForm);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                handled = true;
            }
        }

        private Project GetActiveProject()
        {
            if (_applicationObject.Solution == null)
                return null;

            if (_applicationObject.Solution.FullName == "")
                return null;

            Array projects = (Array)_applicationObject.ActiveSolutionProjects;
            if (projects.Length != 1)
                return null;

            Project activeProject = projects.GetValue(0) as Project;
            if (activeProject == null)
                return null;

            return activeProject;
        }

        public void QueryStatus(string CmdName, EnvDTE.vsCommandStatusTextWanted NeededText, ref EnvDTE.vsCommandStatus status, ref object CommandText)
        {
            if (CmdName == this.GetType().FullName + ".ConfigureSooda")
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
                        status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    else
                        status = vsCommandStatus.vsCommandStatusUnsupported | vsCommandStatus.vsCommandStatusInvisible;
                }
            }
        }

        const string csharpProjectGuid = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
        const string vbProjectGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
    }
}