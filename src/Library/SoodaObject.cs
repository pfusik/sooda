//
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its
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

namespace Sooda {
    public class SoodaObject {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("Sooda.Object");

        // instance fields - initialized in InitRawObject()

        [CLSCompliant(false)]
        protected SoodaFieldData[] _fieldData;
        [CLSCompliant(false)]
        protected object[] _fieldValues;
        private int _dataLoadedMask;
        private SoodaTransaction _transaction;
        private SoodaObjectFlags _flags;
        private object _primaryKeyValue;

        public bool DisableTriggers
        {
            get {
                return (_flags & SoodaObjectFlags.DisableTriggers) != 0;
            }
            set {
                if (value)
                    _flags |= SoodaObjectFlags.DisableTriggers;
                else
                    _flags &= ~SoodaObjectFlags.DisableTriggers;
            }
        }

        private bool InsertMode
        {
            get {
                return (_flags & SoodaObjectFlags.InsertMode) != 0;
            }
            set {
                if (value) {
                    _flags |= SoodaObjectFlags.InsertMode;
                    SetObjectDirty();
                } else
                    _flags &= ~SoodaObjectFlags.InsertMode;
            }
        }

        internal bool VisitedOnCommit
        {
            get {
                return (_flags & SoodaObjectFlags.VisitedOnCommit) != 0;
            }
            set {
                if (value)
                    _flags |= SoodaObjectFlags.VisitedOnCommit;
                else
                    _flags &= ~SoodaObjectFlags.VisitedOnCommit;
            }
        }

