using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Intellisense.Common;

namespace CSScriptIntellisense
{
    internal class Roslyn
    {
        public static string LocateInPluginDir(string fileName, params string[] subDirs)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var item in subDirs)
            {
                var file = Path.Combine(dir, item, fileName);
                if (File.Exists(file))
                    return file;
            }

            return Path.Combine(dir, fileName);
        }
    }

    public class RoslynCompletionEngine
    {
        public delegate void D2(string s);

        static IEngine engine;
        static Assembly intellisense;

        public static IEngine GetInstance()
        {
            Init();
            return engine;
        }

        static bool compatibilityErrorShowing = false;

        static bool interactive
        {
            get { return Environment.GetEnvironmentVariable("NPP_HOSTING") != null; }
        }

        static void WithCompatibilityCheck(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                try
                {
                    // Debug.Assert(false);
                    var host = Assembly.GetEntryAssembly();

                    ////this assembly is already in the plugin dir
                    //var roslynDll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Microsoft.CodeAnalysis.dll");
                    //bool invalidRoslynDeployment = File.Exists(roslynDll); //it supposed to be in "Roslyn" sub-folder but not in the root dir

                    engine = null;
                    if (!compatibilityErrorShowing)
                    {
                        compatibilityErrorShowing = true; //WithCompatibilityCheck can be invoked multiple times from non-UI threads

                        var message = "Cannot Load Roslyn.\n" +
                                      "Roslyn Intellisense will be disabled for this session and the default engine will be used instead." +

                                     "\n\nThe problem can be caused by:\n" +
                                     "  - the absence of .NET 4.6\n" +
                                     "  - Plugin Manager incorrectly deploying plugin files\n" +
                                     "  - Roslyn assemblies internal runtime conflicts.\n" +
                                     "    In this case restarting of Notepad++ usually helps.\n" +
                                     "\n\nError: " + e.Message;

                        //if (invalidRoslynDeployment)
                        //    message = "Cannot Load Roslyn.\n" +
                        //              "Roslyn Intellisense will be disabled for this session and the default engine will be used instead." +
                        //              "\n\nThe problem is caused by the Plugin Manager incorrectly deploying plugin files (problem is reported and acknowledged)." +
                        //              "\nIf you want to use C#7 and don't want to wait for Plugin Manager fix you may need to remove the plugin and install it manually from https://github.com/oleg-shilo/cs-script.npp.";

                        if (interactive)
                            MessageBox.Show(message, "CS-Script");
                        compatibilityErrorShowing = false;
                    }

                    Config.Instance.RoslynIntellisensePerSession = false;

                    e.LogAsDebug();
                }
                catch { }
            }
        }

        public static void Init()
        {
            if (engine != null) return;

            WithCompatibilityCheck(() =>
            {
                var file = Roslyn.LocateInPluginDir("RoslynIntellisense.exe", "Roslyn", @".\", Environment.CurrentDirectory);

                if (!File.Exists(file))
                {
                    var path = Environment.GetEnvironmentVariable("roslynintellisense_path") ?? "";
                    if (File.Exists(path))
                        file = path;
                }

                intellisense = Assembly.LoadFrom(file);
                engine = (IEngine)intellisense.CreateInstance("RoslynIntellisense.Engine");
                engine.SetOption("ReflectionOutDir", Path.Combine(Path.GetTempPath(), "CSScriptNpp\\ReflctedTypes"));
            });

            if (engine != null)
                Task.Factory.StartNew(() => WithCompatibilityCheck(engine.Preload));
        }

        static public GetAutocompletionForDlgt GetAutocompletionFor;

        public delegate IEnumerable<ICompletionData> GetAutocompletionForDlgt(string code, int position, string[] references, string[] includes);

        public IEnumerable<ICompletionData> GetCompletionData(string editorText, int offset, string fileName, bool isControlSpace = true, bool prepareForDisplay = true) // not the best way to put in the whole string every time
        {
            try
            {
                //if (Project == null || string.IsNullOrEmpty(editorText))
                if (string.IsNullOrEmpty(editorText))
                    return new ICompletionData[0];

                if (editorText.Length <= offset)
                    offset = editorText.Length - 1;

                return GetAutocompletionFor(editorText, offset, null, null);
                //return GetCSharpCompletionData(doc, editorText, offset, fileName, isControlSpace, prepareForDisplay);
            }
            catch
            {
                return new ICompletionData[0]; //the exception can happens even for the internal NRefactor-related reasons
            }
        }
    }
}