using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type SoodaObject and values of type Object
    /// </summary>
    public class SoodaObjectToObjectAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the SoodaObjectToObjectAssociation class
        /// </summary>
        public SoodaObjectToObjectAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the Object associated with the given SoodaObject
        /// </summary>
        /// <param name="key">
        /// The SoodaObject whose value to get or set.
        /// </param>
        public virtual Object this[SoodaObject key]
        {
            get
            {
                return (Object) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this SoodaObjectToObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key of the element to add.
        /// </param>
        /// <param name="value">
        /// The Object value of the element to add.
        /// </param>
        public virtual void Add(SoodaObject key, Object value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key to locate in this SoodaObjectToObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(SoodaObject key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key to locate in this SoodaObjectToObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(SoodaObject key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToObjectAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The Object value to locate in this SoodaObjectToObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToObjectAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(Object value)
        {
            foreach (Object item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this SoodaObjectToObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key of the element to remove.
        /// </param>
        public virtual void Remove(SoodaObject key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this SoodaObjectToObjectAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this SoodaObjectToObjectAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Values
        {
            get
            {
                return this.Dictionary.Values;
            }
        }
    }
}
