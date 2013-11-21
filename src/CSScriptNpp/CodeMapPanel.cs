using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSScriptIntellisense;
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
        }

        void mapTxt_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int lineNum = mapTxt.Text.Substring(0, mapTxt.SelectionStart).Split('\n').Count() - 1;
                mapTxt.SelectionLength = 0;
                if (currentMapping.ContainsKey(lineNum))
                {
                    int currentLineNum = Npp.GetCaretLineNumber();
                    int prevLineEnd = Npp.GetLineStart(currentLineNum) - Environment.NewLine.Length;
                    int topScrollOffset = currentLineNum - Npp.GetFirstVisibleLine();

                    int newCaretLine = currentMapping[lineNum] - 1; //SCI lines are 0-based
                    Win32.SendMessage(Npp.CurrentScintilla, SciMsg.SCI_GOTOLINE, newCaretLine, 0);

                    Npp.SetFirstVisibleLine(newCaretLine - topScrollOffset);
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

                        mapTxt.Text = "";
                        GenerateContent();
                    }
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

        public void GenerateContent()
        {
            try
            {
                if (currentFile.IsScriptFile())
                {
                    string code = File.ReadAllText(currentFile);

                    var builder = new StringBuilder();

                    currentMapping.Clear();
                    int lineNumber = 0;

                    foreach (Reflector.CodeMapItem item in Reflector.GetMapOf(code))
                    {
                        //eventually coordinates should go to the attached objet instead 
                        //of being embedded into text
                        builder.AppendLine(item.DisplayName);
                        currentMapping.Add(lineNumber, item.Line);
                        lineNumber++;
                    }

                    mapTxt.Text = builder.ToString();
                }
            }
            catch (Exception)
            {
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
    }
}
