using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace CSScriptNpp
{
    class Debugger : DebuggerServer
    {
        static Debugger()
        {
            DebuggerServer.OnNotificationReceived = HandleNotification;
            DebuggerServer.OnDebuggerStateChanged = HandleDebuggerStateChanged;
            DebuggerServer.OnDebuggeeProcessNotification = message => Plugin.OutputPanel.DebugOutput.WriteLine(message);

            var debugStepPointColor = Color.Yellow;

            Npp.SetIndicatorStyle(INDICATOR_DEBUGSTEP, SciMsg.INDIC_STRAIGHTBOX, debugStepPointColor);
            Npp.SetIndicatorTransparency(INDICATOR_DEBUGSTEP, 90, 255);

            Npp.SetMarkerStyle(MARK_DEBUGSTEP, SciMsg.SC_MARK_SHORTARROW, Color.Black, debugStepPointColor);
            //Npp.SetMarkerStyle(MARK_BREAKPOINT, SciMsg.SC_MARK_CIRCLE, Color.Black, Color.Red);
            Npp.SetMarkerStyle(MARK_BREAKPOINT, CSScriptNpp.Resources.Resources.breakpoint);
        }

        public static void OnCurrentFileChanged()
        {
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

        public static string Invoke(string command, string args)
        {
            string retval = null;
            BeginInvoke(command, args, result => retval = result);

            int start = Environment.TickCount;
            int timeout = 1000;

            while (retval == null && (Environment.TickCount - start) < timeout)
            {
                Thread.Sleep(1);
            }

            return retval;

        }

        public static void BeginInvoke(string command, string args, Action<string> resultHandler)
        {
            //<id>:<command>:<args>
            string id = GetNextInvokeId();
            lock (invokeCompleteHandlers)
            {
                invokeCompleteHandlers.Add(id, resultHandler);
            }
            MessageQueue.AddCommand(NppCategory.Invoke + string.Format("{0}:{1}:{2}", id, command, args ?? ""));
        }

        public static void OnInvokeComplete(string notification)
        {
            //<id>:<result>
            string[] parts = notification.Split(new[] { ':' }, 2);
            string id = parts[0];
            string result = parts[1];

            Action<string> handler = null;

            lock (invokeCompleteHandlers)
            {
                if (invokeCompleteHandlers.ContainsKey(id))
                {
                    handler = invokeCompleteHandlers[id];
                    invokeCompleteHandlers.Remove(id);
                }
            }

            if (handler != null)
                handler(result);
        }

        static void HandleNotification(string message)
        {
            //process=>7924:STARTED
            //source=>c:\Users\osh\Documents\Visual Studio 2012\Projects\ConsoleApplication12\ConsoleApplication12\Program.cs|12:9|12:10

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
                }
                else if (message.StartsWith(NppCategory.Invoke))
                {
                    OnInvokeComplete(message.Substring(NppCategory.Invoke.Length));
                }
                else if (message.StartsWith(NppCategory.Trace))
                {
                    Plugin.OutputPanel.DebugOutput.Write(message.Substring(NppCategory.Trace.Length));
                }
                else if (message.StartsWith(NppCategory.CallStack))
                {
                    Plugin.GetDebugPanel().UpdateCallstack(message.Substring(NppCategory.CallStack.Length));
                }
                else if (message.StartsWith(NppCategory.Locals))
                {
                    Plugin.GetDebugPanel().UpdateLocals(message.Substring(NppCategory.Locals.Length));
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

        static char[] delimiter = new[] { '|' };

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

        static Dictionary<string, IntPtr> breakpoints = new Dictionary<string, IntPtr>();
        static public string EntryBreakpointFile;

        static string BuildBreakpointKey(string file, int line)
        {
            return file + "|" + (line + 1); //server debugger operates in '1-based' lines
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
                DebuggerServer.OnBreak = ()=>
                {
                    DebuggerServer.RemoveBreakpoint(key);
                };
            }
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
                Run(app, args ?? "");
                EntryBreakpointFile = null;
            }
        }
    }
}