using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class DebugExternal : Form
    {
        public static void ShowModal(int selectedProc = -1)
        {
            using (var dialog = new DebugExternal())
            {
                dialog.ShowDialog();
            }
        }

        public DebugExternal()
        {
            InitializeComponent();

            appPath.Text = Config.Instance.LastExternalProcess;
            arguments.Text = Config.Instance.LastExternalProcessArgs;
            platform.SelectedIndex = Config.Instance.LastExternalProcessCpu;
            managedOnly.Checked = Config.Instance.ListManagedProcessesOnly;
            processList.ListViewItemSorter = new ListViewItemComparer(0, sorting[0]);
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //It is extremely important to ensure that at this point DebugPanel is created.
                //If not it will be created automatically on debugger message handling from the non-GUI thread 
                //of the Debugger message. And in result N++ and one of its docked panes will belong to different 
                //threads and N++ will hang.
                Plugin.GetDebugPanel();
                Plugin.ShowOutputPanel()
                      .ClearAllDefaultOutputs()
                      .ShowDebugOutput();

                Config.Instance.LastExternalProcess = appPath.Text;
                Config.Instance.LastExternalProcessArgs = arguments.Text;
                Config.Instance.LastExternalProcessCpu = platform.SelectedIndex;
                Config.Instance.Save();

                var cpu = (Debugger.CpuType)platform.SelectedIndex;
                string app = appPath.Text;
                string args = arguments.Text;
                ThreadPool.QueueUserWorkItem(x => Debugger.Start(app, args, cpu));

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot start debugger to the external process.\n" + ex.Message, "CS-Script");
            }
        }

        private void attacheBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in processList.SelectedItems)
            {
                Close();

                int procId = (int)item.Tag;
                var cpu = (Debugger.CpuType)Enum.Parse(typeof(Debugger.CpuType), item.SubItems[2].Text);
                AttachTo(procId, cpu);

                return; //do only the first selection
            }
        }

        static internal void AttachTo(int procId)
        {
            var cpu = IsWin64(procId) ? Debugger.CpuType.x64 : Debugger.CpuType.x86;
            AttachTo(procId, cpu);
        }

        static internal void AttachTo(int procId, Debugger.CpuType cpu)
        {
            try
            {
                //It is extremely important to ensure that at this point DebugPanel is created.
                //If not it will be created automatically on debugger message handling from the non-GUI thread 
                //of the Debugger message. And in result N++ and one of its docked panes will belong to different 
                //threads and N++ will hang.
                Plugin.GetDebugPanel();
                Plugin.ShowOutputPanel()
                      .ClearAllDefaultOutputs()
                      .ShowDebugOutput();

                ThreadPool.QueueUserWorkItem(x => Debugger.Attach(procId, cpu));
            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot attach debugger to the external process.\n" + e.Message, "CS-Script");
            }
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            Reload();
        }

        List<string> procList = new List<string>();

        private void Reload()
        {
            Cursor = Cursors.WaitCursor;
            processList.Enabled =
            refreshBtn.Enabled = false;

            this.BeginInvoke((Action)delegate
            {
                Repopulete();

                processList.Enabled =
                refreshBtn.Enabled = true;
                Cursor = Cursors.Default;
                processList.Focus();
            });
        }

        void Repopulete(bool refetch = true)
        {
            processList.Items.Clear();
            var sorter = processList.ListViewItemSorter;
            processList.ListViewItemSorter = null;

            Action<string> processData = info =>
                {
                    //<name>:<id>:<cpu>:<runtime>:<title>
                    string[] values = info.Split(new[] { ':' }, 5);

                    bool isManaged = values[3].Contains("Managed");
                    if (!managedOnly.Checked || isManaged)
                    {
                        try
                        {
                            var li = new ListViewItem(values[0]);//name
                            li.SubItems.Add(values[1]); //id
                            li.SubItems.Add(values[2]); //cpu   
                            li.SubItems.Add(values[3]); //runtime
                            li.SubItems.Add(values[4]); //title
                            li.Tag = int.Parse(values[1]);
                            processList.Items.Add(li);
                            Application.DoEvents();
                        }
                        catch { }
                    }
                };

            if (refetch)
            {
                procList.Clear();
                foreach (string info in GetProcessList())
                {
                    processData(info);
                    procList.Add(info);
                }
            }
            else
            {
                foreach (string info in procList)
                    processData(info);
            }

            processList.ListViewItemSorter = sorter;

            foreach (ColumnHeader column in processList.Columns)
                column.Width = -2;
        }

        public static bool IsWin64(int id)
        {
            //it will be no more than a single item. It is either x86 or x64
            var info = GetProcessList(id).FirstOrDefault();
            if (info != null)
            {
                //<name>:<id>:<cpu>:<runtime>:<title>
                string[] values = info.Split(new[] { ':' }, 5);
                return values[2] == "x64";
            }
            return false;
        }

        public static IEnumerable<string> GetProcessList(int procId = -1)
        {
            string host32 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"MDbg\mdbghost_32.exe");
            string host64 = host32.Remove(host32.Length - 6) + "64.exe";
            string file = Path.GetTempFileName();

            var p = new Process();
            p.StartInfo.FileName = host32;
            p.StartInfo.Arguments = (procId == -1 ? "/lp" : "/lp:" + procId);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            p.Start();

            //var output = new List<string>();
            string line = null;
            while (null != (line = p.StandardOutput.ReadLine()))
            {
                yield return line;
            }
            p.WaitForExit();

            p.StartInfo.FileName = host64;
            p.Start();
            while (null != (line = p.StandardOutput.ReadLine()))
            {
                yield return line;
            }
        }

        private void ReloadLocal()
        {
            //limitations: IsManaged cannot be analyzed if running process is of the different CPY type
        }

        private void processList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            sorting[e.Column] = !sorting[e.Column];
            processList.ListViewItemSorter = new ListViewItemComparer(e.Column, sorting[e.Column]);
        }

        Dictionary<int, bool> sorting = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false } };

        private class ListViewItemComparer : IComparer
        {
            int col;
            bool reverse;

            public ListViewItemComparer()
            {
                col = 0;
            }

            public ListViewItemComparer(int column, bool reverse = false)
            {
                this.reverse = reverse;
                this.col = column;
            }

            public int Compare(object x, object y)
            {
                if (reverse)
                    return String.Compare(((ListViewItem)y).SubItems[col].Text, ((ListViewItem)x).SubItems[col].Text);
                else
                    return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }
        }

        private void managedOnly_CheckedChanged(object sender, EventArgs e)
        {
            Config.Instance.ListManagedProcessesOnly = managedOnly.Checked;
            Config.Instance.Save();
            Repopulete(false);
        }

        private void DebugExternal_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true; //to avoid repainting problems run in the UI thread but after some delay
        }

        private void processList_DoubleClick(object sender, EventArgs e)
        {
            attacheBtn.PerformClick();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Reload();
        }

        private void processList_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                attacheBtn.PerformClick();
        }
    }
}