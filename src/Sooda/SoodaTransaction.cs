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
using System.Reflection;
using System.Collections;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

using Sooda.Schema;

using Sooda.ObjectMapper;
using Sooda.Collections;

using Sooda.Logging;

namespace Sooda 
{
    [Flags]
    public enum SoodaTransactionOptions
    {
        NoImplicit = 0x0000,
        Implicit = 0x0001,
    }

    public class SoodaTransaction : IDisposable 
    {
        private static Logger transactionLogger = LogManager.GetLogger("Sooda.Transaction");
        private static LocalDataStoreSlot g_activeTransactionDataStoreSlot = System.Threading.Thread.AllocateDataSlot();

        private SoodaTransaction previousTransaction;

        private SoodaTransactionOptions transactionOptions;
        private TypeToSoodaRelationTableAssociation _relationTables = new TypeToSoodaRelationTableAssociation();
        //private KeyToSoodaObjectMap _objects = new KeyToSoodaObjectMap();
        private bool _useWeakReferences = false;
        private SoodaStatistics _statistics = new SoodaStatistics();
        private WeakSoodaObjectCollection _objectList = new WeakSoodaObjectCollection();
        private Queue _precommitQueue = null;
        private SoodaObjectCollection _deletedObjects = new SoodaObjectCollection();
        private Hashtable _precommittedClass = new Hashtable();
        private SoodaObjectCollection _postCommitQueue = null;
        private SoodaObjectCollection _dirtyObjects = new SoodaObjectCollection();
        private SoodaObjectCollection _strongReferences = new SoodaObjectCollection();
        private StringToWeakSoodaObjectCollectionAssociation _objectsByClass = new StringToWeakSoodaObjectCollectionAssociation();
        private StringToWeakSoodaObjectCollectionAssociation _dirtyObjectsByClass = new StringToWeakSoodaObjectCollectionAssociation();
        private StringToObjectToWeakSoodaObjectAssociation _objectDictByClass = new StringToObjectToWeakSoodaObjectAssociation();

        internal SoodaDataSourceCollection _dataSources = new SoodaDataSourceCollection();
        private StringToISoodaObjectFactoryAssociation factoryForClassName = new StringToISoodaObjectFactoryAssociation();
        private TypeToISoodaObjectFactoryAssociation factoryForType = new TypeToISoodaObjectFactoryAssociation();
        private SoodaObjectToNameValueCollectionAssociation _persistentValues = new SoodaObjectToNameValueCollectionAssociation();
        private Assembly _assembly;
        private SchemaInfo _schema;
        internal bool _savingObjects = false;
        private bool _isPrecommit = false;

        public static Assembly DefaultObjectsAssembly = null;

#region Constructors, Dispose & Finalizer

        public SoodaTransaction() : this(null, SoodaTransactionOptions.Implicit, Assembly.GetCallingAssembly()) {}

        public SoodaTransaction(Assembly objectsAssembly) : this(objectsAssembly, SoodaTransactionOptions.Implicit, Assembly.GetCallingAssembly()) {}

        public SoodaTransaction(SoodaTransactionOptions options) : this(null, options, Assembly.GetCallingAssembly()) {}

        public SoodaTransaction(Assembly objectsAssembly, SoodaTransactionOptions options) : this(objectsAssembly, options, Assembly.GetCallingAssembly()) {}

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

            if (ObjectsAssembly == null)
                throw new SoodaException(@"ObjectsAssembly has not been set for this SoodaTransaction. See http://www.sooda.org/en/faq.html#objectsassembly for more information.");

            this.transactionOptions = options;
            if ((options & SoodaTransactionOptions.Implicit) != 0) 
            {
                previousTransaction = (SoodaTransaction)System.Threading.Thread.GetData(g_activeTransactionDataStoreSlot);
                System.Threading.Thread.SetData(g_activeTransactionDataStoreSlot, this);
            }
        }

