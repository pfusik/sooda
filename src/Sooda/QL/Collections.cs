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
using System.Collections;

namespace Sooda.QL {


    /// <summary>
    ///  A strongly-typed collection of <see cref="SoqlExpression"/> objects.
    /// </summary>
    [Serializable]
    public
    class SoqlExpressionCollection : ICollection, IList, IEnumerable, ICloneable {
#region Interfaces
        /// <summary>
        ///  Supports type-safe iteration over a <see cref="SoqlExpressionCollection"/>.
        /// </summary>
        public interface ISoqlExpressionCollectionEnumerator {
            /// <summary>
            ///  Gets the current element in the collection.
            /// </summary>
            SoqlExpression Current {get
                                    ;
                                   }

            /// <summary>
            ///  Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            ///  The collection was modified after the enumerator was created.
            /// </exception>
            /// <returns>
            ///  <c>true</c> if the enumerator was successfully advanced to the next element;
            ///  <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            bool MoveNext();

            /// <summary>
            ///  Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            void Reset();
        }
#endregion

        private const int DEFAULT_CAPACITY = 16;

#region Implementation (data)

        private SoqlExpression[] m_array;
        private int m_count = 0;
        [NonSerialized]
        private int m_version = 0;
#endregion

#region Static Wrappers
        /// <summary>
        ///  Creates a synchronized (thread-safe) wrapper for a
        ///     <c>SoqlExpressionCollection</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>SoqlExpressionCollection</c> wrapper that is synchronized (thread-safe).
        /// </returns>
        public static SoqlExpressionCollection Synchronized(SoqlExpressionCollection list) {
            if (list == null)
                throw new ArgumentNullException("list");
            return new SyncSoqlExpressionCollection(list);
        }

        /// <summary>
        ///  Creates a read-only wrapper for a
        ///     <c>SoqlExpressionCollection</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>SoqlExpressionCollection</c> wrapper that is read-only.
        /// </returns>
        public static SoqlExpressionCollection ReadOnly(SoqlExpressionCollection list) {
            if (list == null)
                throw new ArgumentNullException("list");
            return new ReadOnlySoqlExpressionCollection(list);
        }
#endregion

#region Construction
        /// <summary>
        ///  Initializes a new instance of the <c>SoqlExpressionCollection</c> class
        ///  that is empty and has the default initial capacity.
        /// </summary>
        public SoqlExpressionCollection() {
            m_array = new SoqlExpression[DEFAULT_CAPACITY];
        }

        /// <summary>
        ///  Initializes a new instance of the <c>SoqlExpressionCollection</c> class
        ///  that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///  The number of elements that the new <c>SoqlExpressionCollection</c> is initially capable of storing.
        /// </param>
        public SoqlExpressionCollection(int capacity) {
            m_array = new SoqlExpression[capacity];
        }

        /// <summary>
        ///  Initializes a new instance of the <c>SoqlExpressionCollection</c> class
        ///  that contains elements copied from the specified <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="c">The <c>SoqlExpressionCollection</c> whose elements are copied to the new collection.</param>
        public SoqlExpressionCollection(SoqlExpressionCollection c) {
            m_array = new SoqlExpression[c.Count];
            AddRange(c);
        }

        /// <summary>
        ///  Initializes a new instance of the <c>SoqlExpressionCollection</c> class
        ///  that contains elements copied from the specified <see cref="SoqlExpression"/> array.
        /// </summary>
        /// <param name="a">The <see cref="SoqlExpression"/> array whose elements are copied to the new list.</param>
        public SoqlExpressionCollection(SoqlExpression[] a) {
            m_array = new SoqlExpression[a.Length];
            AddRange(a);
        }

        protected enum Tag {
            Default
        }

        protected SoqlExpressionCollection(Tag t) {
            m_array = null;
        }
#endregion

#region Operations (type-safe ICollection)
        /// <summary>
        ///  Gets the number of elements actually contained in the <c>SoqlExpressionCollection</c>.
        /// </summary>
        public virtual int Count
        {
            get {
                return m_count;
            }
        }

        /// <summary>
        ///  Copies the entire <c>SoqlExpressionCollection</c> to a one-dimensional
        ///  <see cref="SoqlExpression"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="SoqlExpression"/> array to copy to.</param>
        public virtual void CopyTo(SoqlExpression[] array) {
            this.CopyTo(array, 0);
        }

