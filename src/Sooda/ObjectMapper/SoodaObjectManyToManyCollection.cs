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

using Sooda.Collections;
using Sooda.QL;
using Sooda.Logging;

using Sooda.Schema;

namespace Sooda.ObjectMapper
{
    public class SoodaObjectManyToManyCollection : IList, ISoodaObjectList
    {
        private static Logger logger = LogManager.GetLogger("Sooda.ManyToManyCollection");
        protected SoodaTransaction transaction;
        protected SoodaObjectToInt32Association items = null;
        protected SoodaObjectCollection itemsArray = null;
        protected int masterColumn;
        protected object masterValue;
        protected Type relationType;
        protected Sooda.Schema.RelationInfo relationInfo;
        private SoodaRelationTable relationTable = null;
        private ClassInfo _classInfo;
        private ISoodaObjectFactory _factory;

        public SoodaObjectManyToManyCollection(SoodaTransaction transaction, int masterColumn, object masterValue, Type relationType, Sooda.Schema.RelationInfo relationInfo)
        {
            this.relationInfo = relationInfo;
            this.transaction = transaction;
            this.masterValue = masterValue;
            this.masterColumn = masterColumn;
            this.relationType = relationType;

            _classInfo = (masterColumn == 0) ? relationInfo.GetRef1ClassInfo() : relationInfo.GetRef2ClassInfo();
            _factory = transaction.GetFactory(_classInfo);
        }

        public SoodaObject GetItem(int pos)
        {
            if (itemsArray == null)
                LoadData();
            return itemsArray[pos];
        }

        public int Length
        {
            get
            {
                if (itemsArray == null)
                    LoadData();
                return itemsArray.Count;
            }
        }

        public int Add(SoodaObject obj)
        {
            if (masterColumn == 0)
            {
                SoodaRelationTable rel = this.GetSoodaRelationTable();
                rel.Add(obj.GetPrimaryKeyValue(), this.masterValue);
                return this.InternalAdd(obj);
            }
            else
            {
                SoodaRelationTable rel = this.GetSoodaRelationTable();
                rel.Add(this.masterValue, obj.GetPrimaryKeyValue());
                return this.InternalAdd(obj);
            }
        }

        public void Remove(SoodaObject obj)
        {
            if (masterColumn == 0)
            {
                SoodaRelationTable rel = this.GetSoodaRelationTable();
                rel.Remove(obj.GetPrimaryKeyValue(), this.masterValue);
                this.InternalRemove(obj);
            }
            else
            {
                SoodaRelationTable rel = this.GetSoodaRelationTable();
                rel.Remove(this.masterValue, obj.GetPrimaryKeyValue());
                this.InternalRemove(obj);
            }
        }

        public bool Contains(SoodaObject obj)
        {
            return InternalContains(obj);
        }

        public IEnumerator GetEnumerator()
        {
            if (itemsArray == null)
                LoadData();
            return ((IEnumerable)itemsArray).GetEnumerator();
        }

        protected int InternalAdd(SoodaObject obj)
        {
            if (itemsArray == null)
                return -1;
            if (!items.Contains(obj))
            {
                int pos = itemsArray.Add(obj);
                items.Add(obj, pos);
            }
            return -1;
        }

        protected void InternalRemove(SoodaObject obj)
        {
            if (itemsArray == null)
                return;

            if (!items.Contains(obj))
                return;
            int pos = items[obj];

            SoodaObject lastObj = itemsArray[itemsArray.Count - 1];
            if (lastObj != obj)
            {
                itemsArray[pos] = lastObj;
                items[lastObj] = pos;
            }
            itemsArray.RemoveAt(itemsArray.Count - 1);
            items.Remove(obj);
        }

        public bool InternalContains(SoodaObject obj)
        {
            if (itemsArray == null)
                LoadData();
            return items.Contains(obj);
        }

        protected void LoadDataFromReader()
        {
            SoodaDataSource ds = transaction.OpenDataSource(relationInfo.GetDataSource());
            TableInfo[] loadedTables;
            using (IDataReader reader = ds.LoadRefObjectList(transaction.Schema, relationInfo, masterColumn, masterValue, out loadedTables))
            {
                while (reader.Read())
                {
                    SoodaObject obj = SoodaObject.GetRefFromRecordHelper(transaction, _factory, reader, 0, loadedTables, 0);
                    InternalAdd(obj);
                }
            }
        }

        protected SoodaRelationTable GetSoodaRelationTable()
        {
            if (relationTable == null)
            {
                relationTable = transaction.GetRelationTable(relationType);
            };
            return relationTable;
        }

