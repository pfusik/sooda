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
using System.IO;
using System.Collections;
using System.Threading;

using Sooda.Logging;
using Sooda.Schema;
using Sooda.QL;

namespace Sooda.Caching
{
    public class SoodaInProcessCache : ISoodaCache
    {
        private ReaderWriterLock _rwlock = new ReaderWriterLock();
        private Hashtable _objectCache = new Hashtable();

        // string -> (SoodaCachedCollectionKey->object)
        private Hashtable _class2keys = new Hashtable();
        private object _marker = new object();

        private SoodaCachedCollectionHash _collectionCache = new SoodaCachedCollectionHash();
        private static Logger logger = LogManager.GetLogger("Sooda.Cache");

        private TimeSpan _expirationTimeout = TimeSpan.FromMinutes(1);
        private static bool Enabled = true;

        public TimeSpan ExpirationTimeout
        {
            get { return _expirationTimeout; }
            set { _expirationTimeout = value; }
        }

        SoodaCacheEntry ISoodaCache.Find(string className, object primaryKeyValue)
        {
            _rwlock.AcquireReaderLock(-1);
            try
            {
                Hashtable ht = (Hashtable)_objectCache[className];
                SoodaCacheEntry retVal = null;

                if (ht != null)
                {
                    retVal = (SoodaCacheEntry)ht[primaryKeyValue];
                    if (retVal != null)
                    {
                        if (retVal.Age > ExpirationTimeout)
                        {
                            ht.Remove(primaryKeyValue);
                            retVal = null;
                        }
                    }
                }
                return retVal;
            }
            finally
            {
                _rwlock.ReleaseReaderLock();
            }
        }

        void ISoodaCache.Add(string className, object primaryKeyValue, SoodaCacheEntry entry)
        {
            _rwlock.AcquireWriterLock(-1);
            try
            {
                Hashtable ht = (Hashtable)_objectCache[className];
                if (ht == null)
                {
                    ht = new Hashtable();
                    _objectCache[className] = ht;
                }

                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Add {0}({1}): {2}", className, primaryKeyValue, entry.ToString());
                }

                ht[primaryKeyValue] = entry;
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        void ISoodaCache.Invalidate(string className, object primaryKeyValue, SoodaCacheInvalidateReason reason)
        {
            if (!Enabled)
                return;

            if (logger.IsTraceEnabled)
            {
                logger.Trace("Invalidating object {0}[{1}]. Reason: {2}", className, primaryKeyValue, reason);
            }

            _rwlock.AcquireWriterLock(-1);
            try
            {
                Hashtable ht = (Hashtable)_objectCache[className];
                if (ht != null)
                {
                    // no exception when the key doesn't exist
                    ht.Remove(primaryKeyValue);
                }
                ht = (Hashtable)_class2keys[className];
                if (ht != null)
                {
                    _class2keys.Remove(className);
                    foreach (SoodaCachedCollectionKey key in ht.Keys)
                    {
                        _collectionCache.Remove(key);
                    }
                }
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        void ISoodaCache.Clear()
        {
            _rwlock.AcquireWriterLock(-1);
            try
            {
                _objectCache.Clear();
                _collectionCache.Clear();
                _class2keys.Clear();
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        IList ISoodaCache.LoadCollection(SoodaCachedCollectionKey key)
        {
            if (key == null)
                return null;

            SoodaCachedCollection coll = _collectionCache[key];
            if (coll == null)
                return null;

            return coll.PrimaryKeys;
        }

        private void RegisterDependentCollectionClass(SoodaCachedCollectionKey cacheKey, ClassInfo dependentClass)
        {
            Hashtable ht = (Hashtable)_class2keys[dependentClass.Name];
            if (ht == null)
            {
                ht = new Hashtable();
                _class2keys[dependentClass.Name] = ht;
            }

            // this is actually a set
            ht[cacheKey] = _marker;
        }

        void ISoodaCache.StoreCollection(SoodaCachedCollectionKey cacheKey, IList primaryKeys, ClassInfoCollection dependentClasses)
        {
            if (cacheKey != null)
            {
                _rwlock.AcquireWriterLock(-1);
                try
                {
                    logger.Debug("Storing collection: {0} {1} items", cacheKey, primaryKeys.Count);
                    if (dependentClasses != null)
                    {
                        logger.Debug("Dependent classes: {0}", dependentClasses.Count);
                    }
                    SoodaCachedCollection value = new SoodaCachedCollection(primaryKeys);
                    _collectionCache[cacheKey] = value;

                    RegisterDependentCollectionClass(cacheKey, cacheKey.ClassInfo);
                    if (dependentClasses != null)
                    {
                        for (int i = 0; i < dependentClasses.Count; ++i)
                        {
                            RegisterDependentCollectionClass(cacheKey, dependentClasses[i]);
                        }
                    }
                }
                finally
                {
                    _rwlock.ReleaseWriterLock();
                }
            }
        }

        public void Dump(TextWriter output)
        {
            output.WriteLine("CACHE DUMP:");
            foreach (string className in _objectCache.Keys)
            {
                output.WriteLine(className);

                foreach (DictionaryEntry de in (Hashtable)_objectCache[className])
                {
                    SoodaCacheEntry entry = (SoodaCacheEntry)de.Value;

                    output.Write("{0,8} [", de.Key);
                    bool first = true;
                    for (int i = 0; i < entry.Data.Length; ++i)
                    {
                        object fd = entry.Data.GetBoxedFieldValue(i);
                        if (!first)
                            output.Write("|");
                        output.Write(fd);
                        first = false;
                    }
                    output.Write("]");
                    output.WriteLine();
                }
                output.WriteLine();
            }
            output.WriteLine();
        }

        ISoodaCacheLock ISoodaCache.Lock()
        {
            _rwlock.AcquireWriterLock(-1);
            return new EndCommitCaller(this);
        }

        void ISoodaCache.Unlock(ISoodaCacheLock theLock)
        {
            ((EndCommitCaller)theLock).Dispose();
        }

        internal void DoUnlock()
        {
            _rwlock.ReleaseWriterLock();
        }

        class EndCommitCaller : ISoodaCacheLock
        {
            private SoodaInProcessCache _sipc;

            public EndCommitCaller(SoodaInProcessCache sipc)
            {
                _sipc = sipc;
            }

            public void Dispose()
            {
                _sipc.DoUnlock();
            }
        }
    }
}
