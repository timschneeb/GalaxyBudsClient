// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.HardwareStatus
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Specifies the current status of the Bluetooth hardware.
    /// </summary>
    public enum HardwareStatus : int
    {
        // CE5 is http://msdn.microsoft.com/en-us/library/ms887837.aspx
        // but they seems worse descriptions so don't use them.

        /// <summary>
        /// Status cannot be determined.
        /// </summary>
        /// XXXX &#x201C;The stack is not present.&#x201D; CE5
        Unknown = 0,
        /// <summary>
        /// Bluetooth radio not present.
        /// </summary>
        /// &#x201C;The adapter is not present.&#x201D; CE5
        NotPresent = 1,
        /// <summary>
        /// Bluetooth radio is in the process of starting up.
        /// </summary>
        /// &#x201C;The adapter might be installed.
        /// The stack is currently on the way up. Call again later.&#x201D; CE5
        Initializing = 2,
        /// <summary>
        /// Bluetooth radio is active.
        /// </summary>
        /// &#x201C;The adapter is installed and the stack is running.&#x201D; CE5
        Running = 3,
        /// <summary>
        /// Bluetooth radio is in the process of shutting down.
        /// </summary>
        /// &#x201C;The adapter is installed, but the stack is not running.&#x201D; CE5
        Shutdown = 4,
        /// <summary>
        /// Bluetooth radio is in an error state.
        /// </summary>
        /// &#x201C;The adapter might be installed.
        /// The stack is on the way down. Call again later.&#x201D; CE5
        Error = 5,
    }
}
