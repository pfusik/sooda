// get all products whose category names match:

foreach (Product p in Product.GetList(
    new SoodaWhereClause("Category.Name in ({0},{1})", "Water creatures", "Liquids"), 
    SoodaOrderBy.Ascending("Product.Name")))
{
    Console.WriteLine("product: {0} category: {1}", p.Name, p.Category.Name);
}

// examples of more complex Soql expressions:
//
// find all suppliers which ship beverages

Supplier.GetList(new SoodaWhereClause(
            "Products.Contains(Product where Category.Name = 'Beverages')"));

// 
// find all products supplied by Polish suppliers
// who belong to categories which have at least 10 products
Product.GetList(new SoodaWhereClause(
            "Supplier.Country = 'Poland' and Category.ProductsInThisCategory.Count > 10"
            ));
