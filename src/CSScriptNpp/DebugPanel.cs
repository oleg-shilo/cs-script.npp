using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSScriptIntellisense;
using UltraSharp.Cecil;
using System.Drawing;

namespace CSScriptNpp
{
    public partial class DebugPanel : Form
    {
        public DebugPanel()
        {
            InitializeComponent();
            this.stack.Columns[1].Width = 100;
        }

        public void UpdateCallstack(string data)
        {
            currentSourceCallIndex = 0;
            //<call_info>|<source_location>{$NL}
            string lineDelimiter = "{$NL}";
            stack.Items.Clear();
            var items = data.Split(new string[] { lineDelimiter }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line =>
                             {
                                 var tokens = line.Split(new[] { '|' }, 2);
                                 return new { Call = tokens.First(), Source = tokens.Last() };
                             })
                            .ToArray();

            var g = CreateGraphics();
            int maxWidth = 100;
            foreach (var item in items)
            {
                var li = new ListViewItem("");
                li.SubItems.Add(item.Call);
                li.Tag = item.Source;

                maxWidth = Math.Max(maxWidth, (int)g.MeasureString(item.Call, stack.Font).Width);
                this.stack.Items.Add(li);
            }

            this.stack.Columns[1].Width = maxWidth+10;
        }

        int currentSourceCallIndex = 0;
        private void stack_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Item.Selected)
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            e.DrawText();

            if (e.ColumnIndex == 0)
            {
                if (e.ItemIndex == 0)
                    e.Graphics.DrawImage(Resources.Resources.step, new Point(e.Bounds.X + 2, e.Bounds.Y + 1));
                else if (e.ItemIndex == currentSourceCallIndex)
                    e.Graphics.DrawImage(Resources.Resources.step_display, new Point(e.Bounds.X + 2, e.Bounds.Y + 2));

            }
        }

        private void stack_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void stack_DoubleClick(object sender, EventArgs e)
        {
            string locationSpec = stack.SelectedItems[0].Tag as string;
            
            if (string.IsNullOrEmpty(locationSpec))
                return;

            Debugger.OpenStackLocation(locationSpec);
            currentSourceCallIndex = stack.SelectedIndices[0];
            stack.Invalidate();
        }
    }
}
