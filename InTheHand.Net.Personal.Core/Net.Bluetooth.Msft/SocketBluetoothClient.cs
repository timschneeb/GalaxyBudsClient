// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BluetoothClient
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

//#define WIN32_READ_BTH_DEVICE_INFO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;
using InTheHand.Runtime.InteropServices;
using AsyncResult_BeginDiscoverDevices = InTheHand.Net.AsyncResult<
    InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo[],
    InTheHand.Net.Bluetooth.Msft.SocketBluetoothClient.DiscoDevsParamsWithLive>;
using List_IBluetoothDeviceInfo = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>;

namespace InTheHand.Net.Bluetooth.Msft
{
    class SocketBluetoothClient : IBluetoothClient
    {
        readonly BluetoothFactory _fcty;
        private bool cleanedUp = false;
        private ISocketOptionHelper m_optionHelper;
#if WinXP
        // If SetPin(String) is called before connect we need to know the remote 
        // address to start the BluetoothWin32Authenticator for, so store this 
        // so we can start the authenticator at connect-time.
        string m_pinForConnect;
#endif

        #region Constructor
#if NETCF
        static SocketBluetoothClient()
        {
            InTheHand.Net.PlatformVerification.ThrowException();
        }
#endif

        internal SocketBluetoothClient(BluetoothFactory fcty)
        {
            Debug.Assert(fcty != null, "ArgNull");
            _fcty = fcty;
            try {
                this.Client = CreateSocket();
            } catch (SocketException se) {
                throw new PlatformNotSupportedException("32feet.NET does not support the Bluetooth stack on this device.", se);
            }
            m_optionHelper = CreateSocketOptionHelper(this.Client);
        }
        internal SocketBluetoothClient(BluetoothFactory fcty, BluetoothEndPoint localEP)
            : this(fcty)
        {
            if (localEP == null) {
                throw new ArgumentNullException("localEP");
            }

            //bind to specific local endpoint
            var bindEP = PrepareBindEndPoint(localEP);
            this.Client.Bind(bindEP);
        }
        
        internal SocketBluetoothClient(BluetoothFactory fcty, Socket acceptedSocket)
        {
            Debug.Assert(fcty != null, "ArgNull");
            _fcty = fcty;
            this.Client = acceptedSocket;
            active = true;
            m_optionHelper = CreateSocketOptionHelper(this.Client);
        }


        protected virtual Socket CreateSocket()
        {
            return new Socket(BluetoothAddressFamily, SocketType.Stream, BluetoothProtocolType.RFComm);
        }

        protected virtual AddressFamily BluetoothAddressFamily
        {
            get { return AddressFamily32.Bluetooth; }
        }

        protected virtual BluetoothEndPoint PrepareConnectEndPoint(BluetoothEndPoint serverEP)
        {
            return serverEP;
        }

        protected virtual BluetoothEndPoint PrepareBindEndPoint(BluetoothEndPoint serverEP)
        {
            return serverEP;
        }

        protected virtual ISocketOptionHelper CreateSocketOptionHelper(Socket socket)
        {
            return new MsftSocketOptionHelper(socket);
        }
        #endregion

        #region InquiryAccessCode
        private int iac = BluetoothAddress.Giac; //0x9E8B33;
#if NETCF
        /// <summary>
        /// 
        /// </summary>
        public int InquiryAccessCode
        {
            get
            {
                return iac;
            }
            set
            {
                iac = value;
            }
        }
#else
        public int InquiryAccessCode
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
#endif
        #endregion

        #region Query Length

        //length of time for query
        private TimeSpan inquiryLength = new TimeSpan(0, 0, 10);

        /// <summary>
        /// Amount of time allowed to perform the query.
        /// </summary>
        /// <remarks>On Windows CE the actual value used is expressed in units of 1.28 seconds, so will be the nearest match for the value supplied.
        /// The default value is 10 seconds. The maximum is 60 seconds.</remarks>
        public TimeSpan InquiryLength
        {
            get
            {
                return inquiryLength;
            }
            set
            {
                if ((value.TotalSeconds > 0) && (value.TotalSeconds <= 60)) {
                    inquiryLength = value;
                } else {
                    throw new ArgumentOutOfRangeException("value",
                        "QueryLength must be a positive timespan between 0 and 60 seconds.");
                }
            }
        }
        #endregion

