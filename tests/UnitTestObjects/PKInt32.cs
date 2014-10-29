//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Data;
    using System.Text;
    using Sooda;

    public class PKInt32 : Sooda.UnitTests.Objects.Stubs.PKInt32_Stub
    {
        public PKInt32(SoodaConstructor c)
            :
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKInt32(SoodaTransaction transaction)
            :
                base(transaction)
        {
        }
        public PKInt32()
            :
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }

        public string triggersText;

        public static string GetTriggerText(string trigger, string field, object oldVal, object newVal)
        {
            return string.Format("{0}({1}, {2}, {3})\n", trigger, field, oldVal, newVal);
        }

        protected override void BeforeFieldUpdate(string name, object oldVal, object newVal)
        {
            base.BeforeFieldUpdate(name, oldVal, newVal);
            triggersText += GetTriggerText("Before", name, oldVal, newVal);
        }

        protected override void AfterFieldUpdate(string name, object oldVal, object newVal)
        {
            base.AfterFieldUpdate(name, oldVal, newVal);
            triggersText += GetTriggerText("After", name, oldVal, newVal);
        }
    }
}
