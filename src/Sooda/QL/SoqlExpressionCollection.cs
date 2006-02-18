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


namespace Sooda.QL
{
    /// <summary>
    /// A collection of elements of type SoqlExpression
    /// </summary>
    public class SoqlExpressionCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the SoqlExpressionCollection class.
        /// </summary>
        public SoqlExpressionCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the SoqlExpressionCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new SoqlExpressionCollection.
        /// </param>
        public SoqlExpressionCollection(SoqlExpression[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the SoqlExpressionCollection class, containing elements
        /// copied from another instance of SoqlExpressionCollection
        /// </summary>
        /// <param name="items">
        /// The SoqlExpressionCollection whose elements are to be added to the new SoqlExpressionCollection.
        /// </param>
        public SoqlExpressionCollection(SoqlExpressionCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this SoqlExpressionCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this SoqlExpressionCollection.
        /// </param>
        public virtual void AddRange(SoqlExpression[] items)
        {
            foreach (SoqlExpression item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another SoqlExpressionCollection to the end of this SoqlExpressionCollection.
        /// </summary>
        /// <param name="items">
        /// The SoqlExpressionCollection whose elements are to be added to the end of this SoqlExpressionCollection.
        /// </param>
        public virtual void AddRange(SoqlExpressionCollection items)
        {
            foreach (SoqlExpression item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type SoqlExpression to the end of this SoqlExpressionCollection.
        /// </summary>
        /// <param name="value">
        /// The SoqlExpression to be added to the end of this SoqlExpressionCollection.
        /// </param>
        public virtual void Add(SoqlExpression value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic SoqlExpression value is in this SoqlExpressionCollection.
        /// </summary>
        /// <param name="value">
        /// The SoqlExpression value to locate in this SoqlExpressionCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this SoqlExpressionCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(SoqlExpression value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this SoqlExpressionCollection
        /// </summary>
        /// <param name="value">
        /// The SoqlExpression value to locate in the SoqlExpressionCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(SoqlExpression value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the SoqlExpressionCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the SoqlExpression is to be inserted.
        /// </param>
        /// <param name="value">
        /// The SoqlExpression to insert.
        /// </param>
        public virtual void Insert(int index, SoqlExpression value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the SoqlExpression at the given index in this SoqlExpressionCollection.
        /// </summary>
        public virtual SoqlExpression this[int index]
        {
            get
            {
                return (SoqlExpression)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific SoqlExpression from this SoqlExpressionCollection.
        /// </summary>
        /// <param name="value">
        /// The SoqlExpression value to remove from this SoqlExpressionCollection.
        /// </param>
        public virtual void Remove(SoqlExpression value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by SoqlExpressionCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(SoqlExpressionCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public SoqlExpression Current
            {
                get
                {
                    return (SoqlExpression)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (SoqlExpression)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this SoqlExpressionCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual SoqlExpressionCollection.Enumerator GetEnumerator()
        {
            return new SoqlExpressionCollection.Enumerator(this);
        }
    }
}
