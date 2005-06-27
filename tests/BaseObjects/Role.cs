namespace Sooda.UnitTests.BaseObjects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Role : Sooda.UnitTests.BaseObjects.Stubs.Role_Stub
    {
        public Role(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Role(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Role() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
