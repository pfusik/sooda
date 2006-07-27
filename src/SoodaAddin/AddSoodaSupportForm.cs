using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Sooda.CodeGen;

namespace SoodaAddIn
{
	/// <summary>
	/// Summary description for AddSoodaSupportForm.
	/// </summary>
	public class AddSoodaSupportForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxDatabaseType;
        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxSqlDialect;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxNullableProperties;
        private System.Windows.Forms.ComboBox comboBoxNotNullProperties;
        private System.Windows.Forms.CheckBox checkBoxTypedQueries;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBoxLoggingEngine;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageDatabase;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.TabPage tabPageCodeGen;
        private System.Windows.Forms.CheckBox checkBoxSeparateCompilation;
        private System.Windows.Forms.Button buttonTestConnection;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxSoodaBinaries;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonGenerateDefaultSchema;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AddSoodaSupportForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

        private EnvDTE.Project _project;
        private SoodaProject _soodaProject;
        private string _connectionType;
        private string _connectionString;
        private string _sqlDialect;
        private bool _disableTransactions;
        private bool _stripWhitespaceInLogs;
        private bool _indentQueries;
        private bool _useSafeLiterals;
        private bool _disableUpdateBatch;
        private string _soodaBinariesPath;

        public EnvDTE.Project Project
        {
            get { return _project; }
            set { _project = value; }
        }

        public string SoodaProjectFile
        {
            get { return Path.Combine(Path.GetDirectoryName(Project.FileName), Project.Name + ".soodaproject"); }
        }

        public string SoodaSchemaFile
        {
            get { return Path.Combine(Path.GetDirectoryName(Project.FileName), "SoodaSchema.xml"); }
        }

