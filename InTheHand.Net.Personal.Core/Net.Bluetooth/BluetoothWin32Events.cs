// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.BluetoothWin32Events
// 
// Copyright (c) 2008-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2008-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
#if !NETCF
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
#if !NO_WINFORMS
using System.Windows.Forms;
#endif
using InTheHand.Net.Bluetooth.Msft;
using Microsoft.Win32.SafeHandles;
using System.Globalization;
#endif

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Provides access to the Bluetooth events from the Microsoft stack on
    /// desktop Windows.
    /// </summary>
    /// -
    /// <remarks>
    /// <note>Supported only by the Microsoft stack on desktop Windows.
    /// </note>
    /// <para>The Microsoft Bluetooth stack on Window raises events for various
    /// Bluetooth actions.  We expose that feature via this class.
    /// </para>
    /// <para>Currently it raises two types of event: in-range and out-of-range
    /// using classes: <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32RadioInRangeEventArgs"/>
    /// and <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32RadioOutOfRangeEventArgs"/>.
    /// Both have properties <c>Device</c> which return a <c>BluetoothDeviceInfo</c>.
    /// Then the in-range event also includes a set of flags, which in
    /// Windows XP are: Address, Cod, Name, Paired, Personal, and Connected;
    /// more events are available in Windows 7.  These events are provided on
    /// the <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32RadioInRangeEventArgs"/>
    /// class via properties:
    /// <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32RadioInRangeEventArgs.CurrentState"/>
    /// and <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32RadioInRangeEventArgs.PreviousState"/>,
    /// and also <see cref="P:InTheHand.Net.Bluetooth.BluetoothWin32RadioInRangeEventArgs.GainedStates"/> etc.
    /// </para>
    /// <para>To see the events get an instance of this class via its method
    /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32Events.GetInstance"/>.
    /// Then one should register for the events on that instance and keep a
    /// reference to it.
    /// </para>
    /// <para>Note that just being in range is not enough for
    /// devices to know that the other is present.  Without running device
    /// discovery or a connection attempt the two devices will not see each
    /// other.  Note however that Windows XP also does not raise events when
    /// running device discovery (inquiry), this is fixed in Windows 7
    /// (probably Vista).  See
    /// <see href="http://32feetnetdev.wordpress.com/2010/11/15/device-discovery-improvements-on-msftwin32/">32feet blog: Device Discovery improvements on MSFT+Win32</see>
    /// for more information.
    /// </para>
    /// 
    /// <para>For example when connecting and disconnecting on Windows XP to
    /// another device that is not paired we see:
    /// </para>
    /// <example>
    /// <code lang="none">
    /// 12:23:48.9582648: InRange 000A3A6865BB 'joe',
    ///     now 'Address, Cod, Name, Connected'
    ///     was 'Address, Cod, Name'.
    /// 12:24:16.8009456: InRange 000A3A6865BB 'joe',
    ///     now 'Address, Cod, Name'
    ///     was 'Address, Cod, Name, Connected'.}}
    /// </code>
    /// </example>
    /// <para>For example when connecting and then disconnecting on Windows 7
    /// to another v2.1 device that is paired with we see:
    /// </para>
    /// <example>
    /// <code lang="none">
    /// 20:53:25.5605469: InRange 00190E02C916 'alanlt2ws',
    ///     now 'Address, Cod, Name, Paired, Personal, Connected, SspSupported, SspPaired, Rssi, Eir'
    ///     was 'Address, Cod, Name, Paired, Personal,            SspSupported, SspPaired, Rssi, Eir'.
    /// 20:53:27.7949219: InRange 00190E02C916 'fred',
    ///     now 'Address, Cod, Name, Paired, Personal,            SspSupported, SspPaired, Rssi, Eir'
    ///     was 'Address, Cod, Name, Paired, Personal, Connected, SspSupported, SspPaired, Rssi, Eir'.}}
    /// </code>
    /// </example>
    /// </remarks>
    public class BluetoothWin32Events : IDisposable
    {
#if NO_WINFORMS
        public BluetoothWin32Events()
        {
            throw new NotSupportedException("BluetoothWin32Events is not supported when built with NO_WINFORMS.");
        }

        public BluetoothWin32Events(BluetoothRadio microsoftWin32BluetoothRadio)
            : this()
        {
        }

        public static BluetoothWin32Events GetInstance()
        {
            return new BluetoothWin32Events();
        }

        /// <summary>
        /// &#x201C;This message is sent when any of the following attributes
        /// of a remote Bluetooth device has changed: the device has been
        /// discovered, the class of device, name, connected state, or device
        /// remembered state. This message is also sent when these attributes
        /// are set or cleared.&#x201D;
        /// </summary>
        public event EventHandler<BluetoothWin32RadioInRangeEventArgs> InRange = delegate { };

        /// <summary>
        /// &#x201C;This message is sent when a previously discovered device
        /// has not been found after the completion of the last inquiry.&#x201D;
        /// </summary>
        public event EventHandler<BluetoothWin32RadioOutOfRangeEventArgs> OutOfRange = delegate { };


        /// <summary>
        /// Releases the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
        }
