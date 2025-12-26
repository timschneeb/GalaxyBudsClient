// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexWebRequest
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// SYSLIB0014: WebRequest is obsolete in .NET 6+ but required for OBEX implementation structure.
#pragma warning disable SYSLIB0014

namespace InTheHand.Net
{
#pragma warning disable SYSLIB0014
    /// <summary>
    /// Provides an OBEX implementation of the <see cref="WebRequest"/> class.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>If you want to transfer an file or other object using the standard 
    /// service as used by Windows' Wireless Link / Bluetooth File Transfer Wizard, 
    /// Palm's Beam, Nokia's Send via Infrared, then use the OBEX protocol.  
    /// </para>
    /// <para>The PUT operation is supported, and there is new support for GET,
    /// (see the documentation at the <see cref="P:InTheHand.Net.ObexWebRequest.Method"/>
    /// property).
    /// Changing folders is not supported, nor is getting a folder listing.
    /// </para>
    /// <para>In the previous version there were some issue with handling file names 
    /// that include non-English characters, and connections 
    /// to some device types failed.  Also if the connection to the peer was lost
    /// then the request could hang reading forever.  See the release note and bugs
    /// database for more information.
    /// </para>
    /// </remarks>
    /// -
    /// <example>
    /// For Bluetooth one can use code like the following to send a file:
    /// (Note a failure is signalled by an exception).
    /// <code lang="VB.NET">
    /// Dim addr As BluetoothAddress = BluetoothAddress.Parse("002233445566")
    /// Dim path As String = "HelloWorld.txt"
    /// '
    /// Dim req As New ObexWebRequest(addr, path)
    /// req.ReadFile("Hello World.txt")
    /// Dim rsp As ObexWebResponse = CType(req.GetResponse(),ObexWebResponse)
    /// Console.WriteLine("Response Code: {0} (0x{0:X})", rsp.StatusCode)
    /// </code>
    /// That constructor isn't available for other transports (TCP/IP, IrDA)
    /// so one has to create a Uri to provide the scheme, address, and path
    /// parameters.  Thus use code like the following to send a file.
    /// <code lang="VB.NET">
    /// ' The host part of the URI is the device address, e.g. IrDAAddress.ToString(),
    /// ' and the file part is the OBEX object name.
    /// Dim addr As BluetoothAddress = ...
    /// Dim addrStr As String = addr.ToString("N")
    /// Dim uri As New Uri("obex://" &amp; addrStr &amp; "/HelloWorld.txt")
    /// '
    /// Dim req As New ObexWebRequest(uri)
    /// req.ReadFile("Hello World.txt")
    /// Dim rsp As ObexWebResponse = CType(req.GetResponse(),ObexWebResponse)
    /// Console.WriteLine("Response Code: {0} (0x{0:X})", rsp.StatusCode)
    /// </code>
    /// Or, to send locally generated content use something like the following.
    /// <code lang="VB.NET">
    /// Dim addr As BluetoothAddress = ...
    /// Dim path As String = "HelloWorld2.txt"
    /// '
    /// Dim req As New ObexWebRequest(addr, path)
    /// Using content As Stream = req.GetRequestStream()
    ///    ' Using a StreamWriter to write text to the stream...
    ///    Using wtr As New StreamWriter(content)
    ///       wtr.WriteLine("Hello World GetRequestStream")
    ///       wtr.WriteLine("Hello World GetRequestStream 2")
    ///       wtr.Flush()
    ///       ' Set the Length header value
    ///       req.ContentLength = content.Length
    ///    End Using
    ///    ' In this case closing the StreamWriter also closed the Stream, but ...
    /// End Using
    /// Dim rsp As ObexWebResponse = CType(req.GetResponse(),ObexWebResponse) 
    /// Console.WriteLine("Response Code: {0} (0x{0:X})", rsp.StatusCode)
    /// </code>
    /// See also the ObexPushApplication and ObexPushVB sample programs.
    /// </example>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Are we!?!")]
#pragma warning disable SYSLIB0014 // Type or member is obsolete
    public class ObexWebRequest : WebRequest
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    {
        private System.IO.MemoryStream requestStream = new System.IO.MemoryStream();


        private bool connected = false;
        private Socket s;
        private Stream ns;
        private Stream m_alreadyConnectedObexStream;
        BluetoothPublicFactory _btFactory;


        private ushort remoteMaxPacket = 0x400;
        private int connectionId = 0; // TODO -1 is the "not set" value for ConnectionId see OBEX13.pdf section 2.2.11, 0 is a valid value!

        private static class SchemeNames
        {
            internal const string Prefix = "obex";
            //
            internal const string Default = "obex";
            internal const string Push = "obex-push";
            internal const string Ftp = "obex-ftp";
            internal const string Sync = "obex-sync";
            internal const string Pbap = "obex-pbap";
        }

        #region Constructor
        static ObexWebRequest()
        {
            PlatformVerification.ThrowException();

            //register the obex schemes with the WebRequest base method
            ObexWebRequestCreate owrc = new ObexWebRequestCreate();
            WebRequest.RegisterPrefix(SchemeNames.Default, owrc);
            WebRequest.RegisterPrefix(SchemeNames.Push, owrc);
            WebRequest.RegisterPrefix(SchemeNames.Ftp, owrc);
            WebRequest.RegisterPrefix(SchemeNames.Sync, owrc);
        }

        internal ObexWebRequest(Uri requestUri, BluetoothPublicFactory factory)
            : this(requestUri)
        {
            Debug.Assert(factory != null, "NOT factory!=null");
            _btFactory = factory;
        }

        /// <overloads>
        /// Create a new Obex request with the specified <see cref="Uri"/>.
        /// </overloads>
        /// -
        /// <summary>
        /// Create a new Obex request with the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="requestUri">e.g. "obex://112233445566/HelloWorld.txt"</param>
        /// <remarks>Uri must use one of the following schemes - obex, obex-push, obex-ftp, obex-sync.
        /// The host name must be the device address in short hex, or dotted hex notation - not the default representation using the colon separator</remarks>
        public ObexWebRequest(Uri requestUri)
        {
            if (requestUri == null) {
                throw new ArgumentNullException("requestUri");
            }
            if (!requestUri.Scheme.StartsWith(SchemeNames.Prefix, StringComparison.OrdinalIgnoreCase)) {
                throw new UriFormatException("Scheme type not supported by ObexWebRequest");
            }
            uri = requestUri;
        }

        /// <summary>
        /// [Advanced usage]
        /// Create a new Obex request with the specified <see cref="T:System.Uri"/> 
        /// and the open <see cref="T:System.IO.Stream"/> connection to an OBEX server.
        /// </summary>
        /// -
        /// <param name="requestUri">[Advanced usage]
        /// A url of the form 
        /// &#x201C;<i>scheme</i><c>:///</c><i>filename</i>&#x201D;, 
        /// &#x201C;e.g. <c>obex:///foo.txt</c>&#x201D;.
        /// That is the host part is blank, 
        /// and the scheme and filename parts set as for the other constructor 
        /// <see cref="M:InTheHand.Net.ObexWebRequest.#ctor(System.Uri)"/>
        /// </param>
        /// <param name="stream">An instance of <see cref="T:System.IO.Stream"/>
        /// already connected to an OBEX server.
        /// </param>
        public ObexWebRequest(Uri requestUri, Stream stream)
            : this(requestUri)
        {
            if (requestUri == null) {
                throw new ArgumentNullException("requestUri");
            }
            if (requestUri.Host.Length != 0) {
                throw new ArgumentException("Uri must have no host part when passing in the connection stream.");
            }
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }
            if (!(stream.CanRead && stream.CanWrite)) {
                throw new ArgumentException("Stream must be open for reading and writing.");
            }
            m_alreadyConnectedObexStream = stream;
        }

