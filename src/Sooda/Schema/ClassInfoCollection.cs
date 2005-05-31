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

namespace Sooda.Schema {
    /// <summary>
    /// A collection of elements of type ClassInfo
    /// </summary>
    [Serializable]
    public class ClassInfoCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ClassInfoCollection class.
        /// </summary>
        public ClassInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the ClassInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new ClassInfoCollection.
        /// </param>
        public ClassInfoCollection(ClassInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the ClassInfoCollection class, containing elements
        /// copied from another instance of ClassInfoCollection
        /// </summary>
        /// <param name="items">
        /// The ClassInfoCollection whose elements are to be added to the new ClassInfoCollection.
        /// </param>
        public ClassInfoCollection(ClassInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ClassInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ClassInfoCollection.
        /// </param>
        public virtual void AddRange(ClassInfo[] items)
        {
            foreach (ClassInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another ClassInfoCollection to the end of this ClassInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The ClassInfoCollection whose elements are to be added to the end of this ClassInfoCollection.
        /// </param>
        public virtual void AddRange(ClassInfoCollection items)
        {
            foreach (ClassInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type ClassInfo to the end of this ClassInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The ClassInfo to be added to the end of this ClassInfoCollection.
        /// </param>
        public virtual void Add(ClassInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic ClassInfo value is in this ClassInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The ClassInfo value to locate in this ClassInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this ClassInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ClassInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this ClassInfoCollection
        /// </summary>
        /// <param name="value">
        /// The ClassInfo value to locate in the ClassInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ClassInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the ClassInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the ClassInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The ClassInfo to insert.
        /// </param>
        public virtual void Insert(int index, ClassInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the ClassInfo at the given index in this ClassInfoCollection.
        /// </summary>
        public virtual ClassInfo this[int index]
        {
            get
            {
                return (ClassInfo) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific ClassInfo from this ClassInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The ClassInfo value to remove from this ClassInfoCollection.
        /// </param>
        public virtual void Remove(ClassInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ClassInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(ClassInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public ClassInfo Current
            {
                get
                {
                    return (ClassInfo) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (ClassInfo) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this ClassInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual ClassInfoCollection.Enumerator GetEnumerator()
        {
            return new ClassInfoCollection.Enumerator(this);
        }
    }
}
