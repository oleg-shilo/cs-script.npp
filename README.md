# CS-Script Plugin (CSScript.Npp)   
![](wiki/css_npp_logo_clear.png)

*This Notepad++ plugin requires .NET v4.0 or higher.* 

Total downloads via Notepad++ Plugin Manager: Total Downloads Count: ![](http://www.csscript.net/statistics/css.npp.count.jpeg)            
[Downloads Statistics](http://www.csscript.net/statistics/css.npp.stats.html)

Starting from v1.3 plugin delivers full support for VB.NET syntax. [More reading...](http://csscriptnpp.codeplex.com/wikipage?title=VB%20support)  

____
This plugin allows convenient editing and execution of the C# code (scripts).  It also allows the usual C# intellisense and project management tasks to be performed in a way very similar to the MS Visual Studio.

In addition to this, it provides generic debugging functionality (with the integrated Managed Debugger) as well as the ability to prepare C# scripts for the deployment packages (script+engine or self-contained executable).

Typically user opens the C# file with Notepad++ and after presses 'Load' button on the CS-Script toolbar the all features can be accessed through two Notepad++ dockable panels Project and Output panel. 


Features

Note: the default compiler engine of the plugin is Roslyn. The engine fully supports both C# and VB.NET syntax but there is some usability information for these syntaxes that you may need to be aware of. See C# 6 support for and VB.NET support details

Intellisense
CLR type members auto-complete (Ctrl+Space or type '.')
Add missing 'using' (Ctrl+.)
Show CLR type quick info. (Hover mouse over the type member)
Show Method Overloads popup. (F6 or type '(')
Go to definition (F12)
- in the source code
- in the reconstructed referenced assembly API interface (including XML documentation)
Smart Indentation
Formatting C# source code
CodeMap - panel with the class members of the current .cs document  
 
Based on 'plain vanilla' ECMA-compliant C# code
Inclusion of the dependency scripts via CS-Script directives
Implicit assembly referencing via automatic resolving namesspaces into assemblies
Explicit assembly referencing via CS-Script directives
Debug output interception
Console output interception
Conventional build/execution error reporting
Debugging
- Step Over
- Step In
- Step Out
- Set Next Statement
- Toggle breakpoint
- 'Call Stack' 
- 'Locals' 
Preparing the script deployment package so it can be executed outside of Notepad++.  
The plugin is a part of CS-Script tools for Notepad++ suite. All details on the system requirements, installation and usage can be found on CS-Script.Npp home page.
