using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace testpad
{
    class WaitableQueue<T> : Queue<T>
    {
        AutoResetEvent itemAvailable = new AutoResetEvent(false);

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            itemAvailable.Set();
        }

        public T WaitItem()
        {
            while (true)
            {
                if (base.Count > 0)
                    return base.Dequeue();

                itemAvailable.WaitOne();
            }
        }
    }

    class MessageQueue
    {
        static WaitableQueue<string> commands = new WaitableQueue<string>();

        static public void AddCommand(string data)
        {
            if (data == "npp.exit")
                Debug.WriteLine("");
            commands.Enqueue(data);
        }

        static WaitableQueue<string> notifications = new WaitableQueue<string>();

        static public void AddNotification(string data)
        {
            notifications.Enqueue(data);
        }

        static public void Clear()
        {
            notifications.Clear();
            commands.Clear();
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

    class RemoteChannelServer
    {
        public Action<string> Notify;
        int remoteProcessId;

        public RemoteChannelServer(int rempteProcessId)
        {
            this.remoteProcessId = rempteProcessId;
        }

        public void Start()
        {
            Task.Factory.StartNew(PullNotifications);
            Task.Factory.StartNew(PushCommands);
        }

        public void Stop()
        {
            MessageQueue.AddCommand(NppCommand.Exit);
        }

        public void PullNotifications()
        {
            try
            {
                string name = "npp.css.dbg.notifications." + remoteProcessId;
                using (var pipeServer = new NamedPipeServerStream(name, PipeDirection.In))
                using (var streamReader = new StreamReader(pipeServer))
                {
                    pipeServer.WaitForConnection();
                    Notify("In.Client connected.");

                    while (true)
                    {
                        string message = streamReader.ReadLine();

                        if (message == NppCommand.Exit || message == null)
                            break;

                        MessageQueue.AddNotification(message);
                    }
                }
            }
            catch (IOException e)
            {
                Notify("ERROR: " + e.Message);
            }

            Notify("In.Client disconnected.");
        }

        void PushCommands()
        {
            try
            {
                string name = "npp.css.dbg.commands." + remoteProcessId;
                using (var pipeServer = new NamedPipeServerStream(name, PipeDirection.Out))
                using (var streamWriter = new StreamWriter(pipeServer))
                {
                    pipeServer.WaitForConnection();
                    Notify("Out.Client connected.");
                    streamWriter.AutoFlush = true;

                    while (true)
                    {
                        string command = MessageQueue.WaitForCommand();

                        streamWriter.WriteLine(command);
                        if (command == NppCommand.Exit)
                        {
                            MessageQueue.AddNotification(NppCommand.Exit); //signal to stop the PullNotifications channel
                            break;
                        }
                        pipeServer.WaitForPipeDrain();
                    }
                }
            }
            catch
            {
            }

            Notify("Out.Client disconnected.");
        }
    }
}