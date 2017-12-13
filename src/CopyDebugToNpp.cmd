echo off

set programfiles=%PROGRAMFILES(X86)%

cscs pkill VBCSCompiler

echo "%programfiles%\Notepad++\plugins\CSScriptNpp"

md "%programfiles%\Notepad++\plugins\CSScriptNpp\"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\MDbg\"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins
set config=Debug

copy "CSScriptIntellisense\bin\%config%\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\%config%\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\bin\%config%\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\%config%\CSScriptNpp\*.pdb" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\%config%\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\MDbg\*.pdb" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\launcher.exe" "%plugins%\CSScriptNpp\launcher.exe"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\syntaxer.exe" "%plugins%\CSScriptNpp\syntaxer.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"

copy "CSScriptNpp\CSScriptNpp\cscs.exe" "%plugins%\CSScriptNpp\cscs.exe"
copy "CSScriptNpp\CSScriptNpp\csws.exe" "%plugins%\CSScriptNpp\csws.exe"
copy "CSScriptNpp\CSScriptNpp\cscs.v3.5.exe" "%plugins%\CSScriptNpp\cscs.v3.5.exe"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"
copy "CSScriptNpp\CSScriptNpp\CSSRoslynProvider.dll" "%plugins%\CSScriptNpp\CSSRoslynProvider.dll"

echo ============================

rem C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\Roslyn
rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\Debug\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
pause