        #region Discover Devices
        /// <summary>
        /// Discovers accessible Bluetooth devices and returns their names and addresses.
        /// </summary>
        /// <param name="maxDevices">The maximum number of devices to get information about.</param>
        /// <param name="authenticated">True to return previously authenticated/paired devices.</param>
        /// <param name="remembered">True to return remembered devices.</param>
        /// <param name="unknown">True to return previously unknown devices.</param>
        /// <param name="discoverableOnly">True to return only discoverable devices
        /// (where both in range and in discoverable mode).
        /// When <see langword="true"/> all other flags are ignored.
        /// <strong>Note: Does NOT work on Win32 with the Microsoft stack.</strong>
        /// </param>
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        /// -
        /// <remarks>
        /// <para>The <see paramref="discoverableOnly"/> flag will discover only 
        /// the devices that are in range and are in discoverable mode.  This works 
        /// only on WM/CE with the Microsoft stack, or on any platform with the 
        /// Widcomm stack.
        /// </para>
        /// <para>
        /// It does not work on desktop Windows with the Microsoft 
        /// stack, where the in range and remembered devices are returned already 
        /// merged!  There simple all devices will be returned.  Even the 
        /// <see cref="InTheHand.Net.Sockets.BluetoothDeviceInfo.LastSeen">BluetoothDeviceInfo.LastSeen</see>
        /// property is of no use there: on XP and Vista at least the value provided 
        /// is always simply the current time.
        /// </para>
        /// </remarks>
        IBluetoothDeviceInfo[] IBluetoothClient.DiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly)
        {
            return DiscoverDevices(maxDevices,
                authenticated, remembered, unknown, discoverableOnly,
                null, null);
        }

        public virtual IBluetoothDeviceInfo[] DiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
#if !NETCF
            //   In the MSFT+Win32 API there's no simple way to get only the
            // in-range-discoverable devices, and thus it can't provide
            // 'discoverableOnly' nor live discovery.  So, we register for the
            // native event whilst we run a normal discovery.  We use the
            // native event to raise our live event and to gather the devices
            // to be included in the discoverableOnly result.
            //   Note the event seems NOT be raised on WinXP in this case.
            //   The event (on Win7 at least) raises lots of apparently duplicate
            // events, so we need to do some filtering for both results.
            var realLiveDiscoHandler = liveDiscoHandler;
            //if (realLiveDiscoHandler != null) Debug.Assert(discoverableOnly || unknown, "You want live events from 'remembered'?!?");
            bool keepDiscoOnly = false;
            if (discoverableOnly) {
                keepDiscoOnly = true;
                unknown = authenticated = remembered = true;
                discoverableOnly = false;
            }
            BluetoothWin32Events events;
            EventHandler<BluetoothWin32RadioInRangeEventArgs> radioInRangeHandler;
            List<IBluetoothDeviceInfo> seenDevices;
            if (keepDiscoOnly || realLiveDiscoHandler != null) {
                events = BluetoothWin32Events.GetInstance();
                seenDevices = new List<IBluetoothDeviceInfo>();
                radioInRangeHandler = delegate(object sender, BluetoothWin32RadioInRangeEventArgs e) {
                    var newdevice = e.DeviceWindows;
                    Debug.WriteLine("Radio in range: " + newdevice.DeviceAddress);
                    bool unique = BluetoothDeviceInfo.AddUniqueDevice(seenDevices, newdevice);
                    if (unique && realLiveDiscoHandler != null) {
                        Debug.WriteLine("Live IS unique.");
                        realLiveDiscoHandler(newdevice, liveDiscoState);
                    } else { //COVERAGE
                        Debug.WriteLine("Live is NOT unique ("
                            + unique + "," + realLiveDiscoHandler != null + ").");
                    }
                };
                events.InRange += radioInRangeHandler;
            } else {
                events = null;
                radioInRangeHandler = null;
                seenDevices = null;
            }
            // Don't use the WM live-disco functionality.
            liveDiscoHandler = null;
#endif
            IBluetoothDeviceInfo[] result;
            try {
                result = DoDiscoverDevices(maxDevices,
                    authenticated, remembered, unknown, discoverableOnly,
                    liveDiscoHandler, liveDiscoState);
            } finally {
#if !NETCF
                if (events != null && radioInRangeHandler != null) {
                    events.InRange -= radioInRangeHandler;
                }
#endif
            }
#if !NETCF
            if (keepDiscoOnly) {
                Debug.Assert(seenDevices != null, "Should have created list 'seenDevices' above!");
                Debug.WriteLine("Disco result: " + result.Length + ", liveDevice: " + seenDevices.Count);
                result = BluetoothDeviceInfo.Intersect(result, seenDevices);
                Debug.WriteLine("Result result: " + result.Length);
            }
#endif
            return result;
        }

        IBluetoothDeviceInfo[] DoDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
            WqsOffset.AssertCheckLayout();
            CsaddrInfoOffsets.AssertCheckLayout();
            //
#if !NETCF
            Debug.Assert(liveDiscoHandler == null, "Don't use the NETCF live-disco feature on Win32!");
#endif
#if WinXP
            const bool Win32DiscoverableOnlyIncludesAllDevices = false;
