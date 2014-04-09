using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace npp.CSScriptNpp
{
    class MessageQueue
    {
        static WaitableQueue<string> commands = new WaitableQueue<string>();

        static public void AddCommand(string data)
        {
            commands.Enqueue(data);
        }

        static WaitableQueue<string> notifications = new WaitableQueue<string>();

        static public void AddNotification(string data)
        {
            notifications.Enqueue(data);
        }

        static public string WaitForNotification()
        {
            return notifications.WaitItem();
        }

        static public string WaitForCommand()
        {
            return commands.WaitItem();
        }
    }

    class NppCommand
    {
        public static string Exit = "npp.exit";
    }

    class NppCategory
    {
        public static string SourceCode = "source=>";
        public static string Trace = "trace=>";
        public static string CallStack = "callstack=>";
        public static string Locals = "locals=>";
        public static string Threads = "threads=>";
        public static string Modules = "modules=>";
        public static string Watch = "watch=>";
        public static string Invoke = "invoke=>";
        public static string Exception = "exception=>";
        public static string Process = "process=>";
        public static string Settings = "settings=>";
        public static string Breakpoints = "breakpoints=>";
        public static string State = "state=>";
    }

    class RemoteChannelClient
    {
        public Action<string> Trace;

        public void Stop()
        {
            MessageQueue.AddNotification(NppCommand.Exit);
            MessageQueue.AddCommand(NppCommand.Exit);
        }

        public void Start()
        {
            Task.Factory.StartNew(PullCommands);
            Task.Factory.StartNew(PushNotifications);
        }

        public void PullCommands()
        {
            try
            {
                string name = "npp.css.dbg.commands." + Process.GetCurrentProcess().Id;

                using (var pipeClient = new NamedPipeClientStream(".", name, PipeDirection.In))
                using (var reader = new StreamReader(pipeClient))
                {
                    pipeClient.Connect();
                    Trace("Connected as In.Client.");

                    string command;
                    while ((command = reader.ReadLine()) != null)
                    {
                        MessageQueue.AddCommand(command);

                        if (command == NppCommand.Exit)
                        {
                            MessageQueue.AddNotification(NppCommand.Exit); //shutdown the PushNotifications channel
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace("ERROR: " + e.Message);
            }
            Trace("In.Client disconnected. 2");
            //Process.GetCurrentProcess().Kill();
            MessageQueue.AddCommand("exit");
        }

        void PushNotifications()
        {
            try
            {
                string name = "npp.css.dbg.notifications." + Process.GetCurrentProcess().Id;
                using (var pipeClient = new NamedPipeClientStream(".", name, PipeDirection.Out))
                using (var streamWriter = new StreamWriter(pipeClient))
                {
                    pipeClient.Connect();
                    Trace("Connected as Out.Client.");
                    streamWriter.AutoFlush = true;

                    while (true)
                    {
                        string notification = MessageQueue.WaitForNotification();
                        if (notification == NppCommand.Exit)
                            break;
                        streamWriter.WriteLine(notification);
                        pipeClient.WaitForPipeDrain();
                    }
                }
            }
            catch (Exception e)
            {
                Trace("ERROR: " + e.Message);
            }
            Trace("Out.Client disconnected.");
        }
    }
}
