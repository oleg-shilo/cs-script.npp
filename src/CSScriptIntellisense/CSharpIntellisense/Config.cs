using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptIntellisense
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values)
    /// is faster.
    /// </summary>
    public class Config : IniFile
    {
        public static string Location = Path.Combine(Npp.Editor.GetPluginsConfigDir(), "CSharpIntellisense");

        public static Shortcuts Shortcuts = new Shortcuts();
        public static Config Instance { get { return instance ?? (instance = new Config()); } }
        public static Config instance;

        public string Section = "Intellisense";

        Config()
        {
            base.file = Path.Combine(Location, "settings.ini");

            if (!Directory.Exists(Location))
                Directory.CreateDirectory(Location);
        }

        public string GetFileName()
        {
            return base.file;
        }

        public bool UseArrowToAccept = true;
        public bool UseTabToAccept = true;
        public bool CodeSnippetsEnabled = true;
        public bool InterceptCtrlSpace = true;
        public bool PostFormattingUndoCaretReset = true;
        public bool ShowQuickInfoInStatusBar = false;
        public bool ShowQuickInfoAsNativeNppTooltip = false;
        public bool UseMethodBrackets = false;
        public bool GoToDefinitionOnCtrlClick = true;
        public bool UseCmdContextMenu = true;
        public string ContextMenuCommands = "Go To Definition;Find All References;Auto-add missing 'usings';Rename... (Ctrl+R,R);Format Document";

        public bool UsingRoslyn => true;

        public bool DisableMethodInfo = false;
        public bool DisableMethodInfoAutoPopup = false;
        public bool AutoSuggestOnOpenEndLine = false;
        public bool FormatOnSave = false;
        public bool AutoInsertSingeSuggestion = false;
        public bool AutoSelectFirstSuggestion = true;
        public bool VbSupportEnabled = true;
        public bool FormatAsYouType = true;
        public string DefaultRefAsms = "System.Linq|System.Xml|System.Xml.Linq|System.Windows.Forms|System.Drawing|System.Core|Microsoft.CSharp";
        public string DefaultNamespaces = "System.Collections.Generic|System.Collections|System.Linq|System.Xml.Linq|System.Windows.Forms|System.Xml|Microsoft.CSharp|System.Drawing";
        public string DefaultSearchDirs = "%csscript_inc%";
        public int MemberInfoMaxCharWidth = 100;
        public int MemberInfoMaxLines = 15;
        public bool SmartIndenting = true;
        public bool HybridFormatting = true;
        public bool IgnoreDocExceptions = false;

        public void Save()
        {
            lock (this)
            {
                SetValue(Section, "UseArrowToAccept", UseArrowToAccept);
                SetValue(Section, "UseTabToAccept", UseTabToAccept);
                SetValue(Section, "InterceptCtrlSpace", InterceptCtrlSpace);
                SetValue(Section, "PostFormattingUndoCaretReset", PostFormattingUndoCaretReset);
                //SetValue(Section, "UseMethodBrackets", UseMethodBrackets);
                SetValue(Section, "CodeSnippetsEnabled", CodeSnippetsEnabled);
                SetValue(Section, "ShowQuickInfoInStatusBar", ShowQuickInfoInStatusBar);
                SetValue(Section, "ShowQuickInfoAsNativeNppTooltip", ShowQuickInfoAsNativeNppTooltip);
                SetValue(Section, "IgnoreDocExceptions", IgnoreDocExceptions);
                SetValue(Section, "SmartIndenting", SmartIndenting);
                SetValue(Section, "VbSupportEnabled", VbSupportEnabled);
                SetValue(Section, "DisableMethodInfo", DisableMethodInfo);
                SetValue(Section, "DisableMethodInfoAutoPopup", DisableMethodInfoAutoPopup);
                SetValue(Section, "AutoSelectFirstSuggestion", AutoSelectFirstSuggestion);
                SetValue(Section, "AutoSuggestOnOpenEndLine", AutoSuggestOnOpenEndLine);
                SetValue(Section, "FormatOnSave.v2", FormatOnSave);
                SetValue(Section, "AutoInsertSingeSuggestion", AutoInsertSingeSuggestion);
                SetValue(Section, "UseCmdContextMenu", UseCmdContextMenu);
                SetValue(Section, "GoToDefinitionOnCtrlClick", GoToDefinitionOnCtrlClick);
                SetValue(Section, "ContextMenuCommands", ContextMenuCommands);
                SetValue(Section, "MemberInfoMaxCharWidth", MemberInfoMaxCharWidth);
                SetValue(Section, "DefaultRefAsms", DefaultRefAsms);
                SetValue(Section, "DefaultSearchDirs", DefaultSearchDirs);
                SetValue(Section, "DefaultNamespaces", DefaultNamespaces);
                SetValue(Section, "MemberInfoMaxLines", MemberInfoMaxLines);
                SetValue(Section, "FormatAsYouType", FormatAsYouType);
                UpdateDefaultIncludeFile();
            }
        }

        void UpdateDefaultIncludeFile()
        {
            var defaultRefAssemblies = Config.Instance.DefaultRefAsms
                                                      .Split('|')
                                                      .Where(x => x.HasText())
                                                      .Select(x => $"//css_ref {x.Trim()};")
                                                      .ToArray();

            File.WriteAllLines(DefaultIncludeFile, defaultRefAssemblies);
        }

        internal string DefaultIncludeFile => Path.GetDirectoryName(base.file).PathJoin("include.cs");
        internal string DefaultInclude => $"//css_inc {Config.Instance.DefaultIncludeFile}" + Environment.NewLine;

        string NormalizePathDelimiters(string text)
        {
            if (text.Contains(',') && !text.Contains('|')) // old items separators
                return text.Replace(",", "|").Trim('|');
            return text.Trim('|');
        }

        public void Open()
        {
            //Debug.Assert(false);

            lock (this)
            {
                UseTabToAccept = GetValue(Section, "UseTabToAccept", UseTabToAccept);
                UseArrowToAccept = GetValue(Section, "UseArrowToAccept", UseArrowToAccept);
                InterceptCtrlSpace = GetValue(Section, "InterceptCtrlSpace", InterceptCtrlSpace);
                PostFormattingUndoCaretReset = GetValue(Section, "PostFormattingUndoCaretReset", PostFormattingUndoCaretReset);
                //UseMethodBrackets = GetValue(Section, "UseMethodBrackets", UseMethodBrackets);
                SmartIndenting = GetValue(Section, "SmartIndenting", SmartIndenting);
                CodeSnippetsEnabled = GetValue(Section, "CodeSnippetsEnabled", CodeSnippetsEnabled);
                FormatAsYouType = GetValue(Section, "FormatAsYouType", FormatAsYouType);
                ShowQuickInfoAsNativeNppTooltip = GetValue(Section, "ShowQuickInfoAsNativeNppTooltip", ShowQuickInfoAsNativeNppTooltip);
                IgnoreDocExceptions = GetValue(Section, "IgnoreDocExceptions", IgnoreDocExceptions);
                MemberInfoMaxCharWidth = GetValue(Section, "MemberInfoMaxCharWidth", MemberInfoMaxCharWidth);
                DefaultSearchDirs = NormalizePathDelimiters(GetValue(Section, "DefaultSearchDirs", DefaultSearchDirs));
                DefaultRefAsms = NormalizePathDelimiters(GetValue(Section, "DefaultRefAsms", DefaultRefAsms));
                DefaultNamespaces = NormalizePathDelimiters(GetValue(Section, "DefaultNamespaces", DefaultNamespaces));
                VbSupportEnabled = GetValue(Section, "VbSupportEnabled", VbSupportEnabled);
                MemberInfoMaxLines = GetValue(Section, "MemberInfoMaxLines", MemberInfoMaxLines);
                DisableMethodInfo = GetValue(Section, "DisableMethodInfo", DisableMethodInfo);
                AutoSelectFirstSuggestion = GetValue(Section, "AutoSelectFirstSuggestion", AutoSelectFirstSuggestion);
                DisableMethodInfoAutoPopup = GetValue(Section, "DisableMethodInfoAutoPopup", DisableMethodInfoAutoPopup);
                FormatOnSave = GetValue(Section, "FormatOnSave.v2", FormatOnSave);
                AutoSuggestOnOpenEndLine = GetValue(Section, "AutoSuggestOnOpenEndLine", AutoSuggestOnOpenEndLine);
                AutoInsertSingeSuggestion = GetValue(Section, "AutoInsertSingeSuggestion", AutoInsertSingeSuggestion);
                GoToDefinitionOnCtrlClick = GetValue(Section, "GoToDefinitionOnCtrlClick", GoToDefinitionOnCtrlClick);
                ContextMenuCommands = GetValue(Section, "ContextMenuCommands", ContextMenuCommands);
                UseCmdContextMenu = GetValue(Section, "UseCmdContextMenu", ref contextMenuCommandsJustConfigured, UseCmdContextMenu);

                if (contextMenuCommandsJustConfigured)
                    Save();
            }

            ProcessContextMenuVisibility();

            UpdateDefaultIncludeFile();
        }

        bool contextMenuCommandsJustConfigured = false;

        public bool ProcessContextMenuVisibility()
        {
            bool updated = false;
            try
            {
                var currentProcessExe = System.Diagnostics.Process.GetCurrentProcess().Modules[0].FileName;
                if (!currentProcessExe.EndsWith("notepad++.exe", System.StringComparison.OrdinalIgnoreCase))
                    return false;

                var lines = File.ReadAllLines(npp.ContextMenuFile).ToList();
                bool actuallyConfigured = lines.Any(x => x.Contains("PluginEntryName=\"CS-Script\""));
                if (UseCmdContextMenu != actuallyConfigured)
                {
                    if (!UseCmdContextMenu)
                    {
                        //remove
                        int separator = lines.IndexOfFirst(x => x.Contains("<Item id = \"0\" />"));
                        int lastItem = lines.IndexOfLast(x => x.Contains("PluginEntryName=\"CS-Script\""));

                        if (separator != lastItem + 1)
                            separator = -1; //not our separator

                        if (separator != -1)
                            lines.RemoveAt(separator);
                        lines.RemoveAll(x => x.Contains("PluginEntryName=\"CS-Script\""));

                        File.WriteAllLines(npp.ContextMenuFile, lines.ToArray());
                        updated = true;
                    }
                    else
                    {
                        //insert
                        int start = lines.IndexOfFirst(x => x.Contains("<Item "));

                        var group = "C# Intellisense";
                        var plugin = "CS-Script";
                        var commands = this.ContextMenuCommands.Split(';')
                                           .Select(x => x.Trim())
                                           .Where(x => !string.IsNullOrEmpty(x))
                                           .Reverse();

                        lines.Insert(start, "        <Item id = \"0\" />");
                        foreach (var item in commands)
                            lines.Insert(start, $"        <Item FolderName=\"{group}\" PluginEntryName=\"{plugin}\" PluginCommandItemName=\"{item}\" ItemNameAs=\"{item}\"/>");

                        File.WriteAllLines(npp.ContextMenuFile, lines.ToArray());
                        updated = true;
                    }
                }
            }
            catch
            {
                try
                {
                    UseCmdContextMenu = File.ReadAllLines(npp.ContextMenuFile)
                                            .Any(x => x.Contains("PluginEntryName=\"CS-Script\""));
                }
                catch { }
            }

            if (contextMenuCommandsJustConfigured)
            {
                contextMenuCommandsJustConfigured = false;
                MessageBox.Show("Notepad++ context menu has been updated as the result of the plugin installation/update.\n\nThe changes will take effect only after Notepad++ is restarted.", "CS-Script");
            }
            return updated;
        }
    }
}