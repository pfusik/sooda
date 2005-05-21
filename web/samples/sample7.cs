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
            Console.WriteLine("The name of the category is {0}", c.Name);
        }
    }
}
