using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class Logger
{
    //very expensive performance wise so call it only for errors
    public static void LogAsError(this object error)
    {
        var method = new StackFrame(1).GetMethod();
        string caller = method.DeclaringType.ToString() + "." + method.Name;
        PluginLogger.Error(caller + "|" + error ?? "<null>");
    }

    public static void LogAsDebug(this object message)
    {
        var method = new StackFrame(1).GetMethod();
        string caller = method.DeclaringType.ToString() + "." + method.Name;
        PluginLogger.Debug(caller + "|" + message ?? "<null>");
    }
}

public static class PluginLogger
{
    static public string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Notepad++\plugins\logs\CSScriptNpp");
    static public string logFile = Path.Combine(logDir, "app.log");
    static public int maxSize = 1024 * 100;

    public static void Error(object message)
    {
        Log("Error", message);
    }

    public static void Debug(object message)
    {
        Log("Debug", message);
    }

    static void Log(string type, object msg)
    {
        string message = (msg ?? "").ToString();
        lock (typeof(PluginLogger))
        {
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                if (File.Exists(logFile) && new FileInfo(logFile).Length > maxSize)
                {
                    File.Copy(logFile, Path.ChangeExtension(logFile, ".bak.log"), true);
                    File.Delete(logFile);
                }

                var logData = string.Format("{0:s}|{1:00000000}|{2}|{3}", DateTime.Now, Process.GetCurrentProcess().Id, type, message);

                for (int i = 0; i < 3; i++) //very primitive locking failure handling
                    try { File.AppendAllLines(logFile, new[] { logData }); break; }
                    catch { }
            }
            catch { }
        }
    }
}
