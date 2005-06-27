namespace Sooda.UnitTests.BaseObjects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Car : Sooda.UnitTests.BaseObjects.Stubs.Car_Stub
    {
        public Car(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Car(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Car() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
