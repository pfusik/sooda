using System;

namespace Sooda.CodeGen
{
    /// <summary>
    /// A collection of elements of type ExternalProjectInfo
    /// </summary>
    public class ExternalProjectInfoCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ExternalProjectInfoCollection class.
        /// </summary>
        public ExternalProjectInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the ExternalProjectInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new ExternalProjectInfoCollection.
        /// </param>
        public ExternalProjectInfoCollection(ExternalProjectInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the ExternalProjectInfoCollection class, containing elements
        /// copied from another instance of ExternalProjectInfoCollection
        /// </summary>
        /// <param name="items">
        /// The ExternalProjectInfoCollection whose elements are to be added to the new ExternalProjectInfoCollection.
        /// </param>
        public ExternalProjectInfoCollection(ExternalProjectInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ExternalProjectInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ExternalProjectInfoCollection.
        /// </param>
        public virtual void AddRange(ExternalProjectInfo[] items)
        {
            foreach (ExternalProjectInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another ExternalProjectInfoCollection to the end of this ExternalProjectInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The ExternalProjectInfoCollection whose elements are to be added to the end of this ExternalProjectInfoCollection.
        /// </param>
        public virtual void AddRange(ExternalProjectInfoCollection items)
        {
            foreach (ExternalProjectInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type ExternalProjectInfo to the end of this ExternalProjectInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The ExternalProjectInfo to be added to the end of this ExternalProjectInfoCollection.
        /// </param>
        public virtual void Add(ExternalProjectInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic ExternalProjectInfo value is in this ExternalProjectInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The ExternalProjectInfo value to locate in this ExternalProjectInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this ExternalProjectInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ExternalProjectInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this ExternalProjectInfoCollection
        /// </summary>
        /// <param name="value">
        /// The ExternalProjectInfo value to locate in the ExternalProjectInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ExternalProjectInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the ExternalProjectInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the ExternalProjectInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The ExternalProjectInfo to insert.
        /// </param>
        public virtual void Insert(int index, ExternalProjectInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the ExternalProjectInfo at the given index in this ExternalProjectInfoCollection.
        /// </summary>
        public virtual ExternalProjectInfo this[int index]
        {
            get
            {
                return (ExternalProjectInfo)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific ExternalProjectInfo from this ExternalProjectInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The ExternalProjectInfo value to remove from this ExternalProjectInfoCollection.
        /// </param>
        public virtual void Remove(ExternalProjectInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ExternalProjectInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(ExternalProjectInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public ExternalProjectInfo Current
            {
                get
                {
                    return (ExternalProjectInfo)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (ExternalProjectInfo)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this ExternalProjectInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual ExternalProjectInfoCollection.Enumerator GetEnumerator()
        {
            return new ExternalProjectInfoCollection.Enumerator(this);
        }
    }
}
