@echo off
set DBNAME=SoodaUnitTests
if (%1)==() goto usage
cls
echo.
echo This will drop and re-create %DBNAME% database on %1
echo.
echo You can press Ctrl+C to quit now.
echo.
pause
osql -S %1 -E -n -i dropdb.sql
osql -S %1 -E -n -i createdb.sql
pause
goto quit

:usage
echo Usage: recreatedb HOSTNAME
echo This will drop and re-create %DBNAME% database on HOSTNAME

:quit
