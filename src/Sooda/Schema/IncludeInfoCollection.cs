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
    /// A collection of elements of type IncludeInfo
    /// </summary>
    [Serializable]
    public class IncludeInfoCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the IncludeInfoCollection class.
        /// </summary>
        public IncludeInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the IncludeInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new IncludeInfoCollection.
        /// </param>
        public IncludeInfoCollection(IncludeInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the IncludeInfoCollection class, containing elements
        /// copied from another instance of IncludeInfoCollection
        /// </summary>
        /// <param name="items">
        /// The IncludeInfoCollection whose elements are to be added to the new IncludeInfoCollection.
        /// </param>
        public IncludeInfoCollection(IncludeInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this IncludeInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this IncludeInfoCollection.
        /// </param>
        public virtual void AddRange(IncludeInfo[] items)
        {
            foreach (IncludeInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another IncludeInfoCollection to the end of this IncludeInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The IncludeInfoCollection whose elements are to be added to the end of this IncludeInfoCollection.
        /// </param>
        public virtual void AddRange(IncludeInfoCollection items)
        {
            foreach (IncludeInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type IncludeInfo to the end of this IncludeInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IncludeInfo to be added to the end of this IncludeInfoCollection.
        /// </param>
        public virtual void Add(IncludeInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic IncludeInfo value is in this IncludeInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IncludeInfo value to locate in this IncludeInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this IncludeInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(IncludeInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this IncludeInfoCollection
        /// </summary>
        /// <param name="value">
        /// The IncludeInfo value to locate in the IncludeInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(IncludeInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the IncludeInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the IncludeInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The IncludeInfo to insert.
        /// </param>
        public virtual void Insert(int index, IncludeInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the IncludeInfo at the given index in this IncludeInfoCollection.
        /// </summary>
        public virtual IncludeInfo this[int index]
        {
            get
            {
                return (IncludeInfo) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific IncludeInfo from this IncludeInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The IncludeInfo value to remove from this IncludeInfoCollection.
        /// </param>
        public virtual void Remove(IncludeInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by IncludeInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(IncludeInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public IncludeInfo Current
            {
                get
                {
                    return (IncludeInfo) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (IncludeInfo) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this IncludeInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual IncludeInfoCollection.Enumerator GetEnumerator()
        {
            return new IncludeInfoCollection.Enumerator(this);
        }
    }
}
