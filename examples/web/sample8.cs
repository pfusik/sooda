using System;
using Sooda;

using Soodawind.Objects;

class Sample4 
{
    static void Main()
    {
        using (SoodaTransaction transaction = new SoodaTransaction()) 
        {
            Category c = Category.Load(1);
            c.Name = "New Name";
            transaction.Commit();
        }
    }
}
