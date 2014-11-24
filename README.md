Sooda - Simple Object-Oriented Data Access
==========================================

Sooda is an object-relational mapper (ORM) for the .NET Framework.
It has been used in production since 2005.

Sooda includes a code generator, which can generate the entire Data Access Layer for your application.
Each class of the DAL typically corresponds to a database table,
its properties map to database columns and relationships map to object references and collections.

Features
--------

* transparent object materialization
* support for most data types from the .NET Framework
* natural mapping of one-to-many relations as referenced objects and collections
* natural mapping of many-to-many relations as collections
* various models of object inheritance
* Language Integrated Query (LINQ) including non-restricted `Select` expressions and user-defined methods/properties
* dynamic fields that can be created at runtime
* differential XML serialization and deserialization for moving data across layers
* support for long-running transactions
* data caching with copy-on-write support

The following database engines are supported:

* Microsoft SQL Server - primary development platform, most thoroughly tested, production quality support
* Oracle 8i and higher - secondary development platform
* MySQL 4.x and higher - experimental support
* PostgreSQL 8.x - experimental support

Documentation
-------------

[Full documentation](http://sooda.sourceforge.net/documentation.html).

License
-------

Sooda is open source software distributed under the terms of the BSD license.
