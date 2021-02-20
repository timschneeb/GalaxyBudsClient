// Copyright 2008 Alp Toker <alp@atoker.com>
// This software is made available under the MIT License
// See COPYING for details

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ThePBone.BlueZNet.Interop
{
    static class UnixError
    {
        internal const string LIBC = "libc";

        [DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=false)]
        static extern IntPtr strerror (int errnum);

        public static string GetErrorString (int errnum)
        {
            IntPtr strPtr = strerror (errnum);

            if (strPtr == IntPtr.Zero)
                return "Unknown Unix error";

            return Marshal.PtrToStringAnsi (strPtr);
        }

        // FIXME: Don't hard-code this.
        const int EINTR = 4;

        public static bool ShouldRetry
        {
            get {
                int errno = Marshal.GetLastWin32Error ();
                return errno == EINTR;
            }
        }

        public static UnixException GetLastUnixException ()
        {
            int errno = Marshal.GetLastWin32Error ();
            return new UnixException(errno);
        }
    }
    
    public class UnixException : Exception
    {
        public int Errno { get; }
        public string ErrorString => UnixError.GetErrorString(Errno);
        public UnixException(int errno)
            : base($"Error {errno}: {UnixError.GetErrorString(errno)}")
        {
            Errno = errno;
        }
    }
    
    public class UnixSocketException : UnixException
    {
        public UnixSocketException(int errno) : base(errno){}
    }
}