using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type string and values of type ObjectToSoodaObjectFactoryAssociation
    /// </summary>
    public class StringToStringToISoodaObjectFactoryAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToStringToISoodaObjectFactoryAssociation class
        /// </summary>
        public StringToStringToISoodaObjectFactoryAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the ObjectToSoodaObjectFactoryAssociation associated with the given string
        /// </summary>
        /// <param name="key">
        /// The string whose value to get or set.
        /// </param>
        public virtual ObjectToSoodaObjectFactoryAssociation this[string key]
        {
            get
            {
                return (ObjectToSoodaObjectFactoryAssociation) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringToStringToISoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The string key of the element to add.
        /// </param>
        /// <param name="value">
        /// The ObjectToSoodaObjectFactoryAssociation value of the element to add.
        /// </param>
        public virtual void Add(string key, ObjectToSoodaObjectFactoryAssociation value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToStringToISoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The string key to locate in this StringToStringToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToStringToISoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToStringToISoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The string key to locate in this StringToStringToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToStringToISoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(string key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToStringToISoodaObjectFactoryAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The ObjectToSoodaObjectFactoryAssociation value to locate in this StringToStringToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToStringToISoodaObjectFactoryAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(ObjectToSoodaObjectFactoryAssociation value)
        {
            foreach (ObjectToSoodaObjectFactoryAssociation item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringToStringToISoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The string key of the element to remove.
        /// </param>
        public virtual void Remove(string key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToStringToISoodaObjectFactoryAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToStringToISoodaObjectFactoryAssociation.
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
