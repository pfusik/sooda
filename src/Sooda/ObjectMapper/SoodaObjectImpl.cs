using System;
using System.Reflection;
using System.Xml;

namespace Sooda.ObjectMapper
{
    public class SoodaObjectImpl
    {
        public static bool IsFieldDirty(SoodaObject theObject, int tableNumber, int fieldOrdinal)
        {
            theObject.EnsureFieldsInited();
            return theObject._fieldData[fieldOrdinal].IsDirty;
        }

        public static bool IsFieldNull(SoodaObject theObject, int tableNumber, int fieldOrdinal)
        {
            theObject.EnsureDataLoaded(tableNumber);
            return theObject._fieldValues.IsNull(fieldOrdinal);
        }

        public static object GetBoxedFieldValue(SoodaObject theObject, int tableNumber, int fieldOrdinal)
        {
            theObject.EnsureDataLoaded(tableNumber);
            return theObject._fieldValues.GetBoxedFieldValue(fieldOrdinal);
        }

        public static SoodaObject GetRefFieldValue(ref SoodaObject refCache, SoodaObject theObject, int tableNumber, int fieldOrdinal, SoodaTransaction tran, ISoodaObjectFactory factory) 
        {
            if (refCache != null)
                return refCache;

            theObject.EnsureDataLoaded(tableNumber);

            if (theObject._fieldValues.IsNull(fieldOrdinal))
                return null;

            refCache = factory.GetRef(tran, theObject._fieldValues.GetBoxedFieldValue(fieldOrdinal));
            return refCache;
        }

        public static SoodaObject TryGetRefFieldValue(ref SoodaObject refCache, object fieldValue, SoodaTransaction tran, ISoodaObjectFactory factory) 
        {
            if (refCache != null)
                return refCache;

            if (fieldValue == null)
                return null;

            refCache = factory.TryGet(tran, fieldValue);
            return refCache;
        }

        public static void LoadAllData(SoodaObject theObject)
        {
            theObject.LoadAllData();
        }

        public static void SetPlainFieldValue(SoodaObject theObject, int tableNumber, string fieldName, int fieldOrdinal, object newValue)
        {
            theObject.SetPlainFieldValue(tableNumber, fieldName, fieldOrdinal, newValue);
        }

        public static void SetRefFieldValue(SoodaObject theObject, int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, ref SoodaObject refcache, ISoodaObjectFactory factory)
        {
            theObject.SetRefFieldValue(tableNumber, fieldName, fieldOrdinal, newValue, ref refcache, factory);
        }
    }
}
