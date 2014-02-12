using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Res = CSScriptIntellisense.Images;

namespace CSScriptIntellisense
{
    public partial class AutocompleteForm : Form
    {
        Action<ICompletionData> OnAutocompletionAccepted;

        public AutocompleteForm()
        {
            InitializeComponent();
            OnAutocompletionAccepted = x => { };
        }

        bool keyEventsHooked = false;

        //needs pay extra attention to ensure unhooking as it can potentially intercept host app keys
        void ListenToKeyStroks(bool init)
        {
            if (init)
            {
                if (!keyEventsHooked)
                    this.KeyDown += this.AutocompleteForm_KeyDown;
                keyEventsHooked = true;
            }
            else
            {
                if (keyEventsHooked)
                    this.KeyDown -= this.AutocompleteForm_KeyDown;
                keyEventsHooked = false;
            }
        }

        void AutocompleteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ListenToKeyStroks(false);
        }

        string initialPartialName = null;

        public AutocompleteForm(Action<ICompletionData> onAutocompletionAccepted, IEnumerable<ICompletionData> items, string initialSuggestionHint)
        {
            InitializeComponent();

            OnAutocompletionAccepted = onAutocompletionAccepted;

            rawItems = items;

            initialPartialName = initialSuggestionHint;
        }

        int itemHeight;

        public void FilterFor(string partialName)
        {
            listBox1.Items.Clear();

            Debug.WriteLine("hint: " + partialName);

            //IEnumerable<ICompletionData> items = ProcessSuggestionHint("Get", rawItems);
            IEnumerable<ICompletionData> items = ProcessSuggestionHint(partialName, rawItems);

            listBox1.Items.AddRange(items.ToArray());
            listBox1.SelectedItem = items.FirstOrDefault();

            var g = listBox1.CreateGraphics();
            var wideItem = items.Select(x => (int)g.MeasureString(x.DisplayText, listBox1.Font).Width).Max(x => x);
            this.Width = Math.Min(wideItem + 40, 250);//40 = 20 for icon on left and 20 for scrollbar on right

            this.Height = ((itemHeight + verticalSpacing) * Math.Min(listBox1.Items.Count, 10)) + 5;
        }

        public IEnumerable<ICompletionData> ProcessSuggestionHint(string partialName, IEnumerable<ICompletionData> inputItems)
        {
            if (string.IsNullOrWhiteSpace(partialName))
            {
                return inputItems;
            }
            else
            {
                //try partial matches (both the text an the pattern)
                var query = from item in inputItems
                            let matchCount = item.DisplayText.MatchingStartChars(partialName, ignoreCase: true)
                            group item by matchCount into g
                            orderby g.Key descending
                            select new { MatchingDegree = g.Key, Items = g };

                if (query.Count() > 0)
                    return query.First().Items;
                else
                    return inputItems;
            }
        }

        int verticalSpacing = 6;

        void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            var item = (ICompletionData)listBox1.Items[e.Index];

            string itemString = item.DisplayText;