        internal bool WrittenIntoDatabase
        {
            get {
                return (_flags & SoodaObjectFlags.WrittenIntoDatabase) != 0;
            }
            set {
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
            get {
                return (_flags & SoodaObjectFlags.FromCache) != 0;
            }
            set {
                if (value)
                    _flags |= SoodaObjectFlags.FromCache;
                else
                    _flags &= ~SoodaObjectFlags.FromCache;
            }
        }

        internal SoodaCacheEntry GetCacheEntry() {
            return new SoodaCacheEntry(_dataLoadedMask, _fieldValues);
        }

        private bool DeleteMarker
        {
            get {
                return (_flags & SoodaObjectFlags.MarkedForDeletion) != 0;
            }
            set {
                if (value)
                    _flags = _flags | SoodaObjectFlags.MarkedForDeletion;
                else
                    _flags = (_flags & ~SoodaObjectFlags.MarkedForDeletion);
            }
        }

        internal void SetInsertMode() {
            this.InsertMode = true;
            SetAllDataLoaded();
        }

        public bool IsInsertMode() {
            return this.InsertMode;
            ;
        }

        protected SoodaObject(SoodaConstructor c) {
            // do nothing - we delay all the initialization
        }

        protected SoodaObject(SoodaTransaction tran) {
            InitRawObject(tran);
            InsertMode = true;
            SetAllDataLoaded();
            if (GetClassInfo().SubclassSelectorValue != null) {
                DisableTriggers = true;
                Sooda.Schema.FieldInfo selectorField = GetClassInfo().SubclassSelectorField;
                SetPlainFieldValue(0, selectorField.Name, selectorField.ClassUnifiedOrdinal, GetClassInfo().SubclassSelectorValue);
                DisableTriggers = false;
            }
        }

        private void InitFieldData() {
            ClassInfo ci = GetClassInfo();

            int fieldCount = ci.UnifiedFields.Count;
            _fieldData = new SoodaFieldData[fieldCount];
            _fieldValues = new object[fieldCount];

            if (_primaryKeyValue != null) {
                int ordinal = ci.GetPrimaryKeyField().ClassUnifiedOrdinal;
                _fieldValues[ordinal] = _primaryKeyValue;
            }

            if (InsertMode) {
                SetDefaultNotNullValues();
            }
        }

        private void SetDefaultNotNullValues() {
            ClassInfo ci = GetClassInfo();
            int pkOrdinal = ci.GetPrimaryKeyField().ClassUnifiedOrdinal;

            for (int i = 0; i < _fieldValues.Length; ++i) {
                if (i == pkOrdinal)
                    continue;

                SoodaFieldHandler handler = GetFieldHandler(i);
                if (!handler.IsNullable) {
                    if (ci.UnifiedFields[i].ReferencedClass == null) {
                        _fieldValues[i] = handler.ZeroValue();
                    }
                }
            }
        }

        protected internal void SetUpdateMode(object primaryKeyValue) {
            InsertMode = false;
            SetInitialPrimaryKeyValue(primaryKeyValue);
            SetAllDataNotLoaded();

            if (_fieldValues == null) {
                SoodaCacheEntry cachedData = SoodaCache.FindObjectData(GetClassInfo().Name, primaryKeyValue);
                if (cachedData != null) {
                    logger.Debug("Initializing object {0}({1}) from cache: {2}", this.GetType().Name, primaryKeyValue, cachedData);
                    _fieldValues = cachedData.Data;
                    _fieldData = new SoodaFieldData[_fieldValues.Length];
                    _dataLoadedMask = cachedData.DataLoadedMask;
                    FromCache = true;
                } else {
                    logger.Debug("Object {0}({1}) not in cache. Creating uninitialized object.", this.GetType().Name, primaryKeyValue, cachedData);
                }
            }
        }

        public SoodaTransaction GetTransaction() {
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

        public void MarkForDelete() {
            MarkForDelete(true);
        }

        public void MarkForDelete(bool delete) {
            if (DeleteMarker != delete) {
                DeleteMarker = delete;
                if (delete) {
                    GetTransaction().AddToDeleteQueue(this);
                }
            }
        }

        public bool IsMarkedForDelete() {
            return DeleteMarker;
        }

        protected internal void ObjectDeleting(SoodaObject o, object refId) {
            if (refId is String) {
                EnsureFieldsInited();
                Sooda.Schema.FieldInfo refField = GetClassInfo().FindFieldByName(refId as String);

                switch (refField.DeleteAction) {
                case DeleteAction.Nothing:
                    if (_fieldValues[refField.ClassUnifiedOrdinal] != null) {
                        throw new SoodaDatabaseException("Cannot delete object " + o.GetObjectKeyString() + " because it's referenced by " + GetObjectKeyString() + "." + refId);
                    }
                    break;

                case DeleteAction.Nullify:
                    SoodaFieldHandler fieldHandler = GetFieldHandler(refField.ClassUnifiedOrdinal);
                    if (_fieldValues[refField.ClassUnifiedOrdinal] != null) {
                        if (!fieldHandler.IsNullable) {
                            throw new SoodaDatabaseException("Cannot delete object " + o.GetObjectKeyString() + " because it's referenced by non-nullable " + GetObjectKeyString() + "." + refId + " and delete action is to nullify the field");
                        }

                        GetType().GetProperty(refField.Name).SetValue(this, null, null);
                        Console.WriteLine(GetObjectKeyString() + ". Nullifying " + refField.Name + " of type " + refField.DataType);
                    }
                    break;

                case DeleteAction.Cascade:
                    if (!IsMarkedForDelete()) {
                        Console.WriteLine(GetObjectKeyString() + ". Cascading delete because object referenced by " + refField.Name + " has been deleted");
                        MarkForDelete();
                    }
                    break;

                default:
                    throw new NotSupportedException("Unknown DeleteAction: " + refField.DeleteAction);
                }
            }
        }

        internal object GetFieldValue(int fieldNumber) {
            return _fieldValues[fieldNumber];
        }

        public bool IsFieldDirty(int fieldNumber) {
            EnsureFieldsInited();
            return _fieldData[fieldNumber].IsDirty;
        }

        protected internal virtual SoodaFieldHandler GetFieldHandler(int ordinal) {
            throw new NotImplementedException();
        }

        protected SoodaFieldHandler GetFieldHandler(string name) {
            ClassInfo info = this.GetClassInfo();
            Sooda.Schema.FieldInfo fi = this.GetClassInfo().FindFieldByName(name);

            if (fi != null)
                return GetFieldHandler(fi.ClassUnifiedOrdinal);
            else
                throw new Exception("Field " + name + " not found in " + info.Name);
        }

        protected internal int PrimaryKeyFieldOrdinal
        {
            get {
                return GetClassInfo().GetPrimaryKeyField().ClassUnifiedOrdinal;
            }
        }

        internal void CheckForNulls() {
            EnsureFieldsInited();
            ClassInfo ci = GetClassInfo();

            for (int i = 0; i < _fieldData.Length; ++i) {
                if (!ci.UnifiedFields[i].IsNullable && IsInsertMode()) {
                    if ((_fieldValues[i] == null) && (IsInsertMode() || _fieldData[i].IsDirty))
                        FieldCannotBeNull(ci.UnifiedFields[i].Name);
                }
            }
        }
    protected internal virtual void CheckAssertions() { }
        void FieldCannotBeNull(string fieldName) {
            throw new SoodaException("Field '" + fieldName + "' cannot be null on commit in " + GetObjectKeyString());
        }

        protected internal virtual void IterateOuterReferences(SoodaObjectRefFieldIterator iterator, object context) { }

        public bool IsObjectDirty() {
            return (_flags & SoodaObjectFlags.Dirty) != 0;
        }

        internal void SetObjectDirty() {
            _flags |= SoodaObjectFlags.Dirty;
            _flags &= ~SoodaObjectFlags.WrittenIntoDatabase;
        }

        internal void ResetObjectDirty() {
            _flags &= ~SoodaObjectFlags.Dirty;
            _flags &= ~SoodaObjectFlags.WrittenIntoDatabase;
        }

        public virtual Sooda.Schema.ClassInfo GetClassInfo() {
            throw new NotImplementedException();
        }

        public Object this[String field]
        {
            get {
                return Evaluate(field);
            }
        }

        public string GetObjectKeyString() {
            return String.Format("{0}[{1}]", GetClassInfo().Name, GetPrimaryKeyValue());
        }

        public object GetPrimaryKeyValue() {
            return _primaryKeyValue;

            // int ordinal = GetClassInfo().GetPrimaryKeyField().ClassUnifiedOrdinal;
            // return _fieldData[ordinal].FieldValue;
        }

        protected void SetInitialPrimaryKeyValue(object keyValue) {
            if (_primaryKeyValue == null) {
                _primaryKeyValue = keyValue;
                if (_fieldData != null) {
                    int ordinal = GetClassInfo().GetPrimaryKeyField().ClassUnifiedOrdinal;
                    _fieldValues[ordinal] = _primaryKeyValue;
                }
                // Console.WriteLine("Registering object {0}:{1}", GetClassInfo().Name, keyValue);
                RegisterObjectInTransaction();
            } else {
                throw new SoodaException("Cannot set primary key value more than once.");
            }
        }

        protected internal virtual void AfterDeserialize() { }
        protected virtual void InitNewObject() { }
        protected internal virtual void SetPrimaryKeyValue(object o) {
            throw new SoodaException("Object is read-only!");
        }

        internal bool CanBeCached() {
            return true;
            // return GetClassInfo().Cached;
        }

#region 'Loaded' state management

        internal bool IsAnyDataLoaded() {
            return _dataLoadedMask != 0;
        }

        internal bool IsAllDataLoaded() {
            // 2^N-1 has exactly N lower bits set to 1
            return _dataLoadedMask == (1 << GetClassInfo().UnifiedTables.Count) - 1;
        }

        internal void SetAllDataLoaded() {
            // 2^N-1 has exactly N lower bits set to 1
            _dataLoadedMask = (1 << GetClassInfo().UnifiedTables.Count) - 1;
        }

        internal void SetAllDataNotLoaded() {
            _dataLoadedMask = 0;
        }
        protected internal bool IsDataLoaded(int tableNumber) {
            return (_dataLoadedMask & (1 << tableNumber)) != 0;
        }

        internal void SetDataLoaded(int tableNumber) {
            _dataLoadedMask |= (1 << tableNumber);
        }

        internal void SetDataNotLoaded(int tableNumber) {
            _dataLoadedMask &= ~(1 << tableNumber);
        }

#endregion

        protected internal void LoadDataFromRecord(System.Data.IDataRecord reader, int firstColumn, TableInfo[] tables) {
            int recordPos = firstColumn;
            bool first = true;

            EnsureFieldsInited();

            foreach (TableInfo table in tables) {
                if (table.OrdinalInClass == 0 && !first) {
                    // finish loading after we've encountered first table of another
                    // object
                    break;
                }

                foreach (Sooda.Schema.FieldInfo field in table.Fields) {
                    // don't load primary keys
                    if (!field.IsPrimaryKey) {
                        SoodaFieldHandler handler = GetFieldHandler(field.ClassUnifiedOrdinal);
                        int ordinal = field.ClassUnifiedOrdinal;

                        if (!_fieldData[ordinal].IsDirty) {
                            if (reader.IsDBNull(recordPos))
                                _fieldValues[ordinal] = null;
                            else
                                _fieldValues[ordinal] = handler.RawRead(reader, recordPos);
                        }
                    }
                    recordPos++;
                }

                SetDataLoaded(table.OrdinalInClass);
                first = false;
            }
            if (!IsObjectDirty()) {
                SoodaCache.AddObject(GetClassInfo().Name, GetPrimaryKeyValue(), GetCacheEntry());
                FromCache = true;
            }
        }

        protected void EnsureFieldsInited() {
            if (_fieldData == null)
                InitFieldData();
        }

        protected void EnsureDataLoaded(int tableNumber) {
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

        protected void LoadAllData() {
            for (int i = 0; i < GetClassInfo().UnifiedTables.Count; ++i) {
                if (!IsDataLoaded(i)) {
                    LoadData(i);
                }
            }
        }

        private void LoadData(int tableNumber) {
            LoadDataWithKey(GetPrimaryKeyValue(), tableNumber);
        }

        protected void LoadReadOnlyObject(object keyVal) {
            InsertMode = false;
            SetInitialPrimaryKeyValue(keyVal);
// #warning FIX ME

            LoadDataWithKey(keyVal, 0);
        }

        protected void LoadDataWithKey(object keyVal, int tableNumber) {
            if (logger.IsDebugEnabled) {
                logger.Debug("Loading data for {0}({1}) from table #{2}", GetClassInfo().Name, keyVal, tableNumber);
            };

            try {
                SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());
                TableInfo[] loadedTables;

                IDataReader record = ds.LoadObjectTable(this, keyVal, tableNumber, out loadedTables);
                if (record == null) {
                    Console.WriteLine("LoadObjectTable() failed for {0}.{1} {2}", this.GetType().Name, keyVal, tableNumber);
                    GetTransaction().UnregisterObject(this);
                    throw new SoodaObjectNotFoundException(String.Format("Object {0} not found in the database", GetObjectKeyString()));
                };
                using (record) {
                    LoadDataFromRecord(record, 0, loadedTables);
                    GetTransaction().MaterializeExtraObjects(record, loadedTables);
                }

            } catch (Exception e) {
                GetTransaction().UnregisterObject(this);

                throw new SoodaObjectNotFoundException(String.Format("Object {0} not found in the database: {1}", GetObjectKeyString(), e.ToString()), e);
            }
        }

        protected internal void RegisterObjectInTransaction() {
            GetTransaction().RegisterObject(this);
        }

        internal void DeleteObject() {
            SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());

            try {
                ds.DeleteObject(this);
            } catch (Exception e) {
                throw new SoodaDatabaseException("Cannot delete object in the database", e);
            }
        }

        internal void CommitObjectChanges() {
            SoodaDataSource ds = GetTransaction().OpenDataSource(GetClassInfo().GetDataSource());

            try {
                ds.SaveObjectChanges(this);
            } catch (Exception e) {
                throw new SoodaDatabaseException("Cannot save object to the database " + e.Message, e);
            }

            GetTransaction().AddToPostCommitQueue(this);
        }

        internal void PostCommit() {
            if (IsInsertMode()) {
                AfterObjectInsert();
                InsertMode = false;
            } else {
                AfterObjectUpdate();
            }
        }

        internal void PreCommit() {
            if (IsInsertMode()) {
                BeforeObjectInsert();
            } else {
                BeforeObjectUpdate();
            }
        }

        internal void Serialize(XmlWriter xw, SerializeOptions options) {
            xw.WriteStartElement("object");
            xw.WriteAttributeString("mode", (IsInsertMode()) ? "insert" : "update");
            xw.WriteAttributeString("class", GetClassInfo().Name);
            //xw.WriteAttributeString("key", PrimaryKeyFieldName);
            if (!IsObjectDirty())
                xw.WriteAttributeString("dirty", "false");
            if (PostCommitForced)
                xw.WriteAttributeString("forcepostcommit", "true");

            SoodaFieldHandler pkField = GetFieldHandler(PrimaryKeyFieldOrdinal);
            logger.Debug("Serializing " + GetObjectKeyString() + "...");
            EnsureFieldsInited();
            pkField.Serialize(_fieldValues[PrimaryKeyFieldOrdinal], xw);

            if ((options & SerializeOptions.IncludeNonDirtyFields) != 0) {
                if (!IsAllDataLoaded())
                    LoadAllData();
            };

            foreach (Sooda.Schema.FieldInfo fi in GetClassInfo().UnifiedFields) {
                if (fi == GetClassInfo().GetPrimaryKeyField())
                    continue;

                SoodaFieldHandler field = GetFieldHandler(fi.ClassUnifiedOrdinal);
                string s = fi.Name;

                if (_fieldData[fi.ClassUnifiedOrdinal].IsDirty) {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", s);
                    field.Serialize(_fieldValues[fi.ClassUnifiedOrdinal], xw);
                    xw.WriteEndElement();
                } else if ((options & SerializeOptions.IncludeNonDirtyFields) != 0) {
                    xw.WriteStartElement("field");
                    xw.WriteAttributeString("name", s);
                    field.Serialize(_fieldValues[fi.ClassUnifiedOrdinal], xw);
                    xw.WriteAttributeString("dirty", "false");
                    xw.WriteEndElement();
                };
            }
            if ((options & SerializeOptions.IncludeDebugInfo) != 0) {
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

            if (reader.GetAttribute("dirty") != "false") {
                EnsureFieldsInited();
                CopyOnWrite();

                SoodaFieldHandler field = GetFieldHandler(name);
                object val = field.Deserialize(reader);

                // Console.WriteLine("Deserializing field: {0}", name);

                PropertyInfo pi = GetType().GetProperty(name);
                if (pi.PropertyType.IsSubclassOf(typeof(SoodaObject))) {
                    if (val == null) {
                        pi.SetValue(this, null, null);
                    } else {
                        ISoodaObjectFactory fact = GetTransaction().GetFactory(pi.PropertyType);
                        SoodaObject refVal = fact.GetRef(GetTransaction(), val);
                        pi.SetValue(this, refVal, null);
                    }
                } else {
                    // set as raw

                    int fieldOrdinal = GetClassInfo().FindFieldByName(name).ClassUnifiedOrdinal;

                    _fieldValues[fieldOrdinal] = val;
                    _fieldData[fieldOrdinal].IsDirty = true;
                }

                SetObjectDirty();
            } else {
                // Console.WriteLine("Not deserializing field: {0}", name);
            }
        }

        protected void SetPlainFieldValue(int tableNumber, string fieldName, int fieldOrdinal, object newValue) {
            EnsureFieldsInited();

            if (!DisableTriggers) {
                EnsureDataLoaded(tableNumber);
                try {
                    object oldValue = _fieldValues[fieldOrdinal];
                    if (Object.Equals(oldValue, newValue))
                        return ;
                    object[] triggerArgs = new object[] { oldValue, newValue };

                    MethodInfo mi = this.GetType().GetMethod("BeforeFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);

                    CopyOnWrite();
                    _fieldValues[fieldOrdinal] = newValue;
                    _fieldData[fieldOrdinal].IsDirty = true;

                    mi = this.GetType().GetMethod("AfterFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);

                    SetObjectDirty();
                } catch (Exception e) {
                    throw new Exception("BeforeFieldUpdate raised an exception: ", e);
                }
            } else {
                // optimization here - we don't even need to load old values from database

                _fieldValues[fieldOrdinal] = newValue;
                _fieldData[fieldOrdinal].IsDirty = true;
                SetObjectDirty();
            }
        }

        protected SoodaObject SetRefFieldValue(int tableNumber, string fieldName, int fieldOrdinal, SoodaObject newValue, SoodaObject refcache, ISoodaObjectFactory factory) {
            EnsureFieldsInited();
            EnsureDataLoaded(tableNumber);
            CopyOnWrite();

            try {
                SoodaObject oldValue = RefCache.GetOrCreateObject(refcache, _fieldValues[fieldOrdinal], GetTransaction(), factory);
                if (Object.Equals(oldValue, newValue))
                    return newValue;
                object[] triggerArgs = new object[] { oldValue, newValue };

                if (!DisableTriggers) {
                    MethodInfo mi = this.GetType().GetMethod("BeforeFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
                if (oldValue != null) {
                    MethodInfo mi = this.GetType().GetMethod("BeforeCollectionUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
                if (newValue == null) {
                    _fieldValues[fieldOrdinal] = null;
                } else {
                    _fieldValues[fieldOrdinal] = newValue.GetPrimaryKeyValue();
                }
                _fieldData[fieldOrdinal].IsDirty = true;
                refcache = null;
                SetObjectDirty();
                if (newValue != null) {
                    MethodInfo mi = this.GetType().GetMethod("AfterCollectionUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
                if (!DisableTriggers) {
                    MethodInfo mi = this.GetType().GetMethod("AfterFieldUpdate_" + fieldName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);
                    mi.Invoke(this, triggerArgs);
                }
            } catch (Exception e) {
                throw new Exception("BeforeFieldUpdate raised an exception: ", e);
            }
            return newValue;
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

        public object Evaluate(string propertyAccessChain, bool throwOnError) {
            return Evaluate(propertyAccessChain.Split('.'), throwOnError);
        }

        protected static void PrecacheHelper(Hashtable hash, ISoodaObjectFactory factory, ClassInfo classInfo) {
            DataSourceInfo dsi = classInfo.GetDataSource();
            TableInfo[] loadedTables;

            SoodaDataSource ds = (SoodaDataSource)dsi.CreateDataSource();
            ds.Open();

            using (IDataReader reader = ds.LoadObjectList(classInfo, SoodaWhereClause.Unrestricted, null, out loadedTables)) {
                while (reader.Read()) {
                    object key = reader.GetValue(classInfo.GetPrimaryKeyField().ClassUnifiedOrdinal);

                    SoodaObject obj = factory.GetRawObject(null);
                    obj.LoadDataFromRecord(reader, 0, loadedTables);
                    obj._fieldValues[obj.PrimaryKeyFieldOrdinal] = key;
                    hash[key] = obj;
                }
            }
            ds.Close();
        }

        public static SoodaObject GetRefFromRecordHelper(SoodaTransaction tran, ISoodaObjectFactory factory, object keyValue, IDataRecord record, int firstColumn, TableInfo[] loadedTables) {
            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if ((retVal != null)) {
#warning FIX ME
                if (!retVal.IsDataLoaded(0)) {
                    retVal.LoadDataFromRecord(record, firstColumn, loadedTables);
                }
                return retVal;
            }

            if (factory.GetClassInfo().Subclasses.Count > 0) {
                // more complex case - we have to determine the actual factory to be
                // used for object creation

                int selectorFieldOrdinal = factory.GetClassInfo().SubclassSelectorField.OrdinalInTable;
                object selectorActualValue = record.GetValue(selectorFieldOrdinal);

                if (!selectorActualValue.Equals(factory.GetClassInfo().SubclassSelectorValue)) {
                    ISoodaObjectFactory newFactory = null;

                    if (!factory.GetClassInfo().DisableTypeCache) {
                        newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(factory.GetClassInfo().Name, keyValue);
                    }
                    if (newFactory == null) {
                        foreach (ClassInfo ci in factory.GetClassInfo().Subclasses) {
                            if (selectorActualValue.Equals(ci.SubclassSelectorValue)) {
                                newFactory = tran.GetFactory(ci);
                                break;
                            }
                        }
                        if (newFactory != null) {
                            SoodaTransaction.SoodaObjectFactoryCache.SetObjectFactory(factory.GetClassInfo().Name, keyValue, newFactory);
                        }
                    }

                    if (newFactory == null)
                        throw new Exception("Cannot determine subclass");

                    factory = newFactory;
                }
            }

            retVal = factory.GetRawObject(tran);
            retVal.InsertMode = false;
            retVal.SetInitialPrimaryKeyValue(keyValue);
            retVal.LoadDataFromRecord(record, firstColumn, loadedTables);
            return retVal;
        }

        public static SoodaObject GetRefHelper(SoodaTransaction tran, ISoodaObjectFactory factory, object keyValue) {
            SoodaObject retVal = factory.TryGet(tran, keyValue);
            if (retVal != null)
                return retVal;

            if (factory.GetClassInfo().InheritsFromClass != null) {
                if (tran.ExistsObjectWithKey(factory.GetClassInfo().GetRootClass().Name, keyValue)) {
                    throw new SoodaObjectNotFoundException();
                }
            }

            if (factory.GetClassInfo().Subclasses.Count > 0) {
                ISoodaObjectFactory newFactory = null;

                if (!factory.GetClassInfo().DisableTypeCache) {
                    newFactory = SoodaTransaction.SoodaObjectFactoryCache.FindObjectFactory(factory.GetClassInfo().Name, keyValue);
                }

                if (newFactory != null) {
                    factory = newFactory;
                } else {
                    // if the class is actually inherited, we delegate the responsibility
                    // to the appropriate GetRefFromRecord which will be called by the snapshot

#warning FIX ME - optimize

                    SoodaWhereClause whereClause = new SoodaWhereClause(factory.GetClassInfo().GetPrimaryKeyField().Name + " = {0}", keyValue);
                    IList list = factory.GetList(tran, whereClause, null, SoodaSnapshotOptions.NoTransaction | SoodaSnapshotOptions.NoWriteObjects);
                    if (list.Count == 1)
                        return (SoodaObject)list[0];
                    else {
                        if (list.Count == 0)
                            throw new SoodaObjectNotFoundException("No matching object.");
                        else
                            throw new SoodaObjectNotFoundException("More than one object found. Fatal error.");
                    }
                }
            }

            retVal = factory.GetRawObject(tran);
            if (factory.GetClassInfo().ReadOnly) {
                retVal.LoadReadOnlyObject(keyValue);
            } else {
                retVal.SetUpdateMode(keyValue);
            }
            return retVal;
        }

        public override string ToString() {
            object keyVal = this.GetPrimaryKeyValue();
            if (keyVal == null) {
                return string.Empty;
            } else {
                return keyVal.ToString();
            }
        }

        public void InitRawObject(SoodaTransaction tran) {
            _transaction = tran;
            _dataLoadedMask = 0;
            _flags = SoodaObjectFlags.InsertMode;
            _primaryKeyValue = null;
        }

        private static Type[] rawConstructorParameterTypes = new Type[] { typeof(SoodaConstructor) };
        private static object[] rawConstructorParameterValues = new object[] { SoodaConstructor.Constructor };

        public static SoodaObject GetRawObjectHelper(Type type, SoodaTransaction tran) {
            ConstructorInfo constructorInfo = type.GetConstructor(rawConstructorParameterTypes);
            if (constructorInfo == null) {
                throw new Exception("Constructor taking SoodaConstructor parameter not found in class " + type.FullName);
            }
            SoodaObject obj = (SoodaObject)constructorInfo.Invoke(rawConstructorParameterValues);

            obj.InitRawObject(tran);
            return obj;
        }

        private void CopyOnWrite() {
            if (FromCache) {
                object[] newFieldValues = new object[_fieldValues.Length];
                Array.Copy(_fieldValues, newFieldValues, _fieldValues.Length);
                _fieldValues = newFieldValues;
                FromCache = false;
            }
        }

        protected NameValueCollection GetTrasnsactionPersistentValues()
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
