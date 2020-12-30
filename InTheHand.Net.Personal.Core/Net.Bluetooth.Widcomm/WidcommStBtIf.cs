// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommStBtIf
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    class WidcommStBtIf : IBtIf
    {
        readonly IBtIf _child;
        readonly WidcommPortSingleThreader _st;

        public WidcommStBtIf(WidcommBluetoothFactoryBase factory, IBtIf child)
        {
            _st = factory.GetSingleThreader();
            _child = child;
            Debug.Assert(_st != null, "_st != null");
        }

        #region IBtIf Members

        public void SetParent(WidcommBtInterface parent)
        {
            _child.SetParent(parent);
        }

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
            if (!disposing) // If Finalizing, may be that the thread is dead.
                return;
            if (!WidcommBtInterface.IsWidcommSingleThread(_st)) {
                WidcommPortSingleThreader.MiscNoReturnCommand cmd = _st.AddCommand(
                    new WidcommPortSingleThreader.MiscNoReturnCommand(delegate {
                    _child.Destroy(disposing);
                }));
                cmd.WaitCompletion();
            } else {
                _child.Destroy(disposing);
            }
        }

        public bool StartInquiry()
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.StartInquiry();
            }));
            return cmd.WaitCompletion();
        }

        public void StopInquiry()
        {
            WidcommPortSingleThreader.MiscNoReturnCommand cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscNoReturnCommand(delegate {
                _child.StopInquiry();
            }));
            cmd.WaitCompletion();
        }

        public bool StartDiscovery(BluetoothAddress address, Guid serviceGuid)
        {
            //return _child.StartDiscovery(address, serviceGuid);
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.StartDiscovery(address, serviceGuid);
            }));
            return cmd.WaitCompletion();
        }

        public DISCOVERY_RESULT GetLastDiscoveryResult(out BluetoothAddress address, out ushort p_num_recs)
        {
            return _child.GetLastDiscoveryResult(out address, out p_num_recs);
        }

        public ISdpDiscoveryRecordsBuffer ReadDiscoveryRecords(BluetoothAddress address, int maxRecords, ServiceDiscoveryParams args)
        {
            return _child.ReadDiscoveryRecords(address, maxRecords, args);
        }

        public REM_DEV_INFO_RETURN_CODE GetRemoteDeviceInfo(ref REM_DEV_INFO remDevInfo, IntPtr p_rem_dev_info, int cb)
        {
            return _child.GetRemoteDeviceInfo(ref remDevInfo, p_rem_dev_info, cb);
        }

        public REM_DEV_INFO_RETURN_CODE GetNextRemoteDeviceInfo(ref REM_DEV_INFO remDevInfo, IntPtr p_rem_dev_info, int cb)
        {
            return _child.GetNextRemoteDeviceInfo(ref remDevInfo, p_rem_dev_info, cb);
        }

        public bool GetLocalDeviceVersionInfo(ref DEV_VER_INFO devVerInfo)
        {
            return _child.GetLocalDeviceVersionInfo(ref devVerInfo);
        }

        public bool GetLocalDeviceInfoBdAddr(byte[] bdAddr)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.GetLocalDeviceInfoBdAddr(bdAddr);
            }));
            return cmd.WaitCompletion();
        }

        public bool GetLocalDeviceName(byte[] bdName)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.GetLocalDeviceName(bdName);
            }));
            return cmd.WaitCompletion();
        }

        public void IsStackUpAndRadioReady(out bool stackServerUp, out bool deviceReady)
        {
            var cmd = _st.AddCommand(new WidcommPortSingleThreader.MiscReturnCommand<HoldStackDevice>(delegate {
                HoldStackDevice resultI = new HoldStackDevice();
                _child.IsStackUpAndRadioReady(out resultI.stackServerUp, out resultI.deviceReady);
                return resultI;
            }));
            var resultO = cmd.WaitCompletion();
            stackServerUp = resultO.stackServerUp;
            deviceReady = resultO.deviceReady;
        }

        class HoldStackDevice
        {
            public bool stackServerUp, deviceReady;
        }

        public void IsDeviceConnectableDiscoverable(out bool conno, out bool disco)
        {
            WidcommPortSingleThreader.MiscReturnCommand<HoldConnoDisco> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<HoldConnoDisco>(delegate {
                HoldConnoDisco resultI = new HoldConnoDisco();
                _child.IsDeviceConnectableDiscoverable(out resultI.conno, out resultI.disco);
                return resultI;
            }));
            HoldConnoDisco resultO = cmd.WaitCompletion();
            conno = resultO.conno;
            disco = resultO.disco;
        }

        public void SetDeviceConnectableDiscoverable(bool connectable, bool forPairedOnly, bool discoverable)
        {
            //TODO implement me!!! SetDeviceConnectableDiscoverable
            //Debug.Fail("implement me!!!");
            _child.SetDeviceConnectableDiscoverable(connectable, forPairedOnly, discoverable);
        }

        class HoldConnoDisco
        {
            public bool conno, disco;
        }

        public int GetRssi(byte[] bd_addr)
        {
            WidcommPortSingleThreader.MiscReturnCommand<int> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<int>(delegate {
                return _child.GetRssi(bd_addr);
            }));
            return cmd.WaitCompletion();
        }

        public bool BondQuery(byte[] bd_addr)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.BondQuery(bd_addr);
            }));
            return cmd.WaitCompletion();
        }

        public BOND_RETURN_CODE Bond(BluetoothAddress address, string passphrase)
        {
            WidcommPortSingleThreader.MiscReturnCommand<BOND_RETURN_CODE> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<BOND_RETURN_CODE>(delegate {
                return _child.Bond(address, passphrase);
            }));
            return cmd.WaitCompletion();
        }

        public bool UnBond(BluetoothAddress address)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.UnBond(address);
            }));
            return cmd.WaitCompletion();
        }

        public WBtRc GetExtendedError()
        {
            WidcommPortSingleThreader.MiscReturnCommand<WBtRc> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<WBtRc>(delegate {
                return _child.GetExtendedError();
            }));
            return cmd.WaitCompletion();
        }

        public SDK_RETURN_CODE IsRemoteDevicePresent(byte[] bd_addr)
        {
            WidcommPortSingleThreader.MiscReturnCommand<SDK_RETURN_CODE> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<SDK_RETURN_CODE>(delegate {
                return _child.IsRemoteDevicePresent(bd_addr);
            }));
            return cmd.WaitCompletion();
        }

        public bool IsRemoteDeviceConnected(byte[] bd_addr)
        {
            WidcommPortSingleThreader.MiscReturnCommand<bool> cmd = _st.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<bool>(delegate {
                return _child.IsRemoteDeviceConnected(bd_addr);
            }));
            return cmd.WaitCompletion();
        }

        #endregion
    }
}
