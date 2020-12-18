// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Runtime.InteropServices.Marshal32
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#region Using directives

using System;
using System.Runtime.InteropServices;

#endregion

namespace InTheHand.Runtime.InteropServices
{
    internal static class Marshal32
    {

#if NETCF

#if V1
        #region Alloc HGlobal
        
        // AllocHGlobal doesn't zero memory in CF2 so use this implementation
        public static IntPtr AllocHGlobal(int cb)
        {
            if (cb > 0)
            {
                return LocalAlloc(0x40, cb);
            }
            else
            {
                throw new ArgumentException("Size passed to AllocHGlobal must be greater than zero");
            }
        }

        [DllImport("coredll.dll", SetLastError=true)]
        private static extern IntPtr LocalAlloc(uint uFlags, int uBytes);

        #endregion

        #region Free HGlobal
#if V1
        public static void FreeHGlobal(IntPtr hglobal)
        {
            if (hglobal != IntPtr.Zero)
            {
                IntPtr ptr = LocalFree(hglobal);
            }
            //desktop doesn't throw - just do nothing
            /*else
            {
                throw new ArgumentNullException("Null pointer passed to FreeHGlobal");
            }*/
        }

        [DllImport("coredll.dll", EntryPoint = "LocalFree", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);
#endif
        #endregion
#endif

        #region Ptr To String Ansi
        /// <summary>
        /// Allocates a managed <see cref="String"/>, copies a specified number of characters from an unmanaged ANSI string into it.
        /// </summary>
        /// <param name="ptr">The address of the first character of the unmanaged string.</param>
        /// <param name="len">The byte count of the input string to copy.</param>
        /// <returns>A managed <see cref="String"/> that holds a copy of the native ANSI string if the value of the ptr parameter is not a null reference (Nothing in Visual Basic); otherwise, this method returns a null reference (Nothing in Visual Basic).</returns>
        public static string PtrToStringAnsi(IntPtr ptr, int len)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            byte[] buffer = new byte[len];
            Marshal.Copy(ptr, buffer, 0, len);
            return System.Text.Encoding.ASCII.GetString(buffer, 0, len);
        }
        #endregion
#endif


        #region Read/Write IntPtr
        /// <summary>
        /// NETCF doesn't have <see cref="M:System.Runtime.InteropServices.Marshal.ReadIntPtr(System.IntPtr,System.Int32)"/>
        /// </summary>
        public static IntPtr ReadIntPtr(IntPtr ptr, int index)
        {
            IntPtr ptrResult;
            if (IntPtr.Size == 8) {
#if NETCF && V1
                throw new NotSupportedException("Marshal32.ReadIntPtr 64-bit.");
#else
                Int64 asInt = Marshal.ReadInt64(ptr, index);
                ptrResult = new IntPtr(asInt);
#endif
            } else {
                Int32 asInt = Marshal.ReadInt32(ptr, index);
                ptrResult = new IntPtr(asInt);
            }
            return ptrResult;
        }

        public static IntPtr ReadIntPtr(byte[] buf, int index)
        {
            IntPtr ptrResult;
            if (IntPtr.Size == 8) {
                Int64 asInt = BitConverter.ToInt64(buf, index);
                ptrResult = new IntPtr(asInt);
            } else {
                Int32 asInt = BitConverter.ToInt32(buf, index);
                ptrResult = new IntPtr(asInt);
            }
            return ptrResult;
        }

        public static void WriteIntPtr(IntPtr ptr, int index, IntPtr value)
        {
            if (IntPtr.Size == 8) {
#if NETCF && V1
                throw new NotSupportedException("Marshal32.WriteIntPtr 64-bit.");
#else
                Marshal.WriteInt64(ptr, index, value.ToInt64());
#endif
            } else {
                Marshal.WriteInt32(ptr, index, value.ToInt32());
            }
        }

        public static void WriteIntPtr(byte[] buf, int index, IntPtr value)
        {
            byte[] ptrBytes;
            if (IntPtr.Size == 8) {
                Int64 asInt = value.ToInt64();
                ptrBytes = BitConverter.GetBytes(asInt);
            } else {
                Int32 asInt = value.ToInt32();
                ptrBytes = BitConverter.GetBytes(asInt);
            }
            ptrBytes.CopyTo(buf, index);
        }
        #endregion

    }

}
