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
using System.Collections;

namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.DataSourceInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class DataSourceInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="DataSourceInfoCollection"/>.
	    /// </summary>
        public interface IDataSourceInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.DataSourceInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.DataSourceInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>DataSourceInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>DataSourceInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static DataSourceInfoCollection Synchronized(DataSourceInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncDataSourceInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>DataSourceInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>DataSourceInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static DataSourceInfoCollection ReadOnly(DataSourceInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyDataSourceInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>DataSourceInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public DataSourceInfoCollection()
		{
			m_array = new Sooda.Schema.DataSourceInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>DataSourceInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>DataSourceInfoCollection</c> is initially capable of storing.
		///	</param>
		public DataSourceInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.DataSourceInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>DataSourceInfoCollection</c> class
		///		that contains elements copied from the specified <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>DataSourceInfoCollection</c> whose elements are copied to the new collection.</param>
		public DataSourceInfoCollection(DataSourceInfoCollection c)
		{
			m_array = new Sooda.Schema.DataSourceInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>DataSourceInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.DataSourceInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.DataSourceInfo"/> array whose elements are copied to the new list.</param>
		public DataSourceInfoCollection(Sooda.Schema.DataSourceInfo[] a)
		{
			m_array = new Sooda.Schema.DataSourceInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected DataSourceInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>DataSourceInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>DataSourceInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.DataSourceInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.DataSourceInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.DataSourceInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>DataSourceInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.DataSourceInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.DataSourceInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.DataSourceInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.DataSourceInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="DataSourceInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.DataSourceInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.DataSourceInfo"/> to the end of the <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.DataSourceInfo"/> to be added to the end of the <c>DataSourceInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.DataSourceInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>DataSourceInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.DataSourceInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="DataSourceInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			DataSourceInfoCollection newColl = new DataSourceInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.DataSourceInfo"/> is in the <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.DataSourceInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>DataSourceInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.DataSourceInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.DataSourceInfo"/>
		///		in the <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.DataSourceInfo"/> to locate in the <c>DataSourceInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>DataSourceInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.DataSourceInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>DataSourceInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.DataSourceInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="DataSourceInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.DataSourceInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.DataSourceInfo"/> from the <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.DataSourceInfo"/> to remove from the <c>DataSourceInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.DataSourceInfo"/> was not found in the <c>DataSourceInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.DataSourceInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="DataSourceInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.DataSourceInfo[] temp = new Sooda.Schema.DataSourceInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>DataSourceInfoCollection</c>.</returns>
		public virtual IDataSourceInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>DataSourceInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.DataSourceInfo[] temp = new Sooda.Schema.DataSourceInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.DataSourceInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>DataSourceInfoCollection</c> to the current <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>DataSourceInfoCollection</c> whose elements should be added to the end of the current <c>DataSourceInfoCollection</c>.</param>
		/// <returns>The new <see cref="DataSourceInfoCollection.Count"/> of the <c>DataSourceInfoCollection</c>.</returns>
		public virtual int AddRange(DataSourceInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.DataSourceInfo"/> array to the current <c>DataSourceInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.DataSourceInfo"/> array whose elements should be added to the end of the <c>DataSourceInfoCollection</c>.</param>
		/// <returns>The new <see cref="DataSourceInfoCollection.Count"/> of the <c>DataSourceInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.DataSourceInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="DataSourceInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="DataSourceInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.DataSourceInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.DataSourceInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.DataSourceInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.DataSourceInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.DataSourceInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.DataSourceInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="DataSourceInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, IDataSourceInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private DataSourceInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(DataSourceInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.DataSourceInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncDataSourceInfoCollection : DataSourceInfoCollection
        {
            #region Implementation (data)
            private DataSourceInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncDataSourceInfoCollection(DataSourceInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.DataSourceInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.DataSourceInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.DataSourceInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.DataSourceInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.DataSourceInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.DataSourceInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.DataSourceInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.DataSourceInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IDataSourceInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(DataSourceInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.DataSourceInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyDataSourceInfoCollection : DataSourceInfoCollection
        {
            #region Implementation (data)
            private DataSourceInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyDataSourceInfoCollection(DataSourceInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.DataSourceInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.DataSourceInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.DataSourceInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.DataSourceInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.DataSourceInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.DataSourceInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.DataSourceInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.DataSourceInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IDataSourceInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(DataSourceInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.DataSourceInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.ClassInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class ClassInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="ClassInfoCollection"/>.
	    /// </summary>
        public interface IClassInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.ClassInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.ClassInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>ClassInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>ClassInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static ClassInfoCollection Synchronized(ClassInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncClassInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>ClassInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>ClassInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static ClassInfoCollection ReadOnly(ClassInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyClassInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>ClassInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public ClassInfoCollection()
		{
			m_array = new Sooda.Schema.ClassInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>ClassInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>ClassInfoCollection</c> is initially capable of storing.
		///	</param>
		public ClassInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.ClassInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>ClassInfoCollection</c> class
		///		that contains elements copied from the specified <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>ClassInfoCollection</c> whose elements are copied to the new collection.</param>
		public ClassInfoCollection(ClassInfoCollection c)
		{
			m_array = new Sooda.Schema.ClassInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>ClassInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.ClassInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.ClassInfo"/> array whose elements are copied to the new list.</param>
		public ClassInfoCollection(Sooda.Schema.ClassInfo[] a)
		{
			m_array = new Sooda.Schema.ClassInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected ClassInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>ClassInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>ClassInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.ClassInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.ClassInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.ClassInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>ClassInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.ClassInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.ClassInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.ClassInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.ClassInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="ClassInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.ClassInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.ClassInfo"/> to the end of the <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.ClassInfo"/> to be added to the end of the <c>ClassInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.ClassInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>ClassInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.ClassInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="ClassInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			ClassInfoCollection newColl = new ClassInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.ClassInfo"/> is in the <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.ClassInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>ClassInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.ClassInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.ClassInfo"/>
		///		in the <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.ClassInfo"/> to locate in the <c>ClassInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>ClassInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.ClassInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>ClassInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.ClassInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="ClassInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.ClassInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.ClassInfo"/> from the <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.ClassInfo"/> to remove from the <c>ClassInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.ClassInfo"/> was not found in the <c>ClassInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.ClassInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="ClassInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.ClassInfo[] temp = new Sooda.Schema.ClassInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>ClassInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>ClassInfoCollection</c>.</returns>
		public virtual IClassInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>ClassInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.ClassInfo[] temp = new Sooda.Schema.ClassInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.ClassInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>ClassInfoCollection</c> to the current <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>ClassInfoCollection</c> whose elements should be added to the end of the current <c>ClassInfoCollection</c>.</param>
		/// <returns>The new <see cref="ClassInfoCollection.Count"/> of the <c>ClassInfoCollection</c>.</returns>
		public virtual int AddRange(ClassInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.ClassInfo"/> array to the current <c>ClassInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.ClassInfo"/> array whose elements should be added to the end of the <c>ClassInfoCollection</c>.</param>
		/// <returns>The new <see cref="ClassInfoCollection.Count"/> of the <c>ClassInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.ClassInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="ClassInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="ClassInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.ClassInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.ClassInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.ClassInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.ClassInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.ClassInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.ClassInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="ClassInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, IClassInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private ClassInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(ClassInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.ClassInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncClassInfoCollection : ClassInfoCollection
        {
            #region Implementation (data)
            private ClassInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncClassInfoCollection(ClassInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.ClassInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.ClassInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.ClassInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.ClassInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.ClassInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.ClassInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.ClassInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.ClassInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IClassInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(ClassInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.ClassInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyClassInfoCollection : ClassInfoCollection
        {
            #region Implementation (data)
            private ClassInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyClassInfoCollection(ClassInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.ClassInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.ClassInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.ClassInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.ClassInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.ClassInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.ClassInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.ClassInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.ClassInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IClassInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(ClassInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.ClassInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.TableInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class TableInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="TableInfoCollection"/>.
	    /// </summary>
        public interface ITableInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.TableInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.TableInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>TableInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>TableInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static TableInfoCollection Synchronized(TableInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncTableInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>TableInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>TableInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static TableInfoCollection ReadOnly(TableInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyTableInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>TableInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public TableInfoCollection()
		{
			m_array = new Sooda.Schema.TableInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>TableInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>TableInfoCollection</c> is initially capable of storing.
		///	</param>
		public TableInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.TableInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>TableInfoCollection</c> class
		///		that contains elements copied from the specified <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>TableInfoCollection</c> whose elements are copied to the new collection.</param>
		public TableInfoCollection(TableInfoCollection c)
		{
			m_array = new Sooda.Schema.TableInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>TableInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.TableInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.TableInfo"/> array whose elements are copied to the new list.</param>
		public TableInfoCollection(Sooda.Schema.TableInfo[] a)
		{
			m_array = new Sooda.Schema.TableInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected TableInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>TableInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>TableInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.TableInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.TableInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.TableInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>TableInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.TableInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.TableInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.TableInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.TableInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="TableInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.TableInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.TableInfo"/> to the end of the <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.TableInfo"/> to be added to the end of the <c>TableInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.TableInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>TableInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.TableInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="TableInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			TableInfoCollection newColl = new TableInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.TableInfo"/> is in the <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.TableInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>TableInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.TableInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.TableInfo"/>
		///		in the <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.TableInfo"/> to locate in the <c>TableInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>TableInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.TableInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>TableInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.TableInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="TableInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.TableInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.TableInfo"/> from the <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.TableInfo"/> to remove from the <c>TableInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.TableInfo"/> was not found in the <c>TableInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.TableInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="TableInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.TableInfo[] temp = new Sooda.Schema.TableInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>TableInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>TableInfoCollection</c>.</returns>
		public virtual ITableInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>TableInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.TableInfo[] temp = new Sooda.Schema.TableInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.TableInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>TableInfoCollection</c> to the current <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>TableInfoCollection</c> whose elements should be added to the end of the current <c>TableInfoCollection</c>.</param>
		/// <returns>The new <see cref="TableInfoCollection.Count"/> of the <c>TableInfoCollection</c>.</returns>
		public virtual int AddRange(TableInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.TableInfo"/> array to the current <c>TableInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.TableInfo"/> array whose elements should be added to the end of the <c>TableInfoCollection</c>.</param>
		/// <returns>The new <see cref="TableInfoCollection.Count"/> of the <c>TableInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.TableInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="TableInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="TableInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.TableInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.TableInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.TableInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.TableInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.TableInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.TableInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="TableInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, ITableInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private TableInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(TableInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.TableInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncTableInfoCollection : TableInfoCollection
        {
            #region Implementation (data)
            private TableInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncTableInfoCollection(TableInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.TableInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.TableInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.TableInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.TableInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.TableInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.TableInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.TableInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.TableInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override ITableInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(TableInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.TableInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyTableInfoCollection : TableInfoCollection
        {
            #region Implementation (data)
            private TableInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyTableInfoCollection(TableInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.TableInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.TableInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.TableInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.TableInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.TableInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.TableInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.TableInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.TableInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override ITableInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(TableInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.TableInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.RelationInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class RelationInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="RelationInfoCollection"/>.
	    /// </summary>
        public interface IRelationInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.RelationInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.RelationInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>RelationInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>RelationInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static RelationInfoCollection Synchronized(RelationInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncRelationInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>RelationInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>RelationInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static RelationInfoCollection ReadOnly(RelationInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyRelationInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>RelationInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public RelationInfoCollection()
		{
			m_array = new Sooda.Schema.RelationInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>RelationInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>RelationInfoCollection</c> is initially capable of storing.
		///	</param>
		public RelationInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.RelationInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>RelationInfoCollection</c> class
		///		that contains elements copied from the specified <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>RelationInfoCollection</c> whose elements are copied to the new collection.</param>
		public RelationInfoCollection(RelationInfoCollection c)
		{
			m_array = new Sooda.Schema.RelationInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>RelationInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.RelationInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.RelationInfo"/> array whose elements are copied to the new list.</param>
		public RelationInfoCollection(Sooda.Schema.RelationInfo[] a)
		{
			m_array = new Sooda.Schema.RelationInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected RelationInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>RelationInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>RelationInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.RelationInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.RelationInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.RelationInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>RelationInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.RelationInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.RelationInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.RelationInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.RelationInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="RelationInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.RelationInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.RelationInfo"/> to the end of the <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.RelationInfo"/> to be added to the end of the <c>RelationInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.RelationInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>RelationInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.RelationInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="RelationInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			RelationInfoCollection newColl = new RelationInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.RelationInfo"/> is in the <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.RelationInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>RelationInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.RelationInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.RelationInfo"/>
		///		in the <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.RelationInfo"/> to locate in the <c>RelationInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>RelationInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.RelationInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>RelationInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.RelationInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="RelationInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.RelationInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.RelationInfo"/> from the <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.RelationInfo"/> to remove from the <c>RelationInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.RelationInfo"/> was not found in the <c>RelationInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.RelationInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="RelationInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.RelationInfo[] temp = new Sooda.Schema.RelationInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>RelationInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>RelationInfoCollection</c>.</returns>
		public virtual IRelationInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>RelationInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.RelationInfo[] temp = new Sooda.Schema.RelationInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.RelationInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>RelationInfoCollection</c> to the current <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>RelationInfoCollection</c> whose elements should be added to the end of the current <c>RelationInfoCollection</c>.</param>
		/// <returns>The new <see cref="RelationInfoCollection.Count"/> of the <c>RelationInfoCollection</c>.</returns>
		public virtual int AddRange(RelationInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.RelationInfo"/> array to the current <c>RelationInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.RelationInfo"/> array whose elements should be added to the end of the <c>RelationInfoCollection</c>.</param>
		/// <returns>The new <see cref="RelationInfoCollection.Count"/> of the <c>RelationInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.RelationInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="RelationInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="RelationInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.RelationInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.RelationInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.RelationInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.RelationInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.RelationInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.RelationInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="RelationInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, IRelationInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private RelationInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(RelationInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.RelationInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncRelationInfoCollection : RelationInfoCollection
        {
            #region Implementation (data)
            private RelationInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncRelationInfoCollection(RelationInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.RelationInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.RelationInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.RelationInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.RelationInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.RelationInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.RelationInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.RelationInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.RelationInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IRelationInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(RelationInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.RelationInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyRelationInfoCollection : RelationInfoCollection
        {
            #region Implementation (data)
            private RelationInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyRelationInfoCollection(RelationInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.RelationInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.RelationInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.RelationInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.RelationInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.RelationInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.RelationInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.RelationInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.RelationInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IRelationInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(RelationInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.RelationInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.FieldInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class FieldInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="FieldInfoCollection"/>.
	    /// </summary>
        public interface IFieldInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.FieldInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.FieldInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>FieldInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>FieldInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static FieldInfoCollection Synchronized(FieldInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncFieldInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>FieldInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>FieldInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static FieldInfoCollection ReadOnly(FieldInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyFieldInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>FieldInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public FieldInfoCollection()
		{
			m_array = new Sooda.Schema.FieldInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>FieldInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>FieldInfoCollection</c> is initially capable of storing.
		///	</param>
		public FieldInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.FieldInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>FieldInfoCollection</c> class
		///		that contains elements copied from the specified <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>FieldInfoCollection</c> whose elements are copied to the new collection.</param>
		public FieldInfoCollection(FieldInfoCollection c)
		{
			m_array = new Sooda.Schema.FieldInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>FieldInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.FieldInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.FieldInfo"/> array whose elements are copied to the new list.</param>
		public FieldInfoCollection(Sooda.Schema.FieldInfo[] a)
		{
			m_array = new Sooda.Schema.FieldInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected FieldInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>FieldInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>FieldInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.FieldInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.FieldInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.FieldInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>FieldInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.FieldInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.FieldInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.FieldInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.FieldInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="FieldInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.FieldInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.FieldInfo"/> to the end of the <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.FieldInfo"/> to be added to the end of the <c>FieldInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.FieldInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>FieldInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.FieldInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="FieldInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			FieldInfoCollection newColl = new FieldInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.FieldInfo"/> is in the <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.FieldInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>FieldInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.FieldInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.FieldInfo"/>
		///		in the <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.FieldInfo"/> to locate in the <c>FieldInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>FieldInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.FieldInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>FieldInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.FieldInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="FieldInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.FieldInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.FieldInfo"/> from the <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.FieldInfo"/> to remove from the <c>FieldInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.FieldInfo"/> was not found in the <c>FieldInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.FieldInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="FieldInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.FieldInfo[] temp = new Sooda.Schema.FieldInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>FieldInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>FieldInfoCollection</c>.</returns>
		public virtual IFieldInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>FieldInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.FieldInfo[] temp = new Sooda.Schema.FieldInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.FieldInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>FieldInfoCollection</c> to the current <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>FieldInfoCollection</c> whose elements should be added to the end of the current <c>FieldInfoCollection</c>.</param>
		/// <returns>The new <see cref="FieldInfoCollection.Count"/> of the <c>FieldInfoCollection</c>.</returns>
		public virtual int AddRange(FieldInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.FieldInfo"/> array to the current <c>FieldInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.FieldInfo"/> array whose elements should be added to the end of the <c>FieldInfoCollection</c>.</param>
		/// <returns>The new <see cref="FieldInfoCollection.Count"/> of the <c>FieldInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.FieldInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="FieldInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="FieldInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.FieldInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.FieldInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.FieldInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.FieldInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.FieldInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.FieldInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="FieldInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, IFieldInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private FieldInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(FieldInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.FieldInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncFieldInfoCollection : FieldInfoCollection
        {
            #region Implementation (data)
            private FieldInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncFieldInfoCollection(FieldInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.FieldInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.FieldInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.FieldInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.FieldInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.FieldInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.FieldInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.FieldInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.FieldInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IFieldInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(FieldInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.FieldInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyFieldInfoCollection : FieldInfoCollection
        {
            #region Implementation (data)
            private FieldInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyFieldInfoCollection(FieldInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.FieldInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.FieldInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.FieldInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.FieldInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.FieldInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.FieldInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.FieldInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.FieldInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IFieldInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(FieldInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.FieldInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.IndexInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class IndexInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="IndexInfoCollection"/>.
	    /// </summary>
        public interface IIndexInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.IndexInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.IndexInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>IndexInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>IndexInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static IndexInfoCollection Synchronized(IndexInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncIndexInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>IndexInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>IndexInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static IndexInfoCollection ReadOnly(IndexInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyIndexInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>IndexInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public IndexInfoCollection()
		{
			m_array = new Sooda.Schema.IndexInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>IndexInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>IndexInfoCollection</c> is initially capable of storing.
		///	</param>
		public IndexInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.IndexInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>IndexInfoCollection</c> class
		///		that contains elements copied from the specified <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>IndexInfoCollection</c> whose elements are copied to the new collection.</param>
		public IndexInfoCollection(IndexInfoCollection c)
		{
			m_array = new Sooda.Schema.IndexInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>IndexInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.IndexInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.IndexInfo"/> array whose elements are copied to the new list.</param>
		public IndexInfoCollection(Sooda.Schema.IndexInfo[] a)
		{
			m_array = new Sooda.Schema.IndexInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected IndexInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>IndexInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>IndexInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.IndexInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.IndexInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.IndexInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>IndexInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.IndexInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.IndexInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.IndexInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.IndexInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.IndexInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.IndexInfo"/> to the end of the <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexInfo"/> to be added to the end of the <c>IndexInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.IndexInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>IndexInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.IndexInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="IndexInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			IndexInfoCollection newColl = new IndexInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.IndexInfo"/> is in the <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>IndexInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.IndexInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.IndexInfo"/>
		///		in the <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexInfo"/> to locate in the <c>IndexInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>IndexInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.IndexInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>IndexInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.IndexInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.IndexInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.IndexInfo"/> from the <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexInfo"/> to remove from the <c>IndexInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.IndexInfo"/> was not found in the <c>IndexInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.IndexInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.IndexInfo[] temp = new Sooda.Schema.IndexInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>IndexInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>IndexInfoCollection</c>.</returns>
		public virtual IIndexInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>IndexInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.IndexInfo[] temp = new Sooda.Schema.IndexInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.IndexInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>IndexInfoCollection</c> to the current <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>IndexInfoCollection</c> whose elements should be added to the end of the current <c>IndexInfoCollection</c>.</param>
		/// <returns>The new <see cref="IndexInfoCollection.Count"/> of the <c>IndexInfoCollection</c>.</returns>
		public virtual int AddRange(IndexInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.IndexInfo"/> array to the current <c>IndexInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.IndexInfo"/> array whose elements should be added to the end of the <c>IndexInfoCollection</c>.</param>
		/// <returns>The new <see cref="IndexInfoCollection.Count"/> of the <c>IndexInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.IndexInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.IndexInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.IndexInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.IndexInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.IndexInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.IndexInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.IndexInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="IndexInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, IIndexInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private IndexInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(IndexInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.IndexInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncIndexInfoCollection : IndexInfoCollection
        {
            #region Implementation (data)
            private IndexInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncIndexInfoCollection(IndexInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.IndexInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.IndexInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.IndexInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.IndexInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.IndexInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.IndexInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.IndexInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.IndexInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IIndexInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(IndexInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.IndexInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyIndexInfoCollection : IndexInfoCollection
        {
            #region Implementation (data)
            private IndexInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyIndexInfoCollection(IndexInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.IndexInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.IndexInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.IndexInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.IndexInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.IndexInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.IndexInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.IndexInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.IndexInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IIndexInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(IndexInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.IndexInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.IndexFieldInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class IndexFieldInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="IndexFieldInfoCollection"/>.
	    /// </summary>
        public interface IIndexFieldInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.IndexFieldInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.IndexFieldInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>IndexFieldInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>IndexFieldInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static IndexFieldInfoCollection Synchronized(IndexFieldInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncIndexFieldInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>IndexFieldInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>IndexFieldInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static IndexFieldInfoCollection ReadOnly(IndexFieldInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyIndexFieldInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>IndexFieldInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public IndexFieldInfoCollection()
		{
			m_array = new Sooda.Schema.IndexFieldInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>IndexFieldInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>IndexFieldInfoCollection</c> is initially capable of storing.
		///	</param>
		public IndexFieldInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.IndexFieldInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>IndexFieldInfoCollection</c> class
		///		that contains elements copied from the specified <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>IndexFieldInfoCollection</c> whose elements are copied to the new collection.</param>
		public IndexFieldInfoCollection(IndexFieldInfoCollection c)
		{
			m_array = new Sooda.Schema.IndexFieldInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>IndexFieldInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.IndexFieldInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.IndexFieldInfo"/> array whose elements are copied to the new list.</param>
		public IndexFieldInfoCollection(Sooda.Schema.IndexFieldInfo[] a)
		{
			m_array = new Sooda.Schema.IndexFieldInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected IndexFieldInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>IndexFieldInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.IndexFieldInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.IndexFieldInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.IndexFieldInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>IndexFieldInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.IndexFieldInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.IndexFieldInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.IndexFieldInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.IndexFieldInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexFieldInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.IndexFieldInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.IndexFieldInfo"/> to the end of the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexFieldInfo"/> to be added to the end of the <c>IndexFieldInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.IndexFieldInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.IndexFieldInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="IndexFieldInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			IndexFieldInfoCollection newColl = new IndexFieldInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.IndexFieldInfo"/> is in the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexFieldInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>IndexFieldInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.IndexFieldInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.IndexFieldInfo"/>
		///		in the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexFieldInfo"/> to locate in the <c>IndexFieldInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>IndexFieldInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.IndexFieldInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>IndexFieldInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.IndexFieldInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexFieldInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.IndexFieldInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.IndexFieldInfo"/> from the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.IndexFieldInfo"/> to remove from the <c>IndexFieldInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.IndexFieldInfo"/> was not found in the <c>IndexFieldInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.IndexFieldInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexFieldInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.IndexFieldInfo[] temp = new Sooda.Schema.IndexFieldInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>IndexFieldInfoCollection</c>.</returns>
		public virtual IIndexFieldInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>IndexFieldInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.IndexFieldInfo[] temp = new Sooda.Schema.IndexFieldInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.IndexFieldInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>IndexFieldInfoCollection</c> to the current <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>IndexFieldInfoCollection</c> whose elements should be added to the end of the current <c>IndexFieldInfoCollection</c>.</param>
		/// <returns>The new <see cref="IndexFieldInfoCollection.Count"/> of the <c>IndexFieldInfoCollection</c>.</returns>
		public virtual int AddRange(IndexFieldInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.IndexFieldInfo"/> array to the current <c>IndexFieldInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.IndexFieldInfo"/> array whose elements should be added to the end of the <c>IndexFieldInfoCollection</c>.</param>
		/// <returns>The new <see cref="IndexFieldInfoCollection.Count"/> of the <c>IndexFieldInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.IndexFieldInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexFieldInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="IndexFieldInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.IndexFieldInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.IndexFieldInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.IndexFieldInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.IndexFieldInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.IndexFieldInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.IndexFieldInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="IndexFieldInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, IIndexFieldInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private IndexFieldInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(IndexFieldInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.IndexFieldInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncIndexFieldInfoCollection : IndexFieldInfoCollection
        {
            #region Implementation (data)
            private IndexFieldInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncIndexFieldInfoCollection(IndexFieldInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.IndexFieldInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.IndexFieldInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.IndexFieldInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.IndexFieldInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.IndexFieldInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.IndexFieldInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.IndexFieldInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.IndexFieldInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IIndexFieldInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(IndexFieldInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.IndexFieldInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyIndexFieldInfoCollection : IndexFieldInfoCollection
        {
            #region Implementation (data)
            private IndexFieldInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyIndexFieldInfoCollection(IndexFieldInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.IndexFieldInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.IndexFieldInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.IndexFieldInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.IndexFieldInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.IndexFieldInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.IndexFieldInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.IndexFieldInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.IndexFieldInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IIndexFieldInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(IndexFieldInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.IndexFieldInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class CollectionOnetoManyInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="CollectionOnetoManyInfoCollection"/>.
	    /// </summary>
        public interface ICollectionOnetoManyInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.CollectionOnetoManyInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.CollectionOnetoManyInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>CollectionOnetoManyInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>CollectionOnetoManyInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static CollectionOnetoManyInfoCollection Synchronized(CollectionOnetoManyInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncCollectionOnetoManyInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>CollectionOnetoManyInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>CollectionOnetoManyInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static CollectionOnetoManyInfoCollection ReadOnly(CollectionOnetoManyInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyCollectionOnetoManyInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>CollectionOnetoManyInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public CollectionOnetoManyInfoCollection()
		{
			m_array = new Sooda.Schema.CollectionOnetoManyInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>CollectionOnetoManyInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>CollectionOnetoManyInfoCollection</c> is initially capable of storing.
		///	</param>
		public CollectionOnetoManyInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.CollectionOnetoManyInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>CollectionOnetoManyInfoCollection</c> class
		///		that contains elements copied from the specified <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>CollectionOnetoManyInfoCollection</c> whose elements are copied to the new collection.</param>
		public CollectionOnetoManyInfoCollection(CollectionOnetoManyInfoCollection c)
		{
			m_array = new Sooda.Schema.CollectionOnetoManyInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>CollectionOnetoManyInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array whose elements are copied to the new list.</param>
		public CollectionOnetoManyInfoCollection(Sooda.Schema.CollectionOnetoManyInfo[] a)
		{
			m_array = new Sooda.Schema.CollectionOnetoManyInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected CollectionOnetoManyInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>CollectionOnetoManyInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.CollectionOnetoManyInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>CollectionOnetoManyInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.CollectionOnetoManyInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionOnetoManyInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.CollectionOnetoManyInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> to the end of the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> to be added to the end of the <c>CollectionOnetoManyInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.CollectionOnetoManyInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.CollectionOnetoManyInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="CollectionOnetoManyInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			CollectionOnetoManyInfoCollection newColl = new CollectionOnetoManyInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> is in the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>CollectionOnetoManyInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.CollectionOnetoManyInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.CollectionOnetoManyInfo"/>
		///		in the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> to locate in the <c>CollectionOnetoManyInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>CollectionOnetoManyInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.CollectionOnetoManyInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>CollectionOnetoManyInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionOnetoManyInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.CollectionOnetoManyInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> from the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> to remove from the <c>CollectionOnetoManyInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> was not found in the <c>CollectionOnetoManyInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.CollectionOnetoManyInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionOnetoManyInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.CollectionOnetoManyInfo[] temp = new Sooda.Schema.CollectionOnetoManyInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>CollectionOnetoManyInfoCollection</c>.</returns>
		public virtual ICollectionOnetoManyInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>CollectionOnetoManyInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.CollectionOnetoManyInfo[] temp = new Sooda.Schema.CollectionOnetoManyInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.CollectionOnetoManyInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>CollectionOnetoManyInfoCollection</c> to the current <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>CollectionOnetoManyInfoCollection</c> whose elements should be added to the end of the current <c>CollectionOnetoManyInfoCollection</c>.</param>
		/// <returns>The new <see cref="CollectionOnetoManyInfoCollection.Count"/> of the <c>CollectionOnetoManyInfoCollection</c>.</returns>
		public virtual int AddRange(CollectionOnetoManyInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array to the current <c>CollectionOnetoManyInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.CollectionOnetoManyInfo"/> array whose elements should be added to the end of the <c>CollectionOnetoManyInfoCollection</c>.</param>
		/// <returns>The new <see cref="CollectionOnetoManyInfoCollection.Count"/> of the <c>CollectionOnetoManyInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.CollectionOnetoManyInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionOnetoManyInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionOnetoManyInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.CollectionOnetoManyInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.CollectionOnetoManyInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.CollectionOnetoManyInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.CollectionOnetoManyInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.CollectionOnetoManyInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.CollectionOnetoManyInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="CollectionOnetoManyInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, ICollectionOnetoManyInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private CollectionOnetoManyInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(CollectionOnetoManyInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.CollectionOnetoManyInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncCollectionOnetoManyInfoCollection : CollectionOnetoManyInfoCollection
        {
            #region Implementation (data)
            private CollectionOnetoManyInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncCollectionOnetoManyInfoCollection(CollectionOnetoManyInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.CollectionOnetoManyInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.CollectionOnetoManyInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.CollectionOnetoManyInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.CollectionOnetoManyInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.CollectionOnetoManyInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.CollectionOnetoManyInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.CollectionOnetoManyInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.CollectionOnetoManyInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override ICollectionOnetoManyInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(CollectionOnetoManyInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.CollectionOnetoManyInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyCollectionOnetoManyInfoCollection : CollectionOnetoManyInfoCollection
        {
            #region Implementation (data)
            private CollectionOnetoManyInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyCollectionOnetoManyInfoCollection(CollectionOnetoManyInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.CollectionOnetoManyInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.CollectionOnetoManyInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.CollectionOnetoManyInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.CollectionOnetoManyInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.CollectionOnetoManyInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.CollectionOnetoManyInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.CollectionOnetoManyInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.CollectionOnetoManyInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override ICollectionOnetoManyInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(CollectionOnetoManyInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.CollectionOnetoManyInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}
	
namespace Sooda.Schema
{
	

	/// <summary>
	///		A strongly-typed collection of <see cref="Sooda.Schema.CollectionManyToManyInfo"/> objects.
	/// </summary>
	[Serializable]
public 
  class CollectionManyToManyInfoCollection : ICollection, IList, IEnumerable, ICloneable
	{
        #region Interfaces
	    /// <summary>
	    ///		Supports type-safe iteration over a <see cref="CollectionManyToManyInfoCollection"/>.
	    /// </summary>
        public interface ICollectionManyToManyInfoCollectionEnumerator
        {
		    /// <summary>
		    ///		Gets the current element in the collection.
		    /// </summary>
            Sooda.Schema.CollectionManyToManyInfo Current {get;}

		    /// <summary>
		    ///		Advances the enumerator to the next element in the collection.
		    /// </summary>
		    /// <exception cref="InvalidOperationException">
		    ///		The collection was modified after the enumerator was created.
		    /// </exception>
		    /// <returns>
		    ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
		    ///		<c>false</c> if the enumerator has passed the end of the collection.
		    /// </returns>
            bool MoveNext();

		    /// <summary>
		    ///		Sets the enumerator to its initial position, before the first element in the collection.
		    /// </summary>
            void Reset();
        }
        #endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)
		private Sooda.Schema.CollectionManyToManyInfo[] m_array;
		private int m_count = 0;
		[NonSerialized]
		private int m_version = 0;
		#endregion
	
        #region Static Wrappers
		/// <summary>
		///		Creates a synchronized (thread-safe) wrapper for a 
		///     <c>CollectionManyToManyInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>CollectionManyToManyInfoCollection</c> wrapper that is synchronized (thread-safe).
		/// </returns>
        public static CollectionManyToManyInfoCollection Synchronized(CollectionManyToManyInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncCollectionManyToManyInfoCollection(list);
        }
        
		/// <summary>
		///		Creates a read-only wrapper for a 
		///     <c>CollectionManyToManyInfoCollection</c> instance.
		/// </summary>
		/// <returns>
		///     An <c>CollectionManyToManyInfoCollection</c> wrapper that is read-only.
		/// </returns>
        public static CollectionManyToManyInfoCollection ReadOnly(CollectionManyToManyInfoCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyCollectionManyToManyInfoCollection(list);
        }
        #endregion

	    #region Construction
		/// <summary>
		///		Initializes a new instance of the <c>CollectionManyToManyInfoCollection</c> class
		///		that is empty and has the default initial capacity.
		/// </summary>
		public CollectionManyToManyInfoCollection()
		{
			m_array = new Sooda.Schema.CollectionManyToManyInfo[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		///		Initializes a new instance of the <c>CollectionManyToManyInfoCollection</c> class
		///		that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		///		The number of elements that the new <c>CollectionManyToManyInfoCollection</c> is initially capable of storing.
		///	</param>
		public CollectionManyToManyInfoCollection(int capacity)
		{
			m_array = new Sooda.Schema.CollectionManyToManyInfo[capacity];
		}

		/// <summary>
		///		Initializes a new instance of the <c>CollectionManyToManyInfoCollection</c> class
		///		that contains elements copied from the specified <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="c">The <c>CollectionManyToManyInfoCollection</c> whose elements are copied to the new collection.</param>
		public CollectionManyToManyInfoCollection(CollectionManyToManyInfoCollection c)
		{
			m_array = new Sooda.Schema.CollectionManyToManyInfo[c.Count];
			AddRange(c);
		}

		/// <summary>
		///		Initializes a new instance of the <c>CollectionManyToManyInfoCollection</c> class
		///		that contains elements copied from the specified <see cref="Sooda.Schema.CollectionManyToManyInfo"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> array whose elements are copied to the new list.</param>
		public CollectionManyToManyInfoCollection(Sooda.Schema.CollectionManyToManyInfo[] a)
		{
			m_array = new Sooda.Schema.CollectionManyToManyInfo[a.Length];
			AddRange(a);
		}
		
        protected enum Tag {
            Default
        }

        protected CollectionManyToManyInfoCollection(Tag t)
        {
            m_array = null;
        }
		#endregion
		
		#region Operations (type-safe ICollection)
		/// <summary>
		///		Gets the number of elements actually contained in the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		///		Copies the entire <c>CollectionManyToManyInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.CollectionManyToManyInfo"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.CollectionManyToManyInfo"/> array to copy to.</param>
		public virtual void CopyTo(Sooda.Schema.CollectionManyToManyInfo[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		///		Copies the entire <c>CollectionManyToManyInfoCollection</c> to a one-dimensional
		///		<see cref="Sooda.Schema.CollectionManyToManyInfo"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Sooda.Schema.CollectionManyToManyInfo"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Sooda.Schema.CollectionManyToManyInfo[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
				throw new System.ArgumentException("Destination array was not long enough.");
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
		///		Gets an object that can be used to synchronize access to the collection.
		/// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }
		#endregion
		
		#region Operations (type-safe IList)
		/// <summary>
		///		Gets or sets the <see cref="Sooda.Schema.CollectionManyToManyInfo"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionManyToManyInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual Sooda.Schema.CollectionManyToManyInfo this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		///		Adds a <see cref="Sooda.Schema.CollectionManyToManyInfo"/> to the end of the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> to be added to the end of the <c>CollectionManyToManyInfoCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Sooda.Schema.CollectionManyToManyInfo item)
		{
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		///		Removes all elements from the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Sooda.Schema.CollectionManyToManyInfo[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		///		Creates a shallow copy of the <see cref="CollectionManyToManyInfoCollection"/>.
		/// </summary>
		public virtual object Clone()
		{
			CollectionManyToManyInfoCollection newColl = new CollectionManyToManyInfoCollection(m_count);
			Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
			newColl.m_count = m_count;
			newColl.m_version = m_version;

			return newColl;
		}

		/// <summary>
		///		Determines whether a given <see cref="Sooda.Schema.CollectionManyToManyInfo"/> is in the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>CollectionManyToManyInfoCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Sooda.Schema.CollectionManyToManyInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return true;
			return false;
		}

		/// <summary>
		///		Returns the zero-based index of the first occurrence of a <see cref="Sooda.Schema.CollectionManyToManyInfo"/>
		///		in the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> to locate in the <c>CollectionManyToManyInfoCollection</c>.</param>
		/// <returns>
		///		The zero-based index of the first occurrence of <paramref name="item"/> 
		///		in the entire <c>CollectionManyToManyInfoCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Sooda.Schema.CollectionManyToManyInfo item)
		{
			for (int i=0; i != m_count; ++i)
				if (m_array[i].Equals(item))
					return i;
			return -1;
		}

		/// <summary>
		///		Inserts an element into the <c>CollectionManyToManyInfoCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionManyToManyInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Sooda.Schema.CollectionManyToManyInfo item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
				EnsureCapacity(m_count + 1);

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		///		Removes the first occurrence of a specific <see cref="Sooda.Schema.CollectionManyToManyInfo"/> from the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> to remove from the <c>CollectionManyToManyInfoCollection</c>.</param>
		/// <exception cref="ArgumentException">
		///		The specified <see cref="Sooda.Schema.CollectionManyToManyInfo"/> was not found in the <c>CollectionManyToManyInfoCollection</c>.
		/// </exception>
		public virtual void Remove(Sooda.Schema.CollectionManyToManyInfo item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		///		Removes the element at the specified index of the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionManyToManyInfoCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Sooda.Schema.CollectionManyToManyInfo[] temp = new Sooda.Schema.CollectionManyToManyInfo[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		///		Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

		/// <summary>
		///		gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		///		Returns an enumerator that can iterate through the <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>CollectionManyToManyInfoCollection</c>.</returns>
		public virtual ICollectionManyToManyInfoCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		///		Gets or sets the number of elements the <c>CollectionManyToManyInfoCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get { return m_array.Length; }
			
			set
			{
				if (value < m_count)
					value = m_count;

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Sooda.Schema.CollectionManyToManyInfo[] temp = new Sooda.Schema.CollectionManyToManyInfo[value];
						Array.Copy(m_array, temp, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Sooda.Schema.CollectionManyToManyInfo[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		///		Adds the elements of another <c>CollectionManyToManyInfoCollection</c> to the current <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <c>CollectionManyToManyInfoCollection</c> whose elements should be added to the end of the current <c>CollectionManyToManyInfoCollection</c>.</param>
		/// <returns>The new <see cref="CollectionManyToManyInfoCollection.Count"/> of the <c>CollectionManyToManyInfoCollection</c>.</returns>
		public virtual int AddRange(CollectionManyToManyInfoCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
				EnsureCapacity(m_count + x.Count);
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		///		Adds the elements of a <see cref="Sooda.Schema.CollectionManyToManyInfo"/> array to the current <c>CollectionManyToManyInfoCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Sooda.Schema.CollectionManyToManyInfo"/> array whose elements should be added to the end of the <c>CollectionManyToManyInfoCollection</c>.</param>
		/// <returns>The new <see cref="CollectionManyToManyInfoCollection.Count"/> of the <c>CollectionManyToManyInfoCollection</c>.</returns>
		public virtual int AddRange(Sooda.Schema.CollectionManyToManyInfo[] x)
		{
			if (m_count + x.Length >= m_array.Length)
				EnsureCapacity(m_count + x.Length);

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}
		
		/// <summary>
		///		Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionManyToManyInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		///		<para><paramref name="index"/> is less than zero</para>
		///		<para>-or-</para>
		///		<para><paramref name="index"/> is equal to or greater than <see cref="CollectionManyToManyInfoCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd)?(m_count):(m_count-1);
			if (i < 0 || i > max)
				throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
				newCapacity = min;

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Sooda.Schema.CollectionManyToManyInfo)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Sooda.Schema.CollectionManyToManyInfo)x);
		}

    	bool IList.Contains(object x)
		{
			return this.Contains((Sooda.Schema.CollectionManyToManyInfo)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Sooda.Schema.CollectionManyToManyInfo)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Sooda.Schema.CollectionManyToManyInfo)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Sooda.Schema.CollectionManyToManyInfo)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class
		/// <summary>
		///		Supports simple iteration over a <see cref="CollectionManyToManyInfoCollection"/>.
		/// </summary>
		private class Enumerator : IEnumerator, ICollectionManyToManyInfoCollectionEnumerator
		{
			#region Implementation (data)
			
			private CollectionManyToManyInfoCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			///		Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(CollectionManyToManyInfoCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			///		Gets the current element in the collection.
			/// </summary>
			public Sooda.Schema.CollectionManyToManyInfo Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			///		Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <exception cref="InvalidOperationException">
			///		The collection was modified after the enumerator was created.
			/// </exception>
			/// <returns>
			///		<c>true</c> if the enumerator was successfully advanced to the next element; 
			///		<c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

				++m_index;
				return (m_index < m_collection.Count) ? true : false;
			}

			/// <summary>
			///		Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}
			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return (object)(this.Current); }
			}
			
			#endregion
		}
        #endregion
        
        #region Nested Syncronized Wrapper class
        private class SyncCollectionManyToManyInfoCollection : CollectionManyToManyInfoCollection
        {
            #region Implementation (data)
            private CollectionManyToManyInfoCollection m_collection;
            private object m_root;
            #endregion

            #region Construction
            internal SyncCollectionManyToManyInfoCollection(CollectionManyToManyInfoCollection list) : base(Tag.Default)
            {
                m_root = list.SyncRoot;
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.CollectionManyToManyInfo[] array)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.CollectionManyToManyInfo[] array, int start)
            {
                lock(this.m_root)
                    m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(this.m_root)
                        return m_collection.Count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return this.m_root; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.CollectionManyToManyInfo this[int i]
            {
                get
                {
                    lock(this.m_root)
                        return m_collection[i];
                }
                set
                {
                    lock(this.m_root)
                        m_collection[i] = value; 
                }
            }

            public override int Add(Sooda.Schema.CollectionManyToManyInfo x)
            {
                lock(this.m_root)
                    return m_collection.Add(x);
            }
            
            public override void Clear()
            {
                lock(this.m_root)
                    m_collection.Clear();
            }

            public override bool Contains(Sooda.Schema.CollectionManyToManyInfo x)
            {
                lock(this.m_root)
                    return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.CollectionManyToManyInfo x)
            {
                lock(this.m_root)
                    return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.CollectionManyToManyInfo x)
            {
                lock(this.m_root)
                    m_collection.Insert(pos,x);
            }

            public override void Remove(Sooda.Schema.CollectionManyToManyInfo x)
            {           
                lock(this.m_root)
                    m_collection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(this.m_root)
                    m_collection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize
            {
                get {return m_collection.IsFixedSize;}
            }

            public override bool IsReadOnly
            {
                get {return m_collection.IsReadOnly;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override ICollectionManyToManyInfoCollectionEnumerator GetEnumerator()
            {
                lock(m_root)
                    return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(this.m_root)
                        return m_collection.Capacity;
                }
                
                set
                {
                    lock(this.m_root)
                        m_collection.Capacity = value;
                }
            }

            public override int AddRange(CollectionManyToManyInfoCollection x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }

            public override int AddRange(Sooda.Schema.CollectionManyToManyInfo[] x)
            {
                lock(this.m_root)
                    return m_collection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyCollectionManyToManyInfoCollection : CollectionManyToManyInfoCollection
        {
            #region Implementation (data)
            private CollectionManyToManyInfoCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyCollectionManyToManyInfoCollection(CollectionManyToManyInfoCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(Sooda.Schema.CollectionManyToManyInfo[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Sooda.Schema.CollectionManyToManyInfo[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get {return m_collection.Count;}
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
            
            #region Type-safe IList
            public override Sooda.Schema.CollectionManyToManyInfo this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Sooda.Schema.CollectionManyToManyInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Sooda.Schema.CollectionManyToManyInfo x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Sooda.Schema.CollectionManyToManyInfo x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Sooda.Schema.CollectionManyToManyInfo x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Sooda.Schema.CollectionManyToManyInfo x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override ICollectionManyToManyInfoCollectionEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
                
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(CollectionManyToManyInfoCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Sooda.Schema.CollectionManyToManyInfo[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
	}

}