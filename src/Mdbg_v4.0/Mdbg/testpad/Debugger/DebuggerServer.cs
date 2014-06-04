using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace testpad
{
    class NppCategory
    {
        public static string SourceCode = "source=>";
        public static string Process = "process=>";
        public static string State = "state=>";
        public static string CallStack = "callstack=>";
        public static string Diagnostics = "debugger=>";
    }

    class DebuggerServer
    {
        static public void Break()
        {
            MessageQueue.AddCommand("break");
        }

        static public void Go()
        {
            MessageQueue.AddCommand("go");
        }

        static public void StepOver()
        {
            MessageQueue.AddCommand("next");
        }

        static public void InsertBreakpoint(string file, int line)
        {
            MessageQueue.AddCommand("breakpoint+|"+file+"|"+line);
        }

        static public void StepIn()
        {
            MessageQueue.AddCommand("step");
        }

        static public void StepOut()
        {
            MessageQueue.AddCommand("out");
        }

        static public void Run(string application, string args = null)
        {
            if (string.IsNullOrEmpty(args))
                MessageQueue.AddCommand(string.Format("mo nc on\nrun \"{0}\"", application));
            else
                MessageQueue.AddCommand(string.Format("mo nc on\nrun \"{0}\" {1}", application, args));
        }

        static public bool IsRunning
        {
            get
            {
                return debuggerProcessId != 0;
            }
        }

        static public int DebuggerProcessId
        {
            get
            {
                return debuggerProcessId;
            }
        }

        static event Action<string, int> OnSourceCodePositionChaned;
        static public Action<string> OnNotificationReceived;
        static public Action OnDebuggerStateChanged;

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
                            if (OnSourceCodePositionChaned != null)
                            {
                                string[] parts = message.Substring(NppCategory.SourceCode.Length).Split('|');
                                //OnSourceCodePositionChaned();
                            }
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
            }
            return message;
        }

        static void HandleErrors(Action action)
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

            string debuggerDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var debugger = Process.Start(new ProcessStartInfo
                            {
                                FileName = Path.Combine(debuggerDir, "mdbg.exe"),
                                Arguments = "!load npp.dll",
                                //CreateNoWindow = true,
                                //UseShellExecute = false
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

