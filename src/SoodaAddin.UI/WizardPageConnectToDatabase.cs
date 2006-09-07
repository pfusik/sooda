using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SoodaAddin.UI
{
	public class WizardPageConnectToDatabase : SoodaAddin.UI.WizardPage
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.TextBox textBoxLoginName;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelLoginName;
        private System.Windows.Forms.Label labelPassword;
		private System.ComponentModel.IContainer components = null;

		public WizardPageConnectToDatabase()
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.textBoxLoginName = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelLoginName = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(520, 32);
            this.label1.TabIndex = 3;
            this.label1.Text = "Connect to the database";
            // 
            // comboBox1
            // 
            this.comboBox1.Location = new System.Drawing.Point(176, 48);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(160, 22);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.Text = "(local)";
            this.comboBox1.DropDown += new System.EventHandler(this.comboBox1_DropDown);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.TabIndex = 0;
            this.label2.Text = "Database Server:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 72);
            this.label3.Name = "label3";
            this.label3.TabIndex = 0;
            this.label3.Text = "Connect using:";
            // 
            // radioButton1
            // 
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(24, 96);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(248, 24);
            this.radioButton1.TabIndex = 6;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Windows Authentication";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(24, 120);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(176, 24);
            this.radioButton2.TabIndex = 7;
            this.radioButton2.Text = "SQL Server Authentication";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // textBoxLoginName
            // 
            this.textBoxLoginName.Enabled = false;
            this.textBoxLoginName.Location = new System.Drawing.Point(176, 152);
            this.textBoxLoginName.Name = "textBoxLoginName";
            this.textBoxLoginName.Size = new System.Drawing.Size(160, 22);
            this.textBoxLoginName.TabIndex = 1;
            this.textBoxLoginName.Text = "sa";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Enabled = false;
            this.textBoxPassword.Location = new System.Drawing.Point(176, 176);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(160, 22);
            this.textBoxPassword.TabIndex = 8;
            this.textBoxPassword.Text = "";
            // 
            // labelLoginName
            // 
            this.labelLoginName.Enabled = false;
            this.labelLoginName.Location = new System.Drawing.Point(56, 152);
            this.labelLoginName.Name = "labelLoginName";
            this.labelLoginName.TabIndex = 0;
            this.labelLoginName.Text = "Login name:";
            // 
            // labelPassword
            // 
            this.labelPassword.Enabled = false;
            this.labelPassword.Location = new System.Drawing.Point(56, 176);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.TabIndex = 9;
            this.labelPassword.Text = "Password:";
            // 
            // WizardPageConnectToDatabase
            // 
            this.Controls.Add(this.labelLoginName);
            this.Controls.Add(this.textBoxLoginName);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelPassword);
            this.Name = "WizardPageConnectToDatabase";
            this.Size = new System.Drawing.Size(536, 296);
            this.ResumeLayout(false);

        }
		#endregion

        private void comboBox1_DropDown(object sender, System.EventArgs e)
        {
            Cursor oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {

                if (comboBox1.Items.Count == 0)
                {
                    SQLDMO.SQLServer server = new SQLDMO.SQLServerClass();
                    SQLDMO.NameList names = server.Application.ListAvailableSQLServers();
                    for (int i = 0; i < names.Count; ++i)
                    {
                        string s = Convert.ToString(names.Item(i));
                        if (s != null)
                            comboBox1.Items.Add(s);
                    }
                }
            }
            finally
            {
                Cursor.Current = oldCursor;
            }
        }

        private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
        {
            labelLoginName.Enabled = 
                labelPassword.Enabled = 
                textBoxLoginName.Enabled = 
                textBoxPassword.Enabled = radioButton2.Checked;
        }

        private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
        {
            labelLoginName.Enabled = 
                labelPassword.Enabled = 
                textBoxLoginName.Enabled = 
                textBoxPassword.Enabled = radioButton2.Checked;
        }

        public string ConnectionString
        {
            get 
            { 
                string connectionString = "Server=" + comboBox1.Text + ";";

                if (radioButton1.Checked)
                {
                    connectionString += "Integrated Security=true";
                }
                else
                {
                    connectionString += "User ID=" + textBoxLoginName.Text + ";Password=" + textBoxPassword.Text;
                }
                return connectionString;
            }

        }

        public bool TryConnect()
        {
            Cursor oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR");
                return false;
            }
            finally
            {
                Cursor.Current = oldCursor;
            }
        }
	}
}

