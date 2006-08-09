namespace Sooda.UnitTests.BaseObjects {
  using System;
  using System.Collections;
  using System.Diagnostics;
  using System.Data;
  using Sooda;
  
  
  public class ContactType : Sooda.UnitTests.BaseObjects.Stubs.ContactType_Stub {
    
    public ContactType(SoodaConstructor c) : 
        base(c) {
      // Do not modify this constructor.
    }
    
    public ContactType(SoodaTransaction transaction) : 
        base(transaction) {
      // 
      // TODO: Add construction logic here.
      // 
    }
    
    public ContactType() : 
        this(SoodaTransaction.ActiveTransaction) {
      // Do not modify this constructor.
    }
  }
}
