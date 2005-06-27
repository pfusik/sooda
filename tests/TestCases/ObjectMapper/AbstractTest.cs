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
using System.Diagnostics;
using System.Data;

using Sooda.ObjectMapper;
using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper {
    [TestFixture]
    public class AbstractTest {
        [Test]
        public void AbstractTest1() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(1);
            }
        }

        [Test]
        public void AbstractTest2() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(2);
            }
        }

        [Test]
        public void AbstractTest3() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(3);
            }
        }

        [Test]
        public void AbstractTest4() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(4);
            }
        }

        [Test]
        public void AbstractTest5() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(5);
            }
        }

        [Test]
        public void AbstractTest6() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(6);
            }
        }

        [Test]
        public void AbstractTest7() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                Vehicle v = Vehicle.GetRef(10);
            }
        }

        [Test]
        public void AbstractTest8() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                AbstractMegaSuperBike v = AbstractMegaSuperBike.GetRef(11);
            }
        }

        [Test]
        public void AbstractTest9() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                ConcreteMegaSuperBikeA v = ConcreteMegaSuperBikeA.GetRef(11);
            }
        }

        [Test]
        public void AbstractTest10() {
            using (SoodaTransaction tran = new SoodaTransaction()) {
                ConcreteMegaSuperBikeB v = ConcreteMegaSuperBikeB.GetRef(12);
            }
        }
    }
}
