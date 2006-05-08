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

namespace Sooda.Collections
{
    /// <summary>
    /// A collection of elements of type SoodaDataSource
    /// </summary>
    public class SoodaDataSourceCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the SoodaDataSourceCollection class.
        /// </summary>
        public SoodaDataSourceCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the SoodaDataSourceCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new SoodaDataSourceCollection.
        /// </param>
        public SoodaDataSourceCollection(SoodaDataSource[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the SoodaDataSourceCollection class, containing elements
        /// copied from another instance of SoodaDataSourceCollection
        /// </summary>
        /// <param name="items">
        /// The SoodaDataSourceCollection whose elements are to be added to the new SoodaDataSourceCollection.
        /// </param>
        public SoodaDataSourceCollection(SoodaDataSourceCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this SoodaDataSourceCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this SoodaDataSourceCollection.
        /// </param>
        public virtual void AddRange(SoodaDataSource[] items)
        {
            foreach (SoodaDataSource item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another SoodaDataSourceCollection to the end of this SoodaDataSourceCollection.
        /// </summary>
        /// <param name="items">
        /// The SoodaDataSourceCollection whose elements are to be added to the end of this SoodaDataSourceCollection.
        /// </param>
        public virtual void AddRange(SoodaDataSourceCollection items)
        {
            foreach (SoodaDataSource item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type SoodaDataSource to the end of this SoodaDataSourceCollection.
        /// </summary>
        /// <param name="value">
        /// The SoodaDataSource to be added to the end of this SoodaDataSourceCollection.
        /// </param>
        public virtual void Add(SoodaDataSource value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic SoodaDataSource value is in this SoodaDataSourceCollection.
        /// </summary>
        /// <param name="value">
        /// The SoodaDataSource value to locate in this SoodaDataSourceCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this SoodaDataSourceCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(SoodaDataSource value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this SoodaDataSourceCollection
        /// </summary>
        /// <param name="value">
        /// The SoodaDataSource value to locate in the SoodaDataSourceCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(SoodaDataSource value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the SoodaDataSourceCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the SoodaDataSource is to be inserted.
        /// </param>
        /// <param name="value">
        /// The SoodaDataSource to insert.
        /// </param>
        public virtual void Insert(int index, SoodaDataSource value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the SoodaDataSource at the given index in this SoodaDataSourceCollection.
        /// </summary>
        public virtual SoodaDataSource this[int index]
        {
            get
            {
                return (SoodaDataSource)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific SoodaDataSource from this SoodaDataSourceCollection.
        /// </summary>
        /// <param name="value">
        /// The SoodaDataSource value to remove from this SoodaDataSourceCollection.
        /// </param>
        public virtual void Remove(SoodaDataSource value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by SoodaDataSourceCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(SoodaDataSourceCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public SoodaDataSource Current
            {
                get
                {
                    return (SoodaDataSource)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (SoodaDataSource)(this.wrapped.Current);
                }
            }

            public bool MoveNext()
            {
                return this.wrapped.MoveNext();
            }

            public void Reset()
            {
                this.wrapped.Reset();
            }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the elements of this SoodaDataSourceCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual SoodaDataSourceCollection.Enumerator GetEnumerator()
        {
            return new SoodaDataSourceCollection.Enumerator(this);
        }
    }
}

