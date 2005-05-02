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
using Sooda.ObjectMapper;

namespace Sooda 
{
    [Serializable]
    public class SoodaPair : ISoodaTuple, IComparable
    {
        private object _o1;
        private object _o2;

        public SoodaPair(object o1, object o2)
        {
            _o1 = o1;
            _o2 = o2;
        }

        public override bool Equals(object obj)
        {
            SoodaPair pair2 = obj as SoodaPair;
            if (pair2 == null)
                return false;

            if (!_o1.Equals(pair2._o1))
                return false;

            if (!_o2.Equals(pair2._o2))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _o1.GetHashCode() ^ _o2.GetHashCode();
        }

        public int Length
        {
            get { return 2; }
        }

        public object GetValue(int ordinal)
        {
            if (ordinal == 0)
                return _o1;
            else if (ordinal == 1)
                return _o2;
            else 
                throw new ArgumentException("ordinal");
        }
        
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
