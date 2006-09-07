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
        public System.Windows.Forms.CheckBox checkBoxCreateConfig;
        public System.Windows.Forms.ComboBox comboBoxConfigStyle;
        public System.Windows.Forms.CheckBox checkBoxGenerateSchema;
        public System.Windows.Forms.CheckBox checkBoxAddAttributesToAssemblyInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.LinkLabel linkLabel6;
        private System.Windows.Forms.LinkLabel linkLabel8;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel linkLabel5;
        private System.Windows.Forms.LinkLabel linkLabel7;
        public System.Windows.Forms.CheckBox checkBoxSeparateStubs;
        public System.Windows.Forms.CheckBox checkBoxCreateKeyGen;
        public System.Windows.Forms.CheckBox checkBoxDisableIdentityColumns;
        public System.Windows.Forms.CheckBox checkBoxModifyBuildEvent;
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
            this.checkBoxGenerateSchema = new System.Windows.Forms.CheckBox();
            this.checkBoxCreateConfig = new System.Windows.Forms.CheckBox();
            this.comboBoxConfigStyle = new System.Windows.Forms.ComboBox();
            this.checkBoxModifyBuildEvent = new System.Windows.Forms.CheckBox();
            this.checkBoxSeparateStubs = new System.Windows.Forms.CheckBox();
            this.checkBoxAddAttributesToAssemblyInfo = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxCreateKeyGen = new System.Windows.Forms.CheckBox();
            this.checkBoxDisableIdentityColumns = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.linkLabel6 = new System.Windows.Forms.LinkLabel();
            this.linkLabel8 = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.linkLabel5 = new System.Windows.Forms.LinkLabel();
            this.linkLabel7 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(528, 32);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select options:";
            // 
            // checkBoxGenerateSchema
            // 
            this.checkBoxGenerateSchema.Checked = true;
            this.checkBoxGenerateSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxGenerateSchema.Location = new System.Drawing.Point(32, 96);
            this.checkBoxGenerateSchema.Name = "checkBoxGenerateSchema";
            this.checkBoxGenerateSchema.Size = new System.Drawing.Size(432, 24);
            this.checkBoxGenerateSchema.TabIndex = 6;
            this.checkBoxGenerateSchema.Text = "Create approximate mapping schema by reverse-engineering the database";
            // 
            // checkBoxCreateConfig
            // 
            this.checkBoxCreateConfig.Checked = true;
            this.checkBoxCreateConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateConfig.Location = new System.Drawing.Point(32, 120);
            this.checkBoxCreateConfig.Name = "checkBoxCreateConfig";
            this.checkBoxCreateConfig.Size = new System.Drawing.Size(200, 24);
            this.checkBoxCreateConfig.TabIndex = 7;
            this.checkBoxCreateConfig.Text = "Create configuration file:";
            // 
            // comboBoxConfigStyle
            // 
            this.comboBoxConfigStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxConfigStyle.Items.AddRange(new object[] {
                                                                     "App.config - private",
                                                                     "Sooda.config.xml - shared"});
            this.comboBoxConfigStyle.Location = new System.Drawing.Point(240, 120);
            this.comboBoxConfigStyle.Name = "comboBoxConfigStyle";
            this.comboBoxConfigStyle.Size = new System.Drawing.Size(224, 22);
            this.comboBoxConfigStyle.TabIndex = 8;
            // 
            // checkBoxModifyBuildEvent
            // 
            this.checkBoxModifyBuildEvent.Checked = true;
            this.checkBoxModifyBuildEvent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxModifyBuildEvent.Location = new System.Drawing.Point(32, 168);
            this.checkBoxModifyBuildEvent.Name = "checkBoxModifyBuildEvent";
            this.checkBoxModifyBuildEvent.Size = new System.Drawing.Size(432, 24);
            this.checkBoxModifyBuildEvent.TabIndex = 6;
            this.checkBoxModifyBuildEvent.Text = "Modify project Pre-build Event to generate stubs on each build";
            // 
            // checkBoxSeparateStubs
            // 
            this.checkBoxSeparateStubs.Location = new System.Drawing.Point(32, 240);
            this.checkBoxSeparateStubs.Name = "checkBoxSeparateStubs";
            this.checkBoxSeparateStubs.Size = new System.Drawing.Size(432, 24);
            this.checkBoxSeparateStubs.TabIndex = 6;
            this.checkBoxSeparateStubs.Text = "Compile stubs to a separate DLL";
            // 
            // checkBoxAddAttributesToAssemblyInfo
            // 
            this.checkBoxAddAttributesToAssemblyInfo.Checked = true;
            this.checkBoxAddAttributesToAssemblyInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddAttributesToAssemblyInfo.Location = new System.Drawing.Point(32, 144);
            this.checkBoxAddAttributesToAssemblyInfo.Name = "checkBoxAddAttributesToAssemblyInfo";
            this.checkBoxAddAttributesToAssemblyInfo.Size = new System.Drawing.Size(432, 24);
            this.checkBoxAddAttributesToAssemblyInfo.TabIndex = 6;
            this.checkBoxAddAttributesToAssemblyInfo.Text = "Add necessary attributes to the AssemblyInfo file";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(528, 40);
            this.label2.TabIndex = 9;
            this.label2.Text = "Choose the actions you would like the Wizard to perform and click Finish. Default" +
                " selection should be ok for most new projects.";
            // 
            // checkBoxCreateKeyGen
            // 
            this.checkBoxCreateKeyGen.Checked = true;
            this.checkBoxCreateKeyGen.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCreateKeyGen.Location = new System.Drawing.Point(32, 192);
            this.checkBoxCreateKeyGen.Name = "checkBoxCreateKeyGen";
            this.checkBoxCreateKeyGen.Size = new System.Drawing.Size(432, 24);
            this.checkBoxCreateKeyGen.TabIndex = 6;
            this.checkBoxCreateKeyGen.Text = "Create KeyGen table in the database if it does not exist";
            // 
            // checkBoxDisableIdentityColumns
            // 
            this.checkBoxDisableIdentityColumns.Location = new System.Drawing.Point(32, 216);
            this.checkBoxDisableIdentityColumns.Name = "checkBoxDisableIdentityColumns";
            this.checkBoxDisableIdentityColumns.Size = new System.Drawing.Size(432, 24);
            this.checkBoxDisableIdentityColumns.TabIndex = 6;
            this.checkBoxDisableIdentityColumns.Text = "Disable IDENTITY columns";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Location = new System.Drawing.Point(472, 192);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(56, 23);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Explain";
            this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.Location = new System.Drawing.Point(472, 216);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(56, 23);
            this.linkLabel2.TabIndex = 10;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Explain";
            this.linkLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.Location = new System.Drawing.Point(472, 240);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(56, 23);
            this.linkLabel3.TabIndex = 10;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "Explain";
            this.linkLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // linkLabel6
            // 
            this.linkLabel6.Location = new System.Drawing.Point(472, 168);
            this.linkLabel6.Name = "linkLabel6";
            this.linkLabel6.Size = new System.Drawing.Size(56, 23);
            this.linkLabel6.TabIndex = 10;
            this.linkLabel6.TabStop = true;
            this.linkLabel6.Text = "Explain";
            this.linkLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel6.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel6_LinkClicked);
            // 
            // linkLabel8
            // 
            this.linkLabel8.Location = new System.Drawing.Point(472, 192);
            this.linkLabel8.Name = "linkLabel8";
            this.linkLabel8.Size = new System.Drawing.Size(56, 23);
            this.linkLabel8.TabIndex = 10;
            this.linkLabel8.TabStop = true;
            this.linkLabel8.Text = "Explain";
            this.linkLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel4
            // 
            this.linkLabel4.Location = new System.Drawing.Point(472, 120);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(56, 23);
            this.linkLabel4.TabIndex = 10;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "Explain";
            this.linkLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked_1);
            // 
            // linkLabel5
            // 
            this.linkLabel5.Location = new System.Drawing.Point(472, 96);
            this.linkLabel5.Name = "linkLabel5";
            this.linkLabel5.Size = new System.Drawing.Size(56, 23);
            this.linkLabel5.TabIndex = 10;
            this.linkLabel5.TabStop = true;
            this.linkLabel5.Text = "Explain";
            this.linkLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel5.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel5_LinkClicked);
            // 
            // linkLabel7
            // 
            this.linkLabel7.Location = new System.Drawing.Point(472, 144);
            this.linkLabel7.Name = "linkLabel7";
            this.linkLabel7.Size = new System.Drawing.Size(56, 23);
            this.linkLabel7.TabIndex = 10;
            this.linkLabel7.TabStop = true;
            this.linkLabel7.Text = "Explain";
            this.linkLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel7.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel7_LinkClicked);
            // 
            // WizardPageOptions
            // 
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxConfigStyle);
            this.Controls.Add(this.checkBoxCreateConfig);
            this.Controls.Add(this.checkBoxGenerateSchema);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxModifyBuildEvent);
            this.Controls.Add(this.checkBoxSeparateStubs);
            this.Controls.Add(this.checkBoxAddAttributesToAssemblyInfo);
            this.Controls.Add(this.checkBoxCreateKeyGen);
            this.Controls.Add(this.checkBoxDisableIdentityColumns);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel3);
            this.Controls.Add(this.linkLabel6);
            this.Controls.Add(this.linkLabel8);
            this.Controls.Add(this.linkLabel4);
            this.Controls.Add(this.linkLabel5);
            this.Controls.Add(this.linkLabel7);
            this.EnableFinish = true;
            this.EnableNext = false;
            this.Name = "WizardPageOptions";
            this.Size = new System.Drawing.Size(544, 304);
            this.Load += new System.EventHandler(this.WizardPageOptions_Load);
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
            psi.Arguments = "http://www.sooda.org/test/documentation.html?from=wizard#" + url;
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void linkLabel6_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("compilationvisualstudio");
        }

        private void linkLabel4_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
        
        }

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("keygeneration");
        }

        private void linkLabel2_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("keygeneration");
        }

        private void linkLabel3_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("compilationprocessseparate");
        }

        private void linkLabel5_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("soodaschematoolgenschema");
        }

        private void linkLabel4_LinkClicked_1(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("configuration");
        }

        private void linkLabel7_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenDocUrl("assemblyinfo");
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
	}
}
