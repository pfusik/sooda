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

using Sooda.ObjectMapper;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type Object and values of type WeakSoodaObject
    /// </summary>
    public class ObjectToWeakSoodaObjectAssociation : System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ObjectToWeakSoodaObjectAssociation class
        /// </summary>
        public ObjectToWeakSoodaObjectAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the WeakSoodaObject associated with the given Object
        /// </summary>
        /// <param name="key">
        /// The Object whose value to get or set.
        /// </param>
        public virtual WeakSoodaObject this[Object key]
        {
            get
            {
                return (WeakSoodaObject)this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this ObjectToWeakSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The Object key of the element to add.
        /// </param>
        /// <param name="value">
        /// The WeakSoodaObject value of the element to add.
        /// </param>
        public virtual void Add(Object key, WeakSoodaObject value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this ObjectToWeakSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Object key to locate in this ObjectToWeakSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToWeakSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(Object key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this ObjectToWeakSoodaObjectAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The Object key to locate in this ObjectToWeakSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToWeakSoodaObjectAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(Object key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this ObjectToWeakSoodaObjectAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The WeakSoodaObject value to locate in this ObjectToWeakSoodaObjectAssociation.
        /// </param>
        /// <returns>
        /// true if this ObjectToWeakSoodaObjectAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(WeakSoodaObject value)
        {
            foreach (WeakSoodaObject item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this ObjectToWeakSoodaObjectAssociation.
        /// </summary>
        /// <param name="key">
        /// The Object key of the element to remove.
        /// </param>
        public virtual void Remove(Object key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this ObjectToWeakSoodaObjectAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this ObjectToWeakSoodaObjectAssociation.
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
