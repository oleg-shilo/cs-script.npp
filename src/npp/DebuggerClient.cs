using npp.CSScriptNpp;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Tools.Mdbg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text;

namespace npp
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

    public class DebuggerClient : IMDbgIO
    {
        WaitableQueue<string> mdbgInputQueue = new WaitableQueue<string>();

        IMDbgShell shell;

        RemoteChannelClient channel = new RemoteChannelClient();

        public DebuggerClient(IMDbgShell shell)
        {
            shell.OnStepExecitionFailure = step =>
                {
                    Task.Factory.StartNew(() =>
                        {
                            //step has failed usually it loses the source code location so trigger it
                            //by breaking the already stopped applications.
                            Thread.Sleep(700);
                            ResetAndReport();
                        });
                };

            this.shell = shell;
            channel.Trace = message => Console.WriteLine(message);
            channel.Start();

            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        string command = MessageQueue.WaitForCommand();
                        ExecuteCommand(command);
                    }
                });
        }

        void ResetAndReport()
        {
            try { shell.Debugger.Processes.Active.Go().WaitOne(); }
            catch { }
            try { shell.Debugger.Processes.Active.AsyncStop().WaitOne(); }
            catch { }
            ReportSourceCodePosition();
        }

        void BreakAndReport()
        {
            try { shell.Debugger.Processes.Active.AsyncStop().WaitOne(); }
            catch { }
            ReportSourceCodePosition();
        }

        public void Close()
        {
            ExecuteCommand("npp.exit");
        }

        public void WriteOutput(string outputType, string output)
        {
            Console.WriteLine("{0}:{1}", outputType, output);
        }

        MDbgProcess lastActiveprocess;

        public bool ReadCommand(out string command)
        {
            AnalyseExecutionPosition();

            command = mdbgInputQueue.WaitItem();

            WaitForEvalsCoTomplete();

            return (command != NppCommand.Exit);
        }

        public void Run(string app, string args, params string[] breaksPoints)
        {
            ExecuteCommand(string.Format("mo nc on\nrun \"{0}\" {1}", app, args));
        }

        string[] proceedingCommands = "go,next,step,out".Split(',');

        public void ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            if (command == "break") //not native Mdbg command
            {
                Break(true);
            }
            else if (command.StartsWith("breakpoint")) //not native Mdbg command
            {
                ProcessBreakpoint(command);
            }
            else if (command.StartsWith("gotoframe")) //not native Mdbg command
            {
                ProcessFrameNavigation(command);
            }
            else if (command.StartsWith("gotothread")) //not native Mdbg command
            {
                ProcessThreadSwitch(command);
            }
            else if (command.StartsWith(NppCategory.Invoke)) //not native Mdbg command
            {
                ProcessInvoke(command.Substring(NppCategory.Invoke.Length));
            }
            else if (command.StartsWith(NppCategory.Settings)) //not native Mdbg command
            {
                ProcessSettings(command.Substring(NppCategory.Settings.Length));
            }
            else if (command == NppCommand.Exit)
            {
                Exit();
            }
            else
            {
                if (proceedingCommands.Contains(command))
                {
                    if (!CanProceed())
                        return;
                }
                mdbgInputQueue.Enqueue(command);
            }
        }

        public void ProcessFrameNavigation(string command)
        {
            //gotoframe|<frameId>
            string[] parts = command.Split('|');
            if (parts[0] == "gotoframe")
            {
                string id = parts[1];
                {
                    try
                    {
                        var frame = GetFrameByIndex(shell.Debugger.Processes.Active.Threads.Active, int.Parse(id));
                        shell.Debugger.Processes.Active.Threads.Active.CurrentFrame = frame;
                    }
                    catch (InvalidOperationException)
                    {
                        // if it throws an invalid op, then that means our frames somehow got out of sync and we weren't fully refreshed.
                        return;
                    }

                    ReportCurrentState();
                }
            }
        }

        public void ProcessThreadSwitch(string command)
        {
            //gotothread|<threadId>
            string[] parts = command.Split('|');
            if (parts[0] == "gotothread" && IsInBreakMode())
            {
                string id = parts[1];

                MDbgThread match = null;
                try
                {
                    foreach (MDbgThread t in shell.Debugger.Processes.Active.Threads)
                    {
                        if (t.Id.ToString() == id)
                        {
                            match = t;
                            break;
                        }
                    }
                }
                catch
                {
                    // if it throws an invalid op, then that means our frames somehow got out of sync and we weren't fully refreshed.
                    return;
                }

                if (match != null)
                {
                    shell.Debugger.Processes.Active.Threads.Active = match;
                    ReportCurrentState();
                }
            }
        }

        public void ProcessBreakpoint(string command)
        {
            //<breakpoint-|breakpoint+><file>|<linenumber>
            string[] parts = command.Split('|');
            if (parts[0] == "breakpoint+")
            {
                CreateBreakpoint(parts[1], int.Parse(parts[2]));
            }
            else if (parts[0] == "breakpoint-")
            {
                RemoveBreakpoint(parts[1], int.Parse(parts[2]));
            }
        }

        public void ProcessSettings(string command)
        {
            //<name>=<value>[|<name>=<value>]
            if (command.Contains("breakonexception=true"))
                breakOnException = true;
            else if (command.Contains("breakonexception=false"))
                breakOnException = false;

        }
        public void ProcessInvoke(string command)
        {
            if (test)
            {
                //test = false;
                //Debug.Assert(false);
            }

            //<invokeId>:<action>:<args>
            string[] parts = command.Split(new[] { ':' }, 3);
            string id = parts[0];
            string action = parts[1];
            string args = parts[2];
            string result = "";

            try
            {
                if (IsInBreakMode())
                {
                    if (action == "locals")
                    {
                        if (reportedValues.ContainsKey(args))
                        {
                            MDbgValue value = reportedValues[args];
                            MDbgValue[] items = null;

                            if (value.IsArrayType)
                            {
                                items = value.GetArrayItems();
                            }
                            else if (value.IsComplexType)
                            {
                                items = value.GetFields().Concat(
                                        value.GetProperties()).ToArray();
                            }

                            if (items != null)
                                result = "<items>" + string.Join("", items.Where(x => !x.Name.Contains("$")) //ignore any internal vars
                                                                          .Select(x => Serialize(x))
                                                                          .ToArray()) + "</items>";
                        }
                    }
                    else if (action == "resolve_primitive")
                    {
                        try
                        {
                            MDbgValue value = shell.Debugger.Processes.Active.ResolveVariable(args, shell.Debugger.Processes.Active.Threads.Active.CurrentFrame);

                            if (value != null && !value.IsArrayType && !value.IsComplexType)
                                result = Serialize(value, args);
                        }
                        catch
                        {
                            result = null;
                        }
                    }
                    else if (action == "resolve")
                    {
                        try
                        {
                            MDbgValue value = shell.Debugger.Processes.Active.ResolveVariable(args, shell.Debugger.Processes.Active.Threads.Active.CurrentFrame);

                            if (value != null)
                            {
                                result = "<items>" + Serialize(value, args) + "</items>";

                            }
                        }
                        catch
                        {
                            result = "<items/>";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result = "Error: " + e.Message;
            }
            MessageQueue.AddNotification(NppCategory.Invoke + id + ":" + result);
        }

        Dictionary<string, MDbgValue> reportedValues = new Dictionary<string, MDbgValue>();
        int reportedValuesCount = 0;

        string Serialize(MDbgValue val, string displayName = null)
        {
            lock (reportedValues)
            {
                int valueId = reportedValuesCount++;
                reportedValues.Add(valueId.ToString(), val);

                string name = val.Name;

                XElement result = new XElement("value",
                                               new XAttribute("name", displayName ?? name),
                                               new XAttribute("id", valueId),
                                               new XAttribute("isProperty", val.IsProperty),
                                               new XAttribute("isStatic", val.IsStatic),
                                               new XAttribute("typeName", val.TypeName));

                try
                {
                    if (val.IsArrayType)
                    {
                        // It would be nice to display array length here too.
                        // Add a "dummy" sub-node to signify that this node is expandable. We then trap
                        // the BeforeExpand event to add the real children.
                        result.Add(new XAttribute("isComplex", true),
                                   new XAttribute("isArray", true));
                    }
                    else if (val.IsComplexType)
                    {
                        // This will include both instance and static fields
                        // It will also include all base class fields.
                        result.Add(new XAttribute("isComplex", true),
                                   new XAttribute("isArray", false));
                    }
                    else
                    {
                        // This is a catch-all for primitives.
                        string stValue = val.GetStringValue(false);
                        result.Add(new XAttribute("isComplex", false),
                                   new XAttribute("isArray", false),
                                   new XAttribute("value", stValue));
                    }
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    result.Add(new XAttribute("isComplex", false),
                                   new XAttribute("isArray", false),
                                   new XAttribute("value", "<unavailable>"));
                }

                return result.ToString();
            }
        }

        public void Break(bool reportPosition)
        {
            shell.Debugger.Processes.Active.AsyncStop().WaitOne();

            if (reportPosition)
                ReportCurrentState();
        }

        MDbgFrame GetCurrentFrame()
        {
            try
            {
                return GetCurrentThread().CurrentFrame;
            }
            catch { }
            return null;
        }

        MDbgThread GetCurrentThread()
        {
            try
            {
                return shell.Debugger.Processes.Active.Threads.Active;
            }
            catch { }
            return null;
        }

        //alive and in the break point mode
        bool CanProceed()
        {
            return IsInBreakMode();
        }

        bool IsInBreakMode()
        {
            try
            {
                if (shell.Debugger.Processes.HaveActive)
                {
                    return !shell.Debugger.Processes.Active.IsRunning && shell.Debugger.Processes.Active.IsAlive;
                }
            }
            catch { }
            return false;
        }

        public void Exit()
        {
            mdbgInputQueue.Enqueue("exit");
            if (shell.Debugger.Processes.HaveActive)
                shell.Debugger.Processes.Active.Kill();
            Application.Exit();
        }

        public void StepOver()
        {
            if (CanProceed())
                ExecuteCommand("next");
        }

        public void Go()
        {
            if (CanProceed())
                ExecuteCommand("go");
        }

        public void StepIn()
        {
            if (CanProceed())
                ExecuteCommand("step");
        }

        public void SetInstructionPointer(int line)
        {
            if (CanProceed())
                ExecuteCommand("setip " + line);
        }

        public void StepOut()
        {
            if (CanProceed())
                MessageQueue.AddCommand("out");
        }

        public void Test()
        {
            MessageQueue.AddNotification("test");
        }

        public bool CreateBreakpoint(string file, int line)
        {
            if (shell.Debugger.Processes.HaveActive)
            {
                if (shell.Debugger.Processes.Active.IsRunning)
                {
                    //need to stop as breakpoint can be set only in BREAK mode
                    Break(false);
                    shell.Debugger.Processes.Active.Breakpoints.CreateBreakpoint(file, line);
                    Go();
                }
                else
                {
                    shell.Debugger.Processes.Active.Breakpoints.CreateBreakpoint(file, line);
                }
                return true;
            }
            return false;
        }

        public bool RemoveBreakpoint(string file, int line)
        {
            if (shell.Debugger.Processes.HaveActive)
            {
                foreach (MDbgBreakpoint item in shell.Debugger.Processes.Active.Breakpoints)
                {
                    var location = (BreakpointLineNumberLocation)item.Location;
                    if (location.FileName == file && location.LineNumber == line)
                    {
                        item.Delete();
                        return true;
                    }
                }
            }
            return false;
        }

        string GetCurrentSourcePosition()
        {
            if (!shell.Debugger.Processes.HaveActive)
                return null;

            MDbgProcess process = shell.Debugger.Processes.Active;

            if (process.IsRunning)
                return null;

            if (!process.Threads.Active.HaveCurrentFrame)
                return null; //No frame for current thread #" + thread.Number);

            if (!process.Threads.Active.CurrentFrame.IsInfoOnly)
                return FormatSourcePosition(process.Threads.Active.CurrentFrame);
            else
                return null;
        }

        string FormatSourcePosition(MDbgFrame frame)
        {
            var f = frame;
            while (f != null && f.SourcePosition == null)
            {
                f = f.NextUp;
            }

            if (f == null || f.SourcePosition == null)
                return null;
            else
                return String.Format("{0}|{1}:{2}|{3}:{4}",
                    f.SourcePosition.Path,
                    f.SourcePosition.StartLine,
                    f.SourcePosition.StartColumn,
                    f.SourcePosition.EndLine,
                    f.SourcePosition.EndColumn);
        }

        string FormatCallInfo(MDbgFrame frame)
        {
            var args = string.Join(", ", frame.Function.GetArguments(frame).Select(a => a.TypeName.ReplaceClrAliaces() + " " + a.Name).ToArray());

            return string.Format("{0}!{1}({2}) Line {3}",
                                  Path.GetFileName(frame.Function.Module.CorModule.Assembly.Name),
                                  frame.Function.FullName,
                                  args,
                                  frame.SourcePosition.StartLine);
        }

        void ReportSourceCodePosition()
        {
            string position = GetCurrentSourcePosition();

            if (position != null)
            {
                MessageQueue.AddNotification(NppCategory.SourceCode + position);
            }
        }

        void ReportCurrentState()
        {
            reportedValues.Clear();

            ReportSourceCodePosition();
            ReportCallStack();
            ReportLocals();
            ReportThreads();
        }

        void ReportLogMessage(string message)
        {
            MessageQueue.AddNotification(NppCategory.Trace + message);
        }

        const string lineDelimiter = "{$NL}";

        void ReportCallStack()
        {
            int frameIndex = 0;
            var activeFrame = shell.Debugger.Processes.Active.Threads.Active.HaveCurrentFrame ? shell.Debugger.Processes.Active.Threads.Active.CurrentFrame : null;

            var result = new List<string>();
            foreach (FramePair frame in GetCallStackList(shell.Debugger.Processes.Active.Threads.Active))
            {
                string isActive = (frame.m_frame == activeFrame) ? "+" : "-";

                if (frame.m_frame.SourcePosition != null)
                {
                    string callInfo = FormatCallInfo(frame.m_frame);
                    string sourceRef = "";
                    sourceRef = FormatSourcePosition(frame.m_frame);
                    result.Add(string.Format("{0}{1}|{2}|{3}{4}", isActive, frameIndex, callInfo, sourceRef, lineDelimiter));
                }
                else if (frame.m_frame.IsInfoOnly)
                {
                    result.Add(string.Format("{0}{1}|{2}|{3}{4}", isActive, frameIndex, "[External Code]", "", lineDelimiter));
                }

                frameIndex++;
            }

            if (result.Any())
            {
                string data = string.Join("", result.ToArray());
                MessageQueue.AddNotification(NppCategory.CallStack + data);
            }
        }


        void ReportThreads()
        {
            if (IsInBreakMode())
            {
                MDbgThread tActive = GetCurrentThread();

                var threadsInfo = new StringBuilder();

                foreach (MDbgThread t in shell.Debugger.Processes.Active.Threads)
                {
                    string stFrame = "<unknown>";

                    try
                    {
                        if (t.BottomFrame != null)
                            stFrame = t.BottomFrame.Function.FullName;
                    }
                    catch { }//t.BottomFrame can throw

                    threadsInfo.AppendFormat("<thread id=\"{0}\" name=\"{1}\" active=\"{2}\" number=\"{3}\" />",
                                             t.Id,
                                             stFrame.Replace("<", "$&lt;").Replace(">", "&gt;"),
                                             (t == tActive),
                                             t.Number,
                                             stFrame);
                }

                MessageQueue.AddNotification(NppCategory.Threads + "<threads>" + threadsInfo.ToString() + "</threads>");
            }
        }

        void ReportLocals()
        {
            var result = "";

            try
            {
                if (IsInBreakMode())
                {
                    MDbgFrame frame = GetCurrentFrame();
                    MDbgFunction f = frame.Function;
                    MDbgValue[] locals = f.GetActiveLocalVars(frame);
                    MDbgValue[] arguments = f.GetArguments(frame);

                    result = "<locals>" + string.Join("", arguments.Concat(locals)
                                                                   .Where(x => !x.Name.Contains("$")) //ignore any internal vars
                                                                   .Select(x => Serialize(x))
                                                                   .ToArray()) + "</locals>";
                }
            }
            catch (Exception e)
            {
            }

            MessageQueue.AddNotification(NppCategory.Locals + result);
        }

        void ReportDebugTermination()
        {
            if (lastActiveprocess != null)
            {
                MessageQueue.AddNotification(NppCategory.Process + lastActiveprocessId + ":STOPPED");
                MessageQueue.AddCommand(NppCommand.Exit); //to close all communication channels

                lastActiveprocess = null;
                lastActiveprocessId = 0;
            }
        }

        int lastActiveprocessId;

        public void AnalyseExecutionPosition()
        {
            if (!shell.Debugger.Processes.HaveActive)
            {
                ReportDebugTermination();
                return; // don't try to display current location
            }
            else
            {
                if (lastActiveprocess == null)
                {
                    shell.Debugger.Processes.Active.PostDebugEvent += OnPostDebugEvent;
                    lastActiveprocessId = shell.Debugger.Processes.Active.CorProcess.Id;
                    MessageQueue.AddNotification(NppCategory.Process + lastActiveprocessId + ":STARTED");
                    lastActiveprocess = shell.Debugger.Processes.Active;
                }
            }
        }

        bool test = true;

        bool breakOnException = true;

        void OnPostDebugEvent(object sender, CustomPostCallbackEventArgs e)
        {
            //if (test)
            //{
            //    test = false;
            //    Debug.Assert(false);
            //}

            if (e.CallbackType == ManagedCallbackType.OnProcessExit)
                ReportDebugTermination();

            if (e.CallbackType == ManagedCallbackType.OnBreakpoint || e.CallbackType == ManagedCallbackType.OnBreak || e.CallbackType == ManagedCallbackType.OnStepComplete)
            {
                if (GetCurrentSourcePosition() == null)
                    MessageQueue.AddNotification(NppCategory.State + "NOSOURCEBREAK"); //can be caused by 'Debugger.Break();'

                ReportCurrentState();
            }

            if (e.CallbackType == ManagedCallbackType.OnLogMessage)
            {
                var args = e.CallbackArgs as CorLogMessageEventArgs;
                ReportLogMessage(args.Message);
            }

            if (breakOnException && e.CallbackType == ManagedCallbackType.OnException)
            {
                string position = GetCurrentSourcePosition();

                CorExceptionEventArgs args = (CorExceptionEventArgs)e.CallbackArgs;

                string info = SerializeException(args.Thread.CurrentException).Replace("\r\n", "\n").Replace("\n", "{$NL}");

                if (position != null)
                {
                    if (!position.Contains("css_dbg.cs|")) //If it is a script launcher then just ignore it.
                    {
                        bool isDifferentThread = SwitchToThread(args.Thread);
                        if (!isDifferentThread)
                        {
                            BreakAndReport();
                            MessageQueue.AddNotification(NppCategory.Exception + "user+:User code Exception.{$NL}Location: " + position + "{$NL}" + info);
                        }
                    }
                }
                else
                {
                    MessageQueue.AddNotification(NppCategory.Exception + "user-:Non-user code Exception." + "{$NL}" + info);
                    BreakAndReport();
                }
            }

            WriteOutput("meta=>", e.CallbackType.ToString());
        }

        bool SwitchToThread(CorThread thread)
        {
            try
            {
                foreach (MDbgThread t in shell.Debugger.Processes.Active.Threads)
                {
                    if (t.Id == thread.Id)
                    {
                        if (shell.Debugger.Processes.Active.Threads.Active != t)
                        {
                            shell.Debugger.Processes.Active.Threads.Active = t;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // if it throws an invalid op, then that means our frames somehow got out of sync and we weren't fully refreshed.
            }
            return false;
        }

        string SerializeException(CorValue exception)
        {
            var result = new StringBuilder();
            MDbgValue ex = new MDbgValue(shell.Debugger.Processes.Active, exception);

            result.Append(ex.GetStringValue(0) + ": ");
            foreach (MDbgValue f in ex.GetFields())
            {
                string outputValue = f.GetStringValue(0);

                if (f.Name == "_message")
                {
                    result.AppendLine(outputValue);
                    break;

                }

                //if (f.Name == "_xptrs" || f.Name == "_xcode" || f.Name == "_stackTrace" ||
                //    f.Name == "_remoteStackTraceString" || f.Name == "_remoteStackIndex" ||
                //    f.Name == "_exceptionMethodString")
                //{
                //    continue;
                //}

                //// remove new line characters in string
                //if (outputValue != "<null>" && outputValue != null && (f.Name == "_exceptionMethodString" || f.Name == "_remoteStackTraceString"))
                //{
                //    outputValue = outputValue.Replace('\n', '#');
                //}

                //string name = f.Name.Substring(1,1).ToUpper()+f.Name.Substring(2);
                //result.AppendLine("\t" + name + "=" + outputValue);
            }
            return result.ToString();
        }

        bool EvalsInProgress = false;
        AutoResetEvent EvalsCompleted = new AutoResetEvent(false);

        void WaitForEvalsCoTomplete()
        {
            if (EvalsInProgress)
                EvalsCompleted.WaitOne();
        }

        bool IsSet(CorDebugUserState value, CorDebugUserState bitValue)
        {
            return (value | bitValue) == value;
        }

        bool IsEvalSafe()
        {
            //shell.Debugger.Processes.Active.AsyncStop().WaitOne();

            var proc = shell.Debugger.Processes.Active;
            ICorDebugThread dbgThred = proc.Threads.Active.CorThread.Raw;

            ///http://blogs.msdn.com/b/jmstall/archive/2005/11/15/funceval-rules.aspx
            ///Dangers of Eval: http://blogs.msdn.com/b/jmstall/archive/2005/03/23/400794.aspx
            CorDebugUserState pState;
            dbgThred.GetUserState(out pState);
            if ((!IsSet(pState, CorDebugUserState.USER_UNSAFE_POINT) && IsSet(pState, CorDebugUserState.USER_STOPPED)) || pState == CorDebugUserState.USER_NONE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void ReportWatch()
        {
            if (IsEvalSafe())
                try
                {
                    //the following code can completely derail (dead-lock) the the debugger. 
                    //Bad, bad Debugger!
                    //http://blogs.msdn.com/b/jmstall/archive/2005/11/15/funceval-rules.aspx
                    //Dngers of Eval: http://blogs.msdn.com/b/jmstall/archive/2005/03/23/400794.aspx

                    if (IsInBreakMode())
                    {
                        string result = "";
                        Console.WriteLine("-------------------");
                        string expressionValue = "";
                        try
                        {
                            string expression = "";

                            MDbgValue value = shell.Debugger.Processes.Active.ResolveVariable(expression, shell.Debugger.Processes.Active.Threads.Active.CurrentFrame);

                            if (value != null)
                            {
                                //expressionValue = "<items>" + Serialize(value, expression) + "</items>";
                                expressionValue = Serialize(value, expression);
                                result += expressionValue;
                            }
                            Console.WriteLine(expression + "|" + expressionValue);
                        }
                        catch
                        {
                            //expressionValue = "<items/>";
                        }
                        Console.WriteLine("-------------------");
                    }
                }
                catch (Exception e)
                {
                }
        }

        class FramePair
        {
            public FramePair(MDbgFrame f, String s)
            {
                m_frame = f;
                m_displayString = s;
            }

            public override string ToString()
            {
                return m_displayString;
            }

            internal MDbgFrame m_frame;
            String m_displayString;
        }

        static MDbgFrame GetFrameByIndex(MDbgThread thread, int index)
        {
            MDbgFrame f = thread.BottomFrame;

            int count = 0;
            int depth = 20;

            while (f != null && (depth == 0 || count < depth))
            {
                if (count == index)
                    return f;

                count++;
                f = f.NextUp;
            }

            return null;
        }

        static FramePair[] GetCallStackList(MDbgThread thread)
        {
            MDbgFrame f = thread.BottomFrame;
            MDbgFrame af = thread.HaveCurrentFrame ? thread.CurrentFrame : null;

            var l = new System.Collections.ArrayList();

            int i = 0;
            int depth = 20;
            bool verboseOutput = true;

            while (f != null && (depth == 0 || i < depth))
            {
                string line;
                if (f.IsInfoOnly)
                {
                    line = string.Format(CultureInfo.InvariantCulture, "[{0}]", f.ToString());
                }
                else
                {
                    string frameDescription = "<unknown>";
                    try
                    {
                        // Get IP info.
                        uint ipNative;
                        uint ipIL;
                        CorDebugMappingResult result;
                        f.CorFrame.GetNativeIP(out ipNative);
                        f.CorFrame.GetIP(out ipIL, out result);
                        string frameLocation = String.Format(CultureInfo.InvariantCulture, " N=0x{0:x}, IL=0x{1:x} ({2})",
                            ipNative, ipIL, result.ToString());

                        // This may actually do a ton of work, including evaluating parameters.
                        frameDescription = f.ToString(verboseOutput ? "v" : null) + frameLocation;
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        if (f.Function != null)
                        {
                            frameDescription = f.Function.FullName;
                        }
                    }

                    line = string.Format(CultureInfo.InvariantCulture, "{0}{1}. {2}", f.Equals(af) ? "*" : " ", i, frameDescription);
                    ++i;
                }
                l.Add(new FramePair(f, line));
                f = f.NextUp;
            }
            if (f != null && depth != 0) // means we still have some frames to show....
            {
                l.Add(new FramePair(null,
                    string.Format(CultureInfo.InvariantCulture, "displayed only first {0} frames. For more frames use -c switch", depth)
                    ));
            }

            return (FramePair[])l.ToArray(typeof(FramePair));
        }
    }

}