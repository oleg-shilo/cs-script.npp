using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    public partial class CustomContextMenu : Form
    {
        class ItemInfo
        {
            public string Text;
            public Bitmap Image;
            public Action<string> Handler;

            public bool NewGroup;

            public override string ToString()
            {
                return Text;
            }
        }

        public void Popup()
        {
            Show();
        }

        public void Add(string text, Bitmap image, Action<string> handler)
        {
            items.Add(new ItemInfo { Text = text, Image = image, Handler = handler, NewGroup = nextItemIsSeparator ?? false });
            nextItemIsSeparator = false;
        }

        bool? nextItemIsSeparator;

        public void AddSeparator()
        {
            nextItemIsSeparator = true;
        }

        public CustomContextMenu()
        {
            InitializeComponent();
        }

        List<ItemInfo> items = new List<ItemInfo>();

        int verticalSpacing = 8;
        int itemHeight;
        int separatorHeigt = 1;

        void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            var item = (ItemInfo)listBox1.Items[e.Index];

            SizeF size = e.Graphics.MeasureString(item.ToString(), listBox1.Font);
            e.ItemHeight = (int)size.Height + verticalSpacing;
            e.ItemWidth = (int)size.Width + 16 + 10 + 20; //ensure enough space for the icon and the scrollbar

            if (item.NewGroup)
                e.ItemHeight += separatorHeigt;

            listBox1.HorizontalExtent = e.ItemWidth;
        }

        void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            var item = (ItemInfo)listBox1.Items[e.Index];

            var itemBounds = e.Bounds;

            if (item.NewGroup)
            {
                var separatorBounds = e.Bounds;
                separatorBounds.X += 16 + 2;
                separatorBounds.Width -= 16 + 2;
                separatorBounds.Height = separatorHeigt;
                itemBounds.Y += separatorHeigt;
                itemBounds.Height -= separatorHeigt;

                e.Graphics.FillRectangle(Brushes.LightGray, separatorBounds);
            }

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(253, 244, 191)), itemBounds);
            else
                e.Graphics.FillRectangle(SystemBrushes.Menu, itemBounds);

            Rectangle rect = itemBounds;
            rect.Offset(18, 3);
            e.Graphics.DrawString(item.ToString(), e.Font, SystemBrushes.MenuText, rect, StringFormat.GenericDefault);

            if (item.Image != null)
            {
                Rectangle r = itemBounds;
                r.Width = 16;
                e.Graphics.DrawImage(item.Image, itemBounds.Location);
            }
        }

        void AutocompleteForm_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyCode);
        }

        public void OnKeyDown(Keys key)
        {
            if (Visible)
            {
                if (key == Keys.Escape)
                    Close();

                if (key == Keys.Enter)
                {
                    Close();
                    InvokeHandler();
                }

                if (key == Keys.Up)
                {
                    if (listBox1.SelectedIndex > 0)
                        listBox1.SelectedIndex--;
                }

                if (key == Keys.Down)
                {
                    if (listBox1.SelectedIndex < (listBox1.Items.Count - 1))
                        listBox1.SelectedIndex++;
                }
            }
        }

        void InvokeHandler()
        {
            var info = (listBox1.SelectedItem as ItemInfo);

            if (info != null)
                info.Handler(info.Text);
        }

        void AutocompleteForm_Deactivate(object sender, EventArgs e)
        {
            Close();
        }

        void listBox1_DoubleClick(object sender, EventArgs e)
        {
            Close();
            InvokeHandler();
        }

        //Very important to keep it. It prevents the form from stealing the focus
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        void AutocompleteForm_Load(object sender, EventArgs e)
        {
            var g = listBox1.CreateGraphics();
            itemHeight = (int)g.MeasureString("T", listBox1.Font).Height;

            listBox1.Sorted = false;
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.DrawItem += listBox1_DrawItem;
            listBox1.MeasureItem += listBox1_MeasureItem;
            listBox1.HorizontalScrollbar = true;

            listBox1.Items.AddRange(items.ToArray());
            listBox1.SelectedItem = items.FirstOrDefault();

            var wideItem = items.Select(x => (int)g.MeasureString(x.ToString(), listBox1.Font).Width).Max(x => x);
            this.Width = Math.Min(wideItem + 40 + 80, 350);//40 = 20 for icon on left and 20 for scrollbar on right and 80 for menu-like appearance

            this.Height = ((itemHeight + verticalSpacing) * Math.Min(listBox1.Items.Count, 10)) + 5 + separatorHeigt;
        }
    }
}