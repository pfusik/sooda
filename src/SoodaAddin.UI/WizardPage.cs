using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace SoodaAddin.UI
{
	/// <summary>
	/// Summary description for WizardPage.
	/// </summary>
	public class WizardPage : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        private bool _enableFinish = false;

        public bool EnableFinish
        {
            get { return _enableFinish; }
            set { _enableFinish = value; }
        }

        private bool _enableBack = true;

        public bool EnableBack
        {
            get { return _enableBack; }
            set { _enableBack = value; }
        }

        private bool _enableNext = true;

        public bool EnableNext
        {
            get { return _enableNext; }
            set { _enableNext = value; }
        }

        public WizardPage()
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
            // 
            // WizardPage
            // 
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
            this.Name = "WizardPage";
            this.Size = new System.Drawing.Size(440, 216);

        }
		#endregion
	}
}
