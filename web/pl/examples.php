<?
include "_layout.php";
include "../_format.php";
write_start_page();
?>
<h1>Przyk�ady</h1>
        Sooda udost�pnia bardzo wygodny spos�b manipulowania obiektami i relacjami przechowywanymi w bazie danych.
        Najpro�ciej pokaza� to na kilku przyk�adach:
        <h3>Dost�p do atrybut�w obiekt�w</h3>
        Wszystkie obiekty wyst�puj�ce w bazie danych odwzorowywane s� na obiekty klas .NET a kolumny w tabelach
        staj� si� w�a�ciwo�ciami tych obiekt�w. Dzi�ki temu dost�p do obiekt�w jest bardzo prosty, jak w poni�szym 
        przyk�adzie:
<?
format_csharp("../samples/sample1.cs");
?>

        <h3>Proste w u�yciu relacje</h3>
        Relacje pomi�dzy obiektami w bazie danych (zar�wno "<i>jeden do wielu</i>" jak i "<i>wiele do wielu</i>") 
        odwzorowane s� jako kolekcje obiekt�w implementuj�ce interfejs <span class="type">ICollection</span>.

        Dzi�ki temu wykonywanie podstawowych operacji na powi�zanych obiektach jest proste i intuicyjne. Dost�pne s�
        wszystkie standardowe metody, takie jak <span class="keyword">Add</span>, <span class="keyword">Remove</span>, 
        <span class="keyword">Contains</span>, iterowanie po kolekcjach przy 
        u�yciu <span class="keyword">foreach()</span> oraz indeksatory.
<?
format_csharp("../samples/sample2.cs");
?>

        <h3>Dziedziczenie i polimorfizm</h3>

        Mo�liwe jest odwzorowanie hierarchii klas C#/VB.NET w bazie danych i odwo�ywanie si� do 
        nich w spos�b polimorficzny.

<?
format_csharp("../samples/sample3.cs");
?>

        <h3>J�zyk zapyta�</h3>

        Mo�liwe jest wykorzystanie j�zyka Soql (Sooda Object Query Language), do �adowania obiekt�w z bazy danych. 
        J�zyk Soql w pe�ni wspiera wszystkie cechy Soody takie jak dziedziczenie, relacje 1-N oraz M-N i 
        wyra�enia �cie�kowe o dowolnej d�ugo�ci.

<?
format_csharp("../samples/sample4.cs");
?>
<?php write_end_page(); ?>
