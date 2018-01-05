echo off

set config=Debug
set cpu=x64
set dest_root=C:\Program Files
REM set cpu=.
REM set dest_root=C:\Program Files (x86)

set plugin_root=%dest_root%\Notepad++\plugins
set src_dir=..

md "%plugin_root%\CSScriptNpp"
md "%plugin_root%\CSScriptNpp\Mdbg"

cd "%src_dir%"
copy NppPlugin.Host\bin\%cpu%\Debug\NppPlugin.Host.dll "%plugin_root%\CSScriptNpp.%cpu%.dll"

copy CSScriptNpp\bin\%config%\CSScriptNpp.dll "%plugin_root%\CSScriptNpp\CSScriptNpp.dll"
copy CSScriptIntellisense\bin\%config%\CSScriptIntellisense.dll "%plugin_root%\CSScriptNpp\CSScriptIntellisense.dll"

copy CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll "%plugin_root%\CSScriptNpp\CSScriptLibrary.dll"
copy CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml "%plugin_root%\CSScriptNpp\CSScriptLibrary.xml"
copy CSScriptNpp\CSScriptNpp\CSSRoslynProvider.dll "%plugin_root%\CSScriptNpp\CSSRoslynProvider.dll"
copy CSScriptNpp\CSScriptNpp\syntaxer.exe "%plugin_root%\CSScriptNpp\syntaxer.exe"
             
copy CSScriptIntellisense\CSharpIntellisense\Intellisense.Common.dll "%plugin_root%\CSScriptNpp\Intellisense.Common.dll"
copy CSScriptIntellisense\CSharpIntellisense\Mono.Cecil.dll "%plugin_root%\CSScriptNpp\Mono.Cecil.dll"
copy CSScriptIntellisense\CSharpIntellisense\ICSharpCode.NRefactory.dll "%plugin_root%\CSScriptNpp\ICSharpCode.NRefactory.dll"
copy CSScriptIntellisense\CSharpIntellisense\ICSharpCode.NRefactory.CSharp.dll "%plugin_root%\CSScriptNpp\ICSharpCode.NRefactory.CSharp.dll"

echo -------------------
xcopy CSScriptNpp\CSScriptNpp\Mdbg\*.dll "%plugin_root%\CSScriptNpp\Mdbg" /Y
xcopy CSScriptNpp\CSScriptNpp\Mdbg\*.exe "%plugin_root%\CSScriptNpp\Mdbg" /Y
echo -------------------
copy CSScriptNpp\CSScriptNpp\launcher.exe "%plugin_root%\CSScriptNpp\launcher.exe"
copy CSScriptNpp\CSScriptNpp\Updater.exe "%plugin_root%\CSScriptNpp\Updater.exe"
copy CSScriptNpp\CSScriptNpp\syntaxer.exe "%plugin_root%\CSScriptNpp\syntaxer.exe"
copy CSScriptNpp\CSScriptNpp\npp_jit.exe "%plugin_root%\CSScriptNpp\npp_jit.exe"

copy CSScriptNpp\CSScriptNpp\cscs.exe "%plugin_root%\CSScriptNpp\cscs.exe"
copy CSScriptNpp\CSScriptNpp\csws.exe "%plugin_root%\CSScriptNpp\csws.exe"
copy CSScriptNpp\CSScriptNpp\cscs.v3.5.exe "%plugin_root%\CSScriptNpp\cscs.v3.5.exe"

cd "NppPlugin.Host"