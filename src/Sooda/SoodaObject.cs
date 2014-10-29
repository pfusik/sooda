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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Collections.Specialized;

using Sooda.Schema;
using Sooda.ObjectMapper;

using Sooda.QL;

using Sooda.Logging;
using Sooda.Caching;
using Sooda.Collections;

namespace Sooda
{
    public class SoodaObject
#if DOTNET4
        : System.Dynamic.DynamicObject
#endif
    {
        private static readonly Logger logger = LogManager.GetLogger("Sooda.Object");

        // instance fields - initialized in InitRawObject()

        private byte[] _fieldIsDirty;
        internal SoodaObjectFieldValues _fieldValues;
        private int _dataLoadedMask;
        private SoodaTransaction _transaction;
        private SoodaObjectFlags _flags;
        private object _primaryKeyValue;

        public bool AreFieldUpdateTriggersEnabled()
        {
            return (_flags & SoodaObjectFlags.DisableFieldTriggers) == 0;
        }

        public bool EnableFieldUpdateTriggers()
        {
            return EnableFieldUpdateTriggers(true);
        }

        public bool DisableFieldUpdateTriggers()
        {
            return EnableFieldUpdateTriggers(false);
        }

        public bool EnableFieldUpdateTriggers(bool enable)
        {
            bool oldValue = AreFieldUpdateTriggersEnabled();

            if (!enable)
                _flags |= SoodaObjectFlags.DisableFieldTriggers;
            else
                _flags &= ~SoodaObjectFlags.DisableFieldTriggers;
            return oldValue;
        }

        public bool AreObjectTriggersEnabled()
        {
            return (_flags & SoodaObjectFlags.DisableObjectTriggers) == 0;
        }

        public bool EnableObjectTriggers()
        {
            return EnableObjectTriggers(true);
        }

        public bool DisableObjectTriggers()
        {
            return EnableObjectTriggers(false);
        }

        public bool EnableObjectTriggers(bool enable)
        {
            bool oldValue = AreObjectTriggersEnabled();

            if (!enable)
                _flags |= SoodaObjectFlags.DisableObjectTriggers;
            else
                _flags &= ~SoodaObjectFlags.DisableObjectTriggers;
            return oldValue;
        }


        private bool InsertMode
        {
            get
            {
                return (_flags & SoodaObjectFlags.InsertMode) != 0;
            }
            set
            {
                if (value)
                {
                    _flags |= SoodaObjectFlags.InsertMode;
                    SetObjectDirty();
                }
                else
                    _flags &= ~SoodaObjectFlags.InsertMode;
            }
        }

        internal bool VisitedOnCommit
        {
            get
            {
                return (_flags & SoodaObjectFlags.VisitedOnCommit) != 0;
            }
            set
            {
                if (value)
                    _flags |= SoodaObjectFlags.VisitedOnCommit;
                else
                    _flags &= ~SoodaObjectFlags.VisitedOnCommit;
            }
        }

        bool WrittenIntoDatabase
        {
            get
            {
                return (_flags & SoodaObjectFlags.WrittenIntoDatabase) != 0;
            }
            set
            {
                if (value)
                    _flags |= SoodaObjectFlags.WrittenIntoDatabase;
                else
                    _flags &= ~SoodaObjectFlags.WrittenIntoDatabase;
            }
        }

        bool PostCommitForced
        {
            get
            {
                return (_flags & SoodaObjectFlags.ForcePostCommit) != 0;
            }
            set
            {
                if (value)
                    _flags |= SoodaObjectFlags.ForcePostCommit;
                else
                    _flags &= ~SoodaObjectFlags.ForcePostCommit;
            }
        }

        public void ForcePostCommit()
        {
            _flags |= SoodaObjectFlags.ForcePostCommit;
        }

        internal bool InsertedIntoDatabase
        {
            get { return (_flags & SoodaObjectFlags.InsertedIntoDatabase) != 0; }
            set
            {
                if (value)
                    _flags |= SoodaObjectFlags.InsertedIntoDatabase;
                else
                    _flags &= ~SoodaObjectFlags.InsertedIntoDatabase;
            }
        }

        bool FromCache
        {
            get { return (_flags & SoodaObjectFlags.FromCache) != 0; }
            set
            {
                if (value)
                    _flags |= SoodaObjectFlags.FromCache;
                else
                    _flags &= ~SoodaObjectFlags.FromCache;
            }
        }

        SoodaCacheEntry GetCacheEntry()
        {
            return new SoodaCacheEntry(_dataLoadedMask, _fieldValues);
        }

        internal bool DeleteMarker
        {
            get
            {
                return (_flags & SoodaObjectFlags.MarkedForDeletion) != 0;
            }
            set
            {
                if (value)
                    _flags = _flags | SoodaObjectFlags.MarkedForDeletion;
                else
                    _flags = (_flags & ~SoodaObjectFlags.MarkedForDeletion);
            }
        }

        internal void SetInsertMode()
        {
            this.InsertMode = true;
            SetAllDataLoaded();
        }

        public bool IsInsertMode()
        {
            return this.InsertMode;
        }

        ~SoodaObject()
        {
            // logger.Trace("Finalizer for {0}", GetObjectKeyString());
        }

        protected SoodaObject(SoodaConstructor c)
        {
            GC.SuppressFinalize(this);
        }

        protected SoodaObject(SoodaTransaction tran)
        {
            GC.SuppressFinalize(this);
            tran.Statistics.RegisterObjectInsert();
            SoodaStatistics.Global.RegisterObjectInsert();

            InitRawObject(tran);
            InsertMode = true;
            SetAllDataLoaded();
            if (GetClassInfo().SubclassSelectorValue != null)
            {
                DisableFieldUpdateTriggers();
                Sooda.Schema.FieldInfo selectorField = GetClassInfo().SubclassSelectorField;
                SetPlainFieldValue(0, selectorField.Name, selectorField.ClassUnifiedOrdinal, GetClassInfo().SubclassSelectorValue, null, null);
                EnableFieldUpdateTriggers();
            }
        }

        private void PropagatePrimaryKeyToFields()
        {
            Sooda.Schema.FieldInfo[] primaryKeys = GetClassInfo().GetPrimaryKeyFields();
            SoodaTuple tuple = _primaryKeyValue as SoodaTuple;

            if (tuple != null)
            {
                if (tuple.Length != primaryKeys.Length)
                    throw new InvalidOperationException("Primary key tuple length doesn't match the expected length");

                for (int i = 0; i < primaryKeys.Length; ++i)
                {
                    _fieldValues.SetFieldValue(primaryKeys[i].ClassUnifiedOrdinal, tuple.GetValue(i));
                }
            }
            else
            {
                if (primaryKeys.Length != 1)
                    throw new InvalidOperationException("Primary key is not a scalar.");

                // scalar
                _fieldValues.SetFieldValue(primaryKeys[0].ClassUnifiedOrdinal, _primaryKeyValue);
            }
        }

        protected virtual SoodaObjectFieldValues InitFieldValues(int fieldCount, string[] fieldNames)
        {
            return new SoodaObjectArrayFieldValues(fieldCount);
        }

        private void InitFieldData(bool justLoading)
        {
            if (!InsertMode && GetTransaction().CachingPolicy.ShouldCacheObject(this))
            {
                SoodaCacheEntry cachedData = GetTransaction().Cache.Find(GetClassInfo().GetRootClass().Name, _primaryKeyValue);
                if (cachedData != null)
                {
                    GetTransaction().Statistics.RegisterCacheHit();
                    SoodaStatistics.Global.RegisterCacheHit();

                    if (logger.IsTraceEnabled)
                    {
                        logger.Trace("Initializing object {0}({1}) from cache.", this.GetType().Name, _primaryKeyValue);
                    }
                    _fieldValues = cachedData.Data;
                    _dataLoadedMask = cachedData.DataLoadedMask;
                    FromCache = true;
                    return;
                }

                // we don't register a cache miss when we're just loading
                if (!justLoading)
                {
                    GetTransaction().Statistics.RegisterCacheMiss();
                    SoodaStatistics.Global.RegisterCacheMiss();
                    if (logger.IsTraceEnabled)
                    {
                        logger.Trace("Cache miss. Object {0}({1}) not found in cache.", this.GetType().Name, _primaryKeyValue);
                    }
                }
            }

            ClassInfo ci = GetClassInfo();

            int fieldCount = ci.UnifiedFields.Count;
            _fieldValues = InitFieldValues(fieldCount, ci.OrderedFieldNames);
            GetTransaction().Statistics.RegisterFieldsInited();
            SoodaStatistics.Global.RegisterFieldsInited();

            // primary key was set before the fields - propagate the value
            // back to the field(s)
            if (_primaryKeyValue != null)
            {
                PropagatePrimaryKeyToFields();
            }

            if (InsertMode)
            {
                SetDefaultNotNullValues();
            }
        }

        private void SetDefaultNotNullValues()
        {
            ClassInfo ci = GetClassInfo();

            for (int i = 0; i < _fieldValues.Length; ++i)
            {
                if (ci.UnifiedFields[i].IsPrimaryKey || ci.UnifiedFields[i].ReferencedClass != null)
                    continue;

                SoodaFieldHandler handler = GetFieldHandler(i);
                if (!handler.IsNullable)
                    _fieldValues.SetFieldValue(i, handler.ZeroValue());
            }
        }

        void SetUpdateMode(object primaryKeyValue)
        {
            InsertMode = false;
            SetPrimaryKeyValue(primaryKeyValue);
            SetAllDataNotLoaded();

        }

        public SoodaTransaction GetTransaction()
        {
            return _transaction;
        }


        protected virtual void BeforeObjectInsert() { }
        protected virtual void BeforeObjectUpdate() { }
        protected virtual void BeforeObjectDelete() { }
        protected virtual void AfterObjectInsert() { }
        protected virtual void AfterObjectUpdate() { }
        protected virtual void AfterObjectDelete() { }

        protected virtual void BeforeFieldUpdate(string name, object oldVal, object newVal) { }
        protected virtual void AfterFieldUpdate(string name, object oldVal, object newVal) { }

        public void MarkForDelete()
        {
            MarkForDelete(true, true);
        }

        public void MarkForDelete(bool delete, bool recurse)
        {
            try
            {
                int oldDeletePosition = GetTransaction().DeletedObjects.Count;
                MarkForDelete(delete, recurse, true);
                int newDeletePosition = GetTransaction().DeletedObjects.Count;
                if (newDeletePosition != oldDeletePosition)
                {
                    GetTransaction().SaveObjectChanges(true, null);
                    GetTransaction()._savingObjects = true;

                    foreach (SoodaDataSource source in GetTransaction()._dataSources)
                    {
                        source.BeginSaveChanges();
                    }

                    List<SoodaObject> deleted = GetTransaction().DeletedObjects;

                    for (int i = oldDeletePosition; i < newDeletePosition; ++i)
                    {
                        // logger.Debug("Actually deleting {0}", GetTransaction().DeletedObjects[i].GetObjectKeyString());
                        SoodaObject o = deleted[i];
                        o.CommitObjectChanges();
                        o.SetObjectDirty();
                        GetTransaction().MarkPrecommitted(o);
                    }
                    foreach (SoodaDataSource source in GetTransaction()._dataSources)
                    {
                        source.FinishSaveChanges();
                    }
                    for (int i = oldDeletePosition; i < newDeletePosition; ++i)
                    {
                        deleted[i].AfterObjectDelete();
                    }
                }
            }
            finally
            {
                GetTransaction()._savingObjects = false;
            }
        }

        public void MarkForDelete(bool delete, bool recurse, bool savingChanges)
        {
            if (DeleteMarker != delete)
            {
                BeforeObjectDelete();
                DeleteMarker = delete;

                if (recurse)
                {
                    if (logger.IsTraceEnabled)
                    {
                        logger.Trace("Marking outer references of {0} for delete...", GetObjectKeyString());
                    }
                    for (ClassInfo ci = this.GetClassInfo(); ci != null; ci = ci.InheritsFromClass)
                    {
                        foreach (Sooda.Schema.FieldInfo fi in ci.OuterReferences)
                        {
                            logger.Trace("{0} Delete action: {1}", fi, fi.DeleteAction);
                            if (fi.DeleteAction == DeleteAction.Nothing)
                                continue;

                            ISoodaObjectFactory factory = GetTransaction().GetFactory(fi.ParentClass);

                            SoqlBooleanExpression whereExpression = Soql.FieldEquals(fi.Name, this);
                            SoodaWhereClause whereClause = new SoodaWhereClause(whereExpression);
                            // logger.Debug("loading list where: {0}", whereExpression);
                            IList referencingList = factory.GetList(GetTransaction(), whereClause, null, SoodaSnapshotOptions.KeysOnly);
                            if (fi.DeleteAction == DeleteAction.Cascade)
                            {
                                foreach (SoodaObject o in referencingList)
                                {
                                    o.MarkForDelete(delete, recurse, savingChanges);
                                }
                            }
                            if (fi.DeleteAction == DeleteAction.Nullify)
                            {
                                PropertyInfo pi = GetTransaction().GetFactory(fi.ParentClass).TheType.GetProperty(fi.Name);

                                foreach (SoodaObject o in referencingList)
                                {
                                    pi.SetValue(o, null, null);
                                }
                            }
                        }
                    }
                }
                GetTransaction().DeletedObjects.Add(this);
            }
        }

        public bool IsMarkedForDelete()
        {
            return DeleteMarker;
        }

        internal object GetFieldValue(int fieldNumber)
        {
            return _fieldValues.GetBoxedFieldValue(fieldNumber);
        }

        public bool IsFieldDirty(int fieldNumber)
        {
            if (_fieldIsDirty == null)
                return false;

            int slotNumber = fieldNumber >> 3;
            int bitNumber = fieldNumber & 7;

            return (_fieldIsDirty[slotNumber] & (1 << bitNumber)) != 0;
        }

        public void SetFieldDirty(int fieldNumber, bool dirty)
        {
            if (_fieldIsDirty == null)
            {
                int fieldCount = GetClassInfo().UnifiedFields.Count;
                _fieldIsDirty = new byte[(fieldCount + 7) >> 3];
            }

            int slotNumber = fieldNumber >> 3;
            int bitNumber = fieldNumber & 7;

            if (dirty)
            {
                _fieldIsDirty[slotNumber] |= (byte) (1 << bitNumber);
            }
            else
            {
                _fieldIsDirty[slotNumber] &= (byte) ~(1 << bitNumber);
            }
        }

        protected virtual SoodaFieldHandler GetFieldHandler(int ordinal)
        {
            throw new NotImplementedException();
        }

        internal void CheckForNulls()
        {
            EnsureFieldsInited();
            if (IsInsertMode())
            {
                ClassInfo ci = GetClassInfo();
                for (int i = 0; i < _fieldValues.Length; ++i)
                {
                    if (!ci.UnifiedFields[i].IsNullable && _fieldValues.IsNull(i))
                        FieldCannotBeNull(ci.UnifiedFields[i].Name);
                }
            }
        }
        protected internal virtual void CheckAssertions() { }
        void FieldCannotBeNull(string fieldName)
        {
            throw new SoodaException("Field '" + fieldName + "' cannot be null on commit in " + GetObjectKeyString());
        }

        public bool IsObjectDirty()
        {
            return (_flags & SoodaObjectFlags.Dirty) != 0;
        }

        internal void SetObjectDirty()
        {
            if (!IsObjectDirty())
            {
                EnsureFieldsInited();
                _flags |= SoodaObjectFlags.Dirty;
                GetTransaction().RegisterDirtyObject(this);
            }
            _flags &= ~SoodaObjectFlags.WrittenIntoDatabase;
        }

        internal void ResetObjectDirty()
        {
            _flags &= ~(SoodaObjectFlags.Dirty | SoodaObjectFlags.WrittenIntoDatabase);
        }

        public virtual Sooda.Schema.ClassInfo GetClassInfo()
        {
            throw new NotImplementedException();
        }

        public string GetObjectKeyString()
        {
            return String.Format("{0}[{1}]", GetClassInfo().Name, GetPrimaryKeyValue());
        }

        public object GetPrimaryKeyValue()
        {
            return _primaryKeyValue;
        }

        protected void SetPrimaryKeySubValue(object keyValue, int valueOrdinal, int totalValues)
        {
            SoodaTuple tuple = (SoodaTuple) _primaryKeyValue;
            if (tuple == null)
                _primaryKeyValue = tuple = new SoodaTuple(totalValues);
            tuple.SetValue(valueOrdinal, keyValue);
            if (tuple.IsAllNotNull())
            {
                if (_fieldValues != null)
                    PropagatePrimaryKeyToFields();
                if (IsRegisteredInTransaction())
                    throw new SoodaException("Cannot set primary key value more than once.");
                RegisterObjectInTransaction();
            }
        }

        protected internal void SetPrimaryKeyValue(object keyValue)
        {
            if (_primaryKeyValue == null)
            {
                _primaryKeyValue = keyValue;
                if (_fieldValues != null)
                    PropagatePrimaryKeyToFields();
                RegisterObjectInTransaction();
            }
            else if (IsRegisteredInTransaction())
            {
                throw new SoodaException("Cannot set primary key value more than once.");
            }
        }

        protected internal virtual void AfterDeserialize() { }
        protected virtual void InitNewObject() { }

        #region 'Loaded' state management

        bool IsAllDataLoaded()
        {
            // 2^N-1 has exactly N lower bits set to 1
            return _dataLoadedMask == (1 << GetClassInfo().UnifiedTables.Count) - 1;
        }

        void SetAllDataLoaded()
        {
            // 2^N-1 has exactly N lower bits set to 1
            _dataLoadedMask = (1 << GetClassInfo().UnifiedTables.Count) - 1;
        }

        void SetAllDataNotLoaded()
        {
            _dataLoadedMask = 0;
        }

        bool IsDataLoaded(int tableNumber)
        {
            return (_dataLoadedMask & (1 << tableNumber)) != 0;
        }

        void SetDataLoaded(int tableNumber)
        {
            _dataLoadedMask |= (1 << tableNumber);
        }

        #endregion

        private int LoadDataFromRecord(System.Data.IDataRecord reader, int firstColumnIndex, TableInfo[] tables, int tableIndex)
        {
            int recordPos = firstColumnIndex;
            bool first = true;

            EnsureFieldsInited(true);

            int i;
            int oldDataLoadedMask = _dataLoadedMask;

            for (i = tableIndex; i < tables.Length; ++i)
            {
                TableInfo table = tables[i];
                // logger.Debug("Loading data from table {0}. Number of fields: {1} Record pos: {2} Table index {3}.", table.NameToken, table.Fields.Count, recordPos, tableIndex);

                if (table.OrdinalInClass == 0 && !first)
                {
                    // logger.Trace("Found table 0 of another object. Exiting.");
                    break;
                }

                foreach (Sooda.Schema.FieldInfo field in table.Fields)
                {
                    // don't load primary keys
                    if (!field.IsPrimaryKey)
                    {
                        try
                        {
                            int ordinal = field.ClassUnifiedOrdinal;
                            if (!IsFieldDirty(ordinal))
                            {
                                object value = reader.IsDBNull(recordPos) ? null : GetFieldHandler(ordinal).RawRead(reader, recordPos);
                                _fieldValues.SetFieldValue(ordinal, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error while reading field {0}.{1}: {2}", table.NameToken, field.Name, ex);
                            throw;
                        }
                    }
                    recordPos++;
                }

                SetDataLoaded(table.OrdinalInClass);
                first = false;
            }
            if (!IsObjectDirty() && (!FromCache || _dataLoadedMask != oldDataLoadedMask) && GetTransaction().CachingPolicy.ShouldCacheObject(this))
            {
                TimeSpan expirationTimeout;
                bool slidingExpiration;

                if (GetTransaction().CachingPolicy.GetExpirationTimeout(this, out expirationTimeout, out slidingExpiration))
                {
                    GetTransaction().Cache.Add(GetClassInfo().GetRootClass().Name, GetPrimaryKeyValue(), GetCacheEntry(), expirationTimeout, slidingExpiration);
                    FromCache = true;
                }
            }

            // if we've started with a first table and there are more to be processed
            if (tableIndex == 0 && i != tables.Length)
            {
                // logger.Trace("Materializing extra objects...");
                for (; i < tables.Length; ++i)
                {
                    TableInfo table = tables[i];
                    if (table.OrdinalInClass == 0)
                    {
                        GetTransaction().Statistics.RegisterExtraMaterialization();
                        SoodaStatistics.Global.RegisterExtraMaterialization();

                        // logger.Trace("Materializing {0} at {1}", tables[i].NameToken, recordPos);

                        int pkOrdinal = table.OwnerClass.GetFirstPrimaryKeyField().OrdinalInTable;
                        if (reader.IsDBNull(recordPos + pkOrdinal))
                        {
                            // logger.Trace("Object is null. Skipping.");
                        }
                        else
                        {
                            ISoodaObjectFactory factory = GetTransaction().GetFactory(table.OwnerClass);
                            SoodaObject.GetRefFromRecordHelper(GetTransaction(), factory, reader, recordPos, tables, i);
                        }
                    }
                    else
                    {
                        // TODO - can this be safely called?
                    }
                    recordPos += table.Fields.Count;
                }
                // logger.Trace("Finished materializing extra objects.");
            }

            return tables.Length;
        }

        internal void EnsureFieldsInited()
        {
            EnsureFieldsInited(false);
        }

        void EnsureFieldsInited(bool justLoading)
        {
            if (_fieldValues == null)
                InitFieldData(justLoading);
        }

        internal void EnsureDataLoaded(int tableNumber)
        {
            if (!IsDataLoaded(tableNumber))
            {
                EnsureFieldsInited();
                LoadData(tableNumber);
            }
            else if (InsertMode)
            {
                EnsureFieldsInited();
            }
        }

        internal void LoadAllData()
        {
            // TODO - OPTIMIZE: LOAD DATA FROM ALL TABLES IN A SINGLE QUERY

            for (int i = 0; i < GetClassInfo().UnifiedTables.Count; ++i)
            {
                if (!IsDataLoaded(i))
                {
                    LoadData(i);
                }
            }
        }

        private void LoadData(int tableNumber)
        {
            LoadDataWithKey(GetPrimaryKeyValue(), tableNumber);
        }

        protected void LoadReadOnlyObject(object keyVal)
        {
            InsertMode = false;
            SetPrimaryKeyValue(keyVal);
            // #warning FIX ME
            LoadDataWithKey(keyVal, 0);
        }

        protected void LoadDataWithKey(object keyVal, int tableNumber)
        {
            EnsureFieldsInited();

            if (IsDataLoaded(tableNumber))
                return;

            if (logger.IsTraceEnabled)
            {
                // logger.Trace("Loading data for {0}({1}) from table #{2}", GetClassInfo().Name, keyVal, tableNumber);
            };

            try
            {
                SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());
                TableInfo[] loadedTables;

                using (IDataReader record = ds.LoadObjectTable(this, keyVal, tableNumber, out loadedTables))
                {
                    if (record == null)
                    {
                        logger.Error("LoadObjectTable() failed for {0}", GetObjectKeyString());
                        GetTransaction().UnregisterObject(this);
                        throw new SoodaObjectNotFoundException(String.Format("Object {0} not found in the database", GetObjectKeyString()));
                    }
                    /*
                    for (int i = 0; i < loadedTables.Length; ++i)
                    {
                        logger.Trace("loadedTables[{0}] = {1}", i, loadedTables[i].NameToken);
                    }
                    */
                    LoadDataFromRecord(record, 0, loadedTables, 0);
                    record.Close();
                }
            }
            catch (Exception ex)
            {
                GetTransaction().UnregisterObject(this);
                logger.Error("Exception in LoadDataWithKey({0}): {1}", GetObjectKeyString(), ex);
                throw ex;
            }
        }

        protected void RegisterObjectInTransaction()
        {
            GetTransaction().RegisterObject(this);
        }

        protected bool IsRegisteredInTransaction()
        {
            return GetTransaction().IsRegistered(this);
        }

        void SaveOuterReferences()
        {
            List<KeyValuePair<int, SoodaObject>> brokenReferences = null;

            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields)
            {
                if (fi.ReferencedClass == null)
                    continue;

                object v = _fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal);
                if (v != null)
                {
                    ISoodaObjectFactory factory = GetTransaction().GetFactory(fi.ReferencedClass);
                    SoodaObject obj = factory.TryGet(GetTransaction(), v);

                    if (obj != null && obj != this && obj.IsInsertMode() && !obj.InsertedIntoDatabase)
                    {
                        if (obj.VisitedOnCommit && !obj.WrittenIntoDatabase)
                        {
                            // cyclic reference
                            if (!fi.IsNullable)
                                throw new Exception("Cyclic reference between " + GetObjectKeyString() + " and " + obj.GetObjectKeyString());
                            if (brokenReferences == null)
                            {
                                CopyOnWrite();
                                brokenReferences = new List<KeyValuePair<int, SoodaObject>>();
                            }
                            brokenReferences.Add(new KeyValuePair<int, SoodaObject>(fi.ClassUnifiedOrdinal, obj));
                            _fieldValues.SetFieldValue(fi.ClassUnifiedOrdinal, null);
                        }
                        else
                        {
                            obj.SaveObjectChanges();
                        }
                    }
                }
            }

            if (brokenReferences != null)
            {
                // insert this object without the cyclic references
                CommitObjectChanges();

                foreach (KeyValuePair<int, SoodaObject> pair in brokenReferences)
                {
                    int ordinal = pair.Key;
                    SoodaObject obj = pair.Value;
                    // insert referenced object
                    obj.SaveObjectChanges();
                    // restore reference
                    _fieldValues.SetFieldValue(ordinal, obj.GetPrimaryKeyValue());
                }
            }
        }

