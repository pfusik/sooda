using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type String and values of type ISoodaObjectFactory
    /// </summary>
    public class StringToISoodaObjectFactoryAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToISoodaObjectFactoryAssociation class
        /// </summary>
        public StringToISoodaObjectFactoryAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the ISoodaObjectFactory associated with the given String
        /// </summary>
        /// <param name="key">
        /// The String whose value to get or set.
        /// </param>
        public virtual ISoodaObjectFactory this[String key]
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
        /// Adds an element with the specified key and value to this StringToISoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to add.
        /// </param>
        /// <param name="value">
        /// The ISoodaObjectFactory value of the element to add.
        /// </param>
        public virtual void Add(String key, ISoodaObjectFactory value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToISoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToISoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToISoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToISoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToISoodaObjectFactoryAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The ISoodaObjectFactory value to locate in this StringToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToISoodaObjectFactoryAssociation contains an element with the specified value;
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
        /// Removes the element with the specified key from this StringToISoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to remove.
        /// </param>
        public virtual void Remove(String key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToISoodaObjectFactoryAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToISoodaObjectFactoryAssociation.
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
