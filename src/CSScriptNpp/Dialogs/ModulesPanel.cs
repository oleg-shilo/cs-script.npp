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
    public partial class ModulesPanel : Form
    {
        public ModulesPanel()
        {
            InitializeComponent();
        }

        DbgModuleObject[] ToWatchObjects(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new DbgModuleObject[0];

            var root = XElement.Parse(data);

            //"<module index=\"{0}\" name=\"{1}\" location=\"{2}\" />"
            return root.Elements().Select(item =>
                                          {
                                              return new DbgModuleObject
                                              {
                                                  Number = item.Attribute("index").Value,
                                                  Name = item.Attribute("name").Value,
                                                  Location = item.Attribute("location").Value
                                              };

                                          }).ToArray();
        }

        public void Update(string data)
        {
            threadsList.Items.Clear();

            var g = CreateGraphics();
            int maxWidth = 100;
            foreach (var item in ToWatchObjects(data))
            {
                var li = new ListViewItem(item.Number);
                li.SubItems.Add(item.Name);
                li.SubItems.Add(item.Location);
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

            SizeF size = e.Graphics.MeasureString(e.SubItem.Text, threadsList.Font);
            int requiredWidth = Math.Max(30, (int)size.Width + 20);
            if (threadsList.Columns[e.ColumnIndex].Width < requiredWidth)
                threadsList.Columns[e.ColumnIndex].Width = requiredWidth + 5;
        }

        private void stack_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        class DbgModuleObject
        {
            public string Name;
            public string Number;
            public string Location;
        }
    }
}
