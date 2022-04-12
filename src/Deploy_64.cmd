echo off

set config=Debug
set cpu=x64

rem set dest_root=C:\Users\master\AppData\Local\Notepad++\plugins\CSScriptNpp
set dest_root=C:\Program Files\Notepad++\plugins\CSScriptNpp

set plugin_root=%dest_root%

md "%plugin_root%\CSScriptNpp"
md "%plugin_root%\CSScriptNpp\Mdbg"

echo -------------------
rem xcopy output\plugins\CSScriptNpp\*.* "%plugin_root%\CSScriptNpp" /Y
copy "CSScriptNpp\bin\Debug\CSScriptNpp.dll" "%plugin_root%\CSScriptNpp\CSScriptNpp.dll"

rem xcopy output\plugins\CSScriptNpp\Mdbg\*.* "%plugin_root%\CSScriptNpp\Mdbg" /Y
rem move "%plugin_root%\CSScriptNpp\CSScriptNpp.dll" "%plugin_root%\CSScriptNpp\CSScriptNpp.asm.dll"
rem copy "output\plugins\CSScriptNpp.x64.dll" "%plugin_root%\CSScriptNpp.dll"