#endif
            var prototype = new BluetoothEndPoint(BluetoothAddress.None, BluetoothService.Empty);
            int discoveredDevices = 0;
            List_IBluetoothDeviceInfo al = null;

            IntPtr handle = IntPtr.Zero;
            int lookupresult = 0;
            /* Windows XP SP3
               Begin takes the full Inquiry time.
                        12:09:15.7968750: Begin
                10.265s 12:09:26.0625000: Begin complete
                        12:09:26.0625000: Next
                        12:09:26.0625000: Next Complete
                        ...
                        12:09:26.1718750: Next
                        12:09:26.1718750: Next Complete
                        12:09:26.1718750: End
                        12:09:26.1718750: End Complete
             */
            /* WM 6 SP
               Begin is quick, Next blocks until a device is found.
                       13:46:47.1760000: Begin
                       13:46:47.2350000: Begin complete
                       13:46:47.2360000: Next
               10.537s	13:46:57.7730000: Next Complete
                       13:46:57.8910000: Next
                       13:46:57.8940000: Next Complete
                       13:46:57.8950000: Next
                       13:46:57.8960000: Next Complete
                       13:46:57.8960000: End
                       13:46:57.8990000: End Complete

            */
            System.Text.StringBuilder timings = null;
#if DEBUG
      //      timings = new System.Text.StringBuilder();
#endif
            Action<string> markTime = delegate(string name) {
                if (timings != null)
                    timings.AppendFormat(CultureInfo.InvariantCulture,
                        "{1}: {0}\r\n", name, DateTime.UtcNow.TimeOfDay);
            };

#if WinXP
            if (discoverableOnly && !Win32DiscoverableOnlyIncludesAllDevices) {
                // No way to separate out the devices-in-range on Win32. :-(
                return new IBluetoothDeviceInfo[0];
            }
#endif
#if NETCF
            DateTime discoTime = DateTime.UtcNow;
            if(unknown || discoverableOnly)
            {
#endif
            al = new List_IBluetoothDeviceInfo();
            byte[] buffer = new byte[1024];
            BitConverter.GetBytes(WqsOffset.StructLength_60).CopyTo(buffer, WqsOffset.dwSize_0);
            BitConverter.GetBytes(WqsOffset.NsBth_16).CopyTo(buffer, WqsOffset.dwNameSpace_20);

            int bufferlen = buffer.Length;


            BTHNS_INQUIRYBLOB bib = new BTHNS_INQUIRYBLOB();
            bib.LAP = iac;// 0x9E8B33;

#if NETCF
                bib.length = Convert.ToByte(inquiryLength.TotalSeconds / 1.28);
                bib.num_responses = Convert.ToByte(maxDevices);
#else
            bib.length = Convert.ToByte(inquiryLength.TotalSeconds);
#endif
            GCHandle hBib = GCHandle.Alloc(bib, GCHandleType.Pinned);
            IntPtr pBib = hBib.AddrOfPinnedObject();

            BLOB b = new BLOB(8, pBib);


            GCHandle hBlob = GCHandle.Alloc(b, GCHandleType.Pinned);

            Marshal32.WriteIntPtr(buffer, WqsOffset.lpBlob_56, hBlob.AddrOfPinnedObject());


            //start looking for Bluetooth devices
            LookupFlags flags = LookupFlags.Containers;

#if WinXP
            //ensure cache is cleared on XP when looking for new devices
            if (unknown || discoverableOnly) {
                flags |= LookupFlags.FlushCache;
            }
#endif
            markTime("Begin");
            lookupresult = NativeMethods.WSALookupServiceBegin(buffer, flags, out handle);
            markTime("Begin complete");

            hBlob.Free();
            hBib.Free();

            // TODO ?Change "while(...maxDevices)" usage on WIN32?
            while (discoveredDevices < maxDevices && lookupresult != -1) {
                markTime("Next");
#if NETCF
					lookupresult = NativeMethods.WSALookupServiceNext(handle, LookupFlags.ReturnAddr | LookupFlags.ReturnBlob , ref bufferlen, buffer);
#else
                LookupFlags flagsNext = LookupFlags.ReturnAddr;
#if WIN32_READ_BTH_DEVICE_INFO
                    flagsNext |= LookupFlags.ReturnBlob;
#endif
                lookupresult = NativeMethods.WSALookupServiceNext(handle, flagsNext, ref bufferlen, buffer);
#endif
                markTime("Next Complete");

                if (lookupresult != -1) {
                    //increment found count
                    discoveredDevices++;


                    //status
#if WinXP
                    BTHNS_RESULT status = (BTHNS_RESULT)BitConverter.ToInt32(buffer, WqsOffset.dwOutputFlags_52);
                    bool devAuthd = ((status & BTHNS_RESULT.Authenticated) == BTHNS_RESULT.Authenticated);
                    bool devRembd = ((status & BTHNS_RESULT.Remembered) == BTHNS_RESULT.Remembered);
                    if (devAuthd && !devRembd) {
                        System.Diagnostics.Debug.WriteLine("Win32 BT disco: Auth'd but NOT Remembered.");
                    }
                    bool devUnkwn = !devRembd && !devAuthd;
                    bool include = (authenticated && devAuthd) || (remembered && devRembd) || (unknown && devUnkwn);
                    Debug.Assert(!discoverableOnly, "Expected short circuit for Win32 unsupported discoverableOnly!");
                    if (include)
#else
                        if(true)
#endif
 {
#if NETCF
                            IntPtr lpBlob = (IntPtr)BitConverter.ToInt32(buffer, 56);
                            BLOB ib = (BLOB)Marshal.PtrToStructure(lpBlob, typeof(BLOB));
                            BthInquiryResult bir = (BthInquiryResult)Marshal.PtrToStructure(ib.pBlobData, typeof(BthInquiryResult));
#endif
                        //struct CSADDR_INFO {
                        //    SOCKET_ADDRESS LocalAddr;
                        //    SOCKET_ADDRESS RemoteAddr;
                        //    INT iSocketType;
                        //    INT iProtocol;
                        //}
                        //struct SOCKET_ADDRESS {
                        //    LPSOCKADDR lpSockaddr;
                        //    INT iSockaddrLength;
                        //}
                        //pointer to outputbuffer
                        IntPtr bufferptr = Marshal32.ReadIntPtr(buffer, WqsOffset.lpcsaBuffer_48);
                        //remote socket address
                        IntPtr sockaddrptr = Marshal32.ReadIntPtr(bufferptr, CsaddrInfoOffsets.OffsetRemoteAddr_lpSockaddr_8);
                        //remote socket len
                        int sockaddrlen = Marshal.ReadInt32(bufferptr, CsaddrInfoOffsets.OffsetRemoteAddr_iSockaddrLength_12);


                        SocketAddress btsa = new SocketAddress(AddressFamily32.Bluetooth, sockaddrlen);

                        for (int sockbyte = 0; sockbyte < sockaddrlen; sockbyte++) {
                            btsa[sockbyte] = Marshal.ReadByte(sockaddrptr, sockbyte);
                        }

                        var bep = (BluetoothEndPoint)prototype.Create(btsa);

                        //new deviceinfo
                        IBluetoothDeviceInfo newdevice;

#if NETCF
						newdevice = new WindowsBluetoothDeviceInfo(bep.Address, bir.cod);
                        // Built-in to Win32 so only do on NETCF
                        newdevice.SetDiscoveryTime(discoTime);
#else
                        newdevice = new WindowsBluetoothDeviceInfo(bep.Address);
#if WIN32_READ_BTH_DEVICE_INFO
                        ReadBlobBTH_DEVICE_INFO(buffer, newdevice);
#endif
#endif
                        //add to discovered list
                        al.Add(newdevice);
                        if (liveDiscoHandler != null) {
                            liveDiscoHandler(newdevice, liveDiscoState);
                        }
                    }


                }
            }//while
#if NETCF
			}
