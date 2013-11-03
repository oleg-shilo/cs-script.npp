echo off

md "%programfiles%\Notepad++\plugins\CSScriptNpp"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

copy "CSScriptIntellisense\bin\Debug\CSScriptIntellisense.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\CSharpIntellisense\*.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp"

copy "CSScriptNpp\bin\Debug\CSScriptNpp.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\*.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp"



pause