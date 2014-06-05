echo off 

del CSScriptNpp*.msi
del CSScriptNpp*.zip
del CSScriptNpp*.7z
del CSScriptNpp*.txt

move latest_version.txt latest_version.txt_
move latest_version_dbg.txt latest_version_dbg.txt_

"C:\Program Files\7-Zip\7z.exe" a -t7z CSScriptNpp.7z *.txt Plugins
"C:\Program Files\7-Zip\7z.exe" a CSScriptNpp.zip *.txt Plugins

cscs /l setup
cscs /l package

copy CSScriptNpp*.7z ..\..\..\..\..\Dropbox\Public\CS-S_NPP
copy CSScriptNpp*.zip ..\..\..\..\..\Dropbox\Public\CS-S_NPP
copy CSScriptNpp*.msi ..\..\..\..\..\Dropbox\Public\CS-S_NPP

echo Finalizing...
move latest_version.txt_ latest_version.txt
move latest_version_dbg.txt_ latest_version_dbg.txt

pause