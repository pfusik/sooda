<?php

    function replace_type($keyword, $text)
    {
        return preg_replace("/\\b($keyword)\\b/", "span_class_equals_type\$1end_of_span", $text);
    }

    function replace_keyword($keyword, $text)
    {
        return preg_replace("/\\b($keyword)\\b/", "span_class_equals_keyword\$1end_of_span", $text);
    }

    function strip_spans($pre,$s,$post)
    {
        $s = str_replace("span_class_equals_number", "", $s);
        $s = str_replace("span_class_equals_type", "", $s);
        $s = str_replace("span_class_equals_keyword", "", $s);
        $s = str_replace("span_class_equals_operator", "", $s);
        $s = str_replace("span_class_equals_string", "", $s);
        $s = str_replace("end_of_span", "", $s);
        $s = str_replace("end_of_string_span", "", $s);
        $s = str_replace("end_of_comment_span", "", $s);

        return $pre . $s . $post;
    }
        
    function format_csharp($filename)
    {
        $alltext = "";
        $basedir = dirname($_SERVER["SCRIPT_FILENAME"]);
        $fd = fopen("$basedir/$filename", "r");

        while ($line=fgets($fd,1000))
        {
            $alltext.=$line;
        }

        // keywords

        $alltext = str_replace("\r", "", $alltext);

        $alltext = str_replace(";", "span_class_equals_operator;end_of_span", $alltext);
        $alltext = str_replace("&", "span_class_equals_operator&end_of_span", $alltext);
        $alltext = str_replace("'", "&apos;", $alltext);
        $alltext = str_replace("|", "span_class_equals_operator|end_of_span", $alltext);
        $alltext = str_replace("<", "span_class_equals_operator&lt;end_of_span", $alltext);
        #$alltext = str_replace(">", "span_class_equals_operator'>&gt;end_of_span", $alltext);
		#$alltext = str_replace(" ", "&nbsp;", $alltext);
        $alltext = str_replace("=", "span_class_equals_operator=end_of_span", $alltext);
		#$alltext = str_replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;", $alltext);

        $alltext = replace_keyword("class", $alltext);
        $alltext = replace_keyword("using", $alltext);
        $alltext = replace_keyword("namespace", $alltext);
        $alltext = replace_keyword("struct", $alltext);
        $alltext = replace_keyword("interface", $alltext);
        $alltext = replace_keyword("delegate", $alltext);
        $alltext = replace_keyword("enum", $alltext);
        $alltext = replace_keyword("readonly", $alltext);
        $alltext = replace_keyword("virtual", $alltext);
        $alltext = replace_keyword("override", $alltext);
        $alltext = replace_keyword("extern", $alltext);
        $alltext = replace_keyword("unsafe", $alltext);
        $alltext = replace_keyword("static", $alltext);
        $alltext = replace_keyword("public", $alltext);
        $alltext = replace_keyword("protected", $alltext);
        $alltext = replace_keyword("internal", $alltext);
        $alltext = replace_keyword("private", $alltext);
        $alltext = replace_keyword("abstract", $alltext);
        $alltext = replace_keyword("sealed", $alltext);
        $alltext = replace_keyword("if", $alltext);
        $alltext = replace_keyword("else", $alltext);
        $alltext = replace_keyword("switch", $alltext);
        $alltext = replace_keyword("break", $alltext);
        $alltext = replace_keyword("continue", $alltext);
        $alltext = replace_keyword("return", $alltext);
        $alltext = replace_keyword("case", $alltext);
        $alltext = replace_keyword("for", $alltext);
        $alltext = replace_keyword("do", $alltext);
        $alltext = replace_keyword("while", $alltext);
        $alltext = replace_keyword("foreach", $alltext);
        $alltext = replace_keyword("this", $alltext);
        $alltext = replace_keyword("base", $alltext);
        $alltext = replace_keyword("super", $alltext);
        $alltext = replace_keyword("new", $alltext);
        $alltext = replace_keyword("goto", $alltext);
        $alltext = replace_keyword("checked", $alltext);
        $alltext = replace_keyword("unchecked", $alltext);
        $alltext = replace_keyword("lock", $alltext);
        $alltext = replace_keyword("using", $alltext);
        $alltext = replace_keyword("get", $alltext);
        $alltext = replace_keyword("set", $alltext);
        $alltext = replace_keyword("throw", $alltext);
        $alltext = replace_keyword("try", $alltext);
        $alltext = replace_keyword("catch", $alltext);
        $alltext = replace_keyword("finally", $alltext);
        $alltext = replace_keyword("null", $alltext);
        $alltext = replace_keyword("true", $alltext);
        $alltext = replace_keyword("false", $alltext);

        $alltext = replace_keyword("void", $alltext);
        $alltext = replace_type("sbyte", $alltext);
        $alltext = replace_type("byte", $alltext);
        $alltext = replace_type("short", $alltext);
        $alltext = replace_type("ushort", $alltext);
        $alltext = replace_type("int", $alltext);
        $alltext = replace_type("uint", $alltext);
        $alltext = replace_type("long", $alltext);
        $alltext = replace_type("ulong", $alltext);
        $alltext = replace_type("char", $alltext);
        $alltext = replace_type("float", $alltext);
        $alltext = replace_type("double", $alltext);
        $alltext = replace_type("bool", $alltext);
        $alltext = replace_type("decimal", $alltext);
        $alltext = replace_type("string", $alltext);
        $alltext = replace_type("object", $alltext);

        $alltext = preg_replace("/\/\/(.*)\n/", "span_class_equals_comment//\$1end_of_comment_span\n", $alltext);
        $alltext = preg_replace("/\\\"(.*)\\\"/", "span_class_equals_string\"\$1\"end_of_string_span", $alltext);
        $alltext = preg_replace("/([\\.\\[\\]\\(\\)\\!\\=\\+\\-\\*\\|])/", "span_class_equals_operator\$1end_of_span", $alltext);
		#$alltext = str_replace("\n", "<br>\n", $alltext);

        $alltext = preg_replace("/(span_class_equals_string)(.*?)(end_of_string_span)/e", 'strip_spans("\\1","\\2","\\3")', $alltext);
        $alltext = preg_replace("/(span_class_equals_comment)(.*?)(end_of_comment_span)/e", 'strip_spans("\\1","\\2","\\3")', $alltext);

        $alltext = str_replace("span_class_equals_type", "<span class='keyword'>", $alltext);
        $alltext = str_replace("span_class_equals_number", "<span class='keyword'>", $alltext);
        $alltext = str_replace("span_class_equals_keyword", "<span class='keyword'>", $alltext);
        $alltext = str_replace("span_class_equals_operator", "<span class='operator'>", $alltext);
        $alltext = str_replace("span_class_equals_comment", "<span class='comment'>", $alltext);
        $alltext = str_replace("span_class_equals_string", "<span class='string'>", $alltext);
        $alltext = str_replace("end_of_span", "</span>", $alltext);
        $alltext = str_replace("end_of_comment_span", "</span>", $alltext);
        $alltext = str_replace("end_of_string_span", "</span>", $alltext);

        fclose($fd);

        echo '<pre class="csharp">';
        echo $alltext;
        echo '</pre>';
    }

	function convert_opening_xml_tag($tagname, $remaining)
	{
		$remaining = preg_replace("/\b(\w+)=\"(.*?)\"/", "span_class_equals_xmlattribute\$1end_of_spanspan_class_equals_xmlbracket=end_of_spanspan_class_equals_xmlstring\"\$2\"end_of_span", $remaining);
		return "span_class_equals_xmlbracket&lt;end_of_spanspan_class_equals_xmltag${tagname}end_of_span${remaining}span_class_equals_xmlbracket&gt;end_of_span";
	}

    function format_xml($filename)
    {
        $alltext = "";
        $basedir = dirname($_SERVER["SCRIPT_FILENAME"]);
        $fd = fopen("$basedir/$filename", "r");

        while ($line=fgets($fd,1000))
        {
            $alltext.=$line;
        }

        // keywords

        $alltext = str_replace("\r", "", $alltext);

        $alltext = preg_replace("/<\!--(.*?)-->/s", "span_class_equals_comment&lt;!--\$1--&gt;end_of_comment_span\n", $alltext);

		# $alltext = preg_replace("/(span_class_equals_comment)(.*?)(end_of_comment_span)/e", 'strip_spans("\\1","\\2","\\3")', $alltext);
		$alltext = preg_replace("/<([a-zA-Z]\w+)\b(.*?)>/e", 'convert_opening_xml_tag("\\1","\\2")', $alltext);
		$alltext = preg_replace("/<\/([a-zA-Z]\w+)>/", "span_class_equals_xmlbracket&lt;/end_of_spanspan_class_equals_xmltag\$1end_of_spanspan_class_equals_xmlbracket&gt;end_of_span", $alltext);

        $alltext = str_replace("span_class_equals_type", "<span class='keyword'>", $alltext);
        $alltext = str_replace("span_class_equals_number", "<span class='keyword'>", $alltext);
        $alltext = str_replace("span_class_equals_xmlbracket", "<span class='xmlbracket'>", $alltext);
        $alltext = str_replace("span_class_equals_xmltag", "<span class='xmltag'>", $alltext);
        $alltext = str_replace("span_class_equals_xmlattribute", "<span class='xmlattribute'>", $alltext);
        $alltext = str_replace("span_class_equals_xmlstring", "<span class='xmlstring'>", $alltext);
        $alltext = str_replace("span_class_equals_comment", "<span class=commentxmlstring'>", $alltext);
        $alltext = str_replace("end_of_span", "</span>", $alltext);
        $alltext = str_replace("end_of_comment_span", "</span>", $alltext);
        $alltext = str_replace("end_of_string_span", "</span>", $alltext);

        fclose($fd);

        echo '<pre class="csharp">';
        echo $alltext;
        echo '</pre>';
    }
?>
