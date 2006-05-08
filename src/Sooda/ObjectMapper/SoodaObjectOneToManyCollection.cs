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
    public class SoodaObjectOneToManyCollection : IList, ISoodaObjectList, ISoodaObjectListInternal
    {
        private static Logger logger = LogManager.GetLogger("Sooda.OneToManyCollection");

        private SoodaObjectToInt32Association items = null;
        private SoodaObjectToObjectAssociation tempItems = null;
        private SoodaObjectCollection itemsArray = null;
        private SoodaObject parentObject;
        private string childRefField;
        protected Sooda.Schema.ClassInfo classInfo;
        private SoodaTransaction transaction;
        private Type childType;
        private SoodaWhereClause additionalWhereClause;
        private bool cached;

        private static object markerAdded = new Object();
        private static object markerRemoved = new Object();
        private static object markerUnchanged = new Object();

        public SoodaObjectOneToManyCollection(SoodaTransaction tran, Type childType, SoodaObject parentObject, string childRefField, Sooda.Schema.ClassInfo classInfo, SoodaWhereClause additionalWhereClause, bool cached)
        {
            this.classInfo = classInfo;
            this.transaction = tran;
            this.childType = childType;
            this.parentObject = parentObject;
            this.childRefField = childRefField;
            this.additionalWhereClause = additionalWhereClause;
            this.cached = cached;
        }

        public SoodaObject GetItem(int pos)
        {
            if (items == null)
                LoadData();
            return (SoodaObject)itemsArray[pos];
        }

        public int Length
        {
            get
            {
                if (items == null)
                    LoadData();
                return itemsArray.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (items == null)
                LoadData();
            return items.Keys.GetEnumerator();
        }

        public int Add(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type t = childType;
            System.Reflection.PropertyInfo prop = t.GetProperty(childRefField);
            prop.SetValue(obj, parentObject, null);
            return 0;
        }

        public void Remove(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type t = childType;
            System.Reflection.PropertyInfo prop = t.GetProperty(childRefField);
            prop.SetValue(obj, null, null);
        }

        public bool Contains(object obj)
        {
            if (obj == null)
                return false;

            Type t = childType;
            System.Reflection.PropertyInfo prop = t.GetProperty(childRefField);
            return parentObject == prop.GetValue(obj, null);
        }

        public void InternalAdd(SoodaObject c)
        {
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

        private void LoadData()
        {
            SoodaDataSource ds = transaction.OpenDataSource(classInfo.GetDataSource());
            TableInfo[] loadedTables;

            items = new SoodaObjectToInt32Association();
            itemsArray = new SoodaObjectCollection();

            ISoodaObjectFactory factory = transaction.GetFactory(classInfo);
            SoodaWhereClause whereClause = new SoodaWhereClause(childRefField + " = {0}", parentObject.GetPrimaryKeyValue());

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

                    if (tempItems != null)
                    {
                        object o2 = tempItems[obj];
                        if (o2 == markerRemoved)
                        {
                            continue;
                        }
                    }

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

                using (IDataReader reader = ds.LoadObjectList(transaction.Schema, classInfo, whereClause, null, -1, SoodaSnapshotOptions.Default, out loadedTables))
                {
                    SoodaObjectCollection readObjects = null;

                    if (cached)
                        readObjects = new SoodaObjectCollection();

                    while (reader.Read())
                    {
                        SoodaObject obj = SoodaObject.GetRefFromRecordHelper(transaction, factory, reader, 0, loadedTables, 0);
                        if (readObjects != null)
                            readObjects.Add(obj);

                        if (tempItems != null)
                        {
                            object o = tempItems[obj];
                            if (o == markerRemoved)
                            {
                                continue;
                            }
                        }

                        int pos = itemsArray.Add(obj);
                        items.Add(obj, pos);
                    }
                    if (cached)
                    {
                        IList primaryKeys = Sooda.Caching.CacheUtils.ConvertSoodaObjectListToKeyList(readObjects);
                        transaction.Cache.StoreCollection(cacheKey, classInfo.GetRootClass().Name, primaryKeys, null, true);
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

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void RemoveAt(int index)
        {
            Remove(GetItem(index));
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public int IndexOf(object value)
        {
            object o = items[(SoodaObject)value];
            if (o == null)
                return -1;
            else
                return (int)o;
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                return this.Length;
            }
        }

        public void CopyTo(Array array, int index)
        {
            items.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public ISoodaObjectList GetSnapshot()
        {
            return new SoodaObjectListSnapshot(this);
        }

        public ISoodaObjectList SelectFirst(int n)
        {
            return new SoodaObjectListSnapshot(this, 0, n);
        }

        public ISoodaObjectList SelectLast(int n)
        {
            return new SoodaObjectListSnapshot(this, this.Length - n, n);
        }

        public ISoodaObjectList SelectRange(int from, int to)
        {
            return new SoodaObjectListSnapshot(this, from, to - from);
        }

        public ISoodaObjectList Filter(SoodaObjectFilter filter)
        {
            return new SoodaObjectListSnapshot(this, filter);
        }

        public ISoodaObjectList Filter(SoqlBooleanExpression filterExpression)
        {
            return new SoodaObjectListSnapshot(this, filterExpression);
        }

        public ISoodaObjectList Filter(SoodaWhereClause whereClause)
        {
            return new SoodaObjectListSnapshot(this, whereClause);
        }

        public ISoodaObjectList Sort(IComparer comparer)
        {
            return new SoodaObjectListSnapshot(this, comparer);
        }

        public ISoodaObjectList Sort(string sortOrder)
        {
            return new SoodaObjectListSnapshot(this).Sort(sortOrder);
        }
        
        public ISoodaObjectList Sort(SoqlExpression expression, SortOrder sortOrder)
        {
            return new SoodaObjectListSnapshot(this).Sort(expression, sortOrder);
        }
        
        public ISoodaObjectList Sort(SoqlExpression expression)
        {
            return new SoodaObjectListSnapshot(this).Sort(expression, SortOrder.Ascending);
        }
    }
}
