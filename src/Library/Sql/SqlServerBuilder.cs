//
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission.
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

namespace Sooda.Sql {
    public class SqlServerBuilder : SqlBuilderNamedArg {
        public SqlServerBuilder() { }

        public override string GetDDLCommandTerminator() {
            return Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine;
        }

        public override string GetSQLDataType(Sooda.Schema.FieldInfo fi) {
            switch (fi.DataType) {
            case FieldDataType.Integer:
                return "int";

            case FieldDataType.String:
                if (fi.Size > 4000)
                    return "text";
                else
                    return "varchar(" + fi.Size + ")";

            case FieldDataType.Decimal:
                if (fi.Size < 0)
                    return "decimal";
                else if (fi.Precision < 0)
                    return "decimal(" + fi.Size + ")";
                else 
                    return "decimal(" + fi.Size + "," + fi.Precision + ")";

            case FieldDataType.DateTime:
                return "datetime";

            case FieldDataType.Image:
                return "image";

            case FieldDataType.Long:
                return "bigint";

            case FieldDataType.BooleanAsInteger:
                return "int";

            default:
                throw new NotImplementedException(String.Format("Datatype {0} not supported for this database", fi.DataType.ToString()));
            }
        }

        protected override string GetNameForParameter(int pos) {
            return "@p" + pos.ToString();
        }

        public override SqlTopSupportMode TopSupport
        {
            get {
                return SqlTopSupportMode.SelectTop;
            }
        }
    }

}
