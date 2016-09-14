echo off
set programfiles=%PROGRAMFILES(X86)%
md "..\bin\Plugins\CSScriptNpp"
md "..\bin\Plugins\CSScriptNpp\Mdbg"
md "..\bin\Plugins\CSScriptNpp\Roslyn"
md "..\bin\Plugins\CSScriptNpp\Roslyn.Intellisense"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn.Intellisense"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\Release\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"

copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Release\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%plugins%\CSScriptNpp\CompatibilityTest.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "%plugins%\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "%plugins%\CSScriptNpp\7z.dll"
copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\roslyn\*.*" "%plugins%\CSScriptNpp\Roslyn"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Release\*.*" "%plugins%\CSScriptNpp\Roslyn.Intellisense"
copy "CSScriptNpp\CSScriptNpp\roslyn\roslyn.readme.txt" "%plugins%\CSScriptNpp\Roslyn.Intellisense"

echo ---------------------------------------------------------------
set bin=..\bin\Plugins

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll"  "%bin%\CSScriptNpp"
copy "CSScriptIntellisense\bin\Release\CSharpIntellisense\*.dll" "%bin%\CSScriptNpp"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%bin%\CSScriptNpp"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%plugins%\CSScriptNpp\CSSCodeProvider.v4.0.dll"

copy "CSScriptNpp\bin\release\CSScriptNpp.dll" "%bin%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.exe" "%bin%\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "%bin%\CSScriptNpp"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%bin%\CSScriptNpp\Mdbg"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\*.*" "%bin%\CSScriptNpp\Roslyn\"

REM copy "CSScriptNpp\bin\Release\NLog.dll" "%bin%\CSScriptNpp\NLog.dll"
REM copy "CSScriptNpp\bin\Release\NLog.dll.nlog" "%bin%\CSScriptNpp\NLog.dll.nlog"

copy "CSScriptNpp\CSScriptNpp\roslyn\*.*" "%bin%\CSScriptNpp\Roslyn"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Release\*.*" "%bin%\CSScriptNpp\Roslyn.Intellisense"
copy "CSScriptNpp\CSScriptNpp\roslyn\roslyn.readme.txt" "%bin%\CSScriptNpp\Roslyn.Intellisense"

copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%bin%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%bin%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\CompatibilityTest.exe" "%bin%\CSScriptNpp\CompatibilityTest.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%bin%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "%bin%\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "%bin%\CSScriptNpp\7z.dll"

copy "..\readme.txt" "..\bin\readme.txt"
copy "..\license.txt" "..\bin\license.txt"

echo ----------------------------
rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"

pause