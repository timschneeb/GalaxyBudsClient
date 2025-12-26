// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.BluetoothEndPoint
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using InTheHand.Net.Sockets;
using Utils;
using System.Diagnostics.CodeAnalysis;


namespace InTheHand.Net
{
    /// <summary>
    /// Represents a network endpoint as a Bluetooth address and 
    /// a Service Class Id and/or a port number.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>The BluetoothEndPoint class contains the host, service class id and port 
    /// information needed by an application to connect to a service on a host.
    /// By combining the host's Bluetooth address and class id or port number, 
    /// the BluetoothEndPoint class forms a connection point to a service.
    /// </para>
    /// <para>When used for instance when connecting with <see cref="T:InTheHand.Net.Sockets.BluetoothClient"/>, 
    /// if the port is specified then the connection is made to that port, 
    /// otherwise a SDP lookup is done for a record with the class specified in 
    /// the <see cref="P:InTheHand.Net.BluetoothEndPoint.Service"/> property.
    /// </para>
    /// </remarks>
#if !NETCF
    [Serializable]
#endif
    public class BluetoothEndPoint : EndPoint
    {
        private BluetoothAddress m_id;
        private Guid m_service;
        private int m_port;
        static readonly AddressFamily _addrFamily = AddressFamily32.Bluetooth;
#if NETCF
        private const int defaultPort = 0;
#else
        private const int defaultPort = -1;
        //
        static readonly bool _isBlueZ;
#endif


        #region Class Constructor
#if !NETCF
        static BluetoothEndPoint()
        {
            // Unfortunately we need to do the platform specific EndPoint
            // handling here.  Otherwise we'd need to do it in our both our
            // Connect and Bind methods, but also in the LocalEndPoint and
            // RemoteEndPoint properties etc.
            if (Environment.OSVersion.Platform == PlatformID.Unix
                   || Environment.OSVersion.Platform == (PlatformID)128) {
                // Detect Linux where it'll be BlueZ one presumes...
                // Note Mono as of 2.2 at least returns the same for MacOsX.
                _isBlueZ = true;
                _addrFamily = AddressFamily32.BluetoothOnLinuxBlueZ;
            }
            //Debug.WriteLine("BluetoothEndPoint..cctor _isBlueZ: " + _isBlueZ);
        }
#endif
        #endregion

