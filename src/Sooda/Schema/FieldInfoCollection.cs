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
    /// A collection of elements of type FieldInfo
    /// </summary>
    [Serializable]
    public class FieldInfoCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the FieldInfoCollection class.
        /// </summary>
        public FieldInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the FieldInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new FieldInfoCollection.
        /// </param>
        public FieldInfoCollection(FieldInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the FieldInfoCollection class, containing elements
        /// copied from another instance of FieldInfoCollection
        /// </summary>
        /// <param name="items">
        /// The FieldInfoCollection whose elements are to be added to the new FieldInfoCollection.
        /// </param>
        public FieldInfoCollection(FieldInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this FieldInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this FieldInfoCollection.
        /// </param>
        public virtual void AddRange(FieldInfo[] items)
        {
            foreach (FieldInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another FieldInfoCollection to the end of this FieldInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The FieldInfoCollection whose elements are to be added to the end of this FieldInfoCollection.
        /// </param>
        public virtual void AddRange(FieldInfoCollection items)
        {
            foreach (FieldInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type FieldInfo to the end of this FieldInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The FieldInfo to be added to the end of this FieldInfoCollection.
        /// </param>
        public virtual void Add(FieldInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic FieldInfo value is in this FieldInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The FieldInfo value to locate in this FieldInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this FieldInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(FieldInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this FieldInfoCollection
        /// </summary>
        /// <param name="value">
        /// The FieldInfo value to locate in the FieldInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(FieldInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the FieldInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the FieldInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The FieldInfo to insert.
        /// </param>
        public virtual void Insert(int index, FieldInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the FieldInfo at the given index in this FieldInfoCollection.
        /// </summary>
        public virtual FieldInfo this[int index]
        {
            get
            {
                return (FieldInfo)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific FieldInfo from this FieldInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The FieldInfo value to remove from this FieldInfoCollection.
        /// </param>
        public virtual void Remove(FieldInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by FieldInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(FieldInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public FieldInfo Current
            {
                get
                {
                    return (FieldInfo)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (FieldInfo)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this FieldInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual FieldInfoCollection.Enumerator GetEnumerator()
        {
            return new FieldInfoCollection.Enumerator(this);
        }
    }
}
