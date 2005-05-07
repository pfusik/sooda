use Soodawind
go
delete from KeyGen;
insert into KeyGen
select 'Category',coalesce(max(CategoryID),0) + 1 from Categories
union
select 'Product',coalesce(max(ProductID),0) + 1 from Products
union
select 'Supplier',coalesce(max(SupplierID),0) + 1 from Suppliers
union
select 'Shipper',coalesce(max(ShipperID),0) + 1 from Shippers
union
select 'Employee',coalesce(max(EmployeeID),0) + 1 from Employees
union
select 'Order',coalesce(max(OrderID),0) + 1 from Orders

