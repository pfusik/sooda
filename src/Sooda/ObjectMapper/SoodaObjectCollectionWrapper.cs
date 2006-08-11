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
using System.Diagnostics;
using System.Collections;
#if DOTNET2
using System.Collections.Generic;
#endif

namespace Sooda.ObjectMapper
{
    [DebuggerStepThrough]
    public class SoodaObjectCollectionWrapper : ISoodaObjectList, ISoodaObjectListInternal
    {
        private ISoodaObjectList _theList;

        protected SoodaObjectCollectionWrapper()
        {
            _theList = new SoodaObjectListSnapshot();
        }

        protected SoodaObjectCollectionWrapper(ISoodaObjectList list)
        {
            _theList = list;
        }


        public SoodaObject GetItem(int pos)
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
        public object this[int index]
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

        public void Insert(int index, object value)
        {
            _theList.Insert(index, value);
        }

        void IList.Remove(object value)
        {
            _theList.Remove(value);
        }

        protected void Remove2(object value)
        {
            _theList.Remove(value);
        }

        bool IList.Contains(object value)
        {
            return _theList.Contains(value);
        }

        protected bool Contains2(object value)
        {
            return _theList.Contains(value);
        }

        public void Clear()
        {
            _theList.Clear();
        }

        public int IndexOf(object value)
        {
            return _theList.IndexOf(value);
        }

        int IList.Add(object value)
        {
            return _theList.Add(value);
        }

        protected int Add2(object value)
        {
            return _theList.Add(value);
        }

        public bool IsFixedSize
        {
            get
            {
                return _theList.IsFixedSize;
            }
        }

        public bool IsSynchronized
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

        public void CopyTo(Array array, int index)
        {
            _theList.CopyTo(array, index);
        }

        public object SyncRoot
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
    }

#if DOTNET2
    public class SoodaObjectCollectionWrapperGeneric<T> : SoodaObjectCollectionWrapper, IList<T>
    {
        public SoodaObjectCollectionWrapperGeneric() : base()
        {
        }

        public SoodaObjectCollectionWrapperGeneric(ISoodaObjectList list)
            : base(list)
        {
        }

        #region IList<T> Members

        int IList<T>.IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            base.Insert(index, item);
        }

        void IList<T>.RemoveAt(int index)
        {
            base.RemoveAt(index);
        }

        T IList<T>.this[int index]
        {
            get { return (T)base[index]; }
            set { base[index] = value; }
        }

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add(T item)
        {
            base.Add2(item);
        }

        void ICollection<T>.Clear()
        {
            base.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return base.Contains2(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return base.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return base.IsReadOnly; }
        }

        bool ICollection<T>.Remove(T item)
        {
            base.Remove2(item);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return (T)this[i];
            };
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion
    }
#endif
}
