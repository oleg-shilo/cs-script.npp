using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class BreakpointsPanel : Form
    {
        public BreakpointsPanel()
        {
            InitializeComponent();
            RefreshItems();
            Debugger.OnBreakpointChanged = RefreshItems;
        }

        public void RefreshItems()
        {
            stack.Items.Clear();

            var g = CreateGraphics();
            int maxWidth = 100;
            int index = 1;
            var items = Debugger.GetActiveBreakpoints()
                                .Select(x => new { Data = x, Fields = x.Split('|') })
                                .OrderByDescending(x => x.Fields[0])
                                .ThenByDescending(x => int.Parse(x.Fields[1]))
                                .Select(x => x.Data)
                                .Reverse();

            foreach (var item in items)
            {
                string[] parts = item.Split('|');

                string file = parts[0];
                string line = parts[1];

                var li = new ListViewItem(index.ToString());
                li.SubItems.Add(Path.GetFileName(file));
                li.SubItems.Add(line);
                li.Tag = item;
                li.ToolTipText = string.Format("{0} ({1})", file, line);

                maxWidth = Math.Max(maxWidth, (int)g.MeasureString(li.SubItems[1].Text, stack.Font).Width);
                this.stack.Items.Add(li);
                index++;
            }

            this.stack.Columns[1].Width = maxWidth + 10;
        }

        ToolTip toolTip = new ToolTip();

        private void stack_DoubleClick(object sender, EventArgs e)
        {
            //Breakpoint syntax: <file>|<line>
            string[] parts = (stack.SelectedItems[0].Tag as string).Split('|');

            //Source location syntax: <file>|<start_line>:<start_column>|<end_line>:<end_column>
            Debugger.NavigateToFileLocation(string.Format("{0}|{1}:1|{1}:1", parts[0], parts[1]), showStep: false);
            //do it again; temp trick to ensure proper selection, which otherwise can be missed if the file is just was loaded
            Debugger.NavigateToFileLocation(string.Format("{0}|{1}:1|{1}:1", parts[0], parts[1]), showStep: false); 
        }

        private void stack_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            var cursor = this.PointToClient(MousePosition);
            cursor.Offset(15, 15);
            toolTip.Show(e.Item.ToolTipText, this, cursor.X, cursor.Y, 1700);
        }

        private void removeAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Debugger.RemoveAllBreakpoints();
        }
    }
}