        ~SoodaTransaction() 
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) 
        {
            try
            {
                // transactionLogger.Debug("Disposing transaction");
                if (disposing) 
                {
                    foreach (SoodaDataSource source in _dataSources) 
                    {
                        source.Close();
                    }
                };
                if ((transactionOptions & SoodaTransactionOptions.Implicit) != 0) 
                {
                    if (System.Threading.Thread.GetData(g_activeTransactionDataStoreSlot) != this) 
                    {
                        transactionLogger.Warn("ActiveTransactionDataStoreSlot has been overwritten by someone.");
                    } 
                    System.Threading.Thread.SetData(g_activeTransactionDataStoreSlot, previousTransaction);
                }
                transactionLogger.Debug("Transaction stats:\n\n{0}", Statistics);
                // transactionLogger.Debug("Disposed.");
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

        public static SoodaTransaction ActiveTransaction
        {
            [DebuggerStepThrough]
                get 
                {
                    SoodaTransaction retVal = (SoodaTransaction)System.Threading.Thread.GetData(g_activeTransactionDataStoreSlot);

                    if (retVal == null) 
                    {
                        throw new InvalidOperationException("There's no implicit transaction currently active. Either use explicit transactions or create a new implicit one.");
                    }

                    return retVal;
                }
        }

        internal WeakSoodaObjectCollection GetObjectsByClassName(string className) 
        {
            return _objectsByClass[className];
        }

        internal WeakSoodaObjectCollection GetDirtyObjectsByClassName(string className) 
        {
            return _dirtyObjectsByClass[className];
        }

        public SoodaObjectCollection DirtyObjects
        {
            get { return _dirtyObjects; }
        }

        private ObjectToWeakSoodaObjectAssociation GetObjectDictionaryForClass(string className) 
        {
            ObjectToWeakSoodaObjectAssociation dict = _objectDictByClass[className];
            if (dict == null) 
            {
                dict = new ObjectToWeakSoodaObjectAssociation();
                _objectDictByClass[className] = dict;
            }
            return dict;
        }

        private void AddObjectWithKey(string className, object keyValue, SoodaObject obj) 
        {
            // Console.WriteLine("AddObjectWithKey('{0}',{1})", className, keyValue);
            GetObjectDictionaryForClass(className)[keyValue] = new WeakSoodaObject(obj);
        }

        private void UnregisterObjectWithKey(string className, object keyValue) 
        {
            GetObjectDictionaryForClass(className).Remove(keyValue);
        }

        internal bool ExistsObjectWithKey(string className, object keyValue) 
        {
            return FindObjectWithKey(className, keyValue) != null;
        }

        private SoodaObject FindObjectWithKey(string className, object keyValue) 
        {
            WeakSoodaObject wo = GetObjectDictionaryForClass(className)[keyValue];
            if (wo == null)
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

                WeakSoodaObjectCollection al = _objectsByClass[ci.Name];
                if (al == null) 
                {
                    al = new WeakSoodaObjectCollection();
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
            transactionLogger.Debug("RegisterDirtyObject({0})", o.GetObjectKeyString());
            _dirtyObjects.Add(o);
            for (ClassInfo ci = o.GetClassInfo(); ci != null; ci = ci.InheritsFromClass) 
            {
                WeakSoodaObjectCollection al = _dirtyObjectsByClass[ci.Name];
                if (al == null) 
                {
                    al = new WeakSoodaObjectCollection();
                    _dirtyObjectsByClass[ci.Name] = al;
                }
                al.Add(new WeakSoodaObject(o));
            }
        }

        protected internal bool IsRegistered(SoodaObject o) 
        {
            ObjectToWeakSoodaObjectAssociation classDict = _objectDictByClass[o.GetClassInfo().Name];
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

        private void RemoveWeakSoodaObjectFromCollection(WeakSoodaObjectCollection collection, SoodaObject o)
        {
            for (int i = 0; i < collection.Count; ++i)
            {
                SoodaObject obj = collection[i].TargetSoodaObject;
                if (obj == o)
                {
                    collection.RemoveAt(i);
                    break;
                }
            }
        }

        protected internal void UnregisterObject(SoodaObject o) 
        {
            ObjectToWeakSoodaObjectAssociation classDict = _objectDictByClass[o.GetClassInfo().Name];
            object pkValue = o.GetPrimaryKeyValue();

            if (ExistsObjectWithKey(o.GetClassInfo().Name, pkValue)) 
            {
                UnregisterObjectWithKey(o.GetClassInfo().Name, pkValue);
                for (ClassInfo ci = o.GetClassInfo().InheritsFromClass; ci != null; ci = ci.InheritsFromClass) 
                {
                    UnregisterObjectWithKey(ci.Name, pkValue);
                }
                RemoveWeakSoodaObjectFromCollection(_objectList, o);

                WeakSoodaObjectCollection al = _objectsByClass[o.GetClassInfo().Name];
                if (al != null) 
                {
                    RemoveWeakSoodaObjectFromCollection(al, o);
                }
            }
        }

        public object FindObjectWithKey(string className, object keyValue, Type expectedType) 
        {
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

        public void RegisterDataSource(SoodaDataSource dataSource) 
        {
            _dataSources.Add(dataSource);
        }

        public SoodaDataSource OpenDataSource(string name) 
        {
            return OpenDataSource(Schema.GetDataSourceInfo(name));
        }

        public SoodaDataSource OpenDataSource(Sooda.Schema.DataSourceInfo dataSourceInfo) 
        {
            for (int i = 0; i < _dataSources.Count; ++i)
            {
                if (_dataSources[i].DataSourceInfo.Name == dataSourceInfo.Name)
                    return _dataSources[i];
            }

            SoodaDataSource ds = (SoodaDataSource)dataSourceInfo.CreateDataSource();
            _dataSources.Add(ds);
            ds.Statistics = this.Statistics;
            ds.Open();
            if (_savingObjects)
                ds.BeginSaveChanges();
            return ds;
        }

        void CallPrecommits() 
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
                        _precommittedClass[o.GetClassInfo()] = true;
                        o.PreCommit();
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

        protected static internal void SaveOuterReferences(SoodaObject theObject, string fieldName, object fieldValue, bool isDirty, ref SoodaObject refcache, ISoodaObjectFactory factory, object context) 
        {
            if (fieldValue != null) 
            {
                SoodaObject obj = SoodaObjectImpl.TryGetRefFieldValue(ref refcache, fieldValue, theObject.GetTransaction(), factory);
                if (obj != null && (object)obj != (object)theObject) 
                {
                    if (obj.IsInsertMode())
                    {
                        if (obj.VisitedOnCommit && !obj.WrittenIntoDatabase)
                        {
                            throw new Exception("Cyclic reference between " + theObject.GetObjectKeyString() + " and " + obj.GetObjectKeyString());
                            // cyclic reference
                        } 
                        else 
                        {
                            SaveObjectChanges(obj);
                        }
                    }
                }
            };
        }

        private static SoodaObjectRefFieldIterator saveOuterReferencesIterator = new SoodaObjectRefFieldIterator(SoodaTransaction.SaveOuterReferences);

        static void SaveObjectChanges(SoodaObject o) 
        {
            o.VisitedOnCommit = true;
            o.IterateOuterReferences(saveOuterReferencesIterator, null);
            if (o.WrittenIntoDatabase)
                return ;

            if ((o.IsObjectDirty() || o.IsInsertMode()) && !o.WrittenIntoDatabase) 
            {
                // deletes are performed in a separate pass
                if (!o.IsMarkedForDelete())
                {
                    o.CommitObjectChanges();
                }
                o.WrittenIntoDatabase = true;
            } 
            else if (o.PostCommitForced)
            {
                o.GetTransaction().AddToPostCommitQueue(o);
            }
        }

        internal void SaveObjectChanges(bool isPrecommit, SoodaObjectCollection objectsToPrecommit)
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
                    if (!o.VisitedOnCommit && !o.IsMarkedForDelete()) 
                    {
                        SaveObjectChanges(o);
                    }
                }

                if (_relationTables != null) 
                {
                    foreach (SoodaRelationTable rel in _relationTables.Values) 
                    {
                        rel.SaveTuples(this);
                    }
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
            CallPrecommits();
            CheckCommitConditions();

            _postCommitQueue = new SoodaObjectCollection();
            // TODO - prealloc

            SaveObjectChanges(false, _dirtyObjects);

            // commit all transactions on all data sources

            if (_relationTables != null) 
            {
                foreach (SoodaRelationTable rel in _relationTables.Values) 
                {
                    rel.Commit();
                }
            }

            foreach (SoodaDataSource source in _dataSources) 
            {
                source.Commit();
                // source.Rollback();
            }

            _precommittedClass.Clear();

            CallPostcommits();

            foreach (SoodaObject o in _dirtyObjects) 
            {
                o.ResetObjectDirty();
            }

            _dirtyObjects.Clear();
            _dirtyObjectsByClass.Clear();
        }

        private void _Reset() 
        {
            _objectList.Clear();
            _objectsByClass.Clear();
            _precommitQueue.Clear();
            _relationTables.Clear();
            _precommittedClass.Clear();
        }

        private static object[] relationTableConstructorArguments = new object[0] { };

        private SoodaObject GetObject(ISoodaObjectFactory factory, string keyString) 
        {
            object keyValue = factory.GetPrimaryKeyFieldHandler().RawDeserialize(keyString);
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

        private SoodaObject LoadObject(ISoodaObjectFactory factory , string keyString) 
        {
            object keyValue = factory.GetPrimaryKeyFieldHandler().RawDeserialize(keyString);
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
            SoodaRelationTable table = _relationTables[relationType];
            if (table != null)
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
            if (throwOnError && !factoryForClassName.Contains(className))
                throw new SoodaException("Class " + className + " not registered for Sooda");
            return factoryForClassName[className];
        }

        public ISoodaObjectFactory GetFactory(Type type) 
        {
            return GetFactory(type, true);
        }

        public ISoodaObjectFactory GetFactory(Type type, bool throwOnError) 
        {
            if (throwOnError && !factoryForType.Contains(type))
                throw new SoodaException("Class " + type.Name + " not registered for Sooda");
            return factoryForType[type];
        }

        public ISoodaObjectFactory GetFactory(ClassInfo classInfo) 
        {
            return GetFactory(classInfo, true);
        }

        public ISoodaObjectFactory GetFactory(ClassInfo classInfo, bool throwOnError) 
        {
            if (throwOnError && !factoryForClassName.Contains(classInfo.Name))
                throw new SoodaException("Class " + classInfo.Name + " not registered for Sooda");
            return factoryForClassName[classInfo.Name];
        }

        internal void AddToPostCommitQueue(SoodaObject o) 
        {
            if (_postCommitQueue != null)
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

        internal class KeyStringComparer : IComparer 
        {
            public int Compare(object x, object y) 
            {
                SoodaObject o1 = (SoodaObject)x;
                SoodaObject o2 = (SoodaObject)y;

                int retval = String.CompareOrdinal(o1.GetClassInfo().Name, o2.GetClassInfo().Name);
                if (retval != 0)
                    return retval;

                retval = ((IComparable)o1.GetPrimaryKeyValue()).CompareTo(o2.GetPrimaryKeyValue());
                if (retval != 0)
                    return retval;

                return 0;
            }
        }

        public void Serialize(XmlWriter xw, SoodaSerializeOptions options) 
        {
            xw.WriteStartElement("transaction");

            SoodaObjectCollection orderedObjects = new SoodaObjectCollection();
            foreach (WeakSoodaObject wr in _objectList)
            {
                SoodaObject obj = wr.TargetSoodaObject;
                if (obj != null)
                    orderedObjects.Add(obj);
            }

            if ((options & SoodaSerializeOptions.Canonical) != 0) 
            {
                ArrayList al = new ArrayList(orderedObjects);
                al.Sort(new KeyStringComparer());
                orderedObjects = new SoodaObjectCollection((SoodaObject[])al.ToArray(typeof(SoodaObject)));
            }

            foreach (SoodaObject o in DeletedObjects) 
            {
                o.PreSerialize(xw, options);
            }
            foreach (SoodaObject o in orderedObjects) 
            {
                if (!o.IsMarkedForDelete())
                {
                    if (o.IsObjectDirty() || ((options & SoodaSerializeOptions.IncludeNonDirtyObjects) != 0))
                        o.PreSerialize(xw, options);
                }
            }
            foreach (SoodaObject o in orderedObjects) 
            {
                if (o.IsObjectDirty() || ((options & SoodaSerializeOptions.IncludeNonDirtyObjects) != 0))
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

                                    currentObject.DisableTriggers = false;
                                    currentObject = null;
                                };

                            objectKeyCounter = 0;
                            objectForcePostCommit = false;
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
                                currentObject.DisableTriggers = true;
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
                            currentObject.DisableTriggers = false;
                        }
                    }
                };

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

            SoodaRelationTable retVal = (SoodaRelationTable)ci.Invoke(new object[] {});
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
            return _persistentValues[obj];
        }

        internal string GetPersistentValue(SoodaObject obj, string name)
        {
            NameValueCollection dict = _persistentValues[obj];
            if (dict == null)
                return null;
            return dict[name];
        }

        internal void SetPersistentValue(SoodaObject obj, string name, string value)
        {
            NameValueCollection dict = _persistentValues[obj];
            if (dict == null)
            {
                dict = new NameValueCollection();
                _persistentValues[obj] = dict;
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
			return _precommittedClass.Contains(ci);
		}

        public SoodaObjectCollection DeletedObjects
        {
            get { return _deletedObjects; }
        }
    }
}
