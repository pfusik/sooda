// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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


using Sooda.Schema;

namespace Sooda.Caching
{
    public class SoodaCachedCollectionKeyComplex : SoodaCachedCollectionKey
    {
        private string _whereClause;

        public SoodaCachedCollectionKeyComplex()
        {
        }

        public SoodaCachedCollectionKeyComplex(ClassInfo classInfo, string whereClause)
            : base(classInfo)
        {
            _whereClause = whereClause;
        }

        public string WhereClause
        {
            get { return _whereClause; }
            set { _whereClause = value; }
        }

        public override bool Equals(object obj)
        {
            SoodaCachedCollectionKeyComplex otherKey = obj as SoodaCachedCollectionKeyComplex;
            if (otherKey == null)
                return false;

            return (ClassInfo == otherKey.ClassInfo) && (WhereClause == otherKey.WhereClause);
        }

        public override int GetHashCode()
        {
            return ClassInfo.Name.GetHashCode() ^ WhereClause.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + ClassInfo.Name + " where " + WhereClause + "]";
        }
    }
}
