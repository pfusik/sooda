using System;

namespace Sooda.Schema
{
    /// <summary>
    /// A collection of elements of type TableInfo
    /// </summary>
    [Serializable]
    public class TableInfoCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the TableInfoCollection class.
        /// </summary>
        public TableInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the TableInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new TableInfoCollection.
        /// </param>
        public TableInfoCollection(TableInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the TableInfoCollection class, containing elements
        /// copied from another instance of TableInfoCollection
        /// </summary>
        /// <param name="items">
        /// The TableInfoCollection whose elements are to be added to the new TableInfoCollection.
        /// </param>
        public TableInfoCollection(TableInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this TableInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this TableInfoCollection.
        /// </param>
        public virtual void AddRange(TableInfo[] items)
        {
            foreach (TableInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another TableInfoCollection to the end of this TableInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The TableInfoCollection whose elements are to be added to the end of this TableInfoCollection.
        /// </param>
        public virtual void AddRange(TableInfoCollection items)
        {
            foreach (TableInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type TableInfo to the end of this TableInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The TableInfo to be added to the end of this TableInfoCollection.
        /// </param>
        public virtual void Add(TableInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic TableInfo value is in this TableInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The TableInfo value to locate in this TableInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this TableInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(TableInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this TableInfoCollection
        /// </summary>
        /// <param name="value">
        /// The TableInfo value to locate in the TableInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(TableInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the TableInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the TableInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The TableInfo to insert.
        /// </param>
        public virtual void Insert(int index, TableInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the TableInfo at the given index in this TableInfoCollection.
        /// </summary>
        public virtual TableInfo this[int index]
        {
            get
            {
                return (TableInfo) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific TableInfo from this TableInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The TableInfo value to remove from this TableInfoCollection.
        /// </param>
        public virtual void Remove(TableInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by TableInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(TableInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public TableInfo Current
            {
                get
                {
                    return (TableInfo) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (TableInfo) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this TableInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual TableInfoCollection.Enumerator GetEnumerator()
        {
            return new TableInfoCollection.Enumerator(this);
        }
    }
}
