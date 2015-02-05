using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class FavoritesPanel : Form
    {
        public Action<string> OnOpenScriptRequest;

        public FavoritesPanel()
        {
            InitializeComponent();
            Reload();
            Save();
        }

        public void Add(string script)
        {
            scriptsList.Items.Add(new ScriptInfo { File = script });
            Save();
            Reload();
        }

        public void Save()
        {
            File.WriteAllLines(favListFile, scriptsList.Items.Cast<ScriptInfo>().Select(x => x.File).ToArray());
        }

        string favListFile = Path.Combine(CSScriptHelper.ScriptsDir, "favlist.txt");

        void Reload()
        {
            var selectedScript = scriptsList.SelectedItem as ScriptInfo;

            scriptsList.Items.Clear();
            if (File.Exists(favListFile))
                scriptsList.Items.AddRange(File.ReadAllLines(favListFile)
                                               .Where(line => !string.IsNullOrEmpty(line))
                                               .Select(line => new ScriptInfo { File = line })
                                               .OrderBy(x => x.ToString())
                                               .Distinct()
                                               .ToArray());

            if (selectedScript != null)
                Select(selectedScript.File);
        }

        void Select(string script)
        {
            foreach (ScriptInfo item in scriptsList.Items)
                if (item.File == script)
                {
                    scriptsList.SelectedItem = item;
                    break;
                }
        }

        void Remove()
        {
            if (scriptsList.SelectedItem != null)
            {
                scriptsList.Items.Remove(scriptsList.SelectedItem);
                Save();
                Reload();
            }
        }

        void Open()
        {
            if (OnOpenScriptRequest != null && scriptsList.SelectedItem != null)
            {
                var script = (scriptsList.SelectedItem as ScriptInfo).File;
                if (File.Exists(script))
                {
                    OnOpenScriptRequest(script);
                }
                else if (DialogResult.Yes == MessageBox.Show("File '" + script + "' cannot be found.\nDo you want to remove it from the Favorites list?", "CS-Script", MessageBoxButtons.YesNo))
                {
                    Remove();
                }
            }
        }

        void scriptsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Open();
        }

        void scriptsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Open();
        }

        void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Remove();
        }

        void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        void refreshLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Reload();
        }

        void scriptsList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            var script = scriptsList.Items[e.Index] as ScriptInfo;

            var brush = Brushes.Black;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                brush = Brushes.White;
            }
            else
            {
                if (!File.Exists(script.File))
                    brush = Brushes.Gray;
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);

            }
            e.Graphics.DrawString(script.ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);

            //e.DrawFocusRectangle();
        }

        private void scriptsList_MouseMove(object sender, MouseEventArgs e)
        {
            int hoverIndex = scriptsList.IndexFromPoint(e.X, e.Y);
            if (hoverIndex >= 0 && hoverIndex < scriptsList.Items.Count)
            {
                var newTooltip = (scriptsList.Items[hoverIndex] as ScriptInfo).File;
                if (toolTip1.GetToolTip(scriptsList) != newTooltip)
                    toolTip1.SetToolTip(scriptsList, newTooltip);
            }
            else
                toolTip1.SetToolTip(scriptsList, null);
        }
    }

    class ScriptInfo
    {
        public string File;

        public override string ToString()
        {
            return Path.GetFileName(File);
        }
    }
}
