namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class ConcreteMegaSuperBikeA : Sooda.UnitTests.Objects.Stubs.ConcreteMegaSuperBikeA_Stub
    {
        public ConcreteMegaSuperBikeA(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public ConcreteMegaSuperBikeA(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public ConcreteMegaSuperBikeA() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
