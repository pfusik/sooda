namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class SuperBike : Sooda.UnitTests.Objects.Stubs.SuperBike_Stub
    {
        public SuperBike(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public SuperBike(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public SuperBike() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
