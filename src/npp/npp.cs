using npp;
using System;
using System.Windows.Forms;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]

namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    public abstract class NppExtension : CommandBase
    {
        static DebuggerClient nppDebugger;

        public static void LoadExtension()
        {
            try
            {
                MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands, typeof(NppExtension));
            }
            catch { }

            Gui("");
        }

        [CommandDescription(CommandName = "npp", ShortHelp = "npp [close] - starts/closes a npp interface", LongHelp = "Usage: npp [close]")]
        public static void Gui(string args)
        {
            //System.Diagnostics.Debug.Assert(false);

            if (nppDebugger == null)
                nppDebugger = new DebuggerClient(Shell);

            var ap = new ArgParser(args);
            if (ap.Exists(0))
            {
                if (ap.AsString(0) == "close")
                {
                    if (nppDebugger != null)
                    {
                        nppDebugger.Close();
                        Application.Exit(); // this line will cause the message pump on other thread to quit.
                        return;
                    }
                    else
                        throw new MDbgShellException("NPP not started.");
                }
                else
                    throw new MDbgShellException("invalid argument");
            }

            if (Shell.IO == nppDebugger)
            {
                WriteOutput("NPP already started. Cannot start second instance.");
                return;
            }

            WriteOutput("starting npp");

            Shell.IO = nppDebugger;
        }
    }
}