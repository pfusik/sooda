// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Data;
using System.Collections;

using Sooda.Schema;
using Sooda.Collections;
using Sooda.Caching;
using Sooda.QL;
using Sooda.Logging;

namespace Sooda.ObjectMapper
{
    public class SoodaObjectOneToManyCollection : SoodaObjectCollectionBase, ISoodaObjectList, ISoodaObjectListInternal
    {
        private static Logger logger = LogManager.GetLogger("Sooda.OneToManyCollection");

        private SoodaObjectToObjectAssociation tempItems = null;
        private SoodaObject parentObject;
        private string childRefField;
        private Type childType;
        private SoodaWhereClause additionalWhereClause;
        private bool cached;

        private static readonly object markerAdded = new Object();
        private static readonly object markerRemoved = new Object();

        public SoodaObjectOneToManyCollection(SoodaTransaction tran, Type childType, SoodaObject parentObject, string childRefField, Sooda.Schema.ClassInfo classInfo, SoodaWhereClause additionalWhereClause, bool cached)
            : base(tran, classInfo)
        {
            this.childType = childType;
            this.parentObject = parentObject;
            this.childRefField = childRefField;
            this.additionalWhereClause = additionalWhereClause;
            this.cached = cached;
        }

        public override IEnumerator GetEnumerator()
        {
            if (items == null)
                LoadData();
            return items.Keys.GetEnumerator();
        }

        public override int Add(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type t = childType;
            System.Reflection.PropertyInfo prop = t.GetProperty(childRefField);
            prop.SetValue(obj, parentObject, null);
            return 0;
        }

        public override void Remove(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type t = childType;
            System.Reflection.PropertyInfo prop = t.GetProperty(childRefField);
            prop.SetValue(obj, null, null);
        }

        public override bool Contains(object obj)
        {
            if (obj == null)
                return false;

            Type t = childType;
            System.Reflection.PropertyInfo prop = t.GetProperty(childRefField);
            return parentObject == prop.GetValue(obj, null);
        }

        public void InternalAdd(SoodaObject c)
        {
            if (!childType.IsInstanceOfType(c))
                return;

            if (items == null)
            {
                if (tempItems == null)
                    tempItems = new SoodaObjectToObjectAssociation();
                tempItems[c] = markerAdded;
                return;
            }

            if (items.Contains(c))
                return;

            int pos = itemsArray.Add(c);
            items.Add(c, pos);
        }

        public void InternalRemove(SoodaObject c)
        {
            if (!childType.IsInstanceOfType(c))
                return;

            if (items == null)
            {
                if (tempItems == null)
                    tempItems = new SoodaObjectToObjectAssociation();
                tempItems[c] = markerRemoved;
                return;
            }

            if (!items.Contains(c))
                throw new InvalidOperationException("Attempt to remove object not in collection");

            int pos = items[c];

            SoodaObject lastObj = itemsArray[itemsArray.Count - 1];
            if (lastObj != c)
            {
                itemsArray[pos] = lastObj;
                items[lastObj] = pos;
            }
            itemsArray.RemoveAt(itemsArray.Count - 1);
            items.Remove(c);
        }

        protected override void LoadData()
        {
            SoodaDataSource ds = transaction.OpenDataSource(classInfo.GetDataSource());
            TableInfo[] loadedTables;

            items = new SoodaObjectToInt32Association();
            itemsArray = new SoodaObjectCollection();

            ISoodaObjectFactory factory = transaction.GetFactory(classInfo);
            SoodaWhereClause whereClause = new SoodaWhereClause(Soql.FieldEqualsParam(childRefField, 0), parentObject.GetPrimaryKeyValue());

            if (additionalWhereClause != null)
                whereClause = whereClause.Append(additionalWhereClause);

            string cacheKey = null;

            if (cached)
            {
                // cache makes sense only on clean database
                if (!transaction.HasBeenPrecommitted(classInfo.GetRootClass()))
                {
                    cacheKey = SoodaCache.GetCollectionKey(classInfo, whereClause);
                }
            }
            IEnumerable keysCollection = transaction.Cache.LoadCollection(cacheKey);
            if (keysCollection != null)
            {
                foreach (object o in keysCollection)
                {
                    SoodaObject obj = factory.GetRef(transaction, o);
                    // this binds to cache
                    obj.EnsureFieldsInited();

                    if (tempItems != null && tempItems[obj] == markerRemoved)
                        continue;

                    int pos = itemsArray.Add(obj);
                    items.Add(obj, pos);
                }
            }
            else
            {
                if (cacheKey != null)
                {
                    logger.Debug("Cache miss. {0} not found in cache.", cacheKey);
                    SoodaStatistics.Global.RegisterCollectionCacheMiss();
                    transaction.Statistics.RegisterCollectionCacheMiss();
                }

                using (IDataReader reader = ds.LoadObjectList(transaction.Schema, classInfo, whereClause, null, 0, -1, SoodaSnapshotOptions.Default, out loadedTables))
                {
                    SoodaObjectCollection readObjects = null;

                    if (cached)
                        readObjects = new SoodaObjectCollection();

                    while (reader.Read())
                    {
                        SoodaObject obj = SoodaObject.GetRefFromRecordHelper(transaction, factory, reader, 0, loadedTables, 0);
                        if (readObjects != null)
                            readObjects.Add(obj);

                        if (tempItems != null && tempItems[obj] == markerRemoved)
                            continue;

                        int pos = itemsArray.Add(obj);
                        items.Add(obj, pos);
                    }
                    if (cached)
                    {
                        TimeSpan expirationTimeout;
                        bool slidingExpiration;

                        if (transaction.CachingPolicy.GetExpirationTimeout(
                            classInfo, whereClause, null, 0, -1, readObjects.Count,
                            out expirationTimeout, out slidingExpiration))
                        {
                            StoreInCache(cacheKey, readObjects, null, expirationTimeout, slidingExpiration);
                        }
                    }
                }
            }

            if (tempItems != null)
            {
                foreach (DictionaryEntry entry in tempItems)
                {
                    if (entry.Value == markerAdded)
                    {
                        SoodaObject obj = (SoodaObject)entry.Key;

                        if (!items.Contains(obj))
                        {
                            int pos = itemsArray.Add(obj);
                            items.Add(obj, pos);
                        }
                    }
                }
            }
        }
    }
}
