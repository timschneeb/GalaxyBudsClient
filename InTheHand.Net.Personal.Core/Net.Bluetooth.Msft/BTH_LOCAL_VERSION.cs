// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BTH_LOCAL_VERSION
// 
// Copyright (c) 2003-2006,2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    [StructLayout(LayoutKind.Sequential)]
	internal class BTH_LOCAL_VERSION
	{
		byte	hci_version;
		ushort	hci_revision;
		byte	lmp_version;
		ushort	lmp_subversion;
		ushort	manufacturer;
		long	lmp_features;

		public BluetoothVersion ToVersion()
		{
			return new BluetoothVersion(hci_version, hci_revision, lmp_version, lmp_subversion, manufacturer, lmp_features);
		}

        // TODO Actually is the BTH_LOCAL_VERSION class entirely unused and should be removed?
        // Fake #ctor to stop CFv1 compiler warnings.
        private BTH_LOCAL_VERSION(double fakeCtorToStopCf1CompilerWarnings)
        {
            hci_version = lmp_version = 0;
            hci_revision = lmp_subversion = manufacturer = 0;
            lmp_features = 0;
        }

	}//class
}