#endif

            //stop looking
            if (handle != IntPtr.Zero) {
                markTime("End");
                lookupresult = NativeMethods.WSALookupServiceEnd(handle);
                markTime("End Complete");
            }
            if (timings != null) {
                Debug.WriteLine(timings);
#if !NETCF
                Console.WriteLine(timings);
#endif
            }

#if NETCF
            List_IBluetoothDeviceInfo known = WinCEReadKnownDevicesFromRegistry();
            al = BluetoothClient.DiscoverDevicesMerge(authenticated, remembered, unknown,
                known, al, discoverableOnly, discoTime);
#endif


            //return results
            if (al.Count == 0) {
                //special case for empty collection
                return new IBluetoothDeviceInfo[0] { };
            }

            return (IBluetoothDeviceInfo[])al.ToArray(
#if V1
                typeof(IBluetoothDeviceInfo)
#endif
);
        }

#if WinXP
        private void ReadBlobBTH_DEVICE_INFO(byte[] buffer, IBluetoothDeviceInfo dev)
        {
            // XXXX - Testing only, at least delete the "Console.WriteLine" before use. - XXXX
            IntPtr pBlob = Marshal32.ReadIntPtr(buffer, WqsOffset.lpBlob_56);
            if (pBlob != IntPtr.Zero) {
                BLOB blob = (BLOB)Marshal.PtrToStructure(pBlob, typeof(BLOB));
                if (blob.pBlobData != IntPtr.Zero) {
                    BTH_DEVICE_INFO bdi = (BTH_DEVICE_INFO)Marshal.PtrToStructure(blob.pBlobData, typeof(BTH_DEVICE_INFO));
                    BluetoothDeviceInfoProperties flags = bdi.flags;
                    Debug.WriteLine(String.Format(CultureInfo.InvariantCulture, "[BTH_DEVICE_INFO: {0:X12}, '{1}' = 0x{2:X4}]", bdi.address, flags, bdi.flags));
                    Trace.Assert((flags & ~BDIFMasks.AllOrig) == 0, "*Are* new flags there!");
                }
            }
        }
#endif

