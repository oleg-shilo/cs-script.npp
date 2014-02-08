using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    public class Snippets
    {
        static public Dictionary<string, string> Map = new Dictionary<string, string>();

        public static bool Contains(string snippetTag)
        {
            lock (Map)
            {
                return Map.ContainsKey(snippetTag);
            }
        }

        public static void Init()
        {
            lock (Map)
            {
                if (!File.Exists(ConfigFile))
                    File.WriteAllText(ConfigFile, CSScriptIntellisense.CodeSnippets.Resources.snippets);
                Read(ConfigFile);
            }
        }

        public static string GetTemplate(string snippetTag)
        {
            lock (Map)
            {
                if (Map.ContainsKey(snippetTag))
                    return Map[snippetTag];
                else
                    return null;
            }
        }

        static string ConfigFile
        {
            get
            {
                string configDir = Path.Combine(Npp.GetConfigDir(), "CSharpIntellisense");

                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);

                return Path.Combine(configDir, "snippet.config");
            }
        }

        static FileSystemWatcher configWatcher;

        static public void EditSnippetsConfig()
        {
            Npp.OpenFile(Snippets.ConfigFile);

            if (configWatcher == null)
            {
                string dir = Path.GetDirectoryName(ConfigFile);
                string fileName = Path.GetFileName(ConfigFile);
                configWatcher = new FileSystemWatcher(dir, fileName);
                configWatcher.NotifyFilter = NotifyFilters.LastWrite;
                configWatcher.Changed += configWatcher_Changed;
                configWatcher.EnableRaisingEvents = true;
            }
        }

        static void configWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Init();
        }

        static void Read(string file)
        {
            //Debug.Assert(false);
            Map.Clear();
            try
            {
                var buffer = new StringBuilder();
                var currentTag = "";

                foreach (var line in File.ReadAllLines(file))
                {
                    if (line.StartsWith("#"))
                        continue; //comment line

                    if (line.EndsWith("=>") && !line.StartsWith(" "))
                    {
                        if (currentTag != "")
                        {
                            Map.Add(currentTag, buffer.ToString());
                            buffer.Clear();
                        }

                        currentTag = line.Replace("=>", "").Trim();
                    }
                    else
                        buffer.AppendLine(line);
                }

                if (currentTag != "")
                    Map.Add(currentTag, buffer.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot load code Snippets.\n" + e.Message, "CS-Script");
            }
        }

        public static string PrepareForIncertion(string text, out int selectionStart, out int selectionLength, int charsOffset)
        {
            string offset = new string(' ', charsOffset);
            text = text.Replace(Environment.NewLine, Environment.NewLine + offset);

            selectionStart = -1;
            selectionLength = 0;

            int startPos = text.IndexOf("$");
            if (startPos != -1)
            {

                int endPos = text.IndexOf("$", startPos + 1);
                if (endPos != -1)
                {
                    //'$item$' -> 'item'
                    string placement = text.Substring(startPos, endPos - startPos + 1);
                    string placementValue = placement.Substring(1, placement.Length - 3);
                    selectionStart = startPos;
                    selectionLength = placementValue.Length;
                    return text.Replace(placement, placementValue);
                }
            }

            return text;
        }
    }

    class ActiveDev
    {
        static void Style()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 8, (int)SciMsg.INDIC_PLAIN);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 9, (int)SciMsg.INDIC_SQUIGGLE);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 10, (int)SciMsg.INDIC_TT);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 11, (int)SciMsg.INDIC_DIAGONAL);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 12, (int)SciMsg.INDIC_STRIKE);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 13, (int)SciMsg.INDIC_BOX);
            Win32.SendMessage(sci, SciMsg.SCI_INDICSETSTYLE, 14, (int)SciMsg.INDIC_ROUNDBOX);

            for (int i = 8; i <= 14; i++)
            {
                Win32.SendMessage(sci, SciMsg.SCI_SETINDICATORCURRENT, i, 0);
                Win32.SendMessage(sci, SciMsg.SCI_INDICSETFORE, i, 0x00ff00);
                int iStart = (int)Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, i - 8, 0);
                Win32.SendMessage(sci, SciMsg.SCI_INDICATORFILLRANGE, iStart, 7);
            }
        }

        static void Unstyle()
        {
            IntPtr sci = Plugin.GetCurrentScintilla();

            for (int i = 8; i <= 14; i++)
            {
                //for (int i = 0; i < length; i++)
                //{

                //}

                Win32.SendMessage(sci, SciMsg.SCI_SETINDICATORCURRENT, i, 0);

                //finding the indicator ranges
                //For example indicator 4..6 in the doc 0..10 will have three logical regions:
                //0..4, 4..6, 6..10
                //Probing will produce following when outcome:
                //probe for 0 : 0..4
                //probe for 4 : 4..6
                //probe for 6 : 4..10
                for (int j = 0; j < 500; j++)
                {
                    int iS = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORSTART, i, j);
                    int iE = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATOREND, i, j);
                    Debug.WriteLine("indicator {0}; Test position {1}; iStart: {2}; iEnd: {3};", i, j, iS, iE);
                }

                //finding indicator presence within a range (by probing the range position)
                //For example indicator 4..6 in the doc 0..10 will have three logical regions:
                //0..4, 5..6, 6..10
                //probe for 3 -> 0
                //probe for 4 -> 1
                //probe for 7 -> 0
                for (int j = 0; j < 500; j++)
                {
                    int value = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORVALUEAT, i, j);
                    //Debug.WriteLine("indicator {0}; Test position {1}; iStart: {2}; iEnd: {3};", i, j, iS, iE);
                }

                int lStart = (int)Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, i - 8, 0);
                int iStart = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORSTART, lStart, lStart + 50);
                int iEnd = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATOREND, lStart, lStart + 50);
                Win32.SendMessage(sci, SciMsg.SCI_INDICATORCLEARRANGE, iStart, iEnd - iStart);

                //int iStart = (int)Win32.SendMessage(sci, SciMsg.SCI_POSITIONFROMLINE, i - 8, 0);
                //Win32.SendMessage(sci, SciMsg.SCI_INDICATORCLEARRANGE, iStart, 7);
            }
        }

        static IEnumerable<Point> FindIndicatorRanges(int indicator)
        {
            var ranges = new List<Point>();

            IntPtr sci = Plugin.GetCurrentScintilla();

            int testPosition = 0;

            while (true)
            {
                //finding the indicator ranges
                //For example indicator 4..6 in the doc 0..10 will have three logical regions:
                //0..4, 4..6, 6..10
                //Probing will produce following when outcome:
                //probe for 0 : 0..4
                //probe for 4 : 4..6
                //probe for 6 : 4..10

                int rangeStart = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORSTART, indicator, testPosition);
                int rangeEnd = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATOREND, indicator, testPosition);
                int value = (int)Win32.SendMessage(sci, SciMsg.SCI_INDICATORVALUEAT, indicator, testPosition);
               // if (value == 1) //indicator is present
                 //   Debug.WriteLine("indicator {0}; Test position {1}; iStart: {2}; iEnd: {3};", i, j, iS, iE);
            }

            return null;
        }

    }
}