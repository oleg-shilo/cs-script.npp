echo off

set programfiles=%PROGRAMFILES(X86)%

cscs pkill VBCSCompiler

md "..\bin\Plugins\CSScriptNpp"
md "..\bin\Plugins\CSScriptNpp\Mdbg"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins
set config=Release

copy "CSScriptIntellisense\bin\%config%\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\%config%\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"


copy "CSScriptNpp\bin\%config%\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"


copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%plugins%\CSScriptNpp\launcher.exe"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\syntaxer.exe" "%plugins%\CSScriptNpp\syntaxer.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%plugins%\CSScriptNpp\CompatibilityTest.exe"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"
copy "CSScriptNpp\CSScriptNpp\CSSRoslynProvider.dll" "%plugins%\CSScriptNpp\CSSRoslynProvider.dll"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%plugins%\CSScriptNpp\CSSCodeProvider.v4.0.dll"


echo ---------------------------------------------------------------
set bin=..\bin\Plugins

copy "CSScriptIntellisense\bin\%config%\CSScriptIntellisense.dll" "%bin%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\%config%\CSharpIntellisense\*.dll" "%bin%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%bin%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%bin%\CSScriptNpp\CSScriptLibrary.xml"


copy "CSScriptNpp\bin\%config%\CSScriptNpp\*.exe" "%bin%\CSScriptNpp"

copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.dll" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.exe" "%bin%\CSScriptNpp\Mdbg"


copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%bin%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%bin%\CSScriptNpp\launcher.exe"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%bin%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\syntaxer.exe" "%bin%\CSScriptNpp\syntaxer.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%bin%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%bin%\CSScriptNpp\CompatibilityTest.exe"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%bin%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%bin%\CSScriptNpp\CSScriptLibrary.xml"
copy "CSScriptNpp\CSScriptNpp\CSSRoslynProvider.dll" "%bin%\CSScriptNpp\CSSRoslynProvider.dll"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%bin%\CSScriptNpp\CSSCodeProvider.v4.0.dll"
copy "CSScriptNpp\bin\%config%\CSScriptNpp.dll" "%bin%\CSScriptNpp.dll"

rem -------------------------------------

del "%plugins%\CSScriptNpp\MDbg\*.pdb"
del "%bin%\CSScriptNpp\MDbg\*.pdb"

copy "..\readme.txt" "..\bin\readme.txt"
copy "..\license.txt" "..\bin\license.txt"

echo ----------------------------
rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\%config%\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"

pause