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

namespace Sooda {
    /// <summary>
    /// Specifies options for creation of object list snapshots using GetList().
    /// </summary>
    [Flags]
    public enum SoodaSnapshotOptions
    {
        /// <summary>
        /// Use default options - load data from database, apply changes made within transaction. SoodaWhereClause clause specified as Soql;
        /// </summary>
        Default = 0,

        /// <summary>
        /// Don't apply changes made in transaction. Use this option to perform a database snapshot or
        /// when you're sure that none of transaction objects will affect the result. This helps speed
        /// things a bit.
        /// </summary>
        NoTransaction = 1,

        /// <summary>
        /// Don't make database query. This will walk all database objects of specified class, check
        /// if they match the specified <c>whereClause</c> and add them to the snapshot.
        /// Use this option when you're certain that all objects that you want to include in the collection
        /// have been previously materialized.
        /// </summary>
        NoDatabase = 2,

        NoWriteObjects = 4,
    }
}
