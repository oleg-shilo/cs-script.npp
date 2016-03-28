using Intellisense.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CSScriptIntellisense
{
    public class CssCompletionData : ICompletionData
    {
        public CompletionCategory CompletionCategory { get; set; }
        public string CompletionText { get; set; }
        public string Description { get; set; }
        public DisplayFlags DisplayFlags { get; set; }
        public IconType Icon { get; set; }
        public string DisplayText { get; set; }
        public bool HasOverloads { get; set; }
        public IEnumerable<ICompletionData> OverloadedData { get { return new ICompletionData[0]; } }

        public void AddOverload(ICompletionData data)
        {
        }

        public static ICompletionData[] DefaultRefAsms
        {
            get
            {
                return Config.Instance.DefaultRefAsms
                                           .Split(';', ',')
                                           .Where(x => !string.IsNullOrWhiteSpace(x))
                                           .Select(x => x.Trim())
                                           .Select(x => new CssCompletionData { CompletionText = x, DisplayText = x, Icon = IconType._namespace })
                                           .ToArray();
            }
        }
        public static ICompletionData[] DefaultNamespaces
        {
            get
            {
                return Config.Instance.DefaultNamespaces
                                        .Split(';', ',')
                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                        .Select(x => x.Trim())
                                        .Select(x => new CssCompletionData { CompletionText = x, DisplayText = x, Icon = IconType._namespace })
                                        .ToArray();
            }
        }

        public static ICompletionData[] AllDirectives =
        new ICompletionData[]
        {
            //css_import <file>[, preserve_main][, rename_namespace(<oldName>, <newName>)];
            new CssCompletionData
            {
                CompletionText = "css_inc", DisplayText="//css_inc",
                Description =
@"'Include/Import script' CS-Script directive
//css_inc <file>;
//css_include <file>;

Example:
    //css_inc utils.cs;"
            },

            new CssCompletionData
            {
                CompletionText = "css_ref", DisplayText="//css_ref",
                Description =
@"'Reference assembly' CS-Script directive
//css_ref <file>;
//css_reference <file>;

Example:
    //css_ref ystem.Data.ComponentModel.dll;"
            },

            new CssCompletionData
            {
                CompletionText = "css_args", DisplayText="//css_args",
                Description =
@"'Set command-line arguments' CS-Script directive
//css_args arg0[,arg1]..[,argN];

Example:
    //css_args /dbg, /ac, ""argument one"";"
            },

            new CssCompletionData
            {
                CompletionText = "css_dir", DisplayText="//css_dir",
                Description =
@"'Set probing directory' CS-Script directive
//css_dir <path>;
//css_searchdir <path>;

Examples:
    //css_dir ..\\..\\MyAssemblies;
    //css_dir packages\\**"
            },

            new CssCompletionData {
                CompletionText = "css_nuget", DisplayText="//css_nuget",
                Description =
@"'Reference NuGet package' CS-Script directive
//css_nuget [-noref] [-force[:delay]] [-ver:<version>] [-ng:<nuget arguments>] package0[..[,packageN];

Examples:
    //css_nuget cs-script;
    //css_nuget -ver:4.1.2 NLog;
    //css_nuget -ver:""4.1.1-rc1"" -ng:""-Pre -NoCache"" NLog;"
            },
        };

        public CssCompletionData()
        {
            Icon = IconType.unresolved;
        }

        static string GetHelpFile()
        {
            if (CSScriptHelper.GetEngineExe() == null)
                return null;

            string file = Path.Combine(Path.GetTempPath(), "CSScriptNpp\\ReflctedTypes", "cs-script." + typeof(CSScriptLibrary.CSScript).Assembly.GetName().Version + ".help.txt");

            if (!File.Exists(file))
            {
                var dir = Path.GetDirectoryName(file);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                foreach (string oldFile in Directory.GetFiles(dir, "cs-script.*.help.txt"))
                    try { File.Delete(oldFile); } catch { }

                try
                {
                    string cmdText = string.Format("\"{0}\" > \"{1}\"", CSScriptHelper.GetEngineExe(), file);

                    var p = new Process();
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = string.Format("/C \"{0}\"", cmdText);
                    p.Start();
                    p.WaitForExit();
                }
                catch { }

                if (!File.Exists(file))
                    return null;
            }

            return file;
        }

        public static DomRegion? ResolveDefinition(string directive)
        {
            string helpFile = GetHelpFile();

            if (helpFile != null)
            {
                string[] lines = File.ReadAllLines(helpFile);

                var matchingLine = FindSection(directive, lines);

                if (matchingLine == -1 && (directive == "//css_inc" || directive == "//css_include"))
                {
                    directive = "//css_import"; //'include' aliases are described under main 'import' section in older documentation
                    matchingLine = FindSection(directive, lines);
                }

                if (matchingLine != -1)
                    return new DomRegion { FileName = helpFile, BeginLine = matchingLine + 1, BeginColumn = 0 }; //DomRegion is one based
            }

            return null;
        }

        static int FindSection(string directive, string[] lines)
        {
            string pattern1 = directive;
            string pattern2 = "Alias - " + directive;

            return lines.FindIndex(x => x.StartsWith(pattern1) || x.StartsWith(pattern2));
        }
    }
}