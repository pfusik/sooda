namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class MegaSuperBike : Sooda.UnitTests.Objects.Stubs.MegaSuperBike_Stub
    {
        public MegaSuperBike(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public MegaSuperBike(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public MegaSuperBike() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
