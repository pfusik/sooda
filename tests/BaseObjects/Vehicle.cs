namespace Sooda.UnitTests.BaseObjects
{
    using Sooda;
    using System;
    using System.Linq.Expressions;

    public interface IBaseClassName
    {
        string GetBaseClassName();
    }

    public abstract class Vehicle : Sooda.UnitTests.BaseObjects.Stubs.Vehicle_Stub, IBaseClassName
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

        public string GetBaseClassName()
        {
            return "Vehicle";
        }

        public static Expression<Func<Vehicle, string>> GetBaseClassNameExpression()
        {
            return t => "Vehicle";
        }
    }
}
