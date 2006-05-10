using System;
using System.Text;
using System.Web;

using System.Reflection;
using Sooda;

namespace Sooda.Web
{
    public class SoodaPage : System.Web.UI.Page
    {
        private SoodaTransaction _transaction = null;
        private Assembly _objectsAssembly = null;

        public Assembly ObjectsAssembly
        {
            get { return _objectsAssembly; }
            set { _objectsAssembly = value; }
        }

        public SoodaTransaction Transaction
        {
            get { return _transaction; }
        }

        public SoodaPage()
        {
        }

        protected override void OnPreInit(EventArgs e)
        {
            _transaction = new SoodaTransaction(ObjectsAssembly);
        }

        protected override void OnLoad(EventArgs e)
        {
            string soodaSerializedTransaction = ViewState["SoodaTransaction"] as string;
            if (soodaSerializedTransaction != null)
                _transaction.Deserialize(soodaSerializedTransaction);

            //_transaction.Deserialize
            base.OnLoad(e);
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);
            ViewState["SoodaTransaction"] = _transaction.Serialize();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (_transaction != null)
            {
                _transaction.Dispose();
            }
        }

        public void Commit()
        {
            _transaction.Commit();
            ViewState["SoodaTransaction"] = null;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            ViewState["SoodaTransaction"] = null;
        }
    }
}