        internal void SaveObjectChanges()
        {
            VisitedOnCommit = true;
            if (WrittenIntoDatabase)
                return;

            if (IsObjectDirty())
            {
                SaveOuterReferences();
            }

            if ((IsObjectDirty() || IsInsertMode()) && !WrittenIntoDatabase)
            {
                // deletes are performed in a separate pass
                if (!IsMarkedForDelete())
                {
                    CommitObjectChanges();
                }
                WrittenIntoDatabase = true;
            }
            else if (PostCommitForced)
            {
                GetTransaction().AddToPostCommitQueue(this);
            }
        }

        internal void CommitObjectChanges()
        {
            SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());

            try
            {
                EnsureFieldsInited();
                ds.SaveObjectChanges(this, GetTransaction().IsPrecommit);
            }
            catch (Exception e)
            {
                throw new SoodaDatabaseException("Cannot save object to the database " + e.Message, e);
            }

            //GetTransaction().AddToPostCommitQueue(this);
        }

        internal void InvalidateCacheAfterCommit()
        {
            SoodaCacheInvalidateReason reason;
            if (IsMarkedForDelete())
                reason = SoodaCacheInvalidateReason.Deleted;
            else if (IsInsertMode())
                reason = SoodaCacheInvalidateReason.Inserted;
            else
                reason = SoodaCacheInvalidateReason.Updated;

            GetTransaction().Cache.Invalidate(GetClassInfo().GetRootClass().Name, GetPrimaryKeyValue(), reason);
        }

        internal void PostCommit()
        {
            if (IsInsertMode())
            {
                if (AreObjectTriggersEnabled())
                    AfterObjectInsert();
                InsertMode = false;
            }
            else
            {
                if (AreObjectTriggersEnabled())
                    AfterObjectUpdate();
            }
        }

        internal void CallBeforeCommitEvent()
        {
            if (AreObjectTriggersEnabled())
            {
                if (IsInsertMode())
                    BeforeObjectInsert();
                else
                    BeforeObjectUpdate();
            }
            GetTransaction().AddToPostCommitQueue(this);
        }

        private void SerializePrimaryKey(XmlWriter xw)
        {
            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().GetPrimaryKeyFields())
            {
                int ordinal = fi.ClassUnifiedOrdinal;
                xw.WriteStartElement("key");
                xw.WriteAttributeString("ordinal", ordinal.ToString());
                GetFieldHandler(ordinal).Serialize(_fieldValues.GetBoxedFieldValue(ordinal), xw);
                xw.WriteEndElement();
            }
        }

        // create an empty object just to make sure that the deserialization
        // will find it before any references are used.
        //
        internal void PreSerialize(XmlWriter xw, SoodaSerializeOptions options)
        {
            if (!IsInsertMode() && !IsMarkedForDelete())
                return;

            xw.WriteStartElement("object");
            xw.WriteAttributeString("mode", IsMarkedForDelete() ? "update" : "insert");
            xw.WriteAttributeString("class", GetClassInfo().Name);
            if (IsMarkedForDelete())
                xw.WriteAttributeString("delete", "true");
            SerializePrimaryKey(xw);
            xw.WriteEndElement();
        }

        internal void Serialize(XmlWriter xw, SoodaSerializeOptions options)
        {
            if (IsMarkedForDelete())
                return;

            xw.WriteStartElement("object");
            xw.WriteAttributeString("mode", "update");
            xw.WriteAttributeString("class", GetClassInfo().Name);
            if (!IsObjectDirty())
                xw.WriteAttributeString("dirty", "false");

            if (!AreObjectTriggersEnabled())
                xw.WriteAttributeString("disableobjecttriggers", "true");

            if (PostCommitForced)
                xw.WriteAttributeString("forcepostcommit", "true");

            logger.Trace("Serializing {0}...", GetObjectKeyString());
            EnsureFieldsInited();

            if ((options & SoodaSerializeOptions.IncludeNonDirtyFields) != 0 && !IsAllDataLoaded())
                LoadAllData();

            SerializePrimaryKey(xw);

            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields)
            {
                if (fi.IsPrimaryKey)
                    continue;

                int ordinal = fi.ClassUnifiedOrdinal;
                bool dirty = IsFieldDirty(ordinal);
                if (dirty || (options & SoodaSerializeOptions.IncludeNonDirtyFields) != 0)
                {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", fi.Name);
                    GetFieldHandler(ordinal).Serialize(_fieldValues.GetBoxedFieldValue(ordinal), xw);
                    if (!dirty)
                        xw.WriteAttributeString("dirty", "false");
                    xw.WriteEndElement();
                }
            }

            if ((options & SoodaSerializeOptions.IncludeDebugInfo) != 0)
            {
                xw.WriteStartElement("debug");
                xw.WriteAttributeString("transaction", (_transaction != null) ? "notnull" : "null");
                xw.WriteAttributeString("objectDirty", IsObjectDirty() ? "true" : "false");
                xw.WriteAttributeString("dataLoaded", IsAllDataLoaded() ? "true" : "false");
                xw.WriteAttributeString("disableTriggers", AreFieldUpdateTriggersEnabled() ? "false" : "true");
                xw.WriteAttributeString("disableObjectTriggers", AreObjectTriggersEnabled() ? "false" : "true");
                xw.WriteEndElement();
            }

            NameValueCollection persistentValues = GetTransaction().GetPersistentValues(this);
            if (persistentValues != null)
            {
                foreach (string s in persistentValues.AllKeys)
                {
                    xw.WriteStartElement("persistent");
                    xw.WriteAttributeString("name", s);
                    xw.WriteAttributeString("value", persistentValues[s]);
                    xw.WriteEndElement();
                }
            }

            xw.WriteEndElement();
        }

        internal void DeserializePersistentField(XmlReader reader)
        {
            string name = reader.GetAttribute("name");
            string value = reader.GetAttribute("value");

            SetTransactionPersistentValue(name, value);
        }

        internal void DeserializeField(XmlReader reader)
        {
            string name = reader.GetAttribute("name");

            if (reader.GetAttribute("dirty") != "false")
            {
                EnsureFieldsInited();
                CopyOnWrite();

                int fieldOrdinal = GetFieldInfo(name).ClassUnifiedOrdinal;
                SoodaFieldHandler field = GetFieldHandler(fieldOrdinal);
                object val = field.Deserialize(reader);

                // Console.WriteLine("Deserializing field: {0}", name);

                PropertyInfo pi = GetType().GetProperty(name);
                if (pi.PropertyType.IsSubclassOf(typeof(SoodaObject)))
                {
                    if (val != null)
                    {
                        ISoodaObjectFactory fact = GetTransaction().GetFactory(pi.PropertyType);
                        val = fact.GetRef(GetTransaction(), val);
                    }
                    pi.SetValue(this, val, null);
                }
                else
                {
                    // set as raw

                    _fieldValues.SetFieldValue(fieldOrdinal, val);
                    SetFieldDirty(fieldOrdinal, true);
                }

                SetObjectDirty();
            }
            else
            {
                // Console.WriteLine("Not deserializing field: {0}", name);
            }
        }

        void SetFieldValue(int fieldOrdinal, object value)
        {
            CopyOnWrite();
            _fieldValues.SetFieldValue(fieldOrdinal, value);
            SetFieldDirty(fieldOrdinal, true);
            SetObjectDirty();
        }

        internal void SetPlainFieldValue(int tableNumber, string fieldName, int fieldOrdinal, object newValue, SoodaFieldUpdateDelegate before, SoodaFieldUpdateDelegate after)
        {
            EnsureFieldsInited();

            if (AreFieldUpdateTriggersEnabled())
            {
                EnsureDataLoaded(tableNumber);
                try
                {
                    object oldValue = _fieldValues.GetBoxedFieldValue(fieldOrdinal);
                    if (Object.Equals(oldValue, newValue))
                        return;

                    if (before != null)
                        before(oldValue, newValue);
                    SetFieldValue(fieldOrdinal, newValue);
                    if (after != null)
                        after(oldValue, newValue);

                }
                catch (Exception e)
                {
                    throw new Exception("BeforeFieldUpdate raised an exception: ", e);
                }
            }
            else
            {
                // optimization here - we don't even need to load old values from database
                SetFieldValue(fieldOrdinal, newValue);
            }
        }

        internal void SetRefFieldValue(int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, SoodaObject[] refcache, int refCacheOrdinal, ISoodaObjectFactory factory)
        {
            if (newValue != null)
            {
                // transaction check
                if (newValue.GetTransaction() != this.GetTransaction())
                    throw new SoodaException("Attempted to assign object " + newValue.GetObjectKeyString() + " from another transaction to " + this.GetObjectKeyString() + "." + fieldName);
            }

            EnsureFieldsInited();
            EnsureDataLoaded(tableNumber);

            SoodaObject oldValue = null;

            SoodaObjectImpl.GetRefFieldValue(ref oldValue, this, tableNumber, fieldOrdinal, GetTransaction(), factory);
            if (Object.Equals(oldValue, newValue))
                return;
            object[] triggerArgs = new object[] { oldValue, newValue };

            if (AreFieldUpdateTriggersEnabled())
            {
                MethodInfo mi = this.GetType().GetMethod("BeforeFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                if (mi != null)
                    mi.Invoke(this, triggerArgs);
            }
            Sooda.Schema.FieldInfo fieldInfo = GetClassInfo().UnifiedFields[fieldOrdinal];
            StringCollection backRefCollections = GetTransaction().Schema.GetBackRefCollections(fieldInfo);
            if (oldValue != null && backRefCollections != null)
            {
                foreach (string collectionName in backRefCollections)
                {
                    PropertyInfo coll = oldValue.GetType().GetProperty(collectionName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (coll == null)
                        throw new Exception(collectionName + " not found in " + oldValue.GetType().Name + " while setting " + this.GetType().Name + "." + fieldName);
                    ISoodaObjectListInternal listInternal = (ISoodaObjectListInternal)coll.GetValue(oldValue, null);
                    listInternal.InternalRemove(this);
                }
            }
            SetFieldValue(fieldOrdinal, newValue != null ? newValue.GetPrimaryKeyValue() : null);
            refcache[refCacheOrdinal] = null;
            if (newValue != null && backRefCollections != null)
            {
                foreach (string collectionName in backRefCollections)
                {
                    PropertyInfo coll = newValue.GetType().GetProperty(collectionName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (coll == null)
                        throw new Exception(collectionName + " not found in " + newValue.GetType().Name + " while setting " + this.GetType().Name + "." + fieldName);
                    ISoodaObjectListInternal listInternal = (ISoodaObjectListInternal)coll.GetValue(newValue, null);
                    listInternal.InternalAdd(this);
                }
            }
            if (AreFieldUpdateTriggersEnabled())
            {
                MethodInfo mi = this.GetType().GetMethod("AfterFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                if (mi != null)
                    mi.Invoke(this, triggerArgs);
            }
        }

        public object Evaluate(SoqlExpression expr)
        {
            return Evaluate(expr, true);
        }

        class EvaluateContext : ISoqlEvaluateContext
        {
            private SoodaObject _rootObject;

            public EvaluateContext(SoodaObject rootObject)
            {
                _rootObject = rootObject;
            }

            public object GetRootObject()
            {
                return _rootObject;
            }

            public object GetParameter(int position)
            {
                throw new Exception("No parameters allowed in evaluation.");
            }
        }

        public object Evaluate(SoqlExpression expr, bool throwOnError)
        {
            try
            {
                EvaluateContext ec = new EvaluateContext(this);
                return expr.Evaluate(ec);
            }
            catch
            {
                if (throwOnError)
                    throw;
                else
                    return null;
            }
        }

        public object Evaluate(string[] propertyAccessChain, bool throwOnError)
        {
            try
            {
                object currentObject = this;

                for (int i = 0; i < propertyAccessChain.Length && currentObject != null && currentObject is SoodaObject; ++i)
                {
                    PropertyInfo pi = currentObject.GetType().GetProperty(propertyAccessChain[i]);
                    currentObject = pi.GetValue(currentObject, null);
                }
                return currentObject;
            }
            catch
            {
                if (throwOnError)
                    throw;
                else
                    return null;
            }
        }

        public object Evaluate(string propertyAccessChain)
        {
            return Evaluate(propertyAccessChain, true);
        }

        public object Evaluate(string propertyAccessChain, bool throwOnError)
        {
            return Evaluate(propertyAccessChain.Split('.'), throwOnError);
        }

        static ISoodaObjectFactory GetFactoryFromRecord(SoodaTransaction tran, ISoodaObjectFactory factory, IDataRecord record, int firstColumnIndex, object keyValue, bool loadData)
        {
            ClassInfo classInfo = factory.GetClassInfo();
            List<ClassInfo> subclasses = tran.Schema.GetSubclasses(classInfo);
            if (subclasses.Count == 0)
                return factory;

            // more complex case - we have to determine the actual factory to be used for object creation

            int selectorFieldOrdinal = loadData ? classInfo.SubclassSelectorField.OrdinalInTable : record.FieldCount - 1;
            object selectorActualValue = factory.GetFieldHandler(selectorFieldOrdinal).RawRead(record, firstColumnIndex + selectorFieldOrdinal);
            IComparer comparer = selectorActualValue is string ? (IComparer) CaseInsensitiveComparer.DefaultInvariant : Comparer.DefaultInvariant;
            if (0 == comparer.Compare(selectorActualValue, classInfo.SubclassSelectorValue))
                return factory;

            ISoodaObjectFactory newFactory;
            if (!factory.GetClassInfo().DisableTypeCache)
            {
                newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(classInfo.Name, keyValue);
                if (newFactory != null)
                    return newFactory;
            }

            foreach (ClassInfo ci in subclasses)
            {
                if (0 == comparer.Compare(selectorActualValue, ci.SubclassSelectorValue))
                {
                    newFactory = tran.GetFactory(ci);
                    SoodaTransaction.SoodaObjectFactoryCache.SetObjectFactory(classInfo.Name, keyValue, newFactory);
                    return newFactory;
                }
            }

            throw new Exception("Cannot determine subclass. Selector actual value: " + selectorActualValue + " base class: " + classInfo.Name);
        }

        static SoodaObject GetRefFromRecordHelper(SoodaTransaction tran, ISoodaObjectFactory factory, IDataRecord record, int firstColumnIndex, TableInfo[] loadedTables, int tableIndex, bool loadData)
        {
            object keyValue;
            Sooda.Schema.FieldInfo[] pkFields = factory.GetClassInfo().GetPrimaryKeyFields();
            if (pkFields.Length == 1)
            {
                int pkFieldOrdinal = loadData ? firstColumnIndex + pkFields[0].OrdinalInTable : 0;
                try
                {
                    keyValue = factory.GetPrimaryKeyFieldHandler().RawRead(record, pkFieldOrdinal);
                }
                catch (Exception ex)
                {
                    logger.Error("Error while reading field {0}.{1}: {2}", factory.GetClassInfo().Name, pkFieldOrdinal, ex);
                    throw ex;
                }
            }
            else
            {
                object[] pkParts = new object[pkFields.Length];
                for (int currentPkPart = 0; currentPkPart < pkFields.Length; currentPkPart++)
                {
                    int pkFieldOrdinal = loadData ? firstColumnIndex + pkFields[currentPkPart].OrdinalInTable : currentPkPart;
                    SoodaFieldHandler handler = factory.GetFieldHandler(pkFieldOrdinal);
                    pkParts[currentPkPart] = handler.RawRead(record, pkFieldOrdinal);
                }
                keyValue = new SoodaTuple(pkParts);
                //logger.Debug("Tuple: {0}", keyValue);
            }

            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if (retVal != null)
            {
                if (loadData && !retVal.IsDataLoaded(0))
                    retVal.LoadDataFromRecord(record, firstColumnIndex, loadedTables, tableIndex);
                return retVal;
            }

            factory = GetFactoryFromRecord(tran, factory, record, firstColumnIndex, keyValue, loadData);
            retVal = factory.GetRawObject(tran);
            tran.Statistics.RegisterObjectUpdate();
            SoodaStatistics.Global.RegisterObjectUpdate();
            retVal.InsertMode = false;
            retVal.SetPrimaryKeyValue(keyValue);
            if (loadData)
                retVal.LoadDataFromRecord(record, firstColumnIndex, loadedTables, tableIndex);
            return retVal;
        }

        public static SoodaObject GetRefFromRecordHelper(SoodaTransaction tran, ISoodaObjectFactory factory, IDataRecord record, int firstColumnIndex, TableInfo[] loadedTables, int tableIndex)
        {
            return GetRefFromRecordHelper(tran, factory, record, firstColumnIndex, loadedTables, tableIndex, true);
        }

        internal static SoodaObject GetRefFromKeyRecordHelper(SoodaTransaction tran, ISoodaObjectFactory factory, IDataRecord record)
        {
            return GetRefFromRecordHelper(tran, factory, record, 0, null, -1, false);
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, int keyValue)
        {
            return GetRefHelper(tran, factory, (object)keyValue);
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, string keyValue)
        {
            return GetRefHelper(tran, factory, (object)keyValue);
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, long keyValue)
        {
            return GetRefHelper(tran, factory, (object)keyValue);
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, Guid keyValue)
        {
            return GetRefHelper(tran, factory, (object)keyValue);
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, object keyValue)
        {
            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if (retVal != null)
                return retVal;

            ClassInfo classInfo = factory.GetClassInfo();
            if (classInfo.InheritsFromClass != null && tran.ExistsObjectWithKey(classInfo.GetRootClass().Name, keyValue))
                throw new SoodaObjectNotFoundException();

            if (classInfo.GetSubclassesForSchema(tran.Schema).Count > 0)
            {
                ISoodaObjectFactory newFactory = null;

                if (!classInfo.DisableTypeCache)
                {
                    newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(classInfo.Name, keyValue);
                }

                if (newFactory != null)
                {
                    factory = newFactory;
                }
                else
                {
                    // if the class is actually inherited, we delegate the responsibility
                    // to the appropriate GetRefFromRecord which will be called by the snapshot

                    SoqlBooleanExpression where = null;
                    Sooda.Schema.FieldInfo[] pkFields = classInfo.GetPrimaryKeyFields();
                    object[] par = new object[pkFields.Length];
                    for (int i = 0; i < pkFields.Length; ++i)
                    {
                        par[i] = SoodaTuple.GetValue(keyValue, i);
                        SoqlBooleanExpression cmp = Soql.FieldEqualsParam(pkFields[i].Name, i);
                        where = where == null ? cmp : where.And(cmp);
                    }
                    SoodaWhereClause whereClause = new SoodaWhereClause(where, par);

                    IList list = factory.GetList(tran, whereClause, null, SoodaSnapshotOptions.NoTransaction | SoodaSnapshotOptions.NoWriteObjects | SoodaSnapshotOptions.NoCache);
                    if (list.Count == 1)
                        return (SoodaObject)list[0];
                    else if (list.Count == 0)
                        throw new SoodaObjectNotFoundException("No matching object.");
                    else
                        throw new SoodaObjectNotFoundException("More than one object found. Fatal error.");
                }
            }

            retVal = factory.GetRawObject(tran);
            tran.Statistics.RegisterObjectUpdate();
            SoodaStatistics.Global.RegisterObjectUpdate();
            if (factory.GetClassInfo().ReadOnly)
            {
                retVal.LoadReadOnlyObject(keyValue);
            }
            else
            {
                retVal.SetUpdateMode(keyValue);
            }
            return retVal;
        }

        public override string ToString()
        {
            object keyVal = this.GetPrimaryKeyValue();
            return keyVal == null ? string.Empty : keyVal.ToString();
        }

        public void InitRawObject(SoodaTransaction tran)
        {
            _transaction = tran;
            _dataLoadedMask = 0;
            _flags = SoodaObjectFlags.InsertMode;
            _primaryKeyValue = null;
        }

        internal void CopyOnWrite()
        {
            if (FromCache)
            {
                _fieldValues = _fieldValues.Clone();
                FromCache = false;
            }
        }

        protected NameValueCollection GetTransactionPersistentValues()
        {
            return GetTransaction().GetPersistentValues(this);
        }

        protected void SetTransactionPersistentValue(string name, string value)
        {
            SetObjectDirty();
            GetTransaction().SetPersistentValue(this, name, value);
        }

        protected string GetTransactionPersistentValue(string name)
        {
            return GetTransaction().GetPersistentValue(this, name);
        }

        public virtual string GetLabel(bool throwOnError)
        {
            string labelField = GetClassInfo().GetLabel();
            if (labelField == null)
                return null;

            object o = Evaluate(labelField, throwOnError);

            if (o == null)
                return String.Empty;
            System.Data.SqlTypes.INullable nullable = o as System.Data.SqlTypes.INullable;
            if (nullable != null && nullable.IsNull)
                return String.Empty;

            return Convert.ToString(o);
        }

        Sooda.Schema.FieldInfo GetFieldInfo(string name)
        {
            ClassInfo ci = GetClassInfo();
            Sooda.Schema.FieldInfo fi = ci.FindFieldByName(name);
            if (fi == null)
                throw new Exception("Field " + name + " not found in " + ci.Name);
            return fi;
        }

        object GetTypedFieldValue(Sooda.Schema.FieldInfo fi)
        {
            object value = _fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal);
            if (value != null && fi.References != null)
                value = GetTransaction().GetFactory(fi.References).GetRef(GetTransaction(), value);
            return value;
        }

        public object this[string fieldName]
        {
            get
            {
                Sooda.Schema.FieldInfo fi = GetFieldInfo(fieldName);
                EnsureDataLoaded(fi.Table.OrdinalInClass);
                return GetTypedFieldValue(fi);
            }
            set
            {
                Sooda.Schema.FieldInfo fi = GetFieldInfo(fieldName);
                if (!fi.IsDynamic)
                {
                    // Disallow because:
                    // - the per-field update triggers wouldn't be called
                    // - for references, refcache would get out-of-date
                    // - for references, collections would not be updated
                    // Alternatively we might just set the property via reflection. This wouldn't suffer from the above problems.
                    throw new InvalidOperationException("Cannot set non-dynamic field " + fieldName + " with an indexer");
                }

                EnsureDataLoaded(fi.Table.OrdinalInClass);
                if (AreFieldUpdateTriggersEnabled())
                {
                    object oldValue = GetTypedFieldValue(fi);
                    if (Object.Equals(oldValue, value))
                        return;

                    BeforeFieldUpdate(fieldName, oldValue, value);
                    SoodaObject so = value as SoodaObject;
                    SetFieldValue(fi.ClassUnifiedOrdinal, so != null ? so.GetPrimaryKeyValue() : value);
                    AfterFieldUpdate(fieldName, oldValue, value);
                }
                else
                {
                    SoodaObject so = value as SoodaObject;
                    SetFieldValue(fi.ClassUnifiedOrdinal, so != null ? so.GetPrimaryKeyValue() : value);
                }
            }
        }

#if DOTNET4
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }
#endif
    }
}