#if NETCF
        static class DevicesValueNames
        {
            internal const string DeviceName = "name";
            internal const string ClassOfDevice = "class";
        }

        private static List_IBluetoothDeviceInfo WinCEReadKnownDevicesFromRegistry()
        {
            Func<BluetoothAddress, string, uint, bool, IBluetoothDeviceInfo> makeDev
                = (address, name, classOfDevice, authd)
                    => new WindowsBluetoothDeviceInfo(address, name, classOfDevice, authd);
            var deviceListPath = "Software\\Microsoft\\Bluetooth\\Device";
            return ReadKnownDevicesFromRegistry(deviceListPath, makeDev);
        }

        internal static List_IBluetoothDeviceInfo ReadKnownDevicesFromRegistry(
            string deviceListPath, Func<BluetoothAddress, string, uint, bool, IBluetoothDeviceInfo> makeDev)
        {
            List_IBluetoothDeviceInfo known = new List_IBluetoothDeviceInfo();

            //open bluetooth device key
            RegistryKey devkey = Registry.LocalMachine.OpenSubKey(deviceListPath);
            //bool addFromRegistry = authenticated || remembered;

            if (devkey != null) {

                //enumerate the keys
                foreach (string devid in devkey.GetSubKeyNames()) {
                    BluetoothAddress address;

                    if (BluetoothAddress.TryParse(devid, out address)) {
                        //get friendly name
                        RegistryKey thisdevkey = devkey.OpenSubKey(devid);
                        string name = thisdevkey.GetValue(DevicesValueNames.DeviceName, "").ToString();
                        uint classOfDevice = Convert.ToUInt32(thisdevkey.GetValue(DevicesValueNames.ClassOfDevice, 0));
                        thisdevkey.Close();

                        //add to collection
                        IBluetoothDeviceInfo thisdevice = makeDev(address, name, classOfDevice, true);
                        known.Add(thisdevice);
                    }
                }

                devkey.Close();
            }
            return known;
        }
#endif

