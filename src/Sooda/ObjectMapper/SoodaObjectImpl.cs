
namespace Sooda.ObjectMapper
{
    public class SoodaObjectImpl
    {
        public static bool IsFieldDirty(SoodaObject theObject, int tableNumber, int fieldOrdinal)
        {
            return theObject.IsFieldDirty(fieldOrdinal);
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

        public static void SetPlainFieldValue(SoodaObject theObject, int tableNumber, string fieldName, int fieldOrdinal, object newValue, SoodaFieldUpdateDelegate before, SoodaFieldUpdateDelegate after)
        {
            theObject.SetPlainFieldValue(tableNumber, fieldName, fieldOrdinal, newValue, before, after);
        }

        public static void SetRefFieldValue(SoodaObject theObject, int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, ref SoodaObject refcache, ISoodaObjectFactory factory)
        {
            theObject.SetRefFieldValue(tableNumber, fieldName, fieldOrdinal, newValue, ref refcache, factory);
        }

        public static SoodaObject SelectSingleObject(Sooda.QL.SoqlBooleanExpression expr, ISoodaObjectList list)
        {
            if (list.Count == 0)
                return null;
            if (list.Count > 1)
                throw new SoodaObjectNotFoundException("Not a unique match: '" + expr + "'");
            return list.GetItem(0);
        }

        public static SoodaObject SelectSingleObject(Sooda.SoodaWhereClause expr, ISoodaObjectList list)
        {
            if (list.Count == 0)
                return null;
            if (list.Count > 1)
                throw new SoodaObjectNotFoundException("Not a unique match: '" + expr + "'");
            return list.GetItem(0);
        }

        public static SoodaObjectFieldValues GetFieldValuesForRead(SoodaObject t, int tableNumber)
        {
            t.EnsureDataLoaded(tableNumber);
            return t._fieldValues;
        }
    }
}
