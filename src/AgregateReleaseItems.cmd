echo off
md "..\..\bin\Plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\CSharpIntellisense\*.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp"

copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Release\CSScriptNpp\*.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp"


copy "bin\release\CSScriptNpp.dll" "..\..\bin\Plugins\CSScriptNpp.dll"
copy "bin\release\CSScriptNpp\*.dll" "..\..\bin\Plugins\CSScriptNpp"
copy "bin\release\CSScriptNpp\*.exe" "..\..\bin\Plugins\CSScriptNpp"

copy "..\..\readme.txt" "..\..\bin\readme.txt"
copy "..\..\license.txt" "..\..\bin\license.txt"

pause