#else



#if !NETCF
        //static void Marshal_PtrToStructure<T>(IntPtr ptr, out T result)
        //    where T : struct
        //{
        //    result = (T)Marshal.PtrToStructure(ptr, typeof(T));
        //}

        static T Marshal_PtrToStructure<T>(IntPtr ptr)
            where T : struct
        {
            var result = (T)Marshal.PtrToStructure(ptr, typeof(T));
            return result;
        }

        //--------
        static readonly Int32 _OffsetOfData;
        //
        static WeakReference/*BluetoothWin32Events>*/ _instance;
        //
        BtEventsForm _form;
        Thread _formThread;
        ManualResetEvent _inited;
        Exception _formThreadException;
        readonly IntPtr _bluetoothRadioHandle;
#endif

#if !NETCF
        static BluetoothWin32Events()
        {
            _OffsetOfData = Marshal.OffsetOf(typeof(DEV_BROADCAST_HANDLE__withData),
                "dbch_data__0").ToInt32();
#if DEBUG
            if (IntPtr.Size == 4) {
                Debug.Assert(40 == _OffsetOfData);
            } else {
                Debug.Assert(52 == _OffsetOfData);
            }
#endif
        }
#endif

        /// <summary>
        /// Initialise an instance of the class.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Consider using the method <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32Events.GetInstance"/>
        /// instead of calling this constructor.
        /// </para>
        /// </remarks>
        public BluetoothWin32Events()
        {
#if NETCF
            throw new NotSupportedException("BluetoothWin32Events is Win32 only.");
#else
            BluetoothRadio microsoftWin32BluetoothRadio = null;
            foreach (var cur in BluetoothRadio.AllRadios) {
                if (cur.SoftwareManufacturer == Manufacturer.Microsoft) {
                    microsoftWin32BluetoothRadio = cur;
                    break;
                }
            }//for
            if (microsoftWin32BluetoothRadio == null) {
                throw new InvalidOperationException("There is no Radio using the Microsoft Bluetooth stack.");
            } else {
                IntPtr bluetoothRadioHandle = microsoftWin32BluetoothRadio.Handle;
                if (bluetoothRadioHandle == IntPtr.Zero)
                    throw new ArgumentException("The bluetoothRadioHandle may not be NULL.");
                _bluetoothRadioHandle = bluetoothRadioHandle;
                Start();
            }
#endif
        }

        /// <summary>
        /// Initialise an instance of the class for the specified radio.
        /// </summary>
        /// -
        /// <param name="microsoftWin32BluetoothRadio">
        /// The radio to listen for events from.
        /// Must be non-null and a MSFT+Win32 stack radio.
        /// </param>
        /// -
        /// <remarks>Note that since the Microsoft stack supports only one radio
        /// (controller) there is lilely no benefit in calling this constructor
        /// as opposed to the other constructor or method
        /// <see cref="M:InTheHand.Net.Bluetooth.BluetoothWin32Events.GetInstance"/>.
        /// </remarks>
        public BluetoothWin32Events(BluetoothRadio microsoftWin32BluetoothRadio)
        {
#if NETCF
            throw new NotSupportedException("BluetoothWin32Events is Win32 only.");
#else
            if (microsoftWin32BluetoothRadio == null) throw new ArgumentNullException("microsoftWin32BluetoothRadio");
            if (microsoftWin32BluetoothRadio.SoftwareManufacturer != Manufacturer.Microsoft)
                throw new ArgumentException("The specified Radio does not use the Microsoft Bluetooth stack.");
            IntPtr bluetoothRadioHandle = microsoftWin32BluetoothRadio.Handle;
            if (bluetoothRadioHandle == IntPtr.Zero)
                throw new ArgumentException("The bluetoothRadioHandle may not be NULL.");
            _bluetoothRadioHandle = bluetoothRadioHandle;
            Start();
#endif
        }

        //----
        /// <summary>
        /// Gets a possible shared instance of this class.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>If more that one piece of code is using this class then there
        /// is no need for each to have a private instance.  This method allows
        /// them to access a shared instance.  When first called it creates a
        /// new instance and keeps a weak-reference to it.  Subsequent callers
        /// will then get the same instance.  The instance is kept alive only
        /// as long as at least one caller keeps a reference to it.  If no
        /// references are kept then the instance will be deleted and a new
        /// instance will be created when this method is next called.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>An instance of this class.
        /// </returns>
        public static BluetoothWin32Events GetInstance()
        {
#if NETCF
            throw new NotSupportedException("BluetoothWin32Events is Win32 only.");
#else
            var weakRef = _instance;
            object target;
            if (weakRef == null || (target = weakRef.Target) == null) {
                target = new BluetoothWin32Events();
                _instance = new WeakReference(target);
            } else {
                target.ToString(); // COVERAGE
            }
            return (BluetoothWin32Events)target;
#endif
        }

        //----
        void RaiseInRange(BluetoothWin32RadioInRangeEventArgs e)
        {
            OnInRange(e);
        }

        /// <summary>
        /// Raises the <see cref="E:InTheHand.Net.Bluetooth.BluetoothWin32Events.InRange"/> event.
        /// </summary>
        /// -
        /// <param name="e">A <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32RadioInRangeEventArgs"/>
        /// that contains the event data.
        /// </param>
        protected void OnInRange(BluetoothWin32RadioInRangeEventArgs e)
        {
            InRange(this, e);
        }

        /// <summary>
        /// &#x201C;This message is sent when any of the following attributes
        /// of a remote Bluetooth device has changed: the device has been
        /// discovered, the class of device, name, connected state, or device
        /// remembered state. This message is also sent when these attributes
        /// are set or cleared.&#x201D;
        /// </summary>
        public event EventHandler<BluetoothWin32RadioInRangeEventArgs> InRange = delegate { };

        void RaiseOutOfRange(BluetoothWin32RadioOutOfRangeEventArgs e)
        {
            OnOutOfRange(e);
        }

        /// <summary>
        /// Raises the <see cref="E:InTheHand.Net.Bluetooth.BluetoothWin32Events.OutOfRange"/> event.
        /// </summary>
        /// -
        /// <param name="e">A <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32RadioOutOfRangeEventArgs"/>
        /// that contains the event data.
        /// </param>
        protected void OnOutOfRange(BluetoothWin32RadioOutOfRangeEventArgs e)
        {
            OutOfRange(this, e);
        }

        /// <summary>
        /// &#x201C;This message is sent when a previously discovered device
        /// has not been found after the completion of the last inquiry.&#x201D;
        /// </summary>
        public event EventHandler<BluetoothWin32RadioOutOfRangeEventArgs> OutOfRange = delegate { };

        //----
