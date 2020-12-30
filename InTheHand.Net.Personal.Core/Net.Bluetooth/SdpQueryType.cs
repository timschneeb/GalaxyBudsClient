// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.SdpQueryType
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License


namespace InTheHand.Net.Bluetooth
{

#if NETCF

    // Used by InTheHand.Net.Sockets.BluetoothDeviceInfo.GetServiceRecordsUnparsed(System.Guid)
    // Moved from /BTHNS_RESTRICTIONBLOB.cs.
    internal enum SdpQueryType : int
    {
        SearchRequest = 1,
        AttributeRequest = 2,
        SearchAttributeRequest = 3,
    }//enum

#endif

}