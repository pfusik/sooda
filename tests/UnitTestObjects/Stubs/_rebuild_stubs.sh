#!/bin/bash
SOODA_PATH=../../../bin

MCS_OPTIONS=/debug+ /optimize+
MCS=mcs

SOODA_DLL=$SOODA_PATH/Sooda.dll
OBJECTS_DLL=Sooda.UnitTests.Objects
OBJECTS_ASSEMBLY_INFO=../AssemblyInfo.cs

STUBS_DLL=$OBJECTS_DLL.Stubs
STUBS_ASSEMBLY_INFO=
ADDITIONAL_STUBS_CS=

echo Deleting old stubs...
rm -f $STUBS_DLL.dll $STUBS_DLL.pdb

echo Compiling mini-stubs...
$MCS $MCS_OPTIONS /nologo /target:library /out:$STUBS_DLL.dll /r:$SOODA_DLL /r:System.Data.dll _MiniStubs.csx $ADDITIONAL_STUBS_CS

echo Compiling mini-skeletons...
$MCS $MCS_OPTIONS /nologo /target:library /out:$OBJECTS_DLL.dll /r:$SOODA_DLL /r:$STUBS_DLL.dll /r:System.Data.dll _MiniSkeleton.csx $OBJECTS_ASSEMBLY_INFO

echo Recompiling stubs with mini-skeleton...
$MCS $MCS_OPTIONS /nologo /target:library /out:$STUBS_DLL.dll /r:$SOODA_DLL /r:$OBJECTS_DLL.dll /r:System.Data.dll _Stubs.csx $STUBS_ASSEMBLY_INFO $ADDITIONAL_STUBS_CS

echo Stubs written.
