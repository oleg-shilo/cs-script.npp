using CSScriptIntellisense;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UltraSharp.Cecil;

namespace CSScriptNpp
{
    public partial class CodeMapPanel : Form
    {
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
        }

        void mapTxt_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int lineNum = mapTxt.Text.Substring(0, mapTxt.SelectionStart).Split('\n').Count() - 1;
                mapTxt.SelectionLength = 0;
                if (currentMapping.ContainsKey(lineNum))
                {
                    Npp.GrabFocus();
                    int currentLineNum = Npp.GetCaretLineNumber();
                    int prevLineEnd = Npp.GetLineStart(currentLineNum) - Environment.NewLine.Length;
                    int topScrollOffset = currentLineNum - Npp.GetFirstVisibleLine();

                    int location = currentMapping[lineNum];
                    if (location != -1)
                    {
                        int newCaretLine = location - 1; //SCI lines are 0-based
                        Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, newCaretLine, 0);
                        Npp.SetFirstVisibleLine(newCaretLine - topScrollOffset);
                    }
                }
            }
            catch { } //it is expected to fail if the line does not contain the file content position spec. This is also the reason for not validating any "IndexOf" results.
        }

        string currentFile;
        Dictionary<int, int> currentMapping = new Dictionary<int, int>();

        public void RefreshContent()
        {
            string file = Npp.GetCurrentFile();
            {
                if (file.IsScriptFile())
                {
                    mapTxt.Visible = true;
                    if (file != currentFile)
                    {
                        currentFile = file;

                        watcher.Path = Path.GetDirectoryName(currentFile);
                        watcher.Filter = Path.GetFileName(currentFile);
                    }

                    mapTxt.Text = "";
                    GenerateContent(Npp.GetTextBetween(0));
                }
                else
                    mapTxt.Visible = false;
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
                    string code;
                    if (codeToAnalyse != null)
                        code = codeToAnalyse;
                    else
                        code = File.ReadAllText(currentFile);
                    var members = Reflector.GetMapOf(code).OrderBy(x => x.ParentDisplayName).ToArray();

                    var builder = new StringBuilder();

                    currentMapping.Clear();
                    int lineNumber = 0;

                    string currentType = null;

                    foreach (Reflector.CodeMapItem item in members)
                    {
                        //eventually coordinates should go to the attached object instead
                        //of being embedded into text

                        if (currentType != item.ParentDisplayName)
                        {
                            currentType = item.ParentDisplayName;

                            if (builder.Length != 0)
                            {
                                builder.AppendLine(); //separator
                                currentMapping.Add(lineNumber, -1);
                                lineNumber++;
                            }

                            builder.AppendLine(item.ParentDisplayName);
                            currentMapping.Add(lineNumber, -1);
                            lineNumber++;
                        }

                        string entry = "    " + item.DisplayName;
                        if (Config.Instance.ShowLineNuberInCodeMap)
                            entry += ": Line " + item.Line;

                        builder.AppendLine(entry);
                        currentMapping.Add(lineNumber, item.Line);
                        lineNumber++;

                    }

                    mapTxt.Text = builder.ToString();
                    ErrorMessage = null;
                }
            }
            catch (Reflector.SyntaxErrorException e)
            {
                mapTxt.Text = "";
                ErrorMessage = e.Message;
            }
            catch (Exception e)
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