<?php

$menu = array(
	array( "url" => "index", "label" => "Wprowadzenie" ),
	array( "url" => "quickstart", "label" => "Szybki start" ),
	array( "url" => "components", "label" => "Komponenty" ),
	array( "url" => "tools", "label" => "Narzędzia" ),
	array( "url" => "examples", "label" => "Przykłady" ),
	array( "url" => "screenshots", "label" => "Zrzuty ekranu" ),
	array( "url" => "download", "label" => "Pobierz" ),
	array( "url" => "development", "label" => "Rozwój" ),
	array( "url" => "faq", "label" => "FAQ" ),
	array( "url" => "links", "label" => "Linki" )
);

global $menu;

include '../_menu.php';

function write_start_page()
{
?>
<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
		<title>Sooda</title>
		<link rel="stylesheet" href="../style.css" type="text/css" />
		<script language="javascript" src="../sooda.js"></script>
		<meta content="text/html; charset=windows-1250" http-equiv="Content-Type"></meta>
	</head>
    <body>
	
        <table width="800" cellpadding="0" cellspacing="0" border="0" align="center" valign="top">
            <tr>
                <td class="logo"><img src="../sooda_small.gif" /></td>
                <td class="upperinfo" align="right" class="titledesc" colspan="2">
				<a title="English site" href="../en/"><img class="thinborder" src="../lang_en.gif"></a><br/>
                    Simple Object-Oriented Data Access<br/>Copyright (c) 2003-2004 by Jarosław Kowalski</td>
            </tr>
			<tr>
			<td valign="top" width="200" class="nav">
<?php write_menu() ?>
<? if (strstr($_SERVER["SERVER_NAME"], "sourceforge.net") != false || strstr($_SERVER["SERVER_NAME"], "sf.net") != false) { ?>				
				<br/><a href="http://sourceforge.net"><img src="http://sourceforge.net/sflogo.php?group_id=71422&amp;type=1" width="88" height="31" border="0" alt="SourceForge.net Logo" /></a>
<? } ?>
		</td>
		<td colspan="2" valign="top" width="100%" class="content">
<?php
}

function write_end_page()
{
?>
<br>
		</td>
		</tr>
		</table>
	</body>
</html>
<?php
}
?>
