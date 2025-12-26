using System.Net.Sockets;
using InTheHand.Net.Sockets;
using System;
using System.Net;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    class BluezRfcommEndPoint : BluetoothEndPoint
    {
        #region sockaddr_rc layout
        // * BlueZ
        //struct sockaddr_rc
        //{
        //    sa_family_t rc_family;
        //    bdaddr_t rc_bdaddr;
        //    uint8_t rc_channel;
        //};
        //typedef struct {
        //    uint8_t b[6];
        //} __attribute__((packed)) bdaddr_t;
        const int AddrOffset = 2;
        const int ScnOffset = 8;
        const int ScnLength = 1;
        const int SaLen = 2 + 6 + ScnLength + (1);
        #endregion

        //----
        public BluezRfcommEndPoint(BluetoothAddress address, int scn)
            : base(address, BluetoothService.Empty, scn)
        {
        }

        #region Factory/Clone methods
        internal static BluezRfcommEndPoint CreateConnectEndPoint(BluetoothEndPoint localEP)
            => CreateBindEndPoint(localEP);

        internal static BluezRfcommEndPoint CreateBindEndPoint(BluetoothEndPoint serverEP)
        {
            // Win32 uses -1 for 'auto assign' but BlueZ uses 0.
            var port = serverEP.Port == -1 ? 0 : serverEP.Port;
            return new BluezRfcommEndPoint(serverEP.Address, port);
        }
        #endregion

        //----
        public override AddressFamily AddressFamily => AddressFamily32.BluetoothOnLinuxBlueZ;

        //----
        #region Create & Serialize methods
        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != AddressFamily)
                throw new ArgumentException("Wrong AddressFamily.");
            
            var tmpArr = BluezL2capEndPoint.CopyFromSa(socketAddress, ScnOffset, ScnLength);
            var psm = tmpArr[0];
            
            tmpArr = BluezL2capEndPoint.CopyFromSa(socketAddress, AddrOffset, 6);
            var addr = BluetoothAddress.CreateFromLittleEndian(tmpArr);
            
            return new BluezL2capEndPoint(addr, psm);
        }

        public override SocketAddress Serialize()
        {
            var sa = new SocketAddress(AddressFamily, SaLen);
            
            var scnByte = checked((byte)Port);
            BluezL2capEndPoint.CopyToSa([scnByte], sa, ScnOffset);
            BluezL2capEndPoint.CopyToSa(Address.ToByteArrayLittleEndian(), sa, AddrOffset);
            
            return sa;
        }
        #endregion


    }
}
