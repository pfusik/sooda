namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class AllDataTypes : Sooda.UnitTests.Objects.Stubs.AllDataTypes_Stub
    {
        public AllDataTypes(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public AllDataTypes(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public AllDataTypes() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
