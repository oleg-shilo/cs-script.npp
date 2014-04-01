using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CSScriptNpp
{
    class NppCategory
    {
        public static string SourceCode = "source=>";
        public static string Process = "process=>";
        public static string Trace = "trace=>";
        public static string Threads = "threads=>";
        public static string CallStack = "callstack=>";
        public static string Invoke = "invoke=>";
        public static string Exception = "exception=>";
        public static string Locals = "locals=>";
        public static string State = "state=>";
        public static string Settings = "settings=>";
        public static string Breakpoints = "breakpoints=>";
        public static string Diagnostics = "debugger=>";
    }

    class DebuggerServer
    {
        static DebuggerServer()
        {
            debugAsConsole = Config.Instance.DebugAsConsole;
        }

        static public void Break()
        {
            if (IsRunning) MessageQueue.AddCommand("break");
        }

        static public void AddBreakpoint(string fileLineInfo)
        {
            MessageQueue.AddCommand("breakpoint+|" + fileLineInfo);
        }

        static public void AddWatch(string id, string expression)
        {
            MessageQueue.AddCommand("watch|" + id + "|" + expression);
        }

        static public void RemoveBreakpoint(string fileLineInfo)
        {
            MessageQueue.AddCommand("breakpoint-|" + fileLineInfo);
        }

        static public void Go()
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("go");
                IsInBreak = false;
            }
        }

        static public void StepOver()
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("next");
                IsInBreak = false;
            }
        }

        static public void StepIn()
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("step");
                IsInBreak = false;
            }
        }

        static public void GoToFrame(string frameId)
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("gotoframe|" + frameId);
            }
        }

        static public void GoToThread(string threadId)
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("gotothread|" + threadId);
            }
        }

        static public void StepOut()
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("out");
                IsInBreak = false;
            }
        }

        static public void SendSettings(bool breakOnException)
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand(NppCategory.Settings + "breakonexception=" + (breakOnException ? "true" : "false"));
            }
        }

        static public void SetInstructionPointer(int line)
        {
            if (IsRunning)
            {
                MessageQueue.AddCommand("setip " + line);
            }
        }

        static public void Run(string application, string args = null)
        {
            if (string.IsNullOrEmpty(args))
                MessageQueue.AddCommand(string.Format("mo nc on\nrun \"{0}\"", application));
            else
                MessageQueue.AddCommand(string.Format("mo nc on\nrun \"{0}\" {1}", application, args));
        }

        static bool debugAsConsole;

        static public bool DebugAsConsole
        {
            get { return debugAsConsole; }
            set
            {
                debugAsConsole = value;
                Config.Instance.DebugAsConsole = debugAsConsole;
            }
        }

        static public bool IsRunning
        {
            get
            {
                try
                {
                    return debuggerProcessId != 0 && Process.GetProcessById(debuggerProcessId) != null;
                }
                catch { }
                return false;
            }
        }

        static bool isInBreak;

        static public bool IsInBreak
        {
            get { return isInBreak; }
            set
            {
                if (isInBreak != value)
                {
                    isInBreak = value;
                    if (isInBreak && OnBreak != null)
                        OnBreak();
                }

                if (OnDebuggerStateChanged != null)
                    OnDebuggerStateChanged();

            }
        }

        static public int DebuggerProcessId
        {
            get
            {
                return debuggerProcessId;
            }
        }

        static protected Action<string> OnNotificationReceived; //debugger notification received
        static public event Action OnDebuggerStateChanged; //breakpoint, step advance, process exit

        static protected Action OnBreak; //breakpoint hit
        static protected Action<string> OnDebuggeeProcessNotification; //debugger process state change

        static void Init()
        {
            initialized = true;
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        string message = WaitForNotification();

                        if (message == NppCommand.Exit)
                            continue; //ignore ClientServer debugger hand-shaking

                        if (message.StartsWith(NppCategory.SourceCode))
                        {
                            IsInBreak = true;
                        }
                        else if (message.StartsWith(NppCategory.Process))
                        {
                            if (OnDebuggerStateChanged != null)
                                OnDebuggerStateChanged();
                        }
                        else if (message.StartsWith(NppCategory.State))
                        {
                        }
                        else if (message.StartsWith(NppCategory.Diagnostics))
                        {
                        }

                        if (OnNotificationReceived != null)
                        {
                            //NOTE: do not start any communication with the debugger from any OnNotificationReceived handler as 
                            //the communication channel is blocked until all handlers return
                            OnNotificationReceived(message);
                        }
                    }
                });
        }

        static string WaitForNotification()
        {
            string message = MessageQueue.WaitForNotification();

            if (message.StartsWith(NppCategory.Process) && message.EndsWith(":STARTED"))
            {
                //<category><id>:STARTED
                string id = message.Substring(NppCategory.Process.Length).Split(':').FirstOrDefault();
                int.TryParse(id, out debuggeeProcessId);
                if (debuggeeProcessId != 0)
                    Task.Factory.StartNew(() =>
                        {
                            IsInBreak = false;

                            try
                            {
                                if (OnDebuggeeProcessNotification != null)
                                    OnDebuggeeProcessNotification("The process [" + debuggeeProcessId + "] started");

                                //debugger often stuck even if debuggee is terminated
                                Process.GetProcessById(debuggeeProcessId).WaitForExit();
                                Process.GetProcessById(debuggerProcessId).Kill();
                            }
                            catch { }

                            if (OnDebuggeeProcessNotification != null)
                                OnDebuggeeProcessNotification("The process [" + debuggeeProcessId + "] has exited.");

                            IsInBreak = false;

                            Plugin.GetDebugPanel().Clear();
                        });
            }
            return message;
        }

        public static void HandleErrors(Action action)
        {
            try { action(); }
            catch { }
        }

        static public void Exit()
        {
            MessageQueue.AddCommand(NppCommand.Exit); //this will shutdown the channels

            if (IsRunning)
            {
                HandleErrors(() => Process.GetProcessById(debuggerProcessId).Kill());
                HandleErrors(() => Process.GetProcessById(debuggeeProcessId).Kill());
            }

            debuggeeProcessId =
            debuggerProcessId = 0;
        }

        public static int debuggerProcessId = 0;
        public static int debuggeeProcessId = 0;

        static RemoteChannelServer channel;

        static bool initialized;
        static public bool Start()
        {
            if (!initialized)
                Init();

            if (debuggerProcessId != 0)
                return false;

            MessageQueue.Clear();

            string debuggerApp = Environment.ExpandEnvironmentVariables("%MDBG_EXE%");

            debuggerApp = Path.Combine(Plugin.PluginDir, @"MDbg\mdbg.exe");

            var debugger = Process.Start(new ProcessStartInfo
                            {
                                FileName = debuggerApp,
                                Arguments = "!load npp.dll",
#if !DEBUG 
                                CreateNoWindow = true,
                                UseShellExecute = false
#endif
                            });

            MessageQueue.AddNotification(NppCategory.Diagnostics + debugger.Id + ":STARTED");
            debuggerProcessId = debugger.Id;

            Task.Factory.StartNew(() => WaitForExit(debugger));

            channel = new RemoteChannelServer(debuggerProcessId);
            channel.Notify = message => Console.WriteLine(message);
            channel.Start();

            return true;
        }

        static void WaitForExit(Process debugger)
        {
            debugger.WaitForExit();

            debuggeeProcessId =
            debuggerProcessId = 0;

            MessageQueue.AddCommand(NppCommand.Exit);

            if (OnDebuggerStateChanged != null)
                OnDebuggerStateChanged();

            MessageQueue.AddNotification(NppCategory.Diagnostics + debugger.Id + ":STOPPED");
        }

        static void Notify(string message)
        {
            if (OnNotificationReceived != null)
                OnNotificationReceived(message);
        }
    }
}

