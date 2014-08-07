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
        public void AverageIntFractional()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Average(c => c.ContactId) == 51.5);
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
        public void Max()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> te = ContactType.Linq().Where(t => Contact.Linq().Where(c => c.Type == t).Max(c => c.LastSalary.Value) == 345);
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
    }
}

#endif