        //
        internal static Uri CreateUrl(string scheme, BluetoothAddress target, string path)
        {
            if (string.IsNullOrEmpty(scheme))
                throw new ArgumentNullException("scheme");
            if (path == null)
                throw new ArgumentNullException("path");
            // (No UriBuilder in NETCF).
            var u0 = new Uri(scheme + "://" + target.ToString("N"));
            var u = new Uri(u0, path);
            return u;
        }

        // * Bluetooth Address and Path
        internal ObexWebRequest(string scheme, BluetoothAddress target, string path,
                BluetoothPublicFactory factory)
            : this(CreateUrl(scheme, target, path), factory)
        {
        }

        /// <summary>
        /// Initialize an instance of this class given a scheme, 
        /// a Bluetooth Device Address, and a remote path name.
        /// </summary>
        /// -
        /// <param name="scheme">The Uri scheme. One of 
        /// <c>obex</c>, <c>obex-push</c>, <c>obex-ftp</c>, or <c>obex-sync</c>.
        /// </param>
        /// <param name="target">The Bluetooth Device Address of the OBEX server.
        /// </param>
        /// <param name="path">The path on the OBEX server.
        /// </param>
        public ObexWebRequest(string scheme, BluetoothAddress target, string path)
            : this(CreateUrl(scheme, target, path))
        {
        }

