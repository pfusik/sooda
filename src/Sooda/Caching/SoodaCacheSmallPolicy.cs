using System;
using System.Text;

using Sooda.Schema;

namespace Sooda.Caching
{
    public class SoodaCacheSmallPolicy : ISoodaCachingPolicy
    {
        public bool ShouldCacheObject(SoodaObject theObject)
        {
            if (theObject.GetClassInfo().Cardinality == ClassCardinality.Small)
                return true;
            else
                return false;
        }

        public bool ShouldCacheCollection(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount)
        {
            if (classInfo.Cardinality == ClassCardinality.Small)
                return true;
            else
                return false;
        }

        public bool ShouldCacheRelation(RelationInfo relation, ClassInfo classInfo)
        {
            if (classInfo.Cardinality == ClassCardinality.Small)
                return true;
            else
                return false;
        }
    }
}
