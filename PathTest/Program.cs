using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace PathTest
{
    class Program
    {
        static void Main()
        {
            //string path = Assembly.GetExecutingAssembly().Location;
            string path = @"B:\Temp\ScopeSV\setup.exe";
            //string path = @"C:\Windows\notepad.exe";
            System.Diagnostics.Debug.WriteLine(path);
            string realPath;

            // Get the drive letter 
            string driveLetter = Path.GetPathRoot(path).Replace("\\", "");
            const int pathSize = 256;
            StringBuilder pathBuffer = new StringBuilder(pathSize);
            QueryDosDevice(driveLetter, pathBuffer, pathSize);
            string dosPath = pathBuffer.ToString();
            if (dosPath.Contains("\\??\\"))
            {
                // Strip the \??\ prefix.
                string realRoot = dosPath.Remove(0, 4);

                //Combine the paths.
                realPath = Path.Combine(realRoot, path.Replace(Path.GetPathRoot(path), ""));
            }
            else if (PathIsNetworkPath(path))
            {
                // The pointer in memory to the structure.
                IntPtr buffer = IntPtr.Zero;

                // Wrap in a try/catch block for cleanup.
                try
                {
                    // First, call WNetGetUniversalName to get the size.
                    int size = 0;

                    // Make the call.
                    // Pass IntPtr.Size because the API doesn't like null, even though
                    // size is zero.  We know that IntPtr.Size will be
                    // aligned correctly.
                    int apiRetVal = WNetGetUniversalName(path, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

                    // If the return value is not ERROR_MORE_DATA, then
                    // raise an exception.
                    if (apiRetVal != ERROR_MORE_DATA)
                        // Throw an exception.
                        throw new Win32Exception(apiRetVal);

                    // Allocate the memory.
                    buffer = Marshal.AllocCoTaskMem(size);

                    // Now make the call.
                    apiRetVal = WNetGetUniversalName(path, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                    // If it didn't succeed, then throw.
                    if (apiRetVal != NOERROR)
                        // Throw an exception.
                        throw new Win32Exception(apiRetVal);

                    // Now get the string.  It's all in the same buffer, but
                    // the pointer is first, so offset the pointer by IntPtr.Size
                    // and pass to PtrToStringAnsi.
                    realPath = Marshal.PtrToStringAuto(new IntPtr(buffer.ToInt64() + IntPtr.Size), size);
                    realPath = realPath.Substring(0, realPath.IndexOf('\0'));
                }
                finally
                {
                    // Release the buffer.
                    Marshal.FreeCoTaskMem(buffer);
                }
            }
            else
            {
                realPath = path;
            }
            System.Diagnostics.Debug.WriteLine(realPath);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, uint ucchMax);

        [DllImport("shlwapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PathIsNetworkPath(string pszPath);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        const int ERROR_MORE_DATA = 234;
        const int NOERROR = 0;
    }
}
