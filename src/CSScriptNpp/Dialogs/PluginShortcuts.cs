using System;
using System.Linq;
using System.Text;
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
            var menuItems = Plugin.FuncItems.Items.Where(i => i._pShKey.IsSet)
                                              .Select(i => new { Name = i._itemName, Key = i._pShKey.ToString() });

            var internalItems = Plugin.internalShortcuts.Keys
                                                        .Select(key=>new { Name=Plugin.internalShortcuts[key].Item1, Key=key.ToString() });

            foreach (var item in menuItems.Union(internalItems).OrderBy(x=>x.Name))
            {
                    var li = new ListViewItem(item.Name + "    ");
                    li.SubItems.Add(item.Key + "    ");
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
    }
}