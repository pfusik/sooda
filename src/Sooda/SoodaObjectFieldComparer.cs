// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.Collections;

namespace Sooda
{
    public enum SortOrder
    {
        None,
        Ascending,
        Descending,
    }

    public class SoodaObjectFieldComparer : IComparer
    {
        private string[] _propertyChain;
        private SortOrder _sortOrder;

        public SoodaObjectFieldComparer(string propertyName, SortOrder sortOrder)
        {
            this._propertyChain = propertyName.Split('.');
            this._sortOrder = sortOrder;
        }

        public SoodaObjectFieldComparer(string[] propertyChain, SortOrder sortOrder)
        {
            this._propertyChain = propertyChain;
            this._sortOrder = sortOrder;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("singlefieldcomparer[");
            sb.Append(String.Join(".", _propertyChain));
            sb.Append(' ');
            sb.Append(_sortOrder);
            sb.Append(']');
            return sb.ToString();
        }


        int IComparer.Compare(object o1, object o2)
        {
            SoodaObject dbo1 = o1 as SoodaObject;
            SoodaObject dbo2 = o2 as SoodaObject;

            return Compare(dbo1, dbo2);
        }

        public int Compare(SoodaObject dbo1, SoodaObject dbo2)
        {
            object v1 = dbo1.Evaluate(_propertyChain, false);
            object v2 = dbo2.Evaluate(_propertyChain, false);

            if (_sortOrder == SortOrder.Ascending)
                return DoCompare(v1, v2, dbo1, dbo2);
            else
                return -DoCompare(v1, v2, dbo1, dbo2);
        }

        private static int PrimaryKeyCompare(SoodaObject dbo1, SoodaObject dbo2)
        {
            return ((IComparable)dbo1.GetPrimaryKeyValue()).CompareTo(dbo2.GetPrimaryKeyValue());
        }

        private static int DoCompare(object v1, object v2, SoodaObject dbo1, SoodaObject dbo2)
        {
            if (v1 == null)
            {
                if (v2 == null)
                    return PrimaryKeyCompare(dbo1, dbo2);
                else
                    return -1;  // null is less than anything
            };

            if (v2 == null)
            {
                return 1;   // not null is greater than anything
            }

            int v = ((IComparable)v1).CompareTo(v2);
            if (v == 0)
                return PrimaryKeyCompare(dbo1, dbo2);
            else
                return v;
        }
    }
}
