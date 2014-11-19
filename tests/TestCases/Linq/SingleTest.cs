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

using NUnit.Framework;
using Sooda.UnitTests.BaseObjects;
using System;
using System.Linq;

namespace Sooda.UnitTests.TestCases.Linq
{
    [TestFixture]
    public class SingleTest
    {
        [Test]
        public void First()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).First();
                Assert.AreEqual(1, result.ContactId);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FirstNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Where(c => c.ContactId > 1000).First();
            }
        }

        [Test]
        public void FirstFiltered()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().First(c => c.ContactId == 2);
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FirstFilteredNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().First(c => c.ContactId > 1000);
            }
        }

        [Test]
        public void FirstMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).First(c => c.Active);
                Assert.AreEqual(1, result.ContactId);
            }
        }

        [Test]
        public void FirstOrDefault()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).FirstOrDefault();
                Assert.AreEqual(1, result.ContactId);
            }
        }

        [Test]
        public void FirstOrDefaultNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Where(c => c.ContactId > 1000).FirstOrDefault();
                Assert.IsNull(result);
            }
        }

        [Test]
        public void FirstOrDefaultMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).FirstOrDefault();
                Assert.AreEqual(1, result.ContactId);
            }
        }

        [Test]
        public void FirstOrDefaultFiltered()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().FirstOrDefault(c => c.ContactId == 2);
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        public void FirstOrDefaultFilteredNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().FirstOrDefault(c => c.ContactId > 1000);
                Assert.IsNull(result);
            }
        }

        [Test]
        public void FirstOrDefaultFilteredMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).FirstOrDefault(c => c.Active);
                Assert.AreEqual(1, result.ContactId);
            }
        }

        [Test]
        public void Last()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).Last();
                Assert.AreEqual(53, result.ContactId);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LastNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Where(c => c.ContactId > 1000).Last();
            }
        }

        [Test]
        public void LastFiltered()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Last(c => c.ContactId == 2);
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LastFilteredNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Last(c => c.ContactId > 1000);
            }
        }

        [Test]
        public void LastMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).Last(c => c.Active);
                Assert.AreEqual(53, result.ContactId);
            }
        }

        [Test]
        public void LastOrDefault()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).LastOrDefault();
                Assert.AreEqual(53, result.ContactId);
            }
        }

        [Test]
        public void LastOrDefaultNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Where(c => c.ContactId > 1000).LastOrDefault();
                Assert.IsNull(result);
            }
        }

        [Test]
        public void LastOrDefaultMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).LastOrDefault();
                Assert.AreEqual(53, result.ContactId);
            }
        }

        [Test]
        public void LastOrDefaultFiltered()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().LastOrDefault(c => c.ContactId == 2);
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        public void LastOrDefaultFilteredNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().LastOrDefault(c => c.ContactId > 1000);
                Assert.IsNull(result);
            }
        }

        [Test]
        public void LastOrDefaultFilteredMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().OrderBy(c => c.ContactId).LastOrDefault(c => c.Active);
                Assert.AreEqual(53, result.ContactId);
            }
        }

        [Test]
        public void Single()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Where(c => c.ContactId == 2).Single();
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Where(c => c.ContactId > 1000).Single();
            }
        }

        [Test]
        public void SingleFiltered()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Single(c => c.ContactId == 2);
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleFilteredNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().Single(c => c.ContactId > 1000);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().OrderBy(c => c.ContactId).Single(c => c.Active);
            }
        }

        [Test]
        public void SingleOrDefault()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Where(c => c.ContactId == 2).SingleOrDefault();
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        public void SingleOrDefaultNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().Where(c => c.ContactId > 1000).SingleOrDefault();
                Assert.IsNull(result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleOrDefaultMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().OrderBy(c => c.ContactId).SingleOrDefault();
            }
        }

        [Test]
        public void SingleOrDefaultFiltered()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().SingleOrDefault(c => c.ContactId == 2);
                Assert.AreEqual(2, result.ContactId);
            }
        }

        [Test]
        public void SingleOrDefaultFilteredNotFound()
        {
            using (new SoodaTransaction())
            {
                Contact result = Contact.Linq().SingleOrDefault(c => c.ContactId > 1000);
                Assert.IsNull(result);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleOrDefaultFilteredMultiple()
        {
            using (new SoodaTransaction())
            {
                Contact.Linq().OrderBy(c => c.ContactId).SingleOrDefault(c => c.Active);
            }
        }

        [Test]
        public void SelectFirst()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().OrderBy(c => c.ContactId).Select(c => c.ContactId).First();
                Assert.AreEqual(1, result);
            }
        }

        [Test]
        public void SelectDefaultInt()
        {
            using (new SoodaTransaction())
            {
                int result = Contact.Linq().Where(c => c.Name == "Jarek").Select(c => c.ContactId).FirstOrDefault();
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void SelectSingle()
        {
            using (new SoodaTransaction())
            {
                string result = Contact.Linq().Where(c => c.ContactId == 1).Select(c => c.Name).Single();
                Assert.AreEqual("Mary Manager", result);
            }
        }

        [Test]
        public void SelectFirstFiltered()
        {
            using (new SoodaTransaction())
            {
                string result = Contact.Linq().OrderBy(c => c.ContactId).Select(c => c.Name).First(s => s.StartsWith("M"));
                Assert.AreEqual("Mary Manager", result);
            }
        }
    }
}

#endif
