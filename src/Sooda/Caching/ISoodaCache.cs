using System;
using System.Collections;
using System.Text;

using Sooda.Schema;

namespace Sooda.Caching
{
    public interface ISoodaCache
    {
        void Clear();
        void Invalidate(string className, object primaryKeyValue, SoodaCacheInvalidateReason reason);
        void Add(string className, object primaryKeyValue, SoodaCacheEntry entry);
        SoodaCacheEntry Find(string className, object primaryKeyValue);
        IList LoadCollection(SoodaCachedCollectionKey key);
        void StoreCollection(SoodaCachedCollectionKey cacheKey, IList primaryKeys, ClassInfoCollection dependentClasses);

        ISoodaCacheLock Lock();
        void Unlock(ISoodaCacheLock theLock);
    }
}
