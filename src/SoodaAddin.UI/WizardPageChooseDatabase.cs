using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SoodaAddin.UI
{
    public class WizardPageChooseDatabase : SoodaAddin.UI.WizardPage
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ImageList imageList1;
        private System.ComponentModel.IContainer components = null;

        public WizardPageChooseDatabase()
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

        public string SelectedDatabase
        {
            get
            {
                foreach (ListViewItem it in listView1.SelectedItems)
                {
                    return it.Text;
                }
                return null;
            }
        }

        public void LoadAvailableDatabases(string connectionString)
        {
            Cursor oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("exec sp_databases", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem();
                                item.ImageIndex = 0;
                                item.Tag = item.Text;
                                item.Text = reader.GetString(0);
                                item.SubItems.Add((reader.GetInt32(1) >> 10) + " MB");
                                item.SubItems.Add(Convert.ToString(reader.GetValue(2))); // usually null
                                listView1.Items.Add(item);
                            }
                        }
                    }
                    //listView1.Columns[0].Width = -2;
                    listView1.Columns[1].Width = -2;
                    listView1.Columns[2].Width = -2;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "ERROR: " + ex.ToString());
            }
            finally
            {
                Cursor.Current = oldCursor;
            }

        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WizardPageChooseDatabase));
            this.label1 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(576, 32);
            this.label1.TabIndex = 4;
            this.label1.Text = "Choose a database on the server:";
            //
            // listView1
            //
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                        this.columnHeader1,
                                                                                        this.columnHeader2,
                                                                                        this.columnHeader3});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(24, 48);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(536, 216);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 5;
            this.listView1.View = System.Windows.Forms.View.Details;
            //
            // columnHeader1
            //
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 200;
            //
            // columnHeader2
            //
            this.columnHeader2.Text = "Size";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 120;
            //
            // columnHeader3
            //
            this.columnHeader3.Text = "Remarks";
            this.columnHeader3.Width = 500;
            //
            // imageList1
            //
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
            //
            // WizardPageChooseDatabase
            //
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label1);
            this.Name = "WizardPageChooseDatabase";
            this.Size = new System.Drawing.Size(592, 312);
            this.Load += new System.EventHandler(this.WizardPageChooseDatabase_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private void WizardPageChooseDatabase_Load(object sender, System.EventArgs e)
        {

        }
    }
}

