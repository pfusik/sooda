namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class PKInt64 : Sooda.UnitTests.Objects.Stubs.PKInt64_Stub
    {
        public PKInt64(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKInt64(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public PKInt64() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
