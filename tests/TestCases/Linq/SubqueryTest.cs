//
// Copyright (c) 2014 Piotr Fusik <piotr@fusik.info>
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

#if DOTNET35

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;
namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class SubqueryTest
    {
        [Test]
        public void All()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).All(c => c.LastSalary.Value > 100));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Manager, ContactType.Employee }, te);
            }
        }

        [Test]
        public void Any()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t && c.LastSalary.Value == 345).Any());
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void AnyFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Any(c => c.Type == t && c.LastSalary.Value == 123));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void AnyUnion()
        {
            using (new SoodaTransaction())
            {
                var q1 = ContactType.Linq().Where(t => Contact.Linq().Any(c => c.Type == t && c.LastSalary.Value == 123));
                var q2 = ContactType.Linq().Where(t => Contact.Linq().Any(c => c.Type == t && c.LastSalary.Value == 345));
                IEnumerable<ContactType> te = q1.Union(q2);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer, ContactType.Employee }, te);
            }
        }

        [Test]
        public void Count()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Count() > 1);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee, ContactType.Customer }, te);
            }
        }

        [Test]
        public void CountFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Count(c => c.Type == t) > 1);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee, ContactType.Customer }, te);
            }
        }

        [Test]
        public void Contains()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Contains(Contact.Ed));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void AverageIntFractional()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Average(c => c.ContactId) == 51.5);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void SelectAverageIntFractional()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Select(c => c.ContactId).Average() == 51.5);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void Min()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Min(c => c.LastSalary.Value) == 234);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void SelectMin()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Select(c => c.LastSalary.Value).Min() == 234);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void Max()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Max(c => c.LastSalary.Value) == 345);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void SelectMax()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Select(c => c.LastSalary.Value).Max() == 345);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void Sum()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Sum(c => c.LastSalary.Value) == 234 + 345);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void SelectSum()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Select(c => c.LastSalary.Value).Sum() == 234 + 345);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void SumEmpty()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t && c.ContactId > 1).Sum(c => c.LastSalary.Value) == 0);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Manager }, te);
            }
        }

        [Test]
        public void FirstOrDefault()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Select(c => c.Name).FirstOrDefault().EndsWith("er"));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Manager, ContactType.Customer }, te);
            }
        }

        [Test]
        public void FirstOrDefaultFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).FirstOrDefault(c => c.Name.EndsWith("er")) == null);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        [Test]
        public void LastOrDefault()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se =
                    from t in ContactType.Linq()
                    orderby t.Code
                    select
                        (from c in Contact.Linq()
                        where c.Type == t
                        orderby c.LastSalary.Value
                        select c.Name).LastOrDefault();
                CollectionAssert.AreEqual(new string[] { "Chris Customer", "Eva Employee", "Mary Manager" }, se);
            }
        }

        [Test]
        public void LastOrDefaultFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce =
                    from t in ContactType.Linq()
                    orderby t.Code
                    select Contact.Linq().OrderBy(c => c.LastSalary.Value).LastOrDefault(c => c.Type == t);
                CollectionAssert.AreEqual(new Contact[] { Contact.GetRef(52), Contact.Eva, Contact.Mary }, ce);
            }
        }

        [Test]
        public void SingleOrDefault()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t && c.LastSalary.Value < 0).SingleOrDefault() != null);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void SingleOrDefaultFiltered()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().SingleOrDefault(c => c.Type == t && c.LastSalary.Value < 0) != null);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void FirstOrDefaultInt()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t =>
                    (from c in Contact.Linq()
                    where c.Type == t && c.LastSalary.Value > 123
                    select c.ContactId).FirstOrDefault() == 0);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void FirstOrDefaultNullableDecimal()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t =>
                    (from c in Contact.Linq()
                    where c.Type == t && c.LastSalary.Value > 123
                    select c.LastSalary).FirstOrDefault().IsNull);
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Customer }, te);
            }
        }

        [Test]
        public void TopSalaries()
        {
            using (new SoodaTransaction())
            {
                var l =
                    (from t in ContactType.Linq()
                    orderby Contact.Linq().Where(c => c.Type == t).Max(c => c.LastSalary.Value) descending
                    select new {
                        ContactType = t.Code,
                        TopSalary = Contact.Linq().Where(c => c.Type == t).Max(c => c.LastSalary.Value),
                        MostPaid =
                            (from c in Contact.Linq()
                            where c.Type == t
                            orderby c.LastSalary.Value descending
                            select c.Name).FirstOrDefault()
                    }).ToList();
                CollectionAssert.AreEqual(new string[] { "Employee", "Manager", "Customer" }, l.Select(o => o.ContactType));
                CollectionAssert.AreEqual(new decimal[] { 345M, 123.123456789M, 123M }, l.Select(o => o.TopSalary));
                CollectionAssert.AreEqual(new string[] { "Eva Employee", "Mary Manager", "Chris Customer" }, l.Select(o => o.MostPaid));
            }
        }

        static Expression<Func<Contact, bool>> GetSalaryIs345()
        {
            return c => c.LastSalary.Value == 345;
        }

        [Test]
        public void WhereExpressionMethod()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(GetSalaryIs345()).Any(c => c.Type == t));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        static Expression<Func<Contact, bool>> SalaryIs345
        {
            get
            {
                return c => c.LastSalary.Value == 345;
            }
        }

        [Test]
        public void WhereExpressionProperty()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(SalaryIs345).Any(c => c.Type == t));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }

        static Expression<Func<Contact, bool>> GetNameEndsWith(string suffix)
        {
            return c => c.Name.EndsWith(suffix);
        }

        [Test]
        public void WhereExpressionMethodConstParameter()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Any(GetNameEndsWith("er")));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Manager, ContactType.Customer }, te);
            }
        }

        static Expression<Func<Contact, bool>> GetTypeIs(ContactType t)
        {
            return c => c.Type == t;
        }

        [Test]
        public void WhereExpressionMethodRangeParameter()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Any(GetTypeIs(t)));
                CollectionAssert.AreEquivalent(new ContactType[] { ContactType.Employee }, te);
            }
        }
    }
}

#endif
