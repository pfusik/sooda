namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    using SoodaUnitTestsObjectsStubs = Sooda.UnitTests.Objects.Stubs;
    using Sooda.UnitTests.BaseObjects;
    
    
    public class EightFields : SoodaUnitTestsObjectsStubs.EightFields_Stub
    {
        
        public EightFields(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        
        public EightFields(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        
        public EightFields() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
