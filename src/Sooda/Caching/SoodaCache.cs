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
using System.IO;
using System.Collections;
using System.Threading;

using Sooda.Logging;
using Sooda.Schema;
using Sooda.QL;

namespace Sooda.Caching
{
    public class SoodaCache
    {
        private static ReaderWriterLock _rwlock = new ReaderWriterLock();
        private static Hashtable _objectCache = new Hashtable();

        // string -> (SoodaCachedCollectionKey->object)
        private static Hashtable _class2keys = new Hashtable();
        private static object _marker = new object();

        private static SoodaCachedCollectionHash _collectionCache = new SoodaCachedCollectionHash();
        private static Logger logger = LogManager.GetLogger("Sooda.Cache");

        public static TimeSpan ExpirationTimeout = TimeSpan.FromMinutes(1);
        public static bool Enabled = true;

        public static SoodaCacheEntry FindObjectData(string className, object primaryKeyValue)
        {
            if (!Enabled)
                return null;

            _rwlock.AcquireReaderLock(-1);
            try
            {
                Hashtable ht = (Hashtable)_objectCache[className];
                SoodaCacheEntry retVal = null;

                if (ht != null)
                {
                    retVal = (SoodaCacheEntry)ht[primaryKeyValue];
                    if (retVal != null)
                    {
                        if (retVal.Age > ExpirationTimeout)
                        {
                            ht.Remove(primaryKeyValue);
                            retVal = null;
                        }
                    }
                }

                //if (logger.IsTraceEnabled)
                //{
                //logger.Trace("SoodaCache.FindObjectData('{0}',{1}) {2}", className, primaryKeyValue, (retVal != null) ? "FOUND" : "NOT FOUND");
                //}
                return retVal;
            }
            finally
            {
                _rwlock.ReleaseReaderLock();
            }
        }

