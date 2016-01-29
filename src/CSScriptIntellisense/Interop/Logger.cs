using System.Diagnostics;

public static class Logger
{
    //very expensive performance wise so call use it only for errors
    public static void LogAsError(this object error)
    {
        var method = new StackFrame(1).GetMethod();
        string caller = method.DeclaringType.ToString() + "." + method.Name;
        NLog.LogManager.GetLogger(method.DeclaringType.ToString()).Error(method.Name + "|" + error ?? "<null>");
    }

    public static void LogAsDebug(this object error)
    {
        var method = new StackFrame(1).GetMethod();
        string caller = method.DeclaringType.ToString() + "." + method.Name;
        NLog.LogManager.GetLogger(method.DeclaringType.ToString()).Debug(method.Name + "|" + error ?? "<null>");
    }

    public static void Error(object error)
    {
        var method = new StackFrame(1).GetMethod();
        string caller = method.DeclaringType.ToString() + "." + method.Name;
        NLog.LogManager.GetLogger(method.DeclaringType.ToString()).Error(method.Name + "|" + error ?? "<null>");
    }
}
