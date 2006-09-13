using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace SoodaAddin.UI
{
	/// <summary>
	/// Summary description for SoodaConfigurationWizard.
	/// </summary>
	public class SoodaConfigurationWizard : System.Windows.Forms.Form
	{
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonFinish;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonCancel;
        private System.ComponentModel.Container components = null;

        private ISoodaConfigurationStrategy _strategy;

        public SoodaConfigurationWizard(ISoodaConfigurationStrategy strategy) : this()
        {
            _strategy = strategy;
        }

        public SoodaConfigurationWizard()
		{
			InitializeComponent();
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SoodaConfigurationWizard));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonFinish = new System.Windows.Forms.Button();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(680, 80);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // contentPanel
            // 
            this.contentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.contentPanel.Location = new System.Drawing.Point(8, 88);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Size = new System.Drawing.Size(568, 336);
            this.contentPanel.TabIndex = 1;
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNext.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNext.Location = new System.Drawing.Point(424, 434);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.TabIndex = 2;
            this.buttonNext.Text = "&Next >";
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonFinish
            // 
            this.buttonFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFinish.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFinish.Location = new System.Drawing.Point(504, 434);
            this.buttonFinish.Name = "buttonFinish";
            this.buttonFinish.TabIndex = 2;
            this.buttonFinish.Text = "&Finish";
            this.buttonFinish.Click += new System.EventHandler(this.buttonFinish_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBack.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonBack.Location = new System.Drawing.Point(344, 434);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.TabIndex = 2;
            this.buttonBack.Text = "< &Back";
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(264, 434);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // SoodaConfigurationWizard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(584, 464);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonFinish);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SoodaConfigurationWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sooda Configuration Wizard";
            this.Load += new System.EventHandler(this.SoodaConfigurationWizard_Load);
            this.ResumeLayout(false);

        }
		#endregion

        private WizardPageWelcome _welcomePage;
        private WizardPageConnectToDatabase _connectToDatabasePage;
        private WizardPageChooseDatabase _chooseDatabasePage;
        private WizardPageOptions _optionsPage;

        private void SoodaConfigurationWizard_Load(object sender, System.EventArgs e)
        {
            UpdateTitle();
            _welcomePage = new WizardPageWelcome();
            _welcomePage.buttonBrowse.Click += new EventHandler(buttonBrowse_Click);
            _welcomePage.textBoxProjectPath.Text = _strategy.ProjectFile;
            _connectToDatabasePage = new WizardPageConnectToDatabase();
            _chooseDatabasePage = new WizardPageChooseDatabase();
            _optionsPage = new WizardPageOptions();
            ShowWizardPage(_welcomePage);
        }

        private WizardPage _currentWizardPage;
        private ArrayList _wizardPages = new ArrayList();

        public void ShowWizardPage(WizardPage wizardPage)
        {
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(wizardPage);
            wizardPage.Dock = DockStyle.Fill;
            buttonFinish.Enabled = wizardPage.EnableFinish;
            buttonBack.Enabled = wizardPage.EnableBack;
            buttonNext.Enabled = wizardPage.EnableNext;
            if (wizardPage.EnableFinish)
                AcceptButton = buttonFinish;
            else if (wizardPage.EnableNext)
                AcceptButton = buttonNext;
            else
                AcceptButton = null;

            _currentWizardPage = wizardPage;
        }

        private void buttonFinish_Click(object sender, System.EventArgs e)
        {
            WizardResult result = new WizardResult();
            result.ConnectionString = _connectToDatabasePage.ConnectionString;
            result.SelectedDatabase = _chooseDatabasePage.SelectedDatabase;
            _optionsPage.FillResult(result);

            using (ConfiguringSoodaForm csf = new ConfiguringSoodaForm())
            {
                csf.Strategy = _strategy;
                csf.WizardOptions = result;
                csf.ShowDialog(this);
            }

            //DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonNext_Click(object sender, System.EventArgs e)
        {
            if (_currentWizardPage is WizardPageWelcome)
            {
                if (_welcomePage.textBoxProjectPath.Text.Trim().Length == 0)
                {
                    MessageBox.Show(this, "You must specify a project file to configure.");
                    return;
                }
                if (!File.Exists(_welcomePage.textBoxProjectPath.Text))
                {
                    MessageBox.Show(this, "Project file not found.");
                    return;
                }
                ShowWizardPage(_connectToDatabasePage);
                return;
            }
            if (_currentWizardPage is WizardPageConnectToDatabase)
            {
                if (((WizardPageConnectToDatabase)_currentWizardPage).TryConnect())
                {
                    _chooseDatabasePage.LoadAvailableDatabases(((WizardPageConnectToDatabase)_currentWizardPage).ConnectionString);
                    ShowWizardPage(_chooseDatabasePage);
                    return;
                }
            }

            if (_currentWizardPage is WizardPageConnectToDatabase)
            {
                if (((WizardPageConnectToDatabase)_currentWizardPage).TryConnect())
                {
                    _chooseDatabasePage.LoadAvailableDatabases(((WizardPageConnectToDatabase)_currentWizardPage).ConnectionString);
                    ShowWizardPage(_chooseDatabasePage);
                    return;
                }
            }

            if (_currentWizardPage is WizardPageChooseDatabase)
            {
                string selectedDatabase = ((WizardPageChooseDatabase)_currentWizardPage).SelectedDatabase;
                if (selectedDatabase == null)
                {
                    MessageBox.Show(this, "You must select a database.");
                    return;
                }

                ShowWizardPage(_optionsPage);
                return;
            }
        }

        private void buttonBack_Click(object sender, System.EventArgs e)
        {
            if (_currentWizardPage == _optionsPage)
            {
                ShowWizardPage(_chooseDatabasePage);
                return;
            }
            if (_currentWizardPage == _chooseDatabasePage)
            {
                ShowWizardPage(_connectToDatabasePage);
                return;
            }
            if (_currentWizardPage == _connectToDatabasePage)
            {
                ShowWizardPage(_welcomePage);
                return;
            }
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = _strategy.ProjectFile;
                ofd.Title = "Open Visual Studio Project File";
                ofd.Filter = "Project Files (*.csproj;*.vbproj)|*.csproj;*.vbproj|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _strategy.ProjectFile = ofd.FileName;
                    _welcomePage.textBoxProjectPath.Text = ofd.FileName;
                    Text = "Sooda Configuration Wizard - " + Path.GetFileName(_strategy.ProjectFile);
                }
            }
        }

        private void UpdateTitle()
        {
            if (_strategy.ProjectFile != null)
            {
                Text = "Sooda Configuration Wizard - " + Path.GetFileName(_strategy.ProjectFile);
            }
            else
            {
                Text = "Sooda Configuration Wizard - No Project Selected";
            }
        }
    }
}
