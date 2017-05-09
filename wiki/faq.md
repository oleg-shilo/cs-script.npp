# Frequently Asked Questions

* ...the output window can’t display Chinese words...
If you want to intercept Unicode console output in the CSScriptNpp output panel, then ensure you have set the output encoding correctly in your script:
```c#
Console.OutputEncoding = System.Text.Encoding.UTF8;
```
Be aware that only Debug output panel does not support Unicode due to the Windows Debug API limitations. 

* ...compiling the Hello World example script gives the following error:...Method not found: 'Int32 System.Runtime.InteropServices.Marshal.SizeOf(!!0)'...
If you get the “Int32 System.Runtime.InteropServices.Marshal.SizeOf(!!0)” exception during the script execution then most likely you are using plugin version v1.0.17.0 which became accidentally dependent on .NET 4.5.1. 
You wouldn't have had this problem if you had this version of .NET installed but if you have only .NET 4.0 then you will need to update the plugin to v1.0.17.1 or higher.