<?
include "_layout.php";
write_start_page();
?>
		<h1>Introduction</h1>
		<p>
        <b>Sooda</b> (Simple Object-Oriented Data Access) is an Object-To-Relational Mapping software for the <a href="http://msdn.microsoft.com/net">.NET</a> environment.
        It lets you automatically generate an object-oriented data access layer (DAL) for your application.
        Instead of writing SQL code, you can now focus on writing business rules in object-oriented .NET languages.
		</p>
		<p>
        
		<p>
        Sooda is an open source software distributed under the terms of the <a href="../LICENSE.txt">BSD license</a>.
        <b>SoodaQuery</b> tool is distributed under the terms of <a href="../SoodaQuery_LICENSE.txt">GPL v2</a>, 
        because it makes use of the <a href="http://www.icsharpcode.net/OpenSource/SD/Default.aspx">ICSharpCode.TextEditor.dll</a> component which is GPL.
		</p>
		</p>
		<h2>Features</h2>
		<p>
        Sooda supports the following O/R features:</p>
		<ul>
            <li>transparent object materialization</li>
            <li>clean and natural syntax - the amount of boilerplate code is reduced to the absolute minimum, the code is readable and maintainable</li>
            <li>support for any data type supported by the .NET Framework</li>
            <li>natural mapping of one-to-many relations as referenced objects and collections. <a href="sample_onetomany.html">View Sample</a></li>
            <li>natural mapping of many-to-many relations as collections</li>
            <li>various models of object inheritance</li>
            <li>heterogenous data access (including RDBMS, XML files, Web services, LDAP &amp; AD directories)</li>
            <li>differential XML serialization and deserialization for moving data across layers</li>
            <li>optimistic locking and object versioning with collision detection</li>
			<li>support for long running transactions</li>
            <li>data caching with copy-on-write support</li>
		</ul>
        <p>Sooda supports <a href="http://msdn.microsoft.com/net">Microsoft .NET Framework</a> version 1.0 or later and
        <a href="http://www.go-mono.com">Mono</a>.</p>
<?php write_end_page(); ?>