        #region Constructor
        private BluetoothEndPoint()
        {
            // For XmlSerialization (etc?) use only!
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothEndPoint"/> class with the specified address and service.
        /// </summary>
        /// <param name="address">The Bluetooth address of the device. A six byte array.</param>
        /// <param name="service">The Bluetooth service to use.</param>
        public BluetoothEndPoint(BluetoothAddress address, Guid service)
            : this(address, service, defaultPort)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothEndPoint"/> class with the specified address, service and port number.
        /// </summary>
        /// <param name="address">The Bluetooth address of the device. A six byte array.</param>
        /// <param name="service">The Bluetooth service to use.</param>
        /// <param name="port">Radio channel to use, -1 for any.</param>
        /// -
        /// <remarks>
        /// <para>See the <see cref="T:InTheHand.Net.BluetoothEndPoint"/> documentation for 
        /// how the combination of Service and Port are used when connecting with 
        /// BluetoothClient.
        /// </para>
        /// </remarks>
        public BluetoothEndPoint(BluetoothAddress address, Guid service, int port)
        {
            Debug.Assert(address != null, "NULL address");
            m_id = address;
            m_service = service;
            m_port = port;
        }
        #endregion

        // As seen below the structures on Win32 and WinCE are the same, except that 
        // Win32 has 1-byte alignment turned on.  Thus on Win32 there are also no 
        // 64-bit differences.
        //
        // * Win32
        //typedef ULONGLONG BTH_ADDR, *PBTH_ADDR;
        //
        //#include <pshpack1.h>
        //struct _SOCKADDR_BTH
        //{
        //    USHORT    addressFamily;  // Always AF_BTH
        //    BTH_ADDR  btAddr;         // Bluetooth device address
        //    GUID      serviceClassId; // [OPTIONAL] system will query SDP for port
        //    ULONG     port;           // RFCOMM channel or L2CAP PSM
        //}
        //
        // * WinCE
        //typedef ULONGLONG bt_addr, *pbt_addr, BT_ADDR, *PBT_ADDR;
        //
        //struct _SOCKADDR_BTH
        //{
        //    USHORT   addressFamily;
        //    bt_addr  btAddr;
        //    GUID     serviceClassId;
        //    ULONG    port;
        //}
        // * BlueZ
        //struct sockaddr_rc
        //{
        //    sa_family_t rc_family;
        //    bdaddr_t rc_bdaddr;
        //    uint8_t rc_channel;
        //};
        //typedef struct {
        //    uint8_t b[6];
        //} __attribute__((packed)) bdaddr_t;

        #region Serialize
        /// <summary>
        /// Serializes endpoint information into a <see cref="SocketAddress"/> instance.
        /// </summary>
        /// <returns>A <see cref="SocketAddress"/> instance containing the socket address for the endpoint.</returns>
        public override SocketAddress Serialize()
        {
#if NETCF
            SocketAddress btsa = new SocketAddress(AddressFamily32.Bluetooth, 40);
#else
            int salen = 30;
            if (_isBlueZ) salen = 10;
            SocketAddress btsa = new SocketAddress(_addrFamily, salen);
#endif
            //copy address type
            btsa[0] = checked((byte)_addrFamily);

            //copy device id
            if (m_id != null) {
                byte[] deviceidbytes = m_id.ToByteArray();

                for (int idbyte = 0; idbyte < 6; idbyte++) {
#if NETCF
                    btsa[idbyte + 8] = deviceidbytes[idbyte];
#else
                    btsa[idbyte + 2] = deviceidbytes[idbyte];
#endif
                }
            }

            //copy service clsid
            if (m_service != Guid.Empty) {
                byte[] servicebytes = m_service.ToByteArray();
                for (int servicebyte = 0; servicebyte < 16; servicebyte++) {
#if NETCF
                    btsa[servicebyte + 16] = servicebytes[servicebyte];
#else
                    if (_isBlueZ) // No SvcClassId field on BlueZ.
                        break;
                    btsa[servicebyte + 10] = servicebytes[servicebyte];
#endif
                }
            }

            //copy port
            byte[] portbytes = BitConverter.GetBytes(m_port);
            for (int portbyte = 0; portbyte < 4; portbyte++) {
#if NETCF
                btsa[portbyte + 32] = portbytes[portbyte];
#else
                if (_isBlueZ) {// One byte Channel field on BlueZ.
                    btsa[portbyte + 8] = portbytes[portbyte];
                    break;
                }
                btsa[portbyte + 26] = portbytes[portbyte];
#endif
            }

            //Dump("Serialize", btsa);
            return btsa;
        }
        #endregion

        #region Create
        /// <summary>
        /// Creates an endpoint from a socket address.
        /// </summary>
        /// <param name="socketAddress">The <see cref="SocketAddress"/> to use for the endpoint.</param>
        /// <returns>An <see cref="EndPoint"/> instance using the specified socket address.</returns>
        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress == null) {
                throw new ArgumentNullException("socketAddress");
            }
            //Dump("Create", socketAddress);

