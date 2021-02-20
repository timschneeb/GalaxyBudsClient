// 32feet.NET - Personal Area Networking for .NET
//
// Utils.Pointers
// 
// Copyright (c) 2010-11 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-11 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace Utils
{
    static class Pointers
    {
        internal static IntPtr Add(IntPtr x, int y)
        {
            checked {
                var xi = x.ToInt64();
                xi += y;
                IntPtr p = new IntPtr(xi);
                return p;
            }
        }

        private static IntPtr Add(IntPtr x, IntPtr y)
        {
            checked {
                var xi = x.ToInt64();
                var yi = y.ToInt64();
                xi += yi;
                IntPtr p = new IntPtr(xi);
                return p;
            }
        }

    }
}