        /// <summary>
        ///  Copies the entire <c>SoqlExpressionCollection</c> to a one-dimensional
        ///  <see cref="SoqlExpression"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="SoqlExpression"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(SoqlExpression[] array, int start) {
            if (m_count > array.GetUpperBound(0) + 1 - start)
                throw new System.ArgumentException("Destination array was not long enough.");

            Array.Copy(m_array, 0, array, start, m_count);
        }

        /// <summary>
        ///  Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        /// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get {
                return m_array.IsSynchronized;
            }
        }

        /// <summary>
        ///  Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public virtual object SyncRoot
        {
            get {
                return m_array.SyncRoot;
            }
        }
#endregion

#region Operations (type-safe IList)
        /// <summary>
        ///  Gets or sets the <see cref="SoqlExpression"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///  <para><paramref name="index"/> is less than zero</para>
        ///  <para>-or-</para>
        ///  <para><paramref name="index"/> is equal to or greater than <see cref="SoqlExpressionCollection.Count"/>.</para>
        /// </exception>
        public virtual SoqlExpression this[int index]
        {
            get {
                ValidateIndex(index); // throws
                return m_array[index];
            }
            set {
                ValidateIndex(index); // throws
                ++m_version;
                m_array[index] = value;
            }
        }

        /// <summary>
        ///  Adds a <see cref="SoqlExpression"/> to the end of the <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="SoqlExpression"/> to be added to the end of the <c>SoqlExpressionCollection</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(SoqlExpression item) {
            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            m_array[m_count] = item;
            m_version++;

            return m_count++;
        }

        /// <summary>
        ///  Removes all elements from the <c>SoqlExpressionCollection</c>.
        /// </summary>
        public virtual void Clear() {
            ++m_version;
            m_array = new SoqlExpression[DEFAULT_CAPACITY];
            m_count = 0;
        }

        /// <summary>
        ///  Creates a shallow copy of the <see cref="SoqlExpressionCollection"/>.
        /// </summary>
        public virtual object Clone() {
            SoqlExpressionCollection newColl = new SoqlExpressionCollection(m_count);
            Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
            newColl.m_count = m_count;
            newColl.m_version = m_version;

            return newColl;
        }

        /// <summary>
        ///  Determines whether a given <see cref="SoqlExpression"/> is in the <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="SoqlExpression"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>SoqlExpressionCollection</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(SoqlExpression item) {
            for (int i = 0; i != m_count; ++i)
                if (m_array[i].Equals(item))
                    return true;
            return false;
        }

        /// <summary>
        ///  Returns the zero-based index of the first occurrence of a <see cref="SoqlExpression"/>
        ///  in the <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="SoqlExpression"/> to locate in the <c>SoqlExpressionCollection</c>.</param>
        /// <returns>
        ///  The zero-based index of the first occurrence of <paramref name="item"/>
        ///  in the entire <c>SoqlExpressionCollection</c>, if found; otherwise, -1.
        /// </returns>
        public virtual int IndexOf(SoqlExpression item) {
            for (int i = 0; i != m_count; ++i)
                if (m_array[i].Equals(item))
                    return i;
            return -1;
        }

        /// <summary>
        ///  Inserts an element into the <c>SoqlExpressionCollection</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="SoqlExpression"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///  <para><paramref name="index"/> is less than zero</para>
        ///  <para>-or-</para>
        ///  <para><paramref name="index"/> is equal to or greater than <see cref="SoqlExpressionCollection.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, SoqlExpression item) {
            ValidateIndex(index, true); // throws

            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            if (index < m_count) {
                Array.Copy(m_array, index, m_array, index + 1, m_count - index);
            }

            m_array[index] = item;
            m_count++;
            m_version++;
        }

        /// <summary>
        ///  Removes the first occurrence of a specific <see cref="SoqlExpression"/> from the <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="SoqlExpression"/> to remove from the <c>SoqlExpressionCollection</c>.</param>
        /// <exception cref="ArgumentException">
        ///  The specified <see cref="SoqlExpression"/> was not found in the <c>SoqlExpressionCollection</c>.
        /// </exception>
        public virtual void Remove(SoqlExpression item) {
            int i = IndexOf(item);
            if (i < 0)
                throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");

            ++m_version;
            RemoveAt(i);
        }

        /// <summary>
        ///  Removes the element at the specified index of the <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///  <para><paramref name="index"/> is less than zero</para>
        ///  <para>-or-</para>
        ///  <para><paramref name="index"/> is equal to or greater than <see cref="SoqlExpressionCollection.Count"/>.</para>
        /// </exception>
        public virtual void RemoveAt(int index) {
            ValidateIndex(index); // throws

            m_count--;

            if (index < m_count) {
                Array.Copy(m_array, index + 1, m_array, index, m_count - index);
            }

            // We can't set the deleted entry equal to null, because it might be a value type.
            // Instead, we'll create an empty single-element array of the right type and copy it
            // over the entry we want to erase.
            SoqlExpression[] temp = new SoqlExpression[1];
            Array.Copy(temp, 0, m_array, m_count, 1);
            m_version++;
        }

        /// <summary>
        ///  Gets a value indicating whether the collection has a fixed size.
        /// </summary>
        /// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get {
                return false;
            }
        }

        /// <summary>
        ///  gets a value indicating whether the IList is read-only.
        /// </summary>
        /// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get {
                return false;
            }
        }
