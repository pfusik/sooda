// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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

using System.Collections.Specialized;

namespace Sooda.Collections
{
    /// <summary>
    /// A dictionary with keys of type SoodaObject and values of type NameValueCollection
    /// </summary>
    public class SoodaObjectToNameValueCollectionAssociation : System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the SoodaObjectToNameValueCollectionAssociation class
        /// </summary>
        public SoodaObjectToNameValueCollectionAssociation()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the NameValueCollection associated with the given SoodaObject
        /// </summary>
        /// <param name="key">
        /// The SoodaObject whose value to get or set.
        /// </param>
        public virtual NameValueCollection this[SoodaObject key]
        {
            get
            {
                return (NameValueCollection)this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this SoodaObjectToNameValueCollectionAssociation.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key of the element to add.
        /// </param>
        /// <param name="value">
        /// The NameValueCollection value of the element to add.
        /// </param>
        public virtual void Add(SoodaObject key, NameValueCollection value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToNameValueCollectionAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key to locate in this SoodaObjectToNameValueCollectionAssociation.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToNameValueCollectionAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(SoodaObject key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToNameValueCollectionAssociation contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key to locate in this SoodaObjectToNameValueCollectionAssociation.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToNameValueCollectionAssociation contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(SoodaObject key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this SoodaObjectToNameValueCollectionAssociation contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The NameValueCollection value to locate in this SoodaObjectToNameValueCollectionAssociation.
        /// </param>
        /// <returns>
        /// true if this SoodaObjectToNameValueCollectionAssociation contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(NameValueCollection value)
        {
            foreach (NameValueCollection item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this SoodaObjectToNameValueCollectionAssociation.
        /// </summary>
        /// <param name="key">
        /// The SoodaObject key of the element to remove.
        /// </param>
        public virtual void Remove(SoodaObject key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this SoodaObjectToNameValueCollectionAssociation.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this SoodaObjectToNameValueCollectionAssociation.
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
