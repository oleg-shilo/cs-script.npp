using CSScriptNpp.Dialogs;
using System;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class DebugPanel : Form
    {
        CallStackPanel callstack;
        LocalsPanel locals;
        WatchPanel watch;
        ThreadsPanel threads;
        ModulesPanel modules;
        BreakpointsPanel breakpoints;

        public DebugPanel()
        {
            InitializeComponent();

            UpdateButtonsTooltips();

            locals = new LocalsPanel();
            callstack = new CallStackPanel();
            watch = new WatchPanel();
            threads = new ThreadsPanel();
            modules = new ModulesPanel();
            breakpoints = new BreakpointsPanel();

            tabControl1.AddTab("Locals", locals);
            tabControl1.AddTab("Call Stack", callstack);
            tabControl1.AddTab("Watch", watch);
            tabControl1.AddTab("Threads", threads);
            tabControl1.AddTab("Modules", modules);
            tabControl1.AddTab("Breakpoints", breakpoints);

            try
            {
                tabControl1.SelectedIndex = Config.Instance.DebugPanelInitialTab;
            }
            catch { }

            this.VisibleChanged += (s, e) =>
                                {
                                    if (!Visible)
                                    {
                                        Config.Instance.DebugPanelInitialTab = tabControl1.SelectedIndex;
                                        Config.Instance.Save();
                                    }
                                };

            if (Debugger.DebugAsConsole)
                appTypeCombo.SelectedIndex = 0;
            else
                appTypeCombo.SelectedIndex = 1;

            Debugger.OnDebuggerStateChanged += UpdateControlsState;

            appTypeCombo.Width = 80;
            RefreshBreakOnException();

            UpdateControlsState();
        }

        void UpdateButtonsTooltips()
        {
            goBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("_Debug", "F5"));
            stopBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("Stop", "Shift+F5"));
            stepOverBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("StepOver", "F10"));
            stepOutBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("StepOut", "Shift+F11"));
            stepIntoBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("StepInto", "F11"));
            setNextBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("SetNextIP", "Ctrl+Shift+F10"));
            runToCursorBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("RunToCursor", "Ctrl+F10"));
            toggleBpBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("ToggleBreakpoint", "F9"));
            quickWatchBtn.EmbeddShortcutIntoTooltip(Config.Shortcuts.GetValue("QuickWatch", "Shift+F9"));
        }

        void UpdateControlsState()
        {
            this.InUiThread(() =>
            {
                breakBtn.Enabled = Debugger.IsRunning && !Debugger.IsInBreak;
                stopBtn.Enabled = Debugger.IsRunning;
                goBtn.Enabled = !Debugger.IsRunning || (Debugger.IsRunning && Debugger.IsInBreak);

                runToCursorBtn.Enabled =
                stepIntoBtn.Enabled =
                stepOutBtn.Enabled =
                setNextBtn.Enabled = Debugger.IsRunning && Debugger.IsInBreak;
            });
        }

        public void Clear()
        {
            UpdateCallstack("");
            UpdateLocals("");
            UpdateThreads("");
            UpdateModules("");
            watch.Refresh();
        }

        public void UpdateCallstack(string data)
        {
            callstack.UpdateCallstack(data);
        }

        public void UpdateThreads(string data)
        {
            threads.UpdateThreads(data);
        }

        public void UpdateModules(string data)
        {
            modules.Update(data);
        }

        public void UpdateLocals(string data)
        {
            Invoke((Action)delegate
            {
                locals.SetData(data);
            });
        }

        private void goBtn_Click(object sender, EventArgs e)
        {
            if (Debugger.IsRunning)
            {
                Debugger.Go();
            }
            else
            {
                Plugin.DebugScript();//this will also load the script
            }
        }

        private void stepOverBtn_Click(object sender, EventArgs e)
        {
            Plugin.StepOver();
        }

        private void breakBtn_Click(object sender, EventArgs e)
        {
            Debugger.Break();
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            Debugger.Stop();
        }

        private void stepIntoBtn_Click(object sender, EventArgs e)
        {
            Debugger.StepIn();
        }

        private void stepOutBtn_Click(object sender, EventArgs e)
        {
            Debugger.StepOut();
        }

        private void setNextBtn_Click(object sender, EventArgs e)
        {
            Debugger.SetInstructionPointer();
        }

        private void toggleBpBtn_Click(object sender, EventArgs e)
        {
            Debugger.ToggleBreakpoint();
        }

        private void appTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debugger.DebugAsConsole = (appTypeCombo.SelectedIndex == 0);
        }

        private void runToCursorBtn_Click(object sender, EventArgs e)
        {
            Debugger.RunToCursor();
        }

        private void quickWatch_Click(object sender, EventArgs e)
        {
            QuickWatchPanel.PopupDialog();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Debugger.BreakOnException = !Debugger.BreakOnException;
            RefreshBreakOnException();
        }

        void RefreshBreakOnException()
        {
            if (Debugger.BreakOnException)
            {
                this.breakOnExceptionBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_remove_stoponexc;
                this.breakOnExceptionBtn.ToolTipText = "Disable 'Break On Exception'";
            }
            else
            {
                this.breakOnExceptionBtn.Image = global::CSScriptNpp.Resources.Resources.dbg_set_stoponexc;
                this.breakOnExceptionBtn.ToolTipText = "Enable 'Break On Exception'";
            }
        }
    }
}