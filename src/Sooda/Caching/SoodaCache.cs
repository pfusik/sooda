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
using System.Xml;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;

using Sooda.Logging;
using Sooda.Schema;
using Sooda;
using Sooda.QL;

namespace Sooda.Caching 
{
	public class SoodaCache 
	{
		private static Hashtable _objectCache = new Hashtable();
		private static SoodaCachedCollectionHash _collectionCache = new SoodaCachedCollectionHash();
		private static Logger logger = LogManager.GetLogger("Sooda.Cache");

		public static TimeSpan ExpirationTimeout = TimeSpan.FromMinutes(1);
		public static bool Enabled = true;

		public static SoodaCacheEntry FindObjectData(string className, object primaryKeyValue) 
		{
			if (!Enabled)
				return null;

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

			if (logger.IsTraceEnabled)
			{
				logger.Trace("SoodaCache.FindObjectData('{0}',{1}) {2}", className, primaryKeyValue, (retVal != null) ? "FOUND" : "NOT FOUND");
			}
			return retVal;
		}

		public static void AddObject(string className, object primaryKeyValue, SoodaCacheEntry entry) 
		{
			if (!Enabled)
				return ;

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

		public static void InvalidateObject(string className, object primaryKeyValue) 
		{
			if (!Enabled)
				return ;
			if (logger.IsTraceEnabled)
			{
				logger.Trace("Invalidating object {0}[{1}]", className, primaryKeyValue);
			}

			Hashtable ht = (Hashtable)_objectCache[className];
			if (ht == null)
				return;

			// no exception when the key doesn't exist
			ht.Remove(primaryKeyValue);
		}

		public static void Clear() 
		{
			logger.Debug("Clear");
			_objectCache.Clear();
		}

		public static SoodaCachedCollectionKey GetCollectionKey(ClassInfo ci, SoodaWhereClause wc)
		{
			logger.Debug("GetCollectionKey({0},{1}", ci.Name, wc.WhereExpression);
			if (wc == null)
				return null;

			if (wc.WhereExpression is SoqlBooleanRelationalExpression)
			{
				SoqlBooleanRelationalExpression bre = (SoqlBooleanRelationalExpression)wc.WhereExpression;
				if (bre.op != SoqlRelationalOperator.Equal)
					return null;

				SoqlPathExpression pathExpression;
				object value;

				if (bre.par1 is SoqlPathExpression)
				{
					pathExpression = (SoqlPathExpression)bre.par1;
				}
				else if (bre.par2 is SoqlPathExpression)
				{
					pathExpression = (SoqlPathExpression)bre.par2;
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

				if (bre.par1 is SoqlLiteralExpression)
				{
					value = ((SoqlLiteralExpression)bre.par1).LiteralValue;
				}
				else if (bre.par2 is SoqlLiteralExpression)
				{
					value = ((SoqlLiteralExpression)bre.par2).LiteralValue;
				}
				else if (bre.par1 is SoqlParameterLiteralExpression)
				{
					value = wc.Parameters[((SoqlParameterLiteralExpression)bre.par1).ParameterPosition];
				}
				else if (bre.par2 is SoqlParameterLiteralExpression)
				{
					value = wc.Parameters[((SoqlParameterLiteralExpression)bre.par2).ParameterPosition];
				}
				else
					return null;

				SoodaCachedCollectionKey key = new SoodaCachedCollectionKey(ci, fieldName, value);
				if (logger.IsTraceEnabled)
				{
					logger.Trace("Key: {0}", key);
				}
				return key;
			}
			
			return null;
		}

		public static IEnumerable LoadCollection(SoodaCachedCollectionKey key, SoodaOrderBy orderBy, int topCount)
		{
			if (key == null)
				return null;

			SoodaCachedCollection coll = _collectionCache[key];
			if (coll == null)
				return null;

			if (orderBy != null)
			{
				// TODO - add sorted queries, too
				return null;
			}

			if (topCount != -1)
			{
				// TODO - add 'top' support
				return null;
			}

			return coll.PrimaryKeys;
		}

		public static void StoreCollection(SoodaCachedCollectionKey cacheKey, IList primaryKeys)
		{
			if (cacheKey != null)
			{
				logger.Debug("Storing collection: {0} {1} items", cacheKey, primaryKeys.Count);
				SoodaCachedCollection value = new SoodaCachedCollection(primaryKeys);

				_collectionCache[cacheKey] = value;
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
	}
}
