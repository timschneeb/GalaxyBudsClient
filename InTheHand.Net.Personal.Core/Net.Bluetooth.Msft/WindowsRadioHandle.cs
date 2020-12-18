// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.WindowsRadioHandle
// 
// Copyright (c) 2012 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Msft
{
    internal class WindowsRadioHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Presuming this needed by the pattern?")]
        internal WindowsRadioHandle(bool ownsHandle)
            : base(ownsHandle)
        {
        }

        internal WindowsRadioHandle(IntPtr handle)
            : base(true)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            var success = NativeMethods.CloseHandle(handle);
            return success;
        }
    }

}
