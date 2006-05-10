using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Sooda.Web;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.Web.Tests
{
    public partial class _Default : Sooda.Web.SoodaPage
    {
        public _Default()
        {
            ObjectsAssembly = typeof(_DatabaseSchema).Assembly;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //cl = Contact.GetList(true);
            //Gridview1.DataSource = lds;
            //Gridview1.AllowPaging = true;
            //Gridview1.PageSize = 3;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //cl[0].Name = "aaa";
        }

        protected override void OnPreRender(EventArgs e)
        {
            serialized.Text = Server.HtmlEncode(Transaction.Serialize());
            
            DataBind();
            base.OnPreRender(e);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //Commit();
        }
    }
}
