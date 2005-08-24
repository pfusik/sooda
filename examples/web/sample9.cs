using System;
using Sooda;

using Soodawind.Objects;

class Sample4 
{
    static void Main()
    {
        using (SoodaTransaction transaction = new SoodaTransaction()) 
        {
            Product p = Product.Load(1);

            Console.WriteLine("Product: {0} category: {1} price: {2}", p.Name, p.Category.Name, p.UnitPrice);
        }
    }
}
