echo off

set config=Debug
set cpu=x64
set dest_root=C:\Program Files

set plugin_root=%dest_root%\Notepad++\plugins

md "%plugin_root%\CSScriptNpp"
md "%plugin_root%\CSScriptNpp\Mdbg"

echo -------------------
copy "output\plugins\CSScriptNpp.x64.dll" "%plugin_root%\CSScriptNpp.dll"
xcopy output\plugins\CSScriptNpp\*.* "%plugin_root%\CSScriptNpp" /Y
xcopy output\plugins\CSScriptNpp\Mdbg\*.* "%plugin_root%\CSScriptNpp\Mdbg" /Y

