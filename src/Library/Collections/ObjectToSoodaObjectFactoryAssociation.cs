using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type object and values of type ISoodaObjectFactory
    /// </summary>
    public class ObjectToSoodaObjectFactoryAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ObjectToSoodaObjectFactoryAssociation class
        /// </summary>
        public ObjectToSoodaObjectFactoryAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the ISoodaObjectFactory associated with the given object
        /// </summary>
        /// <param name="key">
        /// The object whose value to get or set.
        /// </param>
        public virtual ISoodaObjectFactory this[object key]
        {
            get
            {
                return (ISoodaObjectFactory) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this ObjectToSoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The object key of the element to add.
        /// </param>
        /// <param name="value">
        /// The ISoodaObjectFactory value of the element to add.
        /// </param>
        public virtual void Add(object key, ISoodaObjectFactory value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this ObjectToSoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The object key to locate in this ObjectToSoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToSoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(object key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this ObjectToSoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The object key to locate in this ObjectToSoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToSoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(object key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this ObjectToSoodaObjectFactoryAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The ISoodaObjectFactory value to locate in this ObjectToSoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToSoodaObjectFactoryAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(ISoodaObjectFactory value)
        {
            foreach (ISoodaObjectFactory item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this ObjectToSoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The object key of the element to remove.
        /// </param>
        public virtual void Remove(object key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this ObjectToSoodaObjectFactoryAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this ObjectToSoodaObjectFactoryAssociation.
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
