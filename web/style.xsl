<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="mode">plain</xsl:param>

    <xsl:variable name="page_id" select="/*[position()=1]/@id" />

    <xsl:output method="xml" 
        indent="no" 
        omit-xml-declaration="yes"
        doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" 
        doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />

    <xsl:template match="/">
        <html>
            <head>
                <link rel="stylesheet" href="style2.css" type="text/css" />
                <link rel="stylesheet" href="syntax.css" type="text/css" />
                <link rel="stylesheet" href="custom.css" type="text/css" />
                <title>Sooda<xsl:if test="$mode = 'web'"> - <xsl:value-of select="document('webmenu.xml')/common/navigation/nav[@href=$page_id]/@label" /></xsl:if></title>
                <meta name="keywords" content="Sooda, O/R mapping, .NET, C#, object relational mapper, persistence, open source, simple object oriented data access, database, sql, soql" />
                <meta name="author" content="Jaroslaw Kowalski" />
                <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
                <meta name="description" content="Sooda is an open source object-to-relational mapping software providing easy-to-use API to create,read,search,update and delete objects without the use of SQL." />
            </head>
            <body>
                <div id="header"><img src="sooda.jpg" alt="Sooda - Simple Object-Oriented Data Access" /></div>
                <xsl:if test="$mode = 'web'">
                    <div id="controls">
                        <xsl:apply-templates select="document('webmenu.xml')/common/navigation" />
                    </div>
                </xsl:if>
                <div id="{$mode}content">
                    <xsl:apply-templates select="content" />
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="section">
        <p>
            <xsl:attribute name="class">heading<xsl:apply-templates select="." mode="section-level" /></xsl:attribute>
            <xsl:apply-templates select="." mode="section-number" />
            <a name="{@id}" id="{@id}">.</a>
            <xsl:apply-templates select="title" />
        </p>
        <div>
            <xsl:attribute name="class">body<xsl:apply-templates select="." mode="section-level" /></xsl:attribute>
            <xsl:choose>
                <xsl:when test="count(body/*)=0 and count(section)=0">
                    <xsl:call-template name="todo" />
                </xsl:when>
                <xsl:otherwise><xsl:apply-templates select="body|section" /></xsl:otherwise>
            </xsl:choose>
        </div>
    </xsl:template>

    <xsl:template match="title">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="section" mode="section-number">
        <xsl:variable name="otherSections" select="../section" />
        <xsl:variable name="thisSection" select="generate-id(.)" />
        <xsl:variable name="suffix" />

        <xsl:variable name="parent" select="ancestor::section[position()=1]" />
        <xsl:if test="$parent"><xsl:apply-templates select="$parent" mode="section-number" />.</xsl:if>
        <xsl:for-each select="$otherSections">
            <xsl:if test="generate-id(.) = $thisSection">
                <xsl:value-of select="position()" />
            </xsl:if>
        </xsl:for-each>
    </xsl:template>

    <xsl:template match="section" mode="section-sort-ordinal">
        <xsl:variable name="otherSections" select="../section" />
        <xsl:variable name="thisSection" select="generate-id(.)" />
        <xsl:variable name="suffix" />

        <xsl:variable name="parent" select="ancestor::section[position()=1]" />
        <xsl:if test="$parent"><xsl:apply-templates select="$parent" mode="section-sort-ordinal" /></xsl:if>
        <xsl:for-each select="$otherSections">
            <xsl:if test="generate-id(.) = $thisSection">
                <xsl:if test="position() &lt; 100">0</xsl:if>
                <xsl:if test="position() &lt; 10">0</xsl:if>
                <xsl:value-of select="position()" />
            </xsl:if>
        </xsl:for-each>
    </xsl:template>

    <xsl:template match="sectionlink">
        <xsl:variable name="sectid" select="@id" />
        <xsl:variable name="sect" select="//section[@id=$sectid]" />

        <xsl:variable name="this_section" select="ancestor-or-self::section[position()=1]" />

        <xsl:variable name="this_sort_ordinal">1<xsl:apply-templates select="$this_section" mode="section-sort-ordinal" /></xsl:variable>
        <xsl:variable name="target_sort_ordinal">1<xsl:apply-templates select="$sect" mode="section-sort-ordinal" /></xsl:variable>

        <xsl:variable name="number_this_sort_ordinal" select="number(substring(concat($this_sort_ordinal,'000000000000000'),1,16))" />
        <xsl:variable name="number_target_sort_ordinal" select="number(substring(concat($target_sort_ordinal,'000000000000000'),1,16))" />


        <!--
        <xsl:choose>
                <xsl:when test="$number_this_sort_ordinal &lt; $number_target_sort_ordinal">later</xsl:when>
                <xsl:otherwise>earlier</xsl:otherwise>
            </xsl:choose>
            -->
        in section "<a href="#{@id}">
        <xsl:apply-templates select="$sect" mode="section-number" />.&#160;<xsl:apply-templates select="$sect/title" />
        </a>"
        <xsl:if test="not($sect)">
            <xsl:message terminate="yes">No such anchor: '<xsl:value-of select="@id" />'</xsl:message>
        </xsl:if>
        <xsl:if test="count($sect) != 1">
            <xsl:message terminate="yes">More than one anchor: <xsl:value-of select="@id" /></xsl:message>
        </xsl:if>
    </xsl:template>

    <xsl:template match="section" mode="section-level">
        <xsl:variable name="otherSections" select="../section" />
        <xsl:variable name="thisSection" select="generate-id(.)" />
        <xsl:variable name="suffix" />

        <xsl:variable name="parent" select="ancestor::section[position()=1]" />
        <xsl:choose>
            <xsl:when test="$parent">
                <xsl:variable name="parentLevel">
                    <xsl:apply-templates select="$parent" mode="section-level" />
                </xsl:variable>
                <xsl:value-of select="$parentLevel + 1" />
            </xsl:when>
            <xsl:otherwise>1</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="table-of-contents">
        <table class="toc">
            <xsl:apply-templates select="//section" mode="toc">
                <xsl:with-param name="maxLevel">
                    <xsl:choose>
                        <xsl:when test="@maxLevel"><xsl:value-of select="@maxLevel" /></xsl:when>
                        <xsl:otherwise>3</xsl:otherwise>
                    </xsl:choose>
                </xsl:with-param>
            </xsl:apply-templates>
        </table>
    </xsl:template>

    <xsl:template match="last-modified-date">
        <xsl:variable name="lastUpdated"><xsl:value-of select="substring(.,18,20)" /></xsl:variable>
        <xsl:if test="string-length($lastUpdated)=20"><xsl:value-of select="$lastUpdated" /></xsl:if>
    </xsl:template>

    <xsl:template match="subversion-revision">
        <xsl:value-of select="substring-before(substring-after(.,'Revision:'),' $')" />
    </xsl:template>

    <xsl:template match="section" mode="toc">
        <xsl:param name="maxLevel">3</xsl:param>
        <xsl:variable name="level"><xsl:apply-templates select="." mode="section-level" /></xsl:variable>

        <xsl:if test="$level &lt;= $maxLevel">
            <tr>
                <td class="toc{$level}">
                    <xsl:apply-templates select="." mode="section-number" />.

                    <a href="#{@id}"><xsl:apply-templates select="title" /></a>
                </td>
            </tr>
        </xsl:if>
    </xsl:template>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="content">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="navigation">
        <table border="0" cellpadding="0" cellspacing="0" class="navtable">
            <xsl:apply-templates select="nav" />
            <tr class="navtablespacer"><td><a class="nav" href="http://sourceforge.net/projects/sooda"><img src="http://sflogo.sourceforge.net/sflogo.php?group_id=71422&amp;type=4" width="125" height="37" border="0" alt="Get Sooda - Simple Object Oriented Data Access at SourceForge.net. Fast, secure and Free Open Source software downloads" /></a></td></tr>
        </table>
    </xsl:template>

    <xsl:template match="nav">
        <xsl:choose>
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
                <tr><td class="nav"><a class="nav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(contains(@href, '/'))">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
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

    <xsl:template match="code[@lang]">
        <pre class="{@lang}">
            <xsl:apply-templates />
        </pre>
    </xsl:template>

    <xsl:template match="code[@lang='C#']">
        <pre class="csharp">
            <xsl:apply-templates />
        </pre>
    </xsl:template>

    <xsl:template match="a[starts-with(@href,'#')]">
        <xsl:variable name="sectid" select="substring-after(@href,'#')" />
        <xsl:variable name="sect" select="//section[@id=$sectid]" />
        <a href="{@href}">
            <xsl:apply-templates />
        </a>
        <xsl:if test="$sect"> (<img src="rightarrow.gif" alt="Section" /><xsl:apply-templates select="$sect" mode="section-number" />)
        </xsl:if>
        <xsl:if test="not($sect)">
            <xsl:message terminate="yes">No such anchor: '<xsl:value-of select="@href" />'</xsl:message>
        </xsl:if>
        <xsl:if test="count($sect) != 1">
            <xsl:message terminate="yes">More than one anchor: <xsl:value-of select="@href" /></xsl:message>
        </xsl:if>
    </xsl:template>

    <xsl:template match="a[starts-with(@href,'http://')]">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
        </xsl:copy>
        <span class="out_link_address">&#160;(<xsl:value-of select="@href" />)</span>
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

    <xsl:template match="body">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template name="todo" match="todo">
        <p style="color: red">TODO: Write me</p>
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
            <a>
                <xsl:attribute name="name"><xsl:value-of select="@id" /></xsl:attribute>
                <xsl:value-of select="position()" />. 
                <xsl:value-of select="@title" />
            </a>
        </h5>
        <xsl:apply-templates />
    </xsl:template>

</xsl:stylesheet>
