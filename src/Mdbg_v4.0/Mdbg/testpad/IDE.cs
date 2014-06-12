using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testpad
{
    public partial class IDE : Form
    {
        DebuggerServer debugger = new DebuggerServer();

        public IDE()
        {
            InitializeComponent();
            
            appName.SelectedIndex = 0;
            appArgs.SelectedIndex = 0;
        }

        void WriteLine(string text)
        {
            Invoke((Action)delegate
            {
                output.Text += text + Environment.NewLine;
            });
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ExecuteCurrentCommand();
            }
        }

        void ExecuteCurrentCommand()
        {
            MessageQueue.AddCommand(comboBox1.Text);
            comboBox1.Text = null;
        }

        private void pause_Click(object sender, EventArgs e)
        {
            DebuggerServer.Break();
        }

        private void go_Click(object sender, EventArgs e)
        {
            DebuggerServer.Go();
        }

        private void stepover_Click(object sender, EventArgs e)
        {
            DebuggerServer.StepOver();
        }

        private void stepin_Click(object sender, EventArgs e)
        {
            DebuggerServer.StepIn();
        }

        private void stepout_Click(object sender, EventArgs e)
        {
            DebuggerServer.StepOut();
        }

        private void execute_Click(object sender, EventArgs e)
        {
            ExecuteCurrentCommand();
        }

        private void insertBreakPoint_Click(object sender, EventArgs e)
        {
            DebuggerServer.InsertBreakpoint(source.Text, (int)lineNumber.Value);
        }

        private void start_Click(object sender, EventArgs e)
        {
            if (DebuggerServer.IsRunning)
            {
                DebuggerServer.Exit();
            }
            else
            {
                if (!string.IsNullOrEmpty(appName.Text))
                {
                    DebuggerServer.Start();
                    DebuggerServer.Run(appName.Text, appArgs.Text);
                }
            }

            RefreshDebuggingState();
        }

        void RefreshDebuggingState()
        {
            SetDebuggingState(DebuggerServer.IsRunning);
        }

        void SetDebuggingState(bool debugging)
        {
            Invoke((Action)delegate
                            {
                                start.Text = (debugging ? "Stop" : "Start");
                                toolStrip1.Enabled =
                                insertBreakPoint.Enabled =
                                execute.Enabled = debugging;
                            });
        }

        private void IDE_Load(object sender, EventArgs e)
        {
            DebuggerServer.OnNotificationReceived = WriteLine;
            DebuggerServer.OnDebuggerStateChanged = RefreshDebuggingState;

            SetDebuggingState(false);
        }

        private void test_Click(object sender, EventArgs e)
        {
            MessageQueue.AddCommand("test");
        }

        private void exit_Click(object sender, EventArgs e)
        {
            DebuggerServer.Exit();
        }

        private void attach_Click(object sender, EventArgs e)
        {
            if (DebuggerServer.IsRunning)
            {
                DebuggerServer.Exit();
            }
            else
            {
                if (!string.IsNullOrEmpty(procId.Text))
                {
                    DebuggerServer.Start();
                    DebuggerServer.Attach(procId.Text);
                }
            }

            RefreshDebuggingState();
        }
    }
}
