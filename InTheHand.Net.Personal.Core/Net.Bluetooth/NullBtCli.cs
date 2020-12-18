using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth
{
    class NullBtCli : CommonBluetoothClient
    {
        internal NullBtCli(NullBluetoothFactory fcty, CommonRfcommStream conn)
            : base(fcty, conn)
        {
        }

        public override IAsyncResult BeginServiceDiscovery(BluetoothAddress address, Guid serviceGuid, AsyncCallback asyncCallback, object state)
        {
            throw new NotImplementedException();
        }

        public override List<int> EndServiceDiscovery(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        protected override void BeginInquiry(int maxDevices, AsyncCallback callback, object state, InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState, DiscoDevsParams args)
        {
            throw new NotImplementedException();
        }

        protected override List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        protected override List<IBluetoothDeviceInfo> GetKnownRemoteDeviceEntries()
        {
            throw new NotImplementedException();
        }

        public override void SetPin(string pin)
        {
            throw new NotImplementedException();
        }

        public override void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }
    }
}
