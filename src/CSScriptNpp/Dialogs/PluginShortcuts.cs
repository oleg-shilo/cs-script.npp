using CSScriptNpp.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptNpp
{
    public partial class PluginShortcuts : Form
    {
        public PluginShortcuts()
        {
            InitializeComponent();
        }

        private void PluginShortcuts_Load(object sender, EventArgs e)
        {
            foreach (var item in Config.Shortcuts.GetConfigInfo().OrderBy(x => x.DisplayName))
            {
                var li = new ListViewItem(item.DisplayName + "    ");
                li.SubItems.Add(item.Shortcut + "    ");
                li.Tag = item;
                this.listView1.Items.Add(li);
            }

            ResizeListViewColumns(this.listView1); //must be done after adding data

            this.listView1.SelectedItems.Clear();
        }

        void ResizeListViewColumns(ListView lv)
        {
            foreach (ColumnHeader column in lv.Columns)
            {
                column.Width = -1;
            }
        }

        private void PluginShortcuts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string file = Config.Shortcuts.GetFileName();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    DateTime timestamp = File.GetLastWriteTimeUtc(file);
                    Process.Start("notepad.exe", file).WaitForExit();
                    if (File.GetLastWriteTimeUtc(file) != timestamp)
                        MessageBox.Show("The settings changes will take affect only after Notepad++ is restarted.", "Notepad++");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: \n" + ex.ToString(), "CS-Script");
                }
            });

            Close();
        }

        bool modified = false;

        void PluginShortcuts_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (modified)
            {
                Visible = false;
                //Application.DoEvents();
                if (MessageBox.Show("The shortcut mapping changes will take affect only after Notepad++ is restarted.\n\n" +
                                    "Do you want to restart Notepad++ now?\n\n" +
                                    "Note: You may need to remap some of the native Notepad++ shortcuts (Settings->Shortcut Mapper...) " +
                                    "if they conflict with the current CS-Script shortcut configuration.",
                                    "CS-Script", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Utils.RestartNpp();
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Modify();
        }

        private void modifyBtn_Click(object sender, EventArgs e)
        {
            Modify();
        }

        void Modify()
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                var info = (Shortcuts.ConfigInfo)item.Tag;
                using (var dlg = new ShortcutBuilder())
                {
                    dlg.Name = info.DisplayName;
                    dlg.Shortcut = info.Shortcut;
                    if (dlg.ShowModal() == DialogResult.OK)
                    {
                        info.Shortcut = dlg.Shortcut;

                        Config.Shortcuts.SetValue(info.Name, info.Shortcut);
                        Config.Shortcuts.Save();

                        item.SubItems[1].Text = info.Shortcut;

                        modified = true;
                    }
                }
                break;
            }
        }
    }
}