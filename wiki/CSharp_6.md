__*Obsolete. Follow to [C# 7](CSharp_7.md) page instead.*__

# C# 6 Support

Please note that the original code analysis engine NRefactory doesn't support C#6 specific features (e.g. string interpolation). This means that while the code containing C# 6.0 statements can be executed and debugged it cannot be parsed by NRefeactory and any Intellisense feature based on NRefectory will be effectively disabled.

To address this problem CS-Script.Npp starting from version v1.0.44 integrated alternative syntax analyser Roslyn, which supports C#6.0 syntax. This triggered gradual migration of the all features on Roslyng while maintaining NRefactory based features available until the migration is complete.

On practical level this means that You can always activate the C# 6 engine (Roslyn) from the settings dialog. But if you do so some of the Intellisense features may work only for the C# 5 features. The following info will help you to understand the level of support offered by both engines:

__*NRefactory engine*__

Enabled if Roslyn is not activated from the config dialog. All features work unless C# 6 code detected.
* Auto-completion 
* Code formatting
* Show Method info and Member info tooltip
* Find References
* Go to Definition

__*Roslyn engine*__

Can only be activated from the config dialog. Only some features have full C# 6 support
* Auto-completion  
* Code formatting

_The following features are work in progress. If Roslyn is activated the they will automatically fall back to NRefactory (which doesn't understand C#6)_

* Show Method info and Member info tooltip. 
* Find References
* Go to Definition


