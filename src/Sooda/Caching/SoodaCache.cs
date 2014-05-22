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
using System.IO;
using System.Collections;
using System.Threading;

using Sooda.Logging;
using Sooda.Schema;
using Sooda.QL;
using System.Globalization;

namespace Sooda.Caching
{
    public class SoodaCache
    {
        private static ISoodaCachingPolicy _defaultCachingPolicy;
        private static ISoodaCache _defaultCache = new SoodaInProcessCache();
        private static Logger logger = LogManager.GetLogger("Sooda.Cache");

        static SoodaCache()
        {
            string cachingPolicy = SoodaConfig.GetString("sooda.cachingPolicy", "none");
            string cacheType = SoodaConfig.GetString("sooda.cache.type", "inprocess");

            switch (cachingPolicy)
            {
                case "none": _defaultCachingPolicy = new SoodaNoCachingPolicy(); break;
                case "all": _defaultCachingPolicy = new SoodaCacheAllPolicy(); break;
                case "small": _defaultCachingPolicy = new SoodaCacheSmallPolicy(); break;
                case "smallmedium": _defaultCachingPolicy = new SoodaCacheSmallAndMediumPolicy(); break;
                default:
                _defaultCachingPolicy = Activator.CreateInstance(Type.GetType(cachingPolicy, true)) as ISoodaCachingPolicy;
                break;
            }

            switch (cacheType)
            {
                case "none": _defaultCache = new SoodaNoCache(); break;
                case "inprocess": _defaultCache = new SoodaInProcessCache(); break;
                default:
                _defaultCache = Activator.CreateInstance(Type.GetType(cacheType, true)) as ISoodaCache;
                break;
            }

            if (_defaultCachingPolicy == null)
                _defaultCachingPolicy = new SoodaNoCachingPolicy();
            if (_defaultCache == null)
                _defaultCache = new SoodaInProcessCache();

            ISoodaCachingPolicyFixedTimeout sft = _defaultCachingPolicy as ISoodaCachingPolicyFixedTimeout;
            if (sft != null)
            {
                sft.SlidingExpiration = Convert.ToBoolean(SoodaConfig.GetString("sooda.cachingPolicy.slidingExpiration", "true"), CultureInfo.InvariantCulture);
                sft.ExpirationTimeout = TimeSpan.FromSeconds(
                    Convert.ToInt32(SoodaConfig.GetString("sooda.cachingPolicy.expirationTimeout", "120"), CultureInfo.InvariantCulture));
            }

            logger.Debug("Default cache: {0} Policy: {1}", _defaultCache.GetType().Name, _defaultCachingPolicy.GetType().Name);
        }

        public static ISoodaCachingPolicy DefaultCachingPolicy
        {
            get { return _defaultCachingPolicy; }
            set { _defaultCachingPolicy = value; }
        }

        public static ISoodaCache DefaultCache
        {
            get { return _defaultCache; }
            set { _defaultCache = value; }
        }

        public static string GetCollectionKey(ClassInfo classInfo, SoodaWhereClause wc)
        {
            return GetCollectionKey(classInfo.Name, wc);
        }

        public static string GetCollectionKey(string className, SoodaWhereClause wc)
        {
            if (wc == null || wc.WhereExpression == null)
                return className + " where True";

            StringWriter sw = new StringWriter();
            SoqlPrettyPrinter pp = new SoqlPrettyPrinter(sw, wc.Parameters);
            pp.PrintExpression(wc.WhereExpression);
            string canonicalWhereClause = sw.ToString();
            return className + " where " + canonicalWhereClause;
        }
    }
}
