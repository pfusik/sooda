!include path.nsh
!include mui.nsh
!cd ..

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE License.txt
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
  !insertmacro MUI_LANGUAGE "English"

; The name of the installer
Name "Sooda"

SetCompress force

; The file to write
OutFile "SetupSooda.exe"

; The default installation directory
InstallDir $PROGRAMFILES\Sooda

; The text to prompt the user to enter a directory
DirText "This will install Sooda Library and tools on your computer. Choose a directory:"

; The stuff to install
Section "Library and Tools"
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File License.txt

  SetOutPath $INSTDIR\bin
  ; Put file there
  File bin\NLog.dll
  File bin\Sooda.dll
  File bin\ICSharpCode.TextEditor.dll
  File bin\SoodaStubGen.exe

  SetOutPath $INSTDIR\docs

;  File ..\..\docs\ClassRef.chm

  CreateDirectory "$SMPROGRAMS\Sooda"
  CreateShortCut  "$SMPROGRAMS\Sooda\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
;  CreateShortCut  "$SMPROGRAMS\Sooda\Class Library Reference.lnk" "$INSTDIR\Docs\ClassRef.chm" ""
  CreateShortCut  "$SMPROGRAMS\Sooda\View Class Library and Tools License.lnk" "$INSTDIR\License.txt" ""

  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda" "" "$INSTDIR\Bin"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda" "DisplayName" "Sooda Class Library"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ExecShell open '$SMPROGRAMS\Sooda'

  Push $INSTDIR
  call AddToPath
SectionEnd ; end the section

Section "Sooda Query Analyzer"
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File SoodaQuery_License.txt

  SetOutPath $INSTDIR\bin
  File bin\ICSharpCode.TextEditor.dll
  File bin\SoodaQuery.exe
  File bin\SoodaQuery.exe.manifest

  CreateDirectory "$SMPROGRAMS\Sooda"
  CreateShortCut  "$SMPROGRAMS\Sooda\View Sooda Query Analyzer License.lnk" "$INSTDIR\SoodaQuery_License.txt" ""
  CreateShortCut  "$SMPROGRAMS\Sooda\Soql Query Analyzer.lnk" "$INSTDIR\bin\SoodaQuery.exe" ""
SectionEnd

Section "VC# .NET 2003 Wizards"
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VC# "ProductDir"
  IfErrors novsnet
  DetailPrint "Visual C# .NET 2003 installed in $0"
  SetOutPath $0
  File /r wizard\CSharpProjects
  File /r "wizard\VC#Wizards"
  Return

novsnet:
  MessageBox MB_OK "Visual C# .NET 2003 was not found. Wizards not installed."
SectionEnd

Section "Uninstall"

  Push $INSTDIR
  call un.RemoveFromPath

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda"
  DeleteRegKey HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda"

  Delete "$SMPROGRAMS\Sooda\*.lnk"
  RMDir "$SMPROGRAMS\Sooda"

  Delete "$INSTDIR\LICENSE.txt"
  Delete "$INSTDIR\SoodaQuery_LICENSE.txt"
  Delete "$INSTDIR\bin\*.dll"
  Delete "$INSTDIR\bin\*.exe.config"
  Delete "$INSTDIR\bin\*.exe.manifest"
  Delete "$INSTDIR\bin\*.exe"
  Delete "$INSTDIR\docs\*.chm"
  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR\bin"
  RMDir "$INSTDIR\docs"
  RMDir "$INSTDIR"
SectionEnd
; eof
