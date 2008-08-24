// 
// SoodaQuery - A Sooda database query tool
// 
// Copyright (C) 2003-2004 Jaroslaw Kowalski
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// Jaroslaw Kowalski (jaak@polbox.com)
// 

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace SoodaQuery {
    /// <summary>
    /// Summary description for ProjectDialog.
    /// </summary>
    public class ProjectDialog : System.Windows.Forms.Form {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBoxProviders;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxServerName;
        private System.Windows.Forms.ComboBox comboBoxDatabase;
        private System.Windows.Forms.Button buttonFetchDbList;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxResultingConnectionString;
        private System.Windows.Forms.TextBox textBoxAdditionalConnectionString;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxConnectionType;
        private System.Windows.Forms.ComboBox comboBoxOleDbDriver;
        private System.Windows.Forms.Label labelOleDbDriver;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxSoodaSchema;
        private System.Windows.Forms.Button buttonSchemaBrowse;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ProjectDialog() {
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
        protected override void Dispose( bool disposing ) {
            if ( disposing ) {
                if (components != null) {
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
        private void InitializeComponent() {
            this.listBoxProviders = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxResultingConnectionString = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxServerName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDatabase = new System.Windows.Forms.ComboBox();
            this.buttonFetchDbList = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxAdditionalConnectionString = new System.Windows.Forms.TextBox();
            this.textBoxConnectionType = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBoxOleDbDriver = new System.Windows.Forms.ComboBox();
            this.labelOleDbDriver = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxSoodaSchema = new System.Windows.Forms.TextBox();
            this.buttonSchemaBrowse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxProviders
            // 
            this.listBoxProviders.Location = new System.Drawing.Point(8, 24);
            this.listBoxProviders.Name = "listBoxProviders";
            this.listBoxProviders.Size = new System.Drawing.Size(240, 95);
            this.listBoxProviders.TabIndex = 0;
            this.listBoxProviders.SelectedIndexChanged += new System.EventHandler(this.listBoxProviders_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Provider:";
            // 
            // textBoxResultingConnectionString
            // 
            this.textBoxResultingConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxResultingConnectionString.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxResultingConnectionString.Location = new System.Drawing.Point(8, 264);
            this.textBoxResultingConnectionString.Multiline = true;
            this.textBoxResultingConnectionString.Name = "textBoxResultingConnectionString";
            this.textBoxResultingConnectionString.ReadOnly = true;
            this.textBoxResultingConnectionString.Size = new System.Drawing.Size(654, 88);
            this.textBoxResultingConnectionString.TabIndex = 2;
            this.textBoxResultingConnectionString.TabStop = false;
            this.textBoxResultingConnectionString.Text = "textBox1";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(552, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Resulting connection string:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(256, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "Server:";
            // 
            // comboBoxServerName
            // 
            this.comboBoxServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxServerName.Location = new System.Drawing.Point(256, 24);
            this.comboBoxServerName.Name = "comboBoxServerName";
            this.comboBoxServerName.Size = new System.Drawing.Size(320, 21);
            this.comboBoxServerName.TabIndex = 1;
            this.comboBoxServerName.TextChanged += new System.EventHandler(this.Text_Changed);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(256, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 16);
            this.label4.TabIndex = 1;
            this.label4.Text = "Username:";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUserName.Location = new System.Drawing.Point(256, 64);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(320, 20);
            this.textBoxUserName.TabIndex = 2;
            this.textBoxUserName.TextChanged += new System.EventHandler(this.Text_Changed);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(256, 104);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(320, 20);
            this.textBoxPassword.TabIndex = 3;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.Text_Changed);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(256, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "Password:";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(256, 128);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 16);
            this.label6.TabIndex = 1;
            this.label6.Text = "Database:";
            // 
            // comboBoxDatabase
            // 
            this.comboBoxDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDatabase.Location = new System.Drawing.Point(256, 144);
            this.comboBoxDatabase.Name = "comboBoxDatabase";
            this.comboBoxDatabase.Size = new System.Drawing.Size(320, 21);
            this.comboBoxDatabase.TabIndex = 4;
            this.comboBoxDatabase.TextChanged += new System.EventHandler(this.Text_Changed);
            // 
            // buttonFetchDbList
            // 
            this.buttonFetchDbList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFetchDbList.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFetchDbList.Location = new System.Drawing.Point(590, 144);
            this.buttonFetchDbList.Name = "buttonFetchDbList";
            this.buttonFetchDbList.Size = new System.Drawing.Size(75, 23);
            this.buttonFetchDbList.TabIndex = 5;
            this.buttonFetchDbList.Text = "&Fetch list";
            this.buttonFetchDbList.Click += new System.EventHandler(this.buttonFetchDbList_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(510, 390);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(590, 390);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 208);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(400, 16);
            this.label7.TabIndex = 1;
            this.label7.Text = "Additional connection string:";
            // 
            // textBoxAdditionalConnectionString
            // 
            this.textBoxAdditionalConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAdditionalConnectionString.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxAdditionalConnectionString.Location = new System.Drawing.Point(8, 224);
            this.textBoxAdditionalConnectionString.Name = "textBoxAdditionalConnectionString";
            this.textBoxAdditionalConnectionString.Size = new System.Drawing.Size(656, 20);
            this.textBoxAdditionalConnectionString.TabIndex = 6;
            this.textBoxAdditionalConnectionString.TextChanged += new System.EventHandler(this.Text_Changed);
            // 
            // textBoxConnectionType
            // 
            this.textBoxConnectionType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConnectionType.Location = new System.Drawing.Point(8, 184);
            this.textBoxConnectionType.Name = "textBoxConnectionType";
            this.textBoxConnectionType.Size = new System.Drawing.Size(656, 20);
            this.textBoxConnectionType.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(8, 168);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(160, 16);
            this.label8.TabIndex = 1;
            this.label8.Text = "Type:";
            // 
            // comboBoxOleDbDriver
            // 
            this.comboBoxOleDbDriver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOleDbDriver.Location = new System.Drawing.Point(8, 144);
            this.comboBoxOleDbDriver.Name = "comboBoxOleDbDriver";
            this.comboBoxOleDbDriver.Size = new System.Drawing.Size(240, 21);
            this.comboBoxOleDbDriver.TabIndex = 4;
            // 
            // labelOleDbDriver
            // 
            this.labelOleDbDriver.Location = new System.Drawing.Point(8, 128);
            this.labelOleDbDriver.Name = "labelOleDbDriver";
            this.labelOleDbDriver.Size = new System.Drawing.Size(240, 16);
            this.labelOleDbDriver.TabIndex = 1;
            this.labelOleDbDriver.Text = "OLEDB Driver";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.Location = new System.Drawing.Point(8, 360);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 23);
            this.label9.TabIndex = 10;
            this.label9.Text = "Sooda Schema";
            // 
            // textBoxSoodaSchema
            // 
            this.textBoxSoodaSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSoodaSchema.Location = new System.Drawing.Point(120, 360);
            this.textBoxSoodaSchema.Name = "textBoxSoodaSchema";
            this.textBoxSoodaSchema.Size = new System.Drawing.Size(464, 20);
            this.textBoxSoodaSchema.TabIndex = 9;
            // 
            // buttonSchemaBrowse
            // 
            this.buttonSchemaBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSchemaBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSchemaBrowse.Location = new System.Drawing.Point(590, 360);
            this.buttonSchemaBrowse.Name = "buttonSchemaBrowse";
            this.buttonSchemaBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonSchemaBrowse.TabIndex = 5;
            this.buttonSchemaBrowse.Text = "&Browse...";
            // 
            // ProjectDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(672, 422);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBoxConnectionType);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.textBoxResultingConnectionString);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxAdditionalConnectionString);
            this.Controls.Add(this.textBoxSoodaSchema);
            this.Controls.Add(this.buttonFetchDbList);
            this.Controls.Add(this.comboBoxServerName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxProviders);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxDatabase);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.comboBoxOleDbDriver);
            this.Controls.Add(this.labelOleDbDriver);
            this.Controls.Add(this.buttonSchemaBrowse);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProjectDialog";
            this.Text = "ProjectDialog";
            this.Load += new System.EventHandler(this.ProjectDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endregion

        private void listBoxProviders_SelectedIndexChanged(object sender, System.EventArgs e) {
            RecalcQueryString();

            string provider = (string)listBoxProviders.SelectedItem;
            switch (provider) {
            case providerNameSqlServer:
                textBoxConnectionType.Text = typeof(SqlConnection).AssemblyQualifiedName;
                textBoxConnectionType.ReadOnly = true;
                buttonFetchDbList.Enabled = true;
                labelOleDbDriver.Enabled = false;
                comboBoxOleDbDriver.Enabled = false;
                comboBoxOleDbDriver.Text = "";
                break;
                case providerNameOracle:
                    textBoxConnectionType.Text = typeof(OracleConnection).AssemblyQualifiedName;
                    textBoxConnectionType.ReadOnly = true;
                    buttonFetchDbList.Enabled = false;
                    labelOleDbDriver.Enabled = false;
                    comboBoxOleDbDriver.Enabled = false;
                    comboBoxOleDbDriver.Text = "";
                    break;


#if !SOODA_NO_ODBC

            case providerNameODBC:
                textBoxConnectionType.Text = typeof(System.Data.Odbc.OdbcConnection).AssemblyQualifiedName;
                textBoxConnectionType.ReadOnly = true;
                buttonFetchDbList.Enabled = false;
                labelOleDbDriver.Enabled = false;
                comboBoxOleDbDriver.Enabled = false;
                comboBoxOleDbDriver.Text = "";
                break;
#endif

            case providerNameOleDB:
                textBoxConnectionType.Text = typeof(System.Data.OleDb.OleDbConnection).AssemblyQualifiedName;
                textBoxConnectionType.ReadOnly = true;
                buttonFetchDbList.Enabled = false;
                labelOleDbDriver.Enabled = true;
                comboBoxOleDbDriver.Enabled = true;
                comboBoxOleDbDriver.Text = "";
                break;

            case providerNameCustom:
                textBoxConnectionType.Text = "";
                textBoxConnectionType.ReadOnly = false;
                buttonFetchDbList.Enabled = false;
                labelOleDbDriver.Enabled = false;
                comboBoxOleDbDriver.Enabled = false;
                comboBoxOleDbDriver.Text = "";
                break;
            }

        }

        const string providerNameOleDB = "OLE DB";
        const string providerNameSqlServer = "SQL Server";
        const string providerNameODBC = "ODBC";
        const string providerNameOracle = "Oracle";
        const string providerNameCustom = "Custom";

        private void ProjectDialog_Load(object sender, System.EventArgs e) {
            listBoxProviders.Items.Add(providerNameSqlServer);
            listBoxProviders.Items.Add(providerNameOracle);
            listBoxProviders.Items.Add(providerNameOleDB);
#if !SOODA_NO_ODBC

            listBoxProviders.Items.Add(providerNameODBC);
#endif

            listBoxProviders.Items.Add(providerNameCustom);
            listBoxProviders.SelectedItem = providerNameSqlServer;
        }

        string CalcQueryString(bool revealPassword, string databaseOverride) {
            string queryString = "";
            string provider = (string)listBoxProviders.SelectedItem;
            string server = comboBoxServerName.Text;
            string userId = textBoxUserName.Text;
            string password = textBoxPassword.Text;
            string database = comboBoxDatabase.Text;
            if (databaseOverride != null)
                database = databaseOverride;

            switch (provider) {
            case providerNameSqlServer:
                queryString += String.Format("Data Source={0}; ", server);
                if (userId == "") {
                    queryString += "Integrated Security=SSPI; ";
                } else {
                    queryString += String.Format("User Id={0}; Password={1}; ", userId, revealPassword ? password : "***");
                }
                if (database != "") {
                    queryString += String.Format("Initial Catalog={0}; ", database);
                }
                break;
                case providerNameOracle:
                    queryString += String.Format("User Id={0};Password={1};Server={2}", userId, revealPassword ? password : "***", server);
                    break;
            }

            queryString += textBoxAdditionalConnectionString.Text;
            if (queryString.EndsWith("; "))
                queryString = queryString.Substring(0, queryString.Length - 2);

            return queryString;
        }

        void RecalcQueryString() {
            textBoxResultingConnectionString.Text = CalcQueryString(false, null);
        }

        private void Text_Changed(object sender, System.EventArgs e) {
            RecalcQueryString();
        }

        private void buttonFetchDbList_Click(object sender, System.EventArgs e) {
            try {
                comboBoxDatabase.Items.Clear();
                using (SqlConnection conn = new SqlConnection(CalcQueryString(true, "master"))) {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select name from master.dbo.sysdatabases", conn);
                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            comboBoxDatabase.Items.Add(reader.GetString(0));
                        }
                    }
                }
                comboBoxDatabase.Focus();
            } catch (Exception ex) {
                MessageBox.Show(this, ex.Message, "ERROR");
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }
    }
}
