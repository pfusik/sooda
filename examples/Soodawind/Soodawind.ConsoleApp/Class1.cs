using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

using Sooda;
using Soodawind.Objects;

[assembly: SoodaStubAssembly(typeof(_DatabaseSchema))]

namespace Soodawind.ConsoleApp
{
	class Class1
	{
        static void DisplayProductsInCategories()
        {
            foreach (Category c in Category.GetList(SoodaWhereClause.Unrestricted))
            {
                Console.WriteLine("Category: {0}. Number of products: {1}", c.Name, c.Products.Count);
                Console.WriteLine("Products sorted by name:");
                foreach (Product p in c.Products.Sort(SoodaOrderBy.Ascending("Name").GetComparer()))
                {
                    Console.WriteLine("   '{0}' from '{1}'", p.Name, p.Supplier.Company);
                }
                Console.WriteLine();


            }
            //Console.WriteLine(Category.Load(1).Name);
        }

        static void DisplaySuppliersFrom(string countryName)
        {
            SupplierList suppliers = Supplier.GetList(
                new SoodaWhereClause("Country = {0}", countryName));

            Console.WriteLine("Companies from {0}. Total: {1}", countryName, suppliers.Count);
            Console.WriteLine();
            foreach (Supplier s in suppliers)
            {
                Console.WriteLine("\"{0}\" {1} {2} {3} {4}", s.Company, s.Address, s.PostalCode, s.City, s.Region.IsNull ? "" : s.Region.Value);
                Console.WriteLine("Phone: {0} Fax: {1}", s.Phone, s.Fax);

                // because Supplier is an object, we can even call methods on it!

                if (s.HasHomePage())
                {
                    Console.WriteLine("HomePage: {0}", s.HomePage);
                }
                else
                {
                    Console.WriteLine("Company doesn't have a homepage.");
                }
                Console.WriteLine("Contact: {0} - {1}", s.ContactName, s.ContactTitle);
                Console.WriteLine("Products supplied by this company: {0}", s.Products.Count);
                foreach (Product p in s.Products)
                {
                    Console.WriteLine("   {0}", p.Name);
                }
                Console.WriteLine("---");
            }
        }

        static void DisplayShippers()
        {
            Console.WriteLine("*** Shipper list: ");
            foreach (Shipper s in Shipper.GetList(SoodaWhereClause.Unrestricted))
            {
                Console.WriteLine("shipper: {0}", s.Company);
            }
            Console.WriteLine();
        }

        static void InsertAndDelete()
        {
            int newShipperID;

            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                DisplayShippers(); 

                Console.WriteLine("Adding new shipper...");
                // create a new Shipper
                Shipper s = new Shipper();
                newShipperID = s.ID;

                s.Company = "New shipper company";
                s.Phone = "123456";

                // display shippers again - it is present in the output

                DisplayShippers();

                transaction.Commit();
            }

            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                Console.WriteLine("Deleting newly created shipper...");
                Shipper.Load(newShipperID).MarkForDelete();
                transaction.Commit();
            }

            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                Console.WriteLine("New shipper count: {0}", Shipper.GetList(SoodaWhereClause.Unrestricted).Count);
                DisplayShippers();
                transaction.Commit();
            }
        }

        static void Update()
        {
            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                Category.Seafood.Name = "Water creatures";
                Category.Beverages.Name = "Liquids";

                Console.WriteLine("The new name of the Seafood category is: {0}", Category.Seafood.Name);
                Console.WriteLine("The new name of the Beverages category is: {0}", Category.Beverages.Name);
                Console.WriteLine();

                // we modified the category name in memory
                // GetList will save the changes temporarily to the
                // database so that the following will work

                foreach (Product p in Product.GetList(new SoodaWhereClause("Category.Name in ({0},{1})", "Water creatures", "Liquids"), SoodaOrderBy.Ascending("Product.Name")))
                {
                    Console.WriteLine("product: {0} category: {1}", p.Name, p.Category.Name);
                }

                // we don't commit here so that the in-memory changes will
                // not be committed to the database. The new transaction will
                // be using "Seafood" and "Beverages" respectively
            }

            Console.WriteLine();
            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                Console.WriteLine("The name of the Seafood category is: {0}", Category.Seafood.Name);
                Console.WriteLine("The name of the Beverages category is: {0}", Category.Beverages.Name);
                Console.WriteLine();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                DisplayProductsInCategories();

                Console.WriteLine("Number of beverages: {0}", 
                    Category.Beverages.Products.Count);

                DisplaySuppliersFrom("Germany");
                DisplaySuppliersFrom("Poland");
                DisplaySuppliersFrom("UK");
            }

            InsertAndDelete();
            Update();
        }
	}
}