        public string AppConfigFile
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Project.FileName), "App.config");
            }
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
            this.comboBoxDatabaseType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.comboBoxSqlDialect = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxNullableProperties = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxNotNullProperties = new System.Windows.Forms.ComboBox();
            this.checkBoxTypedQueries = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBoxLoggingEngine = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageDatabase = new System.Windows.Forms.TabPage();
            this.buttonGenerateDefaultSchema = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.buttonTestConnection = new System.Windows.Forms.Button();
            this.tabPageCodeGen = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSoodaBinaries = new System.Windows.Forms.TextBox();
            this.checkBoxSeparateCompilation = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl1.SuspendLayout();
            this.tabPageDatabase.SuspendLayout();
            this.tabPageCodeGen.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxDatabaseType
            // 
            this.comboBoxDatabaseType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDatabaseType.Location = new System.Drawing.Point(16, 40);
            this.comboBoxDatabaseType.Name = "comboBoxDatabaseType";
            this.comboBoxDatabaseType.Size = new System.Drawing.Size(584, 21);
            this.comboBoxDatabaseType.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(584, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection Type (enter fully qualified type name, including assembly)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(16, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(368, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database Connection String";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConnectionString.Location = new System.Drawing.Point(16, 88);
            this.textBoxConnectionString.Multiline = true;
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(456, 56);
            this.textBoxConnectionString.TabIndex = 2;
            this.textBoxConnectionString.Text = "";
            // 
            // comboBoxSqlDialect
            // 
            this.comboBoxSqlDialect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSqlDialect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSqlDialect.Location = new System.Drawing.Point(16, 168);
            this.comboBoxSqlDialect.Name = "comboBoxSqlDialect";
            this.comboBoxSqlDialect.Size = new System.Drawing.Size(184, 21);
            this.comboBoxSqlDialect.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(16, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(184, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "SQL Dialect";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(480, 464);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(560, 464);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Location = new System.Drawing.Point(8, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(288, 24);
            this.label6.TabIndex = 3;
            this.label6.Text = "Nullable Representation:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // comboBoxNullableProperties
            // 
            this.comboBoxNullableProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxNullableProperties.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNullableProperties.Location = new System.Drawing.Point(8, 80);
            this.comboBoxNullableProperties.Name = "comboBoxNullableProperties";
            this.comboBoxNullableProperties.Size = new System.Drawing.Size(288, 20);
            this.comboBoxNullableProperties.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(320, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(288, 24);
            this.label7.TabIndex = 3;
            this.label7.Text = "Not-null Representation:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // comboBoxNotNullProperties
            // 
            this.comboBoxNotNullProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxNotNullProperties.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNotNullProperties.Location = new System.Drawing.Point(320, 80);
            this.comboBoxNotNullProperties.Name = "comboBoxNotNullProperties";
            this.comboBoxNotNullProperties.Size = new System.Drawing.Size(288, 20);
            this.comboBoxNotNullProperties.TabIndex = 0;
            // 
            // checkBoxTypedQueries
            // 
            this.checkBoxTypedQueries.Location = new System.Drawing.Point(8, 112);
            this.checkBoxTypedQueries.Name = "checkBoxTypedQueries";
            this.checkBoxTypedQueries.Size = new System.Drawing.Size(600, 24);
            this.checkBoxTypedQueries.TabIndex = 8;
            this.checkBoxTypedQueries.Text = "Enable Typed Queries";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.Location = new System.Drawing.Point(16, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(568, 24);
            this.label8.TabIndex = 3;
            this.label8.Text = "Logging Engine";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // comboBoxLoggingEngine
            // 
            this.comboBoxLoggingEngine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLoggingEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLoggingEngine.Location = new System.Drawing.Point(16, 40);
            this.comboBoxLoggingEngine.Name = "comboBoxLoggingEngine";
            this.comboBoxLoggingEngine.Size = new System.Drawing.Size(584, 20);
            this.comboBoxLoggingEngine.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageDatabase);
            this.tabControl1.Controls.Add(this.tabPageCodeGen);
            this.tabControl1.Controls.Add(this.tabPageAdvanced);
            this.tabControl1.Location = new System.Drawing.Point(8, 88);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(624, 368);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPageDatabase
            // 
            this.tabPageDatabase.Controls.Add(this.buttonGenerateDefaultSchema);
            this.tabPageDatabase.Controls.Add(this.checkBox1);
            this.tabPageDatabase.Controls.Add(this.buttonTestConnection);
            this.tabPageDatabase.Controls.Add(this.textBoxConnectionString);
            this.tabPageDatabase.Controls.Add(this.comboBoxDatabaseType);
            this.tabPageDatabase.Controls.Add(this.label4);
            this.tabPageDatabase.Controls.Add(this.comboBoxSqlDialect);
            this.tabPageDatabase.Controls.Add(this.label2);
            this.tabPageDatabase.Controls.Add(this.label1);
            this.tabPageDatabase.Location = new System.Drawing.Point(4, 22);
            this.tabPageDatabase.Name = "tabPageDatabase";
            this.tabPageDatabase.Size = new System.Drawing.Size(616, 342);
            this.tabPageDatabase.TabIndex = 0;
            this.tabPageDatabase.Text = "s";
            // 
            // buttonGenerateDefaultSchema
            // 
            this.buttonGenerateDefaultSchema.Location = new System.Drawing.Point(480, 176);
            this.buttonGenerateDefaultSchema.Name = "buttonGenerateDefaultSchema";
            this.buttonGenerateDefaultSchema.Size = new System.Drawing.Size(120, 48);
            this.buttonGenerateDefaultSchema.TabIndex = 14;
            this.buttonGenerateDefaultSchema.Text = "Generate Default Schema";
            this.buttonGenerateDefaultSchema.Click += new System.EventHandler(this.buttonGenerateDefaultSchema_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(16, 200);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(280, 24);
            this.checkBox1.TabIndex = 13;
            this.checkBox1.Text = "Do not use transactions (NOT RECOMMENDED)";
            // 
            // buttonTestConnection
            // 
            this.buttonTestConnection.Location = new System.Drawing.Point(480, 88);
            this.buttonTestConnection.Name = "buttonTestConnection";
            this.buttonTestConnection.Size = new System.Drawing.Size(120, 56);
            this.buttonTestConnection.TabIndex = 4;
            this.buttonTestConnection.Text = "Test Connection";
            this.buttonTestConnection.Click += new System.EventHandler(this.buttonTestConnection_Click);
            // 
            // tabPageCodeGen
            // 
            this.tabPageCodeGen.Controls.Add(this.textBox1);
            this.tabPageCodeGen.Controls.Add(this.checkBoxTypedQueries);
            this.tabPageCodeGen.Controls.Add(this.label6);
            this.tabPageCodeGen.Controls.Add(this.comboBoxNullableProperties);
            this.tabPageCodeGen.Controls.Add(this.label7);
            this.tabPageCodeGen.Controls.Add(this.comboBoxNotNullProperties);
            this.tabPageCodeGen.Controls.Add(this.label9);
            this.tabPageCodeGen.Location = new System.Drawing.Point(4, 22);
            this.tabPageCodeGen.Name = "tabPageCodeGen";
            this.tabPageCodeGen.Size = new System.Drawing.Size(616, 342);
            this.tabPageCodeGen.TabIndex = 2;
            this.tabPageCodeGen.Text = "Code Generation";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(8, 32);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(600, 20);
            this.textBox1.TabIndex = 9;
            this.textBox1.Text = "";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.Location = new System.Drawing.Point(8, 8);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(288, 24);
            this.label9.TabIndex = 3;
            this.label9.Text = "Output Namespace";
            this.label9.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add(this.label5);
            this.tabPageAdvanced.Controls.Add(this.textBoxSoodaBinaries);
            this.tabPageAdvanced.Controls.Add(this.checkBoxSeparateCompilation);
            this.tabPageAdvanced.Controls.Add(this.comboBoxLoggingEngine);
            this.tabPageAdvanced.Controls.Add(this.label8);
            this.tabPageAdvanced.Controls.Add(this.checkBox2);
            this.tabPageAdvanced.Controls.Add(this.checkBox3);
            this.tabPageAdvanced.Controls.Add(this.checkBox4);
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Size = new System.Drawing.Size(616, 342);
            this.tabPageAdvanced.TabIndex = 1;
            this.tabPageAdvanced.Text = "Advanced Settings";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(16, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(584, 24);
            this.label5.TabIndex = 10;
            this.label5.Text = "Path to Sooda binaries (SoodaStubGen.exe, SoodaCompileStubs.exe)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // textBoxSoodaBinaries
            // 
            this.textBoxSoodaBinaries.Location = new System.Drawing.Point(16, 96);
            this.textBoxSoodaBinaries.Name = "textBoxSoodaBinaries";
            this.textBoxSoodaBinaries.Size = new System.Drawing.Size(584, 20);
            this.textBoxSoodaBinaries.TabIndex = 11;
            this.textBoxSoodaBinaries.Text = "";
            // 
            // checkBoxSeparateCompilation
            // 
            this.checkBoxSeparateCompilation.Location = new System.Drawing.Point(16, 128);
            this.checkBoxSeparateCompilation.Name = "checkBoxSeparateCompilation";
            this.checkBoxSeparateCompilation.Size = new System.Drawing.Size(584, 24);
            this.checkBoxSeparateCompilation.TabIndex = 9;
            this.checkBoxSeparateCompilation.Text = "Compile stubs to a DLL (may speed up large projects but introduces circular assem" +
                "bly reference)";
            // 
            // checkBox2
            // 
            this.checkBox2.Location = new System.Drawing.Point(16, 152);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(584, 24);
            this.checkBox2.TabIndex = 9;
            this.checkBox2.Text = "Do not use parameters for safe literals (numbers and simple strings)";
            // 
            // checkBox3
            // 
            this.checkBox3.Location = new System.Drawing.Point(16, 176);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(584, 24);
            this.checkBox3.TabIndex = 9;
            this.checkBox3.Text = "Indent queries in log file";
            // 
            // checkBox4
            // 
            this.checkBox4.Location = new System.Drawing.Point(16, 200);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(584, 24);
            this.checkBox4.TabIndex = 9;
            this.checkBox4.Text = "Disable batch updates";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(640, 88);
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // AddSoodaSupportForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(640, 496);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddSoodaSupportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Sooda Support to Project";
            this.Load += new System.EventHandler(this.AddSoodaSupportForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageDatabase.ResumeLayout(false);
            this.tabPageCodeGen.ResumeLayout(false);
            this.tabPageAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

        private void LoadConfiguration()
        {
            if (File.Exists(SoodaProjectFile))
            {
                XmlSerializer ser = new XmlSerializer(typeof(SoodaProject));
                using (FileStream fs = File.OpenRead(SoodaProjectFile))
                {
                    _soodaProject = (SoodaProject)ser.Deserialize(fs);
                }
            }
            else
            {
                _soodaProject = new SoodaProject();
            }

            _soodaProject.AssemblyName = Convert.ToString(Project.Properties.Item("AssemblyName").Value);
            _soodaProject.SchemaFile = "SoodaSchema.xml";
            _soodaProject.OutputNamespace = _soodaProject.AssemblyName;

            string preBuildEvent = Convert.ToString(Project.Properties.Item("PreBuildEvent").Value);
            Regex r = new Regex("\"(.*?)SoodaStubGen.exe\"", RegexOptions.IgnoreCase);
            Match m = r.Match(preBuildEvent);
            if (m.Success)
            {
                _soodaBinariesPath = m.Groups[1].Value;
            }
            else
            {
                _soodaBinariesPath = @"%SOODA_DIR%\bin\net-1.1\";
            }

            // load runtime configuration
            _connectionType = "System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            _connectionString = "server=.;database=;user id=;password=";
            _sqlDialect = "mssql";

            string baseDir = Path.GetDirectoryName(Project.FullName);
            if (File.Exists(AppConfigFile))
            {
                XmlDocument doc = new XmlDocument();
                XmlElement e = (XmlElement)doc.SelectSingleNode("/configuration/appSettings/default.connectionType");
                if (e != null)
                    _connectionType = e.GetAttribute("value");
                e = (XmlElement)doc.SelectSingleNode("/configuration/appSettings/default.connectionString");
                if (e != null)
                    _connectionString = e.GetAttribute("value");
                e = (XmlElement)doc.SelectSingleNode("/configuration/appSettings/default.sqlDialect");
                if (e != null)
                    _sqlDialect = e.GetAttribute("value");
            }
        }

        private void AddSoodaSupportForm_Load(object sender, System.EventArgs e)
        {
            LoadConfiguration();

            foreach (string s in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (s.EndsWith("sooda.jpg"))
                {
                    pictureBox1.Image = Image.FromStream(
                        Assembly.GetExecutingAssembly().GetManifestResourceStream(s));
                }
            }
            comboBoxDatabaseType.Items.Add("System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            //comboBoxDatabaseType.Items.Add("System.Data.Odbc.OdbcConnection, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            comboBoxDatabaseType.Items.Add("System.Data.OleDb.OdbcConnection, System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            comboBoxDatabaseType.Items.Add("System.Data.OracleClient.OracleConnection, System.Data.OracleClient, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            comboBoxDatabaseType.Text = _connectionType;

            textBoxConnectionString.Text = _connectionString;

            comboBoxSqlDialect.Items.Add("mssql");
            comboBoxSqlDialect.Items.Add("oracle");
            comboBoxSqlDialect.Items.Add("postgresql");
            comboBoxSqlDialect.Items.Add("mysql");
            comboBoxSqlDialect.SelectedValue = _sqlDialect;

            Text = "Add Sooda Support to Project - " + Project.Name;

            textBoxSoodaBinaries.Text = _soodaBinariesPath;
            FillNullable(comboBoxNullableProperties);
            FillNullable(comboBoxNotNullProperties);

            comboBoxNullableProperties.SelectedIndex = 1;
            comboBoxNotNullProperties.SelectedIndex = 2;

            checkBoxTypedQueries.Checked = true;
        }

        private void FillNullable(ComboBox cb)
        {
            cb.Items.Add("Boxing (Object)");
            cb.Items.Add("SqlTypes (SqlInt32, SqlString, SqlDateTime...)");
            cb.Items.Add("Raw (int, string, DateTime, no access to nullable)");
            cb.Items.Add("Raw + _IsNull property");
            cb.Items.Add(".NET 2.0 Nullable<> (int?, string?, DateTime?)");
        }

        private PrimitiveRepresentation NullableSelectionToPrimitiveRepresentation(ComboBox cb)
        {
            switch (cb.SelectedIndex)
            {
                case 0:
                    return PrimitiveRepresentation.Boxed;

                default:
                case 1:
                    return PrimitiveRepresentation.SqlType;

                case 2:
                    return PrimitiveRepresentation.Raw;

                case 3:
                    return PrimitiveRepresentation.RawWithIsNull;

                case 4:
                    return PrimitiveRepresentation.Nullable;
            }
        }

        private int PrimitiveRepresentationToIndex(PrimitiveRepresentation pr)
        {
            switch (pr)
            {
                case PrimitiveRepresentation.Boxed:
                    return 0;

                default:
                case PrimitiveRepresentation.SqlType:
                    return 1;

                case PrimitiveRepresentation.Raw:
                    return 2;

                case PrimitiveRepresentation.RawWithIsNull:
                    return 3;

                case PrimitiveRepresentation.Nullable:
                    return 4;
            }
        }

        private void SetAppSetting(XmlElement el, string key, string value, string defaultValue)
        {
            XmlElement add = (XmlElement)el.SelectSingleNode("add[@key='" + key + "']");
            if (add == null)
            {
                if (value == defaultValue)
                    return;

                add = el.OwnerDocument.CreateElement("add");
                add.SetAttribute("key", key);
                el.AppendChild(add);
            }
            add.SetAttribute("value", value);
        }

        private void SaveConfiguration()
        {
            XmlSerializer ser = new XmlSerializer(typeof(SoodaProject));
            using (FileStream fs = File.Create(SoodaProjectFile))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", SoodaProject.NamespaceURI);
                ser.Serialize(fs, _soodaProject, ns);
            }

            XmlDocument doc = new XmlDocument();
            if (File.Exists(AppConfigFile))
                doc.Load(AppConfigFile);
            else
            {
                doc.LoadXml("<configuration />");
            }
            XmlElement appSettings = (XmlElement)doc.SelectSingleNode("/configuration/appSettings");
            if (appSettings == null)
            {
                appSettings = doc.CreateElement("appSettings");
                doc.DocumentElement.InsertAfter(appSettings, null);
            }

            SetAppSetting(appSettings, "default.connectionString", _connectionString, null);
            SetAppSetting(appSettings, "default.connectionType", _connectionType, null);
            SetAppSetting(appSettings, "default.sqlDialect", _sqlDialect, null);
            SetAppSetting(appSettings, "default.disableTransactions", _disableTransactions ? "true" : "false", "false");
            SetAppSetting(appSettings, "default.stripWhitespaceInLogs", _stripWhitespaceInLogs ? "true" : "false", "false");
            SetAppSetting(appSettings, "default.indentQueries", _indentQueries ? "true" : "false", "false");
            SetAppSetting(appSettings, "default.useSafeLiterals", _useSafeLiterals ? "true" : "false", "false");
            SetAppSetting(appSettings, "default.disableUpdateBatch", _disableUpdateBatch ? "true" : "false", "false");

            doc.Save(AppConfigFile);
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            string commandLine = "";

            _soodaProject.SeparateStubs = checkBoxSeparateCompilation.Checked;
            if (Project.Kind == "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}")
                _soodaProject.Language = "c#";
            else
                _soodaProject.Language = "vb";

            _soodaProject.NullableRepresentation = NullableSelectionToPrimitiveRepresentation(comboBoxNullableProperties);
            _soodaProject.NotNullRepresentation = NullableSelectionToPrimitiveRepresentation(comboBoxNotNullProperties);
            _soodaProject.WithTypedQueryWrappers = true;

            SaveConfiguration();

            if (!File.Exists(SoodaSchemaFile))
            {
                buttonGenerateDefaultSchema_Click(this, null);
            }

            try
            {
                Project.ProjectItems.AddFromFile(SoodaProjectFile);
            }
            catch
            {
            }

            try
            {
                Project.ProjectItems.AddFromFile(AppConfigFile);
            }
            catch
            {
            }

            commandLine += "\"" + textBoxSoodaBinaries.Text + "\\SoodaStubGen.exe\" \"$(ProjectName).soodaproject\"\n";
            if (checkBoxSeparateCompilation.Checked)
            {
            }
            Project.Properties.Item("PreBuildEvent").Value = commandLine;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonTestConnection_Click(object sender, System.EventArgs e)
        {
            try
            {
                Type t = Type.GetType(comboBoxDatabaseType.Text);
                using (IDbConnection conn = (IDbConnection)Activator.CreateInstance(t))
                {
                    conn.ConnectionString = textBoxConnectionString.Text;
                    conn.Open();
                }
                MessageBox.Show(this, "Connection succeeded.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.ToString());
            }
        }

        private void buttonGenerateDefaultSchema_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.Yes ==  MessageBox.Show(this, "Would you like to scan the database and create default (approximate) mapping schema?\n\nThis will (re)create SoodaSchema.xml file. You can use SoodaSchemaTool.exe to do it edit the file manually.",
                "Question", MessageBoxButtons.YesNo))
            {
                string commandLine = "SoodaSchemaTool.exe";

                using (Process p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo("SoodaSchemaTool.exe", "genschema -connectionstring \"" + _connectionString + "\" -outputfile \"" + SoodaSchemaFile + "\"");
                    p.Start();
                    p.WaitForExit();
                }
            }
        }
	}
}
