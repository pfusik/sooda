function hilite_item(s) {
    var tableRows = document.getElementsByTagName("TR");
    var i;

    var s2 = "'" + s + "'";
    var selectedRow = null;
    for (i = 0; i < tableRows.length; ++i) {
        var tr = tableRows[i];
        var onclick = tr.getAttribute("onclick") + '';
        if (onclick != null && onclick.indexOf(s2) != -1) {
            selectedRow = tr;
            break;
        }
    }
    for (i = 0; i < tableRows.length; ++i) {
        var tr = tableRows[i];

        if (tr == selectedRow) {
            tr.style.backgroundColor = "#3b3b2e";
            tr.style.color = "#ffffc8";
        } else {
            tr.style.backgroundColor = "";
            tr.style.color = "";
        }
    }
}

function onmouseover_row(row) {
    row.className = 'nav_hilite';
}

function onmouseout_row(row) {
    row.className = 'nav_normal';
}

function navigate_to(url) {
    location = url;
}
