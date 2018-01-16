echo off

cscs -l tools\set_host_version.cs




set programfiles=%PROGRAMFILES(X86)%


REM cscs pkill VBCSCompiler

REM md "..\bin\Plugins\CSScriptNpp"
REM md "..\bin\Plugins\CSScriptNpp\Mdbg"
REM md "%programfiles%\Notepad++\plugins\CSScriptNpp"
REM md "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"

REM rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
REM rem so it needs to be done manually.

REM set plugins=%programfiles%\Notepad++\plugins
REM set config=Release

REM copy "CSScriptIntellisense\bin\%config%\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
REM copy "CSScriptIntellisense\bin\%config%\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"


REM copy "CSScriptNpp\bin\%config%\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"

REM copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
REM copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"


REM copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"

REM copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%plugins%\CSScriptNpp\launcher.exe"
REM copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
REM copy "CSScriptNpp\CSScriptNpp\syntaxer.exe" "%plugins%\CSScriptNpp\syntaxer.exe"
REM copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"
REM copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%plugins%\CSScriptNpp\CompatibilityTest.exe"

REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"
REM copy "CSScriptNpp\CSScriptNpp\CSSRoslynProvider.dll" "%plugins%\CSScriptNpp\CSSRoslynProvider.dll"
REM copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%plugins%\CSScriptNpp\CSSCodeProvider.v4.0.dll"


REM echo ---------------------------------------------------------------
REM set bin=..\bin\Plugins

REM copy "CSScriptIntellisense\bin\%config%\CSScriptIntellisense.dll" "%bin%\CSScriptNpp\CSScriptIntellisense.dll"
REM copy "CSScriptIntellisense\bin\%config%\CSharpIntellisense\*.dll" "%bin%\CSScriptNpp"

REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%bin%\CSScriptNpp\CSScriptLibrary.dll"
REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%bin%\CSScriptNpp\CSScriptLibrary.xml"


REM copy "CSScriptNpp\bin\%config%\CSScriptNpp\*.exe" "%bin%\CSScriptNpp"

REM copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.dll" "%bin%\CSScriptNpp\Mdbg"
REM copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.exe" "%bin%\CSScriptNpp\Mdbg"


REM copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%bin%\CSScriptNpp\Mdbg"

REM copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%bin%\CSScriptNpp\launcher.exe"
REM copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%bin%\CSScriptNpp\Updater.exe"
REM copy "CSScriptNpp\CSScriptNpp\syntaxer.exe" "%bin%\CSScriptNpp\syntaxer.exe"
REM copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%bin%\CSScriptNpp\npp_jit.exe"
REM copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%bin%\CSScriptNpp\CompatibilityTest.exe"

REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%bin%\CSScriptNpp\CSScriptLibrary.dll"
REM copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%bin%\CSScriptNpp\CSScriptLibrary.xml"
REM copy "CSScriptNpp\CSScriptNpp\CSSRoslynProvider.dll" "%bin%\CSScriptNpp\CSSRoslynProvider.dll"
REM copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%bin%\CSScriptNpp\CSSCodeProvider.v4.0.dll"
REM copy "CSScriptNpp\bin\%config%\CSScriptNpp.dll" "%bin%\CSScriptNpp.dll"

REM rem -------------------------------------

REM del "%plugins%\CSScriptNpp\MDbg\*.pdb"
REM del "%bin%\CSScriptNpp\MDbg\*.pdb"

REM copy "..\readme.txt" "..\bin\readme.txt"
REM copy "..\license.txt" "..\bin\license.txt"

REM echo ----------------------------
REM rem need to keep it last so copy errors (if any) are visible
REM copy "CSScriptNpp\bin\%config%\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"

pause