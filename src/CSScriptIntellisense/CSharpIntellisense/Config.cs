using System.Diagnostics;
using System.IO;

namespace CSScriptIntellisense
{
    /// <summary>
    /// Of course XML based config is more natural, however ini file reading (just a few values)
    /// is faster.
    /// </summary>
    public class Config : IniFile
    {
        public static string Location = Path.Combine(Npp.GetConfigDir(), "CSharpIntellisense");

        public static Shortcuts Shortcuts = new Shortcuts();
        public static Config Instance { get { return instance ?? (instance = new Config()); } }
        public static Config instance;

        public string Section = "Intellisense";
        public bool HostedByOtherPlugin = false;

        Config()
        {
            HostedByOtherPlugin = !Location.EndsWith("CSharpIntellisense");

            base.file = Path.Combine(Location, "settings.ini");

            if (!Directory.Exists(Location))
                Directory.CreateDirectory(Location);

            Open();
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
        public bool DisableMethodInfo = false;
        public bool FallbackFormatting = false;
        public bool RoslynFormatting = true;
        public bool FormatAsYouType = true;
        public int MemberInfoMaxCharWidth = 100;
        public int MemberInfoMaxLines = 15;
        public bool SmartIndenting = true;
        public bool HybridFormatting = true;
        public bool IgnoreDocExceptions = false;

        public void Save()
        {
            lock (this)
            {
                if (!HostedByOtherPlugin)
                    File.WriteAllText(this.file, ""); //clear to get rid of all obsolete values

                SetValue(Section, "UseArrowToAccept", UseArrowToAccept);
                SetValue(Section, "UseTabToAccept", UseTabToAccept);
                SetValue(Section, "InterceptCtrlSpace", InterceptCtrlSpace);
                SetValue(Section, "PostFormattingUndoCaretReset", PostFormattingUndoCaretReset);
                SetValue(Section, "UseMethodBrackets", UseMethodBrackets);
                SetValue(Section, "CodeSnippetsEnabled", CodeSnippetsEnabled);
                SetValue(Section, "ShowQuickInfoInStatusBar", ShowQuickInfoInStatusBar);
                SetValue(Section, "ShowQuickInfoAsNativeNppTooltip", ShowQuickInfoAsNativeNppTooltip);
                SetValue(Section, "IgnoreDocExceptions", IgnoreDocExceptions);
                SetValue(Section, "SmartIndenting", SmartIndenting);
                SetValue(Section, "DisableMethodInfo", DisableMethodInfo);
                SetValue(Section, "FallbackFormatting", FallbackFormatting);
                SetValue(Section, "RoslynFormatting", RoslynFormatting);
                SetValue(Section, "MemberInfoMaxCharWidth", MemberInfoMaxCharWidth);
                SetValue(Section, "MemberInfoMaxLines", MemberInfoMaxLines);
                SetValue(Section, "FormatAsYouType", FormatAsYouType);
            }
        }

        public void Open()
        {
            lock (this)
            {
                UseTabToAccept = GetValue(Section, "UseTabToAccept", UseTabToAccept);
                UseArrowToAccept = GetValue(Section, "UseArrowToAccept", UseArrowToAccept);
                InterceptCtrlSpace = GetValue(Section, "InterceptCtrlSpace", InterceptCtrlSpace);
                PostFormattingUndoCaretReset = GetValue(Section, "PostFormattingUndoCaretReset", PostFormattingUndoCaretReset);
                UseMethodBrackets = GetValue(Section, "UseMethodBrackets", UseMethodBrackets);
                SmartIndenting = GetValue(Section, "SmartIndenting", SmartIndenting);
                CodeSnippetsEnabled = GetValue(Section, "CodeSnippetsEnabled", CodeSnippetsEnabled);
                FormatAsYouType = GetValue(Section, "FormatAsYouType", FormatAsYouType);
                ShowQuickInfoAsNativeNppTooltip = GetValue(Section, "ShowQuickInfoAsNativeNppTooltip", ShowQuickInfoAsNativeNppTooltip);
                IgnoreDocExceptions = GetValue(Section, "IgnoreDocExceptions", IgnoreDocExceptions);
                MemberInfoMaxCharWidth = GetValue(Section, "MemberInfoMaxCharWidth", MemberInfoMaxCharWidth);
                MemberInfoMaxLines = GetValue(Section, "MemberInfoMaxLines", MemberInfoMaxLines);
                DisableMethodInfo = GetValue(Section, "DisableMethodInfo", DisableMethodInfo);
                FallbackFormatting = GetValue(Section, "FallbackFormatting", FallbackFormatting);
                RoslynFormatting = GetValue(Section, "RoslynFormatting", RoslynFormatting);
            }
        }
    }
}