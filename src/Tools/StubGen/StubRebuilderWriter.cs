//
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of the Jaroslaw Kowalski nor the names of its
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.IO;

namespace Sooda.StubGen {
    public class StubRebuilderWriter {
        const string text = @"@echo off
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
                            set OBJECTS_DLL={0}
                            set OBJECTS_ASSEMBLY_INFO=..\AssemblyInfo.cs

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
                            ";

        public static void WriteStubRebuilder(string filename, string objectsAssemblyName) {
            using (StreamWriter sw = File.CreateText(filename)) {
                sw.Write(text, objectsAssemblyName);
            }
        }
    }
}
