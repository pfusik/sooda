using System;
using System.Text;

using Sooda.Schema;

namespace Sooda.Caching
{
    public interface ISoodaCachingPolicy
    {
        bool ShouldCacheObject(SoodaObject theObject);
        bool ShouldCacheCollection(ClassInfo classInfo, SoodaWhereClause whereClause, SoodaOrderBy orderBy, int topCount);
    }
}
