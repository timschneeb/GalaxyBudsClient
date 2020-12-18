// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

#if NETCF || ANDROID
#define DEBUG // Mapping Trace.WriteLine to Debug.WriteLine for NETCF pre-3.5!
using System;
using System.Diagnostics;

namespace Utils
{
    partial class MiscUtils
    {
        private static void Trace_WriteLine_NETCF(string message)
        {
            // DEBUG always defined above.
            Debug.WriteLine(message);
        }

        private static void Trace_Fail_NETCF(string message)
        {
            // DEBUG always defined above.
            Debug.Fail(message);
        }

        private static void Trace_Assert_ANDROID(bool test)
        {
            // DEBUG always defined above.
            Debug.Assert(test);
        }
        private static void Trace_Assert_ANDROID(bool test, string message)
        {
            // DEBUG always defined above.
            Debug.Assert(test, message);
        }

    }
}
#endif

#if ANDROID
namespace System.Diagnostics
{
    class Trace
    {
        internal static void WriteLine(string message)
        {
            // DEBUG always defined above.
            try {
                Debug.WriteLine(message);
            } catch (System.IO.IOException) {
                //  Debug throws IO error when debugging.
                //  But the write actually appears in the log...
            }
        }

    }
}
#endif
