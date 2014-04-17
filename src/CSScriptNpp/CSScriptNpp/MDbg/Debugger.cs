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
    class Debugger : DebuggerServer
    {
        static Debugger()
        {
            DebuggerServer.OnNotificationReceived = HandleNotification;
            DebuggerServer.OnDebuggerStateChanged += HandleDebuggerStateChanged;
            DebuggerServer.OnDebuggeeProcessNotification = message => Plugin.OutputPanel.DebugOutput.WriteLine(message);

            var debugStepPointColor = Color.Yellow;

            Npp.SetIndicatorStyle(INDICATOR_DEBUGSTEP, SciMsg.INDIC_STRAIGHTBOX, debugStepPointColor);
            Npp.SetIndicatorTransparency(INDICATOR_DEBUGSTEP, 90, 255);

            Npp.SetMarkerStyle(MARK_DEBUGSTEP, SciMsg.SC_MARK_SHORTARROW, Color.Black, debugStepPointColor);
            Npp.SetMarkerStyle(MARK_BREAKPOINT, CSScriptNpp.Resources.Resources.breakpoint);

            Debugger.BreakOnException = Config.Instance.BreakOnException;
        }

        static public void LoadBreakPointsFor(string file)
        {
            string expectedkeyPreffix = file + "|";
            string[] fileBreakpoints = breakpoints.Keys.Where(x => x.StartsWith(expectedkeyPreffix, StringComparison.OrdinalIgnoreCase)).ToArray();

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
            string expectedkeyPreffix = file + "|";
            string[] fileBreakpoints = breakpoints.Keys.Where(x => x.StartsWith(expectedkeyPreffix, StringComparison.OrdinalIgnoreCase)).ToArray();

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

        static void RefreshBreakPointsForCurrentTab()
        {
            try
            {
                string file = Npp.GetCurrentFile();

                string expectedkeyPreffix = file + "|";
                string[] fileBreakpoints = breakpoints.Keys.Where(x => x.StartsWith(expectedkeyPreffix, StringComparison.OrdinalIgnoreCase)).ToArray();

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

        public static void OnCurrentFileChanged()
        {
            string file = Npp.GetCurrentFile();

            string dbg = CSScriptHelper.GetDbgInfoFile(file);

            if (File.Exists(dbg))
                RefreshBreakPointsForCurrentTab();

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

        static void HandleDebuggerStateChanged()
        {
            ClearDebuggingMarkers();
        }

        static Dictionary<string, Action<string>> invokeCompleteHandlers = new Dictionary<string, Action<string>>();
        static int invokeId = 0;
        static string GetNextInvokeId()
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
                string data = Debugger.Invoke("resolve_primitive", content);
                try
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        var dbgValue = XElement.Parse(data);
                        var dbgValueText = dbgValue.Attribute("value").Value;

                        if (dbgValueText.Length > DbgObject.TrancationSize)
                            return content + ": <value has been truncated because it is too long>";
                        else
                            return content + ": " + dbgValue.Attribute("value").Value;
                    }
                }
                catch { }
            }
            return null;
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

            int start = Environment.TickCount;
            int timeout = 1000;

            while (!done)
            {
                if ((Environment.TickCount - start) > timeout)
                {
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

        static void OnWatchData(string notification)
        {
            if (OnWatchUpdate != null)
                OnWatchUpdate(notification);
        }

        static void OnInvokeComplete(string notification)
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

        static int lastNOSOURCEBREAK = 0;

        static public event Action<string> OnWatchUpdate;
        static public event Action OnFrameChanged; //breakpoint, step advance, process exit
        static public event Action<string> OnNotification;

        static int stepsCount = 0;

        static void HandleNotification(string message)
        {
            //process=>7924:STARTED
            //source=>c:\Users\osh\Documents\Visual Studio 2012\Projects\ConsoleApplication12\ConsoleApplication12\Program.cs|12:9|12:10

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

                        Go();
                    }
                    else if (message.EndsWith(":ENDED"))
                    {
                        NotifyOnDebugStepChanges();
                    }
                }
                else if (message.StartsWith(NppCategory.Exception))
                {
                    OnException(message.Substring(NppCategory.Exception.Length));
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
                        NavigateToFileLocation(sourceLocation, showStep: true);
                    }
                }
            });
        }

        static public void NavigateToFileLocation(string sourceLocation, bool showStep)
        {
            var location = FileLocation.Parse(sourceLocation);
            TranslateCompiledLocation(location);

            if (File.Exists(location.File))
            {
                if (Npp.GetCurrentFile().IsSameAs(location.File, true))
                {
                    ShowBreakpointSourceLocation(location, showStep);
                }
                else
                {
                    OnNextFileOpenComplete = () => ShowBreakpointSourceLocation(location, showStep);
                    Npp.OpenFile(location.File); //needs to by asynchronous
                }
            }
        }

        static void NotifyOnDebugStepChanges()
        {
            Task.Factory.StartNew(() =>
            {
                if (OnFrameChanged != null)
                    OnFrameChanged();
            });
        }

        static char[] delimiter = new[] { '|' };
        static char[] nlDelimiter = new[] { '\n' };

        static Action OnNextFileOpenComplete;

        public static void OpenStackLocation(string sourceLocation)
        {
            string file = sourceLocation.Split('|').First();
            OnNextFileOpenComplete = () => ProcessOpenStackLocation(sourceLocation);
            Npp.OpenFile(file); //needs to by asynchronous
        }

        static void ProcessOpenStackLocation(string sourceLocation)
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

        const int MARK_BOOKMARK = 24;
        const int MARK_HIDELINESBEGIN = 23;
        const int MARK_HIDELINESEND = 22;
        const int MARK_DEBUGSTEP = 8;
        const int MARK_BREAKPOINT = 7;
        const int INDICATOR_DEBUGSTEP = 9;

        class FileLocation
        {
            public string File;
            public int Start;
            public int End;
            public int Line;

            static public FileLocation Parse(string sourceLocation)
            {
                var parts = sourceLocation.Split('|');
                string file = parts.First();

                var points = parts.Skip(1).Select(x =>
                {
                    var coordinates = x.Split(':');
                    var line = int.Parse(coordinates.First()) - 1;
                    var column = int.Parse(coordinates.Last()) - 1;
                    return new { Line = line - 1, Column = column, Position = GetPosition(file, line, column) }; //debugger offsets are 1-based; editor offsets are 0-based
                });

                return new FileLocation { File = file, Line = points.First().Line, Start = points.First().Position, End = points.Last().Position };
            }

            //need to read text as we cannot ask NPP to calculate the position as the file may not be opened (e.g. auto-generated)
            static int GetPosition(string file, int line, int column) //offsets are 1-based
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



        static FileLocation lastLocation;

        static List<string> watchExtressions = new List<string>();
        static Dictionary<string, IntPtr> breakpoints = new Dictionary<string, IntPtr>();
        static public string EntryBreakpointFile;
        static public string ScriptFile;
        static public CSScriptHelper.DecorationInfo DecorationInfo;
        static public Action OnBreakpointChanged;

        public static string[] GetActiveBreakpoints()
        {
            return breakpoints.Keys.ToArray();
        }

        static string BuildBreakpointKey(string file, int line)
        {
            return file + "|" + (line + 1); //server debugger operates in '1-based' lines
        }

        static bool breakOnException = false;

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

        static void ToggleBreakpoint(string file, int line)
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

        static void TranslateCompiledLocation(FileLocation location)
        {
            Debug.WriteLine("-----------------------------");
            Debug.WriteLine("Before: {0} ({1},{2})", location.File, location.Start, location.End);
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
                        }
                        Debug.WriteLine("After: {0} ({1},{2})", location.File, location.Start, location.End);
                    }
                }
                catch { }
            }
            Debug.WriteLine("-----------------------------");
        }

        static int TranslateSourceLineLocation(string file, int line)
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

        static string TranslateSourceBreakpoint(string key)
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

        static void ShowBreakpointSourceLocation(FileLocation location, bool processStep)
        {
            if (processStep)
            {
                ClearDebuggingMarkers();

                Npp.PlaceIndicator(INDICATOR_DEBUGSTEP, location.Start, location.End);
                Npp.PlaceMarker(MARK_DEBUGSTEP, location.Line);
                lastLocation = location;
            }

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

        static void ClearDebuggingMarkers()
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

        static public void Start(string app, string args)
        {
            if (!IsRunning)
            {
                Start();
                SendSettings(BreakOnException);
                watchExtressions.ForEach(x => DebuggerServer.AddWatchExpression(x));
                Run(app, args ?? "");
                EntryBreakpointFile = null;
            }
        }
    }
}