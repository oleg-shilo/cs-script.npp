echo off

set config=Debug
set cpu=x86
set dest_root=C:\Program Files (x86)

set plugin_root=%dest_root%\Notepad++ (32)\plugins

md "%plugin_root%\CSScriptNpp"
md "%plugin_root%\CSScriptNpp\Mdbg"

echo -------------------
copy "output\plugins\CSScriptNpp.x86.dll" "%plugin_root%\CSScriptNpp.x86.dll"
xcopy output\plugins\CSScriptNpp\*.* "%plugin_root%\CSScriptNpp" /Y
xcopy output\plugins\CSScriptNpp\Mdbg\*.* "%plugin_root%\CSScriptNpp\Mdbg" /Y

