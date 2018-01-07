using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSScriptIntellisense;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace CSScriptNpp
{
    public static class npp
    {
        /***********************************/

        static public void CancelCalltip()
        {
            Npp.GetCurrentDocument().CallTipCancel();
            Calltip.IsShowing = false;
        }

        class Calltip
        {
            static public bool IsShowing = false;
            static public string LastExpression = null;
            static public string LastEval = null;
            static public string LastDocument = null;
        }

        static public void OnCalltipRequest(int position)
        {
            if (position == -2)
            {
                Calltip.LastEval =
                Calltip.LastExpression = null; //if DBG frame is changed so clear the data
            }
            else
            {
                if (Calltip.IsShowing) return;

                Calltip.IsShowing = true;

                Task.Factory.StartNew(() =>  //must be asynch to allow processing other Debugger notifications
                    {
                        string underMouseExpression = Npp.GetCurrentDocument().GetStatementAtPosition(position);
                        string document = Npp.Editor.GetCurrentFilePath();
                        string tooltip = null;

                        //if (Debugger.IsInBreak) //The calltips are used to show the values of the variables only. For everything else (e.g. MemberInfo) modal borderless forms are used
                        //{
                        if (!string.IsNullOrEmpty(underMouseExpression))
                        {
                            //also need to check expression start position (if not debugging) as the same expression can lead to different tooltip
                            //NOTE: if DBG frame is changed the LastExpression is cleared
                            if (underMouseExpression == Calltip.LastExpression && Calltip.LastDocument == document)
                            {
                                if (Debugger.IsInBreak)
                                    tooltip = Calltip.LastEval;
                            }

                            //if (underMouseExpression != Calltip.LastExpression)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("GetDebugTooltipValue -> expression is changed...");
                            //    System.Diagnostics.Debug.WriteLine("old: " + Calltip.LastExpression);
                            //    System.Diagnostics.Debug.WriteLine("new: " + underMouseExpression);
                            //}

                            //if (Calltip.LastDocument != document)
                            //    System.Diagnostics.Debug.WriteLine("GetDebugTooltipValue -> document is changed...");

                            if (tooltip == null)
                            {
                                if (Debugger.IsInBreak)
                                {
                                    tooltip = Debugger.GetDebugTooltipValue(underMouseExpression);
                                }
                                else if (CSScriptIntellisense.Config.Instance.ShowQuickInfoAsNativeNppTooltip)
                                {
                                    tooltip = CSScriptIntellisense.Plugin.GetMemberUnderCursorInfo().FirstOrDefault();
                                }

                                Calltip.LastDocument = document;
                                Calltip.LastEval = tooltip.TruncateLines(Config.Instance.CollectionItemsInTooltipsMaxCount, "\n<Content was truncated. Use F12 to see the raw API documentation data.>");
                                Calltip.LastExpression = underMouseExpression;
                            }

                            if (tooltip != null)
                            {
                                npp.ShowCalltip(position, tooltip);
                                return;
                            }
                        }

                        Calltip.IsShowing = false;
                    });
            }
        }

        public const int MemberInfoPanelMaxWidth = 20;

        static public void ShowCalltip(int position, string text)
        {
            var document = Npp.GetCurrentDocument();
            document.CallTipCancel();
            document.CallTipShow(position, text);
        }

        public static int ConvertToIntFromRGBA(byte red, byte green, byte blue, byte alpha)
        {
            return ((red << 24) | (green << 16) | (blue << 8) | (alpha));
        }

        static public void SetCalltipTime(int milliseconds)
        {
            IntPtr sci = PluginBase.GetCurrentScintilla();
            IntPtr tt = Win32.SendMessage(sci, SciMsg.SCI_GETMOUSEDWELLTIME, 0, 0);

            //Color: 0xBBGGRR
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPSETFORE, 0x000000, 0);
            Win32.SendMessage(sci, SciMsg.SCI_CALLTIPSETBACK, 0xE3E3E3, 0);

            Win32.SendMessage(sci, SciMsg.SCI_SETMOUSEDWELLTIME, milliseconds, 0);
        }

        // Shocking!!!
        // For selection, ranges, text length, navigation
        // Scintilla operates in units, which are not characters but bytes.
        // thus if for the document content "test" you execute selection(start:0,end:3)
        // it will select the whole word [test]
        // However the same for the Cyrillic content "тест" will
        // select only two characters [те]ст because they compose
        // 4 bytes.
        //
        // Basically in Scintilla language "position" is not a character offset
        // but a byte offset.
        //
        // This is a hard to believe Scintilla flaw!!!
        //
        // The problem is discussed here: https://scintillanet.codeplex.com/discussions/218036
        // And here: https://scintillanet.codeplex.com/discussions/455082

        public static int CharOffsetToPosition(this int offset, string file)
        {
            using (var reader = new StreamReader(file))
            {
                var buffer = new char[offset];
                reader.Read(buffer, 0, offset);
                return Encoding.UTF8.GetByteCount(buffer);
            }
            //return Encoding.UTF8.GetByteCount(File.ReadAllText(file).Remove(offset));
        }

        public static int PositionToCharOffset(this int position, string file)
        {
            using (var reader = File.OpenRead(file))
            {
                var buffer = new byte[position];
                reader.Read(buffer, 0, position);
                return Encoding.UTF8.GetCharCount(buffer);
            }
        }

        static public void SaveAllButNew()
        {
            //Win32.SendMessage(Npp.NppHandle, NppMsg.NPPM_SAVEALLFILES, 0, 0);
            var document = Npp.GetCurrentDocument();

            var files = Npp.Editor.GetOpenFiles();
            var current = Npp.Editor.GetCurrentFilePath();
            foreach (var item in files)
            {
                if (Path.IsPathRooted(item))  //the "new" file is not saved so it has no root
                {
                    Npp.Editor.Open(item)
                              .SaveCurrentFile();
                }
            }
            Npp.Editor.Open(current);
        }

        static public void SaveDocuments(string[] files)
        {
            var document = Npp.GetCurrentDocument();

            var filesToSave = files.Select(x => Path.GetFullPath(x));
            var openFiles = Npp.Editor.GetOpenFiles();
            var current = Npp.Editor.GetCurrentFilePath();

            // the "new" file is not saved so it has no root
            foreach (var item in openFiles.Where(Path.IsPathRooted))
            {
                var path = Path.GetFullPath(item);
                if (filesToSave.Contains(path))
                {
                    Npp.Editor.Open(item)
                              .SaveCurrentFile();
                }
            }
            Npp.Editor.Open(current);
        }
    }
}