!cd ..

; The name of the installer
Name "Sooda"

SetCompress force
SetCompressor lzma

; The file to write
OutFile "SetupSooda.exe"

; The default installation directory
InstallDir $PROGRAMFILES\Sooda

; The text to prompt the user to enter a directory
DirText "This will install Sooda Library and tools on your computer. Choose a directory:"

; The stuff to install
Section "Main"
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  File License.txt
  File SoodaQuery_License.txt

  SetOutPath $INSTDIR\bin
  ; Put file there
  File bin\log4net.dll
  File bin\Sooda.dll
  File bin\ICSharpCode.TextEditor.dll
  File bin\StubGen.exe
  File bin\SoodaQuery.exe
  File bin\SoodaQuery.exe.manifest

  SetOutPath $INSTDIR\docs

;  File ..\..\docs\ClassRef.chm

  CreateDirectory "$SMPROGRAMS\Sooda"
  CreateShortCut  "$SMPROGRAMS\Sooda\Uninstall.lnk" "$INSTDIR\Uninstall.exe" ""
;  CreateShortCut  "$SMPROGRAMS\Sooda\Class Library Reference.lnk" "$INSTDIR\Docs\ClassRef.chm" ""
  CreateShortCut  "$SMPROGRAMS\Sooda\View Class Library and Tools License.lnk" "$INSTDIR\License.txt" ""
  CreateShortCut  "$SMPROGRAMS\Sooda\View SoodaQuery License.lnk" "$INSTDIR\SoodaQuery_License.txt" ""
  CreateShortCut  "$SMPROGRAMS\Sooda\Soql Query Analyzer.lnk" "$INSTDIR\bin\SoodaQuery.exe" ""

  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\Sooda" "" "$INSTDIR\Bin"
  WriteRegStr HKCU "Software\Microsoft\VisualStudio\7.1\AssemblyFolders\Sooda" "" "$INSTDIR\Bin"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda" "" ""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda" "DisplayName" "Sooda Class Library"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ExecShell open '$SMPROGRAMS\Sooda'
SectionEnd ; end the section

Section "Uninstall"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Sooda"
  DeleteRegKey HKCU "Software\Microsoft\VisualStudio\7.0\AssemblyFolders\Sooda"
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
