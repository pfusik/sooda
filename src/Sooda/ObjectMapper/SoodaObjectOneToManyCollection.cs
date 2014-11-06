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

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

using Sooda.Schema;
using Sooda.Caching;
using Sooda.QL;
using Sooda.Logging;

namespace Sooda.ObjectMapper
{
    enum CollectionChange
    {
        Added,
        Removed
    }

    public class SoodaObjectOneToManyCollection : SoodaObjectCollectionBase, ISoodaObjectList, ISoodaObjectListInternal
    {
        static readonly Logger logger = LogManager.GetLogger("Sooda.OneToManyCollection");

        Dictionary<SoodaObject, CollectionChange> tempItems = null;
        readonly SoodaObject parentObject;
        readonly string childRefField;
        readonly Type childType;
        readonly SoodaWhereClause additionalWhereClause;
        readonly bool cached;

        public SoodaObjectOneToManyCollection(SoodaTransaction tran, Type childType, SoodaObject parentObject, string childRefField, Sooda.Schema.ClassInfo classInfo, SoodaWhereClause additionalWhereClause, bool cached)
            : base(tran, classInfo)
        {
            this.childType = childType;
            this.parentObject = parentObject;
            this.childRefField = childRefField;
            this.additionalWhereClause = additionalWhereClause;
            this.cached = cached;
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
                    tempItems = new Dictionary<SoodaObject, CollectionChange>();
                tempItems[c] = CollectionChange.Added;
                return;
            }

            if (items.ContainsKey(c))
                return;

            items.Add(c, itemsArray.Count);
            itemsArray.Add(c);
        }

        public void InternalRemove(SoodaObject c)
        {
            if (!childType.IsInstanceOfType(c))
                return;

            if (items == null)
            {
                if (tempItems == null)
                    tempItems = new Dictionary<SoodaObject, CollectionChange>();
                tempItems[c] = CollectionChange.Removed;
                return;
            }

            int pos;
            if (!items.TryGetValue(c, out pos))
                throw new InvalidOperationException("Attempt to remove object not in collection");

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

            items = new Dictionary<SoodaObject, int>();
            itemsArray = new List<SoodaObject>();

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
            IEnumerable keysCollection = transaction.LoadCollectionFromCache(cacheKey, logger);
            if (keysCollection != null)
            {
                foreach (object o in keysCollection)
                {
                    SoodaObject obj = factory.GetRef(transaction, o);
                    // this binds to cache
                    obj.EnsureFieldsInited();

                    if (tempItems != null)
                    {
                        CollectionChange change;
                        if (tempItems.TryGetValue(obj, out change) && change == CollectionChange.Removed)
                            continue;
                    }

                    items.Add(obj, itemsArray.Count);
                    itemsArray.Add(obj);
                }
            }
            else
            {
                using (IDataReader reader = ds.LoadObjectList(transaction.Schema, classInfo, whereClause, null, 0, -1, SoodaSnapshotOptions.Default, out loadedTables))
                {
                    List<SoodaObject> readObjects = null;

                    if (cached)
                        readObjects = new List<SoodaObject>();

                    while (reader.Read())
                    {
                        SoodaObject obj = SoodaObject.GetRefFromRecordHelper(transaction, factory, reader, 0, loadedTables, 0);
                        if (readObjects != null)
                            readObjects.Add(obj);

                        if (tempItems != null)
                        {
                            CollectionChange change;
                            if (tempItems.TryGetValue(obj, out change) && change == CollectionChange.Removed)
                                continue;
                        }

                        items.Add(obj, itemsArray.Count);
                        itemsArray.Add(obj);
                    }
                    if (cached)
                    {
                        TimeSpan expirationTimeout;
                        bool slidingExpiration;

                        if (transaction.CachingPolicy.GetExpirationTimeout(
                            classInfo, whereClause, null, 0, -1, readObjects.Count,
                            out expirationTimeout, out slidingExpiration))
                        {
                            transaction.StoreCollectionInCache(cacheKey, classInfo, readObjects, null, true, expirationTimeout, slidingExpiration);
                        }
                    }
                }
            }

            if (tempItems != null)
            {
                foreach (KeyValuePair<SoodaObject, CollectionChange> entry in tempItems)
                {
                    if (entry.Value == CollectionChange.Added)
                    {
                        SoodaObject obj = (SoodaObject) entry.Key;

                        if (!items.ContainsKey(obj))
                        {
                            items.Add(obj, itemsArray.Count);
                            itemsArray.Add(obj);
                        }
                    }
                }
            }
        }
    }
}
