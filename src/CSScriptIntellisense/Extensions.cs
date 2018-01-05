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

        public static string GetDirName(this string path)
        {
            return Path.GetDirectoryName(path);
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