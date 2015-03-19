namespace Sooda.UnitTests.BaseObjects
{
    using Sooda;
    using System;
    using System.Linq.Expressions;
  
    public abstract class Vehicle : Sooda.UnitTests.BaseObjects.Stubs.Vehicle_Stub
    {
    
        public Vehicle(SoodaConstructor c) : base(c) {
            // Do not modify this constructor.
        }
    
        public Vehicle(SoodaTransaction transaction) : base(transaction)
        {
            //
            // TODO: Add construction logic here.
            //
        }

        public Vehicle() : this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }

        public string BaseClassName
        {
            get
            {
                return "Vehicle";
            }
        }

        public static Expression<Func<Vehicle, string>> BaseClassNameExpression
        {
            get
            {
                return t => "Vehicle";
            }
        }
    }
}