#endregion

#region Operations (type-safe IEnumerable)

        /// <summary>
        ///  Returns an enumerator that can iterate through the <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <c>SoqlExpressionCollection</c>.</returns>
        public virtual ISoqlExpressionCollectionEnumerator GetEnumerator() {
            return new Enumerator(this);
        }
#endregion

#region Public helpers (just to mimic some nice features of ArrayList)

        /// <summary>
        ///  Gets or sets the number of elements the <c>SoqlExpressionCollection</c> can contain.
        /// </summary>
        public virtual int Capacity
        {
            get {
                return m_array.Length;
            }

            set {
                if (value < m_count)
                    value = m_count;

                if (value != m_array.Length) {
                    if (value > 0) {
                        SoqlExpression[] temp = new SoqlExpression[value];
                        Array.Copy(m_array, temp, m_count);
                        m_array = temp;
                    } else {
                        m_array = new SoqlExpression[DEFAULT_CAPACITY];
                    }
                }
            }
        }

        /// <summary>
        ///  Adds the elements of another <c>SoqlExpressionCollection</c> to the current <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="x">The <c>SoqlExpressionCollection</c> whose elements should be added to the end of the current <c>SoqlExpressionCollection</c>.</param>
        /// <returns>The new <see cref="SoqlExpressionCollection.Count"/> of the <c>SoqlExpressionCollection</c>.</returns>
        public virtual int AddRange(SoqlExpressionCollection x) {
            if (m_count + x.Count >= m_array.Length)
                EnsureCapacity(m_count + x.Count);

            Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
            m_count += x.Count;
            m_version++;

            return m_count;
        }

        /// <summary>
        ///  Adds the elements of a <see cref="SoqlExpression"/> array to the current <c>SoqlExpressionCollection</c>.
        /// </summary>
        /// <param name="x">The <see cref="SoqlExpression"/> array whose elements should be added to the end of the <c>SoqlExpressionCollection</c>.</param>
        /// <returns>The new <see cref="SoqlExpressionCollection.Count"/> of the <c>SoqlExpressionCollection</c>.</returns>
        public virtual int AddRange(SoqlExpression[] x) {
            if (m_count + x.Length >= m_array.Length)
                EnsureCapacity(m_count + x.Length);

            Array.Copy(x, 0, m_array, m_count, x.Length);
            m_count += x.Length;
            m_version++;

            return m_count;
        }

        /// <summary>
        ///  Sets the capacity to the actual number of elements.
        /// </summary>
        public virtual void TrimToSize() {
            this.Capacity = m_count;
        }

#endregion

#region Implementation (helpers)

        /// <exception cref="ArgumentOutOfRangeException">
        ///  <para><paramref name="index"/> is less than zero</para>
        ///  <para>-or-</para>
        ///  <para><paramref name="index"/> is equal to or greater than <see cref="SoqlExpressionCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i) {
            ValidateIndex(i, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///  <para><paramref name="index"/> is less than zero</para>
        ///  <para>-or-</para>
        ///  <para><paramref name="index"/> is equal to or greater than <see cref="SoqlExpressionCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i, bool allowEqualEnd) {
            int max = (allowEqualEnd) ? (m_count) : (m_count - 1);
            if (i < 0 || i > max)
                throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
        }

        private void EnsureCapacity(int min) {
            int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
            if (newCapacity < min)
                newCapacity = min;

            this.Capacity = newCapacity;
        }

