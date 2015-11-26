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

using Sooda.Logging;
using Sooda.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Sooda.ObjectMapper
{
    public class SoodaObjectManyToManyCollection : SoodaObjectCollectionBase, ISoodaObjectList
    {
        static readonly Logger logger = LogManager.GetLogger("Sooda.ManyToManyCollection");
        protected readonly int masterColumn;
        protected readonly object masterValue;
        protected readonly Type relationType;
        protected readonly Sooda.Schema.RelationInfo relationInfo;
        SoodaRelationTable relationTable = null;
        readonly ISoodaObjectFactory _factory;

        public SoodaObjectManyToManyCollection(SoodaTransaction transaction, int masterColumn, object masterValue, Type relationType, Sooda.Schema.RelationInfo relationInfo)
            : base(transaction, masterColumn == 0 ? relationInfo.GetRef1ClassInfo() : relationInfo.GetRef2ClassInfo())
        {
            this.relationInfo = relationInfo;
            this.masterValue = masterValue;
            this.masterColumn = masterColumn;
            this.relationType = relationType;

            _factory = transaction.GetFactory(classInfo);
        }

        public override int Add(object obj)
        {
            SoodaObject so = (SoodaObject) obj;
            object pk = so.GetPrimaryKeyValue();
            SoodaRelationTable rel = this.GetSoodaRelationTable();
            if (masterColumn == 0)
                rel.Add(pk, this.masterValue);
            else
                rel.Add(this.masterValue, pk);
            return this.InternalAdd(so);
        }

        public override void Remove(object obj)
        {
            SoodaObject so = (SoodaObject) obj;
            object pk = so.GetPrimaryKeyValue();
            SoodaRelationTable rel = this.GetSoodaRelationTable();
            if (masterColumn == 0)
                rel.Remove(pk, this.masterValue);
            else
                rel.Remove(this.masterValue, pk);
            this.InternalRemove(so);
        }

        public override bool Contains(object obj)
        {
            return InternalContains((SoodaObject) obj);
        }

        protected int InternalAdd(SoodaObject obj)
        {
            if (itemsArray == null)
                return -1;
            if (!items.ContainsKey(obj))
            {
                items.Add(obj, itemsArray.Count);
                itemsArray.Add(obj);
            }
            return -1;
        }

        protected void InternalRemove(SoodaObject obj)
        {
            if (itemsArray == null)
                return;

            int pos;
            if (!items.TryGetValue(obj, out pos))
                return;

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
            if (obj == null)
                return false;

            if (itemsArray == null)
                LoadData();
            return items.ContainsKey(obj);
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
            }
            return relationTable;
        }

        void OnTupleChanged(object sender, SoodaRelationTupleChangedArgs args)
        {
            if (!masterValue.Equals(masterColumn == 0 ? args.Right : args.Left))
                return;

            SoodaObject obj = _factory.GetRef(transaction, masterColumn == 0 ? args.Left : args.Right);

            if (args.Mode == 1)
                InternalAdd(obj);
            else if (args.Mode == -1)
                InternalRemove(obj);
        }

        protected override void LoadData()
        {
            bool useCache = false; //transaction.CachingPolicy.ShouldCacheRelation(relationInfo, classInfo);
            string cacheKey = null;

            items = new Dictionary<SoodaObject, int>();
            itemsArray = new List<SoodaObject>();

            if (useCache)
            {
                if (!transaction.HasBeenPrecommitted(relationInfo) && !transaction.HasBeenPrecommitted(classInfo))
                {
                    cacheKey = relationInfo.Name + " where " + relationInfo.Table.Fields[1 - masterColumn].Name + " = " + masterValue;
                }
                else
                {
                    logger.Debug("Cache miss. Cannot use cache for {0} where {1} = {2} because objects have been precommitted.", relationInfo.Name, relationInfo.Table.Fields[1 - masterColumn].Name, masterValue);
                    SoodaStatistics.Global.RegisterCollectionCacheMiss();
                    transaction.Statistics.RegisterCollectionCacheMiss();
                }
            }

            IEnumerable keysCollection = transaction.LoadCollectionFromCache(cacheKey, logger);
            if (keysCollection != null)
            {
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
                LoadDataFromReader();
                if (cacheKey != null)
                {
                    TimeSpan expirationTimeout;
                    bool slidingExpiration;

                    if (transaction.CachingPolicy.GetExpirationTimeout(
                        relationInfo, classInfo, itemsArray.Count, out expirationTimeout, out slidingExpiration))
                    {
                        transaction.StoreCollectionInCache(cacheKey, classInfo, itemsArray, new string[] { relationInfo.Name }, true, expirationTimeout, slidingExpiration);
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
                            }
                        }
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
                            }
                        }
                    }
                }
            }
        }
    }
}
