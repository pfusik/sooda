// 
// Copyright (c) 2012 Piotr Fusik <piotr@fusik.info>
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

using Sooda.Linq;

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class FunctionTest
    {
        [Test]
        public void StringLike()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.Like("Mar%"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());

                ce = Contact.Linq().Where(c => c.Name.Like("Mary Manager"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());

                ce = Contact.Linq().Where(c => c.Name.Like("% Customer"));
                Assert.AreEqual(4, ce.Count());

                ce = Contact.Linq().Where(c => c.Name.Like("%e %"));
                Assert.AreEqual(2, ce.Count());

                ce = Contact.Linq().Where(c => c.Name.Like("%"));
                Assert.AreEqual(7, ce.Count());

                ce = Contact.Linq().Where(c => c.Name.Like("%%%"));
                Assert.AreEqual(7, ce.Count());
            }
        }

        [Test]
        public void StringConcat()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => string.Concat(c.Name, c.PrimaryGroup.Name.Value) == "Mary ManagerGroup1");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringRemove()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.Remove(4) == "Mary");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringReplace()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.Replace("M", "G") == "Gary Ganager");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringToLower()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.ToLower() == "mary manager");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringToUpper()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.ToLower() == "MARY MANAGER");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void MathAbs()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Abs(c.LastSalary.Value) == 1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual("Chuck Customer", ce.First().Name);
            }
        }

        [Test]
        public void MathAcos()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Acos(c.PrimaryGroup.Id - 10) == 0);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Eva, ce.First());
            }
        }

        [Test]
        public void MathAsin()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Asin(c.PrimaryGroup.Id - 11) == 0);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Eva, ce.First());
            }
        }

        [Test]
        public void MathAtan()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Atan(c.ContactId - 1) == 0);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void MathCos()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Cos(c.ContactId - 1) == 1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void MathExp()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Exp(c.ContactId - 1) == 1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void MathFloor()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Floor(c.LastSalary.Value) == 123M);
                Assert.AreEqual(2, ce.Count());
            }
        }

        [Test]
        public void MathPow()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Pow(c.ContactId, 2) == 2500);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(50, ce.First().ContactId);
            }
        }

        [Test]
        [SetCulture("pl-PL")] // Polish decimal point is comma, make sure it doesn't land in SQL
        public void MathRound()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Round(c.LastSalary.Value, 2) == 123.12M);
                Assert.AreEqual(1, ce.Count());
            }
        }

        [Test]
        public void MathSign()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Sign(c.LastSalary.Value) == -1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(53, ce.First().ContactId);
            }
        }

        [Test]
        public void MathSin()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Sin(c.ContactId - 1) == 0);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void MathSqrt()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Sqrt(c.ContactId) == 1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void MathTan()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => Math.Tan(c.ContactId - 1) == 0);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void SoodaClass()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.GetType().Name == "Contact");
                Assert.AreEqual(7, ce.Count());

                IEnumerable<Vehicle> ve = Vehicle.Linq().Where(c => c.GetType().Name == "Car");
                Assert.AreEqual(2, ve.Count());
            }
        }

        [Test]
        public void GetPrimaryKey()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => (int) c.GetPrimaryKeyValue() == 1);
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());

                IEnumerable<object> oe = Contact.Linq().Select(c => c.GetPrimaryKeyValue());
                CollectionAssert.AreEquivalent(new object[] { 1, 2, 3, 50, 51, 52, 53 }, oe);
            }
        }
    }
}

#endif
