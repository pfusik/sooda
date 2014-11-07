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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Sooda.ObjectMapper
{
    [DebuggerStepThrough]
    public class SoodaObjectCollectionWrapperGeneric<T> : ISoodaObjectList, ISoodaObjectListInternal, IList<T>
    {
        readonly ISoodaObjectList _theList;

        public SoodaObjectCollectionWrapperGeneric()
        {
            _theList = new SoodaObjectListSnapshot();
        }

        public SoodaObjectCollectionWrapperGeneric(ISoodaObjectList list)
        {
            _theList = list;
        }

        SoodaObject ISoodaObjectList.GetItem(int pos)
        {
            return _theList.GetItem(pos);
        }

        public bool IsReadOnly
        {
            get
            {
                return _theList.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return _theList[index];
            }
            set
            {
                _theList[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            _theList.RemoveAt(index);
        }

        void IList.Insert(int index, object value)
        {
            _theList.Insert(index, value);
        }

        void IList.Remove(object value)
        {
            _theList.Remove(value);
        }

        bool IList.Contains(object value)
        {
            return _theList.Contains(value);
        }

        public void Clear()
        {
            _theList.Clear();
        }

        int IList.IndexOf(object value)
        {
            return _theList.IndexOf(value);
        }

        int IList.Add(object value)
        {
            return _theList.Add(value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return _theList.IsFixedSize;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return _theList.IsSynchronized;
            }
        }

        public int Count
        {
            get
            {
                return _theList.Count;
            }
        }

        public int PagedCount
        {
            get
            {
                return _theList.PagedCount;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _theList.CopyTo(array, index);
        }

        object ICollection.SyncRoot
        {
            get
            {
                return _theList.SyncRoot;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _theList.GetEnumerator();
        }

        public ISoodaObjectList GetSnapshot2()
        {
            return _theList.GetSnapshot();
        }

        ISoodaObjectList ISoodaObjectList.GetSnapshot()
        {
            return _theList.GetSnapshot();
        }

        ISoodaObjectList ISoodaObjectList.Filter(SoodaWhereClause whereClause)
        {
            return _theList.Filter(whereClause);
        }

        ISoodaObjectList ISoodaObjectList.Filter(Sooda.QL.SoqlBooleanExpression filterExpression)
        {
            return _theList.Filter(filterExpression);
        }

        ISoodaObjectList ISoodaObjectList.Filter(SoodaObjectFilter filter)
        {
            return _theList.Filter(filter);
        }

        protected ISoodaObjectList Filter2(SoodaWhereClause whereClause)
        {
            return _theList.Filter(whereClause);
        }

        protected ISoodaObjectList Filter2(Sooda.QL.SoqlBooleanExpression filterExpression)
        {
            return _theList.Filter(filterExpression);
        }

        protected ISoodaObjectList Filter2(SoodaObjectFilter filter)
        {
            return _theList.Filter(filter);
        }

        ISoodaObjectList ISoodaObjectList.Sort(IComparer comparer)
        {
            return _theList.Sort(comparer);
        }

        ISoodaObjectList ISoodaObjectList.Sort(string sortOrder)
        {
            return _theList.Sort(SoodaOrderBy.Parse(sortOrder).GetComparer());
        }

        ISoodaObjectList ISoodaObjectList.Sort(Sooda.QL.SoqlExpression sortExpression)
        {
            return _theList.Sort(SoodaOrderBy.FromExpression(sortExpression, SortOrder.Ascending).GetComparer());
        }

        ISoodaObjectList ISoodaObjectList.Sort(Sooda.QL.SoqlExpression sortExpression, SortOrder sortOrder)
        {
            return _theList.Sort(SoodaOrderBy.FromExpression(sortExpression, sortOrder).GetComparer());
        }

        protected ISoodaObjectList Sort2(IComparer comparer)
        {
            return _theList.Sort(comparer);
        }

        protected ISoodaObjectList Sort2(string sortOrder)
        {
            return _theList.Sort(SoodaOrderBy.Parse(sortOrder).GetComparer());
        }

        protected ISoodaObjectList Sort2(Sooda.QL.SoqlExpression sortExpression)
        {
            return _theList.Sort(SoodaOrderBy.FromExpression(sortExpression, SortOrder.Ascending).GetComparer());
        }

        protected ISoodaObjectList Sort2(Sooda.QL.SoqlExpression sortExpression, SortOrder sortOrder)
        {
            return _theList.Sort(SoodaOrderBy.FromExpression(sortExpression, sortOrder).GetComparer());
        }

        public ISoodaObjectList SelectFirst2(int count)
        {
            return _theList.SelectFirst(count);
        }

        public ISoodaObjectList SelectLast2(int count)
        {
            return _theList.SelectLast(count);
        }

        public ISoodaObjectList SelectRange2(int from, int to)
        {
            return _theList.SelectRange(from, to);
        }

        ISoodaObjectList ISoodaObjectList.SelectFirst(int count)
        {
            return _theList.SelectFirst(count);
        }

        ISoodaObjectList ISoodaObjectList.SelectLast(int count)
        {
            return _theList.SelectLast(count);
        }

        ISoodaObjectList ISoodaObjectList.SelectRange(int from, int to)
        {
            return _theList.SelectRange(from, to);
        }

        public void InternalAdd(SoodaObject o)
        {
            ((ISoodaObjectListInternal)_theList).InternalAdd(o);
            // TODO:  Add SoodaObjectCollectionWrapper.InternalAdd implementation
        }

        public void InternalRemove(SoodaObject o)
        {
            ((ISoodaObjectListInternal)_theList).InternalRemove(o);
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return _theList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _theList.Insert(index, item);
        }

        public T this[int index]
        {
            get { return (T) _theList[index]; }
            set { _theList[index] = value; }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            _theList.Add(item);
        }

        public bool Contains(T item)
        {
            return _theList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _theList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            _theList.Remove(item);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        #endregion
    }
}
