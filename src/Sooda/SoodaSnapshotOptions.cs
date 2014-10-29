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

namespace Sooda
{
    /// <summary>
    /// Specifies options for creation of object list snapshots using GetList().
    /// </summary>
    [Flags]
    public enum SoodaSnapshotOptions
    {
        /// <summary>
        /// Use default options - load data from database, apply changes made within transaction.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Don't apply changes made in transaction. Use this option to perform a database snapshot or
        /// when you're sure that none of transaction objects will affect the result. This helps speed
        /// things a bit.
        /// </summary>
        NoTransaction = 1,

        /// <summary>
        /// Don't precommit modified objects
        /// </summary>
        NoWriteObjects = 4,

        /// <summary>
        /// Verify objects loaded from the database whether they actually match the
        /// where clause.
        /// </summary>
        VerifyAfterLoad = 8,

        /// <summary>
        /// Load only the primary key values. Objects will be materialized
        /// but fields other than primary keys will require a database query.
        /// </summary>
        KeysOnly = 16,

        /// <summary>
        /// Load the collection items from cache if possible.
        /// </summary>
        Cache = 32,

        /// <summary>
        /// Store the collection in cache after loading from the database.
        /// </summary>
        NoCache = 64,

        /// <summary>
        /// Use prefetch definition from the schema to load related objects automatically
        /// </summary>
        PrefetchRelated = 128,
    }
}
