using csscript;
using CSScriptNpp;
using CSScriptNpp.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
            new FavoritesPanel().ShowDialog(); return;
            DebugExternal.ShowModal(); return;
            //new UpdateOptionsPanel("1.0.1.1").ShowDialog(); return;

            //var tt = typeof(List<int>).GetProperties();

            //var args = "dsfsd,df".Split(',');
            //var list = new List<string>(args);
            //var map = new Dictionary<int, int>()
            //{
            //    {1,3},
            //    {2,7}
            //};


            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            ////var panel = new AutoWatchPanel();
            ////panel.Test();

            ////Screen.FromPoint();

            //var dlg = new ShortcutBuilder();
            //dlg.Name = "test gtdrfgfds";
            //dlg.Shortcut = "Ctrl+Shift+Alt+F7";
            ////dlg.ShowDialog();
            ////return;

            //var panel = new DebugPanel();
            //panel.UpdateCallstack("+1|Script.cs.compiled!Script.Main(string[] args) Line 13|{$NL}+2|[External Code]|{$NL}");
            //panel.ShowDialog();

            //Application.Run(new Form1());
        }


    }
}
