<?
include "_layout.php";
include "../_format.php";
write_start_page();
?>
<h1>Examples</h1>
        Sooda provides an easy-to-use and convenient way to manipulate objects and relations stored in relational databases.
        It's best to demonstrate this with some examples:
        <h3>Accessing object attributes</h3>
        All database entities are mapped to .NET objects, where table columns are mapped to object properties.
        With this approach, object access becomes very easy, as shown in the following example:
<?
format_csharp("../samples/sample1.cs");
?>

        <h3>Easy-to-use relations</h3>
        Database relations (both "<i>one to many</i>" and "<i>many to many</i>") 
        are mapped as collections implementing <span class="type">ICollection</span> interface 
        and providing type-safe wrappers.

        This makes performing all basic operations simple and intuitive. All standard collection methods
        are available, namely <span class="keyword">Add</span>, <span class="keyword">Remove</span>, 
        <span class="keyword">Contains</span>, iterating with <span class="keyword">foreach()</span> and indexers.
<?
format_csharp("../samples/sample2.cs");
?>

        <h3>Inheritance and polymorphism</h3>

        It's possible to store map a complex C#/VB.NET class hierarchy with inheritance and access objects
        in a polymorphic manner.

<?
format_csharp("../samples/sample3.cs");
?>

        <h3>Query language</h3>

        Sooda provides an advanced query language called Soql (Sooda Object Query Language).
        You can use Soql to specify conditions to be used for object matching. This is similar to the
        <span class="keyword">where</span> clause in SQL. Soql fully supports all Sooda features, like 
        inheritance one-to-many and many-to-many relations, as well as path expressions of arbitrary length.

<?
format_csharp("../samples/sample4.cs");
?>

        <h3>Schema definition</h3>

        Sooda uses an XML-based schema definition. You provide an XML document that describes
		the mapping between the database (tables, columns, relations) and CLI world (classes, 
		properties, collections). By writing an appropriate schema you can use features like 
		column-renaming, lazy-loading, split-classes, inheritance, collections, enumerations 
		and so on.
<?
# format_xml("../samples/sample5.xml");
?>
<?php write_end_page(); ?>
