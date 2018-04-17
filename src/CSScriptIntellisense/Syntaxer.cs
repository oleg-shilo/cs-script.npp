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
    // Ports:
    // 18000 - Sublime Text 3
    // 18001 - Notepad++
    // 18002 - VSCode CodeMap

    public class Syntaxer
    {
        public static string syntaxerDir;
        public static string cscsFile;
        static int port = 18001;
        static int timeout = 60000;
        static int procId = Process.GetCurrentProcess().Id;

        public static void StartServer(bool onlyIfNotRunning)
        {
            Task.Factory.StartNew(() =>
            {
                if (onlyIfNotRunning && IsRunning())
                    return;

                HandeErrors(() =>
                {
                    // Debug.Assert(false);

                    bool hidden = true;
                    // hidden = false;
                    if (hidden)
                    {
                        var p = new Process();

                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                        p.StartInfo.FileName = syntaxerDir.PathJoin("syntaxer.exe");
                        p.StartInfo.Arguments = $"-listen -port:{port} -timeout:{timeout} \"-cscs_path:{cscsFile}\"";
                        p.Start();
                    }
                    else
                    {
                        Process.Start(syntaxerDir.PathJoin("syntaxer.exe"),
                                      $"-listen -port:{port} -timeout:{timeout} \"-cscs_path:{cscsFile}\"");
                    }
                });

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

        public static bool IsRunning()
        {
            var response = Send($"-client:{procId}");
            return (response != null);
        }

        public static void Exit()
        {
            Send("-exit");
        }

        ////////////////////////////////////////////////

        public static IEnumerable<ICompletionData> GetCompletions(string editorText, string file, int location)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, location, "completion")).ToCompletionData();
        }

        public static string[] FindReferences(string editorText, string file, int location)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, location, "references")).ToReferences();
        }

        public static CodeMapItem[] GetMapOf(string editorText, string file)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, "codemap")).ToCodeMapItems();
        }

        public static DomRegion Resolve(string editorText, string file, int location)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, location, "resolve")).ToDomRegion();
        }

        public static Intellisense.Common.TypeInfo[] GetPossibleNamespaces(string editorText, string file, string word)
        {
            return editorText.WithTempCopy(file,
                tempFile => SendSyntaxCommand(tempFile, $"suggest_usings:{word}")).ToTypeInfos();
        }

        public static string[] GetMemberInfo(string editorText, string file, int location, bool collapseOverloads, out int methodStartPosTemp)
        {
            var overloads = (collapseOverloads ? "-collapseOverloads" : "");

            methodStartPosTemp = 0;
            var result = editorText.WithTempCopy(file,
                                                 tempFile => SendCommand($"-client:{procId}\n" +
                                                                         $"-op:memberinfo\n" +
                                                                         $"-script:{tempFile}\n" +
                                                                         $"-pos:{location}\n" +
                                                                         $"-rich\n" +
                                                                         $"{overloads}"))
                                                 .ToMemberInfoData();

            if (result.Any())
            {
                methodStartPosTemp = result.First().MemberStartPosition;
                return result.Select(x => x.Info).ToArray();
            }
            else
                return new string[0];
        }

        public static string Format(string editorText, string file, ref int caretPosition)
        {
            int location = caretPosition;

            try
            {
                string response = editorText.WithTempCopy(null,
                                                          tempFile => SendSyntaxCommand(tempFile, location, "format"));

                if (response != null && !response.StartsWith("<error>"))
                {
                    // response: $"{caretPos}\n{formattedCode}"
                    int pos = response.IndexOf("\n");
                    if (pos != -1)
                    {
                        caretPosition = int.Parse(response.Substring(0, pos));
                        var result = response.Substring(pos + 1);
                        return result;
                    }
                }
            }
            catch { }
            return null;
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
            if (response == null) StartServer(onlyIfNotRunning: false);
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
            Func<string, string> fixTempFileInsertions = null;

            string tempFileName;
            if (permanentFile == null)
            {
                tempFileName = Path.GetTempFileName();
            }
            else
            {
                var originalName = Path.GetFileName(permanentFile);
                var tempName = Path.ChangeExtension(originalName, ".$temp$" + Path.GetExtension(permanentFile));
                tempFileName = Path.ChangeExtension(permanentFile, ".$temp$" + Path.GetExtension(permanentFile));

                fixTempFileInsertions = txt =>
                {
                    if (!string.IsNullOrEmpty(txt))
                        return txt.Replace(tempName, originalName);
                    else
                        return txt;
                };
            }

            try
            {
                File.WriteAllText(tempFileName, editorText);
                string response = action(tempFileName);

                if (fixTempFileInsertions != null)
                    response = fixTempFileInsertions(response);

                return response == "<null>" ? null : response;
            }
            finally
            {
                try { if (File.Exists(tempFileName)) File.Delete(tempFileName); } catch { }
            }
        }

        public static CompletionType ToCompletionType(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
            {
                string value = data.Replace("event", "_event").Replace("namespace", "_namespace");

                CompletionType type;
                if (!Enum.TryParse(value, out type))
                    type = CompletionType.unresolved;

                return type;
            }
            return CompletionType.unresolved;
        }

        public static CodeMapItem[] ToCodeMapItems(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try
                {
                    return data.GetSerializedLines()
                               .Select(CodeMapItem.Deserialize)
                               .ToArray();
                }
                catch { }
            return new CodeMapItem[0];
        }

        public static Intellisense.Common.MemberInfoData[] ToMemberInfoData(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try
                {
                    return data.GetSerializedLines()
                               .Select(Intellisense.Common.MemberInfoData.Deserialize)
                               .ToArray();
                }
                catch { }
            return new Intellisense.Common.MemberInfoData[0];
        }

        public static Intellisense.Common.TypeInfo[] ToTypeInfos(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try
                {
                    return data.GetSerializedLines()
                               .Select(Intellisense.Common.TypeInfo.Deserialize)
                               .ToArray();
                }
                catch { }
            return new Intellisense.Common.TypeInfo[0];
        }

        public static IEnumerable<ICompletionData> ToCompletionData(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                return data.GetSerializedLines()
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

        public static string[] ToResultStrings(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                return data.GetSerializedLines();

            return new string[0];
        }

        public static DomRegion ToDomRegion(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try { return DomRegion.Deserialize(data); }
                catch { }
            return DomRegion.Empty;
        }

        public static string ToResultString(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try { return data; }
                catch { }
            return null;
        }

        public static string[] ToReferences(this string data)
        {
            if (data != null && !data.StartsWith("<error>"))
                try { return data.Split('\n'); } // references are simple types and syntaxer never returns them as "rich_serialization" serialized
                catch { }
            return new string[0];
        }
    }
}