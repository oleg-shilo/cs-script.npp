using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
                    MessageBox.Show("Error: \n" + ex.ToString(), "Notepad++");
                }
            });

            Close();
        }
    }
}