#if NO_MSFT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Msft
{
	struct BLUETOOTH_DEVICE_INFO
	{
		internal BLUETOOTH_DEVICE_INFO (object x)
		{
		}
	}

    class WindowsBluetoothDeviceInfo : IBluetoothDeviceInfo
    {
		internal WindowsBluetoothDeviceInfo(object xx) { }

		#region IBluetoothDeviceInfo Members

        void IBluetoothDeviceInfo.Merge(IBluetoothDeviceInfo other)
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.SetDiscoveryTime(DateTime dt)
        {
            throw new NotImplementedException();
        }

        bool IBluetoothDeviceInfo.Authenticated
        {
            get { throw new NotImplementedException(); }
        }

        ClassOfDevice IBluetoothDeviceInfo.ClassOfDevice
        {
            get { throw new NotImplementedException(); }
        }

        bool IBluetoothDeviceInfo.Connected
        {
            get { throw new NotImplementedException(); }
        }

        BluetoothAddress IBluetoothDeviceInfo.DeviceAddress
        {
            get { throw new NotImplementedException(); }
        }

        string IBluetoothDeviceInfo.DeviceName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        byte[][] IBluetoothDeviceInfo.GetServiceRecordsUnparsed(Guid service)
        {
            throw new NotImplementedException();
        }

        ServiceRecord[] IBluetoothDeviceInfo.GetServiceRecords(Guid service)
        {
            throw new NotImplementedException();
        }

        IAsyncResult IBluetoothDeviceInfo.BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        ServiceRecord[] IBluetoothDeviceInfo.EndGetServiceRecords(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        Guid[] IBluetoothDeviceInfo.InstalledServices
        {
            get { throw new NotImplementedException(); }
        }

        DateTime IBluetoothDeviceInfo.LastSeen
        {
            get { throw new NotImplementedException(); }
        }

        DateTime IBluetoothDeviceInfo.LastUsed
        {
            get { throw new NotImplementedException(); }
        }

        void IBluetoothDeviceInfo.Refresh()
        {
            throw new NotImplementedException();
        }

        bool IBluetoothDeviceInfo.Remembered
        {
            get { throw new NotImplementedException(); }
        }

        int IBluetoothDeviceInfo.Rssi
        {
            get { throw new NotImplementedException(); }
        }

        void IBluetoothDeviceInfo.SetServiceState(Guid service, bool state, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.SetServiceState(Guid service, bool state)
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.ShowDialog()
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.Update()
        {
            throw new NotImplementedException();
        }

        RadioVersions IBluetoothDeviceInfo.GetVersions()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
#endif
