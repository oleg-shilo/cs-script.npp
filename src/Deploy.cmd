echo off

set config=Debug
set cpu=x86
set dest_root=C:\Program Files (x86)

set plugin_root=%dest_root%\Notepad++ (32)\plugins

md "%plugin_root%\CSScriptNpp"
md "%plugin_root%\CSScriptNpp\Mdbg"

echo -------------------
xcopy output\plugins\CSScriptNpp\*.* "%plugin_root%\CSScriptNpp" /Y
xcopy output\plugins\CSScriptNpp\Mdbg\*.* "%plugin_root%\CSScriptNpp\Mdbg" /Y
rem move "%plugin_root%\CSScriptNpp\CSScriptNpp.dll" "%plugin_root%\CSScriptNpp\CSScriptNpp.asm.dll"
copy "output\plugins\CSScriptNpp.x64.dll" "%plugin_root%\CSScriptNpp\CSScriptNpp.dll"
