using System;
using System.Net.Sockets;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    internal class BluezSocketOptionHelper : ISocketOptionHelper
    {
        readonly Socket _sock;
        private StackConsts.RFCOMM_LM _linkModeSetting;

        internal BluezSocketOptionHelper(Socket sock)
        {
            _sock = sock;
        }

        StackConsts.RFCOMM_LM ReadLinkMode()
        {
            var o = _sock.GetSocketOption(StackConsts.SOL_RFCOMM, StackConsts.so_RFCOMM_LM);
            var i = (int)o;
            var e = (StackConsts.RFCOMM_LM)i;
            Console.WriteLine("Read: {0} 0x{0:X}", e);
            return e;
        }

        private void SetOrClear(StackConsts.RFCOMM_LM bit, bool value)
        {
            _linkModeSetting &= ~bit; // Clear it firstt
            if (value) _linkModeSetting |= bit;
            Console.WriteLine("Setting: {0} 0x{0:X}", _linkModeSetting);
            _sock.SetSocketOption(StackConsts.SOL_RFCOMM, StackConsts.so_RFCOMM_LM, (int)_linkModeSetting);
        }

        //--
        private static bool IsSet(StackConsts.RFCOMM_LM value, StackConsts.RFCOMM_LM bit)
        {
            return 0 != (value & bit);
        }

        #region ISocketOptionHelper Members

        public bool Authenticate
        {
            get { return IsSet(ReadLinkMode(), StackConsts.RFCOMM_LM.Auth); }
            set { SetOrClear(StackConsts.RFCOMM_LM.Auth, value); }
        }

        public bool Encrypt
        {
            get { return IsSet(ReadLinkMode(), StackConsts.RFCOMM_LM.Encrypt); }
            set { SetOrClear(StackConsts.RFCOMM_LM.Encrypt, value); }
        }

        public void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
