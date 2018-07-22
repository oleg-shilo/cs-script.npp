using CSScriptIntellisense.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CSScriptNpp
{
    internal class NppCategory
    {
        public static string SourceCode = "source=>";
        public static string Process = "process=>";
        public static string Trace = "trace=>";
        public static string Threads = "threads=>";
        public static string Modules = "modules=>";
        public static string BreakEntered = "break_entered=>";
        public static string CallStack = "callstack=>";
        public static string Invoke = "invoke=>";
        public static string Exception = "exception=>";
        public static string DbgCommandError = "dbg_error=>";
        public static string Locals = "locals=>";
        public static string State = "state=>";
        public static string Watch = "watch=>";
        public static string Settings = "settings=>";
        public static string Breakpoints = "breakpoints=>";
        public static string Diagnostics = "debugger=>";
    }

    internal class DebuggerServer
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

        static protected void AddWatchExpression(string expression)
        {
            MessageQueue.AddCommand("watch+|" + expression);
        }

        static public void RemoveWatchExpression(string expression)
        {
            MessageQueue.AddCommand("watch-|" + expression);
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
                MessageQueue.AddCommand(NppCategory.Settings + string.Format(
                    "breakonexception={0}|maxItemsInTooltipResolve={1}|maxItemsInResolve={2}",
                    breakOnException.ToString().ToLower(),
                    Config.Instance.CollectionItemsInTooltipsMaxCount,
                    Config.Instance.CollectionItemsInVisualizersMaxCount));
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

        static public void Attach(int proccess)
        {
            MessageQueue.AddCommand("attach " + proccess);
        }

        private static bool debugAsConsole;

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

        private static bool isInBreak;

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
                {
                    Plugin.Log("OnDebuggerStateChanged (isInBreak={0})", isInBreak);
                    OnDebuggerStateChanged();
                }
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

        static public Action OnBreak; //breakpoint hit
        static public Action<string> OnDebuggeeProcessNotification; //debugger process state change

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private static void Init()
        {
            initialized = true;
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        string message = WaitForNotification();

                        if (message == NppCommand.Exit)
                            continue; //ignore ClientServer debugger hand-shaking

                        if (message.StartsWith(NppCategory.BreakEntered))
                        {
                            //Debug.WriteLine("----------------------- "+message);
                            //break_entered=><True|False>
                            IsInBreak = (message == NppCategory.BreakEntered + "True");
                        }
                        else if (message.StartsWith(NppCategory.Process))
                        {
                            if (OnDebuggerStateChanged != null)
                            {
                                OnDebuggerStateChanged();
                            }
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

        private static string WaitForNotification()
        {
            string message = MessageQueue.WaitForNotification();

            if (message.StartsWith(NppCategory.Process) && message.EndsWith(":STARTED"))
            {
                //<category><id>:STARTED
                string id = message.Substring(NppCategory.Process.Length).Split(':').FirstOrDefault();
                int.TryParse(id, out debuggeeProcessId);
                if (debuggeeProcessId != 0)
                {
                    IsInBreak = false;
                    Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                if (OnDebuggeeProcessNotification != null)
                                    OnDebuggeeProcessNotification("The process [" + debuggeeProcessId + "] started");

                                //debugger often stuck even if debuggee is terminated
                                Process.GetProcessById(debuggeeProcessId).WaitForExit();
                                if (debuggerProcessId != 0)
                                    Process.GetProcessById(debuggerProcessId).Kill();
                            }
                            catch { }

                            if (OnDebuggeeProcessNotification != null)
                                OnDebuggeeProcessNotification("The process [" + debuggeeProcessId + "] has exited.");

                            IsInBreak = false;

                            //NppUI.Marshal(() => Dispatcher.Shedule(100, ShowMethodInfo));
                            NppUI.Marshal(() => Plugin.GetDebugPanel().Clear());
                        });
                }
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

        private static RemoteChannelServer channel;

        private static bool initialized;

        static public bool Start(Debugger.CpuType cpu = Debugger.CpuType.Any)
        {
            if (!initialized)
                Init();

            if (debuggerProcessId != 0)
                return false;

            MessageQueue.Clear();

            string debuggerApp = PluginEnv.Locate("mdbg.exe", "MDbg");

            if (cpu == Debugger.CpuType.x86)
                debuggerApp = PluginEnv.Locate("mdbghost_32.exe", "MDbg");
            else if (cpu == Debugger.CpuType.x64)
                debuggerApp = PluginEnv.Locate("mdbghost_64.exe", "MDbg");

            var debugger = Process.Start(new ProcessStartInfo
            {
                FileName = debuggerApp,
                Arguments = "!load npp.dll",
                // #if !DEBUG
                CreateNoWindow = true,
                UseShellExecute = false
                // #endif
            });

            MessageQueue.AddNotification(NppCategory.Diagnostics + debugger.Id + ":STARTED");
            debuggerProcessId = debugger.Id;

            Task.Factory.StartNew(() => WaitForExit(debugger));

            channel = new RemoteChannelServer(debuggerProcessId);
            channel.Notify = message => Console.WriteLine(message);
            channel.Start();

            return true;
        }

        private static void WaitForExit(Process debugger)
        {
            debugger.WaitForExit();

            debuggeeProcessId =
            debuggerProcessId = 0;

            MessageQueue.AddCommand(NppCommand.Exit);

            if (OnDebuggerStateChanged != null)
                OnDebuggerStateChanged();

            MessageQueue.AddNotification(NppCategory.Diagnostics + debugger.Id + ":STOPPED");
        }

        private static void Notify(string message)
        {
            if (OnNotificationReceived != null)
                OnNotificationReceived(message);
        }
    }
}