<?php

$menu = array(
	array( "url" => "index", "label" => "Introduction" ),
    array( "url" => "quickstart", "label" => "Quick Start" ),
	array( "url" => "components", "label" => "Components" ),
	array( "url" => "tools", "label" => "Tools" ),
	array( "url" => "examples", "label" => "Examples" ),
	array( "url" => "screenshots", "label" => "Screenshots" ),
	array( "url" => "download", "label" => "Download" ),
	array( "url" => "development", "label" => "Development" ),
	array( "url" => "faq", "label" => "FAQ" ),
	array( "url" => "links", "label" => "Links" )
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
		<meta content="text/html; charset=iso-8859-1" http-equiv="Content-Type"></meta>
	</head>
    <body>
	
        <table width="800" cellpadding="0" cellspacing="0" border="0" align="center" valign="top">
            <tr>
                <td class="logo"><img src="../sooda_small.gif" /></td>
                <td class="upperinfo" align="right" class="titledesc" colspan="2">
                    <a title="Strona po polsku" href="../pl/"><img class="thinborder" src="../lang_pl.gif"></a><br/>
                    Simple Object-Oriented Data Access<br/>Copyright (c) 2003-2004 by Jaroslaw Kowalski</td>
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