#if !V1
        IAsyncResult IBluetoothClient.BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state)
        {
            return ((IBluetoothClient)this).BeginDiscoverDevices(maxDevices,
                authenticated, remembered, unknown, discoverableOnly,
                callback, state,
                null, null);
        }

        IAsyncResult IBluetoothClient.BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state,
            InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
            var args = new DiscoDevsParamsWithLive(maxDevices,
                authenticated, remembered, unknown, discoverableOnly,
                DateTime.MinValue,
                liveDiscoHandler, liveDiscoState);
            AsyncResult_BeginDiscoverDevices ar = new AsyncResult_BeginDiscoverDevices(
                callback, state, args);
            System.Threading.ThreadPool.QueueUserWorkItem(BeginDiscoverDevices_Runner, ar);
            return ar;
        }

        internal class DiscoDevsParamsWithLive : DiscoDevsParams
        {
            internal readonly InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback _liveDiscoHandler;
            internal readonly object _liveDiscoState;

            public DiscoDevsParamsWithLive(int maxDevices,
                    bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
                    DateTime discoTime,
                    InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
                : base(maxDevices,
                    authenticated, remembered, unknown, discoverableOnly,
                    discoTime)
            {
                _liveDiscoHandler = liveDiscoHandler;
                _liveDiscoState = liveDiscoState;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void BeginDiscoverDevices_Runner(object state)
        {
            AsyncResult_BeginDiscoverDevices ar = (AsyncResult_BeginDiscoverDevices)state;
            ar.SetAsCompletedWithResultOf(() => DiscoverDevices(ar.BeginParameters.maxDevices,
                    ar.BeginParameters.authenticated, ar.BeginParameters.remembered,
                    ar.BeginParameters.unknown, ar.BeginParameters.discoverableOnly,
                    ar.BeginParameters._liveDiscoHandler, ar.BeginParameters._liveDiscoState), false);
        }

        IBluetoothDeviceInfo[] IBluetoothClient.EndDiscoverDevices(IAsyncResult asyncResult)
        {
            AsyncResult_BeginDiscoverDevices ar = (AsyncResult_BeginDiscoverDevices)asyncResult;
            return ar.EndInvoke();
        }
#endif

        #endregion


        #region Active
        private bool active = false;

        /// <summary>
        /// Gets or set a value that indicates whether a connection has been made.
        /// </summary>
        protected bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        #endregion

        #region Available
        public int Available
        {
            get
            {
                EnsureNotDisposed();
                return clientSocket.Available;
            }
        }
        #endregion

        #region Client

        private Socket clientSocket;

        public Socket Client
        {
            get { return clientSocket; }
            set { this.clientSocket = value; }

        }

        #endregion

        #region Connect
        /// <summary>
        /// Connects a client to a specified endpoint.
        /// </summary>
        /// <param name="remoteEP">A <see cref="BluetoothEndPoint"/> that represents the remote device.</param>
        public virtual void Connect(BluetoothEndPoint remoteEP)
        {
            EnsureNotDisposed();
            if (remoteEP == null) {
                throw new ArgumentNullException("remoteEP");
            }

            Connect_StartAuthenticator(remoteEP);
            try {
                var connEP = PrepareConnectEndPoint(remoteEP);
                clientSocket.Connect(connEP);
                active = true;
            } finally {
                Connect_StopAuthenticator();
            }
        }

        #region Begin Connect
        /// <summary>
        /// Begins an asynchronous request for a remote host connection.
        /// The remote host is specified by a <see cref="BluetoothEndPoint"/>. 
        /// </summary>
        /// <param name="remoteEP">A <see cref="BluetoothEndPoint"/> containing the 
        /// address and UUID of the remote service.</param>
        /// <param name="requestCallback">An AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the connect operation.
        /// This object is passed to the requestCallback delegate when the operation is complete.</param>
        /// <returns></returns>
        public virtual IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            EnsureNotDisposed();
            Connect_StartAuthenticator(remoteEP);
            var connEP = PrepareConnectEndPoint(remoteEP);
            return this.Client.BeginConnect(connEP, requestCallback, state);
        }
        #endregion

        #region End Connect
        /// <summary>
        /// Asynchronously accepts an incoming connection attempt.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> object returned by a call to 
        /// <see cref="M:BeginConnect(InTheHand.Net.Sockets.BluetoothEndPoint,System.AsyncCallback,System.Object)"/>
        /// / <see cref="M:BeginConnect(InTheHand.Net.Sockets.BluetoothAddress,System.Guid,System.AsyncCallback,System.Object)"/>.
        /// </param>
        public virtual void EndConnect(IAsyncResult asyncResult)
        {
            try {
                Socket sock = this.Client;
                if (sock == null) {
                    Debug.Assert(cleanedUp, "!cleanedUp");
                    throw new ObjectDisposedException("BluetoothClient");
                } else {
                    sock.EndConnect(asyncResult);
                }
                this.active = true;
            } finally {
                Connect_StopAuthenticator();
            }
        }
        #endregion

        #endregion

        #region Connected
        public bool Connected
        {
            get
            {
                if (clientSocket == null)
                    return false;
                try
                {
                    return clientSocket.Connected;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
        }
        #endregion

        #region Get Stream

        private NetworkStream dataStream;

        protected virtual NetworkStream MakeStream(Socket sock)
        {
            return new NetworkStream(this.Client, true);
        }


        public NetworkStream GetStream()
        {
            EnsureNotDisposed();
            if (!this.Client.Connected) {
                throw new InvalidOperationException("The operation is not allowed on non-connected sockets.");
            }

            if (dataStream == null) {
                dataStream = MakeStream(this.Client);
            }

            return dataStream;
        }

#if TEST_EARLY
        public Stream GetStream2()
        {
            return GetStream();
        }
#endif
        #endregion

        public LingerOption LingerState
        {
#if !NETCF
            get { return Client.LingerState; }
            set { Client.LingerState = value; }
#else
            get
            {
                return (LingerOption)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
            }
            set
            {
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
            }
#endif
        }


        #region Authenticate
        /// <summary>
        /// Gets or sets the authentication state of the current connect or behaviour to use when connection is established.
        /// </summary>
        /// <remarks>
        /// For disconnected sockets, specifies that authentication is required in order for a connect or accept operation to complete successfully.
        /// Setting this option actively initiates authentication during connection establishment, if the two Bluetooth devices were not previously authenticated.
        /// The user interface for passkey exchange, if necessary, is provided by the operating system outside the application context.
        /// For outgoing connections that require authentication, the connect operation fails with WSAEACCES if authentication is not successful.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// For incoming connections, the connection is rejected if authentication cannot be established and returns a WSAEHOSTDOWN error.
        /// </remarks>
        public bool Authenticate
        {
            get { return m_optionHelper.Authenticate; }
            set { m_optionHelper.Authenticate = value; }
        }
        #endregion

        #region Encrypt
        /// <summary>
        /// On unconnected sockets, enforces encryption to establish a connection.
        /// Encryption is only available for authenticated connections.
        /// For incoming connections, a connection for which encryption cannot be established is automatically rejected and returns WSAEHOSTDOWN as the error.
        /// For outgoing connections, the connect function fails with WSAEACCES if encryption cannot be established.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// </summary>
        public bool Encrypt
        {
            get { return m_optionHelper.Encrypt; }
            set { m_optionHelper.Encrypt = value; }
        }
        #endregion


#region Link Key
        /// <summary>
        /// Returns link key associated with peer Bluetooth device.
        /// </summary>
        public Guid LinkKey
        {
            get
            {
                EnsureNotDisposed();
                byte[] bytes = new byte[16];
#if NETCF
                byte[] link = clientSocket.GetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.GetLink, 32);
                Buffer.BlockCopy(link, 16, bytes, 0, 16);
#endif                

                return new Guid(bytes);
            }
        }

#endregion

#region Link Policy
        /// <summary>
        /// Returns the Link Policy of the current connection.
        /// </summary>
        public LinkPolicy LinkPolicy
        {
            get
            {
                EnsureNotDisposed();
#if NETCF
                byte[] policy = clientSocket.GetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.GetLinkPolicy, 4);
                return (LinkPolicy)BitConverter.ToInt32(policy, 0);
#else
                return LinkPolicy.Disabled;
#endif
            }
        }
#endregion


#region Set PIN
        /// <summary>
        /// Sets the PIN associated with the currently connected device.
        /// </summary>
        /// <param name="pin">PIN which must be composed of 1 to 16 ASCII characters.</param>
        /// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        public void SetPin(string pin)
        {
            if (!Connected) {
#if WinXP
                m_pinForConnect = pin;
#else
                SetPin(null, pin);
#endif
            } else {
                EndPoint rep = clientSocket.RemoteEndPoint;
                BluetoothAddress addr = null;
                if (rep != null)
                    addr = ((BluetoothEndPoint)rep).Address;
                if (addr == null)
                    throw new InvalidOperationException(
                        "The socket needs to be connected to detect the remote device"
                        + ", use the other SetPin method..");
                SetPin(addr, pin);
            }
        }

        /// <summary>
        /// Set or change the PIN to be used with a specific remote device.
        /// </summary>
        /// <param name="device">Address of Bluetooth device.</param>
        /// <param name="pin">PIN string consisting of 1 to 16 ASCII characters.</param>
        /// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        public void SetPin(BluetoothAddress device, string pin)
        {
            m_optionHelper.SetPin(device, pin);
        }

        private void Connect_StartAuthenticator(BluetoothEndPoint remoteEP)
        {
#if WinXP
            if (m_pinForConnect != null) {
                SetPin(remoteEP.Address, m_pinForConnect);
            }
#endif
        }

        private void Connect_StopAuthenticator()
        {
#if WinXP
            if (m_pinForConnect != null) {
                SetPin(null, null);
            }
#endif
        }
#endregion


#region Remote Machine Name
        public BluetoothEndPoint RemoteEndPoint
        {
            get
            {
                EnsureNotDisposed();
                return (BluetoothEndPoint)clientSocket.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Gets the name of the remote device.
        /// </summary>
        public string RemoteMachineName
        {
            get
            {
                EnsureNotDisposed();
                return GetRemoteMachineName(clientSocket);
            }
        }

        /// <summary>
        /// Gets the name of the specified remote device.
        /// </summary>
        /// <param name="a">Address of remote device.</param>
        /// <returns>Friendly name of specified device.</returns>
        public string GetRemoteMachineName(BluetoothAddress a)
        {
#if WinXP
            var bdi = _fcty.DoGetBluetoothDeviceInfo(a);
            return bdi.DeviceName;
#else
			byte[] buffer = new byte[504];
			//copy remote device address to buffer
			Buffer.BlockCopy(a.ToByteArray(), 0, buffer, 0, 6);

			try
			{
                clientSocket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.ReadRemoteName, buffer);
				string name = string.Empty;
                name = System.Text.Encoding.Unicode.GetString(buffer, 8, 496);
				
				int offset = name.IndexOf('\0');
				if(offset > -1)
				{
					name = name.Substring(0, offset);
				}

				return name;
			}
			catch(SocketException ex)
			{
                System.Diagnostics.Debug.WriteLine("BluetoothClient GetRemoteMachineName(addr) ReadRemoteName failed: " + ex.Message);
				return null;
			}
#endif
        }

        /// <summary>
        /// Gets the name of a device by a specified socket.
        /// </summary>
        /// <param name="s"> A <see cref="Socket"/>.</param>
        /// <returns>Returns a string value of the computer or device name.</returns>
        public static string GetRemoteMachineName(Socket s)
        {
#if WinXP
            // HACK was new WindowsBluetoothDeviceInfo
            var bdi = new BluetoothDeviceInfo(((BluetoothEndPoint)s.RemoteEndPoint).Address);
            return bdi.DeviceName;
#else
			byte[] buffer = new byte[504];
			//copy remote device address to buffer
			Buffer.BlockCopy(((BluetoothEndPoint)s.RemoteEndPoint).Address.ToByteArray(), 0, buffer, 0, 6);

            string name = "";

			try
			{
                s.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.ReadRemoteName, buffer);
				name = System.Text.Encoding.Unicode.GetString(buffer, 8, 496);
				
				int offset = name.IndexOf('\0');
				if(offset > -1)
				{
					name = name.Substring(0, offset);
				}

				return name;
			}
			catch(SocketException ex)
			{
                System.Diagnostics.Debug.WriteLine("BluetoothClient GetRemoteMachineName(socket) ReadRemoteName failed: " + ex.Message);
                return null;
			}
#endif
        }
#endregion

#region IDisposable Members

        /// <summary>
        /// Releases the unmanaged resources used by the BluetoothClient and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!cleanedUp) {
                if (disposing) {
                    IDisposable idStream = dataStream;
                    if (idStream != null) {
                        //dispose the stream which will also close the socket
                        idStream.Dispose();
                    } else {
                        if (this.Client != null) {
                            this.Client.Close();
                            this.Client = null;
                        }
                    }
                    // TODO ??? m_optionHelper.Dispose();
                }

                cleanedUp = true;
            }
        }

        private void EnsureNotDisposed()
        {
            Debug.Assert(cleanedUp == (clientSocket == null), "always consistent!! ("
                + cleanedUp + " != " + (clientSocket == null) + ")");
            if (cleanedUp || (clientSocket == null))
                throw new ObjectDisposedException("BluetoothClient");
        }

        /// <summary>
        /// Closes the <see cref="BluetoothClient"/> and the underlying connection.
        /// </summary>
        /// -
        /// <seealso cref="M:InTheHand.Net.Sockets.BluetoothClient.Close"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees resources used by the <see cref="BluetoothClient"/> class.
        /// </summary>
        ~SocketBluetoothClient()
        {
            Dispose(false);
        }

