// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.TestUtilities
// 
// Copyright (c) 2009-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2009-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net
{
    static class TestUtilities
    {
        //--------------------------------------------------------------
#if V1
        static object s_isNunitHarness;
#else
        static bool? s_isNunitHarness;
#endif

        public static bool IsUnderTestHarness()
        {
            if (s_isNunitHarness == null) {
                s_isNunitHarness = IsRunningUnderNUnit();
            }
            return (bool)s_isNunitHarness;
        }

#if NETCF
        static bool s_IsInNetcfTestRunner;

        public static void SetIsInNetcfTestRunner()
        {
            s_IsInNetcfTestRunner = true;
        }

        static bool IsRunningUnderNUnit()
        {
            return s_IsInNetcfTestRunner;
        }
#else
        // From http://geekswithblogs.net/ajohns/archive/2004/08/05/9389.aspx
        static bool IsRunningUnderNUnit()
        {
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < trace.FrameCount; i++) {
                System.Reflection.MethodBase methodBase = trace.GetFrame(i).GetMethod();
                /*if (methodBase.IsDefined(typeof(NUnit.Framework.TestAttribute), true)) {
                    return true;
                }*/
                object[] attrArr = methodBase.GetCustomAttributes(false);
                foreach (object attr in attrArr) {
                    if (attr.GetType().Name == "TestAttribute")
                        return true;
                    if (attr.GetType().Name == "TestFixtureSetUpAttribute")
                        return true;
                }
            }
            return false;
        }
#endif

    }
}
