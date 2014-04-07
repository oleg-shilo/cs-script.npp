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
                        foreach (string info in breakpoints.Keys)
                            DebuggerServer.AddBreakpoint(info);

                        if (Debugger.EntryBreakpointFile != null)
                            DebuggerServer.AddBreakpoint(BuildBreakpointKey(Debugger.EntryBreakpointFile, 0)); //line num is 0; debugger is smart enough to move the breakpoint to the very next appropriate line

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
                else if (message.StartsWith(NppCategory.Locals))
                {
                    Plugin.GetDebugPanel().UpdateLocals(message.Substring(NppCategory.Locals.Length));
                    NotifyOnDebugStepChanges(); //zos remove
                }
                else if (message.StartsWith(NppCategory.SourceCode))
                {
                    var sourceLocation = message.Substring(NppCategory.SourceCode.Length);

                    string file = sourceLocation.Split('|').First();

                    if (File.Exists(file))
                    {
                        if (Npp.GetCurrentFile().IsSameAs(file, true))
                        {
                            ProcessDebuggingStepChange(sourceLocation);
                        }
                        else
                        {
                            OnNextFileOpenComplete = () => ProcessDebuggingStepChange(sourceLocation);
                            Npp.OpenFile(file); //needs to by asynchronous
                        }
                    }
                }
            });
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

        static void ProcessDebuggingStepChange(string sourceLocation)
        {
            var location = FileLocation.Parse(sourceLocation);
            ShowBreakpointSourceLocation(location);
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
            public int Line { get { return Npp.GetLineFromPosition(Start); } }

            static public FileLocation Parse(string sourceLocation)
            {
                var parts = sourceLocation.Split('|');
                string file = parts.First();

                var points = parts.Skip(1).Select(x =>
                {
                    var coordinates = x.Split(':');
                    var line = int.Parse(coordinates.First()) - 1;
                    var column = int.Parse(coordinates.Last()) - 1;
                    return Npp.GetPosition(line, column);
                });

                return new FileLocation { File = file, Start = points.First(), End = points.Last() };
            }
        }

        static FileLocation lastLocation;

        static List<string> watchExtressions = new List<string>();
        static Dictionary<string, IntPtr> breakpoints = new Dictionary<string, IntPtr>();
        static public string EntryBreakpointFile;

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
                DebuggerServer.AddBreakpoint(key);
                DebuggerServer.Go();
                DebuggerServer.OnBreak = () =>
                {
                    DebuggerServer.RemoveBreakpoint(key);
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

            string key = BuildBreakpointKey(file, line);

            if (breakpoints.ContainsKey(key))
            {
                Npp.DeleteMarker(breakpoints[key]);
                breakpoints.Remove(key);
                if (IsRunning)
                    DebuggerServer.RemoveBreakpoint(key);
            }
            else
            {
                var handle = Npp.PlaceMarker(MARK_BREAKPOINT, line);
                breakpoints.Add(key, handle);
                if (IsRunning)
                    DebuggerServer.AddBreakpoint(key);
            }
        }

        static void ShowBreakpointSourceLocation(FileLocation location)
        {
            ClearDebuggingMarkers();

            Npp.PlaceIndicator(INDICATOR_DEBUGSTEP, location.Start, location.End);
            Npp.PlaceMarker(MARK_DEBUGSTEP, location.Line);
            lastLocation = location;

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
                DebuggerServer.SetInstructionPointer(Npp.GetCaretLineNumber() + 1); //debugger is 1-based
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