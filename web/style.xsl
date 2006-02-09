<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:param name="page_id_override"></xsl:param>
    <xsl:param name="subpage_id_override"></xsl:param>
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="mode">web</xsl:param>

    <xsl:variable name="page_id" select="concat(/*[position()=1]/@id,$page_id_override)" />
    <xsl:variable name="subpage_id" select="concat(/*[position()=1]/@subid,$subpage_id_override)" />
    <xsl:variable name="common" select="document(concat($mode,'menu.xml'))" />
    
    <xsl:output method="xml" 
        indent="no" 
        doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" 
        doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />

    <xsl:template match="/">
        <html>
            <head>
                <link rel="stylesheet" href="style2.css" type="text/css" />
                <link rel="stylesheet" href="syntax.css" type="text/css" />
                <title>Sooda - <xsl:value-of select="$common/common/navigation/nav[@href=$page_id]/@label" />
                    <xsl:if test="$subpage_id"> - <xsl:value-of select="$common/common/navigation//subnav[@href=$subpage_id]/@label" /></xsl:if></title>
                <meta name="keywords" content="Sooda, O/R mapping, .NET, C#, object relational mapper, persistence, open source, simple object oriented data access, database, sql, soql" />
                <meta name="author" content="Jaroslaw Kowalski" />
                <meta name="description" content="Sooda is an open source object-to-relational mapping software providing easy-to-use API to create,read,search,update and delete objects without the use of SQL." />
            </head>
            <body>
                <div id="header"><img src="sooda.jpg" alt="Sooda - Simple Object-Oriented Data Access" /></div>
                <div id="controls">
                    <xsl:call-template name="controls" />
                </div>
                <div id="content">
                    <span class="underconstruction">
                        This web site is under construction. Some sections may be missing or not up-to-date.
                    </span>
                    <xsl:apply-templates select="content" />
                </div>
                <div id="googlesearch">
                    <!-- SiteSearch Google -->
                    <form method="get" action="http://www.google.com/custom" target="_top">
                        <table border="0">
                            <tr><td nowrap="nowrap" valign="top" align="left" height="32">
