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
using System.Xml.Serialization;

namespace Sooda.Schema {
    /// <summary>
    /// A collection of elements of type IndexFieldInfo
    /// </summary>
    [Serializable]
    public class IndexFieldInfoCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the IndexFieldInfoCollection class.
        /// </summary>
        public IndexFieldInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the IndexFieldInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new IndexFieldInfoCollection.
        /// </param>
        public IndexFieldInfoCollection(IndexFieldInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the IndexFieldInfoCollection class, containing elements
        /// copied from another instance of IndexFieldInfoCollection
        /// </summary>
        /// <param name="items">
        /// The IndexFieldInfoCollection whose elements are to be added to the new IndexFieldInfoCollection.
        /// </param>
        public IndexFieldInfoCollection(IndexFieldInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this IndexFieldInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this IndexFieldInfoCollection.
        /// </param>
        public virtual void AddRange(IndexFieldInfo[] items)
        {
            foreach (IndexFieldInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another IndexFieldInfoCollection to the end of this IndexFieldInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The IndexFieldInfoCollection whose elements are to be added to the end of this IndexFieldInfoCollection.
        /// </param>
        public virtual void AddRange(IndexFieldInfoCollection items)
        {
            foreach (IndexFieldInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type IndexFieldInfo to the end of this IndexFieldInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IndexFieldInfo to be added to the end of this IndexFieldInfoCollection.
        /// </param>
        public virtual void Add(IndexFieldInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic IndexFieldInfo value is in this IndexFieldInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IndexFieldInfo value to locate in this IndexFieldInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this IndexFieldInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(IndexFieldInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this IndexFieldInfoCollection
        /// </summary>
        /// <param name="value">
        /// The IndexFieldInfo value to locate in the IndexFieldInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(IndexFieldInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the IndexFieldInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the IndexFieldInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The IndexFieldInfo to insert.
        /// </param>
        public virtual void Insert(int index, IndexFieldInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the IndexFieldInfo at the given index in this IndexFieldInfoCollection.
        /// </summary>
        public virtual IndexFieldInfo this[int index]
        {
            get
            {
                return (IndexFieldInfo) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific IndexFieldInfo from this IndexFieldInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IndexFieldInfo value to remove from this IndexFieldInfoCollection.
        /// </param>
        public virtual void Remove(IndexFieldInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by IndexFieldInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(IndexFieldInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public IndexFieldInfo Current
            {
                get
                {
                    return (IndexFieldInfo) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (IndexFieldInfo) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this IndexFieldInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual IndexFieldInfoCollection.Enumerator GetEnumerator()
        {
            return new IndexFieldInfoCollection.Enumerator(this);
        }
    }
}
