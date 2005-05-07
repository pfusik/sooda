namespace Soodawind.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Product : Soodawind.Objects.Stubs.Product_Stub
    {
        public Product(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Product(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Product() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
