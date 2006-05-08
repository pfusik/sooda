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
using System.IO;
using System.Collections;
using System.Threading;

using Sooda.Logging;
using Sooda.Schema;
using Sooda.QL;

namespace Sooda.Caching
{
    public class SoodaNoCache : ISoodaCache
    {
        SoodaCacheEntry ISoodaCache.Find(string className, object primaryKeyValue)
        {
            return null;
        }

        void ISoodaCache.Add(string className, object primaryKeyValue, SoodaCacheEntry entry)
        {
        }

        void ISoodaCache.Invalidate(string className, object primaryKeyValue, SoodaCacheInvalidateReason reason)
        {
        }

        void ISoodaCache.Clear()
        {
        }

        void ISoodaCache.Sweep()
        {
        }

        IList ISoodaCache.LoadCollection(string key)
        {
            return null;
        }

        void ISoodaCache.StoreCollection(string cacheKey, string rootClassName, IList primaryKeys, string[] dependentClasses, bool evictWhenItemRemoved)
        {
        }

        ISoodaCacheLock ISoodaCache.Lock()
        {
            return new DummySoodaCacheLock();
        }

        void ISoodaCache.Unlock(ISoodaCacheLock theLock)
        {
        }

        class DummySoodaCacheLock : ISoodaCacheLock
        {
            public void Dispose()
            {
            }
        }

        public void Evict(string className, object primaryKeyValue)
        {
        }

        public void EvictCollection(string cacheKey)
        {
        }
    }
}
