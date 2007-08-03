// get all products whose category names match:

foreach (Product p in Product.GetList(
    ProductField.Category.Name.In("Water creatures", "Liquids")
    SoodaOrderBy.Ascending(ProductField.Name)))
{
    Console.WriteLine("product: {0} category: {1}", p.Name, p.Category.Name);
}

// examples of more complex Soql expressions:
//
// find all suppliers which ship beverages

Supplier.GetList(
        SupplierField.Products.ContainsProductWhere(
            ProductField.Category.Name == "Beverages"));

// 
// find all products supplied by Polish suppliers
// who belong to categories which have at least 10 products
Product.GetList(ProductField.Supplier.Country == "Poland"
        && ProductField.Category.ProductsInThisCategory.Count > 10);
