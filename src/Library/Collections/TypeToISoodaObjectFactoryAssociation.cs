using System;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type Type and values of type ISoodaObjectFactory
    /// </summary>
    public class TypeToISoodaObjectFactoryAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the TypeToISoodaObjectFactoryAssociation class
        /// </summary>
        public TypeToISoodaObjectFactoryAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the ISoodaObjectFactory associated with the given Type
        /// </summary>
        /// <param name="key">
        /// The Type whose value to get or set.
        /// </param>
        public virtual ISoodaObjectFactory this[Type key]
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
        /// Adds an element with the specified key and value to this TypeToISoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The Type key of the element to add.
        /// </param>
        /// <param name="value">
        /// The ISoodaObjectFactory value of the element to add.
        /// </param>
        public virtual void Add(Type key, ISoodaObjectFactory value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this TypeToISoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Type key to locate in this TypeToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this TypeToISoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(Type key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this TypeToISoodaObjectFactoryAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Type key to locate in this TypeToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this TypeToISoodaObjectFactoryAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(Type key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this TypeToISoodaObjectFactoryAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The ISoodaObjectFactory value to locate in this TypeToISoodaObjectFactoryAssociation.
        /// </param>
        /// <returns>
        /// true if this TypeToISoodaObjectFactoryAssociation contains an element with the specified value;
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
        /// Removes the element with the specified key from this TypeToISoodaObjectFactoryAssociation.
        /// </summary>
        /// <param name="key">
        /// The Type key of the element to remove.
        /// </param>
        public virtual void Remove(Type key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this TypeToISoodaObjectFactoryAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this TypeToISoodaObjectFactoryAssociation.
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
