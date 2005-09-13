// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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

namespace Sooda.ObjectMapper {
    [DebuggerStepThrough]
    public class SoodaObjectCollectionWrapper : ISoodaObjectList, ISoodaObjectListInternal {
        private ISoodaObjectList _theList;

        protected SoodaObjectCollectionWrapper(ISoodaObjectList list) {
            _theList = list;
        }


        public SoodaObject GetItem(int pos) {
            return _theList.GetItem(pos);
        }

        public bool IsReadOnly { get {
                                     return _theList.IsReadOnly;
                                 } }
        public object this[int index]
        {
            get {
                return _theList[index];
            }
            set {
                _theList[index] = value;
            }
        }

        public void RemoveAt(int index) {
            _theList.RemoveAt(index);
        }

        public void Insert(int index, object value) {
            _theList.Insert(index, value);
        }

        public void Remove(object value) {
            _theList.Remove(value);
        }

        public bool Contains(object value) {
            return _theList.Contains(value);
        }

        public void Clear() {
            _theList.Clear();
        }

        public int IndexOf(object value) {
            return _theList.IndexOf(value);
        }

        public int Add(object value) {
            return _theList.Add(value);
        }

        public bool IsFixedSize
        {
            get {
                return _theList.IsFixedSize;
            }
        }

        public bool IsSynchronized
        {
            get {
                return _theList.IsSynchronized;
            }
        }

        public int Count
        {
            get {
                return _theList.Count;
            }
        }

        public void CopyTo(Array array, int index) {
            _theList.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get {
                return _theList.SyncRoot;
            }
        }

        public IEnumerator GetEnumerator() {
            return _theList.GetEnumerator();
        }

        public ISoodaObjectList GetSnapshot() {
            return _theList.GetSnapshot();
        }

        public ISoodaObjectList Filter(SoodaWhereClause whereClause) {
            return _theList.Filter(whereClause);
        }

        public ISoodaObjectList Filter(SoodaObjectFilter filter) {
            return _theList.Filter(filter);
        }

        public ISoodaObjectList Sort(IComparer comparer) {
            return _theList.Sort(comparer);
        }

        public ISoodaObjectList SelectFirst(int count) {
            return _theList.SelectFirst(count);
        }

        public ISoodaObjectList SelectLast(int count) {
            return _theList.SelectLast(count);
        }

        public ISoodaObjectList SelectRange(int from, int to) {
            return _theList.SelectRange(from, to);
        }

        public void InternalAdd(SoodaObject o) {
            ((ISoodaObjectListInternal)_theList).InternalAdd(o);
            // TODO:  Add SoodaObjectCollectionWrapper.InternalAdd implementation
        }

        public void InternalRemove(SoodaObject o) {
            ((ISoodaObjectListInternal)_theList).InternalRemove(o);
        }
    }
}
