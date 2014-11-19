namespace Sooda.UnitTests.Objects
{
    using Sooda;
    using SoodaUnitTestsObjectsStubs = Sooda.UnitTests.Objects.Stubs;

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
