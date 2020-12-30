// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.WidcommBluetoothClient
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Generic;
using List_IBluetoothDeviceInfo = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>;
using AsyncResultDD = InTheHand.Net.AsyncResult<
    System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>,
    InTheHand.Net.Bluetooth.Factory.DiscoDevsParams>;
using System.Threading;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    /// <summary>
    /// Provides client connections for Bluetooth network services with Widcomm stack.
    /// </summary>
    internal sealed class WidcommBluetoothClient : InTheHand.Net.Bluetooth.Factory.CommonBluetoothClient
    {
        readonly WidcommBluetoothFactoryBase m_factory;
        readonly WidcommRfcommStreamBase m_connRef;
        //
        string m_passcode;

        //--------------
        // This pair of constructors are required to allow us to keep a 
        // reference to the conn (WidcommRfcommStream).
        internal WidcommBluetoothClient(WidcommBluetoothFactoryBase factory)
            : this(factory, factory.GetWidcommRfcommStream())
        { }
        // This pair of constructors are required to allow us to keep a 
        // reference to the conn (WidcommRfcommStream).
        internal WidcommBluetoothClient(WidcommBluetoothFactoryBase factory, WidcommRfcommStreamBase conn)
            : base(factory, conn)
        {
            Debug.Assert(factory != null, "factory must not be null; is used by GetRemoteMachineName etc.");
            m_factory = factory;
            m_connRef = conn;
            //TEST_EARLY: m_btIf___HACK = m_btIf;
        }


        internal WidcommBluetoothClient(BluetoothEndPoint localEP, WidcommBluetoothFactoryBase factory)
            : this(factory)
        {
            // All we could do is fail if the specified local address isn't the
            // (sole) Widcomm Radio's address.  We also can't tell Widcomm to
            // bind to a specified local port so all we could do is fail if the
            // specified port is not 'unspecified' (i.e. 0/-1).
            throw new NotSupportedException("Don't support binding to a particular local address/port.");
        }

        /// <summary>
        /// Used by WidcommBluetoothListener to return the newly accepted connection.
        /// </summary>
        /// -
        /// <param name="strm">The WidcommRfcommStream containing the newly connected 
        /// RfCommPort.
        /// </param>
        /// <param name="factory">Factory to use in GetRemoteMachineName etc.
        /// </param>
        internal WidcommBluetoothClient(WidcommRfcommStreamBase strm, WidcommBluetoothFactoryBase factory)
            : base(factory, strm)
        {
            Debug.Assert(factory != null, "factory must not be null; is used by GetRemoteMachineName etc.");
            m_factory = factory;
            //m_conn = strm;
        }

        private WidcommBtInterface BtIf
        {
            [DebuggerStepThrough]
            get { return m_factory.GetWidcommBtInterface(); }
        }

        //--------

        IAsyncResult ConnBeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            return m_connRef.BeginConnect(remoteEP, m_passcode, requestCallback, state);
        }

        //----
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
                int[] ports = recBuf.Hack_GetPorts();
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
            m_passcode = pin;
        }

        public override void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException("Use this.SetPin or BluetoothSecurity.PairRequest...");
        }

        //--------------------------------------------------------------
#if NETCF
        // No support for CBtIf::GetRemoteDeviceInfo on my iPAQ.
        static bool s_useRegistryForGetKnownRemoteDevice = true;
#else
        // No way to lookup *all* known devices, need specific CoD value!
        static bool s_useRegistryForGetKnownRemoteDevice = true;
#endif

        /// <summary>
        /// ... Allow the tests to disable the Registry lookup.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by unit-tests.")]
        internal static bool ReadKnownDeviceFromTheRegistry
        {
            set { s_useRegistryForGetKnownRemoteDevice = value; }
            get { return s_useRegistryForGetKnownRemoteDevice; }
        }

        protected override List_IBluetoothDeviceInfo GetKnownRemoteDeviceEntries()
        {
            List_IBluetoothDeviceInfo knownDevices;
            if (s_useRegistryForGetKnownRemoteDevice)
                knownDevices = BtIf.ReadKnownDevicesFromRegistry();
            else
                knownDevices = BtIf.GetKnownRemoteDeviceEntries();
            return knownDevices;
        }

        //----
        protected override void BeginInquiry(int maxDevices,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            IAsyncResult ar = BtIf.BeginInquiry(maxDevices, InquiryLength,
                callback, state, 
                liveDiscoHandler, liveDiscoState,
                args);
        }

        protected override List_IBluetoothDeviceInfo EndInquiry(IAsyncResult ar)
        {
            List_IBluetoothDeviceInfo discoverableDevices = BtIf.EndInquiry(ar);
            return discoverableDevices;
        }

    }

}
