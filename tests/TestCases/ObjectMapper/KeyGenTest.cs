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
using System.Collections;
using System.Threading;

using Sooda.ObjectMapper;
using Sooda.UnitTests.Objects;
using Sooda.UnitTests.BaseObjects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    [TestFixture]
    public class KeyGenTest
    {

        class Runner
        {
            public ArrayList GeneratedKeys = new ArrayList(); 

            public void ThreadProc()
            {
                using (SoodaTransaction t = new SoodaTransaction())
                {
                    for (int i = 0; i < 100; ++i)
                    {
                        Contact c = new Contact();
                        lock (GeneratedKeys)
                        {
                            GeneratedKeys.Add(c.GetPrimaryKeyValue());
                        }
                    }
                }
            }
        }
        
        [Test]
        public void Test1()
        {
            Runner r = new Runner();
            Thread[] threads = new Thread[50];

            Console.WriteLine("Creating threads...");
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i] = new Thread(new ThreadStart(r.ThreadProc));
            }
            Console.WriteLine("Starting threads...");
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i].Start();
            }
            Console.WriteLine("Waiting for threads to join...");
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i].Join();
            }
            Console.WriteLine("Joined. Got {0} keys", r.GeneratedKeys.Count);
            r.GeneratedKeys.Sort();
            for (int i = 0; i < r.GeneratedKeys.Count - 1; ++i)
            {
                Assert.IsTrue((int)r.GeneratedKeys[i] != (int)r.GeneratedKeys[i + 1]);
            }
            Console.WriteLine("Generated keys confirmed to be unique.", r.GeneratedKeys.Count);
        }
    }
}