#if !NETCF
        private void Start()
        {
            if (Application.MessageLoop) {
                StartUseExistingAppLoop();
            } else {
                StartOwnAppLoop();
            }
        }

        private void StartUseExistingAppLoop()
        {
            CreateForm();
            //form.Show(); // Not necessary -- enable for debugging only.
        }

        private void StartOwnAppLoop()
        {
            bool success = false;
            try {
                using (_inited = new System.Threading.ManualResetEvent(false)) {
                    _formThread = new Thread(MessageLoop_Runner);
                    _formThread.IsBackground = true;
                    _formThread.SetApartmentState(ApartmentState.STA);
                    _formThread.Start(this);
                    _inited.WaitOne();
                }
                _inited = null;
                Thread.MemoryBarrier();
                if (_formThreadException != null)
                    throw _formThreadException;
                _formThreadException = null;
                success = true;
            } finally {
                if (!success) {
                    if (_form != null) {
                        //TODO MUST be on UI thread!! form.Dispose();
                        //form = null;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Is rethrown on the creator.")]
        void MessageLoop_Runner(object state)
        {
            try {
                ApartmentState a = _formThread.GetApartmentState();
                Debug.Assert(a == ApartmentState.STA, "!STA!");
                CreateForm();
                //form.Show(); // Not necessary -- enable for debugging only.
                _inited.Set(); // Initial success
                Application.Run();
            } catch (Exception ex) {
                _formThreadException = ex;
                Thread.MemoryBarrier();
            } finally {
                if (_form != null)
                    _form.Dispose();
                if (_inited != null)
                    _inited.Set();
            }
        }

        private void CreateForm()
        {
            _form = new BtEventsForm(this);
            _form.btRegister(_bluetoothRadioHandle);
        }
#endif

        //----
#if NETCF
        /// <summary>
        /// Releases the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
        }
#else
        /// <summary>
        /// Releases the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the instance
        /// and optionally releases the managed resources.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_form",
            Justification = "It is disposed on the UI thread.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                if (_form != null) {
                    MethodInvoker dlgt = DisposeForm;
                    _form.Invoke(dlgt);
                    _form = null;
                    //
                    if (_formThread != null) {
                        int bigTimeoutAllowForDebugging = 60 * 1000;
                        bool terminated = _formThread.Join(bigTimeoutAllowForDebugging);
                        Debug.Assert(terminated, "!terminated");
                        if (_formThreadException != null)
                            throw _formThreadException;
                        _formThread = null;
                    }
                }
            }
        }

        void DisposeForm()
        {
            // Need to call these from the UI thread
            Debug.Assert(!_form.InvokeRequired, "DisposeForm() not on Form-UI thread.");
            _form.Dispose();
            if (_formThread != null) { // Using our own app loop.
                Application.ExitThread();
            }
        }

        //--------------------------------------------------------------
        sealed class RegisterDeviceNotificationSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public RegisterDeviceNotificationSafeHandle()
                : base(true) //ownsHandle
            {
            }

            protected override bool ReleaseHandle()
            {
                return btUnregister(this.handle);
            }

            internal static bool btUnregister(IntPtr hDevNotification)
            {
                System.Diagnostics.Debug.Assert(hDevNotification != IntPtr.Zero, "btUnregister, not registered.");
                bool success = UnsafeNativeMethods.UnregisterDeviceNotification(hDevNotification);
                hDevNotification = IntPtr.Zero;
                System.Diagnostics.Debug.Assert(success, "UnregisterDeviceNotification success false.");
                return success;
            }
        }

        //--------------------------------------------------------------
        sealed class BtEventsForm : Form
        {
            BluetoothWin32Events _parent;
            //----------------
            RegisterDeviceNotificationSafeHandle _hDevNotification;

            internal BtEventsForm(BluetoothWin32Events parent)
            {
                _parent = parent;
                this.Text = "32feet.NET WM_DEVICECHANGE Window";
            }

            protected override void Dispose(bool disposing)
            {
                try {
                    if (_hDevNotification != null && !_hDevNotification.IsClosed) {
                        _hDevNotification.Close();
                    }
                } finally {
                    base.Dispose(disposing);
                }
            }

            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)
                // Keep FxCop happy, for 
            ]
            internal void btRegister(IntPtr bluetoothRadioHandle)
            {
                Debug.Assert(_hDevNotification == null, "btRegister, already set.");
                Debug.Assert(_hDevNotification == null || _hDevNotification.IsInvalid, "btRegister, already registered.");
                IntPtr windowHandle = this.Handle;
                DEV_BROADCAST_HANDLE devHandle = new DEV_BROADCAST_HANDLE(bluetoothRadioHandle);
                RegisterDeviceNotificationSafeHandle hDevNotification
                    = UnsafeNativeMethods.RegisterDeviceNotification_SafeHandle(windowHandle,
                        ref devHandle, RegisterDeviceNotificationFlags.DEVICE_NOTIFY_WINDOW_HANDLE);
                if (hDevNotification.IsInvalid) {
                    throw new Win32Exception(/*error code from GetLastError*/);
                }
                _hDevNotification = hDevNotification;
            }

            //----------------
            [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand,
                Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
            protected override void WndProc(ref Message m)
            {
                _parent.HandleMessage(ref m);
                base.WndProc(ref m);
            }

        }//class Form

        //--
        void HandleMessage(ref Message m)
        {
            WindowMessageId msgId = (WindowMessageId)m.Msg;
            if (msgId == WindowMessageId.DeviceChange) {
                Dbt subId = (Dbt)m.WParam;
                if (subId == Dbt.CustomEvent
                        || subId == Dbt.DeviceArrival
                        || subId == Dbt.DeviceQueryRemove
                        || subId == Dbt.DeviceQueryRemoveFailed
                        || subId == Dbt.DeviceRemoveComplete
                        || subId == Dbt.DeviceRemovePending
                        || subId == Dbt.DeviceTypeSpecific) {
                    DoBroadcastHdr(m);
                }
            } else if (msgId == WindowMessageId.ActivateApp) {
            }
        }

        //----------------
        private void DoBroadcastHdr(Message m)
        {
            //IntPtr pXXX;
            String text = String.Empty;
            DEV_BROADCAST_HDR hdr = (DEV_BROADCAST_HDR)m.GetLParam(typeof(DEV_BROADCAST_HDR));
            if (hdr.dbch_devicetype == DbtDevTyp.Port) {
                DoDevTypPort(ref m, ref text, ref hdr);
            } else if (hdr.dbch_devicetype == DbtDevTyp.Handle) {
                DoDevTypHandle(ref m, ref text);
            }
        }

        private void DoDevTypHandle(ref Message m, ref String text)
        {
#if DEBUG
            WindowsBluetoothDeviceInfo dev = null;
#endif
            DEV_BROADCAST_HANDLE hdrHandle = (DEV_BROADCAST_HANDLE)m.GetLParam(typeof(DEV_BROADCAST_HANDLE));
            var pData = Utils.Pointers.Add(m.LParam, _OffsetOfData);
            if (BluetoothDeviceNotificationEvent.BthPortDeviceInterface == hdrHandle.dbch_eventguid) {
                text += "GUID_BTHPORT_DEVICE_INTERFACE";
            } else if (BluetoothDeviceNotificationEvent.RadioInRange == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_RADIO_IN_RANGE";
                BTH_RADIO_IN_RANGE inRange
                    = (BTH_RADIO_IN_RANGE)Marshal.PtrToStructure(pData, typeof(BTH_RADIO_IN_RANGE));
                text += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " 0x{0:X12}", inRange.deviceInfo.address);
                text += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " is ({0}) 0x{0:X}", inRange.deviceInfo.flags);
                text += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " was ({0}) 0x{0:X}", inRange.previousDeviceFlags);
                var bdi0 = BLUETOOTH_DEVICE_INFO.Create(inRange.deviceInfo);
                var e = BluetoothWin32RadioInRangeEventArgs.Create(
                        inRange.previousDeviceFlags,
                        inRange.deviceInfo.flags, bdi0);
