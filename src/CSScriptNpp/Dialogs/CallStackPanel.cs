using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSScriptNpp.Dialogs
{
    public partial class CallStackPanel : Form
    {

        public CallStackPanel()
        {
            InitializeComponent();
        }

        public static string CurrentFrameFile;

        public void UpdateCallstack(string data)
        {
            //<call_info>|<source_location>{$NL}

            //+0|debugging2.cs.compiled!Scripting.Script..cctor() Line 13|c:\Users\user\Documents\C# Scripts\debugging2.cs|13:13|13:26{$NL}
            //-1|[External Code]|{$NL}-13|[External Code]|{$NL}

            string lineDelimiter = "{$NL}";
            stack.Items.Clear();
            var items = data.Split(new string[] { lineDelimiter }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line =>
                            {
                                var tokens = line.Split(new[] { '|' }, 3);
                                return new { Id = tokens[0], Call = tokens[1], Source = tokens[2] };
                            })
                            .ToArray();

            var g = CreateGraphics();
            int maxWidth = 100;
            string prevCall = "";
            foreach (var item in items)
            {
                //collapse duplicated not navigatable frames e.g. [External Call]
                if (item.Call.StartsWith("[") &&
                    item.Call.EndsWith("]") &&
                    prevCall == item.Call)
                    continue;

                prevCall = item.Call;
                var li = new ListViewItem("");
                li.SubItems.Add(item.Call);
                li.Tag = item.Id;

                if(item.Id.StartsWith("+"))
                    CurrentFrameFile = item.Source.Split(new[] { '|' }, 2).FirstOrDefault();

                maxWidth = Math.Max(maxWidth, (int)g.MeasureString(item.Call, stack.Font).Width);
                this.stack.Items.Add(li);
            }

            this.stack.Columns[1].Width = maxWidth + 10;
        }

        

        void stack_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Item.Selected)
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            e.DrawText();

            if (e.ColumnIndex == 0)
            {
                if (e.ItemIndex == 0)
                    e.Graphics.DrawImage(Resources.Resources.step, new Point(e.Bounds.X + 2, e.Bounds.Y + 1));
                else if (e.Item.Tag != null && e.Item.Tag.ToString().StartsWith("+"))
                    e.Graphics.DrawImage(Resources.Resources.step_display, new Point(e.Bounds.X + 2, e.Bounds.Y + 2));
            }
        }

        private void stack_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void stack_DoubleClick(object sender, EventArgs e)
        {
            //[+]|[-]N
            string frameIndex = (stack.SelectedItems[0].Tag as string).Substring(1);

            if (string.IsNullOrEmpty(frameIndex))
                return;

            Debugger.GoToFrame(frameIndex);

            //Debugger.OpenStackLocation(locationSpec);
            //currentSourceCallIndex = stack.SelectedIndices[0];
            //stack.Invalidate();
        }
    }
}
