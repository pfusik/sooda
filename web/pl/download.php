<?php
include "_layout.php";
write_start_page();
?>
        <h1>Pobierz</h1>
        <p>
        DostÍpne sπ nastÍpujπce wersje biblioteki Sooda oraz narzÍdzi:
        </p>
        <table align="center" width="90%">
            <tr>
                <th>Wersja</th>
                <th>Data</th>
                <th>Opis</th>
                <th>Pobierz</th>
            </tr>
            <tr>
                <td align="center">0.1</td>
                <td align="center">6 marca 2004</td>
                <td>Pierwsza oficjalna wersja.<br/>Wspiera .NET oraz Mono i dzia≥a na MSSQL 2000 oraz PostgreSQL</td>
                <td align="center">
                    <a href="../release/sooda-0.1-src.zip">èrÛd≥a</a><br/>
                    <a href="../release/sooda-0.1-bin.zip">Binaria</a><br/>
                    <a href="../release/sooda-0.1.exe">Instalator Win32</a>
                </td>
            </tr>
            <tr>
                <td align="center">current</td>
                <td align="center"></td>
                <td>Wersja rozwojowa generowana co noc z repozytorium Subversion.</td>
                <td align="center">
                    <a href="../release/sooda-current-src.zip">èrÛd≥a</a>
                </td>
            </tr>
        </table>
<?php write_end_page() ?>