        public static void AddObject(string className, object primaryKeyValue, SoodaCacheEntry entry)
        {
            if (!Enabled)
                return;

            _rwlock.AcquireWriterLock(-1);
            try
            {
                Hashtable ht = (Hashtable)_objectCache[className];
                if (ht == null)
                {
                    ht = new Hashtable();
                    _objectCache[className] = ht;
                }

                if (logger.IsTraceEnabled)
                {
                    logger.Trace("Add {0}({1}): {2}", className, primaryKeyValue, entry.ToString());
                }

                ht[primaryKeyValue] = entry;
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }


        public static void InvalidateObject(string className, object primaryKeyValue)
        {
            if (!Enabled)
                return;

            if (logger.IsTraceEnabled)
            {
                logger.Trace("Invalidating object {0}[{1}]", className, primaryKeyValue);
            }

            _rwlock.AcquireWriterLock(-1);
            try
            {
                Hashtable ht = (Hashtable)_objectCache[className];
                if (ht != null)
                {
                    // no exception when the key doesn't exist
                    ht.Remove(primaryKeyValue);
                }
                ht = (Hashtable)_class2keys[className];
                if (ht != null)
                {
                    _class2keys.Remove(className);
                    foreach (SoodaCachedCollectionKey key in ht.Keys)
                    {
                        // Console.WriteLine("Invalidating '{0}'", key);
                        _collectionCache.Remove(key);
                    }
                }
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        public static void Clear()
        {
            _rwlock.AcquireWriterLock(-1);
            try
            {
                logger.Debug("Clear");
                _objectCache.Clear();
                _collectionCache.Clear();
                _class2keys.Clear();
            }
            finally
            {
                _rwlock.ReleaseWriterLock();
            }
        }

        public static SoodaCachedCollectionKey GetCollectionKey(ClassInfo ci, SoodaWhereClause wc)
        {
            logger.Debug("GetCollectionKey({0},{1})", ci.Name, wc.WhereExpression);
            if (wc == null)
                return new SoodaCachedCollectionKeyAllRecords(ci);

            if (wc.WhereExpression == null)
                return new SoodaCachedCollectionKeyAllRecords(ci);
#if A
            if (wc.WhereExpression is SoqlBooleanLiteralExpression)
            {
                SoqlBooleanLiteralExpression ble = (SoqlBooleanLiteralExpression)wc.WhereExpression;
                if (ble.Value == false)
                    return null;
                return new SoodaCachedCollectionKeyAllRecords(ci);
            }

			if (wc.WhereExpression is SoqlBooleanRelationalExpression)
			{
				SoqlBooleanRelationalExpression bre = (SoqlBooleanRelationalExpression)wc.WhereExpression;
				if (bre.op != SoqlRelationalOperator.Equal)
					return null;

				SoqlPathExpression pathExpression;
				object value;

                SoqlExpression lhs = SoqlExpression.Unwrap(bre.par1);
                SoqlExpression rhs = SoqlExpression.Unwrap(bre.par2);

				if (lhs is SoqlPathExpression)
				{
					pathExpression = (SoqlPathExpression)lhs;
				}
				else if (rhs is SoqlPathExpression)
				{
					pathExpression = (SoqlPathExpression)rhs;
				}
				else
					return null;

				if (pathExpression.Left != null)
				{
					// the expression is A.B - too long for us
					// we only support caching of simple field-based collections
					return null;
				}

				string fieldName = pathExpression.PropertyName;

				if (lhs is SoqlLiteralExpression)
				{
					value = ((SoqlLiteralExpression)lhs).LiteralValue;
				}
				else if (rhs is SoqlLiteralExpression)
				{
					value = ((SoqlLiteralExpression)rhs).LiteralValue;
				}
                else if (lhs is SoqlBooleanLiteralExpression)
                {
                    value = ((SoqlBooleanLiteralExpression)lhs).Value;
                }
                else if (rhs is SoqlBooleanLiteralExpression)
                {
                    value = ((SoqlBooleanLiteralExpression)rhs).Value;
                }
                else if (lhs is SoqlParameterLiteralExpression)
				{
					value = wc.Parameters[((SoqlParameterLiteralExpression)lhs).ParameterPosition];
				}
				else if (rhs is SoqlParameterLiteralExpression)
				{
					value = wc.Parameters[((SoqlParameterLiteralExpression)rhs).ParameterPosition];
				}
				else
					return null;

				SoodaCachedCollectionKey key = new SoodaCachedCollectionKeySimple(ci, fieldName, value);
				return key;
			}
#endif

            StringWriter sw = new StringWriter();
            SoqlPrettyPrinter pp = new SoqlPrettyPrinter(sw, wc.Parameters);
            pp.PrintExpression(wc.WhereExpression);
            string canonicalWhereClause = sw.ToString();
            SoodaCachedCollectionKey key = new SoodaCachedCollectionKeyComplex(ci, canonicalWhereClause);
            //Console.WriteLine("key: {0}", key);
            return key;
        }

        public static IEnumerable LoadCollection(SoodaCachedCollectionKey key)
        {
            if (key == null)
                return null;

            SoodaCachedCollection coll = _collectionCache[key];
            if (coll == null)
                return null;

            return coll.PrimaryKeys;
        }

        private static void RegisterDependentCollectionClass(SoodaCachedCollectionKey cacheKey, ClassInfo dependentClass)
        {
            Hashtable ht = (Hashtable)_class2keys[dependentClass.Name];
            if (ht == null)
            {
                ht = new Hashtable();
                _class2keys[dependentClass.Name] = ht;
            }

            // this is actually a set
            ht[cacheKey] = _marker;
        }

        public static void StoreCollection(SoodaCachedCollectionKey cacheKey, IList primaryKeys, ClassInfoCollection dependentClasses)
        {
            if (cacheKey != null)
            {
                logger.Debug("Storing collection: {0} {1} items", cacheKey, primaryKeys.Count);
                if (dependentClasses != null)
                {
                    logger.Debug("Dependent classes: {0}", dependentClasses.Count);
                }
                SoodaCachedCollection value = new SoodaCachedCollection(primaryKeys);
                _collectionCache[cacheKey] = value;

                RegisterDependentCollectionClass(cacheKey, cacheKey.ClassInfo);
                if (dependentClasses != null)
                {
                    for (int i = 0; i < dependentClasses.Count; ++i)
                    {
                        RegisterDependentCollectionClass(cacheKey, dependentClasses[i]);
                    }
                }
            }
        }

        public static void Dump(TextWriter output)
        {
            output.WriteLine("CACHE DUMP:");
            foreach (string className in _objectCache.Keys)
            {
                output.WriteLine(className);

                foreach (DictionaryEntry de in (Hashtable)_objectCache[className])
                {
                    SoodaCacheEntry entry = (SoodaCacheEntry)de.Value;

                    output.Write("{0,8} [", de.Key);
                    bool first = true;
                    for (int i = 0; i < entry.Data.Length; ++i)
                    {
                        object fd = entry.Data.GetBoxedFieldValue(i);
                        if (!first)
                            output.Write("|");
                        output.Write(fd);
                        first = false;
                    }
                    output.Write("]");
                    output.WriteLine();
                }
                output.WriteLine();
            }
            output.WriteLine();
        }

        public static IDisposable BeginCommit()
        {
            _rwlock.AcquireWriterLock(-1);
            return new EndCommitCaller();
        }

        public static void EndCommit()
        {
            _rwlock.ReleaseWriterLock();
        }

        class EndCommitCaller : IDisposable
        {
            #region IDisposable Members

            public void Dispose()
            {
                SoodaCache.EndCommit();
            }

            #endregion
        }
    }
}
