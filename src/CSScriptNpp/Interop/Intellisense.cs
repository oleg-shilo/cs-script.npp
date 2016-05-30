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
            //Merge Configs
            CSScriptIntellisense.Config.Location = CSScriptNpp.Config.Location;
            CSScriptIntellisense.Plugin.SuppressCodeTolltips = () => Debugger.IsInBreak;
            CSScriptIntellisense.Plugin.DisplayInOutputPanel = message =>
            {
                Plugin.EnsureOutputPanelVisible();
                OutputPanel.DisplayInGenericOutputPanel(message);
            };

            CSScriptIntellisense.Plugin.ResolveCurrentFile =
                () =>
                {
                    if (string.IsNullOrEmpty(ProjectPanel.currentScript))
                        return Npp.GetCurrentFile();
                    else
                        return ProjectPanel.currentScript;
                };
        }
    }
}