#endregion

#region Throw SocketException For HR
        internal static void ThrowSocketExceptionForHR(int errorCode)
        {
            if (errorCode < 0) {
                int socketerror = 0;
                socketerror = Marshal.GetLastWin32Error();

                throw new SocketException(socketerror);
            }
        }

        internal static void ThrowSocketExceptionForHrExceptFor(int errorCode, params int[] nonErrorCodes)
        {
            if (errorCode < 0) {
                int socketerror = 0;
                socketerror = Marshal.GetLastWin32Error();
                if (-1 != Array.IndexOf(nonErrorCodes, socketerror, 0, nonErrorCodes.Length)) {
                    return;
                }
                throw new SocketException(socketerror);
            }
        }
#endregion

        internal class MsftSocketOptionHelper : ISocketOptionHelper
        {
            readonly Socket m_socket;
#if !WinCE
            private bool authenticate = false;
            private BluetoothWin32Authentication m_authenticator;
#endif
            private bool encrypt = false;

            internal MsftSocketOptionHelper(Socket socket)
            {
                m_socket = socket;
            }

#region Authenticate
            /// <summary>
            /// Gets or sets the authentication state of the current connect or behaviour to use when connection is established.
            /// </summary>
            /// <remarks>
            /// For disconnected sockets, specifies that authentication is required in order for a connect or accept operation to complete successfully.
            /// Setting this option actively initiates authentication during connection establishment, if the two Bluetooth devices were not previously authenticated.
            /// The user interface for passkey exchange, if necessary, is provided by the operating system outside the application context.
            /// For outgoing connections that require authentication, the connect operation fails with WSAEACCES if authentication is not successful.
            /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
            /// For incoming connections, the connection is rejected if authentication cannot be established and returns a WSAEHOSTDOWN error.
            /// </remarks>
            public bool Authenticate
            {
                get
                {
#if NETCF
                    byte[] authbytes = m_socket.GetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.GetAuthenticationEnabled, 4);
                    int auth = BitConverter.ToInt32(authbytes, 0);
                    return (auth==0) ? false : true;
#else
                    return authenticate;
#endif
                }
                set
                {
#if NETCF
                    m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.SetAuthenticationEnabled, (int)(value ? 1 : 0));
#else
                    m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.Authenticate, value);
                    authenticate = value;
