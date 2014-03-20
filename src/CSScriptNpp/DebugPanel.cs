using CSScriptNpp.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace CSScriptNpp
{
    public partial class DebugPanel : Form
    {
        AutoWatchPanel locals;
        CallStackPanel callstack;

        public DebugPanel()
        {
            InitializeComponent();

            locals = new AutoWatchPanel();
            callstack = new CallStackPanel();

            tabControl1.AddTab("Auto Watch", locals);
            tabControl1.AddTab("Call Stack", callstack);

            Debugger.OnDebuggerStateChanged += UpdateControlsState;
        }

        void UpdateControlsState()
        {
            breakBtn.Enabled =
            stopBtn.Enabled = Debugger.IsRunning;

            stepIntoBtn.Enabled =
            stepOutBtn.Enabled =
            setNextBtn.Enabled = Debugger.IsRunning && Debugger.IsInBreak;
        }

        public void Clear()
        {
            UpdateCallstack("");
            UpdateLocals("");
        }

        public void UpdateCallstack(string data)
        {
            callstack.UpdateCallstack(data);
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
    }
}