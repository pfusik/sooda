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
