### *More detailed feature description by categories:*
* __*[Intellisense](intellisense.md)*__
* __*[Script Execution](execution.md)*__
----
## Installlation

*System Requirements:* CS-Script plugin requires .NET 4.0 or higher.

You can install the plugin just by activating it from the Notepad++ Plugin Manager:
![](plugin_manager.png)

*The plugin brings an ultimate C# auto-completion (Intellisense) thus it is highly recommended that you disable Notepad++ own auto-completion (e.g. for C# files). Otherwise both auto-completions can start "competing for your attention". See this [article](npp_auto-complete.md) for details.*

To install plugin manually unpack the content of the CSScriptNpp.x.x.x.x.7z (https://github.com/oleg-shilo/cs-script.npp/releases) and copy all files from the `<archive>\Plugins` folder in the `<Program Files>\Notepad++\plugins` directory. 
Though be careful as manual install can lead to the assembly locking problem. You can find the details of the correct manual install procedure [here](manual_installation.md).

## Features

The plugin implements C# Intellisense and allows convenient execution and debugging of the C# scripts (based on the [CS-Script](https://github.com/oleg-shilo/cs-script) execution model). The features include:

* Intellisense
  * CLR type members auto-complete (Ctrl+Space or type '.')
  * Add missing 'using' (Ctrl+.)
  * Show CLR type quick info. (Hover mouse over the type member)
  * Show Method Overloads popup. (F6 or type '(')
  * Go to definition (F12)
    * in the source code
    * in the reconstructed referenced assembly API interface (including XML documentation)
  * Smart Indentation
  * Formatting C# source code
  * CodeMap - panel with the class members of the current .cs document  
* Based on 'plain vanilla' ECMA-compliant C# code
* [Inclusion](http://www.csscript.net/help/Importing_scripts.html) of the dependency scripts via CS-Script directives
* Implicit assembly referencing via automatic resolving namesapaces into assemblies
* [Explicit](http://www.csscript.net/help/using_.net_assemblies.html) assembly referencing via CS-Script directives
* Debug output interception
* Console output interception
* Conventional build/execution error reporting
* Preparing the script deployment package so it can be executed outside of Notepad++.  
* Debugging
  * Step Over
  * Step In
  * Step Out
  * Set Next Statement
  * Toggle breakpoint
  * 'Call Stack' 
  * 'Locals' 
  * 'Quick Watch'
----

The following are the additional online resources where you can find details about CS-Script.Npp:

* Some commonly asked CS-Script questions can be found here: [FAQ](faq.md)
* ![](http://www.codeproject.com/favicon.ico)[CodeProject article about CS-Script plugin](http://www.codeproject.com/Articles/694248/Sharpening-Notepadplusplus)

