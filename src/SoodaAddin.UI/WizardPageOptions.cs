using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace SoodaAddin.UI
{
	/// <summary>
	/// Summary description for WizardPageOptions.
	/// </summary>
	public class WizardPageOptions : SoodaAddin.UI.WizardPage
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelAdvancedOptions;
        public System.Windows.Forms.ComboBox comboBoxConfigStyle;
        public System.Windows.Forms.CheckBox checkBoxCreateConfig;
        public System.Windows.Forms.CheckBox checkBoxModifyBuildEvent;
        public System.Windows.Forms.CheckBox checkBoxSeparateStubs;
        public System.Windows.Forms.CheckBox checkBoxAddAttributesToAssemblyInfo;
        public System.Windows.Forms.CheckBox checkBoxCreateKeyGen;
        public System.Windows.Forms.CheckBox checkBoxDisableIdentityColumns;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.CheckBox checkBoxGenerateSchema;
        private System.Windows.Forms.Button buttonAdvancedOptions;
        public System.Windows.Forms.CheckBox checkBoxGenerateStubs;
        private System.Windows.Forms.LinkLabel linkLabelCreateKeyGen;
        private System.Windows.Forms.LinkLabel linkLabelDisableIdentityColumns;
        private System.Windows.Forms.LinkLabel linkLabelSeparateStubs;
        private System.Windows.Forms.LinkLabel linkLabelModifyBuildEvent;
        private System.Windows.Forms.LinkLabel linkLabelCreateConfig;
        private System.Windows.Forms.LinkLabel linkLabelAddAttributes;
        private System.Windows.Forms.LinkLabel linkLabelGenerateSchema;
        private System.Windows.Forms.LinkLabel linkLabelGenerateStubs;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WizardPageOptions()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxGenerateStubs = new System.Windows.Forms.CheckBox();
            this.panelAdvancedOptions = new System.Windows.Forms.Panel();
            this.linkLabelCreateKeyGen = new System.Windows.Forms.LinkLabel();
            this.comboBoxConfigStyle = new System.Windows.Forms.ComboBox();
            this.checkBoxCreateConfig = new System.Windows.Forms.CheckBox();
            this.checkBoxModifyBuildEvent = new System.Windows.Forms.CheckBox();
            this.checkBoxSeparateStubs = new System.Windows.Forms.CheckBox();
            this.checkBoxAddAttributesToAssemblyInfo = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateKeyGen = new System.Windows.Forms.CheckBox();
            this.checkBoxDisableIdentityColumns = new System.Windows.Forms.CheckBox();
            this.linkLabelDisableIdentityColumns = new System.Windows.Forms.LinkLabel();
            this.linkLabelSeparateStubs = new System.Windows.Forms.LinkLabel();
            this.linkLabelModifyBuildEvent = new System.Windows.Forms.LinkLabel();
            this.linkLabelCreateConfig = new System.Windows.Forms.LinkLabel();
            this.linkLabelAddAttributes = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxGenerateSchema = new System.Windows.Forms.CheckBox();
            this.linkLabelGenerateSchema = new System.Windows.Forms.LinkLabel();
            this.buttonAdvancedOptions = new System.Windows.Forms.Button();
            this.linkLabelGenerateStubs = new System.Windows.Forms.LinkLabel();
            this.panelAdvancedOptions.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(520, 24);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select options:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(8, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(520, 16);
            this.label2.TabIndex = 9;
            this.label2.Text = "Choose the actions you would like the wizard to perform and click Finish.";
            // 
            // checkBoxGenerateStubs
            // 
            this.checkBoxGenerateStubs.Location = new System.Drawing.Point(8, 80);
            this.checkBoxGenerateStubs.Name = "checkBoxGenerateStubs";
            this.checkBoxGenerateStubs.Size = new System.Drawing.Size(432, 24);
            this.checkBoxGenerateStubs.TabIndex = 6;
            this.checkBoxGenerateStubs.Text = "Generate initial stubs and skeleton classes";
            // 
            // panelAdvancedOptions
            // 
            this.panelAdvancedOptions.AutoScroll = true;
            this.panelAdvancedOptions.AutoScrollMargin = new System.Drawing.Size(8, 8);
            this.panelAdvancedOptions.BackColor = System.Drawing.SystemColors.Control;
            this.panelAdvancedOptions.Controls.Add(this.linkLabelCreateKeyGen);
            this.panelAdvancedOptions.Controls.Add(this.comboBoxConfigStyle);
            this.panelAdvancedOptions.Controls.Add(this.checkBoxCreateConfig);
            this.panelAdvancedOptions.Controls.Add(this.checkBoxModifyBuildEvent);
            this.panelAdvancedOptions.Controls.Add(this.checkBoxSeparateStubs);
            this.panelAdvancedOptions.Controls.Add(this.checkBoxAddAttributesToAssemblyInfo);
            this.panelAdvancedOptions.Controls.Add(this.checkBoxCreateKeyGen);
            this.panelAdvancedOptions.Controls.Add(this.checkBoxDisableIdentityColumns);
            this.panelAdvancedOptions.Controls.Add(this.linkLabelDisableIdentityColumns);
            this.panelAdvancedOptions.Controls.Add(this.linkLabelSeparateStubs);
            this.panelAdvancedOptions.Controls.Add(this.linkLabelModifyBuildEvent);
            this.panelAdvancedOptions.Controls.Add(this.linkLabelCreateConfig);
            this.panelAdvancedOptions.Controls.Add(this.linkLabelAddAttributes);
            this.panelAdvancedOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAdvancedOptions.Location = new System.Drawing.Point(3, 18);
            this.panelAdvancedOptions.Name = "panelAdvancedOptions";
            this.panelAdvancedOptions.Size = new System.Drawing.Size(514, 203);
            this.panelAdvancedOptions.TabIndex = 10;
            // 
            // linkLabelCreateKeyGen
            // 
            this.linkLabelCreateKeyGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelCreateKeyGen.Location = new System.Drawing.Point(448, 80);
            this.linkLabelCreateKeyGen.Name = "linkLabelCreateKeyGen";
            this.linkLabelCreateKeyGen.Size = new System.Drawing.Size(56, 23);
            this.linkLabelCreateKeyGen.TabIndex = 24;
            this.linkLabelCreateKeyGen.TabStop = true;
            this.linkLabelCreateKeyGen.Text = "Explain";
            this.linkLabelCreateKeyGen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelCreateKeyGen.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCreateKeyGen_LinkClicked);
            // 
            // comboBoxConfigStyle
            // 
            this.comboBoxConfigStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxConfigStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConfigStyle.Items.AddRange(new object[] {
                                                                     "App.config - private",
                                                                     "Sooda.config.xml - shared"});
            this.comboBoxConfigStyle.Location = new System.Drawing.Point(224, 8);
            this.comboBoxConfigStyle.Name = "comboBoxConfigStyle";
            this.comboBoxConfigStyle.Size = new System.Drawing.Size(216, 22);
            this.comboBoxConfigStyle.TabIndex = 18;
            // 
            // checkBoxCreateConfig
            // 
            this.checkBoxCreateConfig.Checked = true;
            this.checkBoxCreateConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateConfig.Location = new System.Drawing.Point(8, 8);
            this.checkBoxCreateConfig.Name = "checkBoxCreateConfig";
            this.checkBoxCreateConfig.Size = new System.Drawing.Size(208, 24);
            this.checkBoxCreateConfig.TabIndex = 17;
            this.checkBoxCreateConfig.Text = "Create configuration file:";
            // 
            // checkBoxModifyBuildEvent
            // 
            this.checkBoxModifyBuildEvent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxModifyBuildEvent.Checked = true;
            this.checkBoxModifyBuildEvent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxModifyBuildEvent.Location = new System.Drawing.Point(8, 56);
            this.checkBoxModifyBuildEvent.Name = "checkBoxModifyBuildEvent";
            this.checkBoxModifyBuildEvent.Size = new System.Drawing.Size(432, 24);
            this.checkBoxModifyBuildEvent.TabIndex = 15;
            this.checkBoxModifyBuildEvent.Text = "Modify project Pre-build Event to generate stubs on each build";
            // 
            // checkBoxSeparateStubs
            // 
            this.checkBoxSeparateStubs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSeparateStubs.Location = new System.Drawing.Point(8, 128);
            this.checkBoxSeparateStubs.Name = "checkBoxSeparateStubs";
            this.checkBoxSeparateStubs.Size = new System.Drawing.Size(432, 24);
            this.checkBoxSeparateStubs.TabIndex = 12;
            this.checkBoxSeparateStubs.Text = "Compile stubs to a separate DLL";
            // 
            // checkBoxAddAttributesToAssemblyInfo
            // 
            this.checkBoxAddAttributesToAssemblyInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAddAttributesToAssemblyInfo.Checked = true;
            this.checkBoxAddAttributesToAssemblyInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddAttributesToAssemblyInfo.Location = new System.Drawing.Point(8, 32);
            this.checkBoxAddAttributesToAssemblyInfo.Name = "checkBoxAddAttributesToAssemblyInfo";
            this.checkBoxAddAttributesToAssemblyInfo.Size = new System.Drawing.Size(432, 24);
            this.checkBoxAddAttributesToAssemblyInfo.TabIndex = 11;
            this.checkBoxAddAttributesToAssemblyInfo.Text = "Add necessary attributes to the AssemblyInfo file";
            // 
            // checkBoxCreateKeyGen
            // 
            this.checkBoxCreateKeyGen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxCreateKeyGen.Checked = true;
            this.checkBoxCreateKeyGen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateKeyGen.Location = new System.Drawing.Point(8, 80);
            this.checkBoxCreateKeyGen.Name = "checkBoxCreateKeyGen";
            this.checkBoxCreateKeyGen.Size = new System.Drawing.Size(432, 24);
            this.checkBoxCreateKeyGen.TabIndex = 14;
            this.checkBoxCreateKeyGen.Text = "Create KeyGen table in the database if it does not exist";
            // 
            // checkBoxDisableIdentityColumns
            // 
            this.checkBoxDisableIdentityColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxDisableIdentityColumns.Location = new System.Drawing.Point(8, 104);
            this.checkBoxDisableIdentityColumns.Name = "checkBoxDisableIdentityColumns";
            this.checkBoxDisableIdentityColumns.Size = new System.Drawing.Size(432, 24);
            this.checkBoxDisableIdentityColumns.TabIndex = 13;
            this.checkBoxDisableIdentityColumns.Text = "Disable IDENTITY columns";
            // 
            // linkLabelDisableIdentityColumns
            // 
            this.linkLabelDisableIdentityColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelDisableIdentityColumns.Location = new System.Drawing.Point(448, 104);
            this.linkLabelDisableIdentityColumns.Name = "linkLabelDisableIdentityColumns";
            this.linkLabelDisableIdentityColumns.Size = new System.Drawing.Size(56, 23);
            this.linkLabelDisableIdentityColumns.TabIndex = 23;
            this.linkLabelDisableIdentityColumns.TabStop = true;
            this.linkLabelDisableIdentityColumns.Text = "Explain";
            this.linkLabelDisableIdentityColumns.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelDisableIdentityColumns.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDisableIdentityColumns_LinkClicked);
            // 
            // linkLabelSeparateStubs
            // 
            this.linkLabelSeparateStubs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelSeparateStubs.Location = new System.Drawing.Point(448, 128);
            this.linkLabelSeparateStubs.Name = "linkLabelSeparateStubs";
            this.linkLabelSeparateStubs.Size = new System.Drawing.Size(56, 23);
            this.linkLabelSeparateStubs.TabIndex = 26;
            this.linkLabelSeparateStubs.TabStop = true;
            this.linkLabelSeparateStubs.Text = "Explain";
            this.linkLabelSeparateStubs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelSeparateStubs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSeparateStubs_LinkClicked);
            // 
            // linkLabelModifyBuildEvent
            // 
            this.linkLabelModifyBuildEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelModifyBuildEvent.Location = new System.Drawing.Point(448, 56);
            this.linkLabelModifyBuildEvent.Name = "linkLabelModifyBuildEvent";
            this.linkLabelModifyBuildEvent.Size = new System.Drawing.Size(56, 23);
            this.linkLabelModifyBuildEvent.TabIndex = 25;
            this.linkLabelModifyBuildEvent.TabStop = true;
            this.linkLabelModifyBuildEvent.Text = "Explain";
            this.linkLabelModifyBuildEvent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelModifyBuildEvent.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelModifyBuildEvent_LinkClicked);
            // 
            // linkLabelCreateConfig
            // 
            this.linkLabelCreateConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelCreateConfig.Location = new System.Drawing.Point(448, 8);
            this.linkLabelCreateConfig.Name = "linkLabelCreateConfig";
            this.linkLabelCreateConfig.Size = new System.Drawing.Size(56, 23);
            this.linkLabelCreateConfig.TabIndex = 19;
            this.linkLabelCreateConfig.TabStop = true;
            this.linkLabelCreateConfig.Text = "Explain";
            this.linkLabelCreateConfig.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelCreateConfig.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCreateConfig_LinkClicked);
            // 
            // linkLabelAddAttributes
            // 
            this.linkLabelAddAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelAddAttributes.Location = new System.Drawing.Point(448, 32);
            this.linkLabelAddAttributes.Name = "linkLabelAddAttributes";
            this.linkLabelAddAttributes.Size = new System.Drawing.Size(56, 23);
            this.linkLabelAddAttributes.TabIndex = 21;
            this.linkLabelAddAttributes.TabStop = true;
            this.linkLabelAddAttributes.Text = "Explain";
            this.linkLabelAddAttributes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelAddAttributes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelAddAttributes_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.panelAdvancedOptions);
            this.groupBox1.Location = new System.Drawing.Point(8, 112);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(520, 224);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Advanced Options";
            this.groupBox1.Visible = false;
            // 
            // checkBoxGenerateSchema
            // 
            this.checkBoxGenerateSchema.Checked = true;
            this.checkBoxGenerateSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGenerateSchema.Location = new System.Drawing.Point(8, 56);
            this.checkBoxGenerateSchema.Name = "checkBoxGenerateSchema";
            this.checkBoxGenerateSchema.Size = new System.Drawing.Size(440, 24);
            this.checkBoxGenerateSchema.TabIndex = 23;
            this.checkBoxGenerateSchema.Text = "Create approximate mapping schema by reverse-engineering the database";
            // 
            // linkLabelGenerateSchema
            // 
            this.linkLabelGenerateSchema.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelGenerateSchema.Location = new System.Drawing.Point(456, 56);
            this.linkLabelGenerateSchema.Name = "linkLabelGenerateSchema";
            this.linkLabelGenerateSchema.Size = new System.Drawing.Size(56, 23);
            this.linkLabelGenerateSchema.TabIndex = 24;
            this.linkLabelGenerateSchema.TabStop = true;
            this.linkLabelGenerateSchema.Text = "Explain";
            this.linkLabelGenerateSchema.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelGenerateSchema.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGenerateSchema_LinkClicked);
            // 
            // buttonAdvancedOptions
            // 
            this.buttonAdvancedOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdvancedOptions.Location = new System.Drawing.Point(8, 112);
            this.buttonAdvancedOptions.Name = "buttonAdvancedOptions";
            this.buttonAdvancedOptions.Size = new System.Drawing.Size(128, 23);
            this.buttonAdvancedOptions.TabIndex = 25;
            this.buttonAdvancedOptions.Text = "Advanced Options";
            this.buttonAdvancedOptions.Click += new System.EventHandler(this.buttonAdvancedOptions_Click);
            // 
            // linkLabelGenerateStubs
            // 
            this.linkLabelGenerateStubs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelGenerateStubs.Location = new System.Drawing.Point(456, 80);
            this.linkLabelGenerateStubs.Name = "linkLabelGenerateStubs";
            this.linkLabelGenerateStubs.Size = new System.Drawing.Size(56, 23);
            this.linkLabelGenerateStubs.TabIndex = 24;
            this.linkLabelGenerateStubs.TabStop = true;
            this.linkLabelGenerateStubs.Text = "Explain";
            this.linkLabelGenerateStubs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelGenerateStubs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGenerateStubs_LinkClicked);
            // 
            // WizardPageOptions
            // 
            this.Controls.Add(this.buttonAdvancedOptions);
            this.Controls.Add(this.checkBoxGenerateSchema);
            this.Controls.Add(this.linkLabelGenerateSchema);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxGenerateStubs);
            this.Controls.Add(this.linkLabelGenerateStubs);
            this.EnableFinish = true;
            this.EnableNext = false;
            this.Name = "WizardPageOptions";
            this.Size = new System.Drawing.Size(536, 344);
            this.Load += new System.EventHandler(this.WizardPageOptions_Load);
            this.panelAdvancedOptions.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

        private void WizardPageOptions_Load(object sender, System.EventArgs e)
        {
            comboBoxConfigStyle.SelectedIndex = 0;
        }

        private void OpenDocUrl(string url)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "iexplore.exe";
            psi.Arguments = "http://sooda.sourceforge.net/documentation.html?from=wizard#" + url;
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        internal void FillResult(WizardResult result)
        {
            result.ReverseEngineerDatabase = checkBoxGenerateSchema.Checked;
            result.SeparateStubs = checkBoxSeparateStubs.Checked;
            result.ModifyBuildEvent = checkBoxModifyBuildEvent.Checked;
            result.ModifyAssemblyInfo = checkBoxAddAttributesToAssemblyInfo.Checked;
            result.DisableIdentityColumns = checkBoxDisableIdentityColumns.Checked;
            result.CreateAppConfigFile = checkBoxCreateConfig.Checked && comboBoxConfigStyle.SelectedIndex == 0;
            result.CreateSoodaXmlConfigFile = checkBoxCreateConfig.Checked && comboBoxConfigStyle.SelectedIndex == 1;
            result.CreateKeyGenTable = checkBoxCreateKeyGen.Checked;
        }

        private void buttonViewAdvancedOptions_Click(object sender, System.EventArgs e)
        {
        }

        private void buttonAdvancedOptions_Click(object sender, System.EventArgs e)
        {
            groupBox1.Visible = true;
            buttonAdvancedOptions.Visible = false;
        }

        private void linkLabelGenerateSchema_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("soodaschematoolgenschema");
        }

        private void linkLabelGenerateStubs_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("compilation");
        }

        private void linkLabelCreateConfig_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("configuration");
        }

        private void linkLabelAddAttributes_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("assemblyinfo");
        }

        private void linkLabelModifyBuildEvent_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("compilationvisualstudio");
        }

        private void linkLabelCreateKeyGen_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("keygeneration");
        }

        private void linkLabelDisableIdentityColumns_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("keygeneration");
        }

        private void linkLabelSeparateStubs_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("compilationprocessseparate");
        }
	}
}
