namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class ConcreteMegaSuperBikeB : Sooda.UnitTests.Objects.Stubs.ConcreteMegaSuperBikeB_Stub
    {
        public ConcreteMegaSuperBikeB(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public ConcreteMegaSuperBikeB(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public ConcreteMegaSuperBikeB() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
