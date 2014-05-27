using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Reflection;

namespace SoodaAddin.UI
{
	/// <summary>
	/// Summary description for ConfiguringSoodaForm.
	/// </summary>
    public class ConfiguringSoodaForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ISoodaConfigurationStrategy Strategy;
        public WizardResult WizardOptions;
        private System.Windows.Forms.Button buttonClose;

        private Thread _backgroundThread;

        public ConfiguringSoodaForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(472, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please wait. Configuring Sooda...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(8, 48);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(472, 200);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "";
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonClose.Enabled = false;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonClose.Location = new System.Drawing.Point(207, 256);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Close";
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // ConfiguringSoodaForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(488, 288);
            this.ControlBox = false;
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfiguringSoodaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configuring Sooda...";
            this.Load += new System.EventHandler(this.ConfiguringSoodaForm_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private void ConfiguringSoodaForm_Load(object sender, System.EventArgs e)
        {
            _backgroundThread = new Thread(new ThreadStart(ThreadProc));
            _backgroundThread.Start();
        }

        delegate void WriteToLogDelegate(string message);

        private void WriteToLog(string message)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new WriteToLogDelegate(WriteToLog), new object[] { message });
            }
            else
            {
                textBox1.AppendText(message + "\r\n");
            }
        }

        private void SetAppConfigKey(XmlDocument doc, string key, string value)
        {
            WriteToLog("Adding configuration setting: " + key + "=" + value);
            XmlElement appSettings = (XmlElement)doc.SelectSingleNode("/configuration/appSettings");
            if (appSettings == null)
            {
                appSettings = doc.CreateElement("appSettings");
                doc.DocumentElement.AppendChild(appSettings);
            }

            XmlElement keyElement = (XmlElement)appSettings.SelectSingleNode(@"add[@key='" + key + "']");
            if (keyElement == null)
            {
                keyElement = doc.CreateElement("add");
                appSettings.AppendChild(keyElement);
                keyElement.SetAttribute("key", key);
            }
            keyElement.SetAttribute("value", value);
        }

        private void ThreadProc()
        {
            try
            {
                string baseDir = Path.GetDirectoryName(Strategy.ProjectFile);
                string actualConnectionString = WizardOptions.ConnectionString + ";Initial Catalog=" + WizardOptions.SelectedDatabase;
                NameValueCollection configOptions = new NameValueCollection();

                configOptions["default.connectionString"] = actualConnectionString;
                configOptions["default.connectionType"] = "sqlclient";
                configOptions["default.sqlDialect"] = "mssql";
                configOptions["default.commandTimeout"] = "30";
                configOptions["default.disableTransactions"] = "false";
                configOptions["default.stripWhitespaceInLogs"] = "false";
                configOptions["default.indentQueries"] = "true";
                configOptions["default.useSafeLiterals"] = "true";
                configOptions["default.disableUpdateBatch"] = "false";
                configOptions["sooda.defaultObjectsAssembly"] = Strategy.AssemblyName + (WizardOptions.SeparateStubs ? ".Stubs" : "");
                configOptions["sooda.cachingPolicy"] = "none";
                configOptions["sooda.cachingPolicy.expirationTimeout"] = "120";
                configOptions["sooda.cachingPolicy.slidingExpiration"] = "false";
                configOptions["sooda.cache.type"] = "inprocess";

                WriteToLog("Configuring project: " + Strategy.ProjectFile);
                WriteToLog("Adding reference to 'Sooda'");
                Strategy.AddReference("Sooda");
                Strategy.AddReference("System.Drawing");
                if (WizardOptions.CreateAppConfigFile)
                {
                    string appConfig = Path.Combine(baseDir, "App.config");
                    if (!File.Exists(appConfig))
                    {
                        WriteToLog("Creating App.config file: " + appConfig);
                        using (StreamWriter sw = new StreamWriter(appConfig, false, Encoding.Default))
                        {
                            sw.WriteLine("<configuration>");
                            sw.WriteLine("</configuration>");
                        }
                    }

                    WriteToLog("Adding 'App.config' to project.");
                    Strategy.AddContent("App.config");

                    XmlDocument doc = new XmlDocument();
                    doc.Load(appConfig);
                    foreach (string k in configOptions.Keys)
                    {
                        SetAppConfigKey(doc, k, configOptions[k]);
                    }
                    doc.Save(appConfig);
                }

                string soodaProjectFile = Path.GetFileNameWithoutExtension(Strategy.ProjectFile) + ".soodaproject";
                string fullSoodaProjectFile = Path.Combine(baseDir, soodaProjectFile);

                WriteToLog("Creating Sooda Project file: " + fullSoodaProjectFile);
                using (StreamWriter sw = new StreamWriter(fullSoodaProjectFile, false, Encoding.Default))
                {
                    sw.WriteLine("<sooda-project xmlns=\"http://www.sooda.org/schemas/SoodaProject.xsd\">");
                    sw.WriteLine("  <schema-file>SoodaSchema.xml</schema-file>");
                    sw.WriteLine("  <language>c#</language>");
                    sw.WriteLine("  <output-namespace>{0}</output-namespace>", Strategy.DefaultNamespace);
                    sw.WriteLine("  <output-path>.</output-path>");
                    sw.WriteLine("  <nullable-representation>Nullable</nullable-representation>");
                    sw.WriteLine("  <not-null-representation>Raw</not-null-representation>");
                    sw.WriteLine("  <with-indexers>true</with-indexers>");
                    sw.WriteLine("  <with-typed-queries>true</with-typed-queries>");
                    sw.WriteLine("  <embedded-schema-type>Binary</embedded-schema-type>");
                    if (WizardOptions.SeparateStubs)
                    {
                        sw.WriteLine("  <stubs-compiled-separately>true</stubs-compiled-separately>");
                    }
                    sw.WriteLine("  <external-projects>");
                    sw.WriteLine("    <project type=\"{0}\" file=\"{1}\"/>", Strategy.ProjectType, Path.GetFileName(Strategy.ProjectFile));
                    sw.WriteLine("  </external-projects>");
                    sw.WriteLine("</sooda-project>");
                }
                Strategy.AddContent(soodaProjectFile);

                if (!WizardOptions.SeparateStubs)
                {
                    string stubFileExtension = Strategy.FileExtension;

                    if (!File.Exists(Path.Combine(baseDir, "_Stubs" + stubFileExtension)))
                    {
                        using (StreamWriter sw = new StreamWriter(Path.Combine(baseDir, "_Stubs" + stubFileExtension), false))
                        {
                            sw.WriteLine("// This file will be overwritten each time the project is built.");
                        }
                    }
                    if (!File.Exists(Path.Combine(baseDir, "_DBSchema.bin")))
                    {
                        using (StreamWriter sw = new StreamWriter(Path.Combine(baseDir, "_DBSchema.bin"), false))
                        {
                            sw.WriteLine("This file will be overwritten each time the project is built.");
                        }
                    }

                    Strategy.AddCompileUnit("_Stubs" + stubFileExtension);
                    Strategy.AddEmbeddedResource("_DBSchema.bin");
                }

                string soodaSchemaFile = "SoodaSchema.xml";
                string fullSoodaSchemaFile = Path.Combine(baseDir, soodaSchemaFile);

                WriteToLog("Creating Sooda Mapping Schema file: " + fullSoodaSchemaFile);
                using (StreamWriter sw = new StreamWriter(fullSoodaSchemaFile, false, Encoding.Default))
                {
                    sw.WriteLine("<schema xmlns=\"http://www.sooda.org/schemas/SoodaSchema.xsd\">");
                    sw.WriteLine("    <datasource name=\"default\" type=\"Sooda.Sql.SqlDataSource\" />");
                    sw.WriteLine("    <!-- TODO - Add classes and relations here -->");
                    sw.WriteLine("</schema>");
                }
                Strategy.AddContent(soodaSchemaFile);

                if (WizardOptions.ReverseEngineerDatabase)
                {
                    using (Process p = new Process())
                    {
                        string dir = Path.GetDirectoryName(typeof(ConfiguringSoodaForm).Assembly.Location);
                        WriteToLog(dir);

                        p.StartInfo.FileName = Path.Combine(dir, "SoodaSchemaTool.exe");
                        p.StartInfo.Arguments = "genschema -connectionString \"" + actualConnectionString + "\" -outputFile \"" + fullSoodaSchemaFile + "\"";
                        p.StartInfo.UseShellExecute = false;
                        WriteToLog("Spawning " + p.StartInfo.FileName + " Arguments: " + p.StartInfo.Arguments);
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                        while (!p.HasExited)
                        {
                            WriteToLog(p.StandardOutput.ReadLine());
                        }

                        p.WaitForExit();
                    }
                }

                if (WizardOptions.ModifyBuildEvent)
                {
                    string[] lines = Strategy.PreBuildEvent.Replace("\r","").Split('\n');
                    List<string> newLines = new List<string>();

                    newLines.Add("\"%SOODA_DIR%\\bin\\net-2.0\\SoodaStubGen.exe\" \"$(ProjectDir)" + soodaProjectFile + "\"");
                    foreach (string line in lines)
                    {
                        if (line.IndexOf("Sooda") < 0)
                        {
                            newLines.Add(line);
                        }
                    }

                    if (WizardOptions.SeparateStubs)
                    {
                        string extraFiles = "";

                        if (File.Exists(Path.Combine(baseDir, "Properties\\AssemblyInfo.cs")))
                        {
                            extraFiles += " \"$(ProjectDir)Properties\\AssemblyInfo.cs\"";
                        }
                        if (File.Exists(Path.Combine(baseDir, "AssemblyInfo.cs")))
                        {
                            extraFiles += " \"$(ProjectDir)AssemblyInfo.cs\"";
                        }

                        newLines.Add("\"%SOODA_DIR%\\bin\\net-2.0\\SoodaCompileStubs.exe\" \"" + Strategy.AssemblyName + "\" \"$(ProjectDir)Stubs\"" + extraFiles);
                    }

                    string newPreBuildEvent = String.Join("\r\n", newLines.ToArray());
                    WriteToLog("Setting Pre-Build Event to: " + newPreBuildEvent);
                    Strategy.PreBuildEvent = newPreBuildEvent;
                }

                if (WizardOptions.SeparateStubs)
                {
                    Strategy.AddReferenceWithHintPath(Strategy.AssemblyName + ".Stubs", "Stubs\\" + Strategy.AssemblyName + ".Stubs.dll");
                }
                WriteToLog("Saving project...");
                Strategy.SaveProject();
                WriteToLog("Building Sooda Project for the first time...");
                using (Process p = new Process())
                {
                    string dir = Path.GetDirectoryName(typeof(ConfiguringSoodaForm).Assembly.Location);
                    WriteToLog(dir);

                    p.StartInfo.FileName = Path.Combine(dir, "SoodaStubGen.exe");
                    p.StartInfo.Arguments = "\"" + fullSoodaProjectFile + "\"";
                    p.StartInfo.UseShellExecute = false;
                    WriteToLog("Spawning " + p.StartInfo.FileName + " Arguments: " + p.StartInfo.Arguments);
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    while (!p.HasExited)
                    {
                        WriteToLog(p.StandardOutput.ReadLine());
                    }

                    p.WaitForExit();
                }


                WriteToLog("Finished");
                //DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                WriteToLog("ERROR: " + ex.ToString());
            }
            finally
            {
                buttonClose.Enabled = true;
            }
        }

        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
