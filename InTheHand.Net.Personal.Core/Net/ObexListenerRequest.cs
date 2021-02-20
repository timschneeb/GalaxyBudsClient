// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexListenerRequest
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
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

namespace InTheHand.Net
{
    /// <summary>
    /// Describes an incoming OBEX request to an <see cref="T:InTheHand.Net.ObexListener"/> object.
    /// </summary>
    public class ObexListenerRequest
    {
        private byte[] input;
        private WebHeaderCollection headers;
        private EndPoint localEndPoint;
        private EndPoint remoteEndPoint;
        private Uri uri;

        #region Constructor
        internal ObexListenerRequest(byte[] input, WebHeaderCollection headers, EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            this.input = input;
            this.headers = headers;
            this.localEndPoint = localEndPoint;
            this.remoteEndPoint = remoteEndPoint;
        }
        #endregion

        #region Content Length 64
        /// <summary>
        /// Gets the length of the body data included in the request.
        /// <para><b>New in v1.5.51015</b></para>
        /// </summary>
        /// <value>A long value that contains the value from the request's Length header.
        /// This value is -1 if the content length is not known.</value>
        /// <remarks>The Length header expresses the length, in bytes, of the body data that accompanies the request.</remarks>
        public long ContentLength64
        {
            get
            {
                string len = headers["LENGTH"];
                if (len != null && len != "") {
                    return long.Parse(len);
                }
                return -1;
            }
        }
        #endregion

        #region Content Type
        /// <summary>
        /// Gets the MIME type of the body data included in the request.
        /// </summary>
        /// <value>A <see cref="String"/> that contains the text of the request's Type header.</value>
        public string ContentType
        {
            get
            {
                return headers["TYPE"];
            }
        }
        #endregion

        #region Headers
        /// <summary>
        /// Gets the collection of header name/value pairs sent in the request.
        /// </summary>
        /// <value>A <see cref="WebHeaderCollection"/> that contains the OBEX headers included in the request.</value>
        /// <remarks>For a complete list of request headers, see the <see cref="T:InTheHand.Net.ObexHeader"/> enumeration.</remarks>
        public WebHeaderCollection Headers
        {
            get
            {
                return headers;
            }
        }
        #endregion

        #region Local Endpoint
        /// <summary>
        /// Get the device address and service to which the request is directed.
        /// </summary>
        /// -
        /// <remarks>
        /// The <see cref="T:System.Net.EndPoint"/> instance returned will be of the 
        /// subtype that matches the address family that the <see cref="T:InTheHand.Net.ObexListener"/> 
        /// is listening on.  For instance if the listener was created with 
        /// <see cref="T:InTheHand.Net.ObexTransport"/>.<see cref="F:InTheHand.Net.ObexTransport.Bluetooth"/>
        /// then the <see cref="T:System.Net.EndPoint"/> will be of type
        /// <see cref="T:InTheHand.Net.BluetoothEndPoint"/>, and similarly for 
        /// <see cref="T:InTheHand.Net.IrDAEndPoint"/> and
        /// <see cref="T:System.Net.IPEndPoint"/>.
        /// </remarks>
        /// -
        /// <seealso cref="P:InTheHand.Net.ObexListenerRequest.RemoteEndPoint"/>
        public EndPoint LocalEndPoint
        {
            get
            {
                return localEndPoint;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Gets the method specified by the client.
        /// </summary>
        /// <remarks>Only PUT is supported in this version.</remarks>
        public string ObexMethod
        {
            get
            {
                //in a future version we'll support other methods
                return "PUT";
            }
        }
        #endregion

        #region Input Stream
        /// <summary>
        /// Gets a stream that contains the body data sent by the client.
        /// </summary>
        public Stream InputStream
        {
            get
            {
                return new MemoryStream(this.input);
            }
        }
        #endregion

        #region Protocol Version
        /// <summary>
        /// Gets the OBEX version used by the requesting client
        /// </summary>
        public Version ProtocolVersion
        {
            get
            {
                //cheat and hard code to 1.0
                return new Version(1, 0);
            }
        }
        #endregion

        #region Raw Url
        /// <summary>
        /// Gets the URL information (without the host and port) requested by the client.
        /// </summary>
        /// <value>A <see cref="String"/> that contains the raw URL for this request.</value>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "Also have get_Url.")]
        public string RawUrl
        {
            get
            {
                return Url.PathAndQuery;
            }
        }
        #endregion

        #region Remote End Point
        /// <summary>
        /// Gets the device address and service from which the request originated.
        /// </summary>
        /// -
        /// <remarks>
        /// The <see cref="T:System.Net.EndPoint"/> instance returned will be of the 
        /// subtype that matches the address family that the <see cref="T:InTheHand.Net.ObexListener"/> 
        /// is listening on.  For instance if the listener was created with 
        /// <see cref="T:InTheHand.Net.ObexTransport"/>.<see cref="F:InTheHand.Net.ObexTransport.Bluetooth"/>
        /// then the <see cref="T:System.Net.EndPoint"/> will be of type
        /// <see cref="T:InTheHand.Net.BluetoothEndPoint"/>, and similarly for 
        /// <see cref="T:InTheHand.Net.IrDAEndPoint"/> and
        /// <see cref="T:System.Net.IPEndPoint"/>.
        /// </remarks>
        /// -
        /// <example>
        /// C#
        /// <code lang="C#">
        ///   ObexListener lsnr = new ObexListener(ObexTransport.Bluetooth)
        ///   ... ...
        ///   ObexListenerRequest olr = ...
        ///   BluetoothEndPoint remoteEp = (BluetoothEndPoint)olr.RemoteEndPoint;
        ///   BluetoothAddress remoteAddr = remoteEp.Address;
        /// </code>
        /// Visual Basic
        /// <code lang="VB.NET">
        ///   Dim lsnr As New ObexListener(ObexTransport.IrDA)
        ///   ... ...
        ///   Dim olr As ObexListenerRequest = ...
        ///   Dim remoteEp As IrDAEndPoint = CType(olr.RemoteEndPoint, IrDAEndPoint);
        ///   Dim remoteAddr As IrDAAddress = remoteEp.Address;
        /// </code>
        /// </example>
        /// -
        /// <seealso cref="P:InTheHand.Net.ObexListenerRequest.LocalEndPoint"/>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return remoteEndPoint;
            }
        }
        #endregion

