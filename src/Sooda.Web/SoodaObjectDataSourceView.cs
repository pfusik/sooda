using System;
using System.Text;
using System.Web.UI.WebControls;
using System.Web;
using System.Web.UI;
using System.Collections;
using System.Reflection;

namespace Sooda.Web
{
    public class SoodaObjectDataSourceView : DataSourceView
    {
        private SoodaObjectDataSource _owner;

        public override bool CanInsert
        {
            get { return true; }
        }

        public override bool CanUpdate
        {
            get { return true; }
        }

        public override bool CanDelete
        {
            get { return true; }
        }

        public override bool CanSort
        {
            get { return true; }
        }

        public SoodaObjectDataSourceView(SoodaObjectDataSource owner, string viewName) : base(owner, viewName)
        {
            _owner = owner;
        }

        public override void Update(System.Collections.IDictionary keys, System.Collections.IDictionary values, System.Collections.IDictionary oldValues, System.Web.UI.DataSourceViewOperationCallback callback)
        {
            try
            {
                if (keys.Count == 0)
                    throw new SoodaException("No keys passed to SoodaObjectDataSourceView.Update");

                if (keys.Count > 1)
                    throw new SoodaException("More than one key passed to SoodaObjectDataSourceView.Update");

                ISoodaObjectFactory fact = SoodaTransaction.ActiveTransaction.GetFactory(_owner.ClassName);

                // we just want to get the first key from "keys"
                // therefore we break at the end of the loop

                foreach (DictionaryEntry de in keys)
                {
                    SoodaObject obj = SoodaTransaction.ActiveTransaction.GetObject(_owner.ClassName,
                        Convert.ToString(de.Value));

                    Type type = obj.GetType();

                    foreach (DictionaryEntry v in values)
                    {
                        if (oldValues[v.Key] != v.Value)
                        {
                            PropertyInfo pi = type.GetProperty((string)v.Key, BindingFlags.Public | BindingFlags.Instance);
                            if (pi == null)
                                throw new SoodaException(v.Key + " not found in " + type.FullName);

                            pi.SetValue(obj, v.Value, null);
                        }
                    }
                    break;
                }
                callback(1, null);
            }
            catch (Exception ex)
            {
                callback(0, ex);
            }
        }

        public override void Insert(IDictionary values, DataSourceViewOperationCallback callback)
        {
            try
            {
                ISoodaObjectFactory fact = SoodaTransaction.ActiveTransaction.GetFactory(_owner.ClassName);
                SoodaObject obj = fact.CreateNew(SoodaTransaction.ActiveTransaction);
                Type type = obj.GetType();

                foreach (DictionaryEntry v in values)
                {
                    PropertyInfo pi = type.GetProperty((string)v.Key, BindingFlags.Public | BindingFlags.Instance);
                    if (pi == null)
                        throw new SoodaException(v.Key + " not found in " + type.FullName);

                    pi.SetValue(obj, v.Value, null);
                }
                callback(1, null);
            }
            catch (Exception ex)
            {
                callback(0, ex);
            }
        }

        public override void Delete(IDictionary keys, IDictionary oldValues, DataSourceViewOperationCallback callback)
        {
            try
            {
                if (keys.Count == 0)
                    throw new SoodaException("No keys passed to SoodaObjectDataSourceView.Update");

                if (keys.Count > 1)
                    throw new SoodaException("More than one key passed to SoodaObjectDataSourceView.Update");

                ISoodaObjectFactory fact = SoodaTransaction.ActiveTransaction.GetFactory(_owner.ClassName);

                // we just want to get the first key from "keys"
                // therefore we break at the end of the loop

                foreach (DictionaryEntry de in keys)
                {
                    SoodaObject obj = SoodaTransaction.ActiveTransaction.GetObject(_owner.ClassName, Convert.ToString(de.Value));
                    obj.MarkForDelete();
                    break;
                }
                callback(1, null);
            }
            catch (Exception ex)
            {
                callback(0, ex);
            }
        }

        protected override IEnumerable ExecuteSelect(System.Web.UI.DataSourceSelectArguments arguments)
        {
            ISoodaObjectFactory factory = SoodaTransaction.ActiveTransaction.GetFactory(_owner.ClassName, true);

            object value;

            if (_owner.List != null)
            {
                value = _owner.List;
            }
            else if (_owner.Key != null)
            {
                value = SoodaTransaction.ActiveTransaction.GetObject(_owner.ClassName, _owner.Key);
            }
            else
            {
                value = factory.GetList(
                    SoodaTransaction.ActiveTransaction,
                    new SoodaWhereClause(_owner.WhereClause),
                    SoodaOrderBy.Unsorted,
                    SoodaSnapshotOptions.Default);
            }
            if (value is IEnumerable)
            {
                return (IEnumerable)value;
            }
            else
            {
                return new object[] { value };
            }
        }
    }
}
