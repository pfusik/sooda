@echo off
rem *******************************************************************
rem This script will rebuild stubs dll. You may customize paths and 
rem options below
rem *******************************************************************
set SOODA_PATH=..\..\..\bin

set CSC_OPTIONS=/debug+ /optimize+
if (%CSC%)==() set CSC=csc.exe
set CSC=%SYSTEMROOT%\Microsoft.NET\Framework\v1.1.4322\csc.exe

set SCHEMA_RESOURCE=_DBSchema.bin
if not exist %SCHEMA_RESOURCE% set SCHEMA_RESOURCE=_DBSchema.xml

set SOODA_DLL=%SOODA_PATH%\Sooda.dll
set OBJECTS_DLL=Sooda.UnitTests.Objects
set OBJECTS_ASSEMBLY_INFO=

set STUBS_DLL=%OBJECTS_DLL%.Stubs
set STUBS_ASSEMBLY_INFO=
set ADDITIONAL_STUBS_CS=

rem *******************************************************************
echo Deleting old stubs...
if exist %STUBS_DLL%.dll del %STUBS_DLL%.dll
if exist %STUBS_DLL%.pdb del %STUBS_DLL%.pdb

echo Compiling mini-stubs...
%CSC% %CSC_OPTIONS% /nologo /target:library /out:%STUBS_DLL%.dll /r:%SOODA_DLL% _MiniStubs.csx %ADDITIONAL_STUBS_CS%

echo Compiling mini-skeletons...
%CSC% %CSC_OPTIONS% /nologo /target:library /out:%OBJECTS_DLL%.dll /r:%SOODA_DLL% /r:%STUBS_DLL%.dll _MiniSkeleton.csx %OBJECTS_ASSEMBLY_INFO%

echo Recompiling stubs with mini-skeleton...
%CSC% %CSC_OPTIONS% /nologo /target:library /out:%STUBS_DLL%.dll /r:%SOODA_DLL% /r:%OBJECTS_DLL%.dll /res:%SCHEMA_RESOURCE% _Stubs.csx %STUBS_ASSEMBLY_INFO% %ADDITIONAL_STUBS_CS%

echo Stubs written.
