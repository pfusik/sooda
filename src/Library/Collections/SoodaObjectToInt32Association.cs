using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type SoodaObject and values of type Int32
    /// </summary>
    public class SoodaObjectToInt32Association: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the SoodaObjectToInt32Association class
        /// </summary>
        public SoodaObjectToInt32Association()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the Int32 associated with the given SoodaObject
        /// </summary>
        /// <param name="key">
        /// The SoodaObject whose value to get or set.
        /// </param>
        public virtual Int32 this[SoodaObject key]
        {
            get
            {
                return (Int32) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this SoodaObjectToInt32Association.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key of the element to add.
        /// </param>
        /// <param name="value">
        /// The Int32 value of the element to add.
        /// </param>
        public virtual void Add(SoodaObject key, Int32 value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToInt32Association contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key to locate in this SoodaObjectToInt32Association.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToInt32Association contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(SoodaObject key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToInt32Association contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key to locate in this SoodaObjectToInt32Association.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToInt32Association contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(SoodaObject key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToInt32Association contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The Int32 value to locate in this SoodaObjectToInt32Association.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToInt32Association contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(Int32 value)
        {
            foreach (Int32 item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this SoodaObjectToInt32Association.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key of the element to remove.
        /// </param>
        public virtual void Remove(SoodaObject key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this SoodaObjectToInt32Association.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this SoodaObjectToInt32Association.
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
