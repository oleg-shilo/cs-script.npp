using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CSScriptNpp.Dialogs
{
    public partial class ThreadsPanel : Form
    {
        public ThreadsPanel()
        {
            InitializeComponent();
        }

        DbgThreadObject[] ToWatchObjects(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new DbgThreadObject[0];

            var root = XElement.Parse(data);

            //"<thread id=\"{0}\" name=\"{1}\" active=\"{2}\" number=\"{3}\" />"
            return root.Elements().Select(item =>
                                          {
                                              return new DbgThreadObject
                                              {
                                                  Id = item.Attribute("id").Value,
                                                  Name = item.Attribute("name").Value,
                                                  Number = item.Attribute("number").Value,
                                                  IsActive = string.Compare(item.Attribute("active").Value, "true", true) == 0
                                              };

                                          }).ToArray();
        }

        public void UpdateThreads(string data)
        {
            threadsList.Items.Clear();

            var g = CreateGraphics();
            int maxWidth = 100;
            foreach (var item in ToWatchObjects(data))
            {
                var li = new ListViewItem("");
                li.SubItems.Add(item.Number);
                li.SubItems.Add(item.Id);
                li.SubItems.Add(item.Name);
                li.Tag = item;

                maxWidth = Math.Max(maxWidth, (int)g.MeasureString(item.Name, threadsList.Font).Width + 10);
                this.threadsList.Items.Add(li);
            }

            this.threadsList.Columns[3].Width = maxWidth + 10;
        }

        private void stack_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Item.Selected)
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            e.DrawText();

            if (e.ColumnIndex == 0)
            {
                if (e.Item.Tag != null)
                {
                    var obj = (DbgThreadObject)e.Item.Tag;
                    if (obj.IsActive)
                        e.Graphics.DrawImage(Resources.Resources.step, new Point(e.Bounds.X + 2, e.Bounds.Y + 2));
                }
            }
            else
            {
                SizeF size = e.Graphics.MeasureString(e.SubItem.Text, threadsList.Font);
                int requiredWidth = Math.Max(30, (int)size.Width + 20);
                if (threadsList.Columns[e.ColumnIndex].Width < requiredWidth)
                    threadsList.Columns[e.ColumnIndex].Width = requiredWidth + 5;
            }
        }

        private void stack_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void stack_DoubleClick(object sender, EventArgs e)
        {
            if (threadsList.SelectedItems.Count > 0)
            {
                var obj = (DbgThreadObject)threadsList.SelectedItems[0].Tag;
                Debugger.GoToThread(obj.Id);
            }
        }

        class DbgThreadObject
        {
            public string Id;
            public string Name;
            public string Number;
            public bool IsActive;
        }
    }
}
