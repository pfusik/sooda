using System;

using Sooda.ObjectMapper;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type String and values of type ObjectToSoodaObjectAssociation
    /// </summary>
    public class StringToObjectToSoodaObjectAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToObjectToSoodaObjectAssociation class
        /// </summary>
        public StringToObjectToSoodaObjectAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the ObjectToSoodaObjectAssociation associated with the given String
        /// </summary>
        /// <param name="key">
        /// The String whose value to get or set.
        /// </param>
        public virtual ObjectToSoodaObjectAssociation this[String key]
        {
            get
            {
                return (ObjectToSoodaObjectAssociation) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringToObjectToSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to add.
        /// </param>
        /// <param name="value">
        /// The ObjectToSoodaObjectAssociation value of the element to add.
        /// </param>
        public virtual void Add(String key, ObjectToSoodaObjectAssociation value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToObjectToSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToObjectToSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToObjectToSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToObjectToSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToObjectToSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToObjectToSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToObjectToSoodaObjectAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The ObjectToSoodaObjectAssociation value to locate in this StringToObjectToSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToObjectToSoodaObjectAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(ObjectToSoodaObjectAssociation value)
        {
            foreach (ObjectToSoodaObjectAssociation item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringToObjectToSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to remove.
        /// </param>
        public virtual void Remove(String key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToObjectToSoodaObjectAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToObjectToSoodaObjectAssociation.
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
