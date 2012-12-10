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
    public class ScalarTest
    {
        [Test]
        public void All()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().All(c => c.Active);
                Assert.IsTrue(result);
                result = Contact.Linq().All(c => c.Name == "Mary Manager");
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void Any()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().Any();
                Assert.IsTrue(result);
                result = Contact.Linq().Where(c => c.Name == "Mary Manager").Any();
                Assert.IsTrue(result);
                result = Contact.Linq().Where(c => c.Name == "Barbra Streisland").Any();
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void AnyFiltered()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().Any(c => c.Name == "Mary Manager");
                Assert.IsTrue(result);
                result = Contact.Linq().Any(c => c.Name == "Barbra Streisland");
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void AnyFilteredBool()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().Any(c => false);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void Contains()
        {
            using (new SoodaTransaction())
            {
                bool result = Contact.Linq().Contains(Contact.Mary);
                Assert.IsTrue(result);
                result = Contact.Linq().Where(c => c.ContactId > 1).Contains(Contact.Mary);
                Assert.IsFalse(result);
                result = Contact.Linq().Contains(null);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void Count()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Count();
                Assert.AreEqual(7, result);
                result = Contact.Linq().Where(c => c.Name == "Mary Manager").Count();
                Assert.AreEqual(1, result);
                result = Contact.Linq().Where(c => c.Name == "Barbra Streisland").Count();
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void CountFiltered()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Count(c => c.Name == "Mary Manager");
                Assert.AreEqual(1, result);
                result = Contact.Linq().Count(c => c.Name == "Barbra Streisland");
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void CountFilteredBool()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Count(c => true);
                Assert.AreEqual(7, result);
            }
        }

        [Test]
        public void AverageInt()
        {
            using (new SoodaTransaction())
            {
                double result = Contact.Linq().Where(c => c.ContactId <= 3).Average(c => c.ContactId);
                Assert.AreEqual(2.0, result);
            }
        }

        [Test]
        [Description("Fails on SQL Server, as it returns int for Avg(int) - should be fixed for SQL Server with a cast")]
        public void AverageIntFractional()
        {
            using (new SoodaTransaction())
            {
                double result = Contact.Linq().Where(c => c.ContactId <= 2).Average(c => c.ContactId);
                Assert.AreEqual(1.5, result);
            }
        }

        [Test]
        public void AverageDecimal()
        {
            using (new SoodaTransaction())
            {
                decimal result = Contact.Linq().Where(c => new int[] { 2, 3 }.Contains(c.ContactId)).Average(c => c.LastSalary.Value);
                Assert.AreEqual(289.5M, result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AverageEmpty()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Where(c => c.ContactId > 1000).Average(c => c.ContactId);
            }
        }

        [Test]
        public void MinInt()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Min(c => c.ContactId);
                Assert.AreEqual(1, result);
            }
        }

        [Test]
        public void MinDecimal()
        {
            using (new SoodaTransaction())
            {
                decimal result = Contact.Linq().Min(c => c.LastSalary.Value);
                Assert.AreEqual(-1M, result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MinEmpty()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Where(c => c.ContactId > 1000).Min(c => c.ContactId);
            }
        }

        [Test]
        public void MaxInt()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Max(c => c.ContactId);
                Assert.AreEqual(53, result);
            }
        }

        [Test]
        public void MaxDecimal()
        {
            using (new SoodaTransaction())
            {
                decimal result = Contact.Linq().Max(c => c.LastSalary.Value);
                Assert.AreEqual(345M, result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MaxEmpty()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Where(c => c.ContactId > 1000).Max(c => c.ContactId);
            }
        }

        [Test]
        public void SumInt()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Sum(c => c.ContactId);
                Assert.AreEqual(212, result);
            }
        }

        [Test]
        public void SumDecimal()
        {
            using (new SoodaTransaction())
            {
                decimal result = Contact.Linq().Sum(c => c.LastSalary.Value);
                Assert.AreEqual(926.388046789M, result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SumIntEmpty()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Where(c => c.ContactId > 1000).Max(c => c.ContactId);
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SumDecimalEmpty()
        {
            using (new SoodaTransaction())
            {
                decimal result = Contact.Linq().Where(c => c.ContactId > 1000).Max(c => c.LastSalary.Value);
                Assert.AreEqual(0M, result);
            }
        }

        // TODO: Average/Min/Max/Sum long/double/Nullable
    }
}

#endif
