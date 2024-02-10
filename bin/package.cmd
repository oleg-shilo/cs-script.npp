echo off

echo(
echo Patching host assemblies with the version info...

rem  properly rename x86 and x64 host DLLs
css -l -dbg ..\src\tools\set_host_version.cs 

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
xcopy "..\src\CSScriptNpp\CSScriptNpp\cs-script" ".\plugins\CSScriptNpp\cs-script\" /s /Y
xcopy "..\src\CSScriptNpp\CSScriptNpp\cs-syntaxer" ".\plugins\CSScriptNpp\cs-syntaxer\" /s /Y

del ".\plugins\original_*"
del ".\plugins\*.zip"


move latest_version.txt latest_version.txt_                         >nul 2>&1
move latest_version.pre.txt latest_version.pre.txt_                 >nul 2>&1
move latest_version_dbg.txt latest_version_dbg.txt_                 >nul 2>&1

rem goto exit
cd plugins

copy CSScriptNpp.x86.dll CSScriptNpp.dll
"C:\Program Files\7-Zip\7z.exe" a ..\CSScriptNpp.x86.zip  *.dll -x!CSScriptNpp.*.dll CSScriptNpp

copy CSScriptNpp.x64.dll CSScriptNpp.dll
"C:\Program Files\7-Zip\7z.exe" a ..\CSScriptNpp.x64.zip  *.dll -x!CSScriptNpp.*.dll CSScriptNpp

cd ..

css sha256 CSScriptNpp.x64.zip > CSScriptNpp.x64.sha256.txt
css sha256 CSScriptNpp.x86.zip > CSScriptNpp.x86.sha256.txt
rem cscs /l md5 > CSScriptNpp.x64.M5.txt

rem rem cscs /l setup.CPU.cs
css /l package

echo Cleanup...
echo(
move latest_version.txt_ latest_version.txt                         >nul 2>&1
move latest_version.pre.txt_ latest_version.pre.txt                 >nul 2>&1
move latest_version_dbg.txt_ latest_version_dbg.txt                 >nul 2>&1

pause
:exit