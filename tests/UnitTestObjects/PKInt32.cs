namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class PKInt32 : Sooda.UnitTests.Objects.Stubs.PKInt32_Stub
    {
        public PKInt32(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKInt32(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public PKInt32() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
