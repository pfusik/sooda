using System;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Collections;

namespace Sooda.Web
{
    public class SoodaObjectDataSource : DataSourceControl
    {
        private string _className;
        private string _whereClause = "true";
        private string _primaryKey = null;
        private IList _theList = null;

        public SoodaObjectDataSource()
        {
        }

        public string ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public string WhereClause
        {
            get { return _whereClause; }
            set { _whereClause = value; }
        }

        public string Key
        {
            get { return _primaryKey; }
            set { _primaryKey = value; }
        }

        public IList List
        {
            get { return _theList; }
            set { _theList = value; }
        }

        protected override System.Web.UI.DataSourceView GetView(string viewName)
        {
            return new SoodaObjectDataSourceView(this, viewName);
        }
    }
}
