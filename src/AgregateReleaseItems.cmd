echo off
set programfiles=%PROGRAMFILES(X86)%

cscs pkill VBCSCompiler

md "..\bin\Plugins\CSScriptNpp"
md "..\bin\Plugins\CSScriptNpp\Mdbg"
md "..\bin\Plugins\CSScriptNpp\Roslyn"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\Release\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"

copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Release\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%plugins%\CSScriptNpp\launcher.exe"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%plugins%\CSScriptNpp\CompatibilityTest.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%plugins%\CSScriptNpp\CSSCodeProvider.v4.0.dll"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.6.dll" "%plugins%\CSScriptNpp\CSSCodeProvider.v4.6.dll"

copy "CSScriptNpp\CSScriptNpp\roslyn\*.*" "%plugins%\CSScriptNpp\Roslyn"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Release\*.*" "%plugins%\CSScriptNpp\Roslyn"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Release\RoslynIntellisense.exe" "%plugins%\CSScriptNpp\Roslyn\RoslynIntellisense.exe"

echo ---------------------------------------------------------------
set bin=..\bin\Plugins

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll"  "%bin%\CSScriptNpp"
copy "CSScriptIntellisense\bin\Release\CSharpIntellisense\*.dll" "%bin%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"

copy "CSScriptNpp\bin\release\CSScriptNpp.dll" "%bin%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.exe" "%bin%\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "%bin%\CSScriptNpp"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%bin%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%bin%\CSScriptNpp"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.6.dll" "%bin%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\roslyn\*.*" "%bin%\CSScriptNpp\Roslyn"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Release\*.*" "%bin%\CSScriptNpp\Roslyn"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Release\RoslynIntellisense.exe" "%bin%\CSScriptNpp\Roslyn\RoslynIntellisense.exe"

rem -------------------------------------

del "%plugins%\CSScriptNpp\Roslyn\*.vshost.*"
del "%plugins%\CSScriptNpp\Roslyn\*.pdb"
del "%plugins%\CSScriptNpp\Roslyn\*.xml"
del "%plugins%\CSScriptNpp\Roslyn\CSSCodeProvider.v4.6.dll"
rem del "%plugins%\CSScriptNpp\Roslyn\RoslynIntellisense.exe"
rem del "%plugins%\CSScriptNpp\Roslyn\RoslynIntellisense.exe.config"
del "%plugins%\CSScriptNpp\Roslyn\Intellisense.Common.dll"
del "%plugins%\CSScriptNpp\MDbg\*.pdb"


del "%bin%\CSScriptNpp\Roslyn\*.vshost.*"
del "%bin%\CSScriptNpp\Roslyn\*.pdb"
del "%bin%\CSScriptNpp\Roslyn\*.xml"
del "%bin%\CSScriptNpp\Roslyn\CSSCodeProvider.v4.6.dll"
rem del "%bin%\CSScriptNpp\Roslyn\RoslynIntellisense.exe"
rem del "%bin%\CSScriptNpp\Roslyn\RoslynIntellisense.exe.config"
del "%bin%\CSScriptNpp\Roslyn\Intellisense.Common.dll"
del "%bin%\CSScriptNpp\MDbg\*.pdb"

copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%bin%\CSScriptNpp\launcher.exe"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%bin%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%bin%\CSScriptNpp\CompatibilityTest.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%bin%\CSScriptNpp\npp_jit.exe"

copy "..\readme.txt" "..\bin\readme.txt"
copy "..\license.txt" "..\bin\license.txt"

echo ----------------------------
rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"

pause