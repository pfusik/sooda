namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class XmlTest1 : Sooda.UnitTests.Objects.Stubs.XmlTest1_Stub
    {
        public XmlTest1(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public XmlTest1(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public XmlTest1() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
