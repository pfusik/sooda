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
using Sooda.ObjectMapper;

namespace Sooda.Collections
{
    /// <summary>
    /// A collection of elements of type WeakSoodaObject
    /// </summary>
    public class WeakSoodaObjectCollection : System.Collections.CollectionBase, ICloneable
    {
        /// <summary>
        /// Initializes a new empty instance of the WeakSoodaObjectCollection class.
        /// </summary>
        public WeakSoodaObjectCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the WeakSoodaObjectCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new WeakSoodaObjectCollection.
        /// </param>
        public WeakSoodaObjectCollection(WeakSoodaObject[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the WeakSoodaObjectCollection class, containing elements
        /// copied from another instance of WeakSoodaObjectCollection
        /// </summary>
        /// <param name="items">
        /// The WeakSoodaObjectCollection whose elements are to be added to the new WeakSoodaObjectCollection.
        /// </param>
        public WeakSoodaObjectCollection(WeakSoodaObjectCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this WeakSoodaObjectCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this WeakSoodaObjectCollection.
        /// </param>
        public virtual void AddRange(WeakSoodaObject[] items)
        {
            foreach (WeakSoodaObject item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another WeakSoodaObjectCollection to the end of this WeakSoodaObjectCollection.
        /// </summary>
        /// <param name="items">
        /// The WeakSoodaObjectCollection whose elements are to be added to the end of this WeakSoodaObjectCollection.
        /// </param>
        public virtual void AddRange(WeakSoodaObjectCollection items)
        {
            foreach (WeakSoodaObject item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type WeakSoodaObject to the end of this WeakSoodaObjectCollection.
        /// </summary>
        /// <param name="value">
        /// The WeakSoodaObject to be added to the end of this WeakSoodaObjectCollection.
        /// </param>
        public virtual int Add(WeakSoodaObject value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic WeakSoodaObject value is in this WeakSoodaObjectCollection.
        /// </summary>
        /// <param name="value">
        /// The WeakSoodaObject value to locate in this WeakSoodaObjectCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this WeakSoodaObjectCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(WeakSoodaObject value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this WeakSoodaObjectCollection
        /// </summary>
        /// <param name="value">
        /// The WeakSoodaObject value to locate in the WeakSoodaObjectCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(WeakSoodaObject value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the WeakSoodaObjectCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the WeakSoodaObject is to be inserted.
        /// </param>
        /// <param name="value">
        /// The WeakSoodaObject to insert.
        /// </param>
        public virtual void Insert(int index, WeakSoodaObject value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the WeakSoodaObject at the given index in this WeakSoodaObjectCollection.
        /// </summary>
        public virtual WeakSoodaObject this[int index]
        {
            get
            {
                return (WeakSoodaObject)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific WeakSoodaObject from this WeakSoodaObjectCollection.
        /// </summary>
        /// <param name="value">
        /// The WeakSoodaObject value to remove from this WeakSoodaObjectCollection.
        /// </param>
        public virtual void Remove(WeakSoodaObject value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by WeakSoodaObjectCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(WeakSoodaObjectCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public WeakSoodaObject Current
            {
                get
                {
                    return (WeakSoodaObject)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (WeakSoodaObject)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this WeakSoodaObjectCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual WeakSoodaObjectCollection.Enumerator GetEnumerator()
        {
            return new WeakSoodaObjectCollection.Enumerator(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public WeakSoodaObjectCollection Clone()
        {
            return new WeakSoodaObjectCollection(this);
        }
    }
}
