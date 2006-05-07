using System;
using System.Collections;
using System.Text;
using System.IO;

using Sooda.Schema;

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
            public DateTime ExpirationTime;
            public int UsedCount;
            public object Key;
            public object Value;
        }

        private Hashtable _hash = new Hashtable();
        private LruCacheNode _lruHead = null;
        private LruCacheNode _lruTail = null;
        private TimeSpan _timeToLive;
        private bool _slidingExpiration;
        private int _maxItems;
        private DateTime _nextSweepTime = DateTime.MinValue;
        private TimeSpan _sweepEvery;

        public event LruCacheDelegate ItemAdded;
        public event LruCacheDelegate ItemRemoved;
        public event LruCacheDelegate ItemUsed;

        public LruCache(int maxItems, TimeSpan timeToLive, bool slidingExpiration)
        {
            _maxItems = maxItems;
            _timeToLive = timeToLive;
            _slidingExpiration = slidingExpiration;
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

        public bool SlidingExpiration
        {
            get { return _slidingExpiration; }
            set { _slidingExpiration = value; }
        }

        public TimeSpan TimeToLive
        {
            get { return _timeToLive; }
            set
            {
                Clear();
                _timeToLive = value;
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
            set { Set(key, value); }
        }

        public object Get(object key)
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

            //Dump("After Sweep()");
        }

        public void Clear()
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
            if (_slidingExpiration)
                node.ExpirationTime = DateTime.Now + _timeToLive;
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

        public void Set(object key, object value)
        {
            lock (this)
            {
                //Dump("Before Set()");
                CheckForPeriodicSweep(DateTime.Now);
                LruCacheNode node = (LruCacheNode)_hash[key];
                if (node == null)
                {
                    Add(key, value);
                }
                else
                {
                    MoveToLruHead(node);
                    node.Value = value;
                }
                //Dump("After Set()");
            }
        }

        public void Add(object key, object value)
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

                node.Previous = null;
                node.Next = _lruHead;
                node.Key = key;
                node.Value = value;
                node.ExpirationTime = now + _timeToLive;

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

        private void Dump(string title)
        {
            //Console.Write("{0}: ", title);
            //Dump(Console.Out);
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
    }
}
