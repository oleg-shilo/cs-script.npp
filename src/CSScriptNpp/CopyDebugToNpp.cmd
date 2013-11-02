echo off
md "%programfiles%\Notepad++\plugins\CSScriptNpp"

copy "bin\Debug\CSScriptNpp.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp.dll"
copy "bin\Debug\CSScriptNpp\*.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp"
copy "bin\Debug\CSScriptNpp\cscs.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\cscs.exe"
copy "bin\Debug\CSScriptNpp\cscs.v3.5.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\cscs.v3.5.exe"



pause