        /// <summary>
        /// Initialize an instance of this class given 
        /// a Bluetooth Device Address, and a remote path name.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This is equivalent to calling
        /// <see cref="M:InTheHand.Net.ObexWebRequest.#ctor(System.String,InTheHand.Net.BluetoothAddress,System.String)">
        /// ObexWebRequest(String scheme, BluetoothAddress target, String path)
        /// </see>
        /// with scheme &#x201C;<c>obex</c>&#x201D;.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="target">The Bluetooth Device Address of the OBEX server.
        /// </param>
        /// <param name="path">The path on the OBEX server.
        /// </param>
        public ObexWebRequest(BluetoothAddress target, string path)
            : this(SchemeNames.Default, target, path)
        {
        }
        #endregion

        #region Headers
        private WebHeaderCollection headers = new WebHeaderCollection();
        /// <summary>
        /// Specifies a collection of the name/value pairs that make up the OBEX headers.
        /// </summary>
        public override WebHeaderCollection Headers
        {
            get
            {
                return headers;
            }
            set
            {
                headers = value;
            }
        }
        #endregion

        #region Method
        private ObexMethod method = ObexMethod.Put;
        /// <summary>
        /// Gets or sets the method for the request.
        /// </summary>
        /// <remarks>
        /// <para>For Object Exchange the method code is mapped to the equivalent HTTP style method.
        /// For example "PUT", "GET" etc. "PUT" is the default value.
        /// There is new support for GET as of version 2.5.
        /// </para>
        /// <para>To use GET change the <c>Method</c> to "<c>GET</c>" and you must also use
        /// scheme "<c>obex-ftp</c>" in the URL instead of the usual "<c>obex</c>"
        /// -- unless you know that the default OBEX server you are connecting
        /// supports GET.
        /// </para>
        /// <para>For a PUT sample see the <see cref="T:InTheHand.Net.ObexWebRequest">class</see>
        /// documentation.  For GET, see below.
        /// </para>
        /// 
        /// <example>
        /// <code lang="VB.NET">
        /// ' The host part of the URI is the device address, e.g. IrDAAddress.ToString(),
        /// ' and the file part is the OBEX object name.
        /// Dim addr As String = "112233445566"
        /// Dim uri As New Uri("obex-ftp://" &amp; addr &amp; "/HelloWorld.txt")
        /// Dim req As New ObexWebRequest(uri)
        /// req.Method = "GET"
        /// Dim rsp As ObexWebResponse = CType(req.GetResponse(), ObexWebResponse)
        /// Console.WriteLine("Response Code: {0} (0x{0:X})", rsp.StatusCode)
        /// Using content As Stream = rsp.GetResponseStream()
        ///    ' Using a StreamReader to read text from the stream...
        ///    Using rdr As New StreamReader(content)
        ///       While True
        ///          Dim line As String = rdr.ReadLine()
        ///          If line Is Nothing Then Exit While
        ///          Console.WriteLine(line)
        ///       End While
        ///    End Using
        /// End Using
        /// </code>
        /// </example>
        /// </remarks>
        public override string Method
        {
            get
            {
                switch (method) {
                    case ObexMethod.Put:
                        return "PUT";
                    case ObexMethod.Get:
                        return "GET";
                    default:
                        return "";
                }
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                switch (value.ToUpper()) {
                    case "PUT":
                        method = ObexMethod.Put;
                        break;
                    case "GET":
                        method = ObexMethod.Get;
                        break;
                    default:
                        throw new InvalidOperationException("Method not supported");
                }
            }
        }
        #endregion

