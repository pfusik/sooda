// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;


namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type String and values of type ObjectToWeakSoodaObjectAssociation
    /// </summary>
    public class StringToObjectToWeakSoodaObjectAssociation : System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToObjectToWeakSoodaObjectAssociation class
        /// </summary>
        public StringToObjectToWeakSoodaObjectAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the ObjectToWeakSoodaObjectAssociation associated with the given String
        /// </summary>
        /// <param name="key">
        /// The String whose value to get or set.
        /// </param>
        public virtual ObjectToWeakSoodaObjectAssociation this[String key]
        {
            get
            {
                return (ObjectToWeakSoodaObjectAssociation)this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringToObjectToWeakSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to add.
        /// </param>
        /// <param name="value">
        /// The ObjectToWeakSoodaObjectAssociation value of the element to add.
        /// </param>
        public virtual void Add(String key, ObjectToWeakSoodaObjectAssociation value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToObjectToWeakSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToObjectToWeakSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToObjectToWeakSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToObjectToWeakSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToObjectToWeakSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToObjectToWeakSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToObjectToWeakSoodaObjectAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The ObjectToWeakSoodaObjectAssociation value to locate in this StringToObjectToWeakSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this StringToObjectToWeakSoodaObjectAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(ObjectToWeakSoodaObjectAssociation value)
        {
            foreach (ObjectToWeakSoodaObjectAssociation item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringToObjectToWeakSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to remove.
        /// </param>
        public virtual void Remove(String key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToObjectToWeakSoodaObjectAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToObjectToWeakSoodaObjectAssociation.
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
