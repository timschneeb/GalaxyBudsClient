using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Utils
{
    static partial class MiscUtils
    {
        internal static void Trace_WriteLine(string message)
        {
#if !NETCF
            Trace.WriteLine(message);
#else
            Trace_WriteLine_NETCF(message); // :-( T.WL only in 3.5
#endif
        }

        internal static void Trace_WriteLine(string format, params object[] args)
        {
            Trace_WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                format, args));
        }

        // (No Trace.Fail on NETCF nor Android)
        internal static void Trace_Fail(string message)
        {
#if NETCF || ANDROID
            Trace_Fail_NETCF(message);
#else
            Trace.Fail(message);
#endif
        }

        // (No Trace.Assert on Android)
        internal static void Trace_Assert(bool test)
        {
#if ANDROID
            Trace_Assert_ANDROID(test);
#else
            Trace.Assert(test);
#endif
        }
        internal static void Trace_Assert(bool test, string message)
        {
#if ANDROID
            Trace_Assert_ANDROID(test, message);
#else
            Trace.Assert(test, message);
#endif
        }

        [Conditional("DEBUG")]
        internal static void ConsoleDebug_WriteLine(string value)
        {
#if !NETCF
            Console.WriteLine(value);
#endif
        }

        internal static string ToStringQuotedOrNull<T>(T obj)
        {
            if (obj == null)
                return "(null)";
            else
                return "'" + obj.ToString() + "'";
        }

        internal static void AssertEquals<T>(T p1, T p2)
        {
            Debug.Assert(object.Equals(p1, p2),
                "NOT equals: " + ToStringQuotedOrNull(p1)
                + " vs: " + ToStringQuotedOrNull(p2));
        }

    }
}
