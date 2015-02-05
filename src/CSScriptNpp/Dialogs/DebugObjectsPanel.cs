using CSScriptIntellisense;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CSScriptNpp.Dialogs
{
    public partial class DebugObjectsPanel : Form
    {
        private ListViewItem focucedItem;

        public DebugObjectsPanel()
        {
            InitializeComponent();
            InitInPlaceEditor();
        }

        private int selectedSubItem = 0;

        int GetColumnFromOffset(int xOffset)
        {
            int spos = 0;

            for (int i = 0; i < listView1.Columns.Count; i++)
            {
                int epos = spos + listView1.Columns[i].Width;

                if (spos < xOffset && xOffset < epos)
                    return i;

                spos = epos;
            }

            return -1;
        }

        Dictionary<int, int> columnsStartOffset = new Dictionary<int, int>();
        int GetColumnStartOffset(int index)
        {
            if (columnsStartOffset.ContainsKey(index))
                return columnsStartOffset[index];
            return -1;
            //int retval = 0;

            //for (int i = 0; i < index; i++)
            //    retval += listView1.Columns[i].Width;

            //return retval;
        }

        void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            focucedItem = listView1.GetItemAt(e.X, e.Y);

            // Check the subitem clicked .
            int index = GetColumnFromOffset(e.X);
            if (index != -1)
            {
                selectedSubItem = index;
                StartEditing(focucedItem, selectedSubItem);
            }
        }

        void StartEditing(ListViewItem item, int subItem)
        {
            int spos = 0;

            for (int i = 0; i < subItem; i++)
            {
                spos += listView1.Columns[i].Width;
            }

            //currently allow editing name only
            if (subItem != 0) //currently allow editing name only
                return;

            //changing the name of the item allows only fro the root DbgObject
            if (subItem == 0 && (item.Tag as DbgObject).Parent != null)
                return;

            int xOffset = 2;

            editBox.Size = new Size(listView1.Columns[subItem].Width - xOffset, item.Bounds.Bottom - item.Bounds.Top);
            editBox.Location = new Point(spos + xOffset, item.Bounds.Y);
            editBox.IsEditing = true;
            editBox.Show();
            editBox.Text = item.SubItems[subItem].Text;
            editBox.SelectAll();
            editBox.Focus();
        }

        void editBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Return)
                EndEditing();
            else if (e.KeyData == Keys.Escape)
                editBox.Hide();
        }

        public delegate void OnEditCellCompleteHandler(int column, string oldValue, string newValue);

        public event OnEditCellCompleteHandler OnEditCellComplete;

        private void FocusOver(object sender, System.EventArgs e)
        {
            EndEditing();
        }

        void EndEditing()
        {
            if (editBox.IsEditing)
            {
                string oldValue;
                string newValue = editBox.Text;
                editBox.IsEditing = false;
                if (focucedItem.GetDbgObject() != AddNewPlaceholder)
                {
                    oldValue = focucedItem.SubItems[selectedSubItem].Text;
                    focucedItem.SubItems[selectedSubItem].Text = newValue;
                    focucedItem.GetDbgObject().Name = newValue;

                    if (selectedSubItem == 0 && newValue == "")
                    {
                        listView1.Items.Remove(focucedItem);
                        focucedItem = null;
                    }
                }
                else
                {
                    oldValue = null;
                    AddWatchExpression(newValue);
                }
                editBox.Hide();

                if (OnEditCellComplete != null)
                    OnEditCellComplete(selectedSubItem, oldValue, newValue);
            }
        }

        public void ClearWatchExpressions()
        {
            listView1.Items.Clear();
            Debugger.RemoveAllWatch();
            ResetWatchObjects();
        }

        public void StartAddWatch()
        {
            focucedItem = listView1.Items[listView1.Items.Count - 1];
            selectedSubItem = 0;
            StartEditing(focucedItem, selectedSubItem);
        }

        public void AddWatchExpression(string expression)
        {
            expression = expression.Trim();
            if (!string.IsNullOrEmpty(expression))
            {
                AddWatchObject(new DbgObject
                    {
                        DbgId = "",
                        Name = expression,
                        IsExpression = true,
                        Value = "<N/A>",
                        Type = "<N/A>",
                    });

                Debugger.AddWatch(expression);
            }
        }

        public void SetData(string data)
        {
            ResetWatchObjects(ToWatchObjects(data));
            ResizeValueColumn();
        }

        public void UpdateData(string data)
        {
            DbgObject[] freshObjects = ToWatchObjects(data);

            var nonRootItems = new List<ListViewItem>();

            bool updated = false;

            foreach (ListViewItem item in listView1.Items)
            {
                var itemObject = item.Tag as DbgObject;
                if (itemObject != null)
                {
                    itemObject.IsExpanded = false; //if is to be maintained then children need to be required immediately 
                    itemObject.Children = null;
                    itemObject.IsModified = false;
                    if (itemObject.Parent != null)
                    {
                        nonRootItems.Add(item);
                    }
                    else
                    {
                        DbgObject update = freshObjects.Where(x => x.Name == itemObject.Name).FirstOrDefault();
                        if (update != null)
                        {
                            itemObject.CopyDbgDataFrom(update);
                            itemObject.IsModified = (item.SubItems[1].Text != update.Value);
                            item.SubItems[1].Text = update.DispayValue;
                            item.SubItems[2].Text = update.Type;
                            item.ToolTipText = update.Tooltip;
                            updated = true;
                        }
                    }
                }
            }

            nonRootItems.ForEach(x =>
               listView1.Items.Remove(x));

            if (updated)
            {
                listView1.Invalidate();
            }
            ResizeValueColumn();
        }

        void ResizeValueColumn()
        {
            var autoSizeUnlimited = false;

            var g = CreateGraphics();

            var newWidth = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                int visualizerOffset = visualizerIconSize;
                if (item.SubItems[1].Text.StartsWith("\""))
                    visualizerOffset = 0;

                SizeF size = g.MeasureString(item.SubItems[1].Text, listView1.Font);

                int requiredWidth = Math.Max(30, (int)size.Width + visualizerIconSize);
                if (newWidth < requiredWidth)
                {
                    if (autoSizeUnlimited)
                        newWidth = requiredWidth + 5;
                    else
                        newWidth = Math.Min(180, requiredWidth + 5);
                }
            }

            if (listView1.Columns[1].Width < newWidth)
                listView1.Columns[1].Width = newWidth;
        }

        DbgObject[] ToWatchObjects(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new DbgObject[0];

            var root = XElement.Parse(data);

            var values = root.Elements().Select(dbgValue =>
            {
                string valName = dbgValue.Attribute("name").Value;

                if (valName.EndsWith("__BackingField")) //ignore auto-property backing fields
                    return null;

                Func<string, bool> getBoolAttribute = attrName =>
                    {
                        var attr = dbgValue.Attributes(attrName).FirstOrDefault();
                        return attr != null && attr.Value == "true";
                    };

                var dbgObject = new DbgObject();
                dbgObject.DbgId = dbgValue.Attribute("id").Value;
                dbgObject.Name = valName;
                dbgObject.Type = dbgValue.Attribute("typeName").Value.ReplaceClrAliaces();
                dbgObject.IsArray = getBoolAttribute("isArray");
                dbgObject.IsList = getBoolAttribute("isList");
                dbgObject.IsFake = getBoolAttribute("isFake");
                dbgObject.IsPublic = getBoolAttribute("isPublic");
                dbgObject.IsDictionary = getBoolAttribute("isDictionary");
                dbgObject.HasChildren = getBoolAttribute("isComplex");
                dbgObject.IsField = !getBoolAttribute("isProperty");
                dbgObject.IsStatic = getBoolAttribute("isStatic");

                if (!dbgObject.HasChildren)
                {
                    // This is a catch-all for primitives.
                    string stValue = dbgValue.Attribute("value").Value;
                    dbgObject.Value = stValue;
                }
                else
                {
                    XAttribute displayValue = dbgValue.Attribute("rawDisplayValue");
                    if (displayValue != null)
                        dbgObject.Value = displayValue.Value;
                }
                return dbgObject;
            })
            .Where(x => x != null)
            .ToArray();

            var staticMembers = values.Where(x => x.IsStatic);
            var fakeMembers = values.Where(x => x.IsFake);
            var privateMembers = values.Where(x => !x.IsPublic);

            var result = new List<DbgObject>();

            if (values.Count() == 1 && !values[0].HasChildren) //it is a primitive value
            {
                result.Add(values[0]);
            }
            else
            {
                var instanceMembers = values.Where(x => !x.IsStatic && !x.IsFake && x.IsPublic);
                result.AddRange(instanceMembers);

                if (staticMembers.Any())
                    result.Add(
                        new DbgObject
                        {
                            Name = "Static members",
                            HasChildren = true,
                            IsSeparator = true,
                            IsStatic = true,
                            Children = staticMembers.ToArray()
                        });

                if (privateMembers.Any())
                    result.Add(
                        new DbgObject
                        {
                            Name = "Non-Public members",
                            HasChildren = true,
                            IsSeparator = true,
                            Children = privateMembers.ToArray()
                        });

            }

            if (fakeMembers.Any())
            {
                var decoratedResult = new List<DbgObject>(fakeMembers);
                decoratedResult.Add(new DbgObject
                                        {
                                            Name = "Raw View",
                                            HasChildren = true,
                                            IsSeparator = true,
                                            Children = result.ToArray()
                                        });
                return decoratedResult.ToArray();
            }
            else
                return result.ToArray();
        }

        protected void AddWatchObject(DbgObject item)
        {
            int insertionPosition = listView1.Items.Count;

            if (listView1.Items.Count > 0 && listView1.Items[listView1.Items.Count - 1].Tag == AddNewPlaceholder)
                insertionPosition--;
            listView1.Items.Insert(insertionPosition, item.ToListViewItem());
        }

        protected void ResetWatchObjects(params DbgObject[] items)
        {
            listView1.Items.Clear();

            foreach (var item in items.ToListViewItems())
                listView1.Items.Add(item);

            if (!IsReadOnly)
                listView1.Items.Add(AddNewPlaceholder.ToListViewItem());
        }

        DbgObject AddNewPlaceholder = new DbgObject { DbgId = "AddNewPlaceholder", Name = "", IsEditPlaceholder = true };

        void InsertWatchObjects(int index, params DbgObject[] items)
        {
            if (index == listView1.Items.Count)
                listView1.Items.AddRange(items.ToListViewItems().ToArray());
            else
                foreach (var item in items.ToListViewItems().Reverse())
                    listView1.Items.Insert(index, item);
        }

        int triangleMargin = 8;
        int xMargin = 27; //fixed; to accommodate the icon
        int visualizerIconSize = 16;

        Range GetItemExpenderClickableRange(ListViewItem item)
        {
            var dbgObject = (DbgObject)item.Tag;

            int typeColumnStartX = GetColumnStartOffset(0);
            if (typeColumnStartX == -1)
                return new Range { Start = -1, End = -1 };
            else
            {
                int xOffset = 10 * (dbgObject.IndentationLevel); //depends on the indentation
                return new Range { Start = typeColumnStartX + xOffset, End = typeColumnStartX + xOffset + triangleMargin };
            }
        }

        Range GetItemVisualizerClickableRange()
        {
            int typeColumnStartX = GetColumnStartOffset(2);
            if (typeColumnStartX == -1)
                return new Range { Start = -1, End = -1 };
            else
                return new Range { Start = typeColumnStartX - visualizerIconSize, End = typeColumnStartX };
        }

        Range GetItemPinClickableRange()
        {
            int valueColumnStartX = GetColumnStartOffset(1);
            if (valueColumnStartX == -1)
                return new Range { Start = -1, End = -1 };
            else
                return new Range { Start = valueColumnStartX - visualizerIconSize, End = valueColumnStartX };
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();

            var dbgObject = (DbgObject)e.Item.Tag;

            if (dbgObject == AddNewPlaceholder)
                return;

            var textBrush = Brushes.Black;

            if (e.ColumnIndex == 1 && dbgObject.IsModified) //'Value' has changed
                textBrush = Brushes.Red;

            if (!Debugger.IsInBreak)
                textBrush = Brushes.DarkGray;

            columnsStartOffset[e.ColumnIndex] = e.Bounds.X;

            if (e.ColumnIndex == 0)
            {
                var range = GetItemExpenderClickableRange(e.Item);

                int X = e.Bounds.X + range.Start;
                int Y = e.Bounds.Y;

                int textStartX = X + triangleMargin + xMargin;

                SizeF size = e.Graphics.MeasureString(e.Item.Text, listView1.Font);

                int pinOffset = visualizerIconSize + 3;
                if (!dbgObject.HasChildren)
                    pinOffset = 0;

                int requiredWidth = textStartX + (int)size.Width + pinOffset;
                if (e.Bounds.Width < requiredWidth)
                {
                    listView1.Columns[0].Width = requiredWidth + 5;
                    return;
                }

                Image icon;

                if (dbgObject.IsUnresolved)
                    icon = Resources.Resources.unresolved_value;
                else if (dbgObject.IsSeparator)
                {
                    if (dbgObject.IsStatic)
                        icon = Resources.Resources.dbg_container;
                    else
                        icon = Resources.Resources.field;
                }
                else if (dbgObject.IsField)
                    icon = Resources.Resources.field;
                else
                    icon = Resources.Resources.property;

                e.Graphics.DrawImage(icon, range.End + 6, e.Bounds.Y);

                if (dbgObject.HasChildren)
                {
                    int xOffset;
                    int triangleWidth;
                    int triangleHeight;
                    int yOffset;

                    if (dbgObject.IsExpanded)
                    {
                        xOffset = 0;
                        triangleWidth = 7;
                        triangleHeight = 7;
                        yOffset = (e.Bounds.Height - triangleHeight) / 2;

                        e.Graphics.FillPolygon(Brushes.Black, new[]
                        {
                            new Point(X + xOffset + triangleWidth, Y + yOffset),
                            new Point(X + xOffset + triangleWidth, Y + triangleHeight + yOffset),
                            new Point(X + xOffset + triangleWidth - triangleWidth, Y +triangleHeight + yOffset),
                        });
                    }
                    else
                    {
                        xOffset = 2;
                        triangleWidth = 4;
                        triangleHeight = 8;
                        yOffset = (e.Bounds.Height - triangleHeight) / 2;

                        e.Graphics.DrawPolygon(Pens.Black, new[]
                        {
                            new Point(X + xOffset, Y + yOffset),
                            new Point(X + xOffset + triangleWidth, Y + triangleHeight / 2 + yOffset),
                            new Point(X + xOffset,Y + triangleHeight + yOffset),
                        });
                    }
                }

                if (e.Item.Selected)
                {
                    var rect = e.Bounds;
                    rect.Inflate(-1, -1);
                    rect.Offset(textStartX - 5, 0);
                    e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                }

                e.Graphics.DrawString(e.Item.Text, listView1.Font, textBrush, textStartX, Y);

                //if (IsPinnable && !dbgObject.HasChildren && dbgObject.IndentationLevel > 0)
                if (IsPinnable && dbgObject.IsPinable)
                {
                    int x = e.Bounds.X + e.Bounds.Width - Resources.Resources.dbg_pin.Width;
                    int y = e.Bounds.Y;
                    e.Graphics.DrawImage(Resources.Resources.dbg_pin, x, Y);
                }
            }
            else
            {
                string subItemText = e.SubItem.Text;
                if (subItemText.StartsWith("\""))
                    subItemText = subItemText.Replace("\n", "\\n")
                                             .Replace("\r", "\\r")
                                             .Replace("\t", "\\t");

                SizeF size = e.Graphics.MeasureString(subItemText, listView1.Font);

                int visualizerOffset = visualizerIconSize;
                if (e.ColumnIndex != 1 || !dbgObject.IsVisualizable)
                    visualizerOffset = 0;

                int requiredWidth = Math.Max(30, (int)size.Width + 20 + visualizerOffset);

                if (e.ColumnIndex != 1 && listView1.Columns[e.ColumnIndex].Width < requiredWidth)
                {
                    listView1.Columns[e.ColumnIndex].Width = requiredWidth + 5;
                    return;
                }

                if (e.Item.Selected)
                {
                    var rect = e.Bounds;
                    rect.Inflate(-1, -1);
                    e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                }

                if (!dbgObject.IsUnresolved)
                {
                    Rectangle rect = e.Bounds;
                    rect.Width -= visualizerOffset;
                    var format = new StringFormat
                    {
                        FormatFlags = StringFormatFlags.NoWrap,
                        Trimming = StringTrimming.EllipsisWord
                    };
                    e.Graphics.DrawString(subItemText, listView1.Font, textBrush, rect, format);
                }

                if (e.ColumnIndex == 1)
                {
                    if (e.Bounds.Width < requiredWidth)
                        e.Item.ToolTipText = subItemText;
                    else
                        e.Item.ToolTipText = null;

                }

                if (e.ColumnIndex == 1 && dbgObject.IsVisualizable)
                {
                    int x = e.Bounds.X + e.Bounds.Width - visualizerOffset;
                    int y = e.Bounds.Y;
                    e.Graphics.DrawImage(Resources.Resources.dbg_visualise, x, y);
                }
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        public event Action<DbgObject> OnPinClicked;

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (info.Item != null)
            {
                int colIndex = GetColumnFromOffset(e.X);
                if (colIndex == 0)
                {
                    if (GetItemExpenderClickableRange(info.Item).Contains(e.X))
                    {
                        OnExpandItem(info.Item);
                    }

                    var dbgObject = (DbgObject)info.Item.Tag;
                    if (dbgObject != null && dbgObject.IsPinable && IsPinnable)
                    {
                        if (GetItemPinClickableRange().Contains(e.X))
                            if (OnPinClicked != null)
                                OnPinClicked(dbgObject);
                    }
                }
                else if (colIndex == 1)
                {
                    var dbgObject = (DbgObject)info.Item.Tag;
                    if (dbgObject != null && dbgObject.IsVisualizable)
                    {
                        if (GetItemVisualizerClickableRange().Contains(e.X))
                            using (var panel = new TextVisualizer(dbgObject.Name, dbgObject.Value.StripQuotation().Replace("\r", "").Replace("\n", "\r\n")))
                            {
                                if (dbgObject.IsCollection)
                                    panel.InitAsCollection(dbgObject.DbgId);
                                panel.ShowDialog();
                            }
                    }
                }
            }
        }

        void OnExpandItem(ListViewItem item)
        {
            var dbgObject = (item.Tag as DbgObject);
            if (dbgObject.HasChildren)
            {
                if (dbgObject.IsExpanded)
                {
                    var allChildren = listView1.GetAllObjects()
                                               .Where(x => x.IsDescendantOfAny(dbgObject))
                                               .ToArray();

                    foreach (var cObj in allChildren)
                        cObj.IsExpanded = false; //so the items are not expended when/if they are visible again

                    foreach (var c in listView1.LootupListViewItems(allChildren))
                    {
                        listView1.Items.Remove(c);
                    }
                }
                else
                {
                    if (dbgObject.Children == null)
                    {
                        string data = Debugger.Invoke("locals", dbgObject.DbgId);
                        dbgObject.Children = ToWatchObjects(data);
                        dbgObject.HasChildren = dbgObject.Children.Any(); //readjust as complex type (e.g. array) may not have children after the deep inspection
                    }

                    int index = listView1.IndexOfObject(dbgObject);
                    if (index != -1)
                    {
                        InsertWatchObjects(index + 1, dbgObject.Children);
                    }
                }
                dbgObject.IsExpanded = !dbgObject.IsExpanded;

                listView1.Invalidate();
            }
        }

        private ListViewItem GetItemFromPoint(ListView listView, Point mousePosition)
        {
            // translate the mouse position from screen coordinates to
            // client coordinates within the given ListView
            Point localPoint = listView.PointToClient(mousePosition);
            return listView.GetItemAt(localPoint.X, localPoint.Y);
        }

        public bool IsReadOnly = true;
        public bool IsPinnable = false;

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (!IsReadOnly && e.Data.GetDataPresent(DataFormats.StringFormat))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (OnDagDropText != null)
                OnDagDropText((string)e.Data.GetData(DataFormats.StringFormat));
        }

        public event Action<string> OnDagDropText;

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
                DeleteSelected();
        }

        public void DeleteSelected()
        {
            var rootsToRemove = listView1.SelectedItems.Cast<ListViewItem>().Where(x => x.GetDbgObject().Parent == null).ToList();

            DbgObject[] rootObjectsToRemove = rootsToRemove.Select(x => x.GetDbgObject()).ToArray();

            var leafsToRemove = listView1.Items.Cast<ListViewItem>().Where(x => x.GetDbgObject().IsDescendantOfAny(rootObjectsToRemove)).ToList();

            leafsToRemove.ForEach(x => listView1.Items.Remove(x));
            rootsToRemove.ForEach(x =>
                {
                    listView1.Items.Remove(x);

                    string expressionToRemove = x.GetDbgObject().Name;

                    //there can be duplicated expressions left, which is a valid situation and the expressions need to be preserved
                    var identicalExpressions = listView1.Items
                                                        .Cast<ListViewItem>()
                                                        .Where(y => y.GetDbgObject().Name == expressionToRemove);
                    if (!identicalExpressions.Any())
                        Debugger.RemoveWatch(expressionToRemove);
                });
        }

        private void copyValuespMenu_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    var buffer = new StringBuilder();

                    foreach (ListViewItem item in listView1.SelectedItems)
                        buffer.AppendLine(item.GetDbgObject().Value);

                    Clipboard.SetText(buffer.ToString());
                }
                catch { }
            }
        }

        private void copyRowsMenu_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    var buffer = new StringBuilder();

                    foreach (ListViewItem item in listView1.SelectedItems)
                    {
                        var dbgObject = item.GetDbgObject();
                        buffer.AppendLine(string.Format("{0} {1} {2}", dbgObject.Name, dbgObject.Value, dbgObject.Type));
                    }
                    Clipboard.SetText(buffer.ToString());
                }
                catch { }
            }
        }

        ToolTip toolTip = new ToolTip();

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            var cursor = listView1.PointToClient(MousePosition);

            var dbgObject = (DbgObject)e.Item.Tag;

            string toolTipText = e.Item.ToolTipText;
            if (this.IsPinnable && dbgObject.IsPinable && GetItemPinClickableRange().Contains(cursor.X))
                toolTipText = "Click to pin this expression.";
            else if (dbgObject.IsVisualizable && GetItemVisualizerClickableRange().Contains(cursor.X))
                toolTipText = "Click to visualize this value.";

            toolTip.Show(toolTipText, this, cursor.X, cursor.Y - 30, 1000);
        }
    }

    public class Range
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Width { get { return End - Start; } }

        public bool Contains(int point)
        {
            return Start < point && point < End;
        }
    }

    static class Extensions
    {
        //    ListViewItem retva = item.Par
        //}

        public static T[] AllNestedItems<T>(this T item, Func<T, IEnumerable<T>> getChildren)
        {
            int iterator = 0;
            var elementsList = new List<T>();
            var allElements = new List<T>();

            elementsList.Add(item);

            while (iterator < elementsList.Count)
            {
                foreach (T e in getChildren(elementsList[iterator]))
                {
                    elementsList.Add(e);
                    allElements.Add(e);
                }

                iterator++;
            }

            return allElements.ToArray();
        }

        public static IEnumerable<ListViewItem> ToListViewItems(this IEnumerable<DbgObject> items)
        {
            return items.Select(x => x.ToListViewItem());
        }

        public static ListViewItem ToListViewItem(this DbgObject item)
        {
            string name = item.Name;

            var li = new ListViewItem(name);
            li.SubItems.Add(item.DispayValue);
            li.SubItems.Add(item.Type);
            li.Tag = item;
            if (item.IsEditPlaceholder)
                li.ToolTipText = "Double-click to add/edit";
            else
                li.ToolTipText = item.Tooltip;

            return li;
        }

        public static DbgObject GetDbgObject(this ListViewItem item)
        {
            return item.Tag as DbgObject;
        }

        public static IEnumerable<ListViewItem> LootupListViewItems(this ListView listView, IEnumerable<DbgObject> items)
        {
            return listView.Items.Cast<ListViewItem>().Where(x => items.Contains(x.Tag as DbgObject));
        }

        public static IEnumerable<DbgObject> GetAllObjects(this ListView listView)
        {
            return listView.Items.Cast<ListViewItem>().Select(x => x.Tag as DbgObject);
        }

        public static int IndexOfObject(this ListView listView, DbgObject item)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                if (listView.Items[i].Tag == item)
                    return i;
            }
            return -1;
        }

        public static TabControl AddTab(this TabControl control, string tabName, Form content)
        {
            var page = new TabPage
            {
                Padding = new System.Windows.Forms.Padding(3),
                TabIndex = control.TabPages.Count,
                Text = tabName,
                BackColor = Color.White,
                UseVisualStyleBackColor = true
            };

            control.Controls.Add(page);

            content.TopLevel = false;
            content.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            content.Parent = page;
            page.Controls.Add(content);
            content.Dock = DockStyle.Fill;
            content.Visible = true;

            return control;
        }

        public static TabControl SelectTabWith(this TabControl control, Form content)
        {
            var tab = control.Controls
                             .Cast<Control>()
                             .Where(c => c is TabPage)
                             .Cast<TabPage>()
                             .Where(page => page.Controls.Contains(content))
                             .FirstOrDefault();
            if (tab != null)
                control.SelectedTab = tab;
            return control;
        }
    }
}