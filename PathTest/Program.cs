using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace PathTest
{
    class Program
    {
        static void Main()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            System.Diagnostics.Debug.WriteLine(path);

            const int pathSize = 256;
            StringBuilder pathBuffer = new StringBuilder(pathSize);
            QueryDosDevice("R:", pathBuffer, pathSize);
            System.Diagnostics.Debug.WriteLine(pathBuffer.ToString());
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, uint ucchMax);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        const int REMOTE_NAME_INFO_LEVEL = 0x00000002;
        const int ERROR_MORE_DATA = 234;
        const int NOERROR = 0;
    }
}