        #region Connect
        private ObexStatusCode Connect()
        {
            if (!connected) {
                if (ns == null) {
                    try {
#if NETCF
                        // Will be set to false in some Bluetooth cases (e.g. Widcomm and Bluetopia)...
                        bool isWinCeSockets = true;
#endif
                        if (uri.Host.Length == 0) {
                            System.Diagnostics.Debug.Assert(m_alreadyConnectedObexStream != null);
                            System.Diagnostics.Debug.Assert(m_alreadyConnectedObexStream.CanRead
                                && m_alreadyConnectedObexStream.CanWrite);
                            ns = m_alreadyConnectedObexStream;
                        } else {
                            BluetoothAddress ba;
                            IrDAAddress ia;
                            if (BluetoothAddress.TryParse(uri.Host, out ba)) {
                                // No good on Widcomm! s = new Socket(AddressFamily32.Bluetooth, SocketType.Stream, BluetoothProtocolType.RFComm);
                                BluetoothClient cli;
                                if (_btFactory == null) {
                                    cli = new BluetoothClient();
                                } else {
                                    cli = _btFactory.CreateBluetoothClient();
                                }
                                Guid serviceGuid;

                                switch (uri.Scheme) {
                                    case SchemeNames.Ftp:
                                        serviceGuid = BluetoothService.ObexFileTransfer;
                                        break;
                                    //potential for other obex based profiles to be added
                                    case SchemeNames.Sync:
                                        serviceGuid = BluetoothService.IrMCSyncCommand;
                                        break;
                                    case SchemeNames.Pbap:
                                        serviceGuid = BluetoothService.PhonebookAccessPse;
                                        break;
                                    default:
                                        serviceGuid = BluetoothService.ObexObjectPush;
                                        break;
                                }


                                BluetoothEndPoint bep = new BluetoothEndPoint(ba, serviceGuid);
                                cli.Connect(bep);
                                ns = cli.GetStream();
#if NETCF
                                try {
                                    Socket tmp = cli.Client; // Attempt to get the Socket
                                    Debug.Assert(isWinCeSockets);
                                } catch (NotSupportedException) {
                                    isWinCeSockets = false;
                                }
#endif
                            } else if (IrDAAddress.TryParse(uri.Host, out ia)) {
                                //irda
                                s = new Socket(AddressFamily.Irda, SocketType.Stream, ProtocolType.IP);

                                IrDAEndPoint iep = new IrDAEndPoint(ia, "OBEX");

                                s.Connect(iep);
                            } else {
                                //assume a tcp host
                                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                IPAddress ipa;
                                try {
                                    ipa = IPAddress.Parse(uri.Host);
                                } catch {
                                    // Compile-time: warning CS0618: 'System.Net.Dns.Resolve(string)' 
                                    //    is obsolete: 'Resolve is obsoleted for this type, 
                                    //    please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202'
                                    // However GetHostEntry isn't supported on NETCFv1,
                                    // so just keep it and disable the warning on
                                    // the other platforms.
#if V1
/*
#endif
#pragma warning disable 618
#if V1
*/
#endif
                                    ipa = System.Net.Dns.Resolve(uri.Host).AddressList[0];
#if V1
/*
#endif
#pragma warning restore 618
#if V1
*/
#endif
                                }

                                IPEndPoint ipep = new IPEndPoint(ipa, 650);

                                s.Connect(ipep);
                            }

                            if (ns == null) // BluetoothClient used above
                                ns = new NetworkStream(s, true);

#if NETCF
                            if (isWinCeSockets) {
                                // Winsock on WinCE does _not_ support timeouts!
                                ns = new InTheHand.Net.Bluetooth.Factory.TimeoutDecorStream(ns);
                            }
#endif
                            // Timeout
                            ns.ReadTimeout = timeout;
                            ns.WriteTimeout = timeout;
                        }

                        return Connect_Obex();
                    } finally {
                        if (s != null && !s.Connected) {
                            s = null;
                        }
                    }
                }
            }
            Debug.Fail("Don't know that we every get here (Connect when connected).");
            return (ObexStatusCode)0;
        }

        private ObexStatusCode Connect_Obex()
        {
            //do obex negotiation
            byte[] connectPacket;
            if (uri.Scheme == SchemeNames.Ftp) {
                connectPacket = new byte[] { 0x80, 0x00, 26, 0x10, 0x00, 0x20, 0x00, 0x46, 0x00, 19, 0xF9, 0xEC, 0x7B, 0xC4, 0x95, 0x3C, 0x11, 0xD2, 0x98, 0x4E, 0x52, 0x54, 0x00, 0xDC, 0x9E, 0x09 };
            } else {
                connectPacket = new byte[7] { 0x80, 0x00, 0x07, 0x10, 0x00, 0x20, 0x00 };
            }
            ns.Write(connectPacket, 0, connectPacket.Length);

            byte[] receivePacket = new byte[3];
            StreamReadBlockMust(ns, receivePacket, 0, 3);
            if (receivePacket[0] == (byte)(ObexStatusCode.OK | ObexStatusCode.Final)) {
                //get length
                short len = (short)(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receivePacket, 1)) - 3);

