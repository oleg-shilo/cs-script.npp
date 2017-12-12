﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    public partial class ConfigForm : Form
    {
        Config data;

        public ConfigForm()
        {
            InitializeComponent();
        }

        public ConfigForm(Config data)
        {
            this.data = data;

            InitializeComponent();

            useArrow.Checked = data.UseArrowToAccept;
            useMethodBrackets.Checked = data.UseMethodBrackets;
            intercept.Checked = data.InterceptCtrlSpace;
            ignoreDocExceptions.Checked = data.IgnoreDocExceptions;
            formatAsYouType.Checked = data.FormatAsYouType;
            formatOnSave.Checked = data.FormatOnSave;
            autoInsertSingle.Checked = data.AutoInsertSingeSuggestion;
            roslynFormatter.Checked = data.RoslynFormatting;
            roslynIntellisense.Checked = data.RoslynIntellisense;
            useContextMenu.Checked = data.UseCmdContextMenu;
            F12OnCtrlClick.Checked = data.GoToDefinitionOnCtrlClick;
            vbSupport.Checked = data.VbSupportEnabled;

            this.FormClosed += (s, e) =>
                {
                    OnClosing();
                };
        }

        public void OnClosing()
        {
            data.UseArrowToAccept = useArrow.Checked;
            data.InterceptCtrlSpace = intercept.Checked;
            data.IgnoreDocExceptions = ignoreDocExceptions.Checked;
            data.FormatAsYouType = formatAsYouType.Checked;
            data.UseMethodBrackets = useMethodBrackets.Checked;
            data.FormatOnSave = formatOnSave.Checked;
            data.AutoInsertSingeSuggestion = autoInsertSingle.Checked;
            data.RoslynFormatting = //roslynFormatter.Checked;
            data.RoslynIntellisense = roslynIntellisense.Checked;
            data.UseCmdContextMenu = useContextMenu.Checked;
            data.GoToDefinitionOnCtrlClick = F12OnCtrlClick.Checked;
            data.VbSupportEnabled = vbSupport.Checked;
        }

        void ConfigForm_Load(object sender, EventArgs e)
        {
            const string tooltip = "Checking this option remap C# Intellisense to Ctrl+Space.\n" +
                                   "This will also force C# Intellisense to invoke\n" +
                                   "native Notepad++ Auto-Completion for non .cs files." +
                                   "\n" +
                                   "Note that it will also remap \"Add missing 'using'\" to Ctrl+.\n" +
                                   "to make it more consistent with the default Visual Studio\n" +
                                   "shortcut mapping";
            this.toolTip1.SetToolTip(this.intercept, tooltip);
        }

        void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string file = Config.Instance.GetFileName();
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        DateTime timestamp = File.GetLastWriteTimeUtc(file);
                        Process.Start("notepad.exe", file)?.WaitForExit();
                        if (File.GetLastWriteTimeUtc(file) != timestamp)
                            Config.Instance.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(@"Error: \n" + ex, @"Notepad++");
                    }
                });

            Close();
        }

        void ConfigForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}