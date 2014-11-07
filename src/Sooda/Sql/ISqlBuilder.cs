//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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

using System.Data;

using Sooda.Schema;

namespace Sooda.Sql
{
    public interface ISqlBuilder
    {
        string GetSQLDataType(Sooda.Schema.FieldInfo fi);
        string GetSQLOrderBy(Sooda.Schema.FieldInfo fi, bool start);
        string GetDDLCommandTerminator();
        SqlOuterJoinSyntax OuterJoinSyntax { get; }
        SqlTopSupportMode TopSupport { get; }
        int MaxIdentifierLength { get; }
        bool UseSafeLiterals { get; set; }
        string StringConcatenationOperator { get; }

        void BuildCommandWithParameters(IDbCommand command, bool append, string query, object[] par, bool isRaw);

        void GenerateCreateTable(TextWriter tw, TableInfo tableInfo, string additionalSettings, string terminator);
        void GeneratePrimaryKey(TextWriter tw, TableInfo tableInfo, string additionalSettings, string terminator);
        void GenerateForeignKeys(TextWriter tw, TableInfo tableInfo, string terminator);
        void GenerateIndex(TextWriter tw, FieldInfo fieldInfo, string additionalSettings, string terminator);
        void GenerateIndices(TextWriter tw, TableInfo tableInfo, string additionalSettings, string terminator);
        void GenerateSoodaDynamicField(TextWriter tw, string terminator);
        string QuoteIdentifier(string s);
        string GetTruncatedIdentifier(string identifier);
        bool IsFatalException(IDbConnection connection, Exception e);
        bool IsNullValue(object val, Sooda.Schema.FieldInfo fi);
    }
}
