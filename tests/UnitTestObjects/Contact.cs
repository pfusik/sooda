namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    public class Contact : Sooda.UnitTests.Objects.Stubs.Contact_Stub
    {
        public string PersistentValue
        {
            get { return (string)GetTransactionPersistentValue("PersistentValue"); }
            set { SetTransactionPersistentValue("PersistentValue", value); }
        }
        public Contact(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        public Contact(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        public Contact() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
