//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//

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

        public static void SetRefFieldValue(SoodaObject theObject, int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, SoodaObject[] refcache, int refcacheOrdinal, ISoodaObjectFactory factory)
        {
            theObject.SetRefFieldValue(tableNumber, fieldName, fieldOrdinal, newValue, refcache, refcacheOrdinal, factory);
        }

        public static SoodaObject SelectSingleObjectBE(Sooda.QL.SoqlBooleanExpression expr, ISoodaObjectList list)
        {
            if (list.Count == 0)
                return null;
            if (list.Count > 1)
                throw new SoodaObjectNotFoundException("Not a unique match: '" + expr + "'");
            return list.GetItem(0);
        }

        public static SoodaObject SelectSingleObjectWC(Sooda.SoodaWhereClause expr, ISoodaObjectList list)
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
