namespace Soodawind.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Supplier : Soodawind.Objects.Stubs.Supplier_Stub
    {
        public Supplier(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Supplier(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Supplier() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }

        public bool HasHomePage()
        {
            return HomePage != null;
        }
    }
}
