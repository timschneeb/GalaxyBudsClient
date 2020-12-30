// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommL2CapClient.cs
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using PL2CapConn = System.IntPtr;
using PL2CapIf = System.IntPtr;
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    class WidcommL2CapClient : CommonBluetoothClient, IL2CapClient
    {

        public static IL2CapClient Create()
        {
            WidcommBluetoothFactoryBase wf = WidcommBluetoothFactory.GetWidcommIfExists();
            var cli = new WidcommL2CapClient(wf);
            return cli;
        }

        //======
        readonly WidcommBluetoothFactoryBase m_factory;
        readonly WidcommL2CapStream m_connRef;

        //--------------
        // HACK L2CAP parts creation move to factory.
        private static WidcommL2CapStream factory_GetWidcommL2CapStream(WidcommBluetoothFactoryBase fcty)
        {
            var intf = GetWidcommL2CapIf(fcty);
            var port = GetWidcommL2CapPort(intf);
            return new WidcommL2CapStream(port, intf, fcty);
        }
        internal static WidcommL2CapStream GetWidcommL2CapStreamWithThisIf(
            WidcommBluetoothFactoryBase fcty, IRfCommIf intf)
        {
            return new WidcommL2CapStream(GetWidcommL2CapPort(intf), null, fcty);
        }
        internal static IRfCommIf GetWidcommL2CapIf(WidcommBluetoothFactoryBase fcty)
        {
            var intf = new L2CapIf();
            var intfST = new WidcommStRfCommIf(fcty, intf);
            return intfST;
        }
        private static IRfcommPort GetWidcommL2CapPort(IRfCommIf intf) { return new L2CapPort(intf); }

        internal static IBluetoothClient factory_DoGetBluetoothClientForListener(
            WidcommBluetoothFactoryBase fcty, CommonRfcommStream strm)
        {
            return new WidcommL2CapClient(fcty, strm);
        }

        //----

        // This pair of constructors are required to allow us to keep a 
        // reference to the conn (WidcommRfcommStream).
        internal WidcommL2CapClient(WidcommBluetoothFactoryBase factory)
            : this(factory, factory_GetWidcommL2CapStream(factory))
        { }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "localEP")]
        internal WidcommL2CapClient(WidcommBluetoothFactoryBase factory, BluetoothEndPoint localEP)
            : this(factory)
        {
            // All we could do is fail if the specified local address isn't the
            // (sole) Widcomm Radio's address.  We also can't tell Widcomm to
            // bind to a specified local port so all we could do is fail if the
            // specified port is not 'unspecified' (i.e. 0/-1).
            throw new NotSupportedException("Don't support binding to a particular local address/port.");
        }

        internal WidcommL2CapClient(WidcommBluetoothFactoryBase factory, CommonRfcommStream conn)
            : base(factory, conn)
        {
            Debug.Assert(factory != null, "factory must not be null; is used by GetRemoteMachineName etc.");
            m_factory = factory;
            m_connRef = (WidcommL2CapStream)conn; // CAST!!
        }

        private WidcommBtInterface BtIf
        {
            [DebuggerStepThrough]
            get { return m_factory.GetWidcommBtInterface(); }
        }

        //--------
        public int GetMtu()
        {
            return m_connRef.GetMtu();
        }

        //--------

        public override IAsyncResult BeginServiceDiscovery(
            BluetoothAddress address, Guid serviceGuid,
            AsyncCallback asyncCallback, Object state)
        {
            IAsyncResult ar2 = BtIf.BeginServiceDiscovery(address, serviceGuid, SdpSearchScope.ServiceClassOnly,
                asyncCallback, state);
            return ar2;
        }

        public override List<int> EndServiceDiscovery(IAsyncResult ar)
        {
            using (ISdpDiscoveryRecordsBuffer recBuf = BtIf.EndServiceDiscovery(ar)) {
                var portList = new List<int>();
                int[] ports = recBuf.Hack_GetPsms();
                Utils.MiscUtils.Trace_WriteLine("_GetPorts, got {0} records.", recBuf.RecordCount);
#if DEBUG
                if (ports.Length == 0) {
                    Math.Abs(1); // COVERAGE
                } else {
                    Math.Abs(1); // COVERAGE
                }
#endif
                // Do this in reverse order, as Widcomm appears to keep old
                // (out of date!!) service records around, so we want to 
                // use the newests ones in preference.
                for (int i = ports.Length - 1; i >= 0; --i) {
                    int cur = ports[i];
                    portList.Add(cur);
                }//for
                return portList;
            }
        }

        //----
        public override void SetPin(string pin)
        {
            throw new NotImplementedException();
        }

        public override void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException("Use this.SetPin or BluetoothSecurity.PairRequest...");
        }

        //--------------------------------------------------------------
        protected override List<IBluetoothDeviceInfo> GetKnownRemoteDeviceEntries()
        {
            throw new NotSupportedException();
        }
        protected override void BeginInquiry(int maxDevices,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            throw new NotSupportedException();
        }
        protected override List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            throw new NotSupportedException();
        }

        //----------------------------
        internal delegate void L2CapConn_EventReceivedCallbackDelegate(UInt32 eventId, UInt32 data);

        internal enum MyL2CapEvent
        {
            IncomingConnection_Pending,
            ConnectPendingReceived,
            Connected,
            CongestionStatus,
            RemoteDisconnected
        }

        internal static class NativeMethods
        {

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapIf_Create(out PL2CapIf ppObj);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapIf_Destroy(PL2CapIf pObj);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapIf_AssignPsmValue(PL2CapIf pObj,
                ref Guid p_service_guid, UInt16 psm);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern UInt16 L2CapIf_GetPsm(PL2CapIf pObj);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapIf_SetSecurityLevel(PL2CapIf pObj,
                // TODO change to byte[]
                string p_service_name, BTM_SEC security_level,
                [MarshalAs(UnmanagedType.Bool)] bool is_server);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapIf_Register(PL2CapIf pObj);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapIf_Deregister(PL2CapIf pObj);
            //
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapConn_Create(out PL2CapConn ppObj,
                WidcommNativeBits.RfcommPort_DataReceivedCallbackDelegate handleDataReceived,
                L2CapConn_EventReceivedCallbackDelegate handleEvent);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapConn_Destroy(PL2CapConn pObj);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapConn_Connect(PL2CapConn pObj, PL2CapIf pIf, byte[] pAddr);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapConn_Listen(PL2CapConn pObj, PL2CapIf pIf);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapConn_Accept(IntPtr _pIf);
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool L2CapConn_Write(PL2CapConn pObj, byte[] p_data,
                UInt16 len_to_write, out UInt16 p_len_written);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapConn_Disconnect(PL2CapConn pObj);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapConn_GetRemoteBdAddr(PL2CapConn pObj,
                byte[] p_remote_bdaddr, int bufLen);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void L2CapConn_GetProperties(PL2CapConn pObj,
                [MarshalAs(UnmanagedType.Bool)] out bool pIsCongested,
                out UInt16 pMtu
                //, OUT UINT16 *pCid
                );

            /*
             * BTW DK -- BtIfDefinitions.h
#define L2CAP_MIN_MTU                   (48)

//Zong, need to talk to ask to see if we can make them the same
#ifdef _WIN32_WCE
    #define L2CAP_MAX_MTU               1400
    #define L2CAP_DEFAULT_MTU           672
#else
//    Note:  The following is not defined because: a) it is not
//           used in the SDK source code, and b) to avoid a redefinition
//           warning when used with the Windows Bluetooth SDK. (CR 14046)
//    #define L2CAP_MAX_MTU               1696      // 
    #define L2CAP_DEFAULT_MTU           (672)
    #define BNEP_MIN_MTU_SIZE           1691
#endif

#define L2CAP_DEFAULT_FLUSH_TO          0xFFFF
             * 
             * BTW-CE DK
#define L2CAP_MIN_MTU                   48

#ifdef _WIN32_WCE
    #define L2CAP_DEFAULT_MTU           672
#else
    #define L2CAP_DEFAULT_MTU           1691
#endif

#define L2CAP_MAX_MTU               1696            // changed from 1400, SDK 1.5.0
#define BNEP_MIN_MTU_SIZE           1691

#define L2CAP_DEFAULT_FLUSH_TO          0xFFFF
             */
        }//class

        //--------
        internal sealed class WidcommL2CapStream : WidcommRfcommStreamBase
        {
            readonly L2CapPort _portRef;

            internal WidcommL2CapStream(IRfcommPort port, IRfCommIf rfCommIf, WidcommBluetoothFactoryBase factory)
                : base(port, rfCommIf, factory)
            {
                if (port == null) throw new ArgumentNullException("port");
                _portRef = (L2CapPort)port; // CAST!!
            }

            protected override void VerifyPortIsInRange(BluetoothEndPoint bep)
            {
                var psm = checked((ushort)bep.Port);
            }

            internal int GetMtu()
            {
                return _portRef.GetMtu();
            }
        }//class2

    }
}
