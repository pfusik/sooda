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
using System.Data;
using System.Xml;
using System.Text;
using System.Collections;

namespace Sooda
{
    public class SoodaOrderBy
    {
        private string[] columnName;
        private SortOrder[] sortOrder;
        
        public SoodaOrderBy(string columnName, SortOrder sortOrder)
        {
            this.columnName = new string[1];
            this.sortOrder = new SortOrder[1];
            this.columnName[0] = columnName;
            this.sortOrder[0] = sortOrder;
        }

        public SoodaOrderBy(string columnName1, SortOrder sortOrder1, 
                    string columnName2, SortOrder sortOrder2)
        {
            this.columnName = new string[2];
            this.sortOrder = new SortOrder[2];
            this.columnName[0] = columnName1;
            this.sortOrder[0] = sortOrder1;
            this.columnName[1] = columnName2;
            this.sortOrder[1] = sortOrder2;
        }

        public SoodaOrderBy(string columnName1, SortOrder sortOrder1, 
                    string columnName2, SortOrder sortOrder2,
                    string columnName3, SortOrder sortOrder3)
        {
            this.columnName = new string[3];
            this.sortOrder = new SortOrder[3];
            this.columnName[0] = columnName1;
            this.sortOrder[0] = sortOrder1;
            this.columnName[1] = columnName2;
            this.sortOrder[1] = sortOrder2;
            this.columnName[2] = columnName3;
            this.sortOrder[2] = sortOrder3;
        }

        public SoodaOrderBy(string[] columnNames, SortOrder[] sortOrders) 
        {
            this.columnName = columnNames;
            this.sortOrder = sortOrders;
        }

        public IComparer GetComparer()
        {
            if (columnName.Length == 1)
            {
                return new SoodaObjectFieldComparer(columnName[0], sortOrder[0]);
            }
            else
            {
                SoodaObjectMultiFieldComparer retVal = new SoodaObjectMultiFieldComparer();

                for (int i = 0; i < sortOrder.Length; ++i)
                {
                    retVal.AddField(columnName[i], sortOrder[i]);
                }
                return retVal;
            }
        }

        public static SoodaOrderBy Ascending(string columnName)
        {
            return new SoodaOrderBy(columnName, SortOrder.Ascending);
        }

        public static SoodaOrderBy Descending(string columnName)
        {
            return new SoodaOrderBy(columnName, SortOrder.Descending);
        }

        public static readonly SoodaOrderBy Unsorted = null;
    }
}

