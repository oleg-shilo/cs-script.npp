using Intellisense.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltraSharp.Cecil;

namespace CSScriptIntellisense
{
    public class Syntaxer
    {
        public static string syntaxerDir;
        public static string cscsFile;
        static int port = 18001;
        static int timeout = 60000;
        static int procId = Process.GetCurrentProcess().Id;

        public static void StartServer()
        {
            Task.Factory.StartNew(() =>
            {
                HandeErrors(() =>
                    Process.Start(syntaxerDir.PathJoin("syntaxer.exe"),
                                  $"-listen -port:{port} -timeout:{timeout} \"-cscs_path:{cscsFile}\""));

                for (int i = 0; i < 25; i++)
                {
                    var response = Send($"-client:{procId}");
                    if (response != null)
                        break;
                    else
                        Thread.Sleep(1000);
                }
            });
        }

        public static void Exit()
        {
            Send("-exit");
        }

        ////////////////////////////////////////////////

        public static IEnumerable<ICompletionData> SendCompletionRequest(string editorText, string file, int location)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, location, "completion")).ToCompletionData();
        }

        public static string[] SendFindReferencesRequest(string editorText, string file, int location)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, location, "references")).ToReferences();
        }

        public static CodeMapItem[] SendMapOfRequest(string editorText, string file)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, "codemap")).ToCodeMapItems();
        }

        public static DomRegion SendResolveRequest(string editorText, string file, int location)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, location, "resolve")).ToDomRegion();
        }

        ////////////////////////////////////////////////

        static string SendSyntaxCommand(string file, string operation, params string[] extraArgs)
        {
            if (extraArgs.Any())
                return SendCommand($"-client:{procId}\n-op:{operation}\n-script:{file}\n-rich\n" + string.Join("\n", extraArgs));
            else
                return SendCommand($"-client:{procId}\n-op:{operation}\n-script:{file}\n-rich");
        }

        static string SendSyntaxCommand(string file, int location, string operation)
        {
            return SendCommand($"-client:{procId}\n-op:{operation}\n-script:{file}\n-pos:{location}\n-rich");
        }

        static string SendCommand(string command)
        {
            var response = Send(command);
            if (response == null) StartServer();
            return response;
        }

        static string Send(string command)
        {
            try
            {
                using (var clientSocket = new TcpClient())
                {
                    clientSocket.Connect(IPAddress.Loopback, port);
                    clientSocket.WriteAllText(command);
                    return clientSocket.ReadAllText();
                };
            }
            catch { }
            return null;
        }

        public static void HandeErrors(Action action, string logContext = null)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (logContext != null)
                    Logger.Debug(logContext + $": {e}");
            }
        }
    }

    public static class SyntaxerParser
    {
        public static string WithTempCopy(this string editorText, string permanentFile, Func<string, string> action)
        {
            var originalName = Path.GetFileName(permanentFile);
            var tempName = Path.ChangeExtension(originalName, ".$temp$" + Path.GetExtension(permanentFile));

            var tempFileName = Path.ChangeExtension(permanentFile, ".$temp$" + Path.GetExtension(permanentFile));
            try
            {
                File.WriteAllText(tempFileName, editorText);
                string response = action(tempFileName);

                if (!string.IsNullOrEmpty(response))
                    response = response.Replace(tempName, originalName);

                return response == "<null>" ? null : response;
            }
            finally
            {
                try { if (File.Exists(tempFileName)) File.Delete(tempFileName); } catch { }
            }
        }

        public static CompletionType ToCompletionType(this string data)
        {
            if (!data.StartsWith("<error>"))
            {
                string value = data.Replace("event", "_event").Replace("namespace", "_namespace");

                if (!Enum.TryParse(value, out CompletionType type))
                    type = CompletionType.unresolved;

                return type;
            }
            return CompletionType.unresolved;
        }

        public static CodeMapItem[] ToCodeMapItems(this string data)
        {
            if (data != null && data.StartsWith("<error>"))
                try
                {
                    return data.GetLines()
                               .Select(CodeMapItem.Deserialize)
                               .ToArray();
                }
                catch { }
            return new CodeMapItem[0];
        }

        public static IEnumerable<ICompletionData> ToCompletionData(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                return data.GetLines()
                           .Select(x =>
                           {
                               var parts = x.Split(new[] { '|', '\t' });

                               return new Intellisense.Common.CompletionData
                               {
                                   DisplayText = parts[0],
                                   CompletionText = parts[2],
                                   CompletionType = parts[1].ToCompletionType(),
                               };
                           });

            return new List<ICompletionData>();
        }

        public static DomRegion ToDomRegion(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try { return DomRegion.Deserialize(data); }
                catch { }
            return DomRegion.Empty;
        }

        public static string[] ToReferences(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try { return data.Split('\n'); }
                catch { }
            return new string[0];
        }
    }
}