        void OnTupleChanged(object sender, SoodaRelationTupleChangedArgs args)
        {
            SoodaObject obj;

            if (masterColumn == 0)
            {
                obj = _factory.GetRef(transaction, args.Left);
            }
            else
            {
                obj = _factory.GetRef(transaction, args.Right);
            }

            if (args.Mode == 1)
                InternalAdd(obj);
            else if (args.Mode == -1)
                InternalRemove(obj);
        }

        protected void LoadData()
        {
            bool useCache = false; //transaction.CachingPolicy.ShouldCacheRelation(relationInfo, _classInfo);
            string cacheKey = null;

            items = new SoodaObjectToInt32Association();
            itemsArray = new SoodaObjectCollection();

            if (useCache)
            {
                if (!transaction.HasBeenPrecommitted(relationInfo) && !transaction.HasBeenPrecommitted(_classInfo))
                {
                    cacheKey = relationInfo.Name + " where " + relationInfo.Table.Fields[1 - masterColumn].Name + " = " + masterValue;
                }
                else
                {
                    string tmpCacheKey = relationInfo.Name + " where " + relationInfo.Table.Fields[1 - masterColumn].Name + " = " + masterValue;

                    logger.Debug("Cache miss. Cannot use cache for {0} because objects have been precommitted.", cacheKey);
                    SoodaStatistics.Global.RegisterCollectionCacheMiss();
                    transaction.Statistics.RegisterCollectionCacheMiss();
                }
            }

            IEnumerable keysCollection = transaction.Cache.LoadCollection(cacheKey);
            if (keysCollection != null)
            {
                SoodaStatistics.Global.RegisterCollectionCacheHit();
                transaction.Statistics.RegisterCollectionCacheHit();
                foreach (object o in keysCollection)
                {
                    SoodaObject obj = _factory.GetRef(transaction, o);
                    // this binds to cache
                    obj.EnsureFieldsInited();
                    InternalAdd(obj);
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
                LoadDataFromReader();
                if (cacheKey != null)
                {
                    TimeSpan expirationTimeout;
                    bool slidingExpiration;

                    IList primaryKeys = Sooda.Caching.CacheUtils.ConvertSoodaObjectListToKeyList(itemsArray);

                    if (transaction.CachingPolicy.GetExpirationTimeout(
                        relationInfo, _classInfo, primaryKeys.Count, out expirationTimeout, out slidingExpiration))
                    {
                        transaction.Cache.StoreCollection(cacheKey,
                            _classInfo.GetRootClass().Name,
                            primaryKeys,
                            new string[] { relationInfo.Name },
                            true,
                            expirationTimeout,
                            slidingExpiration
                            );
                    }
                }
            }

            SoodaRelationTable rel = GetSoodaRelationTable();
            rel.OnTupleChanged += new SoodaRelationTupleChanged(this.OnTupleChanged);
            if (rel.TupleCount != 0)
            {
                SoodaRelationTable.Tuple[] tuples = rel.Tuples;
                int count = rel.TupleCount;

                if (masterColumn == 1)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (tuples[i].ref1.Equals(masterValue))
                        {
                            SoodaObject obj = _factory.GetRef(transaction, tuples[i].ref2);
                            if (tuples[i].tupleMode > 0)
                            {
                                InternalAdd(obj);
                            }
                            else
                            {
                                InternalRemove(obj);
                            };
                        };
                    }
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (tuples[i].ref2.Equals(masterValue))
                        {
                            SoodaObject obj = _factory.GetRef(transaction, tuples[i].ref1);
                            if (tuples[i].tupleMode > 0)
                            {
                                InternalAdd(obj);
                            }
                            else
                            {
                                InternalRemove(obj);
                            };
                        };
                    }
                }
            };
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
            throw new NotSupportedException();
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        int IList.Add(object o)
        {
            return Add((SoodaObject)o);
        }

        void IList.Remove(object o)
        {
            Remove((SoodaObject)o);
        }

        bool IList.Contains(object o)
        {
            return Contains((SoodaObject)o);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public int Count
        {
            get { return this.Length; }
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

        public ISoodaObjectList Filter(SoodaWhereClause whereClause)
        {
            return new SoodaObjectListSnapshot(this, whereClause);
        }

        public ISoodaObjectList Filter(SoqlBooleanExpression filterExpression)
        {
            return new SoodaObjectListSnapshot(this, filterExpression);
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
