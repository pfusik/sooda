using System;

using Sooda.ObjectMapper;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type String and values of type SoodaObjectCollection
    /// </summary>
    public class StringToSoodaObjectCollectionAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToSoodaObjectCollectionAssociation class
        /// </summary>
        public StringToSoodaObjectCollectionAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the SoodaObjectCollection associated with the given String
        /// </summary>
        /// <param name="key">
        /// The String whose value to get or set.
        /// </param>
        public virtual SoodaObjectCollection this[String key]
        {
            get
            {
                return (SoodaObjectCollection) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringToSoodaObjectCollectionAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to add.
        /// </param>
        /// <param name="value">
        /// The SoodaObjectCollection value of the element to add.
        /// </param>
        public virtual void Add(String key, SoodaObjectCollection value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToSoodaObjectCollectionAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToSoodaObjectCollectionAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToSoodaObjectCollectionAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToSoodaObjectCollectionAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToSoodaObjectCollectionAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToSoodaObjectCollectionAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToSoodaObjectCollectionAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The SoodaObjectCollection value to locate in this StringToSoodaObjectCollectionAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToSoodaObjectCollectionAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(SoodaObjectCollection value)
        {
            foreach (SoodaObjectCollection item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringToSoodaObjectCollectionAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to remove.
        /// </param>
        public virtual void Remove(String key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToSoodaObjectCollectionAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToSoodaObjectCollectionAssociation.
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
