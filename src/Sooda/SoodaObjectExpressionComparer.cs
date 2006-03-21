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
    public class SoodaObjectExpressionComparer : IComparer
    {
        class ExpressionCompareInfo
        {
            public ExpressionCompareInfo(SoqlExpression expression, SortOrder sortOrder)
            {
                this.Expression = expression;
                this.SortOrder = sortOrder;
            }

            public readonly SoqlExpression Expression;
            public readonly SortOrder SortOrder;
        }

        class EvaluateContext : ISoqlEvaluateContext
        {
            private SoodaObject _rootObject;

            public EvaluateContext()
            {
                _rootObject = null;
            }

            public object GetRootObject()
            {
                return _rootObject;
            }

            public void SetRootObject(SoodaObject o)
            {
                _rootObject = o;
            }

            public object GetParameter(int position)
            {
                throw new Exception("No parameters are allowed in expression comparer.");
            }
        }
        
        private ArrayList expressions = new ArrayList();
        private EvaluateContext _context1 = new EvaluateContext();
        private EvaluateContext _context2 = new EvaluateContext();

        public SoodaObjectExpressionComparer() { }

        int IComparer.Compare(object o1, object o2)
        {
            SoodaObject dbo1 = o1 as SoodaObject;
            SoodaObject dbo2 = o2 as SoodaObject;

            return Compare(dbo1, dbo2);
        }

        public void AddExpression(SoqlExpression expression, SortOrder sortOrder)
        {
            expressions.Add(new ExpressionCompareInfo(expression, sortOrder));
        }

        public int Compare(SoodaObject dbo1, SoodaObject dbo2)
        {
            _context1.SetRootObject(dbo1);
            _context2.SetRootObject(dbo2);

            foreach (ExpressionCompareInfo eci in expressions)
            {
                object v1 = eci.Expression.Evaluate(_context1);
                object v2 = eci.Expression.Evaluate(_context2);

                int result = DoCompare(v1, v2);
                if (result != 0)
                {
                    if (eci.SortOrder == SortOrder.Ascending)
                        return result;
                    else
                        return -result;
                }
            }

            return PrimaryKeyCompare(dbo1, dbo2);
        }

        private static int DoCompare(object v1, object v2)
        {
            if (v1 == null)
            {
                if (v2 == null)
                    return 0;
                else
                    return -1;  // null is less than anything
            };

            if (v2 == null)
            {
                return 1;   // not null is greater than anything
            }

            return ((IComparable)v1).CompareTo(v2);
        }

        private static int PrimaryKeyCompare(SoodaObject dbo1, SoodaObject dbo2)
        {
            return ((IComparable)dbo1.GetPrimaryKeyValue()).CompareTo(dbo2.GetPrimaryKeyValue());
        }


        public SoqlExpression[] OrderByExpressions
        {
            get
            {
                ArrayList al = new ArrayList();
                foreach (ExpressionCompareInfo eci in expressions)
                {
                    al.Add(eci.Expression);
                }
                return (SoqlExpression[])al.ToArray(typeof(SoqlExpression));
            }
        }

        public SortOrder[] SortOrders
        {
            get
            {
                ArrayList al = new ArrayList();
                foreach (ExpressionCompareInfo eci in expressions)
                {
                    al.Add(eci.SortOrder);
                }
                return (SortOrder[])al.ToArray(typeof(SortOrder));
            }
        }
    }
}
