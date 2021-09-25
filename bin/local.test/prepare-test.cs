//css_ng csc
////css_args 0
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using static dbg; // to use 'print' instead of 'dbg.print'

var npp = @"C:\Program Files\Notepad++\notepad++.exe";
var npp_original = npp + ".original";
var npp_debug = Path.GetFullPath(@".\notepad++.exe");

var gup = @"C:\Program Files\Notepad++\updater\GUP.exe";
var gup_original = gup + ".original";
var gup_debug = Path.GetFullPath(@".\GUP.exe");

var config = @"C:\Program Files\Notepad++\plugins\Config\nppPluginList.json";
var config_src = @".\..\src\pl.x64.json";

if (args.FirstOrDefault() == "0")
{
    print("Rollback binaries...");
    if (File.Exists(npp_original))
        File.Copy(npp_original, npp, true);

    if (File.Exists(gup_original))
        File.Copy(gup_original, gup, true);

    File.Delete(config);
}
else
{
    print("Prepare binaries for test...");

    if (!File.Exists(npp_original))
        File.Copy(npp, npp_original, true);

    if (!File.Exists(gup_original))
        File.Copy(gup, gup_original, true);

    File.Copy(npp_debug, npp, true);
    File.Copy(gup_debug, gup, true);

    File.Copy(config_src, config, true);
}