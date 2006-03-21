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

using System;
using System.Text;
using System.Collections;

using Sooda.QL;

namespace Sooda
{
    public class SoodaOrderBy
    {
        private SoodaObjectExpressionComparer _comparer;

        public SoodaOrderBy(string columnName, SortOrder sortOrder)
        {
            SoodaObjectExpressionComparer ec = new SoodaObjectExpressionComparer();
            ec.AddExpression(new SoqlPathExpression(columnName), sortOrder);
            _comparer = ec;
        }

        public SoodaOrderBy(string columnName1, SortOrder sortOrder1,
            string columnName2, SortOrder sortOrder2)
        {
            SoodaObjectExpressionComparer ec = new SoodaObjectExpressionComparer();
            ec.AddExpression(new SoqlPathExpression(columnName1), sortOrder1);
            ec.AddExpression(new SoqlPathExpression(columnName2), sortOrder2);
            _comparer = ec;
        }

        public SoodaOrderBy(string columnName1, SortOrder sortOrder1,
            string columnName2, SortOrder sortOrder2,
            string columnName3, SortOrder sortOrder3)
        {
            SoodaObjectExpressionComparer ec = new SoodaObjectExpressionComparer();
            ec.AddExpression(new SoqlPathExpression(columnName1), sortOrder1);
            ec.AddExpression(new SoqlPathExpression(columnName2), sortOrder2);
            ec.AddExpression(new SoqlPathExpression(columnName3), sortOrder3);
            _comparer = ec;
        }

        public SoodaOrderBy(string[] columnNames, SortOrder[] sortOrders)
        {
            SoodaObjectExpressionComparer ec = new SoodaObjectExpressionComparer();
            for (int i = 0; i < columnNames.Length; ++i)
            {
                ec.AddExpression(new SoqlPathExpression(columnNames[i]), sortOrders[i]);
            }
            _comparer = ec;
        }

        public SoodaOrderBy(SoqlExpression expression, SortOrder sortOrder)
        {
            SoodaObjectExpressionComparer ec = new SoodaObjectExpressionComparer();
            ec.AddExpression(expression, sortOrder);
            _comparer = ec;
        }

        public virtual IComparer GetComparer()
        {
            return _comparer;
        }

        public static SoodaOrderBy Ascending(string columnName)
        {
            return new SoodaOrderBy(columnName, SortOrder.Ascending);
        }

        public static SoodaOrderBy Descending(string columnName)
        {
            return new SoodaOrderBy(columnName, SortOrder.Descending);
        }

        public static SoodaOrderBy Parse(string sortString)
        {
            string[] components = sortString.Trim().Split(',');
            string[] columnNames = new string[components.Length];
            SortOrder[] sortOrders = new SortOrder[components.Length];

            for (int i = 0; i < components.Length; ++i)
            {
                string[] tokens = components[i].Trim().Split(' ');
                if (tokens.Length > 2 || tokens.Length == 0)
                {
                    throw new ArgumentException("Invalid order by string");
                }

                SortOrder order = SortOrder.Ascending;
                if (tokens.Length == 2)
                {
                    if (tokens[1].ToLower() == "desc")
                    {
                        order = SortOrder.Descending;
                    }
                }

                columnNames[i] = tokens[0];
                sortOrders[i] = order;
            }

            return new SoodaOrderBy(columnNames, sortOrders);
        }

        public static SoodaOrderBy FromExpression(SoqlExpression sortExpression)
        {
            return FromExpression(sortExpression, SortOrder.Ascending);
        }
            
        public static SoodaOrderBy FromExpression(SoqlExpression sortExpression, SortOrder sortOrder)
        {
            return new SoodaOrderBy(sortExpression, sortOrder);
        }

        public SoqlExpression[] OrderByExpressions
        {
            get { return _comparer.OrderByExpressions; }
        }

        public SortOrder[] SortOrders
        {
            get { return _comparer.SortOrders; }
        }

        public static readonly SoodaOrderBy Unsorted = null;
    }
}

