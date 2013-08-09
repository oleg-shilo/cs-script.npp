md "..\..\bin\Plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp"

copy "bin\Release\CSScriptNpp.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp.dll"
copy "bin\Release\CSScriptNpp\CSScriptLibrary.dll" "%programfiles%\Notepad++\plugins\CSScriptNpp\CSScriptLibrary.dll"
copy "bin\Release\CSScriptNpp\cscs.exe" "%programfiles%\Notepad++\plugins\CSScriptNpp\cscs.exe"

copy "bin\release\CSScriptNpp.dll" "..\..\bin\Plugins\CSScriptNpp.dll"
copy "bin\release\CSScriptNpp\*.dll" "..\..\bin\Plugins\CSScriptNpp"
copy "bin\release\CSScriptNpp\*.exe" "..\..\bin\Plugins\CSScriptNpp"

copy "..\..\readme.txt" "..\..\bin\readme.txt"
copy "..\..\license.txt" "..\..\bin\license.txt"

pause