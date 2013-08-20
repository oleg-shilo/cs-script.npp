echo off
md "%programfiles%\Notepad++\plugins\CSScriptNpp"

copy "bin\Debug\CSScriptNpp.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp.dll"
copy "bin\Debug\CSScriptNpp\CSScriptLibrary.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\CSScriptLibrary.dll"
copy "bin\Debug\CSScriptNpp\cscs.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\cscs.exe"

pause