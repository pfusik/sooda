<?
include "_layout.php";
include "../_format.php";
write_start_page();
?>
<h1>Przyk³ady</h1>
        Sooda udostêpnia bardzo wygodny sposób manipulowania obiektami i relacjami przechowywanymi w bazie danych.
        Najproœciej pokazaæ to na kilku przyk³adach:
        <h3>Dostêp do atrybutów obiektów</h3>
        Wszystkie obiekty wystêpuj¹ce w bazie danych odwzorowywane s¹ na obiekty klas .NET a kolumny w tabelach
        staj¹ siê w³aœciwoœciami tych obiektów. Dziêki temu dostêp do obiektów jest bardzo prosty, jak w poni¿szym 
        przyk³adzie:
<?
format_csharp("../samples/sample1.cs");
?>

        <h3>Proste w u¿yciu relacje</h3>
        Relacje pomiêdzy obiektami w bazie danych (zarówno "<i>jeden do wielu</i>" jak i "<i>wiele do wielu</i>") 
        odwzorowane s¹ jako kolekcje obiektów implementuj¹ce interfejs <span class="type">ICollection</span>.

        Dziêki temu wykonywanie podstawowych operacji na powi¹zanych obiektach jest proste i intuicyjne. Dostêpne s¹
        wszystkie standardowe metody, takie jak <span class="keyword">Add</span>, <span class="keyword">Remove</span>, 
        <span class="keyword">Contains</span>, iterowanie po kolekcjach przy 
        u¿yciu <span class="keyword">foreach()</span> oraz indeksatory.
<?
format_csharp("../samples/sample2.cs");
?>

        <h3>Dziedziczenie i polimorfizm</h3>

        Mo¿liwe jest odwzorowanie hierarchii klas C#/VB.NET w bazie danych i odwo³ywanie siê do 
        nich w sposób polimorficzny.

<?
format_csharp("../samples/sample3.cs");
?>

        <h3>Jêzyk zapytañ</h3>

        Mo¿liwe jest wykorzystanie jêzyka Soql (Sooda Object Query Language), do ³adowania obiektów z bazy danych. 
        Jêzyk Soql w pe³ni wspiera wszystkie cechy Soody takie jak dziedziczenie, relacje 1-N oraz M-N i 
        wyra¿enia œcie¿kowe o dowolnej d³ugoœci.

<?
format_csharp("../samples/sample4.cs");
?>
<?php write_end_page(); ?>
