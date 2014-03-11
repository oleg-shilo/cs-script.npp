using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CSScriptNpp
{
    class Debugger : DebuggerServer
    {
        static Debugger()
        {
            DebuggerServer.OnNotificationReceived = HandleNotification;
            DebuggerServer.OnDebuggerStateChanged = HandleDebuggerStateChanged;
            DebuggerServer.OnDebuggeePrrocessNotification = message => Plugin.OutputPanel.DebugOutput.WriteLine(message);

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
                else if (message.StartsWith(NppCategory.SourceCode))
                {
                    //if (stopsCount != 0) //first stop is not a break point but process entry
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
                                Npp.OpenFile(file);
                            }
                        }
                    }
                }
            });
        }

        static char[] delimiter = new[] { '|' };

        static Action OnNextFileOpenComplete;

        static void ProcessDebuggingStepChange(string sourceLocation)
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


            var location = new FileLocation { File = file, Start = points.First(), End = points.Last() };

            ShowSourceLocation(location);
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
        }

        static FileLocation lastLocation;

        static Dictionary<string, IntPtr> breakpoints = new Dictionary<string, IntPtr>();
        static public string EntryBreakpointFile;

        static string BuildBreakpointKey(string file, int line)
        {
            return file + "|" + (line + 1); //server debugger operates in '1-based' lines
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

        static void ShowSourceLocation(FileLocation location)
        {
            ClearDebuggingMarkers();

            Npp.PlaceIndicator(INDICATOR_DEBUGSTEP, location.Start, location.End);
            Npp.PlaceMarker(MARK_DEBUGSTEP, location.Line);
            lastLocation = location;

            int start = Npp.GetFirstVisibleLine();
            int end = start + Npp.GetLinesOnScreen();
            if (location.Line > end || location.Line < start)
            {
                Npp.SetFirstVisibleLine(Math.Max(location.Line-(end-start)/2, 0));
            }

            Win32.SetForegroundWindow(Npp.NppHandle);
            Win32.SetForegroundWindow(Npp.CurrentScintilla);
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
            ClearDebuggingMarkers();
            DebuggerServer.StepOver();
        }

        static new public void StepIn()
        {
            ClearDebuggingMarkers();
            DebuggerServer.StepIn();
        }

        static new public void StepOut()
        {
            ClearDebuggingMarkers();
            DebuggerServer.StepOut();
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