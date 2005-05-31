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
using System.Diagnostics;
using System.Data;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Collections.Specialized;

using Sooda.Schema;
using Sooda.ObjectMapper;

using Sooda.Collections;

using Sooda.Logging;

namespace Sooda 
{
    public class SoodaObject 
    {
        private static Logger logger = LogManager.GetLogger("Sooda.Object");

        // instance fields - initialized in InitRawObject()

        [CLSCompliant(false)]
        protected SoodaFieldData[] _fieldData;

        [CLSCompliant(false)]
        protected SoodaObjectFieldValues _fieldValues;
        private int _dataLoadedMask;
        private SoodaTransaction _transaction;
        private SoodaObjectFlags _flags;
        private object _primaryKeyValue;
        internal SoodaObjectCollection OuterDeleteReferences;

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
            get 
            {
                return (_flags & SoodaObjectFlags.InsertedIntoDatabase) != 0;
            }
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
            get 
            {
                return (_flags & SoodaObjectFlags.FromCache) != 0;
            }
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

        private bool DeleteMarker
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

        protected SoodaObject(SoodaConstructor c) 
        {
            // do nothing - we delay all the initialization
        }

        protected SoodaObject(SoodaTransaction tran) 
        {
            InitRawObject(tran);
            InsertMode = true;
            SetAllDataLoaded();
            if (GetClassInfo().SubclassSelectorValue != null) 
            {
                DisableTriggers = true;
                Sooda.Schema.FieldInfo selectorField = GetClassInfo().SubclassSelectorField;
                SetPlainFieldValue(0, selectorField.Name, selectorField.ClassUnifiedOrdinal, GetClassInfo().SubclassSelectorValue);
                DisableTriggers = false;
            }
        }

