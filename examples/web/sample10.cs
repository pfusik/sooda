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

            // display all beverage products
            foreach (Product p in c.ProductsInThisCategory)
            {
                Console.WriteLine("Product: {0} category: {1} price: {2}", p.Name, p.Category.Name, p.UnitPrice);
            }

            Console.WriteLine("The total number of beverages: {0}", c.ProductsInThisCategory.Count);

            Product p2 = Product.Load(1);

            // remove a particular product from the collection
            p2.ProductsInThisCategory.Remove(p2);

            // the new p2.Category == null (the product is no longer in the collection)
            Console.WriteLine("New category of product #1 is null: {0}", p2.Category == null);
            
            // iterate over first 3 products (in no particular order)
            foreach (Product p in c.ProductsInThisCategory.SelectFirst(3))
            {
                // do something
            }

            // note that the order is fixed after the collection is first accessed. subsequent accesses
            // can rely on the fact.

            // iterate over last 3 products (in no particular order)
            foreach (Product p in c.ProductsInThisCategory.SelectLast(3))
            {
                // do something
            }
            
            // iterate over the specified range of products (in no particular order)
            foreach (Product p in c.ProductsInThisCategory.SelectRange(3,10))
            {
                // do something
            }
             
            // iterate over the products sorted by name
            foreach (Product p in c.ProductsInThisCategory.Sort(SoodaOrderBy.Ascending("Name").GetComparer()))
            {
                // do something
            }
            
            // get the snapshot of this collection. Updates to c.ProductsInThisCategory (direct or indirect)
            // will not affect the snapshot
            ProductList snapshot = c.ProductsInThisCategory.GetSnapshot();

            foreach (Product p in snapshot)
            {
                // do something
            }

            // another way to iterate the collection. Note that ProductList returns strongly-typed instances
            // of Product

            for (int i = 0; i < snapshot.Count; ++i)
            {
                Product p3 = snapshot[i];
            }
        }
    }
}
