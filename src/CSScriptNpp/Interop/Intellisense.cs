using System.Diagnostics;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp
{
    public class Intellisense
    {
        public static object GetMapOf(string code)
        {
            return UltraSharp.Cecil.Reflector.GetMapOf(code);
        }

        public static void EnsureIntellisenseIntegration()
        {
            //Debug.Assert(false);
            //Merge Configs
            CSScriptIntellisense.Config.Location = PluginEnv.ConfigDir;
            CSScriptNpp.Config.InitData(); //will also exchange required data between configs

            // CSScriptIntellisense.Plugin.SuppressCodeTolltips = () => Debugger.IsInBreak || npp.ShowingModalDialog;
            CSScriptIntellisense.Plugin.DisplayInOutputPanel = message =>
            {
                Plugin.EnsureOutputPanelVisible();
                OutputPanel.DisplayInGenericOutputPanel(message);
            };

            CSScriptIntellisense.Plugin.ResolveCurrentFile =
                () =>
                {
                    if (string.IsNullOrEmpty(ProjectPanel.currentScript))
                        return Npp.Editor.GetCurrentFilePath();
                    else
                        return ProjectPanel.currentScript;
                };
        }
    }
}