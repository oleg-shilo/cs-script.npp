using CSScriptNpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testpad
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var panel = new DebugPanel();
            panel.UpdateCallstack("Script.cs.compiled!Script.Main(string[] args) Line 13|{$NL}[External Code]|{$NL}");
            panel.ShowDialog();
            //Application.Run(new Form1());
        }
    }
}
