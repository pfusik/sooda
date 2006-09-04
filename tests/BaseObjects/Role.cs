namespace Sooda.UnitTests.BaseObjects {
  using System;
  using System.Collections;
  using System.Diagnostics;
  using System.Data;
  using Sooda;
  
  
  public class Role : Sooda.UnitTests.BaseObjects.Stubs.Role_Stub {
    
    public Role(SoodaConstructor c) : 
        base(c) {
      // Do not modify this constructor.
    }
    
    public Role(SoodaTransaction transaction) : 
        base(transaction) {
      // 
      // TODO: Add construction logic here.
      // 
    }
    
    public Role() : 
        this(SoodaTransaction.ActiveTransaction) {
      // Do not modify this constructor.
    }

      public int AfterObjectInsertEventCounter = -1;
      public int AfterObjectUpdateEventCounter = -1;
      public int BeforeObjectInsertEventCounter = -1;
      public int BeforeObjectUpdateEventCounter = -1;

      public static int EventCounter = 0;

      protected override void AfterObjectInsert()
      {
          Console.WriteLine("{0}::AfterObjectInsert()", GetObjectKeyString());
          base.AfterObjectInsert();
          AfterObjectInsertEventCounter = EventCounter++;
      }

      protected override void BeforeObjectInsert()
      {
          Console.WriteLine("{0}::BeforeObjectInsert()", GetObjectKeyString());
          base.BeforeObjectInsert();
          BeforeObjectInsertEventCounter = EventCounter++;

          if (Name == "aaa")
          {
              Role r = new Role();
              r.Name = "second";
              Second = r;
          }
      }

      public Role Second;

      protected override void BeforeObjectUpdate()
      {
          Console.WriteLine("{0}::BeforeObjectUpdate()", GetObjectKeyString());
          base.BeforeObjectUpdate();
          BeforeObjectUpdateEventCounter = EventCounter++;
      }

      protected override void AfterObjectUpdate()
      {
          Console.WriteLine("{0}::AfterObjectUpdate()", GetObjectKeyString());
          base.AfterObjectUpdate();
          AfterObjectUpdateEventCounter = EventCounter++;
      }
  }
}
