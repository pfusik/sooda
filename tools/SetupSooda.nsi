!include path.nsh
!include mui.nsh
!cd ..

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  !define MUI_COMPONENTSPAGE_SMALLDESC

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
InstallDir "$PROGRAMFILES\Sooda ${SOODA_VERSION}"
AutoCloseWindow true
ShowInstDetails nevershow

; The text to prompt the user to enter a directory
DirText "This will install Sooda ${SOODA_VERSION} on your computer. Choose a directory:"

InstType "Full"
InstType "Minimal"

; The stuff to install
Section "Library and Tools"
  SectionIn 1 2
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File License.txt

  SetOutPath $INSTDIR\bin
  ; Put file there
  File ${SOODA_BINARIES}\NLog.dll
  File ${SOODA_BINARIES}\Sooda.dll
  File ${SOODA_BINARIES}\SoodaStubGen.exe

  SetOutPath $INSTDIR\docs

;  File ..\..\docs\ClassRef.chm

  CreateDirectory "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
;  CreateShortCut  "$SMPROGRAMS\Sooda\Class Library Reference.lnk" "$INSTDIR\Docs\ClassRef.chm" ""
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\View Class Library and Tools License.lnk" "$INSTDIR\License.txt" ""

  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda${SOODA_VERSION}" "" "$INSTDIR\Bin"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda${SOODA_VERSION}" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda${SOODA_VERSION}" "DisplayName" "Sooda ${SOODA_VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda${SOODA_VERSION}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
SectionEnd ; end the section

Section "Sooda Query Analyzer"
  SectionIn 1
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File SoodaQuery_License.txt

  SetOutPath $INSTDIR\bin
  File ${SOODA_BINARIES}\ICSharpCode.TextEditor.dll
  File ${SOODA_BINARIES}\SoodaQuery.exe
  File ${SOODA_BINARIES}\SoodaQuery.exe.manifest

  CreateDirectory "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\View Sooda Query Analyzer License.lnk" "$INSTDIR\SoodaQuery_License.txt" ""
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Soql Query Analyzer.lnk" "$INSTDIR\bin\SoodaQuery.exe" ""
SectionEnd

Section "VC# .NET 2003 Wizards"
  SectionIn 1
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

Section "Documentation"
  SectionIn 1
SectionEnd

Section "Add bin directory to PATH"
  SectionIn 1
  Push "$INSTDIR\bin"
  call AddToPath
SectionEnd

Section "Uninstall"
  Push "$INSTDIR\bin"
  call un.RemoveFromPath

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda"
  DeleteRegKey HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda"

  Delete "$SMPROGRAMS\Sooda ${SOODA_VERSION}\*.lnk"
  RMDir "$SMPROGRAMS\Sooda ${SOODA_VERSION}"

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

Function .onInstSuccess
  MessageBox MB_OK "Installation succeeded."
  ExecShell open '$SMPROGRAMS\Sooda ${SOODA_VERSION}'
FunctionEnd

