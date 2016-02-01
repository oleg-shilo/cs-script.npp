using ICSharpCode.NRefactory.Completion;
using System.Collections.Generic;

namespace CSScriptIntellisense
{
    public class CssCompletionData : ICompletionData
    {
        public CompletionCategory CompletionCategory { get; set; }
        public string CompletionText { get; set; }
        public string Description { get; set; }
        public DisplayFlags DisplayFlags { get; set; }
        public string DisplayText { get; set; }
        public bool HasOverloads { get; set; }
        public IEnumerable<ICompletionData> OverloadedData { get { return new ICompletionData[0]; } }

        public void AddOverload(ICompletionData data)
        {
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
    }
}