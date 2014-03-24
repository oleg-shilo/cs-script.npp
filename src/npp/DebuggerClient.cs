using gui.CSScriptNpp;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Tools.Mdbg;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace gui
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

            return (command != NppCommand.Exit);
        }

        public void Run(string app, string args, params string[] breaksPoints)
        {
            ExecuteCommand(string.Format("mo nc on\nrun \"{0}\" {1}", app, args));
        }

        string[] proceedingCommands = "go,next,step,out".Split(',');

        public void ExecuteCommand(string command)
        {
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
            else if (command.StartsWith(NppCategory.Invoke)) //not native Mdbg command
            {
                ProcessInvoke(command.Substring(NppCategory.Invoke.Length));
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

                    ReportsCurrentState();
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

        public void ProcessInvoke(string command)
        {
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
                            {
                                result = Serialize(value, args);
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
                                               new XAttribute("name", displayName??name),
                                               new XAttribute("id", valueId),
                                               new XAttribute("isProperty", val.IsProperty),
                                               new XAttribute("isStatic", val.IsStatic),
                                               new XAttribute("typeName", val.TypeName));

                try
                {
                    string st = null;
                    if (val.IsArrayType)
                    {
                        // It would be nice to display array length here too.
                        // Add a "dummy" sub-node to signify that this node is expandable. We then trap
                        // the BeforeExpand event to add the real children.
                        result.Add(new XAttribute("isComplex", true),
                                   new XAttribute("isArray", true));

                        //string.Format("{0}:{1}  (type='{2}') array:{$MORE}", valueId, reportedValuesCount, name, val.TypeName);
                    }
                    else if (val.IsComplexType)
                    {
                        // This will include both instance and static fields
                        // It will also include all base class fields.
                        result.Add(new XAttribute("isComplex", true),
                                   new XAttribute("isArray", false));
                        //string.Format("{0}:{1}  (type='{2}') fields:{$MORE}", valueId, name, val.TypeName);
                    }
                    else
                    {
                        // This is a catch-all for primitives.
                        string stValue = val.GetStringValue(false);
                        result.Add(new XAttribute("isComplex", false),
                                   new XAttribute("isArray", false),
                                   new XAttribute("value", stValue));
                        //result = string.Format("{0}:{1}  (type='{2}') value={3}", valueId, name, val.TypeName, stValue);
                    }
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    result.Add(new XAttribute("isComplex", false),
                                   new XAttribute("isArray", false),
                                   new XAttribute("value", "<unavailable>"));
                    //result = string.Format("{0}:{1}=<unavailable>", valueId, name);
                }

                return result.ToString();
            }
        }

        public void Break(bool reportPosition)
        {
            shell.Debugger.Processes.Active.AsyncStop().WaitOne();
            if (reportPosition)
                ReportsCurrentState();
        }

        MDbgFrame GetCurrentFrame()
        {
            try
            {
                return shell.Debugger.Processes.Active.Threads.Active.CurrentFrame;
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

            //foreach (var framePair in GetCallStackList(shell.Debugger.Processes.Active.Threads.Active))
            //    if (framePair.m_frame.SourcePosition != null && process.Threads.Active.CurrentFrame == framePair.m_frame)
            //        return FormatSourcePosition(framePair.m_frame);

            if (!process.Threads.Active.CurrentFrame.IsInfoOnly)
                return FormatSourcePosition(process.Threads.Active.CurrentFrame);
            else
                return null;
        }

        string FormatSourcePosition(MDbgFrame frame)
        {
            return String.Format("{0}|{1}:{2}|{3}:{4}",
                frame.SourcePosition.Path,
                frame.SourcePosition.StartLine,
                frame.SourcePosition.StartColumn,
                frame.SourcePosition.EndLine,
                frame.SourcePosition.EndColumn);
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

        void ReportsCurrentState()
        {
            reportedValues.Clear();

            ReportSourceCodePosition();
            ReportCallStack();
            ReportLocals();
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
                    shell.Debugger.Processes.Active.PostDebugEvent += Active_PostDebugEvent;
                    lastActiveprocessId = shell.Debugger.Processes.Active.CorProcess.Id;
                    MessageQueue.AddNotification(NppCategory.Process + lastActiveprocessId + ":STARTED");
                    lastActiveprocess = shell.Debugger.Processes.Active;
                }
            }
        }

        //bool breakOnStepComplete = false;
        bool test = true;

        void Active_PostDebugEvent(object sender, CustomPostCallbackEventArgs e)
        {
            if (test)
            {
                test = false;
                //Debug.Assert(false);
            }

            if (e.CallbackType == ManagedCallbackType.OnProcessExit)
                ReportDebugTermination();

            if (e.CallbackType == ManagedCallbackType.OnBreakpoint || e.CallbackType == ManagedCallbackType.OnBreak || e.CallbackType == ManagedCallbackType.OnStepComplete)
            {
                ReportsCurrentState();
            }

            if (e.CallbackType == ManagedCallbackType.OnLogMessage)
            {
                var args = e.CallbackArgs as CorLogMessageEventArgs;
                ReportLogMessage(args.Message);
            }

            WriteOutput("meta=>", e.CallbackType.ToString());
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

    static class Utils
    {
        static public string ReplaceClrAliaces(this string text, bool hideSystemNamespace = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            else
            {
                var retval = text.ReplaceWholeWord("System.Object", "object")
                                 .ReplaceWholeWord("System.Boolean", "bool")
                                 .ReplaceWholeWord("System.Byte", "byte")
                                 .ReplaceWholeWord("System.SByte", "sbyte")
                                 .ReplaceWholeWord("System.Char", "char")
                                 .ReplaceWholeWord("System.Decimal", "decimal")
                                 .ReplaceWholeWord("System.Double", "double")
                                 .ReplaceWholeWord("System.Single", "float")
                                 .ReplaceWholeWord("System.Int32", "int")
                                 .ReplaceWholeWord("System.UInt32", "uint")
                                 .ReplaceWholeWord("System.Int64", "long")
                                 .ReplaceWholeWord("System.UInt64", "ulong")
                                 .ReplaceWholeWord("System.Object", "object")
                                 .ReplaceWholeWord("System.Int16", "short")
                                 .ReplaceWholeWord("System.UInt16", "ushort")
                                 .ReplaceWholeWord("System.String", "string")
                                 .ReplaceWholeWord("System.Void", "void")
                                 .ReplaceWholeWord("Void", "void");
                if (hideSystemNamespace && retval.StartsWith("System."))
                {
                    string typeName = retval.Substring("System.".Length);
                    if (!typeName.Contains('.')) // it is not a complex namespace
                        retval = typeName;
                }

                return retval;
            }
        }

        static public string ReplaceWholeWord(this string text, string pattern, string replacement)
        {
            return Regex.Replace(text, @"\b(" + pattern + @")\b", replacement);
        }
    }
}