#endregion

#region Implementation (ICollection)

        void ICollection.CopyTo(Array array, int start) {
            Array.Copy(m_array, 0, array, start, m_count);
        }

#endregion

#region Implementation (IList)

        object IList.this[int i]
        {
            get {
                return (object)this[i];
            }
            set {
                this[i] = (SoqlExpression)value;
            }
        }

        int IList.Add(object x) {
            return this.Add((SoqlExpression)x);
        }

        bool IList.Contains(object x) {
            return this.Contains((SoqlExpression)x);
        }

        int IList.IndexOf(object x) {
            return this.IndexOf((SoqlExpression)x);
        }

        void IList.Insert(int pos, object x) {
            this.Insert(pos, (SoqlExpression)x);
        }

        void IList.Remove(object x) {
            this.Remove((SoqlExpression)x);
        }

        void IList.RemoveAt(int pos) {
            this.RemoveAt(pos);
        }

#endregion

#region Implementation (IEnumerable)

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)(this.GetEnumerator());
        }

#endregion

#region Nested enumerator class
        /// <summary>
        ///  Supports simple iteration over a <see cref="SoqlExpressionCollection"/>.
        /// </summary>
    private class Enumerator : IEnumerator, ISoqlExpressionCollectionEnumerator {
#region Implementation (data)

            private SoqlExpressionCollection m_collection;
            private int m_index;
            private int m_version;

#endregion

#region Construction

            /// <summary>
            ///  Initializes a new instance of the <c>Enumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal Enumerator(SoqlExpressionCollection tc) {
                m_collection = tc;
                m_index = -1;
                m_version = tc.m_version;
            }

#endregion

#region Operations (type-safe IEnumerator)

            /// <summary>
            ///  Gets the current element in the collection.
            /// </summary>
            public SoqlExpression Current
            {
                get {
                    return m_collection[m_index];
                }
            }

            /// <summary>
            ///  Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            ///  The collection was modified after the enumerator was created.
            /// </exception>
            /// <returns>
            ///  <c>true</c> if the enumerator was successfully advanced to the next element;
            ///  <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext() {
                if (m_version != m_collection.m_version)
                    throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                ++m_index;
                return (m_index < m_collection.Count) ? true : false;
            }

            /// <summary>
            ///  Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            public void Reset() {
                m_index = -1;
            }
#endregion

#region Implementation (IEnumerator)

            object IEnumerator.Current
            {
                get {
                    return (object)(this.Current);
                }
            }

#endregion

        }
#endregion