                byte[] receivePacket2 = new byte[3 + len];
                Buffer.BlockCopy(receivePacket, 0, receivePacket2, 0, 3);
                StreamReadBlockMust(ns, receivePacket2, 3, len);
                ObexParser.ParseHeaders(receivePacket2, true, ref remoteMaxPacket, null, headers);
                if (headers["CONNECTIONID"] != null) {
                    connectionId = int.Parse(headers["CONNECTIONID"]);
                }
                //ParseHeaders(receivePacket2, headers, null);
            }
            return (ObexStatusCode)receivePacket[0];
        }
        #endregion

        #region Content Type
        /// <summary>
        /// Gets or sets the value of the Type OBEX header.
        /// </summary>
        public override string ContentType
        {
            get
            {
                return headers["TYPE"];
            }
            set
            {
                headers["TYPE"] = value;
            }
        }

        #endregion

        #region Content Length
        /// <summary>
        /// Gets or sets the Length OBEX header.
        /// </summary>
        /// <remarks>This property is mandatory, if not set no data will be sent.
        /// If you use the <see cref="ReadFile"/> helper method this value is automatically populated with the size of the file that was read.</remarks>
        public override long ContentLength
        {
            get
            {
                string len = headers["LENGTH"];
                if (len == null || len == string.Empty) {
                    return 0;
                }
                return long.Parse(len);
            }
            set
            {
                headers["LENGTH"] = value.ToString();
            }
        }
        #endregion

        #region Proxy
        /// <summary>
        /// Not Supported - do not use, this will throw an exception.
        /// </summary>
        public override IWebProxy Proxy
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion

        #region Timeout
        private int timeout = 50000;
        /// <summary>
        /// Gets or sets the time-out value for the <see cref="GetResponse"/> method.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>In versions 3.2 and earlier this property was ignored on
        /// Windows Mobile.  It is now (untested!) supported there,
        /// but not with the Microsoft Bluetooth stack there as it doesn't
        /// support timeouts.
        /// A cunning solution is available let me know of your requirements...
        /// </para>
        /// </remarks>
        /// -
        /// <value>The number of milliseconds to wait before the request times out.
        /// The default is 50,000 milliseconds (50 seconds).
        /// A value of -1 or 0 represents no time-out.</value>
        public override int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (value == -1) {
                    timeout = 0;
                } else {
                    timeout = value;
                }
            }
        }
        #endregion

        #region Uri
        private Uri uri;
        /// <summary>
        /// Gets the original Uniform Resource Identifier (URI) of the request. 
        /// </summary>
        /// <remarks>For an ObexPush request the URI will use the "obex://" prefix, followed by the numerical device id in hex format.
        /// The path section of the URI represents the remote filename of the pushed object. Subfolders are not supported. Some devices may only support specific object types e.g. V-Card.</remarks>
        public override Uri RequestUri
        {
            get
            {
                return uri;
            }
        }
        #endregion

        #region DoPut
        private ObexStatusCode DoPut()
        {
            ObexStatusCode status = 0;

            byte[] buffer = new byte[remoteMaxPacket];

            string filename = uri.PathAndQuery;
            if (!uri.UserEscaped) {
#if !(WinCE && V1)
                // This is a NOP if there's no %xx encodings present.
                filename = Uri.UnescapeDataString(filename);
#else
				// HACK ObexWebRequest -- No Unescape method on NETCFv1!!
#endif
            }
            filename = filename.TrimStart(new char[] { '/' });
            int filenameLength = (filename.Length + 1) * 2;

            int packetLength = 3;
            buffer[0] = (byte)ObexMethod.Put;

            if (connectionId != 0) {
                buffer[packetLength] = (byte)ObexHeader.ConnectionID;
                byte[] raw = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(connectionId));
                raw.CopyTo(buffer, packetLength + 1);
                packetLength += 5;
            }

            buffer[packetLength] = (byte)ObexHeader.Name;
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(filenameLength + 3))).CopyTo(buffer, packetLength + 1);
            System.Text.Encoding.BigEndianUnicode.GetBytes(filename).CopyTo(buffer, packetLength + 3);
            packetLength += (3 + filenameLength);

            string contentType = headers["TYPE"];
            if (contentType != null && contentType != "") {
                int contentTypeLength = (contentType.Length + 1);// *2;
                buffer[packetLength] = (byte)ObexHeader.Type;
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(contentTypeLength + 3))).CopyTo(buffer, packetLength + 1);
                System.Text.Encoding.ASCII.GetBytes(contentType).CopyTo(buffer, packetLength + 3);
                packetLength += (3 + contentTypeLength);
            }
            if (this.ContentLength != 0) {
                buffer[packetLength] = (byte)ObexHeader.Length;
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt32(this.ContentLength))).CopyTo(buffer, packetLength + 1);
                packetLength += 5;
            }

            //write the final packet size
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)packetLength)).CopyTo(buffer, 1);

            //send packet with name header
            ns.Write(buffer, 0, packetLength);

            if (CheckResponse(ref status, false)) {

                int totalBytes = 0;

                byte[] requestBuffer = requestStream.GetBuffer();

                //we really want the content length, but if not available send the whole buffer
                if (this.ContentLength > 0) {
                    totalBytes = (int)this.ContentLength;
                } else {
                    totalBytes = requestBuffer.Length;
                }

                MemoryStream readBuffer = new MemoryStream(requestBuffer);

                while (totalBytes > 0) {
                    int packetLen = AddBody(buffer, 3, ref totalBytes, readBuffer);

                    ns.Write(buffer, 0, packetLen);

                    if (!CheckResponse(ref status, false)) {
                        return status;
                    }
                }
            }

            return status;
        }

        private int AddBody(byte[] buffer, int lenToBodyHeader, ref int totalBytes, MemoryStream readBuffer)
        {
            int thisRequest = 0;
            int lenToBodyContent = lenToBodyHeader + 3;
            if (totalBytes <= (remoteMaxPacket - lenToBodyContent)) {
                thisRequest = totalBytes;

                totalBytes = 0;
                buffer[0] = (byte)ObexMethod.PutFinal;
                buffer[lenToBodyHeader] = (byte)ObexHeader.EndOfBody;

            } else {
                thisRequest = remoteMaxPacket - lenToBodyContent;
                //decrement byte count
                totalBytes -= thisRequest;
                buffer[0] = (byte)ObexMethod.Put;
                buffer[lenToBodyHeader] = (byte)ObexHeader.Body;
            }

            int readBytes = readBuffer.Read(buffer, lenToBodyContent, thisRequest);

            // Before we use the unchecked arithmetic below, check that our
            // maths hasn't failed and we're writing too big length headers.
            ushort check = (ushort)readBytes;
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((short)(readBytes + 3)))).CopyTo(buffer, lenToBodyHeader + 1);

            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((short)(readBytes + lenToBodyContent)))).CopyTo(buffer, 1);
            int packetLen = readBytes + lenToBodyContent;
            return packetLen;
        }
        #endregion

        // Status codes are always final.
        const ObexStatusCode ObexStatus_OK = ObexStatusCode.OK | ObexStatusCode.Final;
        const ObexStatusCode ObexStatus_Continue = ObexStatusCode.Continue | ObexStatusCode.Final;

        #region DoGet
        private ObexStatusCode DoGet(MemoryStream ms, WebHeaderCollection headers)
        {
            ObexStatusCode sc;
            const byte ObexMethod_GetFinal = (byte)ObexMethod.Get | 0x80;

            byte[] buffer = new byte[remoteMaxPacket];

            buffer[0] = ObexMethod_GetFinal;
            int bufferlen = 3;

            //build the packet based on the available headers

            //connectionid (must be first header)
            if (connectionId != 0) {
                buffer[bufferlen] = (byte)ObexHeader.ConnectionID;
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(connectionId)).CopyTo(buffer, bufferlen + 1);

                bufferlen += 5;
            }

            //name
            string filename = uri.PathAndQuery.TrimStart(new char[] { '/' });
            if (filename.Length > 0) {
                const int NullTerminatorLen = 2;
                int filenameLength = filename.Length * 2 + NullTerminatorLen;
                buffer[bufferlen] = (byte)ObexHeader.Name;
                int filenameheaderlen = IPAddress.HostToNetworkOrder((short)(filenameLength + 3));
                BitConverter.GetBytes(filenameheaderlen).CopyTo(buffer, bufferlen + 1);
                System.Text.Encoding.BigEndianUnicode.GetBytes(filename).CopyTo(buffer, bufferlen + 3);

                bufferlen += filenameLength + 3;
            }

            //content type
            string type = this.headers["TYPE"];
            if (type != null) {
                buffer[bufferlen] = (byte)ObexHeader.Type;
                int typeheaderlen = IPAddress.HostToNetworkOrder((short)((type.Length + 1) + 3));
                BitConverter.GetBytes(typeheaderlen).CopyTo(buffer, bufferlen + 1);
                System.Text.Encoding.ASCII.GetBytes(type).CopyTo(buffer, bufferlen + 3);

                bufferlen += type.Length + 4;
            }

            //write total packet size
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)bufferlen)).CopyTo(buffer, 1);

            //send packet with name header
            //then loop until end
            do {
                // Send then Receive
                ns.Write(buffer, 0, bufferlen);
                //
                StreamReadBlockMust(ns, buffer, 0, 3);
                int bytesread = 3;
                //get code
                sc = (ObexStatusCode)buffer[0];
                //get length
                short len = (short)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 1));
                Debug.Assert(len > 0, "not got len!");
                //read all of packet
                StreamReadBlockMust(ns, buffer, bytesread, len - bytesread);
                ObexParser.ParseHeaders(buffer, false, ref remoteMaxPacket, ms, headers);

                //prepare the next request
                buffer[0] = ObexMethod_GetFinal;
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)3)).CopyTo(buffer, 1);
                bufferlen = 3;
            }
            while (sc == ObexStatus_Continue);

            if (sc != ObexStatus_OK) {
                throw new WebException("GET failed with code: " + sc, WebExceptionStatus.ProtocolError);
            }
            return sc;
        }
        #endregion

        #region Check Response
        private bool CheckResponse(ref ObexStatusCode status, bool isConnectResponse)
        {
            if (isConnectResponse)
                throw new ArgumentException("CheckResponse does not know how to parse the connect response");
            byte[] receiveBuffer = new byte[3];
            StreamReadBlockMust(ns, receiveBuffer, 0, receiveBuffer.Length);

            status = (ObexStatusCode)receiveBuffer[0];

            switch (status) {
                case ObexStatusCode.Final | ObexStatusCode.OK:
                case ObexStatusCode.OK:
                case ObexStatusCode.Final | ObexStatusCode.Continue:
                case ObexStatusCode.Continue:
                    //get length
                    short len = (short)(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveBuffer, 1)) - 3);
                    Debug.Assert(len >= 0, "not got len!");

                    if (len > 0) {
                        byte[] receivePacket2 = new byte[len];
                        StreamReadBlockMust(ns, receivePacket2, 0, len);
                        if (len == 5 && ((ObexHeader)receivePacket2[0] == ObexHeader.ConnectionID)) {
                            // ignore ConnectionId
                        } else {
                            Debug.Fail("unused headers...");
                        }
                    }

                    return true;
                default:
                    return false;
            }

            //handle errors please!
        }
        #endregion

        #region Disconnect
        private void DisconnectIgnoreIOErrors()
        {
            if (ns != null) {
                ObexStatusCode status = 0;

                short disconnectPacketSize = 3;
                byte[] disconnectPacket = new byte[8];
                disconnectPacket[0] = (byte)ObexMethod.Disconnect;

                //add connectionid header
                if (connectionId != 0) {
                    disconnectPacket[3] = (byte)ObexHeader.ConnectionID;
                    BitConverter.GetBytes(IPAddress.HostToNetworkOrder(connectionId)).CopyTo(disconnectPacket, 4);
                    disconnectPacketSize += 5;
                }

                //set packet size
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(disconnectPacketSize)).CopyTo(disconnectPacket, 1);

                ns.Write(disconnectPacket, 0, disconnectPacketSize);

                try {
                    CheckResponse(ref status, false);
                } catch (EndOfStreamException) {
                    // If the server has closed the connection already we
                    // normally see EoF on reading for the (first byte of the)
                    // response to our Disconnect request.
                } catch (IOException) {
                    // Other exceptions are possible,
                    // e.g. on _writing_ the Disconnect request.
                }

                ns.Close();
            }
        }
        #endregion

        #region Get Request Stream
        /// <summary>
        /// Gets a <see cref="System.IO.Stream"/> object to use to write request data.
        /// </summary>
        /// -
        /// <returns>A <see cref="T:System.IO.Stream"/> to use to write request data.</returns>
        public override System.IO.Stream GetRequestStream()
        {
            return requestStream;
        }
        #endregion

        #region Read File
        /// <summary>
        /// Reads the contents of the specified file to the request stream.
        /// </summary>
        /// <param name="fileName">The filename (including the path) from which to read.</param>
        /// <remarks>Provides an easy equivalent to manually writing the file contents to the request stream.</remarks>
        public void ReadFile(string fileName)
        {
            FileStream fs = File.OpenRead(fileName);
            long len = 0;
            //read in 1k chunks
            byte[] buffer = new byte[1024];
            int readBytes;
            do {
                readBytes = fs.Read(buffer, 0, buffer.Length);
                len += readBytes;
                requestStream.Write(buffer, 0, readBytes);
            } while (readBytes > 0);

            fs.Close();
            requestStream.Close();

            //write content length
            this.ContentLength = len;
        }
        #endregion

        #region Get Response
        /// <summary>
        /// Returns the OBEX server response.
        /// </summary>
        /// -
        /// <returns>An <see cref="T:InTheHand.Net.ObexWebResponse"/>.</returns>
        /// -
        /// <exception cref="System.Net.WebException">
        /// An error occurred, with the error that occured being stored in the 
        /// <see cref="P:System.Exception.InnerException"/> property.  If the error 
        /// occurred in the connect phase then the <see cref="P:System.Net.WebException.Status"/>
        /// property will have value <see cref="F:System.Net.WebExceptionStatus.ConnectFailure"/>,
        /// and in the operation phase on the desktop CLR it will have value
        /// <see cref="F:System.Net.WebExceptionStatus.UnknownError"/>
        /// </exception>
        public override WebResponse GetResponse()
        {
            ObexStatusCode status;
            MemoryStream ms = new MemoryStream();
            WebHeaderCollection responseHeaders = new WebHeaderCollection();
            //try connecting if not already
            try {
                status = Connect();
                Debug.Assert(status == ObexStatus_OK, "connect was:  " + status);
            } catch (Exception se) {
                throw new WebException("Connect failed.", se, WebExceptionStatus.ConnectFailure, null);
            }

            try {
                switch (this.method) {
                    case ObexMethod.Put:
                    case ObexMethod.PutFinal:
                        status = DoPut();
                        break;
                    case ObexMethod.Get:
                        status = DoGet(ms, responseHeaders);
                        ms.Seek(0, SeekOrigin.Begin);
                        break;
                    default:
                        throw new WebException("Unsupported Method.", new InvalidOperationException(), WebExceptionStatus.ProtocolError, null);
                }
                DisconnectIgnoreIOErrors();
            } catch (WebException) {
                throw; // Don't need to wrap these.
            } catch (Exception ex) {

                throw new WebException("Operation failed.", ex
#if ! WinCE
                        // UnknownError member not supported on CE unfortunately.
                        , WebExceptionStatus.UnknownError, null
#endif
);

            } finally {
                if (ns != null) {
                    ns.Close();
                }
            }

#if false
            if (status != ObexStatus_OK) {
                var failRsp = new ObexWebResponse(null, headers, status);
                throw new WebException("Request failed.", null,
                    WebExceptionStatus.ProtocolError, failRsp);
            }
#endif
            return new ObexWebResponse(ms, responseHeaders, status);
        }
        #endregion

        /// <summary>
        /// A wrapper for Stream.Read that blocks until the requested number of bytes
        /// have been read, and throw an exception if the stream is closed before that occurs.
        /// </summary>
        private static void StreamReadBlockMust(Stream stream, byte[] buffer, int offset, int size)
        {
            int numRead = StreamReadBlock(stream, buffer, offset, size);
            System.Diagnostics.Debug.Assert(numRead <= size);
            if (numRead < size) {
                throw new EndOfStreamException("Connection closed whilst reading an OBEX packet.");
            }
        }

        /// <summary>
        /// A wrapper for Stream.Read that blocks until the requested number of bytes
        /// have been read or the end of the Stream has been reached.
        /// Returns the number of bytes read.
        /// </summary>
        private static int StreamReadBlock(Stream stream, byte[] buffer, int offset, int size)
        {
            int numRead = 0;
            while (size - numRead > 0) {
                int curCount = stream.Read(buffer, offset + numRead, size - numRead);
                if (curCount == 0) { // EoF
                    break;
                }
                numRead += curCount;
            }
            System.Diagnostics.Debug.Assert(numRead <= size);
            return numRead;
        }

        /// <summary>
        /// Begins a request for a OBEX server response.
        /// </summary>
        /// -
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object containing information about the operation.
        /// This object is passed to the callback delegate when the operation is complete.</param>
        /// -
        /// <returns>An <see cref="T:System.IAsyncResult"/> that represents the 
        /// asynchronous operation, which could still be pending.
        /// </returns>
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            AsyncResult<WebResponse> ar = new AsyncResult<WebResponse>(callback, state);
            System.Threading.ThreadPool.QueueUserWorkItem(HackApmRunner_GetResponse, ar);
            return ar;
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Is rethrown by APM caller.")]
        void HackApmRunner_GetResponse(object state)
        {
            AsyncResult<WebResponse> ar = (AsyncResult<WebResponse>)state;
            ar.SetAsCompletedWithResultOf(() => this.GetResponse(), false);
        }

        /// <summary>
        /// Begins a request for a OBEX server response.
        /// </summary>
        /// -
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/>
        /// object that was obtained when the asynchronous operation was started.
        /// </param>
        /// -
        /// <returns>An <see cref="T:InTheHand.Net.ObexWebResponse"/>.</returns>
        /// -
        /// <exception cref="System.Net.WebException">
        /// An error occurred, with the error that occured being stored in the 
        /// <see cref="P:System.Exception.InnerException"/> property.  If the error 
        /// occurred in the connect phase then the <see cref="P:System.Net.WebException.Status"/>
        /// property will have value <see cref="F:System.Net.WebExceptionStatus.ConnectFailure"/>,
        /// and in the operation phase on the desktop CLR it will have value
        /// <see cref="F:System.Net.WebExceptionStatus.UnknownError"/>
        /// </exception>
        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            AsyncResult<WebResponse> ar = (AsyncResult<WebResponse>)asyncResult;
            return ar.EndInvoke();
        }

#if FX4
        public System.Threading.Tasks.Task<WebResponse> GetResponseAsync(object state)
        {
            return System.Threading.Tasks.Task.Factory.FromAsync<WebResponse>(
                BeginGetResponse, EndGetResponse,
                state);
        }
#endif

    }//class
}


