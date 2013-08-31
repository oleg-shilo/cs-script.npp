echo off 

"C:\Program Files\7-Zip\7z.exe" a -t7z CSScriptNpp.7z *.txt Plugins
"C:\Program Files\7-Zip\7z.exe" a CSScriptNpp.zip *.txt Plugins
cscs /l setup

copy CSScriptNpp.7z ..\..\..\..\..\Dropbox\Public\CS-S_NPP\CSScriptNpp.7z
copy CSScriptNpp.zip ..\..\..\..\..\Dropbox\Public\CS-S_NPP\CSScriptNpp.zip
copy CSScriptNpp.msi ..\..\..\..\..\Dropbox\Public\CS-S_NPP\CSScriptNpp.msi

pause