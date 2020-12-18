// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth.Widcomm
{
#if TEST_EARLY
    public 
#endif
    interface ISdpDiscoveryRecordsBuffer : IDisposable
    {
        /// <summary>
        /// Get the number of records that the buffer contains.
        /// </summary>
        /// -
        /// <value>An integer containing the number of records that the buffer contains,
        /// may be zero.
        /// </value>
        /// -
        /// <exception cref="T:System.InvalidOperationException">The buffer has 
        /// not yet been filled with a CSdpDiscoveryRec list.
        /// </exception>
        /// -
        /// <remarks>
        /// <para>In <see cref="F:InTheHand.Net.Bluetooth.Widcomm.SdpSearchScope.ServiceClassOnly">SdpSearchScope.ServiceClassOnly</see>
        /// this returns the actual number of records as the filtering is done by
        /// the stack.  In <see cref="F:InTheHand.Net.Bluetooth.Widcomm.SdpSearchScope.Anywhere">SdpSearchScope.Anywhere</see>
        /// this returns the pre-filtered number of records.  We do the filtering
        /// so this will likely be greater that the matching number of records.
        /// </para>
        /// </remarks>
        int RecordCount { get;}
        //
        int[] Hack_GetPorts();
        int[] Hack_GetPsms();
        ServiceRecord[] GetServiceRecords();
    }
}
