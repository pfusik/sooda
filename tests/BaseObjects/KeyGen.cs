namespace Sooda.UnitTests.BaseObjects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class KeyGen : Sooda.UnitTests.BaseObjects.Stubs.KeyGen_Stub
    {
        public KeyGen(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public KeyGen(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public KeyGen() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