            //if a Bluetooth SocketAddress
            if (socketAddress[0] == (int)_addrFamily) {
                int ibyte;

                byte[] addrbytes = new byte[6];
                for (ibyte = 0; ibyte < 6; ibyte++) {
#if NETCF
                    addrbytes[ibyte] = socketAddress[8 + ibyte];
#else
                    addrbytes[ibyte] = socketAddress[2 + ibyte];
#endif
                }

                byte[] servicebytes = new byte[16];
                for (ibyte = 0; ibyte < 16; ibyte++) {
#if NETCF
                    servicebytes[ibyte] = socketAddress[16 + ibyte];
#else
                    if (_isBlueZ) // No SvcClassId field on BlueZ.
                        break;
                    servicebytes[ibyte] = socketAddress[10 + ibyte];
#endif
                }

                byte[] portbytes = new byte[4];
                for (ibyte = 0; ibyte < 4; ibyte++) {
#if NETCF
                    portbytes[ibyte] = socketAddress[32 + ibyte];
#else
                    if (_isBlueZ) {// One byte Channel field on BlueZ.
                        portbytes[ibyte] = socketAddress[8 + ibyte];
                        break;
                    }
                    portbytes[ibyte] = socketAddress[26 + ibyte];
#endif
                }

                return new BluetoothEndPoint(new BluetoothAddress(addrbytes), new Guid(servicebytes), BitConverter.ToInt32(portbytes, 0));

            } else {
#if DEBUG
                var len = socketAddress.Size;
                var arr = new byte[Math.Min(32, len)];
                Array_Copy(socketAddress, arr, arr.Length);
                var txt = BitConverter.ToString(arr);
                Debug.WriteLine("Non BTH sa passed to BluetoothEndPoint.Create: (len: " + len
                    + ") " + txt);
#endif
                //use generic method
                return base.Create(socketAddress);
            }
        }

        private void Array_Copy(SocketAddress src, byte[] dst, int count)
        {
            Debug.Assert(count <= src.Size);
            for (int i = 0; i < count; ++i) {
                dst[i] = src[i];
            }
        }
        #endregion

        #region Equals
        /// <summary>
        /// Compares two <see cref="BluetoothEndPoint"/> instances for equality.
        /// </summary>
        /// -
        /// <param name="obj">The <see cref="BluetoothEndPoint"/>
        /// to compare with the current instance.
        /// </param>
        /// -
        /// <returns><c>true</c> if <paramref name="obj"/>
        /// is a <see cref="BluetoothEndPoint"/> and equal to the current instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            BluetoothEndPoint bep = obj as BluetoothEndPoint;

            if (bep != null) {
                return (this.Address.Equals(bep.Address) && this.Service.Equals(bep.Service));
            }

