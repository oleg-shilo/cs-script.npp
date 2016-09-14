echo off

set programfiles=%PROGRAMFILES(X86)%

echo "%programfiles%\Notepad++\plugins\CSScriptNpp"

md "%programfiles%\Notepad++\plugins\CSScriptNpp"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\MDbg"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn"
md "%programfiles%\Notepad++\plugins\CSScriptNpp\Roslyn.Intellisense"

rem CSScriptIntellisense.dll cannot be copied from build events as it would copy the assembly before DllExport is performed
rem so it needs to be done manually.

set plugins=%programfiles%\Notepad++\plugins

copy "CSScriptIntellisense\bin\Debug\CSScriptIntellisense.dll" "%plugins%\CSScriptNpp\CSScriptIntellisense.dll"
copy "CSScriptIntellisense\bin\Debug\CSharpIntellisense\*.dll" "%plugins%\CSScriptNpp"

copy "CSScriptNpp\bin\Debug\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\*.exe" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\*.pdb" "%plugins%\CSScriptNpp"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\Mdbg\*.dll" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\Updater.exe" "%plugins%\CSScriptNpp\Updater.exe"
copy "CSScriptNpp\CSScriptNpp\npp_jit.exe" "%plugins%\CSScriptNpp\npp_jit.exe"
copy "CSScriptNpp\CSScriptNpp\7z.exe" "%plugins%\CSScriptNpp\7z.exe"
copy "CSScriptNpp\CSScriptNpp\7z.dll" "%plugins%\CSScriptNpp\7z.dll"

copy "CSScriptNpp\CSScriptNpp\cscs.exe" "%plugins%\CSScriptNpp\cscs.exe"
copy "CSScriptNpp\CSScriptNpp\csws.exe" "%plugins%\CSScriptNpp\csws.exe"
copy "CSScriptNpp\CSScriptNpp\cscs.v3.5.exe" "%plugins%\CSScriptNpp\cscs.v3.5.exe"

copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.dll" "%plugins%\CSScriptNpp\CSScriptLibrary.dll"
copy "CSScriptNpp\CSScriptNpp\CSScriptLibrary.xml" "%plugins%\CSScriptNpp\CSScriptLibrary.xml"

copy "CSScriptNpp\CSScriptNpp\MDbg\*.pdb" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\bin\Debug\CSScriptNpp\Mdbg\*.exe" "%plugins%\CSScriptNpp\Mdbg"
copy "CSScriptNpp\CSScriptNpp\Mdbg\mdbghost*.exe" "%plugins%\CSScriptNpp\Mdbg"

copy "CSScriptNpp\CSScriptNpp\roslyn\csc.exe" "%plugins%\CSScriptNpp\Roslyn\csc.exe"
copy "CSScriptNpp\CSScriptNpp\roslyn\CSSCodeProvider.v4.6.dll" "%plugins%\CSScriptNpp\Roslyn\CSSCodeProvider.v4.6.dll"
copy "CSScriptNpp\CSScriptNpp\CSSCodeProvider.v4.0.dll" "%plugins%\CSScriptNpp\CSSCodeProvider.v4.0.dll"

copy "CSScriptNpp\CSScriptNpp\roslyn\VBCSCompiler.exe" "%plugins%\CSScriptNpp\Roslyn\VBCSCompiler.exe"
copy "CSScriptNpp\CSScriptNpp\roslyn\VBCSCompiler.exe.config" "%plugins%\CSScriptNpp\Roslyn\VBCSCompiler.exe.config"
echo ---------------------
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.Build.Tasks.CodeAnalysis.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.CSharp.Workspaces.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.CSharp.Workspaces.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.VisualBasic.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.VisualBasic.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.Workspaces.Desktop.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.Workspaces.Desktop.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.Workspaces.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.Workspaces.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.AttributedModel.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.AttributedModel.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Convention.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.Convention.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Hosting.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.Hosting.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.Runtime.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.Runtime.dll"
rem copy "CSScriptNpp\CSScriptNpp\roslyn\System.Composition.TypedParts.dll" "%plugins%\CSScriptNpp\Roslyn\System.Composition.TypedParts.dll"
echo ---------------------
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.CSharp.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.CSharp.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeAnalysis.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeAnalysis.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll" "%plugins%\CSScriptNpp\Roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Collections.Immutable.dll" "%plugins%\CSScriptNpp\Roslyn\System.Collections.Immutable.dll"
copy "CSScriptNpp\CSScriptNpp\roslyn\System.Reflection.Metadata.dll" "%plugins%\CSScriptNpp\Roslyn\System.Reflection.Metadata.dll"

copy "Roslyn.Intellisesne\Roslyn.Intellisense\bin\Debug\*.*" "%plugins%\CSScriptNpp\Roslyn.Intellisense"

rem need to keep it last so copy errors (if any) are visible
copy "CSScriptNpp\bin\Debug\CSScriptNpp.dll" "%plugins%\CSScriptNpp.dll"
pause