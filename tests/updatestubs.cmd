@echo off
..\bin\StubGen.exe --separate-stubs --project vs -s DBSchema.xml -n Sooda.UnitTests.Objects -o UnitTestObjects || exit 1
exit
cd UnitTestObjects\Stubs
call _rebuild_stubs.cmd || exit 1
del ..\bin\Sooda.UnitTests.Objects.Stubs.dll
del bin\Sooda.UnitTests.Objects.Stubs.dll


