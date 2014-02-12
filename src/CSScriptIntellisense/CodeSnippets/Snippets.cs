using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    public class SnippetContext
    {
        public static int indicatorId = 8;
        public List<List<Point>> ParametersGroups = new List<List<Point>>();
        public List<Point> Parameters = new List<Point>();
        public Point? CurrentParameter;
        public string CurrentParameterValue = "";
        public string ReplacementString = "";
    }

    public class Snippets
    {
        static public Dictionary<string, string> Map = new Dictionary<string, string>();

        static public void ReplaceTextAtIndicator(string text, Point indicatorRange)
        {
            Npp.SetTextBetween(text, indicatorRange);

            //restore the indicator
            Npp.SetIndicatorStyle(SnippetContext.indicatorId, SciMsg.INDIC_BOX, Color.Blue);
            Npp.PlaceIndicator(SnippetContext.indicatorId, indicatorRange.X, indicatorRange.X + text.Length);
        }

        static public void FinalizeCurrent()
        {
            var indicators = Npp.FindIndicatorRanges(SnippetContext.indicatorId);

            foreach (var range in indicators)
                Npp.ClearIndicator(SnippetContext.indicatorId, range.X, range.Y);

            var caretPoint = indicators.Where(point =>
            {
                string text = Npp.GetTextBetween(point);
                return text == " " || text == "|";
            })
                .FirstOrDefault();

            if (caretPoint.X != caretPoint.Y)
            {
                Npp.SetTextBetween("", caretPoint);
                Npp.SetSelection(caretPoint.X, caretPoint.X);
            }
        }

        static public bool NavigateToNextParam(SnippetContext context)
        {
            var indicators = Npp.FindIndicatorRanges(SnippetContext.indicatorId);

            if (!indicators.Any())
                return false;

            Point currentParam = context.CurrentParameter.Value;
            string currentParamOriginalText = context.CurrentParameterValue;

            Npp.SetSelection(currentParam.X, currentParam.X);
            string currentParamDetectedText = Npp.GetWordAtCursor("\t\n\r ,;'\"".ToCharArray());


            if (currentParamOriginalText != currentParamDetectedText)
            {
                //current parameter is modified, indicator is destroyed so restore the indicator first
                Npp.SetIndicatorStyle(SnippetContext.indicatorId, SciMsg.INDIC_BOX, Color.Blue);
                Npp.PlaceIndicator(SnippetContext.indicatorId, currentParam.X, currentParam.X + currentParamDetectedText.Length);

                indicators = Npp.FindIndicatorRanges(SnippetContext.indicatorId);//needs refreshing as the document is modified

                var paramsInfo = indicators.Select(p => new
                {
                    Index = indicators.IndexOf(p),
                    Text = Npp.GetTextBetween(p),
                    Range = p,
                    Pos = p.X
                })
                                           .OrderBy(x => x.Pos)
                                           .ToArray();

                var paramsToUpdate = paramsInfo.Where(item => item.Text == currentParamOriginalText).ToArray();

                foreach (var param in paramsToUpdate)
                {
                    Snippets.ReplaceTextAtIndicator(currentParamDetectedText, indicators[param.Index]);
                    indicators = Npp.FindIndicatorRanges(SnippetContext.indicatorId);//needs refreshing as the document is modified
                }
            }

            Point? nextParameter = null;

            int currentParamIndex = indicators.FindIndex(x => x.X >= currentParam.X); //can also be logical 'next'
            var prevParamsValues = indicators.Take(currentParamIndex).Select(p => Npp.GetTextBetween(p)).ToList();
            prevParamsValues.Add(currentParamOriginalText);
            prevParamsValues.Add(currentParamDetectedText);
            prevParamsValues.Add(" ");
            prevParamsValues.Add("|");

            foreach (var range in indicators.ToArray())
            {
                if (currentParam.X < range.X && !prevParamsValues.Contains(Npp.GetTextBetween(range)))
                {
                    nextParameter = range;
                    break;
                }
            }

            if (!nextParameter.HasValue)
                nextParameter = indicators.FirstOrDefault();

            context.CurrentParameter = nextParameter;
            if (context.CurrentParameter.HasValue)
            {
                Npp.SetSelection(context.CurrentParameter.Value.X, context.CurrentParameter.Value.Y);
                context.CurrentParameterValue = Npp.GetTextBetween(context.CurrentParameter.Value);
            }

            return true;
        }

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

                return Path.Combine(configDir, "snippet.data");
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

        public static SnippetContext PrepareForIncertion(string rawText, int charsOffset, int documentOffset = 0)
        {
            var retval = new SnippetContext();

            retval.ReplacementString = rawText;

            string offset = new string(' ', charsOffset);
            retval.ReplacementString = retval.ReplacementString.Replace(Environment.NewLine, Environment.NewLine + offset);

            int endPos = -1;
            int startPos = retval.ReplacementString.IndexOf("$");

            while (startPos != -1)
            {
                endPos = retval.ReplacementString.IndexOf("$", startPos + 1);

                if (endPos != -1)
                {
                    //'$item$' -> 'item'
                    int newEndPos = endPos - 2;

                    retval.Parameters.Add(new Point(startPos + documentOffset, newEndPos + 1 + documentOffset));

                    string leftText = retval.ReplacementString.Substring(0, startPos);
                    string rightText = retval.ReplacementString.Substring(endPos + 1);
                    string placementValue = retval.ReplacementString.Substring(startPos + 1, endPos - startPos - 1);

                    retval.ReplacementString = leftText + placementValue + rightText;

                    endPos = newEndPos;
                }
                else
                    break;

                startPos = retval.ReplacementString.IndexOf("$", endPos + 1);
            }

            if (retval.Parameters.Any())
                retval.CurrentParameter = retval.Parameters.FirstOrDefault();

            return retval;
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

        

    }
}