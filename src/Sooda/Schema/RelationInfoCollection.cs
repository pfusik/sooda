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

namespace Sooda.Schema
{
    /// <summary>
    /// A collection of elements of type RelationInfo
    /// </summary>
    [Serializable]
    public class RelationInfoCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the RelationInfoCollection class.
        /// </summary>
        public RelationInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the RelationInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new RelationInfoCollection.
        /// </param>
        public RelationInfoCollection(RelationInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the RelationInfoCollection class, containing elements
        /// copied from another instance of RelationInfoCollection
        /// </summary>
        /// <param name="items">
        /// The RelationInfoCollection whose elements are to be added to the new RelationInfoCollection.
        /// </param>
        public RelationInfoCollection(RelationInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this RelationInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this RelationInfoCollection.
        /// </param>
        public virtual void AddRange(RelationInfo[] items)
        {
            foreach (RelationInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another RelationInfoCollection to the end of this RelationInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The RelationInfoCollection whose elements are to be added to the end of this RelationInfoCollection.
        /// </param>
        public virtual void AddRange(RelationInfoCollection items)
        {
            foreach (RelationInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type RelationInfo to the end of this RelationInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The RelationInfo to be added to the end of this RelationInfoCollection.
        /// </param>
        public virtual void Add(RelationInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic RelationInfo value is in this RelationInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The RelationInfo value to locate in this RelationInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this RelationInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(RelationInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this RelationInfoCollection
        /// </summary>
        /// <param name="value">
        /// The RelationInfo value to locate in the RelationInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(RelationInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the RelationInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the RelationInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The RelationInfo to insert.
        /// </param>
        public virtual void Insert(int index, RelationInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the RelationInfo at the given index in this RelationInfoCollection.
        /// </summary>
        public virtual RelationInfo this[int index]
        {
            get
            {
                return (RelationInfo)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific RelationInfo from this RelationInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The RelationInfo value to remove from this RelationInfoCollection.
        /// </param>
        public virtual void Remove(RelationInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by RelationInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(RelationInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public RelationInfo Current
            {
                get
                {
                    return (RelationInfo)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (RelationInfo)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this RelationInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual RelationInfoCollection.Enumerator GetEnumerator()
        {
            return new RelationInfoCollection.Enumerator(this);
        }

        public void SortByName() 
        {
            System.Collections.IComparer sorter = new NameSortHelper();
            InnerList.Sort(sorter);
        }

        private class NameSortHelper : System.Collections.IComparer 
        {
            public int Compare(object x, object y) 
            {
                  RelationInfo ci1 = (RelationInfo) x;
                  RelationInfo ci2 = (RelationInfo) y;
                  return ci1.Name.CompareTo(ci2.Name);
            }
        }
    }
}
