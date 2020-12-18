// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using List_IBluetoothDeviceInfo = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>;
using AsyncResultDD = InTheHand.Net.AsyncResult<
    System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>,
    InTheHand.Net.Bluetooth.Factory.DiscoDevsParams>;
//
using System;
using System.Diagnostics;
using System.Net.Sockets;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public abstract class CommonDiscoveryBluetoothClient : IBluetoothClient
    {
        // !!!
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member '
        //
        TimeSpan _inquiryLength = TimeSpan.FromSeconds(12);

        protected CommonDiscoveryBluetoothClient()
        {
        }

        //----------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // TODO Abort/Disallow device discovery
        }

        //----------------------------------------
        public IBluetoothDeviceInfo[] DiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly)
        {
            IAsyncResult ar = BeginDiscoverDevices(maxDevices,
                authenticated, remembered, unknown, discoverableOnly,
                null, null);
            return EndDiscoverDevices(ar);
        }

        public IAsyncResult BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state)
        {
            return BeginDiscoverDevices(maxDevices,
                authenticated, remembered, unknown, discoverableOnly,
                callback, state,
                null, null);
        }

        public IAsyncResult BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
            DateTime discoTime = DateTime.UtcNow;
            DiscoDevsParams args = new DiscoDevsParams(maxDevices, authenticated, remembered, unknown, discoverableOnly, discoTime);
            AsyncResultDD arDD = new AsyncResultDD(callback, state, args);
            //
            if (unknown || discoverableOnly) { // No need to do SLOW Inquiry when just want known remembered.
                BeginInquiry(maxDevices, DiscoDevs_InquiryCallback, arDD,
                    liveDiscoHandler, liveDiscoState, args);
            } else {
                arDD.SetAsCompleted(null, true);
            }
            return arDD;
        }

        protected abstract void BeginInquiry(int maxDevices,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void DiscoDevs_InquiryCallback(IAsyncResult ar)
        {
            AsyncResultDD arDD = (AsyncResultDD)ar.AsyncState;
#if !NETCF
            Debug.Assert(System.Threading.Thread.CurrentThread.IsThreadPoolThread,
                "!IsThreadPoolThread (type: " + this.GetType().FullName);
#endif
            arDD.SetAsCompletedWithResultOf(()=> EndInquiry(ar), false);
        }

        protected abstract List_IBluetoothDeviceInfo EndInquiry(IAsyncResult ar);

        public IBluetoothDeviceInfo[] EndDiscoverDevices(IAsyncResult asyncResult)
        {
            AsyncResultDD arDD = (AsyncResultDD)asyncResult;
            List_IBluetoothDeviceInfo discoverableDevices = arDD.EndInvoke();
            DiscoDevsParams args = arDD.BeginParameters;
            // DEBUG Iff 'known' devices only: we complete immediately (sync'ly), and with null result.
            if (args.unknown || args.discoverableOnly) {
                // Result from BeginInquiry callback expected
                Debug.Assert(discoverableDevices != null, "a1");
                Debug.Assert(!arDD.CompletedSynchronously, "a2"); // don't really care however
            } else {
                // Null result from BeginDD method expected.
                Debug.Assert(discoverableDevices == null, "b1");
                Debug.Assert(arDD.CompletedSynchronously, "b2");
            }
            //
            List_IBluetoothDeviceInfo knownDevices = GetKnownRemoteDeviceEntries();
            //
            List_IBluetoothDeviceInfo mergedDevices = BluetoothClient.DiscoverDevicesMerge(
                args.authenticated, args.remembered, args.unknown, knownDevices, discoverableDevices,
                args.discoverableOnly, args.discoTime);
            return mergedDevices.ToArray();
        }

        protected abstract List_IBluetoothDeviceInfo GetKnownRemoteDeviceEntries();

        public TimeSpan InquiryLength
        {
            get { return _inquiryLength; }
            set
            {
                if ((value.TotalSeconds > 0) && (value.TotalSeconds <= 60)) {
                    _inquiryLength = value;
                } else {
                    throw new ArgumentOutOfRangeException("value",
                        "QueryLength must be a positive timespan between 0 and 60 seconds.");
                }
            }
        }

        public int InquiryAccessCode
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Convert the user Inquiry parameters to the formats used by HCI.
        /// </summary>
        /// <param name="maxDevices">The <c>maxDevices</c> parameter from e.g.
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothClient.DiscoverDevices(System.Int32,System.Bool,System.Bool,System.Bool,System.Bool)"/>.
        /// </param>
        /// <param name="inquiryLength">The <see cref="P:InTheHand.Net.Sockets.BluetoothClient.InquiryLength"/> property
        /// <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>.
        /// </param>
        /// <param name="hciMaxResponses">On return contains the Num_Responses value to be passed to the HCI Inquiry command.
        /// If greater that 255 or less than zero, the value 0 will be returned.
        /// HCI uses zero as "Unlimited".
        /// </param>
        /// <param name="hciInquiryLength">On return contains the Inquiry_Length value to be passed to the HCI Inquiry command.
        /// Is scaled by the divisor 1.28secs
        /// and if not in range 1 to 0x30 inclusive is set to 10.
        /// </param>
        internal static void ConvertBthInquiryParams(int maxDevices, TimeSpan inquiryLength,
            out byte hciMaxResponses, out byte hciInquiryLength)
        {
            const byte MaxNumAny = 0;
            const byte MaxTimeUnspecified = 10;
            //
            const double Multiplier = 1.28;
            double tmp = inquiryLength.TotalSeconds / Multiplier;
            hciInquiryLength = checked((byte)tmp);
            if (hciInquiryLength < 1 || hciInquiryLength > 0x30) { // Previously wrongly 0 and 30decimal!!
                Debug.Fail("Invalid InquiryLength: " + inquiryLength + ", gives maxDurations: " + hciInquiryLength);
                hciInquiryLength = MaxTimeUnspecified;
            }
            if (maxDevices < 0 || maxDevices > byte.MaxValue) {
                hciMaxResponses = MaxNumAny;
            } else {
                hciMaxResponses = checked((byte)maxDevices);
            }
        }

        //--------------------------------------------------------------
        public abstract void Connect(BluetoothEndPoint remoteEP);
        public abstract IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state);
        public abstract void EndConnect(IAsyncResult asyncResult);
        //
        public abstract System.Net.Sockets.NetworkStream GetStream();
        public abstract LingerOption LingerState { get; set; }
        public abstract bool Connected { get; }
        //
        public abstract int Available { get; }
        public abstract System.Net.Sockets.Socket Client { get; set; }
        public abstract bool Authenticate { get; set; }
        public abstract bool Encrypt { get; set; }
        public abstract BluetoothEndPoint RemoteEndPoint { get; }
        public abstract string GetRemoteMachineName(BluetoothAddress device);
        public abstract Guid LinkKey { get; }
        public abstract LinkPolicy LinkPolicy { get; }
        public abstract string RemoteMachineName { get; }
        public abstract void SetPin(string pin);
        public abstract void SetPin(BluetoothAddress device, string pin);
    }
}