#endif
                }
            }
#endregion

#region Encrypt
            /// <summary>
            /// On unconnected sockets, enforces encryption to establish a connection.
            /// Encryption is only available for authenticated connections.
            /// For incoming connections, a connection for which encryption cannot be established is automatically rejected and returns WSAEHOSTDOWN as the error.
            /// For outgoing connections, the connect function fails with WSAEACCES if encryption cannot be established.
            /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
            /// </summary>
            public bool Encrypt
            {
                get { return encrypt; }
                set
                {
                    m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.Encrypt, (int)(value ? 1 : 0));
                    encrypt = value;
                }
            }
#endregion

#region Set Pin
            public void SetPin(BluetoothAddress device, string pin)
            {
#if WinXP
                if (pin != null) {
                    m_authenticator = new BluetoothWin32Authentication(device, pin);
                } else {
                    if (m_authenticator != null) {
                        m_authenticator.Dispose();
                    }
                }
#else
                byte[] link = new byte[32];

                //copy remote device address
                if (device != null)
                {
                    Buffer.BlockCopy(device.ToByteArray(), 0, link, 8, 6);
                }

                //copy PIN
                if (pin != null & pin.Length > 0)
                {
                    if (pin.Length > 16)
                    {
                        throw new ArgumentOutOfRangeException("PIN must be between 1 and 16 ASCII characters");
                    }
                    //copy pin bytes
                    byte[] pinbytes = System.Text.Encoding.ASCII.GetBytes(pin);
                    Buffer.BlockCopy(pinbytes, 0, link, 16, pin.Length);
                    BitConverter.GetBytes(pin.Length).CopyTo(link, 0);
                }

                m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.SetPin, link);
#endif
            }
#endregion

        }//class--SocketOptionHelper

    }//class--BluetoothClient

}
