namespace Sooda.UnitTests.Objects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    
    [SoodaSerializable]
    public class Contact : Sooda.UnitTests.Objects.Stubs.Contact_Stub
    {
        private decimal _persistentValue3;

        [SoodaSerializable]
        public int PersistentValue1;
        [SoodaSerializable]
        public bool PersistentValue2;
        [SoodaSerializable]
        public decimal PersistentValue3
        {
            get { return _persistentValue3; }
            set { _persistentValue3 = value; }
        }
        [SoodaSerializable]
        public string PersistentValue4;

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