#if DEBUG
                dev = new WindowsBluetoothDeviceInfo(bdi0);
                Debug.WriteLine("InRange: " + dev.DeviceAddress);
#endif
                RaiseInRange(e);
            } else if (BluetoothDeviceNotificationEvent.RadioOutOfRange == hdrHandle.dbch_eventguid) {
                BTH_RADIO_OUT_OF_RANGE outOfRange
                    = (BTH_RADIO_OUT_OF_RANGE)Marshal.PtrToStructure(pData, typeof(BTH_RADIO_OUT_OF_RANGE));
                text += "GUID_BLUETOOTH_RADIO_OUT_OF_RANGE";
                text += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " 0x{0:X12}", outOfRange.deviceAddress);
                var e = BluetoothWin32RadioOutOfRangeEventArgs.Create(
                        outOfRange.deviceAddress);
                Debug.WriteLine("OutOfRange: " + outOfRange.deviceAddress);
                RaiseOutOfRange(e);
            } else if (BluetoothDeviceNotificationEvent.PinRequest == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_PIN_REQUEST";
                // "This message should be ignored by the application.
                //  If the application must receive PIN requests, the 
                //  BluetoothRegisterForAuthentication function should be used."
            } else if (BluetoothDeviceNotificationEvent.L2capEvent == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_L2CAP_EVENT";
                // struct BTH_L2CAP_EVENT_INFO {
                //   BTH_ADDR bthAddress; USHORT psm; UCHAR connected; UCHAR initiated; }
#if DEBUG
                var l2capE = Marshal_PtrToStructure<BTH_L2CAP_EVENT_INFO>(pData);
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "L2CAP_EVENT: addr: {0:X}, psm: {1}, conn: {2}, init'd: {3}",
                    l2capE.bthAddress, l2capE.psm, l2capE.connected, l2capE.initiated));
