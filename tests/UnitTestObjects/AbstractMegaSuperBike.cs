namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public abstract class AbstractMegaSuperBike : Sooda.UnitTests.Objects.Stubs.AbstractMegaSuperBike_Stub
    {
        public AbstractMegaSuperBike(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public AbstractMegaSuperBike(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public AbstractMegaSuperBike() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
