
## C# 7 Support

Support for C# 7 syntax does not come with .NET out of box. Meaning that even if you have .NET 4.6 installed it's compiling services are no capable of processing C# 7 syntax. Thus Visual Studio 2017 comes with the additional set of compiling tools (Roslyn) for handling the extended C# syntax. 

CS-Script Notepad++ plugin relies on the same compiling model. It is distributed with Roslyn compilers and code analysis services. These services are enabled by default on the fresh installation but you may need to enable them manually for the existing plugin installations if the Roslyn integration was disabled before:
![image](http://download-codeplex.sec.s-msft.com/Download?ProjectName=csscriptnpp&DownloadId=1651281)

Note, .NET does not include support for C# 7 tuples by default. Even in visual Studio 2017 it needs to be added as a NuGet package. In Notepad++ with CS-Script plugin you can add tuples support by adding reference to System.ValueTuple.dll.

```C#
//css_ref %CSScriptNpp_dir%\Roslyn\System.ValueTuple.dll
```

If you want to make the plugin to reference _System.ValueTuple.dll_ for all scripts then you can change the plugin settings to include is as a "default referenced assembly". You can do this by modifying the _settings.ini_ file via the "Edit settings file instead" link in the config dialog: 

![image](http://download-codeplex.sec.s-msft.com/Download?ProjectName=csscriptnpp&DownloadId=1651465)