        #region User Host Address
        /// <summary>
        /// Gets the server address to which the request is directed.
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                if (localEndPoint is BluetoothEndPoint) {
                    BluetoothEndPoint bep = localEndPoint as BluetoothEndPoint;
                    return bep.Address.ToString("P");
                } else if (localEndPoint is IrDAEndPoint) {
                    IrDAEndPoint iep = localEndPoint as IrDAEndPoint;
                    return iep.Address.ToString("P");
                } else if (localEndPoint is IPEndPoint) {
                    IPEndPoint ipep = localEndPoint as IPEndPoint;
                    return ipep.Address.ToString() + ":" + ipep.Port;
                }
                //catchall
                return "";
            }
        }

        #endregion

        #region Url
        /// <summary>
        /// Gets the <see cref="Uri"/> object requested by the client.
        /// </summary>
        /// <value>A <see cref="Uri"/> object that identifies the resource requested by the client.</value>
        public Uri Url
        {
            get
            {
                if (uri == null) {
                    string address;
                    if (this.localEndPoint is BluetoothEndPoint) {
                        address = ((BluetoothEndPoint)this.localEndPoint).Address.ToString();
                    } else {
                        address = ((IrDAEndPoint)this.localEndPoint).Address.ToString();
                    }
                    uri = new Uri("obex-push://" + address + "/" + headers["NAME"]);
                }
                return uri;
            }
        }
        #endregion

        #region Write File
        /// <summary>
        /// Writes the body of the request to the specified file path.
        /// </summary>
        /// <param name="fileName">The filename (including the path) to write to.</param>
        public void WriteFile(string fileName)
        {
            System.IO.FileStream fs = System.IO.File.Create(fileName);
            WriteFile(fs);
            fs.Close();
        }

        internal void WriteFile(Stream fs)
        {
            System.IO.MemoryStream ms = new MemoryStream(input);

            int bytesToRead = (int)this.ContentLength64;
            int bytesRead = 0;

            if (bytesToRead == -1) {
                bytesToRead = input.Length;
            }

            int read;
            byte[] buffer = new byte[1024];
            do {
                read = ms.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, read);
                bytesRead += read;
            } while (read > 0);

            ms.Close();
        }
        #endregion
    }
}
