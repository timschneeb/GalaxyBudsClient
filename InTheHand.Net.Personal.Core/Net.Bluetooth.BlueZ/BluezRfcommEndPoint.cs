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
        {
            return CreateBindEndPoint(localEP);
        }

        internal static BluezRfcommEndPoint CreateBindEndPoint(BluetoothEndPoint serverEP)
        {
            int port;
            // Win32 uses -1 for 'auto assign' but BlueZ uses 0.
            if (serverEP.Port == -1) { // TODO ! Test on L2CAP
                port = 0;
                //TODO } else if (serverEP.Port > BluetoothEndPoint.MaxScn PSM!!) {
                //    // BlueZ doesn't complain in this case!  Do't know what it does...
                //    throw new SocketException((int)SocketError.AddressNotAvailable);
            } else {
                port = serverEP.Port;
            }
            var l2capEp = new BluezRfcommEndPoint(serverEP.Address, port);
            return l2capEp;
        }
        #endregion

        //----
        public override AddressFamily AddressFamily { get { return AddressFamily32.BluetoothOnLinuxBlueZ; } }

        //----
        #region Create & Serialize methods
        #region Create & Serialize methods
        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != AddressFamily)
                throw new ArgumentException("Wrong AddressFamily.");
            byte[] tmpArr;
            //
            tmpArr = BluezL2capEndPoint.CopyFromSa(socketAddress, ScnOffset, ScnLength);
            var tmpByte = tmpArr[0];
            var psm = tmpByte;
            //
            tmpArr = BluezL2capEndPoint.CopyFromSa(socketAddress, AddrOffset, 6);
            //
            var addr = BluetoothAddress.CreateFromLittleEndian(tmpArr);
            var ep = new BluezL2capEndPoint(addr, psm);
            return ep;
        }

        public override SocketAddress Serialize()
        {
            var sa = new SocketAddress(AddressFamily, SaLen);
            byte[] tmpArr;
            //
            var scnByte = checked((byte)Port);
            tmpArr = BitConverter.GetBytes(scnByte);
            BluezL2capEndPoint.CopyToSa(tmpArr, sa, ScnOffset);
            //
            tmpArr = Address.ToByteArrayLittleEndian();
            BluezL2capEndPoint.CopyToSa(tmpArr, sa, AddrOffset);
            //
            return sa;
        }
        #endregion

        #endregion
    }
}
