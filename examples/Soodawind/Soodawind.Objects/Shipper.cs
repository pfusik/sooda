namespace Soodawind.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Shipper : Soodawind.Objects.Stubs.Shipper_Stub
    {
        public Shipper(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Shipper(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Shipper() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
