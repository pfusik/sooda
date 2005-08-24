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
OutFile "SoodaInstaller-${RELEASE_VERSION}.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\Sooda ${SOODA_VERSION}"

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
  File src\Sooda\Schema\SoodaSchema.xsd

  SetOutPath $INSTDIR\bin
  ; Put file there
  File build\${BUILDSUBDIR}\bin\NLog.dll
  File build\${BUILDSUBDIR}\bin\Sooda.dll
  File build\${BUILDSUBDIR}\bin\SoodaStubGen.exe
  File build\${BUILDSUBDIR}\bin\SoodaSchemaTool.exe
  File build\${BUILDSUBDIR}\bin\SoodaCompileStubs.exe
  File build\${BUILDSUBDIR}\bin\Sooda*.pdb

  SetOutPath $INSTDIR\docs
  File build\${BUILDSUBDIR}\help\Sooda.chm

  CreateDirectory "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
  CreateShortCut  "$SMPROGRAMS\Sooda\Sooda Class Library Reference.lnk" "$INSTDIR\Doc\Sooda.chm" ""
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\View Sooda License.lnk" "$INSTDIR\License.txt" ""

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
  File build\${BUILDSUBDIR}\bin\ICSharpCode.TextEditor.dll
  File build\${BUILDSUBDIR}\bin\SoodaQuery.exe
  File build\${BUILDSUBDIR}\bin\SoodaQuery.exe.manifest

  CreateDirectory "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\View Sooda Query Analyzer License.lnk" "$INSTDIR\SoodaQuery_License.txt" ""
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Soql Query Analyzer.lnk" "$INSTDIR\bin\SoodaQuery.exe" ""
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Examples.lnk" "$INSTDIR\examples" ""
SectionEnd

Section "Examples"
  SectionIn 1
  SetOutPath $INSTDIR\examples
  File /r /x _svn examples\*.*
  CreateShortCut  "$SMPROGRAMS\Sooda ${SOODA_VERSION}\Examples.lnk" "$INSTDIR\examples" ""
SectionEnd

Section "VC# .NET 2003 Wizards"
  SectionIn 1
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VC# "ProductDir"
  IfErrors novsnet
  DetailPrint "Visual C# .NET 2003 installed in $0"
  SetOutPath $0
  File /r /x _svn wizard\CSharpProjects
  File /r /x _svn "wizard\VC#Wizards"
  Return

novsnet:
  MessageBox MB_OK "Visual C# .NET 2003 was not found. Wizards not installed."
SectionEnd

Section "Documentation"
  SectionIn 1
  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet
  DetailPrint "Visual Studio .NET 2003 installed in $0"
  SetOutPath "$0\Packages\schemas\xml"
  File src\Sooda\Schema\SoodaSchema.xsd
  Return
novsnet:
  MessageBox MB_OK "Visual Studio .NET 2003 was not found. Schema not installed."

SectionEnd

Section "Add bin directory to PATH"
  SectionIn 1
  Push "$INSTDIR\bin"
  call AddToPath
SectionEnd

Section "Schema for VS.NET 2003 Intellisense"
  SectionIn 1
SectionEnd

Section "Uninstall"
  Push "$INSTDIR\bin"
  call un.RemoveFromPath

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VC# "ProductDir"
  IfErrors novsnet
  Delete "$0CSharpProjects\CSharpSooda*"
  RMDir /r "$0VC#Wizards\CSharpSoodaConsoleWiz"
  RMDir /r "$0VC#Wizards\CSharpSoodaDLLWiz"
  RMDir /r "$0VC#Wizards\CSharpSoodaEXEWiz"
  RMDir /r "$0VC#Wizards\CSharpAddSoodaSchemaWiz"

novsnet:
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda"
  DeleteRegKey HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda"

  ClearErrors
  ReadRegStr $0 HKLM Software\Microsoft\VisualStudio\7.1\Setup\VS "VS7CommonDir"
  IfErrors novsnet2
  Delete "$0\Packages\schemas\xml\SoodaSchema.xsd"

novsnet2:
  Delete "$SMPROGRAMS\Sooda ${SOODA_VERSION}\*.lnk"
  RMDir "$SMPROGRAMS\Sooda ${SOODA_VERSION}"
  RMDir /r "$INSTDIR"
SectionEnd
; eof

Function .onInstSuccess
  ExecShell open '$SMPROGRAMS\Sooda ${SOODA_VERSION}'
FunctionEnd

