#if WinXP
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth
{
    internal static class LmpFeaturesUtils
    {
        [Conditional("DEBUG")]
        internal static void FindUndefinedValues(LmpFeatures lmpFeatures)
        {
#if DEBUG
            Action<LmpFeatures> action = delegate(LmpFeatures bitMask)
            {
                var x = lmpFeatures & bitMask;
                if (x != 0) {
                    var name = Enum.GetName(typeof(LmpFeatures), x);
                    if (name == null) {
                        var msg = "Not defined: 0x" + unchecked((UInt64)x).ToString("X16");
                        Debug.WriteLine(msg);
                        Debug.Fail(msg);
                    }
                }
            };
            ForEachBit(action);
#endif
        }

        [Conditional("DEBUG")]
        internal static void FindUnsetValues(LmpFeatures lmpFeatures)
        {
#if DEBUG
            Action<LmpFeatures> action = delegate(LmpFeatures bitMask)
            {
                var x = lmpFeatures & bitMask;
                if (x == 0) {
                    var name = Enum.GetName(typeof(LmpFeatures), bitMask);
                    string msg;
                    if (name == null) {
                        //msg = "((Not exist: '" + bitMask + "' 0x" + ((UInt64)bitMask).ToString("X16") + "))";
                        //Debug.WriteLine(msg);
                    } else {
                        msg = "Not set: '" + bitMask + "' 0x" + ((UInt64)bitMask).ToString("X16");
                        Debug.WriteLine(msg);
                        Debug.Fail(msg);
                    }
                }
            };
            ForEachBit(action);
#endif
        }

        static void ForEachBit(Action<LmpFeatures> action)
        {
            int dbgCount = 0;
            Int64 i = 1;
            while (i != 0) {
                action((LmpFeatures)i);
                i <<= 1;
                ++dbgCount;
            }//while
            Debug.Assert(dbgCount == 64);
        }
    }
}
#endif