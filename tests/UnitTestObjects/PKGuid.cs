namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class PKGuid : Sooda.UnitTests.Objects.Stubs.PKGuid_Stub
    {
        public PKGuid(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKGuid(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public PKGuid() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
