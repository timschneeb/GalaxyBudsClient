// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Factory.IBluetoothRadio
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public interface IBluetoothRadio
    {
#pragma warning disable 1591
        ClassOfDevice ClassOfDevice { get; }
        IntPtr Handle { get; }
        HardwareStatus HardwareStatus { get; }
        BluetoothAddress LocalAddress { get; }
        RadioMode Mode { get; set; }
        //
        RadioModes Modes { get; }
        //TODO
        void SetMode(bool? connectable, bool? discoverable);
        //
        string Name { get; set; }
        Manufacturer SoftwareManufacturer { get; }
        string Remote { get; }
        //
        HciVersion HciVersion { get; }
        int HciRevision { get; }
        LmpVersion LmpVersion { get; }
        int LmpSubversion { get; }
        LmpFeatures LmpFeatures { get; }
        Manufacturer Manufacturer { get; }
    }
}
