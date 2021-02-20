// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    class WidcommStRfCommIf : IRfCommIf
    {
        readonly IRfCommIf _child;
        readonly WidcommPortSingleThreader _st;

        public WidcommStRfCommIf(WidcommBluetoothFactoryBase factory, IRfCommIf child)
        {
            _st = factory.GetSingleThreader();
            _child = child;
        }

        #region IRfCommIf Members

        public IntPtr PObject { get { return _child.PObject; } }

        public void Create()
        {
            WidcommPortSingleThreader.MiscNoReturnCommand cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscNoReturnCommand(delegate {
                _child.Create();
            }));
            cmd.WaitCompletion();
        }

        public void Destroy(bool disposing)
        {
            WidcommPortSingleThreader.MiscNoReturnCommand cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscNoReturnCommand(delegate {
                _child.Destroy(disposing);
            }));
            cmd.WaitCompletion(disposing);
        }

        public bool ClientAssignScnValue(Guid serviceGuid, int scn)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.ClientAssignScnValue(serviceGuid, scn);
            }));
            return cmd.WaitCompletion();
        }

        public bool SetSecurityLevel(byte[] p_service_name, BTM_SEC securityLevel, bool isServer)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.SetSecurityLevel(p_service_name, securityLevel, isServer);
            }));
            return cmd.WaitCompletion();
        }

        public int GetScn()
        {
            WidcommPortSingleThreader.MiscReturnCommand<int> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<int>(delegate {
                return _child.GetScn();
            }));
            return cmd.WaitCompletion();
        }

        #endregion
    }
}