        protected virtual SoodaObjectFieldValues InitFieldValues() 
        { 
            throw new NotImplementedException();
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

        private void InitFieldData() 
        {
            ClassInfo ci = GetClassInfo();

            int fieldCount = ci.UnifiedFields.Count;
            _fieldData = new SoodaFieldData[fieldCount];
            _fieldValues = InitFieldValues();

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

        protected internal void SetUpdateMode(object primaryKeyValue) 
        {
            InsertMode = false;
            SetPrimaryKeyValue(primaryKeyValue);
            SetAllDataNotLoaded();

            if (_fieldValues == null) 
            {
                SoodaCacheEntry cachedData = SoodaCache.FindObjectData(GetClassInfo().Name, primaryKeyValue);
                if (cachedData != null) 
                {
                    logger.Debug("Initializing object {0}({1}) from cache: {2}", this.GetType().Name, primaryKeyValue, cachedData);
                    _fieldValues = cachedData.Data;
                    _fieldData = new SoodaFieldData[_fieldValues.Length];
                    _dataLoadedMask = cachedData.DataLoadedMask;
                    FromCache = true;
                } 
                else 
                {
                    logger.Debug("Object {0}({1}) not in cache. Creating uninitialized object.", this.GetType().Name, primaryKeyValue, cachedData);
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
            MarkForDelete(true);
        }

        public void MarkForDelete(bool delete) 
        {
            if (DeleteMarker != delete) 
            {
                DeleteMarker = delete;
                if (delete) 
                {
                    GetTransaction().AddToDeleteQueue(this);
                }
                SetObjectDirty();
            }
        }

        public bool IsMarkedForDelete() 
        {
            return DeleteMarker;
        }

        internal object GetDBFieldValue(int fieldNumber) 
        {
            return GetFieldHandler(fieldNumber).GetDBFieldValue(_fieldValues.GetBoxedFieldValue(fieldNumber));
        }

        public bool IsFieldDirty(int fieldNumber) 
        {
            EnsureFieldsInited();
            return _fieldData[fieldNumber].IsDirty;
        }

        protected internal virtual SoodaFieldHandler GetFieldHandler(int ordinal) 
        {
            throw new NotImplementedException();
        }

        protected SoodaFieldHandler GetFieldHandler(string name) 
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

            for (int i = 0; i < _fieldData.Length; ++i) 
            {
                if (!ci.UnifiedFields[i].IsNullable && IsInsertMode()) 
                {
                    if (_fieldValues.IsNull(i) && (IsInsertMode() || _fieldData[i].IsDirty))
                        FieldCannotBeNull(ci.UnifiedFields[i].Name);
                }
            }
        }
        protected internal virtual void CheckAssertions() { }
        void FieldCannotBeNull(string fieldName) 
        {
            throw new SoodaException("Field '" + fieldName + "' cannot be null on commit in " + GetObjectKeyString());
        }

        protected internal virtual void IterateOuterReferences(SoodaObjectRefFieldIterator iterator, object context) { }

        public bool IsObjectDirty() 
        {
            return (_flags & SoodaObjectFlags.Dirty) != 0;
        }

        internal void SetObjectDirty() 
        {
            _flags |= SoodaObjectFlags.Dirty;
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
                if (_fieldData != null) 
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
        protected internal bool IsDataLoaded(int tableNumber) 
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

                if (table.OrdinalInClass == 0 && !first) 
                {
                    break;
                }

                foreach (Sooda.Schema.FieldInfo field in table.Fields) 
                {
                    // don't load primary keys
                    if (!field.IsPrimaryKey) 
                    {
                        SoodaFieldHandler handler = GetFieldHandler(field.ClassUnifiedOrdinal);
                        int ordinal = field.ClassUnifiedOrdinal;

                        if (!_fieldData[ordinal].IsDirty) 
                        {
                            if (reader.IsDBNull(recordPos))
                                _fieldValues.SetFieldValue(ordinal, null);
                            else
                                _fieldValues.SetFieldValue(ordinal, handler.RawRead(reader, recordPos));
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

            if (tableIndex == 0 && i != tables.Length)
            {
                logger.Debug("Materializing extra objects...");
                for (; i < tables.Length; ++i)
                {
                    if (tables[i].OrdinalInClass == 0)
                    {
                        logger.Debug("Materializing {0} at {1}", tables[i].NameToken, recordPos);

                        int pkOrdinal = tables[i].OwnerClass.GetFirstPrimaryKeyField().OrdinalInTable;
                        if (reader.IsDBNull(recordPos + pkOrdinal))
                        {
                            logger.Debug("Object is null. Skipping.");
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
                logger.Debug("Finished materializing extra objects.");
            }

            return tables.Length;
        }

        protected void EnsureFieldsInited() 
        {
            if (_fieldData == null)
                InitFieldData();
        }

        protected void EnsureDataLoaded(int tableNumber) 
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

        protected internal void LoadAllData() 
        {
#warning FIXME: Optimize!
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
            if (logger.IsDebugEnabled) 
            {
                logger.Debug("Loading data for {0}({1}) from table #{2}", GetClassInfo().Name, keyVal, tableNumber);
            };

            try 
            {
                SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());
                TableInfo[] loadedTables;

                IDataReader record = ds.LoadObjectTable(this, keyVal, tableNumber, out loadedTables);
                if (record == null) 
                {
                    logger.Debug("LoadObjectTable() failed for {0}", GetObjectKeyString());
                    GetTransaction().UnregisterObject(this);
                    throw new SoodaObjectNotFoundException(String.Format("Object {0} not found in the database", GetObjectKeyString()));
                };
                using (record) 
                {
                    LoadDataFromRecord(record, 0, loadedTables, 0);
                }

            } 
            catch (Exception ex) 
            {
                GetTransaction().UnregisterObject(this);
                logger.Error("Exception in LoadDataWithKey({0}): {1}", GetObjectKeyString(), ex);
                throw;
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

        internal void CommitObjectChanges() 
        {
            SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());

            try 
            {
                ds.SaveObjectChanges(this);
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

        internal void Serialize(XmlWriter xw, SerializeOptions options) 
        {
            xw.WriteStartElement("object");
            xw.WriteAttributeString("mode", (IsInsertMode()) ? "insert" : "update");
            xw.WriteAttributeString("class", GetClassInfo().Name);
            if (!IsObjectDirty())
                xw.WriteAttributeString("dirty", "false");
            if (PostCommitForced)
                xw.WriteAttributeString("forcepostcommit", "true");

            logger.Debug("Serializing " + GetObjectKeyString() + "...");
            EnsureFieldsInited();

            if ((options & SerializeOptions.IncludeNonDirtyFields) != 0) 
            {
                if (!IsAllDataLoaded())
                    LoadAllData();
            };

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

            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields) 
            {
                if (fi.IsPrimaryKey)
                    continue;

                SoodaFieldHandler field = GetFieldHandler(fi.ClassUnifiedOrdinal);
                string s = fi.Name;

                if (_fieldData[fi.ClassUnifiedOrdinal].IsDirty) 
                {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", s);
                    field.Serialize(_fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal), xw);
                    xw.WriteEndElement();
                } 
                else if ((options & SerializeOptions.IncludeNonDirtyFields) != 0) 
                {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", s);
                    field.Serialize(_fieldValues.GetBoxedFieldValue(fi.ClassUnifiedOrdinal), xw);
                    xw.WriteAttributeString("dirty", "false");
                    xw.WriteEndElement();
                };
            }
            if ((options & SerializeOptions.IncludeDebugInfo) != 0) 
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
                    _fieldData[fieldOrdinal].IsDirty = true;
                }

                SetObjectDirty();
            } 
            else 
            {
                // Console.WriteLine("Not deserializing field: {0}", name);
            }
        }

        protected void SetPlainFieldValue(int tableNumber, string fieldName, int fieldOrdinal, object newValue) 
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
                    object[] triggerArgs = new object[] { oldValue, newValue };

                    MethodInfo mi = this.GetType().GetMethod("BeforeFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);

                    CopyOnWrite();
                    _fieldValues.SetFieldValue(fieldOrdinal, newValue);
                    _fieldData[fieldOrdinal].IsDirty = true;

                    mi = this.GetType().GetMethod("AfterFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);

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
                _fieldData[fieldOrdinal].IsDirty = true;
                SetObjectDirty();
            }
        }

        protected void SetRefFieldValue(int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, ref SoodaObject refcache, ISoodaObjectFactory factory) 
        {
            EnsureFieldsInited();
            EnsureDataLoaded(tableNumber);
            CopyOnWrite();

            try 
            {
                SoodaObject oldValue = null;
                
                RefCache.GetOrCreateObject(ref oldValue, _fieldValues, fieldOrdinal, GetTransaction(), factory);
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
                _fieldData[fieldOrdinal].IsDirty = true;
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
                keyValue = factory.GetPrimaryKeyFieldHandler().RawRead(record, firstColumnIndex + pkFieldOrdinal);
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
                        pkParts[currentPkPart++] = factory.GetPrimaryKeyFieldHandler().RawRead(record, pkFieldOrdinal);
                    }
                }
                keyValue = new SoodaTuple(pkParts);
                logger.Debug("Tuple: {0}", keyValue);
            }

            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if ((retVal != null)) 
            {
#warning FIX ME
                if (!retVal.IsDataLoaded(0)) 
                {
                    retVal.LoadDataFromRecord(record, firstColumnIndex, loadedTables, tableIndex);
                }
                return retVal;
            }

            if (factory.GetClassInfo().Subclasses.Count > 0) 
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
                        foreach (ClassInfo ci in factory.GetClassInfo().Subclasses) 
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
            retVal.InsertMode = false;
            retVal.SetPrimaryKeyValue(keyValue);
            retVal.LoadDataFromRecord(record, firstColumnIndex, loadedTables, tableIndex);
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

            if (factory.GetClassInfo().Subclasses.Count > 0) 
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

#warning FIX ME - OPTIMIZE

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

        private void CopyOnWrite() 
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
