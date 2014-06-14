using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSScriptNpp
{
    class WaitableQueueDbg<T> : Queue<T>
    {
        public string Name { get; set; }

        AutoResetEvent itemAvailable = new AutoResetEvent(true);

        public new void Enqueue(T item)
        {
            lock (this)
            {
                base.Enqueue(item);
                if (item.ToString().Contains("invoke"))
                    Debug.WriteLine("Queue is about to signal (" + Count + ")");
                itemAvailable.Set();
                if (item.ToString().Contains("invoke"))
                    Debug.WriteLine("Queue is signaled (" + Count + ")");
            }
        }

        int ccc = 0;
        public T WaitItem()
        {
            ccc++;
            Debug.WriteLine("Start Waiting...");
            while (true)
            {
                lock (this)
                {
                    if (base.Count > 0)
                    {
                        if (ccc == 4)
                            Debug.WriteLine("!!!!!(" + Count + ")");
                        Debug.WriteLine("Dequeueing...(" + Count + ")");

                        return base.Dequeue();
                    }
                }

                if (Name == "Notifications") Debug.WriteLine("Waiting...(" + Count + ")");
                itemAvailable.WaitOne(1000);
                if (Name == "Notifications") Debug.WriteLine("Waiting is over ...(" + Count + ")");
            }
        }
    }

    class WaitableQueue<T> : Queue<T>
    {
        public string Name { get; set; }

        AutoResetEvent itemAvailable = new AutoResetEvent(true);

        public new void Enqueue(T item)
        {
            lock (this)
            {
                base.Enqueue(item);
                itemAvailable.Set();
            }
        }

        public T WaitItem()
        {
            while (true)
            {
                lock (this)
                {
                    if (base.Count > 0)
                    {
                        return base.Dequeue();
                    }
                }

                itemAvailable.WaitOne();
            }
        }
    }

    class MessageQueue
    {
        static WaitableQueue<string> commands = new WaitableQueue<string>();
        static WaitableQueue<string> notifications = new WaitableQueue<string>();
        static WaitableQueue<string> automationCommans = new WaitableQueue<string>();

        static public void AddCommand(string data)
        {
            if (data == "")
                Debug.Assert(false);

            if (data == "npp.exit")
                Debug.WriteLine("");
            commands.Enqueue(data);
        }


        static public void AddNotification(string data)
        {
            notifications.Enqueue(data);
        }

        static public void AddAutomationCommand(string data)
        {
            automationCommans.Enqueue(data);
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

        static public string WaitForAutomationCommand()
        {
            return automationCommans.WaitItem();
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

                        Debug.WriteLine(message);
                        MessageQueue.AddNotification(message);
                    }
                }
            }
            catch (IOException e)
            {
                Notify("ERROR: " + e.Message);
            }

            Notify("In.Client disconnected");
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
                //Notify("ERROR: " + e.Message);
            }

            Notify("Out.Client disconnected.");
        }

        public void PullAutomationCommands()
        {
            try
            {
                string name = "npp.css.ide.commands." + remoteProcessId;
                using (var pipeServer = new NamedPipeServerStream(name, PipeDirection.In))
                using (var streamReader = new StreamReader(pipeServer))
                {
                    pipeServer.WaitForConnection();
                    while (true)
                    {
                        string message = streamReader.ReadLine();
                        Debug.WriteLine(message);
                        MessageQueue.AddAutomationCommand(message);
                    }
                }
            }
            catch (IOException e)
            {
                Notify("ERROR: " + e.Message);
            }
        }
    }
}