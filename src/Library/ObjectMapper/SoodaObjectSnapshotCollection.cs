// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Text;
using System.Collections;
using System.Reflection;

using Sooda.Schema;

namespace Sooda.ObjectMapper
{
    public class SoodaObjectListSnapshot : ISoodaObjectList
    {
		public SoodaObjectListSnapshot(IList list)
		{
			foreach (SoodaObject o in list)
			{
				AddObjectToSnapshot(o);
			}
		}

		public SoodaObjectListSnapshot(IList list, SoodaObjectFilter filter)
        {
            foreach (SoodaObject o in list)
            {
                if (filter(o))
                    AddObjectToSnapshot(o);
            }
        }

        public SoodaObjectListSnapshot(IList list, int first, int length)
        {
            this.classInfo = null;
            items.Capacity = length;

            int start = first;
            if (start < 0)
            {
                length += start;
                start = 0;
            };
            if (start + length > list.Count)
                length = list.Count - start;

            for (int i = 0; i < length; ++i)
            {
                items.Add(list[start + i]);
            }
        }

        public SoodaObjectListSnapshot(IList list, IComparer comp)
        {
            items.Capacity = list.Count;
            for (int i = 0; i < list.Count; ++i)
            {
                items.Add(list[i]);
            }
            items.Sort(comp);
        }

        public SoodaObjectListSnapshot(SoodaTransaction tran, SoodaObjectFilter filter, ClassInfo ci)
        {
            this.classInfo = ci;
            SoodaObjectCollection al = tran.GetObjectsByClassName(ci.Name);

            if (al != null)
            {
                // al.Clone() is needed because
                // the filter expression may materialize new objects
                // during checking. This way we avoid "collection modified" exception

                foreach (SoodaObject obj in (SoodaObjectCollection)al.Clone())
                {
                    if (filter(obj))
                    {
                        items.Add(obj);
                    }
                }
            }
        }

        protected SoodaObjectListSnapshot()
        {
        }

        protected void AddObjectToSnapshot(SoodaObject o)
        {
            items.Add(o);
        }

        public SoodaObjectListSnapshot(SoodaTransaction t, SoodaWhereClause whereClause, SoodaOrderBy orderBy, SoodaSnapshotOptions options, ClassInfo ci)
        {
            this.classInfo = ci;

            if ((options & SoodaSnapshotOptions.NoWriteObjects) == 0)
            {
                t.SaveObjectChanges();
                t.SaveObjectChanges();
                t.SaveObjectChanges();
                t.SaveObjectChanges();
                t.SaveObjectChanges();
            }

            LoadList(t, whereClause, orderBy, options);
        }

        private void LoadList(SoodaTransaction t, SoodaWhereClause whereClause, SoodaOrderBy orderBy, SoodaSnapshotOptions options)
        {
            Hashtable itemsHash = new Hashtable();
            SoodaTransaction transaction = t;

            ISoodaObjectFactory factory = t.GetFactory(classInfo);

            if ((options & SoodaSnapshotOptions.NoDatabase) == 0)
            {
                SoodaDataSource ds = transaction.OpenDataSource(classInfo.GetDataSource());
                TableInfo[] loadedTables;

                using (IDataReader reader = ds.LoadObjectList(classInfo, whereClause, orderBy, out loadedTables))
                {
                    while (reader.Read())
                    {
                        SoodaObject obj = factory.GetRefFromRecord(transaction, reader, 0, loadedTables);

                        int pos = items.Add(obj);
                        itemsHash.Add(obj, pos);
                    };
                }
            }
        }

        public SoodaObject GetItem(int pos)
        {
            return (SoodaObject)items[pos];
        }

        public int Add(object obj)
        {
            return items.Add(obj);
        }

        public void Remove(object obj)
        {
            items.Remove(obj);
        }

        public bool Contains(object obj)
        {
            return items.Contains(obj);
        }

        public IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        private ArrayList items = new ArrayList();
        private ClassInfo classInfo;

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get { return GetItem(index); }
            set 
            {
                throw new NotSupportedException();
            }
        }

        public void RemoveAt(int index)
        {
            Remove(GetItem(index));
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            items.Clear();
        }

        public int IndexOf(object value)
        {
            return items.IndexOf(value);
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

		public ISoodaObjectList GetSnapshot()
		{
			return this;
		}

		public ISoodaObjectList SelectFirst(int n)
		{
			return new SoodaObjectListSnapshot(this, 0, n);
		}

		public ISoodaObjectList SelectLast(int n)
		{
			return new SoodaObjectListSnapshot(this, this.Count - n, n);
		}

		public ISoodaObjectList SelectRange(int from, int to)
		{
			return new SoodaObjectListSnapshot(this, from, to - from);
		}

		public ISoodaObjectList Filter(SoodaObjectFilter filter)
		{
			return new SoodaObjectListSnapshot(this, filter);
		}

		public ISoodaObjectList Sort(IComparer comparer)
		{
			return new SoodaObjectListSnapshot(this, comparer);
		}
	}
}
