using CSScriptNpp.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CSScriptNpp
{
    internal class Debugger : DebuggerServer
    {
        static Debugger()
        {
            DebuggerServer.OnNotificationReceived = HandleNotification;
            DebuggerServer.OnDebuggerStateChanged += HandleDebuggerStateChanged;
            DebuggerServer.OnDebuggeeProcessNotification = message => Plugin.OutputPanel.DebugOutput.WriteLine(message);

            var debugStepPointColor = ColorFromConfig(Config.Instance.DebugStepPointColor, Color.Yellow);

            //selection of the debug step line
            Npp.SetIndicatorStyle(INDICATOR_DEBUGSTEP, SciMsg.INDIC_STRAIGHTBOX, debugStepPointColor);
            Npp.SetIndicatorTransparency(INDICATOR_DEBUGSTEP, 90, 255);

            //left 'panel' arrow and breakpoint image
            Npp.SetMarkerStyle(MARK_DEBUGSTEP, SciMsg.SC_MARK_SHORTARROW, ColorFromConfig(Config.Instance.DebugStepPointForeColor, Color.Black), debugStepPointColor);
            Npp.SetMarkerStyle(MARK_BREAKPOINT, CSScriptNpp.Resources.Resources.breakpoint);

            Debugger.BreakOnException = Config.Instance.BreakOnException;
        }

        static Color ColorFromConfig(string name, Color defaultColor)
        {
            try
            {
                return Color.FromName(name);
            }
            catch
            {
                return defaultColor;
            }
        }

        static public void LoadBreakPointsFor(string file)
        {
            string expectedkeyPrefix = file + "|";
            string[] fileBreakpoints = breakpoints.Keys.Where(x => x.StartsWith(expectedkeyPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();

            foreach (var key in fileBreakpoints)
                breakpoints.Remove(key); //clear old if any

            try
            {
                string dbg = CSScriptHelper.GetDbgInfoFile(file);
                if (File.Exists(dbg))
                    foreach (var key in File.ReadAllLines(dbg).Skip(1))
                        breakpoints.Add(key, IntPtr.Zero);
            }
            catch { }
        }

        static public void SaveBreakPointsFor(string file)
        {
            string expectedkeyPrefix = file + "|";
            string[] fileBreakpoints = breakpoints.Keys.Where(x => x.StartsWith(expectedkeyPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (fileBreakpoints.Any())
            {
                try
                {
                    string dbg = CSScriptHelper.GetDbgInfoFile(file, true);
                    string[] header = new[] { "#script: " + file };
                    File.WriteAllLines(dbg, header.Concat(fileBreakpoints).ToArray());
                }
                catch { }
            }
            else
            {
                string dbg = CSScriptHelper.GetDbgInfoFile(file);
                if (File.Exists(dbg))
                    try
                    {
                        File.Delete(dbg);
                    }
                    catch { }
            }
        }

        private static void PlaceBreakPointsForCurrentTab()
        {
            try
            {
                string file = Npp.GetCurrentFile();

                string expectedkeyPrefix = file + "|";
                string[] fileBreakpoints = breakpoints.Keys.Where(x => x.StartsWith(expectedkeyPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();

                foreach (var key in fileBreakpoints)
                {
                    if (breakpoints[key] == IntPtr.Zero) //not placed yet
                    {
                        //key = <file>|<line + 1> //server debugger operates in '1-based' and NPP in '0-based' lines
                        int line = int.Parse(key.Split('|').Last()) - 1;
                        breakpoints[key] = Npp.PlaceMarker(MARK_BREAKPOINT, line);
                    }
                }
            }
            catch { }
        }

        public static void RefreshBreakPointsFromContent()
        {
            var chengedFiles = new List<string>();

            foreach (string key in breakpoints.Keys.ToArray())
            {
                IntPtr marker = breakpoints[key];
                if (marker != IntPtr.Zero)
                {
                    int line = Npp.GetLineOfMarker(marker);
                    if (line != -1 && !key.EndsWith("|" + (line + 1)))
                    {
                        //key = <file>|<line + 1> //server debugger operates in '1-based' and NPP in '0-based' lines
                        string file = key.Split('|').First();

                        if (!chengedFiles.Contains(file))
                            chengedFiles.Add(file);

                        string newKey = file + "|" + (line + 1);

                        breakpoints.Remove(key);
                        breakpoints.Add(newKey, marker);
                    }
                }
            }

            if (OnBreakpointChanged != null)
                OnBreakpointChanged();
        }

        public static void OnCurrentFileChanged()
        {
            string file = Npp.GetCurrentFile();

            string dbg = CSScriptHelper.GetDbgInfoFile(file);

            if (File.Exists(dbg))
                PlaceBreakPointsForCurrentTab();

            if (!IsRunning)
            {
                OnNextFileOpenComplete = null;
                Npp.ClearIndicator(INDICATOR_DEBUGSTEP, 0, -1); //clear all document
            }
            else
            {
                if (OnNextFileOpenComplete != null)
                {
                    OnNextFileOpenComplete();
                    OnNextFileOpenComplete = null;
                }
            }

            if (OnBreakpointChanged != null)
                OnBreakpointChanged();
        }

        private static void HandleDebuggerStateChanged()
        {
            ClearDebuggingMarkers();
        }

        private static Dictionary<string, Action<string>> invokeCompleteHandlers = new Dictionary<string, Action<string>>();
        private static int invokeId = 0;

        private static string GetNextInvokeId()
        {
            lock (typeof(Debugger))
            {
                invokeId++;
                return invokeId.ToString();
            }
        }

        public static string GetDebugTooltipValue(string content)
        {
            if (IsInBreak && IsRunning && !string.IsNullOrWhiteSpace(content))
            {
                string data = Debugger.InvokeResolve("resolve_primitive", content);
                try
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        var dbgValue = XElement.Parse(data);
                        if (dbgValue.Name == "items")
                        {
                            string[] items = dbgValue.Elements()
                                                     .Select(e =>
                                                         {
                                                             var displayValue = e.Attribute("rawDisplayValue");
                                                             if (displayValue != null && displayValue.Value != "")
                                                                 return string.Format(" " + displayValue.Value);
                                                             else
                                                                 return string.Format(" {0}={1}", e.Attribute("name").Value, e.Attribute("value").Value);
                                                         }).ToArray();
                            if (items.Any())
                                return content + ": \n" + string.Join("\n", items);
                            else
                                return content + ": <empty>";
                        }
                        else
                        {
                            var dbgValueText = dbgValue.Attribute("value").Value;

                            if (dbgValueText.Length > DbgObject.TrancationSize)
                                return content + ": <value has been truncated because it is too long>";
                            else
                                return content + ": " + dbgValue.Attribute("value").Value;
                        }
                    }
                }
                catch { }
            }
            return null;
        }

        public static string InvokeResolve(string command, string expression)
        {
            //string code = File.ReadAllText(CallStackPanel.CurrentFrameFile);
            //string[] usings = Reflector.GetCodeUsings(code);
            //string args = expression + "{$NL}" + string.Join("{$NL}", usings);
            //return Invoke(command, args);

            return Invoke(command, expression);
        }

        public static string Invoke(string command, string args)
        {
            string retval = null;
            bool done = false;

            var id = BeginInvoke(command, args, result =>
                {
                    retval = result ?? "";
                    done = true;
                });
                    Debug.WriteLine("---------- Begin Invoke: "+id+" ----------");

            int start = Environment.TickCount;
            int timeout = 10000; //hard to find good timeout

            while (!done)
            {
                if ((Environment.TickCount - start) > timeout)
                {
                    Debug.WriteLine("---------- Timeout ----------");
                    lock (invokeCompleteHandlers)
                    {
                        if (invokeCompleteHandlers.ContainsKey(id))
                        {
                            invokeCompleteHandlers.Remove(id);
                        }
                    }
                    break;
                }
                else
                    Thread.Sleep(1);
            }

            return retval;
        }

        public static string BeginInvoke(string command, string args, Action<string> resultHandler)
        {
            //<id>:<command>:<args>
            string id = GetNextInvokeId();
            lock (invokeCompleteHandlers)
            {
                invokeCompleteHandlers.Add(id, resultHandler);
            }
            // Debug.WriteLine("Invoke send: " + id + "; " + string.Format("{0}:{1}:{2}", id, command, args ?? ""));
            MessageQueue.AddCommand(NppCategory.Invoke + string.Format("{0}:{1}:{2}", id, command, args ?? ""));
            return id;
        }

        public static void OnDebuggerFailure(string message)
        {
            string data = message.Replace("{$NL}", "\n");
            //<command>:<error>
            string[] parts = data.Split(new[] { ':' }, 2);
            string lastCommand = parts[0];
            string error = parts[1];

            if (lastCommand == "attach" || lastCommand == "run" || lastCommand == "") //critical failure
            {
                if (IsRunning)
                    Stop();

                //it is important not to display msgbox in this thread as it would release message pump
                //while we are handling the current Windows message. We are in 'NppUI.Marshal'
                if (lastCommand == "")
                    ThreadPool.QueueUserWorkItem(x => MessageBox.Show(string.Format("Debugger failed to execute last command.\n{0} ", error), "CS-Script"));
                else
                    ThreadPool.QueueUserWorkItem(x => MessageBox.Show(string.Format("Debugger failed to execute '{0}' command.\n{1} ", lastCommand, error), "CS-Script"));
            }
        }

        public static void OnException(string message)
        {
            //user+:<description>{$NL}Location: <file|line,column|line,column>{$NL}<exceptionName>{$NL}<info>
            string exceptionData = message.Replace("{$NL}", "\n");

            if (exceptionData.StartsWith("user+"))
            {
                string[] lines = exceptionData.Substring("user+".Length + 1).Split(nlDelimiter, 4);

                string description = lines[0];
                string location = lines[1];
                string excName = lines[2];
                string info = lines[3];

                string[] locationParts = location.Split('|');

                string exceptionMessageShort = string.Format("{0}\r\n  {1}\r\n  {2} ({3})", description, excName, locationParts[0], locationParts[1].Replace(":", ", "));
                Plugin.OutputPanel.ShowDebugOutput().WriteLine(exceptionMessageShort);

                string exceptionMessageFull = string.Format("{0}\n{1} ({2})\n{3}\n{4}",
                                                  description,
                                                  locationParts[0], locationParts[1].Replace(":", ", "),
                                                  excName,
                                                  info);

                var nativeWindow = new NativeWindow();
                nativeWindow.AssignHandle(Plugin.NppData._nppHandle);
                MessageBox.Show(nativeWindow, exceptionMessageFull, "CS-Script");
            }
            else
            {
                //user-:<description>{$NL}<exceptionName>{$NL}<info>
                string[] lines = exceptionData.Substring("user+".Length + 1).Split(nlDelimiter, 2);
                Plugin.OutputPanel.ShowDebugOutput().WriteLine(lines[0] + "\r\n  " + lines[1]);
                Go();
            }
        }

        private static void OnWatchData(string notification)
        {
            if (OnWatchUpdate != null)
                OnWatchUpdate(notification);
        }

        private static void OnInvokeComplete(string notification)
        {
            //<id>:<result>
            string[] parts = notification.Split(new[] { ':' }, 2);
            string id = parts[0];
            string result = parts[1];

            Action<string> handler = null;

            Debug.WriteLine("Invoke received: " + id + "; " + result);

            lock (invokeCompleteHandlers)
            {
                if (invokeCompleteHandlers.ContainsKey(id))
                {
                    handler = invokeCompleteHandlers[id];
                    invokeCompleteHandlers.Remove(id);
                }
            }

            Debug.WriteLine("Invoke returned: id=" + id + " size=" + result.Length);

            if (handler != null)
                handler(result);
        }

        private static int lastNOSOURCEBREAK = 0;

        static public event Action<string> OnWatchUpdate;

        static public event Action OnFrameChanged; //breakpoint, step advance, process exit

        static public event Action<string> OnNotification;

        private static int stepsCount = 0;
        private static bool resumeOnNextBreakPoint = false;

        private static void HandleNotification(string notification)
        {
            //process=>7924:STARTED
            //source=>c:\Users\osh\Documents\Visual Studio 2012\Projects\ConsoleApplication12\ConsoleApplication12\Program.cs|12:9|12:10

            string message = notification;

            Debug.WriteLine("Received: " + message);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (OnNotification != null)
                        OnNotification(message);
                }
                catch { }
            });

            HandleErrors(() =>
            {
                if (message.StartsWith(NppCategory.Process))
                {
                    ClearDebuggingMarkers();

                    if (message.EndsWith(":STARTED"))
                    {
                        stepsCount = 0;
                        DecorationInfo = CSScriptHelper.GetDecorationInfo(Debugger.ScriptFile);

                        foreach (string info in breakpoints.Keys)
                            DebuggerServer.AddBreakpoint(TranslateSourceBreakpoint(info));

                        if (Debugger.EntryBreakpointFile != null)
                        {
                            //line num is 0; debugger is smart enough to move the breakpoint to the very next appropriate line
                            string key = BuildBreakpointKey(Debugger.EntryBreakpointFile, 0);

                            if (DecorationInfo != null && DecorationInfo.AutoGenFile == Debugger.EntryBreakpointFile)
                            {
                                key = BuildBreakpointKey(DecorationInfo.AutoGenFile, DecorationInfo.InjectedLineNumber + 1);
                            }

                            DebuggerServer.AddBreakpoint(key);
                        }

                        //By default Mdbg always enters break mode at start/attach completed
                        //however we should only resume after mdbg finished the initialization (first break is reported).
                        resumeOnNextBreakPoint = true;
                    }
                    else if (message.EndsWith(":ENDED"))
                    {
                        NotifyOnDebugStepChanges();
                    }
                }
                else if (message.StartsWith(NppCategory.BreakEntered))
                {
                    if (resumeOnNextBreakPoint)
                    {
                        resumeOnNextBreakPoint = false;
                        Go();
                    }
                }
                else if (message.StartsWith(NppCategory.Exception))
                {
                    OnException(message.Substring(NppCategory.Exception.Length));
                }
                else if (message.StartsWith(NppCategory.DbgCommandError))
                {
                    OnDebuggerFailure(message.Substring(NppCategory.DbgCommandError.Length));
                }
                else if (message.StartsWith(NppCategory.Invoke))
                {
                    OnInvokeComplete(message.Substring(NppCategory.Invoke.Length));
                }
                else if (message.StartsWith(NppCategory.Watch))
                {
                    OnWatchData(message.Substring(NppCategory.Watch.Length));
                }
                else if (message.StartsWith(NppCategory.Trace))
                {
                    Plugin.OutputPanel.DebugOutput.Write(message.Substring(NppCategory.Trace.Length));
                }
                else if (message.StartsWith(NppCategory.State))
                {
                    if (message.Substring(NppCategory.State.Length) == "NOSOURCEBREAK")
                        Task.Factory.StartNew(() =>
                        {
                            //The break can be caused by 'Debugger.Break();'
                            //Trigger user break to force source code entry if available as a small usability improvement.
                            if ((Environment.TickCount - lastNOSOURCEBREAK) > 1500) //Just in case, prevent infinite loop.
                            {
                                lastNOSOURCEBREAK = Environment.TickCount;
                                Thread.Sleep(200); //even 80 is enough
                                Debugger.Break();
                            }
                        });
                }
                else if (message.StartsWith(NppCategory.CallStack))
                {
                    Plugin.GetDebugPanel().UpdateCallstack(message.Substring(NppCategory.CallStack.Length));
                }
                else if (message.StartsWith(NppCategory.Threads))
                {
                    Plugin.GetDebugPanel().UpdateThreads(message.Substring(NppCategory.Threads.Length));
                }
                else if (message.StartsWith(NppCategory.Modules))
                {
                    Plugin.GetDebugPanel().UpdateModules(message.Substring(NppCategory.Modules.Length));
                }
                else if (message.StartsWith(NppCategory.Locals))
                {
                    Plugin.GetDebugPanel().UpdateLocals(message.Substring(NppCategory.Locals.Length));
                    NotifyOnDebugStepChanges(); //zos remove
                }
                else if (message.StartsWith(NppCategory.SourceCode))
                {
                    stepsCount++;

                    var sourceLocation = message.Substring(NppCategory.SourceCode.Length);

                    if (stepsCount == 1 && sourceLocation.Contains("css_dbg.cs|"))
                    {
                        //ignore the first source code break as it is inside of the CSScript.Npp debug launcher
                    }
                    else
                    {
                        NavigateToFileLocation(sourceLocation);
                    }
                }
            });
        }

        static public void NavigateToFileLocation(string sourceLocation)
        {
            var location = FileLocation.Parse(sourceLocation);
            location.Start = Npp.CharOffsetToPosition(location.Start, location.File);
            location.End = Npp.CharOffsetToPosition(location.End, location.File);

            TranslateCompiledLocation(location);

            if (File.Exists(location.File))
            {
                if (Npp.GetCurrentFile().IsSameAs(location.File, true))
                {
                    ShowBreakpointSourceLocation(location);
                }
                else
                {
                    OnNextFileOpenComplete = () => ShowBreakpointSourceLocation(location);
                    Npp.OpenFile(location.File); //needs to by asynchronous
                }
            }
        }

        private static void NotifyOnDebugStepChanges()
        {
            Task.Factory.StartNew(() =>
            {
                if (OnFrameChanged != null)
                    OnFrameChanged();
            });
        }

        private static char[] delimiter = new[] { '|' };
        private static char[] nlDelimiter = new[] { '\n' };

        private static Action OnNextFileOpenComplete;

        public static void OpenStackLocation(string sourceLocation)
        {
            string file = sourceLocation.Split('|').First();
            OnNextFileOpenComplete = () => ProcessOpenStackLocation(sourceLocation);
            Npp.OpenFile(file); //needs to by asynchronous
        }

        private static void ProcessOpenStackLocation(string sourceLocation)
        {
            var location = Debugger.FileLocation.Parse(sourceLocation);

            int start = Npp.GetFirstVisibleLine();
            int end = start + Npp.GetLinesOnScreen();
            if (location.Line > end || location.Line < start)
            {
                Npp.SetFirstVisibleLine(Math.Max(location.Line - (end - start) / 2, 0));
            }

            Npp.SetCaretPosition(location.Start);
            Npp.ClearSelection(); //need this one as otherwise parasitic selection can be triggered

            Win32.SetForegroundWindow(Npp.NppHandle);
            Win32.SetForegroundWindow(Npp.CurrentScintilla);
        }

        private const int MARK_BOOKMARK = 24;
        private const int MARK_HIDELINESBEGIN = 23;
        private const int MARK_HIDELINESEND = 22;
        private const int MARK_DEBUGSTEP = 8;
        private const int MARK_BREAKPOINT = 7;
        private const int INDICATOR_DEBUGSTEP = 9;

        internal class FileLocation
        {
            public string File;
            public int Start;
            public int End;
            public int Line;

            public bool IsSame(FileLocation location)
            {
                return File == location.File &&
                       Start == location.Start &&
                       End == location.End &&
                       Line == location.Line;
            }

            static public FileLocation Parse(string sourceLocation)
            {
                var parts = sourceLocation.Split('|');
                string file = parts.First();

                var points = parts.Skip(1).Select(x =>
                {
                    var coordinates = x.Split(':');
                    var line = int.Parse(coordinates.First()) - 1;
                    var column = int.Parse(coordinates.Last()) - 1;
                    return new { Line = line, Column = column, Position = GetPosition(file, line, column) }; //debugger offsets are 1-based; editor offsets are 0-based
                });

                return new FileLocation { File = file, Line = points.First().Line, Start = points.First().Position, End = points.Last().Position };
            }

            //need to read text as we cannot ask NPP to calculate the position as the file may not be opened (e.g. auto-generated)
            private static int GetPosition(string file, int line, int column) //offsets are 1-based
            {
                using (var reader = new StreamReader(file))
                {
                    int lineCount = 0;
                    int columnCount = 0;
                    int pos = 0;

                    while (reader.Peek() >= 0)
                    {
                        var c = (char)reader.Read();

                        if (lineCount == line && columnCount == column)
                            break;

                        pos++;

                        if (lineCount == line)
                            columnCount++;

                        if (c == '\n')
                            lineCount++;
                    }

                    return pos;
                }
            }
        }

        private static FileLocation lastLocation;

        private static List<string> watchExtressions = new List<string>();
        private static Dictionary<string, IntPtr> breakpoints = new Dictionary<string, IntPtr>();
        static public string EntryBreakpointFile;
        static public string ScriptFile;
        static public CSScriptHelper.DecorationInfo DecorationInfo;
        static public Action OnBreakpointChanged;

        public static string[] GetActiveBreakpoints()
        {
            return breakpoints.Keys.ToArray();
        }

        private static string BuildBreakpointKey(string file, int line)
        {
            return file + "|" + (line + 1); //server debugger operates in '1-based' lines
        }

        private static bool breakOnException = false;

        static public bool BreakOnException
        {
            get { return breakOnException; }
            set
            {
                breakOnException = value;
                SendSettings(breakOnException);
                if (breakOnException != Config.Instance.BreakOnException)
                {
                    Config.Instance.BreakOnException = breakOnException;
                    Config.Instance.Save();
                }
            }
        }

        static public void RunToCursor()
        {
            if (IsRunning && IsInBreak)
            {
                string file = Npp.GetCurrentFile();
                int line = Npp.GetCaretLineNumber();
                string key = BuildBreakpointKey(file, line);
                string actualBreakpoint = TranslateSourceBreakpoint(key);
                DebuggerServer.AddBreakpoint(actualBreakpoint);
                DebuggerServer.Go();
                DebuggerServer.OnBreak = () =>
                {
                    DebuggerServer.RemoveBreakpoint(actualBreakpoint);
                };
            }
        }

        static public void AddWatch(string expression)
        {
            if (!watchExtressions.Contains(expression))
            {
                watchExtressions.Add(expression);
            }
            if (IsRunning)
                DebuggerServer.AddWatchExpression(expression);
        }

        static public void RemoveWatch(string expression)
        {
            if (watchExtressions.Contains(expression))
            {
                watchExtressions.Remove(expression);
                if (IsRunning)
                    DebuggerServer.RemoveWatchExpression(expression);
            }
        }

        static public void RemoveAllWatch()
        {
            watchExtressions.ForEach(x => DebuggerServer.RemoveWatchExpression(x));
            watchExtressions.Clear();
        }

        static public void ToggleBreakpoint(int lineClick = -1)
        {
            string file = Npp.GetCurrentFile();

            int line = 0;

            if (lineClick != -1)
                line = lineClick;
            else
                line = Npp.GetCaretLineNumber();

            bool isAutoGenerated = file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);

            if (!isAutoGenerated)
                ToggleBreakpoint(file, line);

            if (OnBreakpointChanged != null)
                OnBreakpointChanged();
        }

        static public void RemoveAllBreakpoints()
        {
            foreach (var key in breakpoints.Keys)
            {
                Npp.DeleteMarker(breakpoints[key]);
                if (IsRunning)
                    DebuggerServer.RemoveBreakpoint(key);
            }
            breakpoints.Clear();

            foreach (string file in Npp.GetOpenFiles())
            {
                string dbgInfo = CSScriptHelper.GetDbgInfoFile(file, false);
                if (File.Exists(dbgInfo))
                    File.Delete(dbgInfo);
            }

            if (OnBreakpointChanged != null)
                OnBreakpointChanged();
        }

        private static void ToggleBreakpoint(string file, int line)
        {
            string key = BuildBreakpointKey(file, line);

            if (breakpoints.ContainsKey(key))
            {
                Npp.DeleteMarker(breakpoints[key]);
                breakpoints.Remove(key);
                if (IsRunning)
                {
                    string actualKey = TranslateSourceBreakpoint(key);
                    DebuggerServer.RemoveBreakpoint(key);
                }
            }
            else
            {
                var handle = Npp.PlaceMarker(MARK_BREAKPOINT, line);
                breakpoints.Add(key, handle);
                if (IsRunning)
                {
                    string actualKey = TranslateSourceBreakpoint(key);
                    DebuggerServer.AddBreakpoint(actualKey);
                }
            }
        }

        static public void TranslateCompiledLocation(FileLocation location)
        {
            //Shocking!!!
            //For selection, ranges, text length, navigation
            //Scintilla operates in units, which are not characters but bytes.
            //thus if for the document content "test" you execute selection(start:0,end:3)
            //it will select the whole word [test]
            //However the same for the Cyrillic content "тест" will
            //select only two characters [те]ст because they compose
            //4 bytes.
            //
            //Basically in Scintilla language "position" is not a character offset
            //but a byte offset.
            //
            //This is a hard to believe Scintilla flaw!!!
            //The problem is discussed here: https://scintillanet.codeplex.com/discussions/218036
            //And here: https://scintillanet.codeplex.com/discussions/455082

            //Debug.WriteLine("-----------------------------");
            //Debug.WriteLine("Before: {0} ({1},{2})", location.File, location.Start, location.End);
            //return;
            if (DecorationInfo != null)
            {
                try
                {
                    if (location.File.IsSameAs(DecorationInfo.AutoGenFile, true))
                    {
                        location.File = DecorationInfo.ScriptFile;
                        if (location.Start > (DecorationInfo.IngecionStart + DecorationInfo.IngecionLength))
                        {
                            location.Start -= DecorationInfo.IngecionLength;
                            location.End -= DecorationInfo.IngecionLength;
                            location.Line--;
                        }
                        //Debug.WriteLine("After: {0} ({1},{2})", location.File, location.Start, location.End);
                    }
                }
                catch { }
            }
            Debug.WriteLine("-----------------------------");
        }

        private static int TranslateSourceLineLocation(string file, int line)
        {
            if (DecorationInfo != null)
            {
                if (file.IsSameAs(DecorationInfo.ScriptFile, true))
                {
                    if (line >= DecorationInfo.InjectedLineNumber)
                        line++;

                    return line;
                }
            }
            return line;
        }

        private static string TranslateSourceBreakpoint(string key)
        {
            //return key;
            if (DecorationInfo == null)
            {
                return key;
            }
            else
            {
                try
                {
                    //<file>|<line> //server debugger operates in '1-based' lines
                    string[] parts = key.Split('|');

                    string file = parts[0];
                    int line = int.Parse(parts[1]) - 1;

                    if (file.IsSameAs(DecorationInfo.ScriptFile, true))
                    {
                        if (line >= DecorationInfo.InjectedLineNumber)
                            line++;

                        return DecorationInfo.AutoGenFile + "|" + (line + 1);
                    }
                }
                catch { }
                return key;
            }
        }

        private static void ShowBreakpointSourceLocation(FileLocation location)
        {
            if (lastLocation != null && lastLocation.IsSame(location)) return;
            lastLocation = location;

            ClearDebuggingMarkers();

            Npp.PlaceIndicator(INDICATOR_DEBUGSTEP, location.Start, location.End);
            Npp.PlaceMarker(MARK_DEBUGSTEP, location.Line);

            int start = Npp.GetFirstVisibleLine();
            int end = start + Npp.GetLinesOnScreen();
            if (location.Line > end || location.Line < start)
            {
                Npp.SetFirstVisibleLine(Math.Max(location.Line - (end - start) / 2, 0));
            }

            Npp.SetCaretPosition(location.Start);
            Npp.ClearSelection(); //need this one as otherwise parasitic selection can be triggered

            Win32.SetForegroundWindow(Npp.NppHandle);
        }

        private static void ClearDebuggingMarkers()
        {
            if (lastLocation != null)
            {
                if (string.Compare(Npp.GetCurrentFile(), lastLocation.File, true) == 0)
                {
                    Npp.ClearIndicator(INDICATOR_DEBUGSTEP, lastLocation.Start, lastLocation.End);
                    Npp.DeleteAllMarkers(MARK_DEBUGSTEP);
                }
            }
        }

        static public void Stop()
        {
            if (IsRunning)
                Exit();
        }

        static new public void Go()
        {
            ClearDebuggingMarkers();
            DebuggerServer.Go();
        }

        static new public void StepOver()
        {
            if (IsRunning && IsInBreak)
            {
                ClearDebuggingMarkers();
                DebuggerServer.StepOver();
            }
        }

        static new public void StepIn()
        {
            if (IsRunning && IsInBreak)
            {
                ClearDebuggingMarkers();
                DebuggerServer.StepIn();
            }
        }

        static new public void GoToFrame(string frameId)
        {
            if (IsRunning && IsInBreak)
            {
                DebuggerServer.GoToFrame(frameId);
            }
        }

        static new public void GoToThread(string threadId)
        {
            if (IsRunning && IsInBreak)
            {
                DebuggerServer.GoToThread(threadId);
            }
        }

        static new public void StepOut()
        {
            if (IsRunning && IsInBreak)
            {
                ClearDebuggingMarkers();
                DebuggerServer.StepOut();
            }
        }

        static public void SetInstructionPointer()
        {
            if (IsRunning && IsInBreak)
            {
                ClearDebuggingMarkers();

                int line = TranslateSourceLineLocation(Npp.GetCurrentFile(), Npp.GetCaretLineNumber());

                DebuggerServer.SetInstructionPointer(line + 1); //debugger is 1-based
                DebuggerServer.Break(); //need to break to trigger reporting the current step
            }
        }

        public enum CpuType
        {
            Any,
            x86,
            x64,
        }

        static public void Attach(int process, CpuType cpu)
        {
            if (!IsRunning)
            {
                Start(cpu);
                SendSettings(BreakOnException);
                watchExtressions.ForEach(x => DebuggerServer.AddWatchExpression(x));
                Attach(process);
                EntryBreakpointFile = null;
            }
        }

        static public void Start(string app, string args, CpuType cpu)
        {
            if (!IsRunning)
            {
                lastLocation = null;
                Start(cpu);
                SendSettings(BreakOnException);
                watchExtressions.ForEach(x => DebuggerServer.AddWatchExpression(x));
                Run(app, args ?? "");
                EntryBreakpointFile = null;
            }
        }
    }
}