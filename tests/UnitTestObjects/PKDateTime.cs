namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class PKDateTime : Sooda.UnitTests.Objects.Stubs.PKDateTime_Stub
    {
        public PKDateTime(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKDateTime(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public PKDateTime() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
