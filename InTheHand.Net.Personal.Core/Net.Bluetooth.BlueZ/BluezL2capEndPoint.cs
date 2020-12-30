using System;
using System.Net;
using System.Net.Sockets;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    internal class BluezL2capEndPoint : BluetoothEndPoint
    {
        #region sockaddr_l2 layout
        /*
         * ==== l2cap.h ====
         * /* L2CAP socket address *--/
         * struct sockaddr_l2 {
         * 	sa_family_t	l2_family;
         * 	unsigned short	l2_psm;
         * 	bdaddr_t	l2_bdaddr;
         * 	unsigned short	l2_cid;
         * };
         * 
         */
        const int PsmOffset = 2;
        const int PsmLength = 2;
        const int AddrOffset = 4;
        const int CidOffset = 10;
        const int SaLen = 2 + PsmLength + 6 + /*(2) +*/ 2;
        // 12 or 14??
        // e.g. SA @ BzL2Ep.Create IN: family: 31 0x0000001F, size: 14, < 1F 00 F5 56 BB 65 68 3A 0A 00 8A 00 00 00 >
        #endregion

        //----
        public BluezL2capEndPoint(BluetoothAddress address, int psm)
            : base(address, BluetoothService.Empty, psm)
        {
        }

        #region Factory/Clone methods
        internal static BluezL2capEndPoint CreateConnectEndPoint(BluetoothEndPoint localEP)
        {
            return CreateBindEndPoint(localEP);
        }

        internal static BluezL2capEndPoint CreateBindEndPoint(BluetoothEndPoint serverEP)
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
            var l2capEp = new BluezL2capEndPoint(serverEP.Address, port);
            return l2capEp;
        }
        #endregion

        //----
        public override AddressFamily AddressFamily { get { return AddressFamily32.BluetoothOnLinuxBlueZ; } }

        //----
        #region Create & Serialize methods
        public override EndPoint Create(SocketAddress socketAddress)
        {
            Dump("BzL2Ep.Create IN", socketAddress);
            if (socketAddress.Family != AddressFamily)
                throw new ArgumentException("Wrong AddressFamily.");
            if (socketAddress.Size < SaLen)
                throw new ArgumentException("Too short sockaddr_l2 expected at least "
                    + SaLen + ", but was: " + socketAddress.Size + ".");
            byte[] tmpArr;
            //
            tmpArr = CopyFromSa(socketAddress, PsmOffset, PsmLength);
            var tmpI16 = BitConverter.ToInt16(tmpArr, 0);
            //tmpI16 = IPAddress.NetworkToHostOrder(tmpI16);
            var psm = unchecked((UInt16)tmpI16);
            //
            tmpArr = CopyFromSa(socketAddress, AddrOffset, 6);
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
            var psmU16_ = checked((UInt16)Port);
            var tmpI16 = unchecked((Int16)psmU16_);
            //Console.WriteLine("tmpI16 a: 0x" + tmpI16.ToString("X"));
            //tmpI16 = IPAddress.HostToNetworkOrder(tmpI16);
            //Console.WriteLine("tmpI16 b: 0x" + tmpI16.ToString("X"));
            tmpArr = BitConverter.GetBytes(tmpI16);
            //Console.WriteLine("tmpArr: " + BitConverter.ToString(tmpArr));
            CopyToSa(tmpArr, sa, PsmOffset);
            //
            tmpArr = Address.ToByteArrayLittleEndian();
            CopyToSa(tmpArr, sa, AddrOffset);
            //
            Dump("BzL2Ep.Serialize", sa);
            return sa;
        }
        #endregion

        #region SocketAddress array accessors
        internal static byte[] CopyFromSa(SocketAddress sa, int saOffset, int len)
        {
            byte[] tmpArr = new byte[len];
            for (int i = 0; i < tmpArr.Length; ++i) {
                tmpArr[i] = sa[i + saOffset];
            }
            return tmpArr;
        }

        internal static void CopyToSa(byte[] arr, SocketAddress sa, int saOffset)
        {
            for (int i = 0; i < arr.Length; ++i) {
                sa[i + saOffset] = arr[i];
            }
        }
        #endregion

    }
}
