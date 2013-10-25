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
    public class EmbeddedIntellisense
    {
        public static object GetMapOf(string code)
        {
            return UltraSharp.Cecil.Reflector.GetMapOf(code);
        }
    }

    //for legacy reasons this class implements loosely coupled Reflection based extension model. Eventually it will need to be migrated to the referencing base model (as EmbeddedIntellisense)  
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

        public static void EnsureIntellisenseIntegration()
        {
            //Debug.Assert(false);
            if (Config.Instance.IntegrateWithIntellisense)
            {
                if (!integrated)
                {
                    integrated = true;

                    Type plugin = GetIntellisensePlugin();
                    
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

                        routine = plugin.GetField("DisplayInOutputPanel");
                        Action<string> displayInOutputPanel = OutputPanel.DisplayInGenericOutputPanel;
                        routine.SetValue(null, displayInOutputPanel);

                        ensureCurrentFileParsed = plugin.GetMethod("EnsureCurrentFileParsed");
                        
                        var showConfigMethod = plugin.GetMethod("ShowConfig");
                        ShowConfig = () => showConfigMethod.Invoke(null, new object[0]);
                    }
                }
            }
            else
            {
                if (integrated)
                {
                    integrated = false;

                    ensureCurrentFileParsed = null;

                    Type plugin = GetIntellisensePlugin();
                    
                    if (plugin != null)
                    {
                        FieldInfo routine = plugin.GetField("ResolveCurrentFile");
                        if (routine != null)
                        {
                            Func<string> getCurrentScript = Npp.GetCurrentFile; //this is a default CSScriptIntellisense implementation
                            routine.SetValue(null, getCurrentScript);
                        }

                        //Just ignoring DisplayInOutputPanel as the default implementation is not known (lost)
                    }
                }
            }
        }

        public static Action ShowConfig = null;

        static Type GetIntellisensePlugin()
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
