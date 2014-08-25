// 
// Copyright (c) 2012-2014 Piotr Fusik <piotr@fusik.info>
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
using Sooda.UnitTests.Objects;

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
        public void StringSubstring()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.Substring(1, 2) == "ar");
                CollectionAssert.AreEquivalent(new Contact[] { Contact.Mary, Contact.GetRef(51) }, ce);
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
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.ToUpper() == "MARY MANAGER");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringStartsWith()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.StartsWith("Mary M"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringEndsWith()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.EndsWith("anager"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void StringContains()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.Name.Contains("ry Ma"));
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());
            }
        }

        [Test]
        public void IntToString()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() where c.ContactId.ToString().Contains("2") orderby c.ContactId select c.ContactId;
                CollectionAssert.AreEqual(new int[] { 2, 52 }, ie);
            }
        }

        [Test]
        public void LongToString()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() where (1000000000000 + c.ContactId).ToString() == "1000000000003" select c.ContactId;
                CollectionAssert.AreEqual(new int[] { 3 }, ie);
            }
        }

        [Test]
        public void DoubleToString()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie =
                    from c in Contact.Linq()
                    where (c.ContactId / 2.0).ToString() == "0.5"
                        || (c.ContactId / 2.0).ToString() == "0,5"
                        || (c.ContactId / 2.0).ToString() == ".5"
                        || (c.ContactId / 2.0).ToString() == ",5"
                    select c.ContactId;
                CollectionAssert.AreEqual(new int[] { 1 }, ie);
            }
        }

        [Test]
        public void DecimalToString()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie =
                    from c in Contact.Linq()
                    where (c.ContactId / 2M).ToString().StartsWith("0.5")
                        || (c.ContactId / 2M).ToString().StartsWith("0,5")
                        || (c.ContactId / 2M).ToString().StartsWith(".5")
                        || (c.ContactId / 2M).ToString().StartsWith(",5")
                    select c.ContactId;
                CollectionAssert.AreEqual(new int[] { 1 }, ie);
            }
        }

        [Test]
        public void BoolToString()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<int> ie = from c in Contact.Linq() where (c.Name == "Mary Manager").ToString().StartsWith("T") select c.ContactId;
                CollectionAssert.AreEqual(new int[] { 1 }, ie);
            }
        }

        [Test]
        public void DateTimeYear()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Year == 2014);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Year == 2013);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeMonth()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Month == 7);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Month == 6);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeDay()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Day == 23);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Day == 3);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeDayOfYear()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.DayOfYear == 204);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.DayOfYear == 203);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeHour()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Hour == 13);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Hour == 1);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeMinute()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Minute == 5);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Minute == 0);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeSecond()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Second == 47);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Second == 0);
                Assert.IsFalse(b);
            }
        }

        [Test]
        public void DateTimeMillisecond()
        {
            using (new SoodaTransaction())
            {
                bool b = AllDataTypes.Linq().Any(a => a.NnDateVal.Millisecond == 123);
                Assert.IsTrue(b);
                b = AllDataTypes.Linq().Any(a => a.NnDateVal.Millisecond == 0);
                Assert.IsFalse(b);
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
        public void GetPrimaryKeyValue()
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

        [Test]
        public void GetLabel()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<Contact> ce = Contact.Linq().Where(c => c.GetLabel(false) == "Mary Manager");
                Assert.AreEqual(1, ce.Count());
                Assert.AreEqual(Contact.Mary, ce.First());

                IEnumerable<string> se = Contact.Linq().Select(c => c.GetLabel(false));
                CollectionAssert.AreEquivalent(new string[] { "Mary Manager", "Ed Employee", "Eva Employee", "Catie Customer", "Caroline Customer", "Chris Customer", "Chuck Customer" }, se);

                se = from c in Contact.Linq() orderby c.GetLabel(false) select c.GetLabel(false);
                CollectionAssert.AreEquivalent(new string[] { "Caroline Customer", "Catie Customer", "Chris Customer", "Chuck Customer", "Ed Employee", "Eva Employee", "Mary Manager" }, se);
            }
        }

        [Test]
        public void GetLabelNone()
        {
            using (new SoodaTransaction())
            {
                string[] sa = ContactType.Linq().Select(t => t.GetLabel(false)).ToArray();
                Assert.AreEqual(3, sa.Length);
                Assert.IsNull(sa[0]);
                Assert.IsNull(sa[1]);
                Assert.IsNull(sa[2]);

                int c = ContactType.Linq().Count(t => t.GetLabel(false) == null);
                Assert.AreEqual(3, c);
            }
        }

        [Test]
        public void OrderByGetLabelNone()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<ContactType> ce = ContactType.Linq().OrderBy(t => t.GetLabel(false));
                CollectionAssert.AreEquivalent(ContactType.GetList(true), ce);
            }
        }

        [Test]
        public void GetLabelInt()
        {
            using (new SoodaTransaction())
            {
                IEnumerable<string> se = from r in Role.Linq() orderby r.Id select r.GetLabel(false);
                CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, se);
            }
        }

        [Test]
        public void GetLabelReferencedObject()
        {
            using (new SoodaTransaction())
            {
                string[] sa = (from v in Vehicle.Linq() orderby v.Id select v.GetLabel(false)).ToArray();
                Assert.AreEqual(9, sa.Length);
                Assert.IsTrue(string.IsNullOrEmpty(sa[0]));
                Assert.AreEqual("Mary Manager", sa[1]);
                Assert.AreEqual("Ed Employee", sa[2]);
                Assert.IsTrue(string.IsNullOrEmpty(sa[3]));
                Assert.IsTrue(string.IsNullOrEmpty(sa[4]));
                Assert.IsTrue(string.IsNullOrEmpty(sa[5]));
                Assert.IsTrue(string.IsNullOrEmpty(sa[6]));
                Assert.IsTrue(string.IsNullOrEmpty(sa[7]));
                Assert.IsTrue(string.IsNullOrEmpty(sa[8]));
            }
        }

        [Test]
        public void GetSoodaClassPrimaryKeyValueLabel()
        {
            using (new SoodaTransaction())
            {
                var oa = Contact.Linq().OrderBy(c => c.ContactId).Take(2).Select(c => new {
                        ClassName = c.GetType().Name,
                        Id = c.GetPrimaryKeyValue(),
                        Label = c.GetLabel(false)
                    }).ToArray();

                Assert.AreEqual(2, oa.Length);
                Assert.AreEqual("Contact", oa[0].ClassName);
                Assert.AreEqual(1, oa[0].Id);
                Assert.AreEqual("Mary Manager", oa[0].Label);
                Assert.AreEqual("Contact", oa[1].ClassName);
                Assert.AreEqual(2, oa[1].Id);
                Assert.AreEqual("Ed Employee", oa[1].Label);
            }
        }
    }
}

#endif
