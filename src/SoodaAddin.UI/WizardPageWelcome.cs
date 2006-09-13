using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoodaAddin.UI
{
	public class WizardPageWelcome : SoodaAddin.UI.WizardPage
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBoxProjectPath;
        public System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label label5;
		private System.ComponentModel.IContainer components = null;

		public WizardPageWelcome()
		{
			// This call is required by the Windows Form Designer.
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
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxProjectPath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(568, 32);
            this.label1.TabIndex = 2;
            this.label1.Text = "Welcome to Sooda Configuration Wizard!";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label2.Location = new System.Drawing.Point(8, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(568, 48);
            this.label2.TabIndex = 1;
            this.label2.Text = "We will connect to the database you choose, generate mapping schema and modify yo" +
                "ur Visual Studio project file to support automatic generation of stub classes fo" +
                "r Sooda.";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label3.Location = new System.Drawing.Point(8, 184);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(568, 48);
            this.label3.TabIndex = 1;
            this.label3.Text = "Make sure that you have created a database that the wizard will be connecting to " +
                "(only MS SQL is supported at the moment). When you are ready to proceed, click N" +
                "ext.";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label4.Location = new System.Drawing.Point(8, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(568, 24);
            this.label4.TabIndex = 1;
            this.label4.Text = "This wizard will walk you through the steps necessary to add support for Sooda to" +
                " your project.";
            // 
            // textBoxProjectPath
            // 
            this.textBoxProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProjectPath.Location = new System.Drawing.Point(8, 152);
            this.textBoxProjectPath.Name = "textBoxProjectPath";
            this.textBoxProjectPath.ReadOnly = true;
            this.textBoxProjectPath.Size = new System.Drawing.Size(488, 22);
            this.textBoxProjectPath.TabIndex = 0;
            this.textBoxProjectPath.Text = "";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonBrowse.Location = new System.Drawing.Point(504, 152);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.TabIndex = 3;
            this.buttonBrowse.Text = "Browse...";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label5.Location = new System.Drawing.Point(8, 128);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(568, 24);
            this.label5.TabIndex = 1;
            this.label5.Text = "Choose a Visual Studio Project file:";
            // 
            // WizardPageWelcome
            // 
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxProjectPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.EnableBack = false;
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.Name = "WizardPageWelcome";
            this.Size = new System.Drawing.Size(584, 256);
            this.ResumeLayout(false);

        }
		#endregion
	}
}

