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
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Collections.Specialized;

using Sooda.Schema;

using Sooda.ObjectMapper;
using Sooda.Collections;
using Sooda.Caching;

using Sooda.Logging;
using System.ComponentModel;

namespace Sooda
{
    [Flags]
    public enum SoodaTransactionOptions
    {
        NoImplicit = 0x0000,
        Implicit = 0x0001,
    }

    public class SoodaTransaction : Component, IDisposable
    {
        private static Logger transactionLogger = LogManager.GetLogger("Sooda.Transaction");
        private static IDefaultSoodaTransactionStrategy _defaultTransactionStrategy = new SoodaThreadBoundTransactionStrategy();

        private SoodaTransaction previousTransaction;

        private SoodaTransactionOptions transactionOptions;
        private readonly Dictionary<Type, SoodaRelationTable> _relationTables = new Dictionary<Type, SoodaRelationTable>();
        //private KeyToSoodaObjectMap _objects = new KeyToSoodaObjectMap();
        private bool _useWeakReferences = false;
        private SoodaStatistics _statistics = new SoodaStatistics();
        private readonly List<WeakSoodaObject> _objectList = new List<WeakSoodaObject>();
        private Queue _precommitQueue = null;
        private readonly List<SoodaObject> _deletedObjects = new List<SoodaObject>();
        private readonly Hashtable _precommittedClassOrRelation = new Hashtable();
        private readonly List<SoodaObject> _postCommitQueue = new List<SoodaObject>();
        private readonly List<SoodaObject> _dirtyObjects = new List<SoodaObject>();
        private readonly List<SoodaObject> _strongReferences = new List<SoodaObject>();
        private readonly Dictionary<string, List<WeakSoodaObject>> _objectsByClass = new Dictionary<string, List<WeakSoodaObject>>();
        private readonly Dictionary<string, List<WeakSoodaObject>> _dirtyObjectsByClass = new Dictionary<string, List<WeakSoodaObject>>();
        private readonly Dictionary<string, Dictionary<object, WeakSoodaObject>> _objectDictByClass = new Dictionary<string, Dictionary<object, WeakSoodaObject>>();
        private readonly StringCollection _disabledKeyGenerators = new StringCollection();

        internal readonly List<SoodaDataSource> _dataSources = new List<SoodaDataSource>();
        private readonly Dictionary<string, ISoodaObjectFactory> factoryForClassName = new Dictionary<string, ISoodaObjectFactory>();
        private readonly Dictionary<Type, ISoodaObjectFactory> factoryForType = new Dictionary<Type, ISoodaObjectFactory>();
        private readonly Dictionary<SoodaObject, NameValueCollection> _persistentValues = new Dictionary<SoodaObject, NameValueCollection>();
        private IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;
        private Assembly _assembly;
        private SchemaInfo _schema;
        internal bool _savingObjects = false;
        private bool _isPrecommit = false;
        private ISoodaCachingPolicy _cachingPolicy = SoodaCache.DefaultCachingPolicy;
        private ISoodaCache _cache = SoodaCache.DefaultCache;

        public static Assembly DefaultObjectsAssembly = null;

        #region Constructors, Dispose & Finalizer

        static SoodaTransaction()
        {
            string defaultObjectsAssembly = SoodaConfig.GetString("sooda.defaultObjectsAssembly");
            if (defaultObjectsAssembly != null)
                DefaultObjectsAssembly = Assembly.Load(defaultObjectsAssembly);
        }

        public SoodaTransaction() : this(null, SoodaTransactionOptions.Implicit, Assembly.GetCallingAssembly()) { }

        public SoodaTransaction(Assembly objectsAssembly) : this(objectsAssembly, SoodaTransactionOptions.Implicit, Assembly.GetCallingAssembly()) { }

        public SoodaTransaction(SoodaTransactionOptions options) : this(null, options, Assembly.GetCallingAssembly()) { }

        public SoodaTransaction(Assembly objectsAssembly, SoodaTransactionOptions options) : this(objectsAssembly, options, Assembly.GetCallingAssembly()) { }

