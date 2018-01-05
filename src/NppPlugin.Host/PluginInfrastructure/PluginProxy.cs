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
        static object[] _empty = new object[0];
        static MethodInfo _isUnicode;
        static MethodInfo _setInfo;
        static MethodInfo _getFuncsArray;
        static MethodInfo _messageProc;
        static MethodInfo _getName;
        static MethodInfo _beNotified;

        public static bool isUnicode() => (bool)_isUnicode.Invoke(null, _empty);

        public static void setInfo(NppData notepadPlusData) => _setInfo.Invoke(null, new object[] { notepadPlusData });

        public static IntPtr getFuncsArray(ref int nbF)
        {
            var args = new object[] { nbF };
            var result = (IntPtr)_getFuncsArray.Invoke(null, args);
            nbF = (int)args[0];
            return result;
        }

        public static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam) => (uint)_messageProc.Invoke(null, new object[] { Message, wParam, lParam });

        public static IntPtr getName() => (IntPtr)_getName.Invoke(null, _empty);

        public static void beNotified(IntPtr notifyCode) => _beNotified.Invoke(null, new object[] { notifyCode });

        public static void Init()
        {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string pluginName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(thisAssembly));

            string pluginPath = Path.Combine(Path.GetDirectoryName(thisAssembly), pluginName, pluginName + ".dll");

            Type exports = Assembly.LoadFrom(pluginPath)
                                   .GetTypes()
                                   .FirstOrDefault(t => t.Name == "UnmanagedExports");

            exports.GetMethod("bind").Invoke(null, new object[] { thisAssembly }); // call so plugin does not have to resolve/probe NppPlugin.Host assembly

            _isUnicode = exports.GetMethod("isUnicode");
            _setInfo = exports.GetMethod("setInfo");
            _getFuncsArray = exports.GetMethod("getFuncsArray");
            _messageProc = exports.GetMethod("messageProc");
            _getName = exports.GetMethod("getName");
            _beNotified = exports.GetMethod("beNotified");
        }
    }
}