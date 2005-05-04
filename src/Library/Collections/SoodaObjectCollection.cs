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

namespace Sooda.Collections
{
    /// <summary>
    /// A collection of elements of type SoodaObject
    /// </summary>
    public class SoodaObjectCollection: System.Collections.CollectionBase, ICloneable
    {
        /// <summary>
        /// Initializes a new empty instance of the SoodaObjectCollection class.
        /// </summary>
        public SoodaObjectCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the SoodaObjectCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new SoodaObjectCollection.
        /// </param>
        public SoodaObjectCollection(SoodaObject[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the SoodaObjectCollection class, containing elements
        /// copied from another instance of SoodaObjectCollection
        /// </summary>
        /// <param name="items">
        /// The SoodaObjectCollection whose elements are to be added to the new SoodaObjectCollection.
        /// </param>
        public SoodaObjectCollection(SoodaObjectCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this SoodaObjectCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this SoodaObjectCollection.
        /// </param>
        public virtual void AddRange(SoodaObject[] items)
        {
            foreach (SoodaObject item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another SoodaObjectCollection to the end of this SoodaObjectCollection.
        /// </summary>
        /// <param name="items">
        /// The SoodaObjectCollection whose elements are to be added to the end of this SoodaObjectCollection.
        /// </param>
        public virtual void AddRange(SoodaObjectCollection items)
        {
            foreach (SoodaObject item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type SoodaObject to the end of this SoodaObjectCollection.
        /// </summary>
        /// <param name="value">
        /// The SoodaObject to be added to the end of this SoodaObjectCollection.
        /// </param>
        public virtual int Add(SoodaObject value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic SoodaObject value is in this SoodaObjectCollection.
        /// </summary>
        /// <param name="value">
        /// The SoodaObject value to locate in this SoodaObjectCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this SoodaObjectCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(SoodaObject value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this SoodaObjectCollection
        /// </summary>
        /// <param name="value">
        /// The SoodaObject value to locate in the SoodaObjectCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(SoodaObject value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the SoodaObjectCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the SoodaObject is to be inserted.
        /// </param>
        /// <param name="value">
        /// The SoodaObject to insert.
        /// </param>
        public virtual void Insert(int index, SoodaObject value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the SoodaObject at the given index in this SoodaObjectCollection.
        /// </summary>
        public virtual SoodaObject this[int index]
        {
            get
            {
                return (SoodaObject) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific SoodaObject from this SoodaObjectCollection.
        /// </summary>
        /// <param name="value">
        /// The SoodaObject value to remove from this SoodaObjectCollection.
        /// </param>
        public virtual void Remove(SoodaObject value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by SoodaObjectCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(SoodaObjectCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public SoodaObject Current
            {
                get
                {
                    return (SoodaObject) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (SoodaObject) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this SoodaObjectCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual SoodaObjectCollection.Enumerator GetEnumerator()
        {
            return new SoodaObjectCollection.Enumerator(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public SoodaObjectCollection Clone()
        {
            return new SoodaObjectCollection(this);
        }
    }
}
