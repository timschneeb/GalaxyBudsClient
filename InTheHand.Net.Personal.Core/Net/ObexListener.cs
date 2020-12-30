// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexListener
// 
// Copyright (c) 2003-2007 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

//#define ADD_SERVICE_NAME_TO_SDP_RECORD


using System;
using System.Net;
using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net
{
    /// <summary>
    /// Provides a simple, programmatically controlled OBEX protocol listener.
    /// </summary>
    public class ObexListener
    {
        //changed sdp to use the uuid16 of the obexpush service
        private static readonly byte[] ServiceRecordExpected = new byte[] {
		    0x35,0x25,0x09,0x00,0x01,0x35,0x03,0x19,
		    0x11,0x05,0x09,0x00,0x04,0x35,0x11,0x35,
		    0x03,0x19,0x01,0x00,0x35,0x05,0x19,0x00,
		    0x03,0x08,0x00,0x35,0x03,0x19,0x00,0x08,
		    0x09,0x03,0x03,0x35,0x02,0x08,0xFF};
        private const int ServiceRecordExpectedPortOffset = 26;

        /*0x35,0x33,0x09,0x00,0x01,0x35,0x11,0x1c,
        0x05,0x11,0x00,0x00,0x00,0x00,0x00,0x10,
        0x80,0x00,0x00,0x80,0x5F,0x9B,0x34,0xFB,
        0x09,0x00,0x04,0x35,0x11,0x35,0x03,0x19,
        0x01,0x00,0x35,0x05,0x19,0x00,0x03,0x08,
        0x00,0x35,0x03,0x19,0x00,0x08,0x09,0x03,
        0x03,0x35,0x02,0x08,0xFF};*/

        private ObexTransport transport;
#if NO_IRDA
        private readonly object iListener;
#else
        private IrDAListener iListener;
#endif
        private BluetoothListener bListener;
        BluetoothPublicFactory _btFactory;
        private TcpListener tListener;

        private volatile bool listening = false;

        /// <overloads>
        /// Initializes a new instance of the ObexListener class.
        /// </overloads>
        /// -
        /// <summary>
        /// Initializes a new instance of the ObexListener class using the Bluetooth transport.
        /// </summary>
        public ObexListener()
            : this(ObexTransport.Bluetooth)
        {
        }
        /// <summary>
        /// Initializes a new instance of the ObexListener class specifiying the transport to use.
        /// </summary>
        /// -
        /// <param name="transport">Specifies the transport protocol to use.
        /// </param>
        public ObexListener(ObexTransport transport)
            : this(transport, null)
        {
        }


        private ObexListener(ObexTransport transport, BluetoothPublicFactory factory)
        {
#if NETCF
            PlatformVerification.ThrowException();
#endif
            _btFactory = factory;
            switch (transport) {
                case ObexTransport.Bluetooth:
                    ServiceRecord record = CreateServiceRecord();
                    if (_btFactory == null) {
                        bListener = new BluetoothListener(BluetoothService.ObexObjectPush, record);
                    } else {
                        bListener = _btFactory.CreateBluetoothListener(BluetoothService.ObexObjectPush, record);
                    }
                    bListener.ServiceClass = ServiceClass.ObjectTransfer;
                    break;
                case ObexTransport.IrDA:
#if NO_IRDA
                    throw new NotSupportedException("No IrDA on this platform.");
#else
                    iListener = new IrDAListener("OBEX");
                    break;
#endif
                case ObexTransport.Tcp:
                    tListener = new TcpListener(IPAddress.Any, 650);
                    break;
                default:
                    throw new ArgumentException("Invalid transport specified");
            }
            this.transport = transport;
        }

        internal ObexListener(BluetoothPublicFactory factory)
            : this(ObexTransport.Bluetooth, factory)
        {
            Debug.Assert(factory != null, "NOT factory!=null");
            Debug.Assert(factory == _btFactory, "_btFactory NOT set correctly in other .ctor");
        }

        private static ServiceRecord CreateServiceRecord()
        {
            ServiceElement englishUtf8PrimaryLanguage = CreateEnglishUtf8PrimaryLanguageServiceElement();
            ServiceRecord record = new ServiceRecord(
                new ServiceAttribute(InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList,
                    new ServiceElement(ElementType.ElementSequence,
                        new ServiceElement(ElementType.Uuid16, (UInt16)0x1105))),
                new ServiceAttribute(InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProtocolDescriptorList,
                    ServiceRecordHelper.CreateGoepProtocolDescriptorList()),
#if ADD_SERVICE_NAME_TO_SDP_RECORD
                // Could add ServiceName, ProviderName etc here.
                new ServiceAttribute(InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.LanguageBaseAttributeIdList,
                    englishUtf8PrimaryLanguage),
                new ServiceAttribute(ServiceRecord.CreateLanguageBasedAttributeId(
                        InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ProviderName,
                        LanguageBaseItem.PrimaryLanguageBaseAttributeId),
                    new ServiceElement(ElementType.TextString, "32feet.NET")),
#endif
                //
                new ServiceAttribute(InTheHand.Net.Bluetooth.AttributeIds.ObexAttributeId.SupportedFormatsList,
                    new ServiceElement(ElementType.ElementSequence,
                        new ServiceElement(ElementType.UInt8, (byte)0xFF)))
                );
            return record;
        }

        private static ServiceElement CreateEnglishUtf8PrimaryLanguageServiceElement()
        {
            ServiceElement englishUtf8PrimaryLanguage = LanguageBaseItem.CreateElementSequenceFromList(
                new LanguageBaseItem[] {
                    new LanguageBaseItem("en", LanguageBaseItem.Utf8EncodingId, LanguageBaseItem.PrimaryLanguageBaseAttributeId)
                });
            return englishUtf8PrimaryLanguage;
        }

        // HACK Remove ObexListener.TestRecordAsExpected -- after one general release?
        private void TestRecordAsExpected(byte[] serviceRecord_Expected, BluetoothListener bListener)
        {
#if ! ADD_SERVICE_NAME_TO_SDP_RECORD
            serviceRecord_Expected[ServiceRecordExpectedPortOffset] = (byte)bListener.LocalEndPoint.Port;
            ServiceRecord record = bListener.ServiceRecord;
            byte[] actualRecordBytes = record.ToByteArray();
            ServiceRecord tmpSeeExpectedFormat = ServiceRecord.CreateServiceRecordFromBytes(serviceRecord_Expected);
            Arrays_Equal(serviceRecord_Expected, actualRecordBytes);
#endif
        }

        internal static void Arrays_Equal(byte[] expected, byte[] actual) // as NETCFv1 not Generic <T>
        {
            if (expected.Length != actual.Length) {
                throw new InvalidOperationException("diff lengs!!!");
            }
            for (int i = 0; i < expected.Length; ++i) {
                if (!expected[i].Equals(actual[i])) {
                    throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "diff at {0}, x: 0x{1:X2}, y: 0x{2:X2} !!!", i, expected[i], actual[i]));
                }
            }
        }

        //--------------------------------------------------------------

        #region Auth/Encrypt
        /// <summary>
        /// Get or set whether the transport connection (e.g. Bluetooth) will
        /// require Authentication.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Only Bluetooth supports this, TCP/IP and IrDA do not.
        /// On Bluetooth this uses <see cref="P:InTheHand.Net.Sockets.BluetoothListener.Authenticate">BluetoothListener.Authenticate</see>.
        /// </para>
        /// </remarks>
        public bool Authenticate
        {
            get
            {
                if (!IsBluetoothListener())
                    return false;
                return bListener.Authenticate;
            }
            set
            {
                if (!IsBluetoothListener()) 
                    throw new InvalidOperationException("Setting Authenticate is only supported on Bluetooth ObexListeners.");
                bListener.Authenticate = value;
            }
        }

        /// <summary>
        /// Get or set whether the transport connection (e.g. Bluetooth) will
        /// require Encryption.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Only Bluetooth supports this, TCP/IP and IrDA do not.
        /// On Bluetooth this uses <see cref="P:InTheHand.Net.Sockets.BluetoothListener.Encrypt">BluetoothListener.Encrypt</see>.
        /// </para>
        /// </remarks>
        public bool Encrypt
        {
            get
            {
                if (!IsBluetoothListener())
                    return false;
                return bListener.Encrypt;
            }
            set
            {
                if (!IsBluetoothListener())
                    throw new InvalidOperationException("Setting Encrypt is only supported on Bluetooth ObexListeners.");
                bListener.Encrypt = value;
            }
        }

        private bool IsBluetoothListener()
        {
            if (bListener == null) { // Non-Bluetooth
                Debug.Assert(tListener != null || iListener != null, "No listener created!");
                return false;
            }
            return true;
        }
        #endregion


        /// <summary>
        /// Gets a value that indicates whether the <see cref="T:InTheHand.Net.ObexListener"/> has been started.
        /// </summary>
        public bool IsListening
        {
            get
            {
                return listening;
            }
        }

        /// <summary>
        /// Allows this instance to receive incoming requests.
        /// </summary>
        public void Start()
        {
            switch (transport) {
                case ObexTransport.Bluetooth:
                    bListener.Start();
                    TestRecordAsExpected(ServiceRecordExpected, bListener);
                    break;
#if !NO_IRDA
                case ObexTransport.IrDA:
                    iListener.Start();
                    break;
#endif
                case ObexTransport.Tcp:
                    tListener.Start();
                    break;
            }

            listening = true;
        }

        /// <summary>
        /// Causes this instance to stop receiving incoming requests.
        /// </summary>
        public void Stop()
        {
            listening = false;
            switch (transport) {
                case ObexTransport.Bluetooth:
                    bListener.Stop();
                    break;
#if !NO_IRDA
                case ObexTransport.IrDA:
                    iListener.Stop();
                    break;
#endif
                case ObexTransport.Tcp:
                    tListener.Stop();
                    break;
            }
        }

        /// <summary>
        /// Shuts down the ObexListener.
        /// </summary>
        public void Close()
        {
            if (listening) {
                this.Stop();
            }
        }

        /// <summary>
        /// Waits for an incoming request and returns when one is received.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This method blocks waiting for a new connection.  It will
        /// return when a new connection completes or 
        /// <see cref="M:InTheHand.Net.ObexListener.Stop"/>/<see cref="M:InTheHand.Net.ObexListener.Close"/>
        /// has been called.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>Returns a <see cref="T:InTheHand.Net.ObexListenerContext"/>
        /// or <see langword="null"/> if
        /// <see cref="M:InTheHand.Net.ObexListener.Stop"/>/<see cref="M:InTheHand.Net.ObexListener.Close"/>
        /// has been called.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ObexListenerContext GetContext()
        {
            if (!listening) {
                throw new InvalidOperationException("Listener not started");
            }

            try {
                SocketClientAdapter s;

                switch (transport) {
                    case ObexTransport.Bluetooth:
                        s = new SocketClientAdapter(bListener.AcceptBluetoothClient());
                        break;
                    case ObexTransport.IrDA:
#if NO_IRDA
                        throw new NotSupportedException("No IrDA on this platform.");
#else
                        s = new SocketClientAdapter(iListener.AcceptIrDAClient());
                        break;
#endif
                    default:
                        s = new SocketClientAdapter(tListener.AcceptTcpClient());
                        break;
                }
                Debug.WriteLine(s.GetHashCode().ToString("X8") + ": Accepted", "ObexListener");

                return new ObexListenerContext(s);
            } catch {
                return null;
            }
        }
    }
}
