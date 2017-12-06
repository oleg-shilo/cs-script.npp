echo off

set programfiles=%PROGRAMFILES(X86)%

cscs pkill VBCSCompiler

echo "%programfiles%\Notepad++\plugins\CSScriptNpp"

md "%programfiles%\Notepad++\plugins\CSScriptNpp\"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\MDbg\"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn\"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins

copy "CSScriptIntellisense\bin\Debug\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\Debug\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\bin\Debug\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\*.pdb" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"

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
copy "CSScriptNpp\CSScriptNpp\roslyn\*" "%plugins%\CSScriptNpp\Roslyn\"
copy "CSScriptNpp\CSScriptNpp\roslyn\CSSCodeProvider.v4.6.dll" "%plugins%\CSScriptNpp\Roslyn\CSSCodeProvider.v4.6.dll"

echo ============================
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Debug\RoslynIntellisense.exe" "%plugins%\CSScriptNpp\Roslyn\RoslynIntellisense.exe"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Debug\RoslynIntellisense.pdb" "%plugins%\CSScriptNpp\Roslyn\RoslynIntellisense.pdb"
echo ============================

rem md "%plugins%\CSScriptNpp\Roslyn_Intellisense"
rem move "%plugins%\CSScriptNpp\Roslyn_Intellisense" "%plugins%\CSScriptNpp\Roslyn.Intellisense"

set plugin_roslyn_intellisesne=%plugins%\CSScriptNpp\Roslyn

copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Debug\*.exe" "%plugin_roslyn_intellisesne%"
copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Debug\*.dll" "%plugin_roslyn_intellisesne%"
rem del "%plugin_roslyn_intellisesne%\RoslynIntellisense.exe"
del "%plugin_roslyn_intellisesne%\Intellisense.Common.dll"


rem C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\Roslyn
rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\Debug\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
pause