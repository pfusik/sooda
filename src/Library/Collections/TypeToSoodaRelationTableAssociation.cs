using System;

using Sooda.ObjectMapper;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type Type and values of type SoodaRelationTable
    /// </summary>
    public class TypeToSoodaRelationTableAssociation: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the TypeToSoodaRelationTableAssociation class
        /// </summary>
        public TypeToSoodaRelationTableAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the SoodaRelationTable associated with the given Type
        /// </summary>
        /// <param name="key">
        /// The Type whose value to get or set.
        /// </param>
        public virtual SoodaRelationTable this[Type key]
        {
            get
            {
                return (SoodaRelationTable) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this TypeToSoodaRelationTableAssociation.
        /// </summary>
        /// <param name="key">
        /// The Type key of the element to add.
        /// </param>
        /// <param name="value">
        /// The SoodaRelationTable value of the element to add.
        /// </param>
        public virtual void Add(Type key, SoodaRelationTable value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this TypeToSoodaRelationTableAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Type key to locate in this TypeToSoodaRelationTableAssociation.
        /// </param>
        /// <returns>
        /// true if this TypeToSoodaRelationTableAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(Type key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this TypeToSoodaRelationTableAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Type key to locate in this TypeToSoodaRelationTableAssociation.
        /// </param>
        /// <returns>
        /// true if this TypeToSoodaRelationTableAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(Type key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this TypeToSoodaRelationTableAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The SoodaRelationTable value to locate in this TypeToSoodaRelationTableAssociation.
        /// </param>
        /// <returns>
        /// true if this TypeToSoodaRelationTableAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(SoodaRelationTable value)
        {
            foreach (SoodaRelationTable item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this TypeToSoodaRelationTableAssociation.
        /// </summary>
        /// <param name="key">
        /// The Type key of the element to remove.
        /// </param>
        public virtual void Remove(Type key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this TypeToSoodaRelationTableAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this TypeToSoodaRelationTableAssociation.
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