            return base.Equals(obj);

        }
        #endregion

        #region Get Hash Code
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            // TODO (Include Service and Port in the GetHashCode)
            return this.Address.GetHashCode();
        }
        #endregion

        #region To String
        /// <summary>
        /// Returns the string representation of the BluetoothEndPoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We try to follow existing examples where possible; JSR-82 and similar
        /// use a URI of the form:</para>
        /// <code lang="none">bluetooth://xxxxxxxxxxxx:xx</code>
        /// or:
        /// <code lang="none">bluetooth://xxxxxxxxxxxx:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx</code>
        /// or in some serialport only situations:
        /// <code lang="none">btspp://</code>
        /// <para>So we follow that pattern here, but of course without the URI prefix.
        /// If the form with the URI is required then the prefix can simply be appended.</para>
        /// <para>
        /// If the port is non default then we use that, otherwise just the full guid.
        /// </para>
        /// <para>Some examples are:</para>
        /// To the ObexObjectPush service:
        /// <code lang="none">"04E2030405F6:0000110500001000800000805f9b34fb"</code>
        /// To the SerialPort service:
        /// <code lang="none">"04E2030405F6:0000110100001000800000805f9b34fb"</code>
        /// With an Empty service GUID:
        /// <code lang="none">"04E2030405F6:00000000000000000000000000000000"</code>
        /// With port 9:
        /// <code lang="none">"04E2030405F6:9"</code>
        /// </remarks>
        /// <returns>The string representation of the BluetoothEndPoint.</returns>
        public override string ToString()
        {
            //if port is set then use that in uri else use full service guid
            if (this.m_port != defaultPort) {
                return Address.ToString() + ":" + Port.ToString();
            } else {
                return Address.ToString() + ":" + Service.ToString("N");
            }
        }
        #endregion

        #region Address Family
        /// <summary>
        /// Gets the address family of the Bluetooth address. 
        /// </summary>
        public override AddressFamily AddressFamily
        {
            [DebuggerStepThrough]
            get { return (AddressFamily)32; }
        }
        #endregion

        #region Address
        /// <summary>
        /// Gets or sets the Bluetooth address of the endpoint.
        /// </summary>
        /// <seealso cref="BluetoothAddress"/>
        public BluetoothAddress Address
        {
            [DebuggerStepThrough]
            get { return m_id; }
            [DebuggerStepThrough]
            set { m_id = value; }
        }
        #endregion

        #region Service
        /// <summary>
        /// Gets or sets the Bluetooth service to use for the connection.
        /// </summary>
        /// <seealso cref="Bluetooth.BluetoothService"/>
        public Guid Service
        {
            [DebuggerStepThrough]
            get { return m_service; }
            [DebuggerStepThrough]
            set { m_service = value; }
        }
        #endregion

        #region Port
        /// <summary>
        /// Gets or sets the service channel number of the endpoint.
        /// </summary>
        public int Port
        {
            [DebuggerStepThrough]
            get { return m_port; }
            [DebuggerStepThrough]
            set { m_port = value; }
        }

        /// <summary>
        /// Gets whether a <see cref="P:InTheHand.Net.BluetoothEndPoint.Port"/> is set.
        /// </summary>
        public bool HasPort
        {
            get
            {
                // To suit both CE and desktop Windows.
                if (m_port == 0) return false;
                if (m_port == -1) return false;
                return true;
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a copy of the <see cref="BluetoothEndPoint"/>.
        /// </summary>
        /// <remarks>Creates a copy including of the internal <see cref="T:InTheHand.Net.BluetoothAddress"/>
        /// </remarks>
        /// <returns>A copy of the <see cref="BluetoothEndPoint"/>.
        /// </returns>
        public object Clone()
        {
            BluetoothEndPoint addr2 = new BluetoothEndPoint(
                (BluetoothAddress)this.Address.Clone(), this.Service, this.Port);
            return addr2;
        }
        #endregion

        #region Consts
        /// <summary>
        /// Specifies the minimum value that can be assigned to the Port property.
        /// </summary>
        public const int MinPort = 1;

        /// <summary>
        /// Specifies the maximum value that can be assigned to the Port property.
        /// </summary>
        public const int MaxPort = 0xffff;


        /// <summary>
        /// The minimum valid Server Channel Number, 1.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Bluetooth's rfcomm.pdf: Part F:1 -- RFCOMM with TS 07.10 -- Serial Port Emulation
        /// </para>
        /// <para>
        /// Section 5.4:
        /// </para>
        /// <list type="table">
        ///    &#x201C;The RFCOMM server channel number is a [five-bit field].
        ///    Server applications registering with an RFCOMM service interface are assigned a
        ///    Server Channel number in the range 1�30. [0 and 31 should not be used since
        ///    the corresponding DLCIs are reserved in TS 07.10]&#x201D;
        /// </list>
        /// </remarks>
        public const int MinScn = 1;
        /// <summary>
        /// The maximum valid Server Channel Number, 30.
        /// </summary>
        /// <remarks><see cref="F:InTheHand.Net.BluetoothEndPoint.MinScn"/>
        /// </remarks>
        public const int MaxScn = 30;
        #endregion

        #region Utils
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.String.Format(System.IFormatProvider,System.String,System.Object[])")]
        internal static void Dump(string name, SocketAddress btsa)
        {
            var bldr = new System.Text.StringBuilder(3 * btsa.Size);
            for (int i = 0; i < btsa.Size; ++i) {
                bldr.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "{0:X2} ", btsa[i]);
            }
            if (bldr.Length > 0) bldr.Length -= 1;
            var t = $"SA @ {name,9}: family: {btsa.Family} 0x{btsa.Family:X}, size: {btsa.Size}, < {bldr} >";
            Debug.WriteLine(t);
            MiscUtils.ConsoleDebug_WriteLine(t + ".");
        }
        #endregion
    }
}
