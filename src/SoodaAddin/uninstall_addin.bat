@echo off
%WINDIR%\Microsoft.NET\Framework\v1.1.4322\regasm.exe /u /nologo /tlb /codebase SoodaAddin.dll
regedit /s SoodaAddinVS2003_uninstall.reg