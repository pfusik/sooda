<?
include "_layout.php";
write_start_page();
?>
        <h1>Download</h1>
        <p>
        The following versions of Sooda are available for download. Click on the appropriate link to start a download.
        </p>
        <table align="center" width="90%">
            <tr>
                <th>Version</th>
                <th>Date</th>
                <th>Description</th>
                <th>Download</th>
            </tr>
            <tr>
                <td align="center">0.1</td>
                <td align="center">6 Mar 2004</td>
                <td>First release.<br/>Supports .NET and Mono and works on MSSQL 2000 and PostgreSQL databases.</td>
                <td align="center">
                    <a href="../release/sooda-0.1-src.zip">Sources</a><br/>
                    <a href="../release/sooda-0.1-bin.zip">Binaries</a><br/>
                    <a href="../release/sooda-0.1.exe">Win32 Installer</a>
                </td>
            </tr>
            <tr>
                <td align="center">current</td>
                <td align="center"></td>
                <td>Nightly snapshot of the Subversion repository.</td>
                <td align="center">
                    <a href="../release/sooda-current-src.zip">Sources</a>
                </td>
            </tr>
        </table>
<?php write_end_page(); ?>

