echo off

echo(
echo Patching host assemblies with the version info...

cscs -l ..\src\tools\set_host_version.cs

echo(
echo Clearing old files...

del .\CSScriptNpp*.msi  >nul 2>&1
del .\CSScriptNpp*.zip  >nul 2>&1
del .\CSScriptNpp*.7z   >nul 2>&1
del .\CSScriptNpp*.txt  >nul 2>&1
del .\CSScriptNpp*.html >nul 2>&1
del .\*.wixpdb          >nul 2>&1


del  .\plugins\* /s /Q  >nul 2>&1
rd .\plugins            >nul 2>&1
md .\plugins            >nul 2>&1

echo(
echo Agregating plugin files...
xcopy "..\src\output\plugins" ".\plugins" /s /Y
del ".\plugins\original_*"
del ".\plugins\*.zip"

move latest_version.txt latest_version.txt_                         >nul 2>&1
move latest_version.pre.txt latest_version.pre.txt_                 >nul 2>&1
move latest_version_dbg.txt latest_version_dbg.txt_                 >nul 2>&1

"C:\Program Files\7-Zip\7z.exe" a -t7z CSScriptNpp.x86.7z *.txt -x!plugins\CSScriptNpp.x64.dll plugins
"C:\Program Files\7-Zip\7z.exe" a -t7z CSScriptNpp.x64.7z *.txt -x!plugins\CSScriptNpp.x86.dll plugins

"C:\Program Files\7-Zip\7z.exe" a CSScriptNpp.x64.zip *.txt -x!plugins\CSScriptNpp.x86.dll plugins
"C:\Program Files\7-Zip\7z.exe" a CSScriptNpp.x86.zip *.txt -x!plugins\CSScriptNpp.x64.dll plugins

cscs /l setup
cscs /l package

echo Cleanup...
echo(
move latest_version.txt_ latest_version.txt                         >nul 2>&1
move latest_version.pre.txt_ latest_version.pre.txt                 >nul 2>&1
move latest_version_dbg.txt_ latest_version_dbg.txt                 >nul 2>&1

pause