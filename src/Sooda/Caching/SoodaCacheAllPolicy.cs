using System;
using System.Text;

using Sooda.Schema;

namespace Sooda.Caching
{
    public class SoodaCacheAllPolicy : ISoodaCachingPolicy
    {
        public bool ShouldCacheObject(SoodaObject theObject)
        {
            return true;
        }

        public bool ShouldCacheCollection(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount)
        {
            return true;
        }
    }
}