            SizeF size = e.Graphics.MeasureString(item.DisplayText, listBox1.Font);
            e.ItemHeight = (int)size.Height + verticalSpacing;
            e.ItemWidth = (int)size.Width + 16 + 10 + 10; //ensure enough space for the icon and the scrollbar
            listBox1.HorizontalExtent = e.ItemWidth;
        }

        Dictionary<string, Image> icons = new Dictionary<string, Image>();

        Image GetImageFor(ICompletionData item)
        {
            var itemType = EntityType.None;
            var declarationType = DeclarationType.None;
            bool isExtensionMethod = false;

            if (item is IEntityCompletionData)
            {
                itemType = (item as IEntityCompletionData).Entity.EntityType;
            }

            if (item is CompletionData)
            {
                var data = (item as CompletionData);
                declarationType = data.DeclarationType;
                isExtensionMethod = data.IsExtensionMethod;
            }

            switch (itemType)
            {
                case EntityType.Constructor:
                case EntityType.Destructor:
                    return Res.Images.constructor;

                case EntityType.Method:
                    {
                        if (isExtensionMethod)
                            return Res.Images.extension_method;
                        else
                            return Res.Images.method;
                    }
                case EntityType.Event:
                    return Res.Images._event;

                case EntityType.Field:
                    return Res.Images.field;

                case EntityType.Property:
                    return Res.Images.property;

                default:
                    break;
            }

            switch (declarationType)
            {
                case DeclarationType.None:
                    break;

                case DeclarationType.Namespace:
                    return Res.Images._namespace;

                case DeclarationType.Type:
                    return Res.Images.constructor;

                case DeclarationType.Variable:
                case DeclarationType.Parameter:
                    return Res.Images.field;

                case DeclarationType.Event:
                    return Res.Images._event;

                case DeclarationType.Unresolved:
                    return Res.Images.unresolved;

                default:
                    break;
            }

            return null;
        }

        void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            var item = (ICompletionData)listBox1.Items[e.Index];

            e.DrawBackground();

            Brush brush = Brushes.Black;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                brush = Brushes.White;

            Rectangle rect = e.Bounds;
            rect.Offset(16, 3);
            e.Graphics.DrawString(item.DisplayText, e.Font, brush, rect, StringFormat.GenericDefault);

            var image = GetImageFor(item);
            if (image != null)
            {
                Rectangle r = e.Bounds;
                r.Width = 16;
                e.Graphics.FillRectangle(Brushes.White, r);
                e.Graphics.DrawImage(image, e.Bounds.Location);
            }
        }

        IEnumerable<ICompletionData> rawItems;

        private void AutocompleteForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();

            if (e.KeyCode == Keys.Enter ||
               (e.KeyCode == Keys.Right && Config.Instance.UseArrowToAccept) ||
               (e.KeyCode == Keys.Tab && Config.Instance.UseTabToAccept))
            {
                ListenToKeyStroks(false);
                Close();
                OnAutocompletionAccepted(listBox1.SelectedItem as ICompletionData);
            }
        }

        private void AutocompleteForm_Deactivate(object sender, EventArgs e)
        {
            ListenToKeyStroks(false);
            Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ListenToKeyStroks(false);
            Close();
            OnAutocompletionAccepted(listBox1.SelectedItem as ICompletionData);
        }

        private void AutocompleteForm_Load(object sender, EventArgs e)
        {
            var g = listBox1.CreateGraphics();
            itemHeight = (int)g.MeasureString("T", listBox1.Font).Height;

            listBox1.Sorted = false;
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.DrawItem += listBox1_DrawItem;
            listBox1.MeasureItem += listBox1_MeasureItem;
            listBox1.HorizontalScrollbar = true;

            FilterFor(initialPartialName);
            ListenToKeyStroks(true);
        }
    }

    static class Extensions
    {
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
        
        public static int IndexOf<T>(this IEnumerable<T> items, T itemToFind)
        {
            int retVal = 0;
            foreach (var item in items)
            {
                if (item.Equals(itemToFind)) return retVal;
                retVal++;
            }
            return -1;
        }

        public static int MatchingStartChars(this string text, string pattern, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
                return 0;

            if (ignoreCase)
            {
                text = text.ToLower();
                pattern = pattern.ToLower();
            }

            for (int i = 0; i < pattern.Length && i < text.Length; i++)
            {
                if (text[i] != pattern[i])
                    return i;
            }
            return Math.Min(pattern.Length, text.Length);
        }

        //http://www.softcircuits.com/Blog/post/2010/01/10/Implementing-Word-Wrap-in-C.aspx
        public static string WordWrap(this string text, int width)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return text;
            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                int eol = text.IndexOf(Environment.NewLine, pos);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + Environment.NewLine.Length;
                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;
                        if (len > width)
                            len = BreakLine(text, pos, width);
                        sb.Append(text, pos, len);
                        sb.Append(Environment.NewLine);
                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && Char.IsWhiteSpace(text[pos]))
                            pos++;
                    } while (eol > pos);
                }
                else sb.Append(Environment.NewLine); // Empty line
            }
            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        public static int BreakLine(this string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }
    }
}