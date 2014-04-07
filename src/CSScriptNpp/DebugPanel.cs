using CSScriptNpp.Dialogs;
using System;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class DebugPanel : Form
    {
        WatchPanel watch;
        LocalsPanel locals;
        ThreadsPanel threads;
        CallStackPanel callstack;

        public DebugPanel()
        {
            InitializeComponent();

            watch = new WatchPanel();
            locals = new LocalsPanel();
            threads = new ThreadsPanel();
            callstack = new CallStackPanel();

            tabControl1.AddTab("Locals", locals);
            tabControl1.AddTab("Call Stack", callstack);
            tabControl1.AddTab("Watch", watch);
            tabControl1.AddTab("Threads", threads);

            if (Debugger.DebugAsConsole)
                appTypeCombo.SelectedIndex = 0;
            else
                appTypeCombo.SelectedIndex = 1;

            Debugger.OnDebuggerStateChanged += UpdateControlsState;

            appTypeCombo.Width = 80;
            RefreshBreakOnException();
        }

        void UpdateControlsState()
        {
            breakBtn.Enabled =
            stopBtn.Enabled = Debugger.IsRunning;

            runToCursorBtn.Enabled =
            stepIntoBtn.Enabled =
            stepOutBtn.Enabled =
            setNextBtn.Enabled = Debugger.IsRunning && Debugger.IsInBreak;
        }

        public void Clear()
        {
            UpdateCallstack("");
            UpdateLocals("");
            UpdateThreads("");
        }

        public void UpdateCallstack(string data)
        {
            callstack.UpdateCallstack(data);
        }

        public void UpdateThreads(string data)
        {
            threads.UpdateThreads(data);
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

        private void tobbleBpBtn_Click(object sender, EventArgs e)
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