<?php
include "_layout.php";
write_start_page();
?>
		<h1>Wprowadzenie</h1>
		<p>
		System <b>Sooda</b> (Simple Object-Oriented Data Access) pozwala na automatyczne generowanie 
		obiektowej warstwy dostêpu do danych (Data Access Layer) dla aplikacji dzia³aj¹cych
		na platformie .NET.
		</p>
		<p>
		Sooda jest oprogramowaniem open source dystrybuowanym 
        na zasadach <a href="../LICENSE.txt">licencji BSD</a>. Narzêdzie <b>SoodaQuery</b> dystrybuowane jest
        na zasadach licencji <a href="../SoodaQuery_LICENSE.txt">GPL v2</a>, poniewa¿ wykorzystuje komponent 
        <a href="http://www.icsharpcode.net/OpenSource/SD/Default.aspx">ICSharpCode.TextEditor.dll</a>.
		</p>
		<h2>Mo¿liwoœci systemu</h2>
		<p>
		System udostêpnia nastêpuj¹ce mechanizmy wspieraj¹ce tworzenie rozwi¹zañ bazodanowych:</p>
		<ul>
			<li>automatyczne materializowanie obiektów bazodanowych jako obiektów w œrodowisku zarz¹dzanym</li>
			<li>obs³uga wszystkich prostych typów danych wspieranych przez CLI + mo¿liwoœæ definiowania w³asnych typów i sposobów odwzorowania</li>
			<li>naturalne odwzorowanie relacji 1-N w formie obiektów powi¹zanych i kolekcji</li>
			<li>naturalne odwzorowanie relacji N-N w formie kolekcji obiektów</li>
			<li>przezroczysty dostêp do heterogenicznych Ÿróde³ danych (RDBMS, pliki XML, katalogi LDAP i ActiveDirectory)</li>
			<li>serializacja i deserializacja do formatu XML</li>
			<li>optymistyczne blokowanie i wersjonowanie obiektów w pamiêci</li>
			<li>wsparcie dla d³ugo dzia³aj¹cych transakcji (<i>long running transactions</i>)</li>
		</ul>
		<p>Sooda wspiera <a href="http://msdn.microsoft.com/net">Microsoft .NET Framework</a> w wersji 1.x, mo¿liwa jest tak¿e kompilacja 
		z u¿yciem <a href="http://www.go-mono.com">Mono</a>, lecz nie wszystkie funkcjonalnoœci
		dzia³aj¹ w tym œrodowisku.</p>
<?php
write_end_page();
?>			
