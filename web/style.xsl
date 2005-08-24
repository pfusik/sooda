<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="sourceforge">0</xsl:param>
    <xsl:param name="mode">web</xsl:param>

    <xsl:variable name="common_file" select="concat($mode, 'menu.xml')" />
    <xsl:variable name="page_id" select="/*[position()=1]/@id" />
    <xsl:variable name="common" select="document($common_file)" />

    <xsl:output method="xml" indent="no" />

    <xsl:template match="/">
        <html>
            <head>
                <link rel="stylesheet" href="style.css" type="text/css" />
                <title>Sooda</title>
            </head>
            <body width="100%">
                <table align="center" class="page" cellpadding="0" cellspacing="0">
                    <tr>
                        <td class="header" colspan="2"><img src="titlebanner.jpg" /></td>
                    </tr>
                    <tr>
                        <td valign="top" class="controls">
                            <xsl:call-template name="controls" />
                        </td>
                        <td valign="top" align="left" class="content">
                            <xsl:apply-templates select="content" />
                        </td>
                    </tr>
                    <tr>
                        <td class="hostedby">
                            <xsl:if test="$sourceforge='1'">
                                <!-- Start of StatCounter Code -->
                                <script type="text/javascript" language="javascript">
                                    var sc_project=575055; 
                                    var sc_partition=4; 
                                    var sc_security="e249d6a5"; 
                                </script>

                                <script type="text/javascript" language="javascript" src="http://www.statcounter.com/counter/counter.js"></script><noscript><a href="http://www.statcounter.com/" target="_blank"><img  src="http://c5.statcounter.com/counter.php?sc_project=575055&amp;amp;java=0&amp;amp;security=e249d6a5" alt="free web stats" border="0" /></a> </noscript>
                                <!-- End of StatCounter Code -->
                                <br/>
                                <a href="http://sourceforge.net"><img src="http://sourceforge.net/sflogo.php?group_id=71422&amp;type=1" width="88" height="31" border="0" alt="SourceForge.net Logo" /></a>
                            </xsl:if>
                        </td>                                                                         
                        <td class="copyright">Copyright (c) 2003-2005 by Jarosław Kowalski <img width="1" height="1"><xsl:attribute name="src">http://jaak.sav.net/transpixel.gif</xsl:attribute></img></td>
                    </tr>
                </table>
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
        <xsl:apply-templates select="nav" />
        <p/>
        <a href="http://www.cenqua.com/clover.net"><img src="http://www.cenqua.com/images/cloverednet1.gif" width="89" height="33" border="0" alt="Code Coverage by Clover.NET"/></a>
    </xsl:template>
    
    <xsl:template match="nav">
        <xsl:choose>
            <xsl:when test="$page_id = @href"><a class="nav_selected"><xsl:value-of select="@label" /></a></xsl:when>
            <xsl:otherwise>
                <a class="nav"><xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute><xsl:value-of select="@label" /></a>
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
