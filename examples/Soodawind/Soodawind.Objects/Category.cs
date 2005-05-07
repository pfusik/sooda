namespace Soodawind.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Category : Soodawind.Objects.Stubs.Category_Stub
    {
        public Category(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Category(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Category() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
