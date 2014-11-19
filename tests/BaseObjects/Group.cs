namespace Sooda.UnitTests.BaseObjects {
    using Sooda;
  
  
  public class Group : Sooda.UnitTests.BaseObjects.Stubs.Group_Stub {
    
    public Group(SoodaConstructor c) : 
        base(c) {
      // Do not modify this constructor.
    }
    
    public Group(SoodaTransaction transaction) : 
        base(transaction) {
      // 
      // TODO: Add construction logic here.
      // 
    }
    
    public Group() : 
        this(SoodaTransaction.ActiveTransaction) {
      // Do not modify this constructor.
    }
  }
}
