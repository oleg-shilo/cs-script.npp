# CS-Script Plugin (CSScript.Npp)   
<img align="right" src="wiki/css_npp_logo_clear.png" alt="" style="float:right">

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://oleg-shilo.github.io/cs-script/Donation.html)

[![Github All Releases](https://img.shields.io/github/downloads/oleg-shilo/cs-script.npp/total.svg)]()

[Download latest release](https://github.com/oleg-shilo/cs-script.npp/releases)

*This Notepad++ plugin requires .NET v4.0 or higher.* 

----

### .NET Framework -> .NET  transition 

_The current release of CS-Script targets .NET Core family runtime (.NET 6).<br> 
CS-Script started targeting .NET 5+ about a year ago as .NET Framework further development has been efectively cancelled by teh .NET team._ 

_Though Notepad++ CS-Script plugin is still shipped with the engine targeting by default .NET Framework. This is done as a reflection of the fact that many users of this plugin are still relying on .NET Framework.<br>_
_However the plugin can be configured to use the .NET Core version of the script engine (you can install it from [here](https://github.com/oleg-shilo/cs-script/releases)). <br>_
_The next version of the pluging will still maintain .NET Framework engine but by default will trry to auto integrate with .NET Core.
And the next release after that will have the .NET Framework option only availabe as a manual configuration/deployment step._

----

Total downloads via Notepad++ Plugin Manager: Total Downloads Count: ![](http://www.csscript.net/statistics/css.npp.count.jpeg)            
[Downloads Statistics](http://www.csscript.net/statistics/css.npp.stats.html)

\* _statistics does not include x64 downloads nor downloads after Notepad++ discontinued shiping editor with the x86 plugin manager included_ 

Starting from v1.3 plugin delivers full support for VB.NET syntax. [More reading...](https://github.com/oleg-shilo/cs-script.npp/wiki/VB.NET-Support)  

You can also run scripts targeting [.NET 5/Core runtime](https://github.com/oleg-shilo/cs-script.npp/wiki/.NET-Core).
____
This plugin allows convenient editing and execution of the C# code (scripts).  It also allows the usual C# intellisense and project management tasks to be performed in a way very similar to the MS Visual Studio.

In addition to this, it provides generic debugging functionality (with the integrated Managed Debugger) as well as the ability to prepare C# scripts for the deployment packages (script+engine or self-contained executable).

Typically user opens the C# file with Notepad++ and after presses 'Load' button on the CS-Script toolbar the all features can be accessed through two Notepad++ dockable panels Project and Output panel. 

![](wiki/css_npp.gif)

![](wiki/debugger.png)

## Features

Note: the default compiler engine of the plugin is Roslyn. The engine fully supports both C# and VB.NET syntax but there is some usability information for these syntaxes that you may need to be aware of. See [C# 7 support](https://github.com/oleg-shilo/cs-script.npp/wiki/C%23-7-support) for and [VB.NET support](https://github.com/oleg-shilo/cs-script.npp/wiki/VB.NET-Support) details.

* Intellisense
  * CLR type members auto-complete (Ctrl+Space or type '.')
  * Add missing 'using' (Ctrl+.)
  * Show CLR type quick info. (Hover mouse over the type member)
  * Show Method Overloads popup. (F6 or type '(')
  * Go to definition (F12)
    - in the source code
    - in the reconstructed referenced assembly API interface (including XML documentation)
  * Smart Indentation
  * Formatting C# source code
  * CodeMap - panel with the class members of the current .cs document  
 
* Based on 'plain vanilla' ECMA-compliant C# code
* Inclusion of the dependency scripts via CS-Script directives
* Implicit assembly referencing via automatic resolving namespaces into assemblies
* Explicit assembly referencing via CS-Script directives
* Debug output interception
* Console output interception
* Conventional build/execution error reporting
* Debugging
  - Step Over
  - Step In
  - Step Out
  - Set Next Statement
  - Toggle breakpoint
  - 'Call Stack' 
  - 'Locals' 
Preparing the script deployment package so it can be executed outside of Notepad++.  
The plugin is a part of CS-Script tools for Notepad++ suite. All details on the system requirements, installation and usage can be found on CS-Script.Npp home page.

## Usage

After the installation start Notepad++ and click "Project Panel" button on the toolbar (or "Project Panel" menu item in the Plugins->CS-Script menu). 

![](wiki/toolbar.png)

Then Click 'New Script' button. The script is ready. Just press F5 and see the script being executed.

![](wiki/new_script.png)

![](wiki/CSScript.png)
