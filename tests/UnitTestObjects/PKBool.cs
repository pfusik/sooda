namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class PKBool : Sooda.UnitTests.Objects.Stubs.PKBool_Stub
    {
        public PKBool(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public PKBool(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public PKBool() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
