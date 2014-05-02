echo off
md "..\bin\Plugins\CSScriptNpp"
md "..\bin\Plugins\CSScriptNpp\Mdbg"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\CSharpIntellisense\*.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp"

copy "CSScriptNpp\bin\Release\CSScriptNpp.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Release\CSScriptNpp\*.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "%programfiles%\Notepad++\plugins\CSScriptNpp"
copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\7z.dll"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\Mdbg"

copy "CSScriptIntellisense\bin\Release\CSScriptIntellisense.dll"  "..\bin\Plugins\CSScriptNpp"
copy "CSScriptIntellisense\CSharpIntellisense\*.dll" "..\bin\Plugins\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp.dll" "..\bin\Plugins\CSScriptNpp.dll"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.exe" "..\bin\Plugins\CSScriptNpp"
copy "CSScriptNpp\bin\release\CSScriptNpp\*.pdb" "..\bin\Plugins\CSScriptNpp"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.dll" "..\bin\Plugins\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Release\CSScriptNpp\Mdbg\*.exe" "..\bin\Plugins\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\Updater.exe" "..\bin\Plugins\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "..\bin\Plugins\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "..\bin\Plugins\CSScriptNpp\7z.dll"

copy "..\readme.txt" "..\bin\readme.txt"
copy "..\license.txt" "..\bin\license.txt"

pause