#endif
            } else if (BluetoothDeviceNotificationEvent.HciEvent == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_HCI_EVENT";
                // struct BTH_HCI_EVENT_INFO {
                //   BTH_ADDR bthAddress; UCHAR connectionType; UCHAR connected; } 
#if DEBUG
                var hciE = Marshal_PtrToStructure<BTH_HCI_EVENT_INFO>(pData);
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "HCI_EVENT: addr: {0:X}, type: {1}, conn: {2}",
                    hciE.bthAddress, hciE.connectionType, hciE.connected));
#endif
            }
                // -- New somewhere after WinXP.
            else if (BluetoothDeviceNotificationEvent.AuthenticationRequestEvent == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_AUTHENTICATION_REQUEST";
                // Same content as BluetoothRegisterForAuthenticationEx
            } else if (BluetoothDeviceNotificationEvent.KeyPressEvent == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_KEYPRESS_EVENT";
                // struct BTH_HCI_KEYPRESS_INFO {
                //   BTH_ADDR  BTH_ADDR; UCHAR   NotificationType; // HCI_KEYPRESS_XXX value }
            } else if (BluetoothDeviceNotificationEvent.HciVendorEvent == hdrHandle.dbch_eventguid) {
                text += "GUID_BLUETOOTH_HCI_VENDOR_EVENT";
            }
                // -- Unknown
            else {
                text += "Unknown event: " + hdrHandle.dbch_eventguid;
            }
            Debug.WriteLine("Text: " + text);
        }

        private static void DoDevTypPort(ref Message m, ref String text, ref DEV_BROADCAST_HDR hdr)
        {
#if false==false
            text += "Port: ";
            //DEV_BROADCAST_PORT hdrPort = (DEV_BROADCAST_PORT)m.GetLParam(typeof(DEV_BROADCAST_PORT));
            const int OffsetOfStringMember = 12; // ints not pointers, so fixed size.
            System.Diagnostics.Debug.Assert(OffsetOfStringMember
                == Marshal.OffsetOf(typeof(DEV_BROADCAST_PORT), "____dbcp_name").ToInt64());
            int cbSpaceForString = hdr.dbch_size - OffsetOfStringMember;
            String str;
            if (cbSpaceForString > 0) {
                Int64 startPtrXX = OffsetOfStringMember + m.LParam.ToInt64();
                IntPtr startPtr = (IntPtr)startPtrXX;
                // We won't use the length parameter in method PtrToStringUni as the
                // string we have here is null-terminated and often has trailing nulls
                // also, using the length overload would force their inclusion.
                str = System.Runtime.InteropServices.Marshal.PtrToStringUni(startPtr);
            } else {
                str = null;
            }
            text += str;
#endif
        }


        //--------------------------------------------------------------
        // From bth_def.h
        //--------------------------------------------------------------------------

        static class BluetoothDeviceNotificationEvent
        {
            /// <summary>
            /// &#x201C;&#x201D;
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bth")]
            public static readonly Guid BthPortDeviceInterface = new Guid("{0850302A-B344-4fda-9BE9-90576B8D46F0}");
            /// <summary>
            /// &#x201C;This message is sent when any of the following attributes of a remote Bluetooth device has changed:
            /// the device has been discovered, the class of device, name, connected state, or device remembered state.
            /// This message is also sent when these attributes are set or cleared.&#x201D;
            /// </summary>
            public static readonly Guid RadioInRange = new Guid("{EA3B5B82-26EE-450E-B0D8-D26FE30A3869}");
            /// <summary>
            /// &#x201C;This message is sent when a previously discovered device has not been found after the completion of the last inquiry.
            /// This message will not be sent for remembered devices.
            /// The BTH_ADDRESS structure is the address of the device that was not found.&#x201D;
            /// </summary>
            public static readonly Guid RadioOutOfRange = new Guid("{E28867C9-C2AA-4CED-B969-4570866037C4}");
            /// <summary>
            /// &#x201C;This message should be ignored by the application.
            /// If the application must receive PIN requests, the BluetoothRegisterForAuthentication function should be used.&#x201D;
            /// </summary>
            public static readonly Guid PinRequest = new Guid("{BD198B7C-24AB-4B9A-8C0D-A8EA8349AA16}");
            /// <summary>
            /// &#x201C;This message is sent when an L2CAP channel between the local radio and a remote Bluetooth device has been established or terminated. 
            /// For L2CAP channels that are multiplexers, such as RFCOMM, this message is only sent when the underlying channel is established, 
            /// not when each multiplexed channel, such as an RFCOMM channel, is established or terminated.&#x201D;
            /// </summary>
            public static readonly Guid L2capEvent = new Guid("{7EAE4030-B709-4AA8-AC55-E953829C9DAA}");
            /// <summary>
            /// &#x201C;This message is sent when a remote Bluetooth device connects or disconnects at the ACL level.&#x201D;
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hci")]
            public static readonly Guid HciEvent = new Guid("{FC240062-1541-49BE-B463-84C4DCD7BF7F}");
            // New somewhere after WinXP.
            public static readonly Guid AuthenticationRequestEvent = new Guid("{5DC9136D-996C-46DB-84F5-32C0A3F47352}");
            public static readonly Guid KeyPressEvent = new Guid("{D668DFCD-0F4E-4EFC-BFE0-392EEEC5109C}");
            public static readonly Guid HciVendorEvent = new Guid("{547247e6-45bb-4c33-af8c-c00efe15a71d}");
        }

        struct BTH_RADIO_IN_RANGE
        {
            internal BTH_DEVICE_INFO deviceInfo;
            internal BluetoothDeviceInfoProperties previousDeviceFlags;
        }

        struct BTH_RADIO_OUT_OF_RANGE
        {
            internal Int64 deviceAddress;
        }

        /// <summary>
        /// Buffer associated with GUID_BLUETOOTH_L2CAP_EVENT
        /// </summary>
        struct BTH_L2CAP_EVENT_INFO
        {
            /// <summary>
            /// Remote radio address which the L2CAP event is associated with
            /// </summary>
            internal readonly Int64 bthAddress;

            /// <summary>
            /// The PSM that is either being connected to or disconnected from
            /// </summary>
            internal readonly ushort psm;

            /// <summary>
            /// If != 0, then the channel has just been established.  If == 0, then the
            /// channel has been destroyed.  Notifications for a destroyed channel will
            /// only be sent for channels successfully established.
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            internal readonly bool connected;

            /// <summary>
            /// If != 0, then the local host iniated the l2cap connection.  If == 0, then
            /// the remote host initated the connection.  This field is only valid if
            /// connect is != 0.
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            internal readonly bool initiated;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "shutUpCompiler")]
            internal BTH_L2CAP_EVENT_INFO(Version shutUpCompiler)
            {
                this.bthAddress = 0;
                this.psm = 0;
                this.connected = false;
                this.initiated = false;
            }
        }

        enum HCI_CONNNECTION_TYPE : byte
        {
            ACL = (1),
            SCO = (2),
            LE = (3), // Added in Windows 8
        }

        /// <summary>
        /// Buffer associated with GUID_BLUETOOTH_HCI_EVENT
        /// </summary>
        struct BTH_HCI_EVENT_INFO
        {
            /// <summary>
            /// Remote radio address which the HCI event is associated with
            /// </summary>
            internal readonly Int64 bthAddress;

            /// <summary>
            /// HCI_CONNNECTION_TYPE_XXX value
            /// </summary>
            internal readonly HCI_CONNNECTION_TYPE connectionType;

            /// <summary>
            /// If != 0, then the underlying connection to the remote radio has just
            /// been estrablished.  If == 0, then the underlying conneciton has just been
            /// destroyed.
            /// </summary>
            [MarshalAs(UnmanagedType.U1)]
            internal readonly bool connected;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "shutUpCompiler")]
            internal BTH_HCI_EVENT_INFO(Version shutUpCompiler)
            {
                this.bthAddress = 0;
                this.connectionType = 0;
                this.connected = false;
            }
        }

        //--------

        enum RegisterDeviceNotificationFlags
        {
            DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000,
            DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001,
            DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004
        };

        static class UnsafeNativeMethods
        {
            //[Obsolete("SafeHandle one please!")]
            //[DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true,
            //    EntryPoint = "RegisterDeviceNotification")]
            //internal static extern IntPtr RegisterDeviceNotification_IntPtr(
            //   IntPtr hRecipient,
            //   ref DEV_BROADCAST_HANDLE notificationFilter,
            //   RegisterDeviceNotificationFlags flags
            //   );

            [DllImport("User32.dll", CharSet = CharSet.Unicode, SetLastError = true,
                EntryPoint = "RegisterDeviceNotification")]
            internal static extern RegisterDeviceNotificationSafeHandle RegisterDeviceNotification_SafeHandle(
               IntPtr hRecipient,
               ref DEV_BROADCAST_HANDLE notificationFilter,
               RegisterDeviceNotificationFlags flags
               );

            [DllImport("User32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool UnregisterDeviceNotification(IntPtr handle);
        }

        //--------------------------------------------------------------------------
        // From DBT.h
        //--------------------------------------------------------------------------

        enum WindowMessageId
        {
            DeviceChange = 0x0219,
            //
            ActivateApp = 0x1C,
        }//class

        enum Dbt
        {
            /// <summary>
            /// A request to change the current configuration (dock or undock) has been canceled. 
            /// </summary>
            ConfigChangeCanceled = 0x0019,
            /// <summary>
            /// The current configuration has changed, due to a dock or undock. 
            /// </summary>
            ConfigChanged = 0x0018,
            /// <summary>
            /// A custom event has occurred. 
            /// </summary>
            /// <remarks>Windows NT 4.0 and Windows 95:  This value is not supported.</remarks>
            CustomEvent = 0x8006,
            /// <summary>
            /// A device or piece of media has been inserted and is now available. 
            /// </summary>
            DeviceArrival = 0x8000,
            /// <summary>
            /// Permission is requested to remove a device or piece of media. Any application can deny this request and cancel the removal. 
            /// </summary>
            DeviceQueryRemove = 0x8001,
            /// <summary>
            /// A request to remove a device or piece of media has been canceled. 
            /// </summary>
            DeviceQueryRemoveFailed = 0x8002,
            /// <summary>
            /// A device or piece of media has been removed. 
            /// </summary>
            DeviceRemoveComplete = 0x8004,
            /// <summary>
            /// A device or piece of media is about to be removed. Cannot be denied. 
            /// </summary>
            DeviceRemovePending = 0x8003,
            /// <summary>
            /// A device-specific event has occurred. 
            /// </summary>
            DeviceTypeSpecific = 0x8005,
            /// <summary>
            /// A device has been added to or removed from the system. 
            /// </summary>
            /// <remarks>Windows NT 4.0 and Windows Me/98/95:  This value is not supported.</remarks>
            DevNodesChanged = 0x0007,
            /// <summary>
            /// Permission is requested to change the current configuration (dock or undock). 
            /// </summary>
            QueryChangeConfig = 0x0017,
            /// <summary>
            /// The meaning of this message is user-defined. 
            /// </summary>
            UserDefined = 0xFFFF,
        }//class

        enum DbtDevTyp : uint
        {
            /// <summary>
            /// oem-defined device type
            /// </summary>
            Oem = 0x00000000,
            /// <summary>
            /// devnode number
            /// /// </summary>
            DevNode = 0x00000001,
            /// <summary>
            /// 
            /// </summary>
            Volume = 0x00000002,
            /// <summary>
            /// l
            /// </summary>
            Port = 0x00000003,
            /// <summary>
            /// network resource
            /// </summary>
            Network = 0x00000004,
            /// <summary>
            /// device interface class
            /// </summary>
            DeviceInterface = 0x00000005,
            /// <summary>
            /// file system handle
            /// </summary>
            Handle = 0x00000006
        }//enum



        struct DEV_BROADCAST_HDR
        {
            internal Int32 dbch_size;
            internal DbtDevTyp dbch_devicetype;
            internal Int32 dbch_reserved;
        }//struct

        //[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,
        //   CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        struct DEV_BROADCAST_PORT
        {
            internal DEV_BROADCAST_HDR header;
            //[//error CS0647: Error emitting 'System.Runtime.InteropServices.MarshalAsAttribute' attribute -- 'SizeConst is required for a fixed string.'
            //System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr)]
            //public IntPtr ____dbcp_name;//String
            internal byte[] ____dbcp_name;//String
        }//struct

        struct DEV_BROADCAST_HANDLE
        {
            // 32-bit:     3*4 + 2*4 + 16 + 4 + a = 6*4+16 = 24+16 + a = 40 + a = 44
            // 64-bit: (3+1)*4 + 2*8 + 16 + 4 + a = 16+16+16+4 + a     = 52 + a = 56
            const int SizeWithoutFakeDataArray = 40;
            const int SizeOfOneByteWithPadding = 4;
            const int SizeWithFakeDataArray = SizeWithoutFakeDataArray + SizeOfOneByteWithPadding;
            static int ActualSizeWithFakeDataArray;

            public DEV_BROADCAST_HDR header;
            //--
            internal readonly IntPtr dbch_handle;
            internal readonly IntPtr dbch_hdevnotify;
            internal readonly Guid dbch_eventguid;
            internal readonly Int32 dbch_nameoffset;
            // We can't include the fake data array because we use this struct as 
            // the first field in other structs!
            // byte dbch_data__0; //dbch_data[1];

            //----
            public DEV_BROADCAST_HANDLE(IntPtr deviceHandle)
            {
                this.header.dbch_reserved = 0;
                this.dbch_hdevnotify = IntPtr.Zero;
                this.dbch_eventguid = Guid.Empty;
                this.dbch_nameoffset = 0;
                //--
                this.header.dbch_devicetype = DbtDevTyp.Handle;
                this.dbch_handle = deviceHandle;
                //System.Diagnostics.Debug.Assert(
                //    SizeWithoutFakeDataArray == System.Runtime.InteropServices.Marshal.SizeOf(typeof(DEV_BROADCAST_HANDLE)),
                //    "Size not as expected");
                if (ActualSizeWithFakeDataArray == 0) {
                    int actualSizeWithoutFakeDataArray = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DEV_BROADCAST_HANDLE));
                    ActualSizeWithFakeDataArray = Pad(1 + actualSizeWithoutFakeDataArray, IntPtr.Size);
                }
                this.header.dbch_size = ActualSizeWithFakeDataArray;
                //this.header.dbch_size = actualSizeWithoutFakeDataArray;

            }

            private static int Pad(int size, int alignment)
            {
                int x = size + alignment - 1;
                x /= alignment;
                x *= alignment;
                return x;
            }

        }//struct

        struct DEV_BROADCAST_HANDLE__withData
        {
            public DEV_BROADCAST_HDR header;
            //--
            readonly IntPtr dbch_handle;
            readonly IntPtr dbch_hdevnotify;
            readonly Guid dbch_eventguid;
            readonly Int32 dbch_nameoffset;
            readonly byte dbch_data__0; //dbch_data[1];

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="weAreForPinvoke")]
            private DEV_BROADCAST_HANDLE__withData(int weAreForPinvoke)
            {
                this.header = new DEV_BROADCAST_HDR();
                this.dbch_handle = IntPtr.Zero;
                this.dbch_hdevnotify = IntPtr.Zero;
                this.dbch_eventguid = Guid.Empty;
                this.dbch_nameoffset = this.dbch_nameoffset = 0;
                this.dbch_data__0 = this.dbch_data__0 = 0;
            }
        }//struct

        static class ShutUpCompiler
        {

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal static void ShutUpCompilerTheStructFieldAsUsed()
            {
                DEV_BROADCAST_HDR hdr;
                hdr.dbch_size = 0;
                hdr.dbch_devicetype = DbtDevTyp.DevNode;
                hdr.dbch_reserved = 0;
                //----
                DEV_BROADCAST_PORT port;
                port.header = hdr;
                port.____dbcp_name = null; //IntPtr.Zero;
                //----
                //DEV_BROADCAST_HANDLE handle;
                //handle.dbch_handle = IntPtr.Zero;
                //handle.dbch_hdevnotify = IntPtr.Zero;
                //handle.dbch_eventguid = Guid.Empty;
                //handle.dbch_nameoffset = 0;
                //
                BTH_RADIO_IN_RANGE inRange;
                inRange.deviceInfo.flags = 0;
                inRange.previousDeviceFlags = 0;
                BTH_RADIO_OUT_OF_RANGE oor;
                oor.deviceAddress = 0;
                //
                BTH_DEVICE_INFO di;
                di.address = 0;
                di.classOfDevice = 0;
                di.name = new byte[0];
            }
        }

#endif
#endif
    }
}
