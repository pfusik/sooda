using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type Object and values of type SoodaObject
    /// </summary>
    public class ObjectToSoodaObjectAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ObjectToSoodaObjectAssociation class
        /// </summary>
        public ObjectToSoodaObjectAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the SoodaObject associated with the given Object
        /// </summary>
        /// <param name="key">
        /// The Object whose value to get or set.
        /// </param>
        public virtual SoodaObject this[Object key]
        {
            get
            {
                return (SoodaObject) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this ObjectToSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The Object key of the element to add.
        /// </param>
        /// <param name="value">
        /// The SoodaObject value of the element to add.
        /// </param>
        public virtual void Add(Object key, SoodaObject value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this ObjectToSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Object key to locate in this ObjectToSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(Object key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this ObjectToSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Object key to locate in this ObjectToSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(Object key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this ObjectToSoodaObjectAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The SoodaObject value to locate in this ObjectToSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToSoodaObjectAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(SoodaObject value)
        {
            foreach (SoodaObject item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this ObjectToSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The Object key of the element to remove.
        /// </param>
        public virtual void Remove(Object key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this ObjectToSoodaObjectAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this ObjectToSoodaObjectAssociation.
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
