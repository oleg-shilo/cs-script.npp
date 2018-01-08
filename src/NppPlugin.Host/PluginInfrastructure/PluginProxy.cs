using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppPlugin.DllExport;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Kbg.NppPluginNET
{
    static class PluginProxy
    {
        static IUnmanagedExports plugin;

        public static bool isUnicode()
        {
            return plugin.isUnicode();
        }

        public static void setInfo(NppData notepadPlusData)
        {
            plugin.setInfo(notepadPlusData);
        }

        public static IntPtr getFuncsArray(ref int nbF)
        {
            return plugin.getFuncsArray(ref nbF);
        }

        public static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return plugin.messageProc(Message, wParam, lParam);
        }

        public static IntPtr getName()
        {
            return plugin.getName();
        }

        public static void beNotified(IntPtr notifyCode)
        {
            plugin.beNotified(notifyCode);
        }

        public static void Init()
        {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string pluginName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(thisAssembly));

            string pluginPath = Path.Combine(Path.GetDirectoryName(thisAssembly), pluginName, pluginName + ".dll");

            Assembly pluginAssembly = Assembly.LoadFrom(pluginPath);

            // At this stage any call to pluginAssembly.GetTypes() will throw asm probing error.
            // That's why we need to bing host and plugin first, so plugin does not have to
            // resolve/probe NppPlugin.Host assembly.

            Type binder = pluginAssembly.GetType("NppPluginBinder");
            binder.GetMethod("bind").Invoke(null, new object[] { thisAssembly });

            Type exports = Assembly.LoadFrom(pluginPath)
                                   .GetTypes()
                                   .FirstOrDefault(t => t.GetInterface("IUnmanagedExports") != null);

            plugin = (IUnmanagedExports)Activator.CreateInstance(exports);
        }
    }
}