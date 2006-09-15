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
OutFile "Sooda-${RELEASE_VERSION}${SOODA_DEBUG}.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\Sooda ${SOODA_VERSION}"

; The text to prompt the user to enter a directory
DirText "This will install Sooda ${SOODA_VERSION} on your computer. Choose a directory:"

InstType "Full"
InstType ".NET 1.1 / Visual Studio.NET 2003 Support"
InstType ".NET 2.0 / Visual Studio 2005 Support"
InstType "Minimal"

; The stuff to install
Section "Core Files"
  SectionIn 1 2 3 4 RO

  SetOutPath $INSTDIR
  File License.txt
  File src\Sooda\Schema\SoodaSchema.xsd
  File src\Sooda.CodeGen\SoodaProject.xsd

  # create shortcuts

  CreateDirectory "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\View Sooda License.lnk" "$INSTDIR\License.txt" ""

  # register uninstaller

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda${SOODA_VERSION}" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda${SOODA_VERSION}" "DisplayName" "Sooda ${SOODA_VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda${SOODA_VERSION}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  # register SOODA_DIR environment variable

  WriteRegStr HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "SOODA_DIR" "$INSTDIR"
  SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=5000
SectionEnd ; end the section

SectionGroup ".NET 1.1 / Visual Studio.NET 2003 Support"

Section ".NET 1.1 Libraries and Tools"
  SectionIn 1 2 4

  SetOutPath $INSTDIR\bin\net-1.1
  File /r build\net-1.1${SOODA_DEBUG}\bin\*Interop*.dll
  File /r build\net-1.1${SOODA_DEBUG}\bin\*Sooda*.dll
  File /r build\net-1.1${SOODA_DEBUG}\bin\*Sooda*.exe
  File /r build\net-1.1${SOODA_DEBUG}\bin\*Sooda*.xml
SectionEnd

Section "Debug Symbols"
  SectionIn 1 2

  SetOutPath $INSTDIR\bin\net-1.1
  File /nonfatal /r build\net-1.1${SOODA_DEBUG}\bin\*Sooda*.pdb
SectionEnd

Section "Visual Studio 2003 Support"
  SectionIn 1 2
  WriteRegStr HKLM "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda" "" "$INSTDIR\bin\net-1.1"

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet
  DetailPrint "Visual Studio .NET 2003 installed in $0"
  SetOutPath "$0\Packages\schemas\xml"
  File src\Sooda\Schema\SoodaSchema.xsd
  File src\Sooda.CodeGen\SoodaProject.xsd
novsnet:
SectionEnd

SectionGroupEnd

SectionGroup ".NET 2.0 / Visual Studio 2005 Support"

Section ".NET 2.0 Libraries and Tools"
  SectionIn 1 3 4

  SetOutPath $INSTDIR\bin\net-2.0
  File /r build\net-2.0${SOODA_DEBUG}\bin\*Interop*.dll
  File /r build\net-2.0${SOODA_DEBUG}\bin\*Sooda*.dll
  File /r build\net-2.0${SOODA_DEBUG}\bin\*Sooda*.exe
  File /r build\net-2.0${SOODA_DEBUG}\bin\*Sooda*.xml
SectionEnd

Section "Debug Symbols"
  SectionIn 1 3

  SetOutPath $INSTDIR\bin\net-2.0
  File /nonfatal /r build\net-2.0${SOODA_DEBUG}\bin\*Sooda*.pdb
SectionEnd

Section "Visual Studio 2005 Support"
  SectionIn 1 3
  WriteRegStr HKLM "Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\Sooda" "" "$INSTDIR\bin\net-2.0"

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\8.0\Setup\VS "ProductDir"
  IfErrors novsnet
  DetailPrint "Visual Studio .NET 2005 installed in $0"
  SetOutPath "$0\xml\schemas"
  File src\Sooda\Schema\SoodaSchema.xsd
  File src\Sooda.CodeGen\SoodaProject.xsd

novsnet:
  CreateShortCut "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Add Sooda Support to Visual Studio Project.lnk" "$INSTDIR\bin\net-2.0\ConfigureSoodaProject.exe" ""
SectionEnd

SectionGroupEnd

Section "Documentation"
  SectionIn 1 2 3
  SetOutPath $INSTDIR\doc
  File build\Sooda.chm
  CreateShortCut "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Sooda Documentation.lnk" "$INSTDIR\Doc\Sooda.chm" ""
SectionEnd

Section "Uninstall"
  Push "$INSTDIR\bin"
  call un.RemoveFromPath

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet2
  Delete "$0\Packages\schemas\xml\SoodaSchema.xsd"
  Delete "$0\Packages\schemas\xml\SoodaProject.xsd"

novsnet2:
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\8.0\Setup\VS "ProductDir"
  IfErrors novsnet3
  Delete "$0\xml\schemas\SoodaSchema.xsd"
  Delete "$0\xml\schemas\SoodaProject.xsd"

novsnet3:
  Delete "$SMPROGRAMS\Sooda ${SOODA_VERSION}\*.lnk"
  RMDir "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  RMDir /r "$INSTDIR"

  # delete AssemblyFolders

  DeleteRegKey HKLM "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda"
  DeleteRegKey HKLM "Software\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\Sooda"

  # unregister environment variable SOODA_DIR

  DeleteRegValue HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "SOODA_DIR"
  SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=5000

  # unregister the uninstaller
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda"
SectionEnd
; eof

Function .onInstSuccess
  ExecShell open '$SMPROGRAMS\Sooda ${SOODA_VERSION}'
FunctionEnd

