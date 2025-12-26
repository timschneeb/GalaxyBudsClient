// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexWebResponse
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System.Net;
using System.IO;
using System.Diagnostics.CodeAnalysis;

// SYSLIB0014: WebResponse is obsolete
#pragma warning disable SYSLIB0014

namespace InTheHand.Net
{
    /// <summary>
    /// Provides an OBEX implementation of the <see cref="WebResponse"/> class.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Are we!?!")]
#pragma warning disable SYSLIB0014 // Type or member is obsolete
    public class ObexWebResponse : WebResponse
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    {
        private MemoryStream responseStream;
        private WebHeaderCollection responseHeaders;
        private ObexStatusCode statusCode;

        internal ObexWebResponse(MemoryStream stream, WebHeaderCollection headers, ObexStatusCode code)
        {
            this.responseStream = stream;
            this.responseHeaders = headers;
            this.statusCode = code;
        }

        #region Headers
        /// <summary>
        /// Gets the headers associated with this response from the server.
        /// </summary>
        public override WebHeaderCollection Headers
        {
            get
            {
                return this.responseHeaders;
            }
        }
        #endregion

        #region Content Length
        /// <summary>
        /// Gets the length of the content returned by the request.
        /// </summary>
        public override long ContentLength
        {
            get
            {
                string len = this.responseHeaders["LENGTH"];
                if (len != null && len != string.Empty) {
                    return long.Parse(len);
                }
                return 0;
            }
            set
            {
            }
        }
        #endregion

        #region Content Type
        /// <summary>
        /// Gets the content type of the response.
        /// </summary>
        public override string ContentType
        {
            get
            {
                return this.responseHeaders["TYPE"];
            }
            set
            {
            }
        }
        #endregion

        #region Status Code
        /// <summary>
        /// Returns a status code to indicate the outcome of the request.
        /// </summary>
        /// -
        /// <remarks><para>Note, if a error occurs locally then the status code
        /// <see cref="F:InTheHand.Net.ObexStatusCode.InternalServerError"/> is returned.
        /// Therefore that error code could signal local or remote errors.
        /// </para>
        /// </remarks>
        public ObexStatusCode StatusCode
        {
            get
            {
                return statusCode;
            }
        }
        #endregion

        #region GetResponseStream
        /// <summary>
        /// Gets the stream used to read the body of the response from the server.
        /// </summary>
        /// -
        /// <returns>A <see cref="T:System.IO.Stream"/> containing the body of the response.</returns>
        public override Stream GetResponseStream()
        {
            return responseStream;
        }
        #endregion

        #region Close
        /// <summary>
        /// Frees the resources held by the response.
        /// </summary>
        public override void Close()
        {
            if (responseStream != null) {
                responseStream.Close();
            }
        }
        #endregion


        #region WriteFile
        /// <summary>
        /// Writes the contents of the response to the specified file path.
        /// </summary>
        /// <param name="fileName">The filename (including the path) from which to read.</param>
        [System.Obsolete()]
        public void WriteFile(string fileName)
        {
            FileStream fs = File.Create(fileName);

            //read in 1k chunks
            byte[] buffer = new byte[1024];
            int readBytes;
            do {
                readBytes = responseStream.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, readBytes);
            } while (readBytes > 0);

            responseStream.Close();
            fs.Close();
        }
        #endregion
    }
}
