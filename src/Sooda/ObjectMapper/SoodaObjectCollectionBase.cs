// 
// Copyright (c) 2014 Piotr Fusik <piotr@fusik.info>
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

using Sooda;
using Sooda.Collections;
using Sooda.QL;
using Sooda.Schema;

namespace Sooda.ObjectMapper
{
    public abstract class SoodaObjectCollectionBase : IList, ISoodaObjectList
    {
        protected SoodaTransaction transaction;
        protected ClassInfo classInfo;
        protected SoodaObjectCollection itemsArray = null;
        protected SoodaObjectToInt32Association items = null;

        protected SoodaObjectCollectionBase(SoodaTransaction transaction, ClassInfo classInfo)
        {
            this.transaction = transaction;
            this.classInfo = classInfo;
        }

        public SoodaObject GetItem(int pos)
        {
            if (itemsArray == null)
                LoadData();
            return itemsArray[pos];
        }

        public int Length
        {
            get
            {
                if (itemsArray == null)
                    LoadData();
                return itemsArray.Count;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            if (itemsArray == null)
                LoadData();
            return ((IEnumerable) itemsArray).GetEnumerator();
        }

        public abstract int Add(object obj);

        public abstract void Remove(object obj);

        public abstract bool Contains(object obj);

        protected abstract void LoadData();

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return GetItem(index);
            }
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
            throw new NotSupportedException();
        }

        public int IndexOf(object value)
        {
            if (items == null)
                LoadData();
            if (items.Contains((SoodaObject) value))
                return items[(SoodaObject) value];
            else
                return -1;
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

        public int PagedCount
        {
            get { return this.Length; }
        }

        public int Count
        {
            get
            {
                return this.Length;
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (itemsArray == null)
                LoadData();
            ((ICollection) itemsArray).CopyTo(array, index);
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
            return new SoodaObjectListSnapshot(this);
        }

        public ISoodaObjectList SelectFirst(int n)
        {
            return new SoodaObjectListSnapshot(this, 0, n);
        }

        public ISoodaObjectList SelectLast(int n)
        {
            return new SoodaObjectListSnapshot(this, this.Length - n, n);
        }

        public ISoodaObjectList SelectRange(int from, int to)
        {
            return new SoodaObjectListSnapshot(this, from, to - from);
        }

        public ISoodaObjectList Filter(SoodaObjectFilter filter)
        {
            return new SoodaObjectListSnapshot(this, filter);
        }

        public ISoodaObjectList Filter(SoqlBooleanExpression filterExpression)
        {
            return new SoodaObjectListSnapshot(this, filterExpression);
        }

        public ISoodaObjectList Filter(SoodaWhereClause whereClause)
        {
            return new SoodaObjectListSnapshot(this, whereClause);
        }

        public ISoodaObjectList Sort(IComparer comparer)
        {
            return new SoodaObjectListSnapshot(this, comparer);
        }

        public ISoodaObjectList Sort(string sortOrder)
        {
            return new SoodaObjectListSnapshot(this).Sort(sortOrder);
        }
        
        public ISoodaObjectList Sort(SoqlExpression expression, SortOrder sortOrder)
        {
            return new SoodaObjectListSnapshot(this).Sort(expression, sortOrder);
        }
        
        public ISoodaObjectList Sort(SoqlExpression expression)
        {
            return new SoodaObjectListSnapshot(this).Sort(expression, SortOrder.Ascending);
        }
    }
}
