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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Sooda;

namespace Sooda.Linq
{
    public class SoodaQueryable<T> : IOrderedQueryable<T>
    {
        public IQueryProvider Provider { get; private set; }

        public Expression Expression { get; private set; }

        public Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        public SoodaQueryable(SoodaTransaction transaction, Sooda.Schema.ClassInfo classInfo, SoodaSnapshotOptions options)
        {
            this.Provider = new SoodaQueryProvider(transaction, classInfo, options);
            this.Expression = Expression.Constant(this);
        }

        public SoodaQueryable(IQueryProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable ie = this.Provider.Execute<IEnumerable>(this.Expression);
            return ie.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            IEnumerable ie = this.Provider.Execute<IEnumerable>(this.Expression);
            return ie.Cast<T>().GetEnumerator();
        }
    }
}

#endif
