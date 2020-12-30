// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Sockets.BluetoothDeviceInfo
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Sockets
{
    /// <summary>
    /// Provides information about an available device obtained by the client during device discovery.
    /// </summary>
    public class BluetoothDeviceInfo : IComparable
    {
        readonly IBluetoothDeviceInfo m_impl;

        //--------
        internal BluetoothDeviceInfo(IBluetoothDeviceInfo impl)
        {
            m_impl = impl;
        }

        // Needed for SdpBrowserCF
        /// <summary>
        /// Initializes an instance of the <see cref="T:InTheHand.Net.Sockets.BluetoothDeviceInfo"/> class 
        /// for the device with the given address.
        /// </summary>
        /// -
        /// <param name="address">The <see cref="T:InTheHand.Net.BluetoothAddress"/>.</param>
        public BluetoothDeviceInfo(BluetoothAddress address)
            : this(BluetoothFactory.Factory.DoGetBluetoothDeviceInfo(address))
        {
        }

        //--------
        internal void Merge(IBluetoothDeviceInfo other) // used by BluetoothClient.DiscoverDevicesMerge etc
        {
            // TODO make this a throw
            Debug.Assert(this.DeviceAddress.Equals(other.DeviceAddress), "diff.addresses!");
            m_impl.Merge(other);
        }

        //--------

        /// <summary>
        /// Forces the system to refresh the device information.
        /// </summary>
        /// -
        /// <remarks>
        /// See <see cref="P:InTheHand.Net.Sockets.BluetoothDeviceInfo.DeviceName"/>
        /// for one reason why this method is necessary.
        /// </remarks>
        public void Refresh()
        {
            m_impl.Refresh();
        }

        /// <summary>
        /// Updates the device name used to display the device, affects the local computer cache.
        /// </summary>
        /// <remarks>On Windows CE this only affects devices which are already paired.</remarks>
        public void Update()
        {
            m_impl.Update();
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        public BluetoothAddress DeviceAddress
        {
            [DebuggerStepThrough]
            get { return m_impl.DeviceAddress; }
        }

        /// <summary>
        /// Gets a name of a device.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Note, that due the way in which Bluetooth device discovery works,
        /// the existence and address of a device is known first, but a separate
        /// query has to be carried out to find whether the device also has a name.
        /// This means that if a device is discovered afresh then this property might
        /// return only a text version of the device&#x2019;s address and not its
        /// name, one can also see this in the Windows&#x2019; Bluetooth device dialogs
        /// where the device appears first with its address and the name is later
        /// updated.  To see the name, wait for some time and access this property again
        /// having called <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.Refresh"/>
        /// in the meantime.
        /// </para>
        /// </remarks>
        public string DeviceName
        {
            [DebuggerStepThrough]
            get { return m_impl.DeviceName; }
            set { m_impl.DeviceName = value; }
        }

        /// <summary>
        /// Returns the Class of Device of the remote device.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// Some CE 4.2 devices such as original PPC2003 devices don't have the native 
        /// API on which this property depends &#x2014; it was added as part of a hotfix. 
        /// The property will always return zero in such a case.  On WM/CE we also 
        /// attempt to get the CoD value as part of the discovery process; this is 
        /// of course only works for devices in-range.
        /// </para>
        /// </remarks>
        public ClassOfDevice ClassOfDevice
        {
            get { return m_impl.ClassOfDevice; }
        }

        /// <summary>
        /// Returns the signal strength for the Bluetooth connection with the peer device.
        /// <para><b>Supports only on some platforms.</b></para>
        /// </summary>
        /// -
        /// <value>Valid values for this property are -128 to 128.  It returns
        /// <see cref="F:System.Int32.MinValue">Int32.MinValue</see> on failure.
        /// </value>
        /// -
        /// <remarks>
        /// <para>Thus there are multiple reasons which this property can return
        /// the error value (i.e. <see cref="F:System.Int32.MinValue">Int32.MinValue</see>).
        /// </para>
        /// <list type="number">
        /// <item>On an unsupported platform, e.g. MSFT+Win32, or MSFT+CE/WM on an
        /// older version.  See below.
        /// </item>
        /// <item>The remote device is not turned-on or in range.  See below.
        /// </item>
        /// <item>On Widcomm, there is no connection to the remote device.  See below.
        /// </item>
        /// </list>
        /// 
        /// <para>Platform support:</para>
        /// <list type="bullet">
        /// <item>Does <b>not</b> work on Win32 with the Microsoft Bluetooth stack.
        /// That platform provide no support for RSSI, please contact Microsoft
        /// to complain.
        /// </item>
        /// <item>Works on Windows Mobile 5.0, Windows Embedded CE 6.0, or later
        /// versions.
        /// </item>
        /// <item>Works on Widcomm, both platforms.
        /// We will <i>not</i> try to connect, see below.
        /// </item>
        /// </list>
        /// <para>
        /// </para>
        /// 
        /// <para>Finally, to get an RSSI value Bluetooth requires an open
        /// connection to the peer device.
        /// On Widcomm we will <i>not</i> attempt to connect, so the caller must
        /// ensure that there's a connection --
        /// perhaps it could call <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.GetServiceRecords(System.Guid)"/>
        /// just before accessing this property.
        /// On CE/WM if there is no active connection, then we will attempt to
        /// create one.  This of course <i>can</i> be <i>slow</i>, and <i>will</i>
        /// be slow if the remote device is not in range.
        /// (Bluetooth 2.1 supports getting the RSSI value at discovery time which
        /// might provide the solution for many cases.  However only the MSFT+Win32
        /// stack specifically supports v2.1, and of course it doesn't support RSSI
        /// at all!)
        /// </para>
        /// <para>Note that the Bluetooth specification doesn't require that the
        /// radio hardware provides any great precision in its RSSI readings.
        /// The spec says for instance, in v2.1 Volume 2 Part E ("HCI") Section 7.5.4:
        /// &#x201C;Note: how accurate the dB values will be depends on the Bluetooth hardware.
        /// The only requirements for the hardware are that the Bluetooth device is able to
        /// tell whether the RSSI is inside, above or below the Golden Device Power Range.&#x201D;
        /// </para>
        /// </remarks>
        public int Rssi { get { return m_impl.Rssi; } }

        /// <summary>
        /// Returns a list of services which are already installed for use on the calling machine.
        /// </summary>
        /// <remarks>
        /// <para>This property returns the services already configured for use. 
        /// Those are the ones that are checked in the &#x201C;Services&#x201D; tab
        /// of the device&#x2019;s property sheet in the Bluetooth Control panel.
        /// I presume the behaviour is similar on CE.
        /// </para>
        /// <para>Will only return available services for paired devices.
        /// </para>
        /// <para>It of course will also only returns standard system services which Windows understands.
        /// (On desktop Windows this method calls the OS function <c>BluetoothEnumerateInstalledServices</c>).
        /// </para>
        /// <para>To see all the services that a device advertises use the 
        /// <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.GetServiceRecords(System.Guid)"/>
        /// method.
        /// </para>
        /// </remarks>
        public Guid[] InstalledServices
        {
            get { return m_impl.InstalledServices; }
        }

        #region SetServiceState
        /// <summary>
        /// Enables or disables services for a Bluetooth device.
        /// </summary>
        /// <param name="service">The service GUID on the remote device.</param>
        /// <param name="state">Service state - TRUE to enable the service, FALSE to disable it.</param>
        /// <remarks>
        /// When called on Windows CE, the device will require a soft-reset to enabled the settings.
        /// 
        ///<note>
        /// <para>The system maintains a mapping of service guids to supported drivers for
        /// Bluetooth-enabled devices. Enabling a service installs the corresponding
        /// device driver. Disabling a service removes the corresponding device driver.
        /// If a non-supported service is enabled, a driver will not be installed.
        /// </para>
        /// </note>
        /// <para>This overload is silent on error; the other overload raises an exception
        /// if required
        /// (<see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.SetServiceState(System.Guid,System.Boolean,System.Boolean)"/>).
        /// </para>
        /// </remarks>
        /// -
        /// <exception cref="T:System.PlatformNotSupportedException">
        /// Thrown if this method is called on Windows CE platforms.</exception>
        public void SetServiceState(Guid service, bool state)
        {
            m_impl.SetServiceState(service, state);
        }

        /// <summary>
        /// Enables or disables services for a Bluetooth device.
        /// </summary>
        /// <param name="service">The service GUID on the remote device.</param>
        /// <param name="state">Service state - TRUE to enable the service, FALSE to disable it.</param>
        /// <param name="throwOnError">Whether the method should raise an exception
        /// when 
        /// </param>
        /// <remarks>
        /// When called on Windows CE, the device will require a soft-reset to enabled the settings.
        ///<note>
        /// <para>The system maintains a mapping of service guids to supported drivers for
        /// Bluetooth-enabled devices. Enabling a service installs the corresponding
        /// device driver. Disabling a service removes the corresponding device driver.
        /// If a non-supported service is enabled, a driver will not be installed.
        /// </para>
        /// </note>
        /// </remarks>
        /// -
        /// <exception cref="T:System.ComponentModel.Win32Exception">The call failed.
        /// </exception>
        public void SetServiceState(Guid service, bool state, bool throwOnError)
        {
            m_impl.SetServiceState(service, state, throwOnError);
        }
        #endregion

        #region GetServiceRecords
        /// <summary>
        /// Run an SDP query on the device&#x2019;s Service Discovery Database.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// For instance to see whether the device has an an Serial Port service
        /// search for UUID <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.SerialPort"/>,
        /// or too find all the services that use RFCOMM use 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.RFCommProtocol"/>,
        /// or all the services use 
        /// <see cref="F:InTheHand.Net.Bluetooth.BluetoothService.L2CapProtocol"/>.
        /// </para>
        /// <para>
        /// If the device isn&#x2019;t accessible a <see cref="T:System.Net.Sockets.SocketException"/>
        /// with <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/>
        /// 10108 (0x277C) occurs.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="service">The UUID to search for, as a <see cref="T:System.Guid"/>.
        /// </param>
        /// -
        /// <returns>The parsed record as an 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>.
        /// </returns>
        /// -
        /// <example>
        /// <code lang="VB.NET">
        /// Dim bdi As BluetoothDeviceInfo = ...
        /// Dim records As ServiceRecord() = bdi.GetServiceRecords(BluetoothService.RFCommProtocol)
        /// ' Dump each to console
        /// For Each curRecord As ServiceRecord In records
        ///    ServiceRecordUtilities.Dump(Console.Out, curRecord)
        /// Next
        /// </code>
        /// </example>
        /// 
        /// -
        /// <exception cref="T:System.Net.Sockets.SocketException">
        /// The query failed.
        /// </exception>
        public ServiceRecord[] GetServiceRecords(Guid service)
        {
            return m_impl.GetServiceRecords(service);
        }

        /// <summary>
        /// Begins an asynchronous Service Record lookup query.
        /// </summary>
        /// -
        /// <param name="service">See <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.GetServiceRecords(System.Guid)"/>.
        /// </param>
        /// <param name="callback">An optional asynchronous callback, to be called 
        /// when the query is complete.
        /// </param>
        /// <param name="state">A user-provided object that distinguishes this 
        /// particular asynchronous Service Record lookup query from other requests.
        /// </param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous Service Record lookup query, which could still be pending.
        /// </returns>
        public IAsyncResult BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
#if !V1
            return m_impl.BeginGetServiceRecords(service, callback, state);
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// Ends an asynchronous Service Record lookup query.
        /// </summary>
        /// -
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/>
        /// object that was obtained when the asynchronous operation was started.
        /// </param>
        /// -
        /// <returns>The parsed record as an 
        /// <see cref="T:InTheHand.Net.Bluetooth.ServiceRecord"/>.
        /// </returns>
        public ServiceRecord[] EndGetServiceRecords(IAsyncResult asyncResult)
        {
#if !V1
            return m_impl.EndGetServiceRecords(asyncResult);
#else
            throw new NotSupportedException();
#endif
        }

#if FX4
        public System.Threading.Tasks.Task<ServiceRecord[]> GetServiceRecordsAsync(Guid service, object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync<Guid, ServiceRecord[]>(
                BeginGetServiceRecords, EndGetServiceRecords,
                service, state);
        }
#endif

        /// <summary>
        /// Run an SDP query on the device&#x2019;s Service Discovery Database,
        /// returning the raw byte rather than a parsed record.
        /// </summary>
        /// -
        /// <remarks>
        /// If the device isn&#x2019;t accessible a <see cref="T:System.Net.Sockets.SocketException"/>
        /// with <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/>
        /// 10108 (0x277C) occurs.
        /// </remarks>
        /// -
        /// <param name="service">The UUID to search for, as a <see cref="T:System.Guid"/>.
        /// </param>
        /// -
        /// <returns>An array of array of <see cref="T:System.Byte"/>.</returns>
        /// -
        /// <exception cref="T:System.Net.Sockets.SocketException">
        /// The query failed.
        /// </exception>
        public byte[][] GetServiceRecordsUnparsed(Guid service)
        {
            return m_impl.GetServiceRecordsUnparsed(service);
        }
        #endregion

        /// <summary>
        /// Gets the radio version and manufacturer information for the device.
        /// Needs a connection to the device.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Includes information such as the LMP versions, supported
        /// features and the manufacturer of the radio/Bluetooth Controller.
        /// </para>
        /// <para>If the device is not connected this information cannot be
        /// obtained; an error will occur if there is no connection.
        /// The values will be cached until <see cref="Refresh"/> is called.
        /// </para>
        /// <para>This feature is currently supported only on the
        /// Microsoft Bluetooth stack on both desktop Windows and Windows
        /// Mobile. However Windows XP does not provide this information.
        /// Implementation is possible on some of the other Bluetooth stacks
        /// and will depend on demand/support for the user community.
        /// </para>
        /// </remarks>
        /// -
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// An error occurred, desktop Windows returns error code
        /// 1167 ERROR_DEVICE_NOT_CONNECTED and Windows Mobile returns error code
        /// 1168 ERROR_NOT_FOUND.
        /// Windows XP which does not support this functionality returns error code
        /// 2 ERROR_FILE_NOT_FOUND.
        /// </exception>
        /// <exception cref="System.NotImplementedException">
        /// Not yet implemented.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// This stack does not support getting this information.
        /// </exception>
        /// -
        /// <returns>The radio version etc information as a
        /// <see cref="RadioVersions"/> instance.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public RadioVersions GetVersions()
        {
            return m_impl.GetVersions();
        }

        /// <summary>
        /// Specifies whether the device is connected.
        /// </summary>
        /// <remarks>Not supported under Windows CE and will always return false.</remarks>
        /// <seealso cref="Remembered"/>
        /// <seealso cref="Authenticated"/>
        public bool Connected
        {
            get { return m_impl.Connected; }
        }

        /// <summary>
        /// Specifies whether the device is a remembered device. Not all remembered devices are authenticated.
        /// </summary>
        /// -
        /// <remarks>Now supported under Windows CE &#x2014; will return the same as 
        /// <see cref="P:InTheHand.Net.Sockets.BluetoothDeviceInfo.Authenticated"/>.
        /// </remarks>
        /// <seealso cref="Connected"/>
        /// <seealso cref="Authenticated"/>
        public bool Remembered
        {
            get { return m_impl.Remembered; }
        }

        /// <summary>
        /// Specifies whether the device is authenticated, paired, or bonded. All authenticated devices are remembered.
        /// </summary>
        /// <remarks>Is now supported on both CE and XP.</remarks>
        /// <seealso cref="Connected"/>
        /// <seealso cref="Remembered"/>
        public bool Authenticated
        {
            get { return m_impl.Authenticated; }
        }

        /// <summary>
        /// Date and Time this device was last seen by the system.
        /// </summary>
        /// -
        /// <remarks><para>Is set by the Inquiry (Device Discovery) process on
        /// the stacks where we handle Inquiry directly &#x2014; that is
        /// every platform except the Microsoft stack on Win32 (MSFT+Win32),
        /// so is supported under MSFT+WM, Widcomm, Bluetopia, etc, etc.
        /// </para>
        /// <para>This value is supported on Windows 7 with the Microsoft stack.
        /// It it not supported on earlier Win32 versions as the native 
        /// API has a bug.  The value provided is always simply the current 
        /// time, e.g. after a discovery for every device returned this value has 
        /// the time of the discovery operation.  Tracked by workitem 
        /// <see href="http://www.codeplex.com/32feet/WorkItem/View.aspx?WorkItemId=10280">10280</see>.
        /// </para>
        /// </remarks>
        /// -
        /// <value>
        /// An instance of <see cref="T:System.DateTime"/> containing the time in UTC,
        /// or <c>DateTime</c>.<see cref="F:System.DateTime.MinValue"/>
        /// if there's no value.
        /// </value>
        public DateTime LastSeen
        {
            get
            {
                AssertUtc(m_impl.LastSeen);
                return m_impl.LastSeen;
            }
        }

        /// <summary>
        /// Date and Time this device was last used by the system.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Not supported on most stacks: Widcomm, Bluetopia, MSFT+WM 
        /// and will return <see cref="F:System.DateTime.MinValue">DateTime.MinValue</see>
        /// </para>
        /// <para>Is supported on Windows 7 with the Microsoft stack.  Is not
        /// supported on earlier Win32 versions &#x2014; there it just always
        /// returns the current time, see <see cref="P:InTheHand.Net.Sockets.BluetoothDeviceInfo.LastSeen"/>.
        /// </para>
        /// </remarks>
        /// -
        /// <value>
        /// An instance of <see cref="T:System.DateTime"/> containing the time in UTC,
        /// or <c>DateTime</c>.<see cref="F:System.DateTime.MinValue"/> 
        /// if there's no value.
        /// </value>
        public DateTime LastUsed
        {
            get
            {
                AssertUtc(m_impl.LastUsed);
                return m_impl.LastUsed;
            }
        }

        private void AssertUtc(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
                return;
            Debug.Assert(dateTime.Kind == DateTimeKind.Utc, "Is: " + dateTime.Kind);
        }

        /// <summary>
        /// Displays information about the device.
        /// </summary>
        public void ShowDialog()
        {
            m_impl.ShowDialog();
        }

        //--------

        #region Equals
        /// <summary>
        /// Compares two <see cref="BluetoothDeviceInfo"/> instances for equality.
        /// </summary>
        /// -
        /// <param name="obj">The <see cref="BluetoothDeviceInfo"/>
        /// to compare with the current instance.
        /// </param>
        /// -
        /// <returns><c>true</c> if <paramref name="obj"/>
        /// is a <see cref="BluetoothDeviceInfo"/> and equal to the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            //objects are equal if device address matches
            BluetoothDeviceInfo bdi = obj as BluetoothDeviceInfo;
            if (bdi != null) {
                return this.DeviceAddress.Equals(bdi.DeviceAddress);
            }
            if (!IsAMsftInternalType(obj)) {
                Debug.Fail("Who's comparing "
                    + (obj == null ? "<null>" : "'" + obj.GetType().FullName + "'")
                    + " to BDI!");
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// E.g. used internally by WPF.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static bool IsAMsftInternalType(object obj)
        {
            bool msft = obj != null
                && obj.GetType().FullName.Equals("MS.Internal.NamedObject", StringComparison.Ordinal);
            return msft;
        }
        #endregion

        #region Auxiliary Equals methods
        internal static bool EqualsIBDI(IBluetoothDeviceInfo x, IBluetoothDeviceInfo y)
        {
            bool eq = x.DeviceAddress.Equals(y.DeviceAddress);
            return eq;
        }

        internal static bool EqualsIBDI(IBluetoothDeviceInfo x, object obj)
        {
            IBluetoothDeviceInfo y = obj as IBluetoothDeviceInfo;
            if (y != null) {
                return EqualsIBDI(x, y);
            }
            Debug.Fail("Who's comparing "
                + (obj == null ? "<null>" : "'" + obj.GetType().FullName + "'")
                + " to BDI!");
            return object.Equals(x, obj);
        }

        internal static int ListIndexOf(System.Collections.Generic.List<IBluetoothDeviceInfo> list, IBluetoothDeviceInfo item)
        {
            int idx = list.FindIndex(
                delegate(IBluetoothDeviceInfo cur) { return item.DeviceAddress.Equals(cur.DeviceAddress); });
            return idx;
        }

        internal static bool AddUniqueDevice(List<IBluetoothDeviceInfo> list,
            IBluetoothDeviceInfo bdi)
        {
            int idx = BluetoothDeviceInfo.ListIndexOf(list, bdi);
            AssertManualExistsIf(idx, list, bdi);
            if (idx == -1) {
                list.Add(bdi);
                return true;
            } else {
                Debug.WriteLine("Replace device");
                // Check the new info versus the previously discovered device.
                IBluetoothDeviceInfo bdiOld = list[idx];
                Debug.Assert(bdiOld.DeviceAddress.Equals(bdi.DeviceAddress));
                //Debug.Assert(deviceName != null);
                //Debug.Assert(deviceName.Length != 0);
                //Debug.Assert(bdiOld.ClassOfDevice.Equals(bdi.ClassOfDevice));
                // Replace
                list[idx] = bdi;
                return false;
            }
        }

        [Conditional("DEBUG")]
        private static void AssertManualExistsIf(int index, List<IBluetoothDeviceInfo> list, IBluetoothDeviceInfo item)
        {
            bool found = false;
            foreach (IBluetoothDeviceInfo cur in list) {
                if (cur.DeviceAddress == item.DeviceAddress)
                    found = true;
            }
            Debug.Assert(found == (index != -1), "manual found != list->object found");
        }

        internal static IBluetoothDeviceInfo[] Intersect(IBluetoothDeviceInfo[] list, List<IBluetoothDeviceInfo> seenDevices)
        {
            var copy = new List<IBluetoothDeviceInfo>(list.Length);
            foreach (var cur in list) {
                if (-1 != BluetoothDeviceInfo.ListIndexOf(seenDevices, cur)) {
                    copy.Add(cur);
                }
            }//for
            return copy.ToArray();
        }

        #endregion

        #region Get Hash Code
        /// <summary>
        /// Returns the hash code for this instance. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return this.DeviceAddress.GetHashCode();
        }
        #endregion


        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            //objects are equal if device address matches
            BluetoothDeviceInfo bdi = obj as BluetoothDeviceInfo;
            if (bdi != null) {
                return ((IComparable)this.DeviceAddress).CompareTo(bdi);
            }
            return -1;
        }

        #endregion

        //----
        internal static BluetoothDeviceInfo[] Wrap(IBluetoothDeviceInfo[] orig)
        {
            // Bah no Array.ConvertAll method in NETCF.
            BluetoothDeviceInfo[] wrapped = new BluetoothDeviceInfo[orig.Length];
            for (int i = 0; i < orig.Length; ++i) {
                wrapped[i] = new BluetoothDeviceInfo(orig[i]);
            }
            return wrapped;
        }

    }
}
