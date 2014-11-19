//
// Copyright (c) 2014 Piotr Fusik <piotr@fusik.info>
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

using Sooda.QL;

namespace Sooda.Linq
{
    interface ISoodaQuerySource
    {
        SoodaTransaction Transaction { get; }
        Sooda.Schema.ClassInfo ClassInfo { get; }
        SoodaSnapshotOptions Options { get; }
        SoqlBooleanExpression Where { get; }
    }

    public class SoodaQuerySource<T> : SoodaQueryable<T>, ISoodaQuerySource
    {
        readonly SoodaTransaction _transaction;
        readonly Sooda.Schema.ClassInfo _classInfo;
        readonly SoodaSnapshotOptions _options;
        readonly SoqlBooleanExpression _where;

        SoodaTransaction ISoodaQuerySource.Transaction
        {
            get
            {
                return _transaction;
            }
        }

        Sooda.Schema.ClassInfo ISoodaQuerySource.ClassInfo
        {
            get
            {
                return _classInfo;
            }
        }

        SoodaSnapshotOptions ISoodaQuerySource.Options
        {
            get
            {
                return _options;
            }
        }

        SoqlBooleanExpression ISoodaQuerySource.Where
        {
            get
            {
                return _where;
            }
        }

        public SoodaQuerySource(SoodaTransaction transaction, Sooda.Schema.ClassInfo classInfo, SoodaSnapshotOptions options)
        {
            _transaction = transaction;
            _classInfo = classInfo;
            _options = options;
            _where = null;
        }

        public SoodaQuerySource(SoodaTransaction transaction, Sooda.Schema.ClassInfo classInfo, SoqlBooleanExpression where)
        {
            _transaction = transaction;
            _classInfo = classInfo;
            _options = SoodaSnapshotOptions.Default;
            _where = where;
        }
    }
}

#endif