        private SoodaTransaction(Assembly objectsAssembly, SoodaTransactionOptions options, Assembly callingAssembly)
        {
            if (objectsAssembly != null)
                ObjectsAssembly = objectsAssembly;

            if (ObjectsAssembly == null)
            {
                SoodaStubAssemblyAttribute[] attrs = (SoodaStubAssemblyAttribute[])callingAssembly.GetCustomAttributes(typeof(SoodaStubAssemblyAttribute), false);
                if (attrs != null && attrs.Length == 1)
                {
                    ObjectsAssembly = attrs[0].Assembly;
                }
            }

            if (ObjectsAssembly == null)
                ObjectsAssembly = DefaultObjectsAssembly;

            this.transactionOptions = options;
            if ((options & SoodaTransactionOptions.Implicit) != 0)
            {
                previousTransaction = _defaultTransactionStrategy.SetDefaultTransaction(this);
            }
        }

        ~SoodaTransaction()
        {
            Dispose(false);
        }

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private new void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                // transactionLogger.Debug("Disposing transaction");
                if (disposing)
                {
                    foreach (SoodaDataSource source in _dataSources)
                    {
                        source.Close();
                    }
                    if ((transactionOptions & SoodaTransactionOptions.Implicit) != 0)
                    {
                        if (this != _defaultTransactionStrategy.SetDefaultTransaction(previousTransaction))
                        {
                            transactionLogger.Warn("ActiveTransactionDataStoreSlot has been overwritten by someone.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                transactionLogger.Error("Error while disposing transaction {0}", ex);
                throw;
            }
        }

        #endregion

        public bool UseWeakReferences
        {
            get { return _useWeakReferences; }
            set { _useWeakReferences = value; }
        }

        public static IDefaultSoodaTransactionStrategy DefaultTransactionStrategy
        {
            get { return _defaultTransactionStrategy; }
            set { _defaultTransactionStrategy = value; }
        }

        public static SoodaTransaction ActiveTransaction
        {
            [DebuggerStepThrough]
            get
            {
                SoodaTransaction retVal = _defaultTransactionStrategy.GetDefaultTransaction();
                if (retVal == null)
                    throw new InvalidOperationException("There's no implicit transaction currently active. Either use explicit transactions or create a new implicit one.");

                return retVal;
            }
        }

        public static bool HasActiveTransaction
        {
            [DebuggerStepThrough]
            get
            {
                if (_defaultTransactionStrategy.GetDefaultTransaction() == null)
                    return false;

                return true;
            }
        }

        internal List<WeakSoodaObject> GetObjectsByClassName(string className)
        {
            List<WeakSoodaObject> objects;
            _objectsByClass.TryGetValue(className, out objects);
            return objects;
        }

        internal List<WeakSoodaObject> GetDirtyObjectsByClassName(string className)
        {
            List<WeakSoodaObject> objects;
            _dirtyObjectsByClass.TryGetValue(className, out objects);
            return objects;
        }

        public List<SoodaObject> DirtyObjects
        {
            get { return _dirtyObjects; }
        }

        private Dictionary<object, WeakSoodaObject> GetObjectDictionaryForClass(string className)
        {
            Dictionary<object, WeakSoodaObject> dict;
            if (!_objectDictByClass.TryGetValue(className, out dict))
            {
                dict = new Dictionary<object, WeakSoodaObject>();
                _objectDictByClass[className] = dict;
            }
            return dict;
        }

        private void AddObjectWithKey(string className, object keyValue, SoodaObject obj)
        {
            // Console.WriteLine("AddObjectWithKey('{0}',{1})", className, keyValue);
            if (keyValue == null) keyValue = "";
            GetObjectDictionaryForClass(className)[keyValue] = new WeakSoodaObject(obj);
        }

        private void UnregisterObjectWithKey(string className, object keyValue)
        {
            if (keyValue == null) keyValue = "";
            GetObjectDictionaryForClass(className).Remove(keyValue);
        }

        internal bool ExistsObjectWithKey(string className, object keyValue)
        {
            if (keyValue == null) keyValue = "";
            return FindObjectWithKey(className, keyValue) != null;
        }

        private SoodaObject FindObjectWithKey(string className, object keyValue)
        {
            if (keyValue == null) keyValue = "";
            WeakSoodaObject wo;
            if (!GetObjectDictionaryForClass(className).TryGetValue(keyValue, out wo))
                return null;
            return wo.TargetSoodaObject;
        }

        protected internal void RegisterObject(SoodaObject o)
        {
            // Console.WriteLine("Registering object {0}...", o.GetObjectKey());

            object pkValue = o.GetPrimaryKeyValue();
            // Console.WriteLine("Adding key: " + o.GetObjectKey() + " of type " + o.GetType());
            for (ClassInfo ci = o.GetClassInfo(); ci != null; ci = ci.InheritsFromClass)
            {
                AddObjectWithKey(ci.Name, pkValue, o);

                List<WeakSoodaObject> al;
                if (!_objectsByClass.TryGetValue(ci.Name, out al))
                {
                    al = new List<WeakSoodaObject>();
                    _objectsByClass[ci.Name] = al;
                }
                al.Add(new WeakSoodaObject(o));
            }

            if (!UseWeakReferences)
                _strongReferences.Add(o);

            _objectList.Add(new WeakSoodaObject(o));

            if (_precommitQueue != null)
                _precommitQueue.Enqueue(o);

        }

        protected internal void RegisterDirtyObject(SoodaObject o)
        {
            // transactionLogger.Debug("RegisterDirtyObject({0})", o.GetObjectKeyString());
            _dirtyObjects.Add(o);
            for (ClassInfo ci = o.GetClassInfo(); ci != null; ci = ci.InheritsFromClass)
            {
                List<WeakSoodaObject> al;
                if (!_dirtyObjectsByClass.TryGetValue(ci.Name, out al))
                {
                    al = new List<WeakSoodaObject>();
                    _dirtyObjectsByClass[ci.Name] = al;
                }
                al.Add(new WeakSoodaObject(o));
            }
        }

        protected internal bool IsRegistered(SoodaObject o)
        {
            object pkValue = o.GetPrimaryKeyValue();

            if (ExistsObjectWithKey(o.GetClassInfo().Name, pkValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RemoveWeakSoodaObjectFromCollection(List<WeakSoodaObject> collection, SoodaObject o)
        {
            for (int i = 0; i < collection.Count; ++i)
            {
                if (collection[i].TargetSoodaObject == o)
                {
                    collection.RemoveAt(i);
                    break;
                }
            }
        }

        protected internal void UnregisterObject(SoodaObject o)
        {
            object pkValue = o.GetPrimaryKeyValue();

            if (ExistsObjectWithKey(o.GetClassInfo().Name, pkValue))
            {
                UnregisterObjectWithKey(o.GetClassInfo().Name, pkValue);
                for (ClassInfo ci = o.GetClassInfo().InheritsFromClass; ci != null; ci = ci.InheritsFromClass)
                {
                    UnregisterObjectWithKey(ci.Name, pkValue);
                }
                RemoveWeakSoodaObjectFromCollection(_objectList, o);

                List<WeakSoodaObject> al;
                if (_objectsByClass.TryGetValue(o.GetClassInfo().Name, out al))
                {
                    RemoveWeakSoodaObjectFromCollection(al, o);
                }
            }
        }

        public object FindObjectWithKey(string className, object keyValue, Type expectedType)
        {
            if (keyValue == null) keyValue = "";
            object o = FindObjectWithKey(className, keyValue);
            if (o == null)
                return null;

            if (expectedType.IsAssignableFrom(o.GetType()))
                return o;
            else
            {
                // Console.WriteLine("FAILING TryGet for {0}:{1} because it's of type {2} instead of {3}", className, keyValue, o.GetType(), expectedType);
                return null;
            }
        }

        public object FindObjectWithKey(string className, int keyValue, Type expectedType)
        {
            return FindObjectWithKey(className, (object)keyValue, expectedType);
        }

        public object FindObjectWithKey(string className, long keyValue, Type expectedType)
        {
            return FindObjectWithKey(className, (object)keyValue, expectedType);
        }

        public object FindObjectWithKey(string className, string keyValue, Type expectedType)
        {
            return FindObjectWithKey(className, (object)keyValue, expectedType);
        }

        public object FindObjectWithKey(string className, Guid keyValue, Type expectedType)
        {
            return FindObjectWithKey(className, (object)keyValue, expectedType);
        }

        public void RegisterDataSource(SoodaDataSource dataSource)
        {
            dataSource.Statistics = this.Statistics;
            dataSource.IsolationLevel = IsolationLevel;
            _dataSources.Add(dataSource);
        }

        public SoodaDataSource OpenDataSource(string name)
        {
            return OpenDataSource(Schema.GetDataSourceInfo(name));
        }

        public SoodaDataSource OpenDataSource(Sooda.Schema.DataSourceInfo dataSourceInfo)
        {
            foreach (SoodaDataSource dataSource in _dataSources)
            {
                if (dataSource.Name == dataSourceInfo.Name)
                    return dataSource;
            }

            SoodaDataSource ds = (SoodaDataSource)dataSourceInfo.CreateDataSource();
            _dataSources.Add(ds);
            ds.Statistics = this.Statistics;
            ds.IsolationLevel = IsolationLevel;
            ds.Open();
            if (_savingObjects)
                ds.BeginSaveChanges();
            return ds;
        }

        public IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set
            {
                _isolationLevel = value;
                foreach (SoodaDataSource sds in _dataSources)
                {
                    sds.IsolationLevel = value;
                }
            }
        }

        void CallBeforeCommitEvents()
        {
            foreach (SoodaObject o in _dirtyObjects)
            {
                _precommitQueue.Enqueue(o);
            }

            while (_precommitQueue.Count > 0)
            {
                SoodaObject o = (SoodaObject)_precommitQueue.Dequeue();
                if (!o.IsMarkedForDelete())
                {
                    if (o.IsObjectDirty())
                    {
                        o.CallBeforeCommitEvent();
                    }
                }
            }

            _precommitQueue = null;
        }

        void CallPostcommits()
        {
            for (int i = 0; i < _postCommitQueue.Count; ++i)
            {
                _postCommitQueue[i].PostCommit();
            }
        }

        public void SaveObjectChanges()
        {
            SaveObjectChanges(true, null);
        }

        internal void MarkPrecommitted(SoodaObject o)
        {
            _precommittedClassOrRelation[o.GetClassInfo().GetRootClass().Name] = true;
        }

        internal void PrecommitObject(SoodaObject o)
        {
            if (!o.VisitedOnCommit && !o.IsMarkedForDelete())
            {
                MarkPrecommitted(o);
                o.SaveObjectChanges();
            }
        }

        internal void PrecommitRelation(RelationInfo ri)
        {
            _precommittedClassOrRelation[ri.Name] = true;
        }

        internal void SaveObjectChanges(bool isPrecommit, List<SoodaObject> objectsToPrecommit)
        {
            try
            {
                if (objectsToPrecommit == null)
                    objectsToPrecommit = _dirtyObjects;

                _isPrecommit = isPrecommit;
                foreach (SoodaDataSource source in _dataSources)
                {
                    source.BeginSaveChanges();
                }

                _savingObjects = true;

                foreach (SoodaObject o in objectsToPrecommit)
                {
                    o.VisitedOnCommit = false;
                }

                foreach (SoodaObject o in objectsToPrecommit)
                {
                    if (!o.VisitedOnCommit && !o.IsMarkedForDelete())
                    {
                        MarkPrecommitted(o);
                        o.SaveObjectChanges();
                    }
                }

                foreach (SoodaRelationTable rel in _relationTables.Values)
                {
                    rel.SaveTuples(this, isPrecommit);
                }

                foreach (SoodaDataSource source in _dataSources)
                {
                    source.FinishSaveChanges();
                }
            }
            finally
            {
                _savingObjects = false;
            }
        }

        private void Reset()
        {
            _dirtyObjects.Clear();
            _dirtyObjectsByClass.Clear();
            _objectDictByClass.Clear();
            _objectList.Clear();
            _objectsByClass.Clear();
            _relationTables.Clear();
        }

        public void Rollback()
        {
            Reset();

            // rollback all transactions on all data sources

            foreach (SoodaDataSource source in _dataSources)
            {
                source.Rollback();
            }
        }

        public void CheckCommitConditions()
        {
            foreach (SoodaObject o in _dirtyObjects)
            {
                if (o.IsObjectDirty() || o.IsInsertMode())
                    o.CheckForNulls();
            }

            foreach (SoodaObject o in _dirtyObjects)
            {
                o.CheckAssertions();
            }
        }

        public void Commit()
        {
            _precommitQueue = new Queue(_dirtyObjects.Count);
            CallBeforeCommitEvents();
            CheckCommitConditions();

            SaveObjectChanges(false, _dirtyObjects);

            // commit all transactions on all data sources

            foreach (SoodaRelationTable rel in _relationTables.Values)
            {
                rel.Commit();
            }

            foreach (SoodaDataSource source in _dataSources)
            {
                source.Commit();
            }

            using (Cache.Lock())
            {
                foreach (SoodaObject o in _dirtyObjects)
                {
                    o.InvalidateCacheAfterCommit();
                }
                foreach (SoodaRelationTable rel in _relationTables.Values)
                {
                    rel.InvalidateCacheAfterCommit(Cache);
                }
            }

            _precommittedClassOrRelation.Clear();

            CallPostcommits();

            foreach (SoodaObject o in _dirtyObjects)
            {
                o.ResetObjectDirty();
            }

            _dirtyObjects.Clear();
            _dirtyObjectsByClass.Clear();
        }

        private SoodaObject GetObject(ISoodaObjectFactory factory, string keyString)
        {
            object keyValue = factory.GetPrimaryKeyFieldHandler().RawDeserialize(keyString);
            if (keyValue == null) keyValue = "";
            return factory.GetRef(this, keyValue);
        }

        public SoodaObject GetObject(string className, string keyString)
        {
            return GetObject(GetFactory(className), keyString);
        }

        public SoodaObject GetObject(Type type, string keyString)
        {
            return GetObject(GetFactory(type), keyString);
        }

        private SoodaObject LoadObject(ISoodaObjectFactory factory, string keyString)
        {
            object keyValue = factory.GetPrimaryKeyFieldHandler().RawDeserialize(keyString);
            if (keyValue == null) keyValue = "";
            SoodaObject obj = factory.GetRef(this, keyValue);
            obj.LoadAllData();
            return obj;
        }

        public SoodaObject LoadObject(string className, string keyString)
        {
            return LoadObject(GetFactory(className), keyString);
        }

        public SoodaObject LoadObject(Type type, string keyString)
        {
            return LoadObject(GetFactory(type), keyString);
        }

        private SoodaObject GetNewObject(ISoodaObjectFactory factory)
        {
            return factory.CreateNew(this);
        }

        public SoodaObject GetNewObject(string className)
        {
            return GetNewObject(GetFactory(className));
        }

        public SoodaObject GetNewObject(Type type)
        {
            return GetNewObject(GetFactory(type));
        }

        internal SoodaRelationTable GetRelationTable(Type relationType)
        {
            SoodaRelationTable table;
            if (_relationTables.TryGetValue(relationType, out table))
                return table;

            table = (SoodaRelationTable)Activator.CreateInstance(relationType);
            _relationTables[relationType] = table;
            return table;
        }

        public Assembly ObjectsAssembly
        {
            get
            {
                return _assembly;
            }
            set
            {
                _assembly = value;

                if (_assembly != null)
                {
                    if (!_assembly.IsDefined(typeof(SoodaObjectsAssemblyAttribute), false))
                    {
                        SoodaStubAssemblyAttribute sa = (SoodaStubAssemblyAttribute)Attribute.GetCustomAttribute(_assembly, typeof(SoodaStubAssemblyAttribute), false);
                        if (sa != null)
                            _assembly = sa.Assembly;
                    }

                    SoodaObjectsAssemblyAttribute soa = (SoodaObjectsAssemblyAttribute)Attribute.GetCustomAttribute(_assembly, typeof(SoodaObjectsAssemblyAttribute), false);
                    if (soa == null)
                    {
                        throw new ArgumentException("Invalid objects assembly: " + _assembly.FullName + ". Must be the stubs assembly and define assembly:SoodaObjectsAssemblyAttribute");
                    }

                    ISoodaSchema schema = Activator.CreateInstance(soa.DatabaseSchemaType) as ISoodaSchema;
                    if (schema == null)
                        throw new ArgumentException("Invalid objects assembly: " + _assembly.FullName + ". Must define a class implementing ISoodaSchema interface.");

                    foreach (ISoodaObjectFactory fact in schema.GetFactories())
                    {
                        factoryForClassName[fact.GetClassInfo().Name] = fact;
                        factoryForType[fact.TheType] = fact;
                    }
                    _schema = schema.Schema;
                }
            }
        }

        public ISoodaObjectFactory GetFactory(string className)
        {
            return GetFactory(className, true);
        }

        public ISoodaObjectFactory GetFactory(string className, bool throwOnError)
        {
            ISoodaObjectFactory factory;
            if (!factoryForClassName.TryGetValue(className, out factory) && throwOnError)
                throw new SoodaException("Class " + className + " not registered for Sooda");
            return factory;
        }

        public ISoodaObjectFactory GetFactory(Type type)
        {
            return GetFactory(type, true);
        }

        public ISoodaObjectFactory GetFactory(Type type, bool throwOnError)
        {
            ISoodaObjectFactory factory;
            if (!factoryForType.TryGetValue(type, out factory) && throwOnError)
                throw new SoodaException("Class " + type.Name + " not registered for Sooda");
            return factory;
        }

        public ISoodaObjectFactory GetFactory(ClassInfo classInfo)
        {
            return GetFactory(classInfo, true);
        }

        public ISoodaObjectFactory GetFactory(ClassInfo classInfo, bool throwOnError)
        {
            return GetFactory(classInfo.Name, throwOnError);
        }

        internal void AddToPostCommitQueue(SoodaObject o)
        {
            if (transactionLogger.IsTraceEnabled)
                transactionLogger.Trace("Adding {0} to post-commit queue", o.GetObjectKeyString());
            _postCommitQueue.Add(o);
        }

        public string Serialize()
        {
            StringWriter sw = new StringWriter();
            Serialize(sw, SoodaSerializeOptions.DirtyOnly);
            return sw.ToString();
        }

        public string Serialize(SoodaSerializeOptions opt)
        {
            StringWriter sw = new StringWriter();
            Serialize(sw, opt);
            return sw.ToString();
        }

        public void Serialize(TextWriter tw, SoodaSerializeOptions options)
        {
            XmlTextWriter xtw = new XmlTextWriter(tw);

            xtw.Formatting = Formatting.Indented;
            Serialize(xtw, options);
        }

        static int Compare(SoodaObject o1, SoodaObject o2)
        {
            int retval = string.CompareOrdinal(o1.GetClassInfo().Name, o2.GetClassInfo().Name);
            if (retval != 0)
                return retval;

            return ((IComparable) o1.GetPrimaryKeyValue()).CompareTo(o2.GetPrimaryKeyValue());
        }

        public void Serialize(XmlWriter xw, SoodaSerializeOptions options)
        {
            xw.WriteStartElement("transaction");

            List<SoodaObject> orderedObjects = new List<SoodaObject>();
            foreach (WeakSoodaObject wr in _objectList)
            {
                SoodaObject obj = wr.TargetSoodaObject;
                if (obj != null)
                    orderedObjects.Add(obj);
            }

            if ((options & SoodaSerializeOptions.Canonical) != 0)
            {
                orderedObjects.Sort(Compare);
            }

            foreach (SoodaObject o in DeletedObjects)
            {
                o.PreSerialize(xw, options);
            }
            foreach (SoodaObject o in orderedObjects)
            {
                if (!o.IsMarkedForDelete())
                {
                    if (o.IsObjectDirty() || (options & SoodaSerializeOptions.IncludeNonDirtyObjects) != 0)
                        o.PreSerialize(xw, options);
                }
            }
            foreach (SoodaObject o in orderedObjects)
            {
                if (o.IsObjectDirty() || (options & SoodaSerializeOptions.IncludeNonDirtyObjects) != 0)
                    o.Serialize(xw, options);
            }
            // serialize N-N relation tables
            foreach (SoodaRelationTable rel in _relationTables.Values)
            {
                rel.Serialize(xw, options);
            }
            xw.WriteEndElement();
        }

        public void Deserialize(string s)
        {
            StringReader sr = new StringReader(s);
            XmlTextReader reader = new XmlTextReader(sr);

            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            Deserialize(reader);
        }

        public void Deserialize(XmlReader reader)
        {
            Reset();

            SoodaObject currentObject = null;
            SoodaRelationTable currentRelation = null;
            bool inDebug = false;

            // state data for just-being-read object

            bool objectForcePostCommit = false;
            bool objectDisableObjectTriggers = false;
            bool objectDelete = false;
            string objectClassName;
            string objectMode = null;
            object[] objectPrimaryKey = null;
            ClassInfo objectClassInfo;
            ISoodaObjectFactory objectFactory = null;
            int objectKeyCounter = 0;
            int objectTotalKeyCounter = 0;

            try
            {
                _savingObjects = true;

                // in case we get any "deleteobject" which require us to delete the objects 
                // within transaction
                foreach (SoodaDataSource source in _dataSources)
                {
                    source.BeginSaveChanges();
                }

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && !inDebug)
                    {
                        switch (reader.Name)
                        {
                            case "field":
                                if (currentObject == null)
                                    throw new Exception("Field without an object during deserialization!");

                                currentObject.DeserializeField(reader);
                                break;

                            case "persistent":
                                if (currentObject == null)
                                    throw new Exception("Field without an object during deserialization!");

                                currentObject.DeserializePersistentField(reader);
                                break;

                            case "object":
                                if (currentObject != null)
                                {
                                    // end deserialization

                                    currentObject.EnableFieldUpdateTriggers();
                                    currentObject = null;
                                };

                                objectKeyCounter = 0;
                                objectForcePostCommit = false;
                                objectDisableObjectTriggers = false;
                                objectClassName = reader.GetAttribute("class");
                                objectMode = reader.GetAttribute("mode");
                                objectDelete = false;
                                objectFactory = GetFactory(objectClassName);
                                objectClassInfo = objectFactory.GetClassInfo();
                                objectTotalKeyCounter = objectClassInfo.GetPrimaryKeyFields().Length;
                                if (objectTotalKeyCounter > 1)
                                    objectPrimaryKey = new object[objectTotalKeyCounter];
                                if (reader.GetAttribute("forcepostcommit") != null)
                                    objectForcePostCommit = true;
                                if (reader.GetAttribute("disableobjecttriggers") != null)
                                    objectDisableObjectTriggers = true;
                                if (reader.GetAttribute("delete") != null)
                                    objectDelete = true;
                                break;

                            case "key":
                                int ordinal = Convert.ToInt32(reader.GetAttribute("ordinal"));
                                object val = objectFactory.GetFieldHandler(ordinal).RawDeserialize(reader.GetAttribute("value"));

                                if (objectTotalKeyCounter > 1)
                                {
                                    objectPrimaryKey[objectKeyCounter] = val;
                                }

                                objectKeyCounter++;

                                if (objectKeyCounter == objectTotalKeyCounter)
                                {
                                    object primaryKey;

                                    if (objectTotalKeyCounter == 1)
                                    {
                                        primaryKey = val;
                                    }
                                    else
                                    {
                                        primaryKey = new SoodaTuple(objectPrimaryKey);
                                    }

                                    currentObject = BeginObjectDeserialization(objectFactory, primaryKey, objectMode);
                                    if (objectForcePostCommit)
                                        currentObject.ForcePostCommit();
                                    if (objectDisableObjectTriggers)
                                        currentObject.DisableObjectTriggers();
                                    currentObject.DisableFieldUpdateTriggers();
                                    if (objectDelete)
                                    {
                                        DeletedObjects.Add(currentObject);
                                        currentObject.DeleteMarker = true;
                                        currentObject.CommitObjectChanges();
                                        currentObject.SetObjectDirty();
                                    }
                                }
                                break;

                            case "transaction":
                                break;

                            case "relation":
                                currentRelation = GetRelationFromXml(reader);
                                break;

                            case "tuple":
                                currentRelation.DeserializeTuple(reader);
                                break;

                            case "debug":
                                if (!reader.IsEmptyElement)
                                {
                                    inDebug = true;
                                }
                                break;

                            default:
                                throw new NotImplementedException("Element not implemented in deserialization: " + reader.Name);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "debug")
                        {
                            inDebug = false;
                        }
                        else if (reader.Name == "object")
                        {
                            currentObject.EnableFieldUpdateTriggers();
                        }
                    }
                }

                foreach (WeakSoodaObject wr in _objectList)
                {
                    SoodaObject ob = wr.TargetSoodaObject;
                    if (ob != null)
                    {
                        ob.AfterDeserialize();
                    }
                }
            }
            finally
            {
                _savingObjects = false;

                foreach (SoodaDataSource source in _dataSources)
                {
                    source.FinishSaveChanges();
                }
            }
        }

        private SoodaRelationTable GetRelationFromXml(XmlReader reader)
        {
            string className = reader.GetAttribute("type");
            Type t = Type.GetType(className, true, false);
            ConstructorInfo ci = t.GetConstructor(new Type[] { });

            SoodaRelationTable retVal = (SoodaRelationTable)ci.Invoke(new object[] { });
            _relationTables[t] = retVal;
            retVal.BeginDeserialization(Int32.Parse(reader.GetAttribute("tupleCount")));
            return retVal;
        }

        private SoodaObject BeginObjectDeserialization(ISoodaObjectFactory factory, object pkValue, string mode)
        {
            SoodaObject retVal = null;

            retVal = factory.TryGet(this, pkValue);
            if (retVal == null)
            {
                if (mode == "update")
                {
                    transactionLogger.Debug("Object not found. GetRef() ing");
                    retVal = factory.GetRef(this, pkValue);
                }
                else
                {
                    transactionLogger.Debug("Object not found. Getting new raw object.");
                    retVal = factory.GetRawObject(this);
                    Statistics.RegisterObjectUpdate();
                    SoodaStatistics.Global.RegisterObjectUpdate();
                    retVal.SetPrimaryKeyValue(pkValue);
                    retVal.SetInsertMode();
                }
            }
            else
            {
                if (mode == "insert")
                {
                    retVal.SetInsertMode();
                }
            }

            return retVal;
        }

        public SchemaInfo Schema
        {
            get { return _schema; }
        }

        public static ISoodaObjectFactoryCache SoodaObjectFactoryCache = new SoodaObjectFactoryCache();

        internal NameValueCollection GetPersistentValues(SoodaObject obj)
        {
            NameValueCollection dict;
            _persistentValues.TryGetValue(obj, out dict);
            return dict;
        }

        internal string GetPersistentValue(SoodaObject obj, string name)
        {
            NameValueCollection dict;
            if (!_persistentValues.TryGetValue(obj, out dict))
                return null;
            return dict[name];
        }

        internal void SetPersistentValue(SoodaObject obj, string name, string value)
        {
            NameValueCollection dict;
            if (!_persistentValues.TryGetValue(obj, out dict))
            {
                dict = new NameValueCollection();
                _persistentValues.Add(obj, dict);
            }
            dict[name] = value;
        }

        internal bool IsPrecommit
        {
            get { return _isPrecommit; }
        }

        public SoodaStatistics Statistics
        {
            get { return _statistics; }
        }

        internal bool HasBeenPrecommitted(ClassInfo ci)
        {
            return _precommittedClassOrRelation.Contains(ci.GetRootClass().Name);
        }

        internal bool HasBeenPrecommitted(RelationInfo ri)
        {
            return _precommittedClassOrRelation.Contains(ri.Name);
        }

        public List<SoodaObject> DeletedObjects
        {
            get { return _deletedObjects; }
        }

        public ISoodaCachingPolicy CachingPolicy
        {
            get { return _cachingPolicy; }
            set { _cachingPolicy = value; }
        }

        public ISoodaCache Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public bool IsKeyGeneratorDisabled(string className)
        {
            return _disabledKeyGenerators.Contains(className);
        }

        private class RevertDisableKeyGenerators : IDisposable
        {
            private SoodaTransaction _trans;
            private StringCollection _classNames;

            internal RevertDisableKeyGenerators(SoodaTransaction trans, StringCollection classNames)
            {
                _trans = trans;
                _classNames = classNames;
            }

            public void Dispose()
            {
                foreach (string className in _classNames)
                    _trans._disabledKeyGenerators.Remove(className);
            }
        }

        public IDisposable DisableKeyGenerators(params string[] classNames)
        {
            StringCollection disabled = new StringCollection();
            foreach (string className in classNames)
            {
                if (_disabledKeyGenerators.Contains(className))
                    continue;
                _disabledKeyGenerators.Add(className);
                disabled.Add(className);
            }
            return new RevertDisableKeyGenerators(this, disabled);
        }

        internal IEnumerable LoadCollectionFromCache(string cacheKey, Logger logger)
        {
            IEnumerable keysCollection = this.Cache.LoadCollection(cacheKey);
            if (keysCollection != null)
            {
                SoodaStatistics.Global.RegisterCollectionCacheHit();
                this.Statistics.RegisterCollectionCacheHit();
            }
            else if (cacheKey != null)
            {
                logger.Debug("Cache miss. {0} not found in cache.", cacheKey);
                SoodaStatistics.Global.RegisterCollectionCacheMiss();
                this.Statistics.RegisterCollectionCacheMiss();
            }
            return keysCollection;
        }

        internal void StoreCollectionInCache(string cacheKey, ClassInfo classInfo, IList list, string[] dependentClasses, bool evictWhenItemRemoved, TimeSpan expirationTimeout, bool slidingExpiration)
        {
            object[] keys = new object[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                keys[i] = ((SoodaObject) list[i]).GetPrimaryKeyValue();
            }

            this.Cache.StoreCollection(cacheKey, classInfo.GetRootClass().Name, keys, dependentClasses, evictWhenItemRemoved, expirationTimeout, slidingExpiration);
        }
    }
}
