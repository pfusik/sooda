namespace Sooda.UnitTests.BaseObjects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Bike : Sooda.UnitTests.BaseObjects.Stubs.Bike_Stub
    {
        public Bike(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Bike(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Bike() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
