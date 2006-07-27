@echo off
%WINDIR%\Microsoft.NET\Framework\v1.1.4322\regasm.exe /nologo /tlb /codebase SoodaAddin.dll
regedit /s SoodaAddinVS2003.reg