// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.L2CapPort.cs
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal sealed class L2CapPort : IRfcommPort
    {
        IRfCommIf _intf;
        //
        IntPtr m_pPort;
        WidcommRfcommStreamBase m_parent;
        //
        // Stop the delegates being GC'd, as the native code is calling their thunks.
        WidcommNativeBits.RfcommPort_DataReceivedCallbackDelegate m_handleDataReceived;
        WidcommL2CapClient.L2CapConn_EventReceivedCallbackDelegate m_handleEvent;

        //----
        internal L2CapPort(IRfCommIf intf)
        {
            _intf = intf;
        }

        //----
        public void SetParentStream(WidcommRfcommStreamBase parent)
        {
            if (m_parent != null)
                throw new InvalidOperationException("Can only have one parent.");
            m_parent = parent;
        }

        public void Create()
        {
            m_handleDataReceived = HandleDataReceived;
            m_handleEvent = HandleEventReceived;
            WidcommL2CapClient.NativeMethods.L2CapConn_Create(out m_pPort, m_handleDataReceived, m_handleEvent);
            Utils.MiscUtils.Trace_WriteLine("WidcommRfcommPort.Create'd: " + DebugId);
            if (m_pPort == IntPtr.Zero)
                throw new InvalidOperationException("Native object creation failed.");
        }

        public string DebugId
        {
            get
            {
                if (m_pPort == IntPtr.Zero)
                    Utils.MiscUtils.Trace_WriteLine("Can't call get_DebugId before initialised.");
                return m_pPort.ToInt64().ToString("X");
            }
        }

        void HandleDataReceived(IntPtr buffer, UInt16 len)
        {
            Utils.MiscUtils.Trace_WriteLine("HandleReceive: len: {0}", len);
            byte[] arr = WidcommUtils.GetByteArray(buffer, len);
            m_parent.HandlePortReceive(arr, this);
        }

        void HandleEventReceived(UInt32 eventId0, UInt32 data)
        {
            var eventIdA = (WidcommL2CapClient.MyL2CapEvent)eventId0;
            PORT_EV? eventId = ConvertEvent(eventIdA, data);
            Utils.MiscUtils.Trace_WriteLine("{0} HandleEvent: was: '{1}'=0x{2:X} data: 0x{3:X}, mapped to: '{4}'",
                DateTime.Now.TimeOfDay.ToString(),
                eventIdA, eventId0, data, eventId);
            if (eventIdA == WidcommL2CapClient.MyL2CapEvent.IncomingConnection_Pending) {
                Accept();
            }
            if (!eventId.HasValue)
                return;
            m_parent.HandlePortEvent((PORT_EV)eventId, this);
        }

        private static PORT_EV? ConvertEvent(
            WidcommL2CapClient.MyL2CapEvent eventId, UInt32 data)
        {
            PORT_EV? eventIdOut = null;
            switch (eventId) {
                case WidcommL2CapClient.MyL2CapEvent.IncomingConnection_Pending:
                    // Incoming Pending! All we do is call Accept, and once the
                    // connection is actually made we'll get the 'Connected' event.
                    break;
                case WidcommL2CapClient.MyL2CapEvent.ConnectPendingReceived:
                    break;
                case WidcommL2CapClient.MyL2CapEvent.Connected:
                    eventIdOut = PORT_EV.CONNECTED;
                    break;
                case WidcommL2CapClient.MyL2CapEvent.CongestionStatus:
                    var isCongested = Convert.ToBoolean(data);
                    if (!isCongested) {
                        eventIdOut = PORT_EV.TXEMPTY;
                        //Utils.MiscUtils.Trace_WriteLine("  CongestionStatus isCongested=False -> TXEMPTY");
                    } else {
                        //Utils.MiscUtils.Trace_WriteLine("  CongestionStatus isCongested=True  -> null");
                    }
                    break;
                case WidcommL2CapClient.MyL2CapEvent.RemoteDisconnected:
                    eventIdOut = PORT_EV.CONNECT_ERR;
                    var reason = (L2CapDisconnectReason)data;
                    Utils.MiscUtils.Trace_WriteLine("  RemoteDisconnected reason: " + reason);
                    break;
                default:
                    break;
            }
            return eventIdOut;
        }

        public PORT_RETURN_CODE OpenClient(int scn__, byte[] address)
        {
            if (address == null || address.Length != 6)
                throw new ArgumentException("Parameter 'address' must be non-null and six-bytes long.");
            PORT_RETURN_CODE ret = ConvertResult(WidcommL2CapClient.NativeMethods.L2CapConn_Connect(m_pPort, _intf.PObject, address));
            Utils.MiscUtils.Trace_WriteLine("WidcommL2CapClient.NativeMethods.L2CapConn_OpenClient ret: {0}=0x{0:X}", ret);
            return ret;
        }

        public PORT_RETURN_CODE OpenServer(int scn__)
        {
            PORT_RETURN_CODE ret = ConvertResult(WidcommL2CapClient.NativeMethods.L2CapConn_Listen(m_pPort, _intf.PObject));
            Utils.MiscUtils.Trace_WriteLine("WidcommL2CapClient.NativeMethods.L2CapConn_OpenServer ret: {0}=0x{0:X}", ret);
            return ret;
        }

        private static PORT_RETURN_CODE ConvertResult(bool p)
        {
            if (p)
                return PORT_RETURN_CODE.SUCCESS;
            return PORT_RETURN_CODE.UNKNOWN_ERROR;
        }

        public void Accept()
        {
            // TODO Presumably this is allowed on the Widcomm thread??
            bool success = WidcommL2CapClient.NativeMethods.L2CapConn_Accept(m_pPort);
            Debug.Assert(success, "L2CapConn_Accept fail.");
        }

        public PORT_RETURN_CODE Write(byte[] data, ushort lenToWrite, out ushort lenWritten)
        {
            return ConvertResult(WidcommL2CapClient.NativeMethods.L2CapConn_Write(m_pPort, data, lenToWrite, out lenWritten));
        }

        public bool IsConnected(out BluetoothAddress p_remote_bdaddr)
        {
            byte[] bdaddr = new byte[WidcommStructs.BD_ADDR_LEN];
            WidcommL2CapClient.NativeMethods.L2CapConn_GetRemoteBdAddr(m_pPort, bdaddr, bdaddr.Length);
            p_remote_bdaddr = WidcommUtils.ToBluetoothAddress(bdaddr);
            return true;
        }

        public PORT_RETURN_CODE Close()
        {
            Utils.MiscUtils.Trace_WriteLine("L2CapPort.Close(): " + DebugId);
            WidcommL2CapClient.NativeMethods.L2CapConn_Disconnect(m_pPort);
            return PORT_RETURN_CODE.SUCCESS;
        }

        public void Destroy()
        {
            Utils.MiscUtils.Trace_WriteLine("L2CapPort.Destroy(): " + DebugId);
            Debug.Assert(m_pPort != IntPtr.Zero, "WidcommRfcommPort Already Destroyed");
            if (m_pPort != IntPtr.Zero) {
                WidcommL2CapClient.NativeMethods.L2CapConn_Destroy(m_pPort);
                m_pPort = IntPtr.Zero;
            }
        }

        enum L2CapDisconnectReason
        {
            DisconnectInd = 0,
            // -- SIG, and BtIfDefinitions.h --
            // - Both
            /// <summary>
            /// Presumably this is surfaced as a OnConnectionPending
            /// </summary>
            XXPending = 1,
            PsmNotSupported = 2,
            SecurityBlock = 3,
            NoResources = 4,
            // -- BtIfDefinitions.h --
            Timeout = 0xEEEE,
            // - Win32
            CfgUnacceptable_params = 5,       // added SDK 1.8.0.900
            CfgFailedNoReason = 6,
            CfgUnknownOptions = 7,
            // - WCE
            LocalPowerOff = 0xFFFE,          // added SDK 1.5.0, BTW-CE 1.5.0
            // - Both
            NoLink = 255,        /* Add a couple of our own for internal use */
        }

        //----
        internal int GetMtu()
        {
            bool isCongested;
            UInt16 mtu;
            WidcommL2CapClient.NativeMethods.L2CapConn_GetProperties(m_pPort,
                out isCongested, out mtu);
            return mtu;
        }
    }
}
