using CSScriptIntellisense;
using Intellisense.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UltraSharp.Cecil;

namespace CSScriptNpp
{
    public partial class CodeMapPanel : Form
    {
        class MemberInfo
        {
            public int Line = -1;
            public string Content = "";
            public string ContentType = "";
            public string ContentIndent = "";

            public override string ToString()
            {
                return Content;
            }
        }

        static public CodeMapPanel Instance;

        public CodeMapPanel()
        {
            InitializeComponent();
            Instance = this;
            watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += watcher_Changed;
            watcher.EnableRaisingEvents = false;
            ErrorMessage = null;
            membersList.AttachMouseControlledZooming(MembersList_OnZoom);
            UpdateItemHeight();
        }

        private void MembersList_OnZoom(Control sender, bool zoomIn)
        {
            var fontSizeDelta = zoomIn ? 2 : -2;
            sender.ChangeFontSize(fontSizeDelta);
            UpdateItemHeight();
        }

        void UpdateItemHeight()
        {
            var textHeight = (int)membersList.CreateGraphics().MeasureString("H", membersList.Font).Height;
            membersList.ItemHeight = textHeight + 2;
        }

        void MembersList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (membersList.SelectedItem != null)
            {
                var info = (membersList.SelectedItem as MemberInfo);
                try
                {
                    membersList.SelectedItem = null;
                    if (info.Line != -1)
                    {
                        Npp.GrabFocus();
                        int currentLineNum = Npp.GetCaretLineNumber();
                        int prevLineEnd = Npp.GetLineStart(currentLineNum) - Environment.NewLine.Length;
                        int topScrollOffset = currentLineNum - Npp.GetFirstVisibleLine();

                        Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, info.Line, 0);
                        Npp.SetFirstVisibleLine(info.Line - topScrollOffset);
                    }
                }
                catch { } //it is expected to fail if the line does not contain the file content position spec. This is also the reason for not validating any "IndexOf" results.
            }
        }

        string currentFile;
        Dictionary<int, int> currentMapping = new Dictionary<int, int>();

        public void RefreshContent()
        {
            string file = Npp.GetCurrentFile();
            if (file.IsScriptFile() || file.IsPythonFile())
            {
                membersList.Visible = true;
                if (file != currentFile)
                {
                    currentFile = file;

                    watcher.Path = Path.GetDirectoryName(currentFile);
                    watcher.Filter = Path.GetFileName(currentFile);
                }

                if (file.IsScriptFile())
                    GenerateContent(Npp.GetTextBetween(0));
                else if (file.IsPythonFile())
                    GenerateContentPython(Npp.GetTextBetween(0));
            }
            else
            {
                membersList.Visible = false;
            }
        }

        StringFormat format = new StringFormat
        {
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };

        void memberList_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index != -1)
                {
                    e.DrawBackground();
                    var info = membersList.Items[e.Index] as MemberInfo;

                    var brush = Brushes.Black;

                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        brush = Brushes.White;
                    else
                        e.Graphics.FillRectangle(Brushes.White, e.Bounds);

                    var bounds = e.Bounds;
                    var normalFont = e.Font;
                    var italicFont = new Font(e.Font, FontStyle.Italic);

                    bounds.Offset(info.ContentIndent.Length * 3, 0);

                    var font = italicFont;
                    e.Graphics.DrawString(info.ContentType, font, Brushes.Blue, bounds, StringFormat.GenericDefault);
                    var size = e.Graphics.MeasureString(info.ContentType, font);
                    bounds.Offset((int)size.Width, 0);

                    font = normalFont;
                    e.Graphics.DrawString(info.Content, font, brush, bounds, format);

                    //e.DrawFocusRectangle();
                }
            }
            catch
            {
#if DEBUG
                throw;
#endif
                // Ignore all rendering errors and continue.
                // Then can be caused even unusual focus management. Nothing we can do about it and showing message box
                // is neither useful nor informative.
            }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            GenerateContent();
        }

        FileSystemWatcher watcher;

        public void GenerateContent(string codeToAnalyse = null)
        {
            try
            {
                if (currentFile.IsScriptFile())
                {
                    Debug.WriteLine("GenerateContent");
                    string code;
                    if (currentFile.Contains("CSScriptNpp\\ReflctedType"))
                    {
                        var safeCode = new StringBuilder();

                        File.ReadAllLines(currentFile).ForEach(line =>
                            {
                                if (line.Contains("sealed struct") && line.Contains(": ValueType"))
                                    safeCode.AppendLine(line.Replace("sealed struct", "class").Replace(": ValueType", "//: ValueType"));
                                else if (line.Contains("partial enum"))
                                    safeCode.AppendLine(line.Replace("partial enum", "partial class"));
                                else
                                    safeCode.AppendLine(line);
                            });
                        code = safeCode.ToString();
                    }
                    else
                    {
                        if (codeToAnalyse != null)
                            code = codeToAnalyse;
                        else
                            code = File.ReadAllText(currentFile);
                    }

                    var members = SimpleCodeCompletion.GetMapOf(code, currentFile).OrderBy(x => x.ParentDisplayName).ToArray();

                    membersList.Items.Clear();

                    string currentType = null;

                    foreach (CodeMapItem item in members)
                    {
                        if (currentType != item.ParentDisplayName)
                        {
                            currentType = item.ParentDisplayName;

                            if (membersList.Items.Count != 0)
                                membersList.Items.Add(new MemberInfo { Line = -1 });

                            membersList.Items.Add(new MemberInfo { Content = item.ParentDisplayName, Line = -1 });
                        }

                        membersList.Items.Add(new MemberInfo { Content = item.DisplayName, ContentIndent = "    ", Line = item.Line - 1 });
                    }

                    ErrorMessage = null;
                }
            }
            catch (SyntaxErrorParsingException e)
            {
                membersList.Items.Clear();
                ErrorMessage = e.Message;
            }
            catch
            {
            }
        }

        public void GenerateContentPython(string codeToAnalyse = null)
        {
            try
            {
                if (currentFile.IsPythonFile())
                {
                    string[] code;
                    if (codeToAnalyse != null)
                        code = codeToAnalyse.Split('\n');
                    else
                        code = File.ReadAllLines(currentFile);

                    membersList.Items.Clear();

                    for (int i = 0; i < code.Length; i++)
                    {
                        var line = code[i].TrimStart();

                        if (line.StartsWithAny("def ", "class "))
                        {
                            var info = new MemberInfo();
                            info.ContentIndent = new string(' ', (code[i].Length - line.Length));
                            info.Line = i;

                            if (line.StartsWith("class "))
                            {
                                membersList.Items.Add(new MemberInfo { Line = -1 });
                                info.ContentType = "class ";
                                info.Content = line.Substring("class ".Length).TrimEnd();
                            }
                            else
                            {
                                info.ContentType = "def ";
                                info.Content = line.Substring("def ".Length).TrimEnd();
                            }

                            membersList.Items.Add(info);
                        }
                    }

                    ErrorMessage = null;
                }
            }
            catch (SyntaxErrorParsingException e)
            {
                membersList.Items.Clear();
                ErrorMessage = e.Message;
            }
            catch
            {
            }
        }

        public string ErrorMessage
        {
            get
            {
                return error.Text;
            }

            set
            {
                error.Text = value;
                error.Visible = (!string.IsNullOrEmpty(error.Text));
            }
        }

        private void CodeMapPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
                RefreshContent();
            try
            {
                if (watcher.Path != null)
                    watcher.EnableRaisingEvents = this.Visible;
            }
            catch { }
        }

        private void refreshLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            RefreshContent();
            Cursor = Cursors.Default;
        }
    }
}