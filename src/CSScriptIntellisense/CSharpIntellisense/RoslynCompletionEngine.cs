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
            var file = Path.Combine(dir, fileName);

            foreach (var item in subDirs)
                if (File.Exists(file))
                    return file;
                else
                    file = Path.Combine(dir, item, fileName);

            return file;
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
                    engine = null;
                    if (!compatibilityErrorShowing)
                    {
                        compatibilityErrorShowing = true; //WithCompatibilityCheck can be invoked multiple times from non-UI threads
                        MessageBox.Show("Cannot use Roslyn Intelisesnse.\nError: " + e.Message + "\n\nThis can be caused by the absence of .NET 4.6.\n\nRoslyn Intellisense will be disabled and default engine will be used instead. You can always reenable Roslyn Intellisense from the settings dialog.", "CS-Script");
                        compatibilityErrorShowing = false;
                    }
                    Config.Instance.RoslynIntellisense = false;
                    Config.Instance.Save();
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
                var file = Roslyn.LocateInPluginDir("RoslynIntellisense.exe", "Roslyn.Intellisense");

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