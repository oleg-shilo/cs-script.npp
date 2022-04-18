using System;
using System.Runtime.InteropServices;

namespace CSScriptNpp
{
    // based on work of Ray Koopa: http://www.codeproject.com/Articles/878605/Getting-all-Special-Folders-in-NET
    public class KnownFolders
    {
        [Flags]
        enum KnownFolderFlags : uint
        {
            DefaultPath = 0x00000400,
            DontVerify = 0x00004000
        }

        static Guid Downloads = new Guid("{374DE290-123F-4565-9164-39C4925E467B}");

        public static string UserDownloads
        {
            get
            {
                return GetPath(Downloads,
                (uint)(KnownFolderFlags.DefaultPath | KnownFolderFlags.DontVerify),
                false);
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        static string GetPath(Guid knownFolder, uint flags, bool defaultUser)
        {
            IntPtr outPath;
            int result = SHGetKnownFolderPath(knownFolder, flags, new IntPtr(defaultUser ? -1 : 0), out outPath);
            if (result >= 0)
                return Marshal.PtrToStringUni(outPath);
            else
                throw new ExternalException("Unable to retrieve the known folder path. It may not be available on this system.", result);
        }
    }
}