// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Utilities
// 
// Copyright (c) 2003-2007 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace InTheHand.Net
{
    internal static class PlatformVerification
    {

        public static void ThrowException()
        {
#if NETCF
            if (System.Environment.OSVersion.Platform != PlatformID.WinCE)
            {
                throw new PlatformNotSupportedException("This .NET Compact Framework version of the 32feet.NET assembly can not be run on the desktop .NET framework");
            }
#endif
		}

#if !V1
        static bool? s_isMonoRuntime;
#else
        static object s_isMonoRuntime;
#endif

        public static bool IsMonoRuntime
        {
            get
            {
                if (s_isMonoRuntime == null) {
                    Type t = Type.GetType("Mono.Runtime");
                    s_isMonoRuntime = (t != null);
                }
                return (bool)s_isMonoRuntime;
            }
        }

    }

    // <summary>
    // Helper class for GCHandle.
    // </summary>
    internal static class GCHandleHelper
    {
#if NETCF && V1
        private static bool v1 = false;

        static GCHandleHelper()
        {
            string s = "Marshal";
            GCHandle h = GCHandle.Alloc(s.ToCharArray(), GCHandleType.Pinned);
            IntPtr p = h.AddrOfPinnedObject();
            short c1 = Marshal.ReadInt16(p);
            if (c1 != 0x4d)
            {
                v1 = true;
            }
            else
            {
                v1 = false;
            }

            h.Free();
        }
#endif

        // <summary>
        // Returns the memory address of the pinned item.
        // </summary>
        // <param name="handle">GCHandle allocated as Pinned.</param>
        // <returns>Address of the pinned item.</returns>
        public static IntPtr AddrOfPinnedObject(GCHandle handle)
        {
#if NETCF && V1
            if (v1 && handle.Target.GetType().IsClass)
            {
                return new IntPtr(handle.AddrOfPinnedObject().ToInt32() + 4);
            }
            else
            {
                return handle.AddrOfPinnedObject();
            }
#else
            //on CF2 or desktop use the standard API method
            return handle.AddrOfPinnedObject();
#endif

        }

    }

    //--------------------------------------------------------------------------
    internal static class ExceptionExtension
    {
        /// <summary>
        /// Get the normal first line of <c>Exception.ToString()</c>,
        /// that is without the stack trace lines.
        /// </summary>
        /// -
        /// <remarks>
        /// Get the normal first line of <c>Exception.ToString()</c>,
        /// that is including details of all inner exceptions,
        /// but without the stack trace lines.
        /// e.g. <c>System.IO.IOException: An established connection was aborted by the software in your host machine. ---> System.Net.Sockets.SocketException: An established connection was aborted by the software in your host machine.</c>
        /// </remarks>
        /// -
        /// <param name="this">The exception.
        /// </param>
        /// -
        /// <returns>A string containing the first line of the <c>Exception.ToString()</c>.
        /// </returns>
        public static string ToStringNoStackTrace(Exception @this)
        {
            using (StringReader rdr = new StringReader(@this.ToString())) {
                return rdr.ReadLine();
            }
        }
    }

    //--------------------------------------------------------------------------
    internal
#if ! V1
    static
#endif
    class ExceptionFactory
    {
#if V1
        private ExceptionFactory() { }
#endif

        /// <exclude/>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
#if CODE_ANALYSIS
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#param",
            Justification = "Verbatim copy of FX version!")]
#endif
        public static ArgumentOutOfRangeException ArgumentOutOfRangeException(String paramName, String message)
        {
#if V1 //&& NETCF
            return new ArgumentOutOfRangeException(message);
#else
            return new ArgumentOutOfRangeException(paramName, message);
#endif
        }

    }//class

    /*
    //--------------------------------------------------------------------------
    internal
#if ! V1
    static
#endif
    class StringUtilities
    {
#if V1
        private StringUtilities() { }
#endif

        /// <exclude/>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static bool IsNullOrEmpty(String value)
        {
#if V1
            if (value == null) return true;
            if (value.Length == 0) return true;
            return false;
#else
            return String.IsNullOrEmpty(value);
#endif
        }

#if !V1
        public static string String_Join<T>(System.Collections.Generic.IList<T> objects)
        {
            const string Sepa = ", ";
#if !NETCF && DEBUG
            T[] objectArray = new T[objects.Count];
            objects.CopyTo(objectArray, 0);
            string[] arr = Array.ConvertAll<T, string>(objectArray, delegate(T cur) {
                return cur.ToString();
            });
            string allX = string.Join(", ", arr);
#endif
            StringBuilder bldr = new StringBuilder();
            foreach (T cur in objects) {
                bldr.Append(cur.ToString());
                bldr.Append(Sepa);
            }
            if (bldr.Length > 0)
                bldr.Length -= Sepa.Length;
#if !NETCF && DEBUG
            Debug.Assert(bldr.ToString() == allX, "two ways equal?!!");
#endif
            return bldr.ToString();
        }
#endif

    }//class
    */
}
