namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class ExtendedBike : Sooda.UnitTests.Objects.Stubs.ExtendedBike_Stub
    {
        public ExtendedBike(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public ExtendedBike(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public ExtendedBike() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
