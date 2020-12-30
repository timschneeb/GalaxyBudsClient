// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Windows.Forms.ISelectBluetoothDevice
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the Microsoft Public License (Ms-PL) - see License.txt


#if ! NO_WINFORMS
using System;
using InTheHand.Net.Bluetooth;

namespace InTheHand.Windows.Forms
{
    interface ISelectBluetoothDevice
    {
        void Reset();
        bool ShowAuthenticated { get; set; }
        bool ShowRemembered { get; set; }
        bool ShowUnknown { get; set; }
        bool ShowDiscoverableOnly { get; set; }
        bool ForceAuthentication { get; set; }
        string Info { get; set; }
#if WinXP
        bool AddNewDeviceWizard { get; set; }
        bool SkipServicesPage { get; set; }
#endif
        void SetClassOfDevices(ClassOfDevice[] classOfDevices);
        void SetFilter(Predicate<InTheHand.Net.Sockets.BluetoothDeviceInfo> filterFn,
            SelectBluetoothDeviceDialog.PFN_DEVICE_CALLBACK msftFilterFn);
    }
}
#endif