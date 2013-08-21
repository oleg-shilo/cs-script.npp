using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSScriptLibrary;

namespace CSScriptNpp
{
    public class Intellisense
    {
        static bool integrated = false;
        static MethodInfo ensureCurrentFileParsed;

        public static void Refresh()
        {
            EnsureIntellisenseIntegration();

            if (ensureCurrentFileParsed != null)
                ensureCurrentFileParsed.Invoke(null, new object[0]);
        }

        static void EnsureIntellisenseIntegration()
        {
            if (Config.Instance.IntegratewithIntellisense)
            {
                if (!integrated)
                {
                    integrated = true;

                    Type plugin = GetIntellisencePlugin();
                    
                    if (plugin != null)
                    {
                        FieldInfo routine = plugin.GetField("ResolveCurrentFile");

                        Func<string> getCurrentScript = () =>
                        {
                            if (string.IsNullOrEmpty(ProjectPanel.currentScript))
                                return Npp.GetCurrentFile();
                            else
                                return ProjectPanel.currentScript;
                        };
                        routine.SetValue(null, getCurrentScript);

                        ensureCurrentFileParsed = plugin.GetMethod("EnsureCurrentFileParsed");
                    }
                }
            }
            else
            {
                if (integrated)
                {
                    integrated = false;

                    ensureCurrentFileParsed = null;

                    Type plugin = GetIntellisencePlugin();
                    
                    if (plugin != null)
                    {
                        FieldInfo routine = plugin.GetField("ResolveCurrentFile");
                        if (routine != null)
                        {
                            Func<string> getCurrentScript = Npp.GetCurrentFile; //this is a default CSScriptIntellisense implementation
                            routine.SetValue(null, getCurrentScript);
                        }
                    }
                }
            }
        }

        static Type GetIntellisencePlugin()
        {
            //this implementation allows old "stand-alone" CSScriptIntellisense.dll to be integrated with CSScriptNpp as well as the new embedded CSScriptIntellisense.dll

            bool useChildPlugin = true;
            if (useChildPlugin)
            {
                return typeof(CSScriptIntellisense.Plugin);
            }
            else
            {
                var intellisenseAsm = AppDomain.CurrentDomain
                                               .GetAssemblies()
                                               .Where(asm => asm.FullName.StartsWith("CSScriptIntellisense"))
                                               .FirstOrDefault();

                if (intellisenseAsm != null)
                    return intellisenseAsm.GetType("CSScriptIntellisense.Plugin");
                else
                    return null;
            }
        }
    }
}
