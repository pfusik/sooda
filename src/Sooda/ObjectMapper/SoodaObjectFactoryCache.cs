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

using System;
using Sooda;
using Sooda.Collections;

using Sooda.Logging;

namespace Sooda.ObjectMapper {
    public class SoodaObjectFactoryCache : ISoodaObjectFactoryCache {
        private StringToStringToISoodaObjectFactoryAssociation _classes = new StringToStringToISoodaObjectFactoryAssociation();
        private Logger logger = LogManager.GetLogger("Sooda.FactoryCache");

        private ObjectToSoodaObjectFactoryAssociation GetObjectFactoryDictionaryForClass(string className) {
            ObjectToSoodaObjectFactoryAssociation dict = _classes[className];
            if (dict == null) {
                dict = new ObjectToSoodaObjectFactoryAssociation();
                _classes[className] = dict;
            }
            return dict;
        }

        private void AddObjectWithKey(string className, object keyValue, ISoodaObjectFactory factory) {
            GetObjectFactoryDictionaryForClass(className).Add(keyValue, factory);
        }

        private void UnregisterObjectWithKey(string className, object keyValue) {
            GetObjectFactoryDictionaryForClass(className).Remove(keyValue);
        }

        internal bool ExistsObjectWithKey(string className, object keyValue) {
            return GetObjectFactoryDictionaryForClass(className).Contains(keyValue);
        }

        private ISoodaObjectFactory FindObjectWithKey(string className, object keyValue) {
            return GetObjectFactoryDictionaryForClass(className)[keyValue];
        }

        public ISoodaObjectFactory FindObjectFactory(string className, object primaryKeyValue) {
            ISoodaObjectFactory fact = FindObjectWithKey(className, primaryKeyValue);
            if (logger.IsDebugEnabled) {
                if (fact == null) {
                    logger.Debug("{0}[{1}] not found in the factory cache", className, primaryKeyValue);
                } else {
                    logger.Debug("{0}[{1}] found in factory cache as {2}", className, primaryKeyValue, fact.GetClassInfo().Name);
                }
            }
            return fact;
        }

        public void SetObjectFactory(string className, object primaryKeyValue, ISoodaObjectFactory factory) {
            if (logger.IsDebugEnabled) {
                logger.Debug("Adding {0}[{1}]={2} to the factory cache", className, primaryKeyValue, factory.GetClassInfo().Name);
            }
            AddObjectWithKey(className, primaryKeyValue, factory);
        }

        public void Invalidate() {
            logger.Debug("Invalidating factory cache");
            _classes.Clear();
        }
    }
}
