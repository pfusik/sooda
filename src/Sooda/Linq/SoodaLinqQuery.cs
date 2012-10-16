//
// Copyright (c) 2010-2012 Piotr Fusik <piotr@fusik.info>
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

#if DOTNET35

using System;

using Sooda;
using Sooda.QL;
using Sooda.Schema;

namespace Sooda.Linq
{
    class SoodaLinqQuery
    {
        public readonly SoodaTransaction Transaction;
        public readonly ClassInfo ClassInfo;
        public readonly SoodaSnapshotOptions Options;
        public SoqlBooleanExpression WhereClause = null;
        public SoodaOrderBy OrderByClause = null;
        public int TopCount = -1;

        public SoodaLinqQuery(SoodaTransaction transaction, ClassInfo classInfo, SoodaSnapshotOptions options)
        {
            this.Transaction = transaction;
            this.ClassInfo = classInfo;
            this.Options = options;
        }

        public SoodaLinqQuery Where(SoqlBooleanExpression additionalWhere)
        {
            if (this.TopCount >= 0)
                throw new NotSupportedException("Take().Where() not supported");
            return new SoodaLinqQuery(this.Transaction, this.ClassInfo, this.Options) {
                WhereClause = this.WhereClause == null ? additionalWhere : this.WhereClause.And(additionalWhere),
                OrderByClause = this.OrderByClause
            };
        }

        public SoodaLinqQuery OrderBy(string methodName, SoqlExpression expression)
        {
            if (this.TopCount >= 0)
                throw new NotSupportedException("Take().OrderBy() not supported");
            SoodaOrderBy orderBy;
            switch (methodName)
            {
            case "OrderBy":
                orderBy = new SoodaOrderBy(expression, SortOrder.Ascending, this.OrderByClause);
                break;
            case "OrderByDescending":
                orderBy = new SoodaOrderBy(expression, SortOrder.Descending, this.OrderByClause);
                break;
            case "ThenBy":
                orderBy = new SoodaOrderBy(this.OrderByClause, expression, SortOrder.Ascending);
                break;
            case "ThenByDescending":
                orderBy = new SoodaOrderBy(this.OrderByClause, expression, SortOrder.Descending);
                break;
            default:
                throw new ArgumentOutOfRangeException("methodName");
            }
            return new SoodaLinqQuery(this.Transaction, this.ClassInfo, this.Options) {
                WhereClause = this.WhereClause,
                OrderByClause = orderBy
            };
        }

        public SoodaLinqQuery Take(int count)
        {
            if (count < 0)
                count = 0;
            if (this.TopCount >= 0 && this.TopCount <= count)
                return this;
            return new SoodaLinqQuery(this.Transaction, this.ClassInfo, this.Options) {
                WhereClause = this.WhereClause,
                OrderByClause = this.OrderByClause,
                TopCount = count
            };
        }
    }
}

#endif
