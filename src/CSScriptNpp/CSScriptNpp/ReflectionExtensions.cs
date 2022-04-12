using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CSScriptNpp
{
    public static class MiscExtensions
    {
        public static string Run(this string app, params string[] args)
        {
            var p = new Process();
            p.StartInfo.FileName = app;
            p.StartInfo.Arguments = string.Join(" ", args.Select(x => $"\"{x}\"").ToArray());
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            p.Start();
            return p.StandardOutput.ReadToEnd();
        }
    }

    public static class ReflectionExtensions
    {
        public static T To<T>(this object obj)
        {
            return (T)obj;
        }

        public static string ToUri(this string path)
        {
            if (path.StartsWith("http"))
                return path;
            else
                return new Uri(path).AbsoluteUri;
        }

        public static string PathJoin(this Environment.SpecialFolder folder, params object[] items)
        {
            return Path.Combine(new[] { Environment.GetFolderPath(folder) }.Concat(items.Select(x => x?.ToString())).ToArray());
        }

        public static string PathJoin(this string path, params string[] items)
        {
            return Path.Combine(new[] { path }.Concat(items).ToArray());
        }

        public static string GetDirName(this string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static object GetField(this object obj, string name, bool throwOnError = true)
        {
            //Note GetField(s) does not return base class fields like GetProperty does.
            //BindingFlags.FlattenHierarchy does not make any difference (Reflection bug).
            //Thus we have to process base classes explicitly.
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return field.GetValue(obj);
                type = type.BaseType;
            }

            if (throwOnError)
                throw new Exception("ReflectionExtensions: cannot find field " + name);

            return null;
        }

        public static void SetField(this object obj, string name, object value, bool throwOnError = true)
        {
            //Note GetField(s) does not return base class fields like GetProperty does.
            //BindingFlags.FlattenHierarchy does not make any difference (Reflection bug).
            //Thus we have to process base classes explicitly.
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }

            if (throwOnError)
                throw new Exception("ReflectionExtensions: cannot find field " + name);
        }

        public static object GetProp(this object obj, string name)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            return property.GetValue(obj, null);
        }

        public static void SetProp(this object obj, string name, object value)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            property.SetValue(obj, value, null);
        }

        public static object Call(this object obj, string name, params object[] args)
        {
            var method = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (method == null)
                throw new Exception("ReflectionExtensions: cannot find method " + name);
            return method.Invoke(obj, args);
        }

        public static void CallIfExists(this object obj, string name, params object[] args)
        {
            var method = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (method != null)
                method.Invoke(obj, args);
        }
    }
}

// later (cleaner) solution
static class Utils
{
    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

    static bool Is64Bit()
    {
        if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
            return true;
        else
            return false;
    }

    static bool Is32BitProcessOn64BitProcessor()
    {
        bool retVal;

        IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);

        return retVal;
    }

    public static bool Is64BitOperatingSystem()
    {
        return Environment.OSVersion.Is64Bit();
    }

    public static bool Is64Bit(this OperatingSystem os)
    {
        // note cannot use Environment.Is64BitOperatingSystem as it is not available on all .NET versions
        return Is64Bit() || Is32BitProcessOn64BitProcessor();
    }
}