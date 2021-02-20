// 32feet.NET - Personal Area Networking for .NET
//
// Utils.Process2
// 
// Copyright (c) 2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Utils
{
    static class Process2
    {
        /// <summary>
        /// NETCF Version of: Creates an array of new Process components and associates them with all the process resources on the local computer that share the specified process name.
        /// </summary>
        /// -
        /// <param name="processName">e.g. "BTExplorer"
        /// </param>
        /// -
        /// <returns>An array of type <see cref="T:System.Diagnostics.Process"/>
        /// that represents the process resources running the specified application or file.
        /// </returns>
        public static Process[] GetProcessesByName(string processName)
        {
#if !NETCF
            Debug.Fail("Using NETCF GetProcessesByName not supported on desktop!");
            return new Process[0];
#else
            //
            var exeName = processName + ".exe";
            uint pid = 0;
            IntPtr hSnapshot = NativeMethods.CreateToolhelp32Snapshot(
                TH32CS.SnapProcess | TH32CS.SnapNoHeaps, pid);
            if (hSnapshot == IntPtr.Zero || hSnapshot == (IntPtr)(-1))
                throw new Win32Exception();
            try {
                bool got;
                var list = new List<Process>();
                var pe = new PROCESSENTRY32(new Version(0, 0));
                got = NativeMethods.Process32First(hSnapshot, ref pe);
                while (got) {
                    var match = exeName.Equals(pe.szExeFile, StringComparison.OrdinalIgnoreCase);
                    if (match) {
                        var proc = Process.GetProcessById(pe.th32ProcessID);
                        list.Add(proc);
                    }
                    got = NativeMethods.Process32Next(hSnapshot, ref pe);
                }
                int gle = Marshal.GetLastWin32Error();
                if (gle != NativeMethods.ERROR_NO_MORE_FILES) {
                    throw new Win32Exception();
                }
                return list.ToArray();
            } finally {
                var success = NativeMethods.CloseToolhelp32Snapshot(hSnapshot);
                Debug.Assert(success, "INFO: fail on CloseToolhelp32Snapshot");
            }
#endif
        }

#if NETCF
        static class NativeMethods
        {
            const string ThDll = "Toolhelp";

            // winerror.h
            internal const int ERROR_NO_MORE_FILES = 18;

            //
            [DllImport(ThDll, SetLastError = true)]
            internal extern static IntPtr CreateToolhelp32Snapshot(TH32CS flags, uint th32ProcessID);

            [DllImport(ThDll, SetLastError = true)]
            internal extern static bool CloseToolhelp32Snapshot(IntPtr hSnapshot);

            [DllImport(ThDll, SetLastError = true)]
            internal extern static bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

            [DllImport(ThDll, SetLastError = true)]
            internal extern static bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        }

        [Flags]
        enum TH32CS : uint
        {
            SNAPHEAPLIST = 0x00000001,
            SnapProcess = 0x00000002,
            SNAPTHREAD = 0x00000004,
            SNAPMODULE = 0x00000008,
            SnapNoHeaps = 0x40000000,	// optimization to not snapshot heaps
            SNAPALL = (SNAPHEAPLIST | SnapProcess | SNAPTHREAD | SNAPMODULE),
            GETALLMODS = 0x80000000,
        }

        struct PROCESSENTRY32
        {
            const int MAX_PATH = 260; // windef.h
            //
            int dwSize;
            uint cntUsage;
            internal readonly int th32ProcessID;
            uint th32DefaultHeapID;
            uint th32ModuleID;
            uint cntThreads;
            uint th32ParentProcessID;
            int pcPriClassBase;
            uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal readonly string szExeFile;
            uint th32MemoryBase;
            uint th32AccessKey;

            public PROCESSENTRY32(Version dummy)
                : this()
            {
                dwSize = Marshal.SizeOf(this);
            }
        }
#endif

    }
}
