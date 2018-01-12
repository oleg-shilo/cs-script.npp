// NPP plugin platform for .Net v0.93.96 by Kasper B. Graversen etc.
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Kbg.NppPluginNET.PluginInfrastructure;
using NppPlugin.DllExport;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Kbg.NppPluginNET
{
    /*
        ```
        <x86 ProgramFiles on x64 Windows>
        ─ Program Files (x86)
        └─ Notepad++
            └─ plugins
                ├─ MyPlugin
                |  └─ MyPlugin.dll <AnyCPU assembly>
                └─ MyPlugin.x86.dll <renamed x86 version of NppPlugin.Host.dll>

        ************************

        <x64 ProgramFiles on x64 Windows>
        ─ Program Files
        └─ Notepad++
            └─ plugins
                ├─ MyPlugin
                |  └─ MyPlugin.dll <AnyCPU assembly>
                └─ MyPlugin.x64.dll <renamed x64 version of NppPlugin.Host.dll>

        ```

        From deployment point of view MyPlugin.x86.dll (host) depends on MyPlugin.dll (plugin).
        From CLR point of view it's vise versa, MyPlugin.dll (plugin) depends on MyPlugin.x86.dll (host).

        Thus to solve this problem the above file structure is used to facilitate deployment dependencies.
        And at runtime this host assembly calls at startup the plugin's NppPluginBinder.bind method to
        solve the reversed CLR dependency.
     */

    public interface IUnmanagedExports
    {
        bool isUnicode();

        void setInfo(NppData notepadPlusData);

        IntPtr getFuncsArray(ref int nbF);

        uint messageProc(uint Message, IntPtr wParam, IntPtr lParam);

        IntPtr getName();

        void beNotified(IntPtr notifyCode);
    }

    class UnmanagedExports
    {
        static UnmanagedExports()
        {
            // Debug.Assert(false);
            PluginProxy.Init();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return PluginProxy.isUnicode();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            PluginProxy.setInfo(notepadPlusData);
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            return PluginProxy.getFuncsArray(ref nbF);
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return PluginProxy.messageProc(Message, wParam, lParam);
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            return PluginProxy.getName();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            PluginProxy.beNotified(notifyCode);
        }
    }
}