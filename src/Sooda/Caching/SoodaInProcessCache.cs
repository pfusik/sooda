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
        private LruCache _objectCache = new LruCache(-1, TimeSpan.FromMinutes(5), false);

        // string -> (LruCache(SoodaCachedCollectionKey))
        private Hashtable _collectionsDependentOnClass = new Hashtable();
        private object _marker = new object();

        private LruCache _collectionCache = new LruCache(-1, TimeSpan.FromMinutes(5), false);
        private static Logger logger = LogManager.GetLogger("Sooda.Cache");

        private TimeSpan _expirationTimeout = TimeSpan.FromMinutes(1);
        private static bool Enabled = true;

        public TimeSpan ExpirationTimeout
        {
            get { return _objectCache.TimeToLive; }
            set { _objectCache.TimeToLive = value; }
        }

        public SoodaInProcessCache()
        {
            _objectCache.ItemRemoved += new LruCacheDelegate(_objectCache_ItemRemoved);
            _collectionCache.ItemRemoved += new LruCacheDelegate(_collectionCache_ItemRemoved);
        }

        void _collectionCache_ItemRemoved(object sender, LruCacheEventArgs args)
        {
            logger.Trace("Collection removed: {0}", args.Key);
        }

        void _objectCache_ItemRemoved(object sender, LruCacheEventArgs args)
        {
            SoodaCacheEntry e = (SoodaCacheEntry)args.Value;
            logger.Trace("Item removed: {0}. Invalidating dependent collections...", e);
            IList l = e.GetDependentCollections();
            if (l != null)
            {
                foreach (SoodaCachedCollection ck in e.GetDependentCollections())
                {
                    InvalidateCollection(ck);
                }
            }
        }

        public SoodaCacheEntry Find(string className, object primaryKeyValue)
        {
            _rwlock.AcquireReaderLock(-1);
            try
            {
                SoodaCacheKey cacheKey = new SoodaCacheKey(className, primaryKeyValue);
                return (SoodaCacheEntry)_objectCache[cacheKey];
            }
            finally
            {
                _rwlock.ReleaseReaderLock();
            }
        }

        public void Add(string className, object primaryKeyValue, SoodaCacheEntry entry)
        {
            _rwlock.AcquireWriterLock(-1);
            try
            {
                SoodaCacheKey cacheKey = new SoodaCacheKey(className, primaryKeyValue);
                _objectCache[cacheKey] = entry;
                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Adding {0} to cache.", cacheKey);
                }
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        public void InvalidateCollection(SoodaCachedCollection collection)
        {
            LruCache cachedCollectionsDependentOnClass = 
                (LruCache)_collectionsDependentOnClass[collection.RootClassName];
            cachedCollectionsDependentOnClass.Remove(collection.CollectionKey);
            _collectionCache.Remove(collection.CollectionKey);
        }

        public void Invalidate(string className, object primaryKeyValue, SoodaCacheInvalidateReason reason)
        {
            if (!Enabled)
                return;

            if (logger.IsTraceEnabled)
            {
                logger.Trace("Invalidating object {0}({1}). Reason: {2}", className, primaryKeyValue, reason);
            }

            _rwlock.AcquireWriterLock(-1);
            try
            {
                SoodaCacheKey cacheKey = new SoodaCacheKey(className, primaryKeyValue);
                _objectCache.Remove(cacheKey);

                LruCache cachedCollectionsDependentOnClass = (LruCache)_collectionsDependentOnClass[className];
                if (cachedCollectionsDependentOnClass != null)
                {
                    foreach (string key in cachedCollectionsDependentOnClass.Keys)
                    {
                        _collectionCache.Remove(key);
                    }
                    cachedCollectionsDependentOnClass.Clear();
                }
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        public void Clear()
        {
            _rwlock.AcquireWriterLock(-1);
            try
            {
                _objectCache.Clear();
                _collectionCache.Clear();
                _collectionsDependentOnClass.Clear();
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }


        public void Evict(string className, object primaryKeyValue)
        {
            _objectCache.Remove(new SoodaCacheKey(className, primaryKeyValue));
        }

        public void EvictCollection(string cacheKey)
        {
            _collectionCache.Remove(cacheKey);
        }

        public IList LoadCollection(string key)
        {
            if (key == null)
                return null;

            SoodaCachedCollection cc = (SoodaCachedCollection)_collectionCache[key];
            if (cc == null)
                return null;

            IList retval = cc.PrimaryKeys;
            logger.Trace("Collection: {0} retrieved from cache: {1} items", key, retval.Count);
            return retval;
        }

        private void RegisterDependentCollectionClass(string cacheKey, string dependentClassName)
        {
            LruCache cache = (LruCache)_collectionsDependentOnClass[dependentClassName];
            if (cache == null)
            {
                cache = new LruCache(-1, ExpirationTimeout, false);
                _collectionsDependentOnClass[dependentClassName] = cache;
            }

            // this is actually a set
            cache[cacheKey] = _marker;
        }

        public void StoreCollection(string cacheKey, string rootClassName, IList primaryKeys, string[] dependentClassNames, bool evictWhenItemRemoved)
        {
            if (cacheKey != null)
            {
                _rwlock.AcquireWriterLock(-1);
                try
                {
                    if (logger.IsTraceEnabled)
                    {
                        logger.Trace("Storing collection: {0} {1} items. Dependent on: [ {2} ]", cacheKey, primaryKeys.Count, String.Join(",",dependentClassNames));
                    }

                    SoodaCachedCollection cc = new SoodaCachedCollection(cacheKey, rootClassName, primaryKeys);

                    if (evictWhenItemRemoved)
                    {
                        foreach (object o in primaryKeys)
                        {
                            SoodaCacheKey k = new SoodaCacheKey(rootClassName, o);
                            SoodaCacheEntry e = (SoodaCacheEntry)_objectCache[k];
                            if (e != null)
                            {
                                //logger.Trace("Registering {0} as dependent of {1}", cacheKey, e);
                                e.RegisterDependentCollection(cc);
                            };
                        }
                    }
                    _collectionCache[cacheKey] = cc;

                    RegisterDependentCollectionClass(cacheKey, rootClassName);
                    if (dependentClassNames != null)
                    {
                        for (int i = 0; i < dependentClassNames.Length; ++i)
                        {
                            RegisterDependentCollectionClass(cacheKey, dependentClassNames[i]);
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
            output.WriteLine("Item cache");
            _objectCache.Dump(output);
            output.WriteLine("Collections:");
            _collectionCache.Dump(output);
        }

        public ISoodaCacheLock Lock()
        {
            _rwlock.AcquireWriterLock(-1);
            return new EndCommitCaller(this);
        }

        public void Unlock(ISoodaCacheLock theLock)
        {
            ((EndCommitCaller)theLock).Dispose();
        }

        internal void DoUnlock()
        {
            _rwlock.ReleaseWriterLock();
        }

        public void Sweep()
        {
            _rwlock.AcquireWriterLock(-1);
            _objectCache.Sweep();
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
