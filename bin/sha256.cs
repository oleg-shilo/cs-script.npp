using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ConsoleApplicationMD5test
{
    class Program
    {
        private static void Main(string[] args)
        {
            string fileName = args.First();

            byte[] checksum = new SHA256Managed().ComputeHash(File.ReadAllBytes(fileName));

            var sha256 = BitConverter.ToString(checksum).Replace("-", "");

            Console.WriteLine(sha256);
        }
    }
}