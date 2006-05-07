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
        void Evict(string className, object primaryKeyValue);
        void Add(string className, object primaryKeyValue, SoodaCacheEntry entry);
        SoodaCacheEntry Find(string className, object primaryKeyValue);
        IList LoadCollection(string cacheKey);
        void StoreCollection(string cacheKey, string rootClassName, IList primaryKeys, string[] dependentClasses, bool evictWhenItemRemoved);
        void EvictCollection(string cacheKey);
        void Sweep();

        ISoodaCacheLock Lock();
        void Unlock(ISoodaCacheLock theLock);
    }
}