#region Nested Syncronized Wrapper class
    private class SyncSoqlExpressionCollection : SoqlExpressionCollection {
#region Implementation (data)
            private SoqlExpressionCollection m_collection;
            private object m_root;
#endregion

#region Construction

            internal SyncSoqlExpressionCollection(SoqlExpressionCollection list) : base(Tag.Default) {
                m_root = list.SyncRoot;
                m_collection = list;
            }
#endregion

#region Type-safe ICollection
            public override void CopyTo(SoqlExpression[] array) {
                lock (this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(SoqlExpression[] array, int start) {
                lock (this.m_root)
                    m_collection.CopyTo(array, start);
            }
            public override int Count
            {
                get {
                    lock (this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get {
                    return true;
                }
            }

            public override object SyncRoot
            {
                get {
                    return this.m_root;
                }
            }
#endregion

#region Type-safe IList
            public override SoqlExpression this[int i]
            {
                get {
                    lock (this.m_root)
                        return m_collection[i];
                }
                set {
                    lock (this.m_root)
                        m_collection[i] = value;
                }
            }

            public override int Add(SoqlExpression x) {
                lock (this.m_root)
                    return m_collection.Add(x);
            }

            public override void Clear() {
                lock (this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(SoqlExpression x) {
                lock (this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(SoqlExpression x) {
                lock (this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, SoqlExpression x) {
                lock (this.m_root)
                    m_collection.Insert(pos, x);
            }

            public override void Remove(SoqlExpression x) {
                lock (this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos) {
                lock (this.m_root)
                    m_collection.RemoveAt(pos);
            }

            public override bool IsFixedSize
            {
                get {
                    return m_collection.IsFixedSize;
                }
            }

            public override bool IsReadOnly
            {
                get {
                    return m_collection.IsReadOnly;
                }
            }
#endregion

#region Type-safe IEnumerable
            public override ISoqlExpressionCollectionEnumerator GetEnumerator() {
                lock (m_root)
                    return m_collection.GetEnumerator();
            }
#endregion

#region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get {
                    lock (this.m_root)
                        return m_collection.Capacity;
                }

                set {
                    lock (this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(SoqlExpressionCollection x) {
                lock (this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(SoqlExpression[] x) {
                lock (this.m_root)
                    return m_collection.AddRange(x);
            }
#endregion

        }
#endregion

#region Nested Read Only Wrapper class
    private class ReadOnlySoqlExpressionCollection : SoqlExpressionCollection {
#region Implementation (data)
            private SoqlExpressionCollection m_collection;
#endregion

#region Construction

            internal ReadOnlySoqlExpressionCollection(SoqlExpressionCollection list) : base(Tag.Default) {
                m_collection = list;
            }
#endregion

#region Type-safe ICollection
            public override void CopyTo(SoqlExpression[] array) {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(SoqlExpression[] array, int start) {
                m_collection.CopyTo(array, start);
            }
            public override int Count
            {
                get {
                    return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get {
                    return m_collection.IsSynchronized;
                }
            }

            public override object SyncRoot
            {
                get {
                    return this.m_collection.SyncRoot;
                }
            }
#endregion

#region Type-safe IList
            public override SoqlExpression this[int i]
            {
                get {
                    return m_collection[i];
                }
                set {
                    throw new NotSupportedException("This is a Read Only Collection and can not be modified");
                }
            }

            public override int Add(SoqlExpression x) {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Clear() {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(SoqlExpression x) {
                return m_collection.Contains(x);
            }

            public override int IndexOf(SoqlExpression x) {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, SoqlExpression x) {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(SoqlExpression x) {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos) {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool IsFixedSize
            {
                get {
                    return true;
                }
            }

            public override bool IsReadOnly
            {
                get {
                    return true;
                }
            }
#endregion

#region Type-safe IEnumerable
            public override ISoqlExpressionCollectionEnumerator GetEnumerator() {
                return m_collection.GetEnumerator();
            }
#endregion

#region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get {
                    return m_collection.Capacity;
                }

                set {
                    throw new NotSupportedException("This is a Read Only Collection and can not be modified");
                }
            }

            public override int AddRange(SoqlExpressionCollection x) {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(SoqlExpression[] x) {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
#endregion

        }
#endregion

    }

}
namespace Sooda.QL {
    public
    class PrefixToExpressionTypeMap : IDictionary, ICollection, IEnumerable, ICloneable {
        protected Hashtable innerHash;

#region "Constructors"

        public PrefixToExpressionTypeMap() {
            innerHash = new Hashtable();
        }
        public PrefixToExpressionTypeMap(PrefixToExpressionTypeMap original) {
            innerHash = new Hashtable (original.innerHash);
        }
        public PrefixToExpressionTypeMap(IDictionary dictionary) {
            innerHash = new Hashtable (dictionary);
        }

        public PrefixToExpressionTypeMap(int capacity) {
            innerHash = new Hashtable(capacity);
        }

        public PrefixToExpressionTypeMap(IDictionary dictionary, float loadFactor) {
            innerHash = new Hashtable(dictionary, loadFactor);
        }

        public PrefixToExpressionTypeMap(IHashCodeProvider codeProvider, IComparer comparer) {
            innerHash = new Hashtable (codeProvider, comparer);
        }

        public PrefixToExpressionTypeMap(int capacity, int loadFactor) {
            innerHash = new Hashtable(capacity, loadFactor);
        }

        public PrefixToExpressionTypeMap(IDictionary dictionary, IHashCodeProvider codeProvider, IComparer comparer) {
            innerHash = new Hashtable (dictionary, codeProvider, comparer);
        }

        public PrefixToExpressionTypeMap(int capacity, IHashCodeProvider codeProvider, IComparer comparer) {
            innerHash = new Hashtable (capacity, codeProvider, comparer);
        }

        public PrefixToExpressionTypeMap(IDictionary dictionary, float loadFactor, IHashCodeProvider codeProvider, IComparer comparer) {
            innerHash = new Hashtable (dictionary, loadFactor, codeProvider, comparer);
        }

        public PrefixToExpressionTypeMap(int capacity, float loadFactor, IHashCodeProvider codeProvider, IComparer comparer) {
            innerHash = new Hashtable (capacity, loadFactor, codeProvider, comparer);
        }


#endregion

#region Implementation of IDictionary
        public PrefixToExpressionTypeMapEnumerator GetEnumerator() {
            return new PrefixToExpressionTypeMapEnumerator(this);
        }

        System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new PrefixToExpressionTypeMapEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Remove(string key) {
            innerHash.Remove (key);
        }
        void IDictionary.Remove(object key) {
            Remove ((string)key);
        }

        public bool Contains(string key) {
            return innerHash.Contains(key);
        }
        bool IDictionary.Contains(object key) {
            return Contains((string)key);
        }

        public void Clear() {
            innerHash.Clear();
        }

        public void Add(string key, SoqlExpressionType value) {
            innerHash.Add (key, value);
        }
        void IDictionary.Add(object key, object value) {
            Add ((string)key, (SoqlExpressionType)value);
        }

        public bool IsReadOnly
        {
            get {
                return innerHash.IsReadOnly;
            }
        }

        public SoqlExpressionType this[string key]
        {
            get {
                return (SoqlExpressionType) innerHash[key];
            }
            set {
                innerHash[key] = value;
            }
        }
        object IDictionary.this[object key]
        {
            get {
                return this[(string)key];
            }
            set {
                this[(string)key] = (SoqlExpressionType)value;
            }
        }

        public System.Collections.ICollection Values
        {
            get {
                return innerHash.Values;
            }
        }

        public System.Collections.ICollection Keys
        {
            get {
                return innerHash.Keys;
            }
        }

        public bool IsFixedSize
        {
            get {
                return innerHash.IsFixedSize;
            }
        }
#endregion

#region Implementation of ICollection
        public void CopyTo(System.Array array, int index) {
            innerHash.CopyTo (array, index);
        }

        public bool IsSynchronized
        {
            get {
                return innerHash.IsSynchronized;
            }
        }

        public int Count
        {
            get {
                return innerHash.Count;
            }
        }

        public object SyncRoot
        {
            get {
                return innerHash.SyncRoot;
            }
        }
#endregion

#region Implementation of ICloneable
        public PrefixToExpressionTypeMap Clone() {
            PrefixToExpressionTypeMap clone = new PrefixToExpressionTypeMap();
            clone.innerHash = (Hashtable) innerHash.Clone();

            return clone;
        }
        object ICloneable.Clone() {
            return Clone();
        }
#endregion

#region "HashTable Methods"
        public bool ContainsKey (string key) {
            return innerHash.ContainsKey(key);
        }
        public bool ContainsValue (SoqlExpressionType value) {
            return innerHash.ContainsValue(value);
        }
        public static PrefixToExpressionTypeMap Synchronized(PrefixToExpressionTypeMap nonSync) {
            PrefixToExpressionTypeMap sync = new PrefixToExpressionTypeMap();
            sync.innerHash = Hashtable.Synchronized(nonSync.innerHash);

            return sync;
        }
#endregion

        internal Hashtable InnerHash
        {
            get {
                return innerHash;
            }
        }
    }

    public class PrefixToExpressionTypeMapEnumerator : IDictionaryEnumerator {
        private IDictionaryEnumerator innerEnumerator;

        internal PrefixToExpressionTypeMapEnumerator (PrefixToExpressionTypeMap enumerable) {
            innerEnumerator = enumerable.InnerHash.GetEnumerator();
        }

#region Implementation of IDictionaryEnumerator
        public string Key
        {
            get {
                return (string)innerEnumerator.Key;
            }
        }
        object IDictionaryEnumerator.Key
        {
            get {
                return Key;
            }
        }


        public SoqlExpressionType Value
        {
            get {
                return (SoqlExpressionType)innerEnumerator.Value;
            }
        }
        object IDictionaryEnumerator.Value
        {
            get {
                return Value;
            }
        }

        public System.Collections.DictionaryEntry Entry
        {
            get {
                return innerEnumerator.Entry;
            }
        }

#endregion

#region Implementation of IEnumerator
        public void Reset() {
            innerEnumerator.Reset();
        }

        public bool MoveNext() {
            return innerEnumerator.MoveNext();
        }

        public object Current
        {
            get {
                return innerEnumerator.Current;
            }
        }
#endregion

    }

}
