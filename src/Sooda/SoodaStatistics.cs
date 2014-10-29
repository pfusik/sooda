//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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
using System.Text;

namespace Sooda
{
    public class SoodaStatistics
    {
        private static readonly SoodaStatistics _globalStatistics = new SoodaStatistics();

        internal SoodaStatistics() { }

        // query timings

        private int _databaseQueries = 0;
        private double _totalQueryTime = 0.0;
        private double _minQueryTime = 0.0;
        private double _maxQueryTime = 0.0;
        private double _avgQueryTime = 0.0;
        private double _lastQueryTime = 0.0;
        private int _cacheMisses = 0;
        private int _cacheHits = 0;
        private int _collectionCacheMisses = 0;
        private int _collectionCacheHits = 0;
        private int _extraMaterializations = 0;
        private int _objectInserts = 0;
        private int _objectUpdates = 0;
        private int _objectsWithoutFields = 0;

        public double TotalQueryTime
        {
            get { return _totalQueryTime; }
        }

        public double LastQueryTime
        {
            get { return _lastQueryTime; }
        }

        public double AvgQueryTime
        {
            get { return _avgQueryTime; }
        }

        public double MinQueryTime
        {
            get { return _minQueryTime; }
        }

        public double MaxQueryTime
        {
            get { return _maxQueryTime; }
        }

        public int DatabaseQueries
        {
            get { return _databaseQueries; }
        }

        public int CacheHits
        {
            get { return _cacheHits; }
        }

        public int CacheMisses
        {
            get { return _cacheMisses; }
        }

        public int CollectionCacheHits
        {
            get { return _collectionCacheHits; }
        }

        public int CollectionCacheMisses
        {
            get { return _collectionCacheMisses; }
        }

        public double CacheHitRatio
        {
            get
            {
                int total = _cacheHits + _cacheMisses;
                if (total == 0)
                    return 0.0;

                return (double)_cacheHits / total;
            }
        }

        public double CollectionCacheHitRatio
        {
            get
            {
                int total = _collectionCacheHits + _collectionCacheMisses;
                if (total == 0)
                    return 0.0;

                return (double)_collectionCacheHits / total;
            }
        }

        public int ExtraMaterializations
        {
            get { return _extraMaterializations; }
        }

        public int ObjectInserts
        {
            get { return _objectInserts; }
        }

        public int ObjectUpdates
        {
            get { return _objectUpdates; }
        }

        public int ObjectsWithoutFields
        {
            get { return _objectsWithoutFields; }
        }

        public double ObjectsWithoutFieldsRatio
        {
            get
            {
                int totalObjects = ObjectInserts + ObjectUpdates;
                if (totalObjects == 0)
                    return 0.0;
                else
                    return (double)_objectsWithoutFields / totalObjects;
            }
        }

        public static SoodaStatistics Global
        {
            get { return _globalStatistics; }
        }

        internal void RegisterQueryTime(double timeInSeconds)
        {
            _databaseQueries++;
            _totalQueryTime += timeInSeconds;
            _lastQueryTime = timeInSeconds;
            _avgQueryTime = _totalQueryTime / _databaseQueries;

            if (_databaseQueries == 1)
            {
                _minQueryTime = timeInSeconds;
                _maxQueryTime = timeInSeconds;
            }
            else
            {
                _maxQueryTime = Math.Max(_maxQueryTime, timeInSeconds);
                _minQueryTime = Math.Min(_minQueryTime, timeInSeconds);
            }
        }

        internal void RegisterCacheHit()
        {
            _cacheHits++;
        }

        internal void RegisterCacheMiss()
        {
            _cacheMisses++;
        }

        internal void RegisterCollectionCacheHit()
        {
            _collectionCacheHits++;
        }

        internal void RegisterCollectionCacheMiss()
        {
            _collectionCacheMisses++;
        }

        internal void RegisterExtraMaterialization()
        {
            _extraMaterializations++;
        }

        internal void RegisterObjectUpdate()
        {
            _objectUpdates++;
            _objectsWithoutFields++;
        }

        internal void RegisterObjectInsert()
        {
            _objectInserts++;
            _objectsWithoutFields++;
        }

        internal void RegisterFieldsInited()
        {
            _objectsWithoutFields--;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Queries:          count={0} total={1}ms avg={2}ms min={3}ms max={4}ms last={5}ms\n",
                DatabaseQueries,
                Math.Round(TotalQueryTime * 1000.0, 3),
                Math.Round(AvgQueryTime * 1000.0, 3),
                Math.Round(MinQueryTime * 1000.0, 3),
                Math.Round(MaxQueryTime * 1000.0, 3),
                Math.Round(LastQueryTime * 1000.0, 3));
            sb.AppendFormat("Object Cache:     hits={0} misses={1} ratio={2}%\n", CacheHits, CacheMisses, Math.Round(CacheHitRatio * 100.0, 2));
            sb.AppendFormat("Collection Cache: hits={0} misses={1} ratio={2}%\n", CollectionCacheHits, CollectionCacheMisses, Math.Round(CollectionCacheHitRatio * 100.0, 2));
            sb.AppendFormat("Objects:          inserted={0} updated={1} extra={2} nofields={3} ({4}%)\n",
                ObjectInserts, ObjectUpdates,
                ExtraMaterializations, ObjectsWithoutFields, Math.Round(ObjectsWithoutFieldsRatio * 100.0, 2));

            return sb.ToString();
        }
    }
}
