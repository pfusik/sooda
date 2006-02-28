// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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
using System.Reflection;
using System.Xml;
using System.Collections.Specialized;

using Sooda.Schema;
using Sooda.ObjectMapper;

using Sooda.QL;

using Sooda.Logging;
using Sooda.Caching;

namespace Sooda 
{
    public class SoodaObject 
    {
        private static Logger logger = LogManager.GetLogger("Sooda.Object");

        // instance fields - initialized in InitRawObject()

        internal byte[] _fieldIsDirty;
        internal SoodaObjectFieldValues _fieldValues;
        private int _dataLoadedMask;
        private SoodaTransaction _transaction;
        private SoodaObjectFlags _flags;
        private object _primaryKeyValue;

        public bool DisableTriggers
        {
            get 
            {
                return (_flags & SoodaObjectFlags.DisableTriggers) != 0;
            }
            set 
            {
                if (value)
                    _flags |= SoodaObjectFlags.DisableTriggers;
                else
                    _flags &= ~SoodaObjectFlags.DisableTriggers;
            }
        }

        protected bool AreFieldUpdateTriggersEnabled()
        {
            return !DisableTriggers;
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

        internal bool WrittenIntoDatabase
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

        internal bool PostCommitForced
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

        internal bool FromCache
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

        internal SoodaCacheEntry GetCacheEntry() 
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
            ;
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
                DisableTriggers = true;
                Sooda.Schema.FieldInfo selectorField = GetClassInfo().SubclassSelectorField;
                SetPlainFieldValue(0, selectorField.Name, selectorField.ClassUnifiedOrdinal, GetClassInfo().SubclassSelectorValue, null, null);
                DisableTriggers = false;
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
                int ordinal = primaryKeys[0].ClassUnifiedOrdinal;
                _fieldValues.SetFieldValue(ordinal, _primaryKeyValue);
            }
        }

        protected virtual SoodaObjectFieldValues InitFieldValues(int fieldCount)
        {
            return new SoodaObjectArrayFieldValues(fieldCount);
        }

