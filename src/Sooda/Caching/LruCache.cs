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
using System.Collections;
using System.Data;
using System.IO;

namespace Sooda.Caching
{
    public class LruCacheEventArgs : EventArgs
    {
        public readonly object Key;
        public readonly object Value;

        public LruCacheEventArgs(object key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    public delegate void LruCacheDelegate(object sender, LruCacheEventArgs args);

    public class LruCache
    {
        class LruCacheNode
        {
            public LruCacheNode Previous;
            public LruCacheNode Next;
            public DateTime AddedTime;
            public DateTime LastAccessTime;
            public DateTime ExpirationTime;
            public bool SlidingExpiration;
            public TimeSpan ExpirationTimeout;
            public int UsedCount;
            public object Key;
            public object Value;
        }

        private Hashtable _hash = new Hashtable();
        private LruCacheNode _lruHead = null;
        private LruCacheNode _lruTail = null;
        private int _maxItems;
        private DateTime _nextSweepTime = DateTime.MinValue;
        private TimeSpan _sweepEvery;

        public event LruCacheDelegate ItemAdded;
        public event LruCacheDelegate ItemRemoved;
        public event LruCacheDelegate ItemUsed;

        public LruCache(int maxItems)
        {
            _maxItems = maxItems;
            SweepEvery = TimeSpan.Zero;
        }

        public int MaxItems
        {
            get { return _maxItems; }
            set
            {
                _maxItems = value;
                Clear();
            }
        }

        public ICollection Keys
        {
            get { return _hash.Keys; }
        }

        public TimeSpan SweepEvery
        {
            get { return _sweepEvery; }
            set
            {
                _sweepEvery = value;
                if (value == TimeSpan.Zero)
                    _nextSweepTime = DateTime.MaxValue;
                else
                    _nextSweepTime = DateTime.Now.Add(value);
            }
        }

        public object this[object key]
        {
            get { return Get(key); }
        }

        public object Get(object key)
        {
            lock (this)
            {
                DateTime now = DateTime.Now;

                CheckForPeriodicSweep(now);

                LruCacheNode node = (LruCacheNode)_hash[key];
                if (node == null)
                    return null;
                if (now >= node.ExpirationTime)
                {
                    TrimBeforeNode(node);
                    return null;
                }
                MoveToLruHead(node);
                return node.Value;
            }
        }

        private void CheckForPeriodicSweep(DateTime now)
        {
            if (now >= _nextSweepTime)
            {
                Sweep();
                _nextSweepTime = now + _sweepEvery;
            }
        }

        public void Sweep()
        {
            lock (this)
            {
                DateTime now = DateTime.Now;

                //Dump("Before Sweep()");

                for (LruCacheNode node = _lruHead; node != null; node = node.Next)
                {
                    if (now >= node.ExpirationTime)
                    {
                        TrimBeforeNode(node);
                        return;
                    }
                }
            }

            //Dump("After Sweep()");
        }

        public void Clear()
        {
            lock (this)
            {
                //Dump("Before Clear()");
                _hash.Clear();
                if (ItemRemoved != null)
                {
                    for (LruCacheNode n = _lruHead; n != null; n = n.Next)
                    {
                        ItemRemoved(this, new LruCacheEventArgs(n.Key, n.Value));
                    }
                }
                _lruHead = _lruTail = null;
                //Dump("After Clear()");
            }
        }

        private void TrimBeforeNode(LruCacheNode node)
        {
            if (node.Previous == null)
            {
                Clear();
                return;
            }

            lock (this)
            {
                _lruTail = node.Previous;
                _lruTail.Next = null;

                for (LruCacheNode n = node; n != null; n = n.Next)
                {
                    _hash.Remove(n.Key);
                    if (ItemRemoved != null)
                    {
                        ItemRemoved(this, new LruCacheEventArgs(n.Key, n.Value));
                    }
                }
            }
            //Dump("After TrimBeforeNode()");
        }

        private void MoveToLruHead(LruCacheNode node)
        {
            //Dump("Before MoveToLruHead(" + node.Key + ")");
            if (ItemUsed != null)
            {
                ItemUsed(this, new LruCacheEventArgs(node.Key, node.Value));
            }

            if (node != _lruHead)
            {
                node.Previous.Next = node.Next;
                if (node.Next != null)
                {
                    node.Next.Previous = node.Previous;
                }
                else
                {
                    _lruTail = node.Previous;
                }

                if (_lruHead != null)
                {
                    _lruHead.Previous = node;
                }
                node.Next = _lruHead;
                node.Previous = null;
                _lruHead = node;
            }
            DateTime now = DateTime.Now;
            node.LastAccessTime = now;
            if (node.SlidingExpiration)
                node.ExpirationTime = now + node.ExpirationTimeout;
            node.UsedCount++;
            //Dump("After MoveToLruHead()");
        }

        public void Remove(object key)
        {
            lock (this)
            {
                LruCacheNode node = (LruCacheNode)_hash[key];
                if (node != null)
                    RemoveNode(node);
            }
        }

        private void RemoveNode(LruCacheNode node)
        {
            if (node.Previous != null)
                node.Previous.Next = node.Next;

            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }
            else
            {
                _lruTail = node.Previous;
            }

            if (node == _lruHead)
                _lruHead = node.Next;

            node.Previous = null;
            node.Next = null;
            _hash.Remove(node.Key);
            if (ItemRemoved != null)
            {
                ItemRemoved(this, new LruCacheEventArgs(node.Key, node.Value));
            }
        }

        public void Set(object key, object value, TimeSpan expirationTimeout, bool slidingExpiration)
        {
            lock (this)
            {
                //Dump("Before Set()");
                CheckForPeriodicSweep(DateTime.Now);
                LruCacheNode node = (LruCacheNode)_hash[key];
                if (node == null)
                {
                    Add(key, value, expirationTimeout, slidingExpiration);
                }
                else
                {
                    MoveToLruHead(node);
                    node.Value = value;
                }
                //Dump("After Set()");
            }
        }

        public void Add(object key, object value, TimeSpan expirationTimeout, bool slidingExpiration)
        {
            lock (this)
            {
                DateTime now = DateTime.Now;

                CheckForPeriodicSweep(now);
                if (_maxItems != -1 && _hash.Count >= _maxItems)
                {
                    RemoveLruItem();
                    //Dump("After RemoveLruItem");
                }

                LruCacheNode node = new LruCacheNode();

                node.AddedTime = now;
                node.SlidingExpiration = slidingExpiration;
                node.ExpirationTimeout = expirationTimeout;
                node.LastAccessTime = now;
                node.Previous = null;
                node.Next = _lruHead;
                node.Key = key;
                node.Value = value;
                node.ExpirationTime = now + expirationTimeout;

                if (_lruHead != null)
                {
                    _lruHead.Previous = node;
                }
                _lruHead = node;
                if (_lruTail == null)
                    _lruTail = node;
                _hash.Add(key, node);
                if (ItemAdded != null)
                {
                    ItemAdded(this, new LruCacheEventArgs(node.Key, node.Value));
                }
                //Dump("After Add()");
            }
        }

        private void RemoveLruItem()
        {
            if (_lruTail != null)
            {
                RemoveNode(_lruTail);
            }
        }

        public int Count
        {
            get { return _hash.Count; }
        }

        internal void Dump(TextWriter output)
        {
            int cnt = 0;

            for (LruCacheNode node = _lruHead; node != null; node = node.Next)
            {
                Console.WriteLine("{0} = {1} (used:{2} expire:{3})", node.Key, node.Value, node.UsedCount, node.ExpirationTime);
                if (node.Previous != null && node.Previous.Next != node)
                    throw new Exception("LRU assertion failed #1");
                if (node.Previous == null && node != _lruHead)
                    throw new Exception("LRU assertion failed #2");
                if (node.Next != null && node.Next.Previous != node)
                    throw new Exception("LRU assertion failed #3");
                if (node.Next == null && node != _lruTail)
                    throw new Exception("LRU assertion failed #4");
                cnt++;
            }

            if (cnt != _hash.Count)
            {
                throw new Exception("LRU assertion failed #5, was " + cnt + " should be " + _hash.Count);
            }
            Console.WriteLine();
        }

        public void FillSnapshotTable(DataSet dataSet, string tableName)
        {
            lock (this)
            {
                DataTable dt = dataSet.Tables.Add(tableName);
                dt.Columns.Add("LruPosition",typeof(int));
                dt.Columns.Add("Class",typeof(string));
                dt.Columns.Add("Key",typeof(string));
                dt.Columns.Add("Value",typeof(string));
                dt.Columns.Add("AddedTime",typeof(DateTime));
                dt.Columns.Add("LastAccessTime",typeof(DateTime));
                dt.Columns.Add("ExpirationTime",typeof(DateTime));
                dt.Columns.Add("UsedCount",typeof(int));

                int pos = 0;

                for (LruCacheNode node = _lruHead; node != null; node = node.Next)
                {
                    DataRow row = dt.NewRow();

                    string className;
                    string keyValue;

                    SoodaCacheKey ck1 = node.Key as SoodaCacheKey;
                    SoodaCachedCollection scc = node.Value as SoodaCachedCollection;

                    if (ck1 != null)
                    {
                        className = ck1.ClassName;
                        keyValue = Convert.ToString(ck1.KeyValue);
                    }
                    else if (scc != null)
                    {
                        className = scc.RootClassName;
                        keyValue = scc.CollectionKey;
                    }
                    else
                    {
                        className = Convert.ToString(node.Key);
                        keyValue = null;
                    }

                    row.ItemArray = new object[]
                    {
                        pos++,
                        className,
                        keyValue,
                        Convert.ToString(node.Value),
                        node.AddedTime,
                        node.LastAccessTime,
                        node.ExpirationTime,
                        node.UsedCount
                    };
                    dt.Rows.Add(row);
                }
            }
        }
    }
}
