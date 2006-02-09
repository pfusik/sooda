INTRODUCTION
============

This sample project demonstrates the basic usage of Sooda. It's based on a
standard Northwind sample database.

REQUIREMENTS
============

You need to have:

1. Visual Studio 2003
   OR
   NAnt 0.85 or later (free - available at http://nant.sourceforge.net)

2. SQL Server 2000, MSDE or SQL Server 2005 installed locally

3. Administrative access to SQL Server (required by the db setup scripts only).
   If you don't have admin rights, you'll have to set up the database manually.

ABOUT THE DATABASE
==================

The database is a modification of the standard Northwind database that comes 
with SQL Server 2000 with the following changes:

1. Removed IDENTITY columns. Sooda doesn't support them (yet ?).
2. Added KeyGen table 

SAMPLE PROJECTS
===============

Soodawind.Objects - is a business object library
Soodawind.ConsoleApp - is a sample console application that uses 
                       Soodawind.Objects

CREATING THE DATABASE
=====================

This is simple. Just spawn "install_database.bat" located in the "Database" 
directory and the database will be set up for you. If you don't have the 
admin rights, you may want to review 'database.sql' and run it by hand.

BUILDING SAMPLES USING VISUAL STUDIO 2003
=========================================

Just open 'Soodawind.sln' and rebuild the solution. Make sure to have
Sooda properly installed, otherwise VS.NET will not be able to locate the 
assemblies 
and/or find SoodaStubGen.exe utility.

BUILDING SAMPLES USING NANT
===========================

This is very simple. Just type 'nant' and the project should build itself to a 
directory under "build".

