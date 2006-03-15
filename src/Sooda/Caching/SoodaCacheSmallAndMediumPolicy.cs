using System;
using System.Text;

using Sooda.Schema;

namespace Sooda.Caching
{
    public class SoodaCacheSmallAndMediumPolicy : ISoodaCachingPolicy
    {
        public bool ShouldCacheObject(SoodaObject theObject)
        {
            if (theObject.GetClassInfo().Cardinality == ClassCardinality.Small
                || theObject.GetClassInfo().Cardinality == ClassCardinality.Medium)
                return true;
            else
                return false;
        }

        public bool ShouldCacheCollection(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount)
        {
            if (classInfo.Cardinality == ClassCardinality.Small || classInfo.Cardinality == ClassCardinality.Medium)
                return true;
            else
                return false;
        }
    }
}