<input type="hidden" name="domains" value="www.sooda.org"></input>
<input type="text" name="q" size="20" maxlength="255" value=""></input>
<input type="submit" name="sa" value="Google Search"></input>
</td></tr>
<tr>
<td nowrap="nowrap">
<table>
<tr>
<td>
<input type="radio" name="sitesearch" value=""></input>
<font size="-1" color="#000000">Web</font>
</td>
<td>
<input type="radio" name="sitesearch" value="www.sooda.org" checked="checked"></input>
<font size="-1" color="#000000">www.sooda.org</font>
</td>
</tr>
</table>
<input type="hidden" name="client" value="pub-2535373996863248"></input>
<input type="hidden" name="forid" value="1"></input>
<input type="hidden" name="ie" value="UTF-8"></input>
<input type="hidden" name="oe" value="UTF-8"></input>
<input type="hidden" name="cof" value="GALT:#0066CC;GL:1;DIV:#999999;VLC:336633;AH:center;BGC:FFFFFF;LBGC:FF9900;ALC:0066CC;LC:0066CC;T:000000;GFNT:666666;GIMP:666666;FORID:1;"></input>
<input type="hidden" name="hl" value="en"></input>
</td></tr></table>
</form>
<!-- SiteSearch Google -->
                </div>
                <div id="googleads">
                    <script type="text/javascript"><xsl:comment>
                            google_ad_client = "pub-2535373996863248";
                            google_ad_width = 120;
                            google_ad_height = 600;
                            google_ad_format = "120x600_as";
                            google_ad_type = "text_image";
                            google_ad_channel ="";
                            google_color_border = "5290ee";
                            google_color_bg = "FFFFFF";
                            google_color_link = "0000FF";
                            google_color_url = "008000";
                            google_color_text = "000000";
                            //</xsl:comment></script>
                    <script type="text/javascript"
                        src="http://pagead2.googlesyndication.com/pagead/show_ads.js">
                    </script>
                    </div>
                </body>
            </html>
        </xsl:template>

        <xsl:template match="@* | node()">
            <xsl:copy>
                <xsl:apply-templates select="@* | node()" />
            </xsl:copy>
        </xsl:template>

        <xsl:template match="content">
            <xsl:apply-templates />
        </xsl:template>

        <xsl:template name="controls">
            <xsl:apply-templates select="$common/common/navigation" />
        </xsl:template>

        <xsl:template match="navigation">
            <table border="0" cellpadding="0" cellspacing="0" class="navtable">
                <xsl:apply-templates select="nav" />
                <tr>
                    <td class="logobutton">
                        <a href="http://www.cenqua.com/clover.net"><img src="http://www.cenqua.com/images/cloverednet1.gif" width="89" height="33" border="0" alt="Code Coverage by Clover.NET"/></a>
                    </td>
                </tr>
                <tr><td class="logobutton">
                        <!-- Start of StatCounter Code -->
                        <script type="text/javascript" language="javascript">
                            var sc_project=575055; 
                            var sc_partition=4; 
                            var sc_security="e249d6a5"; 
                        </script>

                        <script type="text/javascript" language="javascript" src="http://www.statcounter.com/counter/counter.js"></script><noscript><a href="http://www.statcounter.com/" target="_blank"><img  src="http://c5.statcounter.com/counter.php?sc_project=575055&amp;amp;java=0&amp;amp;security=e249d6a5" alt="free web stats" border="0" /></a> </noscript>
                        <!-- End of StatCounter Code -->
                </td></tr>
                <tr><td class="logobutton">
                        <a href="http://validator.w3.org/check?uri=referer"><img
                                src="http://www.w3.org/Icons/valid-xhtml10"
                                border="0" 
                                alt="Valid XHTML 1.0 Transitional" height="31" width="88" /></a>
                    </td>
                </tr>  
                <!--
                <tr>
                    <td class="logobutton" style="font-family: Tahoma; color: white; font-size: 10px">Copyright (c) 2003-2006 by <a style="color: white" href="mailto:jaak@jkowalski.net">Jaroslaw Kowalski</a></td>
                </tr>
                -->
                <tr class="navtablespacer"><td></td></tr>
            </table>
        </xsl:template>

        <xsl:template match="nav">
            <xsl:choose>
                <xsl:when test="$page_id = @href and subnav">
                    <tr>
                        <td class="nav_selected">
                            <table class="submenu" cellpadding="0" cellspacing="0">
                                <tr><td>
                                        <a class="nav_selected">
                                            <xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute>
                                            <xsl:value-of select="@label" />
                                        </a>
                                </td></tr>
                                <xsl:if test="subnav">
                                    <xsl:apply-templates select="subnav" />
                                </xsl:if>
                            </table>
                        </td>
                    </tr>
                </xsl:when>
                <xsl:when test="$page_id = @href">
                    <tr>
                        <td class="nav_selected">
                            <a class="nav_selected">
                                <xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute>
                                <xsl:value-of select="@label" />
                        </a>
                    </td>
                </tr>
            </xsl:when>
            <xsl:otherwise>
                <tr><td class="nav"><a class="nav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(@noext)">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="subnav">
        <xsl:choose>
            <xsl:when test="$subpage_id = @href"><tr class="subnav"><td><a class="subnav_selected" href="{@href}.{$file_extension}"><xsl:value-of select="@label" /></a></td></tr></xsl:when>
            <xsl:otherwise>
                <tr class="subnav"><td><a class="subnav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(@noext)">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="link">
        <a href="{@href}.{$file_extension}"><xsl:apply-templates /></a>
    </xsl:template>

    <xsl:template match="x">
        <xsl:apply-templates mode="xml-example" />
    </xsl:template>

    <xsl:template match="commandline">
        <code class="commandline">
            <xsl:apply-templates />
        </code>
    </xsl:template>

    <xsl:include href="syntax.xsl" />

    <xsl:template match="*" mode="xml-example">
        <xsl:choose>
            <xsl:when test="count(descendant::node()) = 0">
                <span class="xmlbracket">&lt;</span>
                <span class="xmlelement"><xsl:value-of select="name()" /></span>
                <xsl:apply-templates select="@*" mode="xml-example" />
                <span class="xmlbracket"> /&gt;</span>
            </xsl:when>
            <xsl:otherwise>
                <span class="xmlbracket">&lt;</span>
                <span class="xmlelement"><xsl:value-of select="name()" /></span>
                <xsl:apply-templates select="@*" mode="xml-example" />
                <span class="xmlbracket">&gt;</span>
                <xsl:apply-templates mode="xml-example" />
                <span class="xmlbracket">&lt;/</span>
                <span class="xmlelement"><xsl:value-of select="name()" /></span>
                <span class="xmlbracket">&gt;</span>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="@*[name()='xml:space']" mode="xml-example"></xsl:template>
    <xsl:template match="attribute::*" mode="xml-example"><xsl:text> </xsl:text><span class="xmlattribute"><xsl:value-of select="name()"/></span><span class="xmlpunct">=</span><span class="xmlattribtext">"<xsl:value-of select="." />"</span></xsl:template>

    <xsl:template match="comment()" mode="xml-example">
        <span class="xmlcomment">&lt;!--<xsl:value-of select="." />--&gt;</span>
    </xsl:template>
    <xsl:template match="node()" mode="xml-example" priority="-10">
        <xsl:copy>
            <xsl:value-of select="namespace::*" />
            <xsl:apply-templates mode="xml-example" />
        </xsl:copy>
    </xsl:template>

    <!-- FAQ -->

    <xsl:template match="faq-index">
        <ol>
            <xsl:apply-templates select="//faq" mode="faq-index" />
        </ol>
    </xsl:template>

    <xsl:template match="faq-answers">
        <xsl:apply-templates select="faq" mode="faq-body" />
    </xsl:template>

    <xsl:template match="faq" mode="faq-index">
        <li><a>
                <xsl:attribute name="href">#<xsl:value-of select="@id" /></xsl:attribute>
                <xsl:value-of select="@title" />
            </a>
        </li>
    </xsl:template>

    <xsl:template match="faq" mode="faq-body">
        <hr />
        <h5>
            <a><xsl:value-of select="position()" />. 
                <xsl:attribute name="name"><xsl:value-of select="@id" /></xsl:attribute>
                <xsl:value-of select="@title" />
            </a>
        </h5>
        <xsl:apply-templates />
    </xsl:template>
</xsl:stylesheet>
