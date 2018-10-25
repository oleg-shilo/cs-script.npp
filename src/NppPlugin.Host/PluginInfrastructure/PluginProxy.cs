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

        static string ProbeFile(params string[] paths)
        {
            var file = Path.Combine(paths);
            if (File.Exists(file))
                return file;
            else
                return null;
        }

        public static void Init()
        {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string pluginName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(thisAssembly));

            string baseDir = Path.GetDirectoryName(thisAssembly);

            string pluginPath = ProbeFile(baseDir, pluginName + ".asm.dll") ??
                                ProbeFile(baseDir, pluginName, pluginName + ".dll") ??
                                ProbeFile(baseDir, pluginName, pluginName + ".asm.dll");

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