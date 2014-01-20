using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    public partial class MemberInfoPanel : Form, IPopupForm
    {
        public struct ItemInfo
        {
            public string Text;
            public int? ArgumentCount;

            public override string ToString()
            {
                return Text;
            }
        }

        public Point LeftBottomCorner;

        int index = 0;

        public bool IsVisible()
        {
            return Visible;
        }

        int lastHintCount = -1;
        public void ProcessMethodOverloadHint(IEnumerable<string> hint)
        {
            int hitCount = (hint == null ? 0 : hint.Count());

            if (lastHintCount == hitCount)
                return; //nothing changed so no need to do anything

            lastHintCount = hitCount;

            string initaialSelection = (index == 0 ? null : items[index].Text);

            items.Clear();

            if (hitCount == 0)
                items.AddRange(rawItems);
            else
                items.AddRange(rawItems.Where(s => !s.ArgumentCount.HasValue || s.ArgumentCount.Value >= hitCount));

            index = 0;
            if (initaialSelection != null)
                for (int i = 0; i < items.Count; i++)
                    if (items[i].Text == initaialSelection)
                    {
                        index = i;
                        break;
                    }

            ResetActiveText();
        }

        public void AddData(IEnumerable<string> methodSignatures)
        {
            foreach (string item in methodSignatures)
            {
                ItemInfo info = new ItemInfo { Text = ReformatSignatureInfo(item) };
                if (!Simple)
                    info.ArgumentCount = NRefactoryExtensions.GetArgumentCount(info.Text);
                items.Add(info);
            }

            rawItems.AddRange(items);

            index = 0;
            ResetActiveText();
        }

        string ReformatSignatureInfo(string signarure)
        {
            string[] lines = signarure.GetLines();
            if (lines.Length > 1)
            {
                int maxLength = Math.Max(lines[0].Length, 100);

                int longestDocumentationLine = lines.Max(x => x.Length);

                if (longestDocumentationLine > maxLength)
                {
                    signarure = signarure.WordWrap(maxLength);
                }
            }
            return signarure;
        }

        string RemoveTypeCategory(string memberInfo)
        {
            return memberInfo.Split(new[] { ':' }, 2).Last();
        }

        void ResetActiveText()
        {
            SizeF size = MeasureDisplayArea();
            this.Width = (int)size.Width;
            this.Height = (int)size.Height;
            this.Left = LeftBottomCorner.X;
            this.Top = LeftBottomCorner.Y - (int)size.Height - 10;

            Invalidate();
        }

        public bool Simple = false;

        public bool AutoClose { get { return Simple; } }

        public MemberInfoPanel()
        {
            InitializeComponent();
            ResetIdleTimer();
        }

        public List<ItemInfo> items = new List<ItemInfo>();
        List<ItemInfo> rawItems = new List<ItemInfo>();

        public void kbdHook_KeyDown(Keys key, int repeatCount)
        {
            OnKeyDown(key);
        }

        void MemberInfoPanel_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyCode);
        }

        void ResetIdleTimer()
        {
            timer1.Enabled = false;
            timer1.Enabled = true;
        }

        void OnKeyDown(Keys key)
        {
            ResetIdleTimer();

            if (Simple)
            {
                TryClose();
            }
            else
            {
                if (key == Keys.Escape || key == Keys.Enter)
                    TryClose();

                if (key == Keys.Up)
                    Decrement();

                if (key == Keys.Down && !Simple)
                    Increment();
            }
        }

        void QuickInfoPanel_Deactivate(object sender, EventArgs e)
        {
            //if "!Simple" then focus stays on Scintilla
            if (Simple)
            {
                TryClose();
            }
        }

        void Increment()
        {
            index++;
            if (index >= items.Count)
                index = 0;
            ResetActiveText();
        }

        void Decrement()
        {
            index--;
            if (index < 0)
                index = items.Count - 1;
            ResetActiveText();
        }

        Brush bgdBrush = new SolidBrush(Color.FromArgb(0xE7E8EC));
        Pen borderPen = Pens.DarkGray;
        static float controlButtonSize = 8;
        static int docSeparatorHeight = 7;
        float controlButtonsPadding = 3;

        SizeF? resolution;
        SizeF MeasureDisplayArea()
        {
            string info = items[index].Text;

            if (!Simple)
                info = RemoveTypeCategory(info);

            Graphics g = this.CreateGraphics();
            SizeF size = g.MeasureString(info, this.Font);

            if (!Simple)
            {
                SizeF statsSize = g.MeasureString(GetOverloadingStats(), this.Font);
                size.Width = size.Width + statsSize.Width + controlButtonSize * 2 + controlButtonsPadding * 2 + 5;
            }

            if (!resolution.HasValue)
                resolution = g.MeasureString("Method", this.Font);

            int extraHight = 0;
            if (info.GetLines(2).Length > 1) //has API documentation
            {
                extraHight = docSeparatorHeight;
            }

            return new SizeF(size.Width + 10, size.Height + 20 + extraHight);
        }

        RectangleF upButtonArea = new RectangleF(0, 0, controlButtonSize, controlButtonSize);
        RectangleF downButtonArea = new RectangleF(0, 0, controlButtonSize, controlButtonSize);

        Font docFont;
        Font DocFont
        {
            get
            {
                if (docFont == null)
                    docFont = new Font(this.Font, FontStyle.Italic);
                return docFont;
            }
        }

        void QuickInfoPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            var area = this.ClientRectangle;

            float yOffset = 10;
            float xOffset = 5;

            e.Graphics.FillRectangle(bgdBrush, area);
            e.Graphics.DrawRectangle(borderPen, area);

            string infoText = items[index].Text;
            if (Simple)
            {
                infoText = items[index].Text;
            }
            else
            {
                upButtonArea.X = xOffset;
                upButtonArea.Y = yOffset + 3f;
                e.Graphics.FillTranagle(Brushes.Black, xOffset, yOffset + 3f, controlButtonSize, controlButtonSize, true);

                xOffset += controlButtonSize + controlButtonsPadding;

                string stats = GetOverloadingStats();
                SizeF size = e.Graphics.MeasureString(stats, this.Font);
                e.Graphics.DrawString(stats, this.Font, Brushes.Black, xOffset, yOffset);
                xOffset += size.Width + controlButtonsPadding;

                downButtonArea.X = xOffset;
                downButtonArea.Y = yOffset + 3f;
                e.Graphics.FillTranagle(Brushes.Black, xOffset, yOffset + 3f, controlButtonSize, controlButtonSize, false);
                xOffset += controlButtonSize + controlButtonsPadding;

                infoText = RemoveTypeCategory(items[index].Text);
            }

            //e.Graphics.DrawString(RemoveTypeCategory(items[index].Text), this.Font, Brushes.Black, xOffset, yOffset);

            string[] parts = infoText.GetLines(2);

            e.Graphics.DrawString(parts.First(), this.Font, Brushes.Black, xOffset, yOffset);
            if (parts.Length > 1)
                e.Graphics.DrawString(parts.Last(), this.DocFont, Brushes.Black, xOffset, yOffset + resolution.Value.Height + docSeparatorHeight);
        }

        string GetOverloadingStats()
        {
            return string.Format("{0} of {1}", index + 1, items.Count);
        }

        private void MemberInfoPanel_MouseDown(object sender, MouseEventArgs e)
        {
            ResetIdleTimer();

            if (!Simple)
            {
                if (downButtonArea.Contains(e.Location))
                    Increment();
                else if (upButtonArea.Contains(e.Location))
                    Decrement();
            }

            if (!this.ClientRectangle.Contains(e.Location))
                TryClose();
        }

        private void MemberInfoPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                Capture = false;
                TryClose();
            }
        }

        private void OnIdelTimer_Tick(object sender, EventArgs e)
        {
            TryClose();
        }

        void TryClose()
        {
            try
            {
                Close();
            }
            catch { } //form can be already disposed
        }

        private void MemberInfoPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Simple)
                ResetIdleTimer(); //prevent closing if the mouse is over the form in !Simple mode
        }

        private void MemberInfoPanel_Load(object sender, EventArgs e)
        {
            Capture = true;
        }
    }

    static class GraphicsExtensions
    {
        public static void FillTranagle(this Graphics g, Brush brush, float x, float y, float width, float height, bool pointUp)
        {
            var gp = new GraphicsPath(FillMode.Winding);

            if (pointUp)
                gp.AddLines(new PointF[] { new PointF(x, y + height), new PointF(x + width, y + height), new PointF(x + width / 2, y) });
            else
                gp.AddLines(new PointF[] { new PointF(x, y), new PointF(x + width, y), new PointF(x + width / 2, y + height) });

            g.FillPath(brush, gp);
        }
    }

    public interface IPopupForm
    {
        bool AutoClose { get; }
    }
}