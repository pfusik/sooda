namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class PKString : Sooda.UnitTests.Objects.Stubs.PKString_Stub
    {
        public PKString(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKString(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public PKString() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