        private void InitFieldData() 
        {
            ClassInfo ci = GetClassInfo();

            int fieldCount = ci.UnifiedFields.Count;
            _fieldValues = InitFieldValues(fieldCount);
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
                if (ci.UnifiedFields[i].IsPrimaryKey)
                    continue;

                SoodaFieldHandler handler = GetFieldHandler(i);
                if (!handler.IsNullable) 
                {
                    if (ci.UnifiedFields[i].ReferencedClass == null) 
                    {
                        _fieldValues.SetFieldValue(i, handler.ZeroValue());
                    }
                }
            }
        }

        internal void SetUpdateMode(object primaryKeyValue) 
        {
            InsertMode = false;
            SetPrimaryKeyValue(primaryKeyValue);
            SetAllDataNotLoaded();

            if (_fieldValues == null) 
            {
                SoodaCacheEntry cachedData = SoodaCache.FindObjectData(GetClassInfo().Name, primaryKeyValue);
                if (cachedData != null) 
                {
                    GetTransaction().Statistics.RegisterCacheHit();
                    SoodaStatistics.Global.RegisterCacheHit();

                    if (logger.IsTraceEnabled)
                    {
                        logger.Trace("Initializing object {0}({1}) from cache: {2}", this.GetType().Name, primaryKeyValue, cachedData);
                    }
                    _fieldValues = cachedData.Data;
                    _dataLoadedMask = cachedData.DataLoadedMask;
                    FromCache = true;
                } 
                else 
                {
                    GetTransaction().Statistics.RegisterCacheMiss();
                    SoodaStatistics.Global.RegisterCacheMiss();
                    if (logger.IsTraceEnabled)
                    {
                        logger.Trace("Object {0}({1}) not in cache. Creating uninitialized object.", this.GetType().Name, primaryKeyValue, cachedData);
                    }
                }
            }
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
                    GetTransaction()._savingObjects = true;

                    foreach (SoodaDataSource source in GetTransaction()._dataSources)
                    {
                        source.BeginSaveChanges();
                    }
                    for (int i = oldDeletePosition; i < newDeletePosition; ++i)
                    {
                        // logger.Debug("Actually deleting {0}", GetTransaction().DeletedObjects[i].GetObjectKeyString());
                        GetTransaction().DeletedObjects[i].CommitObjectChanges();
                        GetTransaction().DeletedObjects[i].SetObjectDirty();
                    }
                    foreach (SoodaDataSource source in GetTransaction()._dataSources) 
                    {
                        source.FinishSaveChanges();
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

                            SoqlBooleanExpression whereExpression = 
                                new SoqlBooleanRelationalExpression(
                                new SoqlPathExpression(fi.Name), 
                                new SoqlLiteralExpression(GetPrimaryKeyValue()),
                                SoqlRelationalOperator.Equal);
                                
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
                ClassInfo ci = GetClassInfo();

                int fieldCount = ci.UnifiedFields.Count;
                _fieldIsDirty = new byte[(fieldCount + 7) / 8];
            }

            int slotNumber = fieldNumber >> 3;
            int bitNumber = fieldNumber & 7;

            if (dirty)
            {
                _fieldIsDirty[slotNumber] |= (byte)(1 << bitNumber);
            }
            else
            {
                _fieldIsDirty[slotNumber] &= (byte)~(1 << bitNumber);
            }
        }

        protected virtual SoodaFieldHandler GetFieldHandler(int ordinal) 
        {
            throw new NotImplementedException();
        }

        internal SoodaFieldHandler GetFieldHandler(string name) 
        {
            ClassInfo info = this.GetClassInfo();
            Sooda.Schema.FieldInfo fi = this.GetClassInfo().FindFieldByName(name);

            if (fi != null)
                return GetFieldHandler(fi.ClassUnifiedOrdinal);
            else
                throw new Exception("Field " + name + " not found in " + info.Name);
        }

        internal void CheckForNulls() 
        {
            EnsureFieldsInited();
            ClassInfo ci = GetClassInfo();

            for (int i = 0; i < _fieldValues.Length; ++i) 
            {
                if (!ci.UnifiedFields[i].IsNullable && IsInsertMode()) 
                {
                    if (_fieldValues.IsNull(i) && (IsInsertMode() || IsFieldDirty(i)))
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
            _flags &= ~SoodaObjectFlags.Dirty;
            _flags &= ~SoodaObjectFlags.WrittenIntoDatabase;
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

        bool IsPrimaryKeyReadyForRegistration(object keyValue)
        {
            if (keyValue is SoodaTuple)
            {
                return ((SoodaTuple)keyValue).IsAllNotNull();
            }
            else
            {
                return true;
            }
        }

        protected internal void SetPrimaryKeySubValue(object keyValue, int valueOrdinal, int totalValues)
        {
            if (_primaryKeyValue == null)
                _primaryKeyValue = new SoodaTuple(totalValues);
            ((SoodaTuple)_primaryKeyValue).SetValue(valueOrdinal, keyValue);
            if (IsPrimaryKeyReadyForRegistration(_primaryKeyValue))
            {
                if (_fieldValues != null) 
                    PropagatePrimaryKeyToFields();
                if (IsRegisteredInTransaction())
                {
                    throw new SoodaException("Cannot set primary key value more than once.");
                }
                else
                {
                    RegisterObjectInTransaction();
                }
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

        internal bool CanBeCached() 
        {
            return true;
            // return GetClassInfo().Cached;
        }

        #region 'Loaded' state management

        internal bool IsAnyDataLoaded() 
        {
            return _dataLoadedMask != 0;
        }

        internal bool IsAllDataLoaded() 
        {
            // 2^N-1 has exactly N lower bits set to 1
            return _dataLoadedMask == (1 << GetClassInfo().UnifiedTables.Count) - 1;
        }

        internal void SetAllDataLoaded() 
        {
            // 2^N-1 has exactly N lower bits set to 1
            _dataLoadedMask = (1 << GetClassInfo().UnifiedTables.Count) - 1;
        }

        internal void SetAllDataNotLoaded() 
        {
            _dataLoadedMask = 0;
        }

        internal bool IsDataLoaded(int tableNumber) 
        {
            return (_dataLoadedMask & (1 << tableNumber)) != 0;
        }

        internal void SetDataLoaded(int tableNumber) 
        {
            _dataLoadedMask |= (1 << tableNumber);
        }

        internal void SetDataNotLoaded(int tableNumber) 
        {
            _dataLoadedMask &= ~(1 << tableNumber);
        }

        #endregion

        private int LoadDataFromRecord(System.Data.IDataRecord reader, int firstColumnIndex, TableInfo[] tables, int tableIndex) 
        {
            int recordPos = firstColumnIndex;
            bool first = true;

            EnsureFieldsInited();

            int i;

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
                            SoodaFieldHandler handler = GetFieldHandler(field.ClassUnifiedOrdinal);
                            int ordinal = field.ClassUnifiedOrdinal;

                            if (!IsFieldDirty(ordinal)) 
                            {
                                if (reader.IsDBNull(recordPos))
                                    _fieldValues.SetFieldValue(ordinal, null);
                                else
                                    _fieldValues.SetFieldValue(ordinal, handler.RawRead(reader, recordPos));
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error while reading field {0}.{1}: {2}", table.NameToken, field.Name, ex);
                            throw ex;
                        }
                    }
                    recordPos++;
                }

                SetDataLoaded(table.OrdinalInClass);
                first = false;
            }
            if (!IsObjectDirty()) 
            {
                SoodaCache.AddObject(GetClassInfo().Name, GetPrimaryKeyValue(), GetCacheEntry());
                FromCache = true;
            }

            // if we've started with a first table and there are more to be processed
            if (tableIndex == 0 && i != tables.Length)
            {
                // logger.Trace("Materializing extra objects...");
                for (; i < tables.Length; ++i)
                {
                    if (tables[i].OrdinalInClass == 0)
                    {
                        GetTransaction().Statistics.RegisterExtraMaterialization();
                        SoodaStatistics.Global.RegisterExtraMaterialization();

                        // logger.Trace("Materializing {0} at {1}", tables[i].NameToken, recordPos);

                        int pkOrdinal = tables[i].OwnerClass.GetFirstPrimaryKeyField().OrdinalInTable;
                        if (reader.IsDBNull(recordPos + pkOrdinal))
                        {
                            // logger.Trace("Object is null. Skipping.");
                        }
                        else
                        {
                            ISoodaObjectFactory factory = GetTransaction().GetFactory(tables[i].OwnerClass);
                            SoodaObject.GetRefFromRecordHelper(GetTransaction(), factory, reader, recordPos, tables, i);
                        }
                    }
                    else
                    {
                        // TODO - can this be safely called?
                    }
                    recordPos += tables[i].Fields.Count;
                }
                // logger.Trace("Finished materializing extra objects.");
            }

            return tables.Length;
        }

        internal void EnsureFieldsInited() 
        {
            if (_fieldValues == null)
                InitFieldData();
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
            if (logger.IsTraceEnabled) 
            {
                logger.Trace("Loading data for {0}({1}) from table #{2}", GetClassInfo().Name, keyVal, tableNumber);
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
                    };
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

        protected internal void RegisterObjectInTransaction() 
        {
            GetTransaction().RegisterObject(this);
        }

        protected internal bool IsRegisteredInTransaction() 
        {
            return GetTransaction().IsRegistered(this);
        }

        internal void SaveOuterReferences()
        {
            // iterate outer references

            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields)
            {
                if (fi.ReferencedClass == null)
                    continue;

                object v = _fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal);
                if (v != null)
                {
                    ISoodaObjectFactory factory = GetTransaction().GetFactory(fi.ReferencedClass);
                    SoodaObject obj = factory.TryGet(GetTransaction(), v);

                    if (obj != null && (object)obj != (object)this) 
                    {
                        if (obj.IsInsertMode())
                        {
                            if (obj.VisitedOnCommit && !obj.WrittenIntoDatabase)
                            {
                                throw new Exception("Cyclic reference between " + GetObjectKeyString() + " and " + obj.GetObjectKeyString());
                                // cyclic reference
                            } 
                            else 
                            {
                                obj.SaveObjectChanges();
                            }
                        }
                    }
                }
            }
        }

        internal void SaveObjectChanges() 
        {
            VisitedOnCommit = true;
            if (IsObjectDirty())
            {
                SaveOuterReferences();
            }

            if (WrittenIntoDatabase)
                return ;

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
                SoodaCache.InvalidateObject(GetClassInfo().Name, GetPrimaryKeyValue());
            } 
            catch (Exception e) 
            {
                throw new SoodaDatabaseException("Cannot save object to the database " + e.Message, e);
            }

            GetTransaction().AddToPostCommitQueue(this);
        }

        internal void PostCommit() 
        {
            if (IsInsertMode()) 
            {
                AfterObjectInsert();
                InsertMode = false;
            } 
            else 
            {
                AfterObjectUpdate();
            }
        }

        internal void PreCommit() 
        {
            if (IsInsertMode()) 
            {
                BeforeObjectInsert();
            } 
            else 
            {
                BeforeObjectUpdate();
            }
        }

        private void SerializePrimaryKey(XmlWriter xw)
        {
            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields) 
            {
                if (!fi.IsPrimaryKey)
                    continue;

                SoodaFieldHandler field = GetFieldHandler(fi.ClassUnifiedOrdinal);
                string s = fi.Name;

                xw.WriteStartElement("key");
                xw.WriteAttributeString("ordinal", fi.ClassUnifiedOrdinal.ToString());
                field.Serialize(_fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal), xw);
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

            if (PostCommitForced)
                xw.WriteAttributeString("forcepostcommit", "true");

            logger.Trace("Serializing " + GetObjectKeyString() + "...");
            EnsureFieldsInited();

            if ((options & SoodaSerializeOptions.IncludeNonDirtyFields) != 0) 
            {
                if (!IsAllDataLoaded())
                    LoadAllData();
            };

            SerializePrimaryKey(xw);

            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields) 
            {
                if (fi.IsPrimaryKey)
                    continue;

                SoodaFieldHandler field = GetFieldHandler(fi.ClassUnifiedOrdinal);
                string s = fi.Name;

                if (IsFieldDirty(fi.ClassUnifiedOrdinal)) 
                {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", s);
                    field.Serialize(_fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal), xw);
                    xw.WriteEndElement();
                } 
                else if ((options & SoodaSerializeOptions.IncludeNonDirtyFields) != 0) 
                {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", s);
                    field.Serialize(_fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal), xw);
                    xw.WriteAttributeString("dirty", "false");
                    xw.WriteEndElement();
                };
            }
            if ((options & SoodaSerializeOptions.IncludeDebugInfo) != 0) 
            {
                xw.WriteStartElement("debug");
                xw.WriteAttributeString("transaction", (_transaction != null) ? "notnull" : "null");
                xw.WriteAttributeString("objectDirty", IsObjectDirty() ? "true" : "false");
                xw.WriteAttributeString("dataLoaded", IsAllDataLoaded() ? "true" : "false");
                xw.WriteAttributeString("disableTriggers", DisableTriggers ? "true" : "false");
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

                SoodaFieldHandler field = GetFieldHandler(name);
                object val = field.Deserialize(reader);

                // Console.WriteLine("Deserializing field: {0}", name);

                PropertyInfo pi = GetType().GetProperty(name);
                if (pi.PropertyType.IsSubclassOf(typeof(SoodaObject))) 
                {
                    if (val == null) 
                    {
                        pi.SetValue(this, null, null);
                    } 
                    else 
                    {
                        ISoodaObjectFactory fact = GetTransaction().GetFactory(pi.PropertyType);
                        SoodaObject refVal = fact.GetRef(GetTransaction(), val);
                        pi.SetValue(this, refVal, null);
                    }
                } 
                else 
                {
                    // set as raw

                    int fieldOrdinal = GetClassInfo().FindFieldByName(name).ClassUnifiedOrdinal;

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

        internal void SetPlainFieldValue(int tableNumber, string fieldName, int fieldOrdinal, object newValue, SoodaFieldUpdateDelegate before, SoodaFieldUpdateDelegate after) 
        {
            EnsureFieldsInited();

            if (!DisableTriggers) 
            {
                EnsureDataLoaded(tableNumber);
                try 
                {
                    object oldValue = _fieldValues.GetBoxedFieldValue(fieldOrdinal);
                    if (Object.Equals(oldValue, newValue))
                        return ;

                    if (before != null)
                        before(oldValue, newValue);

                    CopyOnWrite();
                    _fieldValues.SetFieldValue(fieldOrdinal, newValue);
                    SetFieldDirty(fieldOrdinal, true);

                    if (after != null)
                        after(oldValue, newValue);
                    
                    SetObjectDirty();
                } 
                catch (Exception e) 
                {
                    throw new Exception("BeforeFieldUpdate raised an exception: ", e);
                }
            } 
            else 
            {
                // optimization here - we don't even need to load old values from database

                _fieldValues.SetFieldValue(fieldOrdinal, newValue);
                SetFieldDirty(fieldOrdinal, true);
                SetObjectDirty();
            }
        }

        internal void SetRefFieldValue(int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, ref SoodaObject refcache, ISoodaObjectFactory factory) 
        {
            if (newValue != null)
            {
                // transaction check
                if (newValue.GetTransaction() != this.GetTransaction())
                    throw new SoodaException("Attempted to assign object " + newValue.GetObjectKeyString() + " from another transaction to " + this.GetObjectKeyString() + "." + fieldName);
            }

            EnsureFieldsInited();
            EnsureDataLoaded(tableNumber);
            CopyOnWrite();

            try 
            {
                SoodaObject oldValue = null;
                
                SoodaObjectImpl.GetRefFieldValue(ref oldValue, this, tableNumber, fieldOrdinal, GetTransaction(), factory);
                if (Object.Equals(oldValue, newValue))
                    return;
                object[] triggerArgs = new object[] { oldValue, newValue };

                if (!DisableTriggers) 
                {
                    MethodInfo mi = this.GetType().GetMethod("BeforeFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
                if (oldValue != null) 
                {
                    MethodInfo mi = this.GetType().GetMethod("BeforeCollectionUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
                if (newValue == null) 
                {
                    _fieldValues.SetFieldValue(fieldOrdinal, null);
                } 
                else 
                {
                    _fieldValues.SetFieldValue(fieldOrdinal, newValue.GetPrimaryKeyValue());
                }
                SetFieldDirty(fieldOrdinal, true);
                refcache = null;
                SetObjectDirty();
                if (newValue != null) 
                {
                    MethodInfo mi = this.GetType().GetMethod("AfterCollectionUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
                if (!DisableTriggers) 
                {
                    MethodInfo mi = this.GetType().GetMethod("AfterFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
            } 
            catch (Exception e) 
            {
                throw new Exception("BeforeFieldUpdate raised an exception: ", e);
            }
        }

        public object Evaluate(string[] propertyAccessChain, bool throwOnError) 
        {
            try 
            {
                object currentObject = this;

                for (int i = 0; i < (propertyAccessChain.Length) && (currentObject != null) && (currentObject is SoodaObject); ++i) 
                {
                    Type currenttype = currentObject.GetType();
                    string prop = (string)propertyAccessChain[i];
                    PropertyInfo pi = currenttype.GetProperty(prop);
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

        public static SoodaObject GetRefFromRecordHelper(SoodaTransaction tran, ISoodaObjectFactory factory, IDataRecord record, int firstColumnIndex, TableInfo[] loadedTables, int tableIndex)
        {
            object keyValue;
            int pkSize = factory.GetClassInfo().GetPrimaryKeyFields().Length;

            if (pkSize == 1)
            {
                int pkFieldOrdinal = factory.GetClassInfo().GetFirstPrimaryKeyField().OrdinalInTable;
                try
                {
                    keyValue = factory.GetPrimaryKeyFieldHandler().RawRead(record, firstColumnIndex + pkFieldOrdinal);
                }
                catch (Exception ex)
                {
                    logger.Error("Error while reading field {0}.{1}: {2}", factory.GetClassInfo().Name, firstColumnIndex + pkFieldOrdinal, ex);
                    throw ex;
                }
            }
            else
            {
                object[] pkParts = new object[pkSize];
                int currentPkPart = 0;
                foreach (Sooda.Schema.FieldInfo fi in factory.GetClassInfo().UnifiedFields)
                {
                    if (fi.IsPrimaryKey)
                    {
                        int pkFieldOrdinal = firstColumnIndex + fi.OrdinalInTable;
                        SoodaFieldHandler handler = factory.GetFieldHandler(pkFieldOrdinal);
                        pkParts[currentPkPart++] = handler.RawRead(record, pkFieldOrdinal);
                    }
                }
                keyValue = new SoodaTuple(pkParts);
                //logger.Debug("Tuple: {0}", keyValue);
            }

            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if (retVal != null)
            {
                if (!retVal.IsDataLoaded(0)) 
                {
                    retVal.LoadDataFromRecord(record, firstColumnIndex, loadedTables, tableIndex);
                }
                return retVal;
            }

            ClassInfoCollection subclasses = tran.Schema.GetSubclasses(factory.GetClassInfo()); 

            if (subclasses.Count > 0) 
            {
                // more complex case - we have to determine the actual factory to be
                // used for object creation

                int selectorFieldOrdinal = factory.GetClassInfo().SubclassSelectorField.OrdinalInTable;
                object selectorActualValue = record.GetValue(firstColumnIndex + selectorFieldOrdinal);
                IComparer comparer = Comparer.DefaultInvariant;
                if (selectorActualValue is string)
                    comparer = CaseInsensitiveComparer.DefaultInvariant;
                    
                if (0 != comparer.Compare(selectorActualValue, factory.GetClassInfo().SubclassSelectorValue))
                {
                    ISoodaObjectFactory newFactory = null;

                    if (!factory.GetClassInfo().DisableTypeCache) 
                    {
                        newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(factory.GetClassInfo().Name, keyValue);
                    }
                    if (newFactory == null) 
                    {
                        foreach (ClassInfo ci in subclasses) 
                        {
                            if (0 == comparer.Compare(selectorActualValue, ci.SubclassSelectorValue))
                            {
                                newFactory = tran.GetFactory(ci);
                                break;
                            }
                        }
                        if (newFactory != null) 
                        {
                            SoodaTransaction.SoodaObjectFactoryCache.SetObjectFactory(factory.GetClassInfo().Name, keyValue, newFactory);
                        }
                    }

                    if (newFactory == null)
                        throw new Exception("Cannot determine subclass. Selector actual value: " + selectorActualValue + " base class: " + factory.GetClassInfo().Name);

                    factory = newFactory;
                }
            }

            retVal = factory.GetRawObject(tran);
            tran.Statistics.RegisterObjectUpdate();
            SoodaStatistics.Global.RegisterObjectUpdate();
            retVal.InsertMode = false;
            retVal.SetPrimaryKeyValue(keyValue);
            retVal.LoadDataFromRecord(record, firstColumnIndex, loadedTables, tableIndex);
            return retVal;
        }

        internal static SoodaObject GetRefFromKeyRecordHelper(SoodaTransaction tran, ISoodaObjectFactory factory, IDataRecord record)
        {
            object keyValue;
            int pkSize = factory.GetClassInfo().GetPrimaryKeyFields().Length;

            // TODO - REFACTORING: Merge GetRefFromKeyRecordHelper with GetRefFromRecordHelper

            if (pkSize == 1)
            {
                try
                {
                    keyValue = factory.GetPrimaryKeyFieldHandler().RawRead(record, 0);
                }
                catch (Exception ex)
                {
                    logger.Error("Error while reading field {0}.{1}: {2}", factory.GetClassInfo().Name, 0, ex);
                    throw ex;
                }
            }
            else
            {
                object[] pkParts = new object[pkSize];
                for (int currentPkPart = 0; currentPkPart < pkSize; currentPkPart++)
                {
                    SoodaFieldHandler handler = factory.GetFieldHandler(currentPkPart);
                    pkParts[currentPkPart] = handler.RawRead(record, currentPkPart);
                }
                keyValue = new SoodaTuple(pkParts);
                //logger.Debug("Tuple: {0}", keyValue);
            }

            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if ((retVal != null)) 
            {
                return retVal;
            }

            ClassInfoCollection subclasses = tran.Schema.GetSubclasses(factory.GetClassInfo()); 

            if (subclasses.Count > 0) 
            {
                // more complex case - we have to determine the actual factory to be
                // used for object creation

                // we have the selector field value as the last one in the record

                int selectorFieldOrdinal = record.FieldCount - 1;
                object selectorActualValue = record.GetValue(selectorFieldOrdinal);
                IComparer comparer = Comparer.DefaultInvariant;
                if (selectorActualValue is string)
                    comparer = CaseInsensitiveComparer.DefaultInvariant;
                    
                if (0 != comparer.Compare(selectorActualValue, factory.GetClassInfo().SubclassSelectorValue))
                {
                    ISoodaObjectFactory newFactory = null;

                    if (!factory.GetClassInfo().DisableTypeCache) 
                    {
                        newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(factory.GetClassInfo().Name, keyValue);
                    }
                    if (newFactory == null) 
                    {
                        foreach (ClassInfo ci in subclasses) 
                        {
                            if (0 == comparer.Compare(selectorActualValue, ci.SubclassSelectorValue))
                            {
                                newFactory = tran.GetFactory(ci);
                                break;
                            }
                        }
                        if (newFactory != null) 
                        {
                            SoodaTransaction.SoodaObjectFactoryCache.SetObjectFactory(factory.GetClassInfo().Name, keyValue, newFactory);
                        }
                    }

                    if (newFactory == null)
                        throw new Exception("Cannot determine subclass. Selector actual value: " + selectorActualValue + " base class: " + factory.GetClassInfo().Name);

                    factory = newFactory;
                }
            }

            retVal = factory.GetRawObject(tran);
            tran.Statistics.RegisterObjectUpdate();
            SoodaStatistics.Global.RegisterObjectUpdate();
            retVal.InsertMode = false;
            retVal.SetPrimaryKeyValue(keyValue);
            return retVal;
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, object keyValue) 
        {
            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if (retVal != null)
                return retVal;

            if (factory.GetClassInfo().InheritsFromClass != null) 
            {
                if (tran.ExistsObjectWithKey(factory.GetClassInfo().GetRootClass().Name, keyValue)) 
                {
                    throw new SoodaObjectNotFoundException();
                }
            }

            if (factory.GetClassInfo().GetSubclassesForSchema(tran.Schema).Count > 0) 
            {
                ISoodaObjectFactory newFactory = null;

                if (!factory.GetClassInfo().DisableTypeCache) 
                {
                    newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(factory.GetClassInfo().Name, keyValue);
                }

                if (newFactory != null) 
                {
                    factory = newFactory;
                } 
                else 
                {
                    // if the class is actually inherited, we delegate the responsibility
                    // to the appropriate GetRefFromRecord which will be called by the snapshot

                    // TODO - OPTIMIZE: REMOVE PARSING

                    StringBuilder query = new StringBuilder();
                    Sooda.Schema.FieldInfo[] pkFields = factory.GetClassInfo().GetPrimaryKeyFields();
                    object[] par = new object[pkFields.Length];

                    for (int i = 0; i < pkFields.Length; ++i)
                    {
                        if (i > 0)
                            query.Append(" and ");
                        query.Append("(");
                        query.Append(pkFields[i].Name);
                        query.Append("={");
                        query.Append(i);
                        query.Append("})");
                        par[i] = SoodaTuple.GetValue(keyValue, i);
                    }

                    SoodaWhereClause whereClause = new SoodaWhereClause(query.ToString(), par);
                    IList list = factory.GetList(tran, whereClause, null, SoodaSnapshotOptions.NoTransaction | SoodaSnapshotOptions.NoWriteObjects);
                    if (list.Count == 1)
                        return (SoodaObject)list[0];
                    else 
                    {
                        if (list.Count == 0)
                            throw new SoodaObjectNotFoundException("No matching object.");
                        else
                            throw new SoodaObjectNotFoundException("More than one object found. Fatal error.");
                    }
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
            if (keyVal == null) 
            {
                return string.Empty;
            } 
            else 
            {
                return keyVal.ToString();
            }
        }

        public void InitRawObject(SoodaTransaction tran) 
        {
            _transaction = tran;
            _dataLoadedMask = 0;
            _flags = SoodaObjectFlags.InsertMode;
            _primaryKeyValue = null;
        }

        private static Type[] rawConstructorParameterTypes = new Type[] { typeof(SoodaConstructor) };
        private static object[] rawConstructorParameterValues = new object[] { SoodaConstructor.Constructor };

        internal void CopyOnWrite() 
        {
            if (FromCache) 
            {
                SoodaObjectFieldValues newFieldValues = _fieldValues.Clone();
                _fieldValues = newFieldValues;
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

        public string GetLabel(bool throwOnError)
        {
            string labelField = GetClassInfo().GetLabel();
            if (labelField == null)
                return null;

            object o = Evaluate(labelField, throwOnError);
            if (o == null)
                return String.Empty;

            if (o is System.Data.SqlTypes.INullable)
            {
                if (((System.Data.SqlTypes.INullable)o).IsNull)
                    return String.Empty;
            }

            return Convert.ToString(o);
        }
    } // class SoodaObject
} // namespace
