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

namespace Sooda.Schema
{
    /// <summary>
    /// A collection of elements of type IndexInfo
    /// </summary>
    [Serializable]
    public class IndexInfoCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the IndexInfoCollection class.
        /// </summary>
        public IndexInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the IndexInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new IndexInfoCollection.
        /// </param>
        public IndexInfoCollection(IndexInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the IndexInfoCollection class, containing elements
        /// copied from another instance of IndexInfoCollection
        /// </summary>
        /// <param name="items">
        /// The IndexInfoCollection whose elements are to be added to the new IndexInfoCollection.
        /// </param>
        public IndexInfoCollection(IndexInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this IndexInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this IndexInfoCollection.
        /// </param>
        public virtual void AddRange(IndexInfo[] items)
        {
            foreach (IndexInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another IndexInfoCollection to the end of this IndexInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The IndexInfoCollection whose elements are to be added to the end of this IndexInfoCollection.
        /// </param>
        public virtual void AddRange(IndexInfoCollection items)
        {
            foreach (IndexInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type IndexInfo to the end of this IndexInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IndexInfo to be added to the end of this IndexInfoCollection.
        /// </param>
        public virtual void Add(IndexInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic IndexInfo value is in this IndexInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IndexInfo value to locate in this IndexInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this IndexInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(IndexInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this IndexInfoCollection
        /// </summary>
        /// <param name="value">
        /// The IndexInfo value to locate in the IndexInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(IndexInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the IndexInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the IndexInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The IndexInfo to insert.
        /// </param>
        public virtual void Insert(int index, IndexInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the IndexInfo at the given index in this IndexInfoCollection.
        /// </summary>
        public virtual IndexInfo this[int index]
        {
            get
            {
                return (IndexInfo)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific IndexInfo from this IndexInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IndexInfo value to remove from this IndexInfoCollection.
        /// </param>
        public virtual void Remove(IndexInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by IndexInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(IndexInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public IndexInfo Current
            {
                get
                {
                    return (IndexInfo)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (IndexInfo)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this IndexInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual IndexInfoCollection.Enumerator GetEnumerator()
        {
            return new IndexInfoCollection.Enumerator(this);
        }
    }
}
