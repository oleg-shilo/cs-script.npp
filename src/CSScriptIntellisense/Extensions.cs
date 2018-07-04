using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltraSharp.Cecil;

namespace CSScriptIntellisense
{
    public static class SocketExtensions
    {
        public static string PathJoin(this string path, params string[] items)
        {
            return Path.Combine(new[] { path }.Concat(items).ToArray());
        }

        /// <summary>
        /// Determines whether the file has the specified extension (e.g. ".cs").
        /// <para>Note it is case insensitive.</para>
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static bool HasExtension(this string file, string extension)
        {
            return !string.IsNullOrWhiteSpace(file) && file.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        public static string Path(this Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        public static bool HasText(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        public static string GetDirName(this string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static int MapLine(this string text_after, int line_before, string text_before)
        {
            // map a position/line before and after text formatting
            // formatting does not remove nor introduces any new character but only whitespces

            string[] lines_before = text_before.GetLines();
            bool was_an_empty_line = (lines_before[line_before].Trim() == "");
            bool had_more_text = lines_before.Skip(line_before).Any(x => x.Trim() != "");

            int lines_including_line_before = line_before + 1;

            var pattern = lines_before.Take(lines_including_line_before).JoinLines("").Trim();

            int result = -1;
            for (int i = 0; i < pattern.Length; i++)
            {
                char char_before = pattern[i];

                if (!Char.IsWhiteSpace(char_before))
                {
                    result++;
                    for (; result < text_after.Length; result++)
                    {
                        char char_after = text_after[result];
                        if (!Char.IsWhiteSpace(char_after))
                        {
                            if (char_before != char_after)
                                return -1;
                            else
                                break;
                        }
                    }
                }
            }

            if (result != -1)
            {
                var matching_length = result + 1;
                string[] matching_lines_after = text_after.Substring(0, matching_length).GetLines();

                var extra = 0;
                if (was_an_empty_line && had_more_text)
                    extra = 1;

                return matching_lines_after.Count() - 1 + extra; // -1 because returning the line index not the count
            }
            else
                return result;
        }

        public static int MapPos(this string text_after, int pos_before, string text_before)
        {
            // map a position before and after text formatting
            // formatting does not remove nor introduces any new character but only whitespces

            int result = -1;
            for (int i = 0; i <= pos_before; i++)
            {
                char char_before = text_before[i];

                if (!Char.IsWhiteSpace(char_before))
                {
                    result++;
                    for (; result < text_after.Length; result++)
                    {
                        char char_after = text_after[result];
                        if (!Char.IsWhiteSpace(char_after))
                        {
                            if (char_before != char_after)
                                return -1;
                            else
                                break;
                        }
                    }
                }
            }

            return result;
        }

        public static string EnsureDir(this string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static byte[] GetBytes(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static string GetString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] ReadAllBytes(this TcpClient client)
        {
            var bytes = new byte[client.ReceiveBufferSize];
            var len = client.GetStream()
                            .Read(bytes, 0, bytes.Length);
            var result = new byte[len];
            Array.Copy(bytes, result, len);
            return result;
        }

        public static string ReadAllText(this TcpClient client)
        {
            return client.ReadAllBytes().GetString();
        }

        public static void WriteAllBytes(this TcpClient client, byte[] data)
        {
            var stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void WriteAllText(this TcpClient client, string data)
        {
            client.WriteAllBytes(data.GetBytes());
        }
    }
}