using System;
using System.Text;

using Sooda.Schema;

namespace Sooda.Caching
{
    public class SoodaNoCachingPolicy : ISoodaCachingPolicy
    {
        public bool ShouldCacheObject(SoodaObject theObject)
        {
            return false;
        }

        public bool ShouldCacheCollection(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount)
        {
            return false;
        }

        public bool ShouldCacheRelation(RelationInfo relation, ClassInfo classInfo)
        {
            return false;
        }
    }
}
