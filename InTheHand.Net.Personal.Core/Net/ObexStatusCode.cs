// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexStatusCode
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net
{
    /// <summary>
    /// Specifies the status codes returned for an Object Exchange (OBEX) operation.
    /// </summary>
    /// <remarks>OBEX codes are directly related to their HTTP equivalents - see <see cref="System.Net.HttpStatusCode"/>.</remarks>
    [Flags]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum ObexStatusCode : byte
    {
        /// <summary>
        /// Applied to another code to indicate this is the only response or final response in a series.
        /// </summary>
        Final = 0x80,

        /// <summary>
        /// Equivalent to HTTP status 100.
        /// Continue indicates that the client can continue with its request.
        /// </summary>
        Continue = 0x10,
        /// <summary>
        /// Equivalent to HTTP status 200.
        /// OK indicates that the request succeeded and that the requested information is in the response.
        /// This is the most common status code to receive.
        /// </summary>
        OK = 0x20,
        /// <summary>
        /// Equivalent to HTTP status 201.
        /// Created indicates that the request resulted in a new resource created before the response was sent. 
        /// </summary>
        Created = 0x21,
        /// <summary>
        /// Equivalent to HTTP status 202.
        /// Accepted indicates that the request has been accepted for further processing.
        /// </summary>
        Accepted = 0x22,
        /// <summary>
        /// Equivalent to HTTP status 203.
        /// NonAuthoritativeInformation indicates that the returned metainformation is from a cached copy instead of the origin server and therefore may be incorrect.
        /// </summary>
        NonAuthorativeInformation = 0x23,
        /// <summary>
        /// Equivalent to HTTP status 204.
        /// NoContent indicates that the request has been successfully processed and that the response is intentionally blank.
        /// </summary>
        NoContent = 0x24,
        /// <summary>
        /// Equivalent to HTTP status 205.
        /// ResetContent indicates that the client should reset (not reload) the current resource.
        /// </summary>
        ResetContent = 0x25,
        /// <summary>
        /// Equivalent to HTTP status 206.
        /// PartialContent indicates that the response is a partial response as requested by a GET request that includes a byte range.
        /// </summary>
        PartialContent = 0x26,

        /// <summary>
        /// Equivalent to HTTP status 300.
        /// MultipleChoices indicates that the requested information has multiple representations.
        /// </summary>
        MultipleChoices = 0x30,
        /// <summary>
        /// Equivalent to HTTP status 301.
        /// MovedPermanently indicates that the requested information has been moved to the URI specified in the Location header.
        /// The default action when this status is received is to follow the Location header associated with the response.
        /// </summary>
        MovedPermanently = 0x31,
        /// <summary>
        /// Equivalent to HTTP status 302.
        /// Redirect indicates that the requested information is located at the URI specified in the Location header.
        /// The default action when this status is received is to follow the Location header associated with the response.
        /// When the original request method was POST, the redirected request will use the GET method.
        /// </summary>
        MovedTemporarily = 0x32,
        /// <summary>
        /// Equivalent to HTTP status 303.
        /// SeeOther automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET.
        /// </summary>
        SeeOther = 0x33,
        /// <summary>
        /// Equivalent to HTTP status 304.
        /// NotModified indicates that the client's cached copy is up to date.
        /// The contents of the resource are not transferred.
        /// </summary>
        NotModified = 0x34,
        /// <summary>
        /// Equivalent to HTTP status 305.
        /// UseProxy indicates that the request should use the proxy server at the URI specified in the Location header.
        /// </summary>
        UseProxy = 0x35,

        /// <summary>
        /// Equivalent to HTTP status 400.
        /// BadRequest indicates that the request could not be understood by the server. BadRequest is sent when no other error is applicable, or if the exact error is unknown or does not have its own error code. 
        ///
        /// <see cref="T:InTheHand.Net.ObexWebRequest"/> reports errors through 
        /// <see cref="P:InTheHand.Net.ObexWebResponse.StatusCode">ObexWebResponse.StatusCode</see>,
        /// this status code is overloaded by it to report failure to connect to the server.
        /// </summary>
        BadRequest = 0x40,
        /// <summary>
        /// Equivalent to HTTP status 401.
        /// Unauthorized indicates that the requested resource requires authentication. The WWW-Authenticate header contains the details of how to perform the authentication.
        /// </summary>
        Unauthorized = 0x41,
        /// <summary>
        /// Equivalent to HTTP status 402.
        /// PaymentRequired is reserved for future use.
        /// </summary>
        PaymentRequired = 0x42,
        /// <summary>
        /// Equivalent to HTTP status 403.
        /// Forbidden indicates that the server refuses to fulfill the request.
        /// </summary>
        Forbidden = 0x43,
        /// <summary>
        /// Equivalent to HTTP status 404.
        /// NotFound indicates that the requested resource does not exist on the server.
        /// </summary>
        NotFound = 0x44,
        /// <summary>
        /// Equivalent to HTTP status 405.
        /// MethodNotAllowed indicates that the request method (POST or GET) is not allowed on the requested resource.
        /// </summary>
        MethodNotAllowed = 0x45,
        /// <summary>
        /// Equivalent to HTTP status 406.
        /// NotAcceptable indicates that the client has indicated with Accept headers that it will not accept any of the available representations of the resource.
        /// </summary>
        NotAcceptable = 0x46,
        /// <summary>
        /// Equivalent to HTTP status 407.
        /// ProxyAuthenticationRequired indicates that the requested proxy requires authentication.
        /// The Proxy-authenticate header contains the details of how to perform the authentication.
        /// </summary>
        ProxyAuthenticationRequired = 0x47,
        /// <summary>
        /// Equivalent to HTTP status 408.
        /// RequestTimeout indicates that the client did not send a request within the time the server was expecting the request.
        /// </summary>
        RequestTimeout = 0x48,
        /// <summary>
        /// Equivalent to HTTP status 409.
        /// Conflict indicates that the request could not be carried out because of a conflict on the server.
        /// </summary>
        Conflict = 0x49,
        /// <summary>
        /// Equivalent to HTTP status 410.
        /// Gone indicates that the requested resource is no longer available.
        /// </summary>
        Gone = 0x4A,
        /// <summary>
        /// Equivalent to HTTP status 411.
        /// LengthRequired indicates that the required Content-length header is missing.
        /// </summary>
        LengthRequired = 0x4B,
        /// <summary>
        /// Equivalent to HTTP status 412.
        /// PreconditionFailed indicates that a condition set for this request failed, and the request cannot be carried out.
        /// Conditions are set with conditional request headers like If-Match, If-None-Match, or If-Unmodified-Since.
        /// </summary>
        PreconditionFailed = 0x4C,
        /// <summary>
        /// Equivalent to HTTP status 413.
        /// RequestEntityTooLarge indicates that the request is too large for the server to process.
        /// </summary>
        RequestedEntityTooLarge = 0x4D,
        /// <summary>
        /// Equivalent to HTTP status 414.
        /// RequestUriTooLong indicates that the URI is too long.
        /// </summary>
        RequestedUrlTooLarge = 0x4E,
        /// <summary>
        /// Equivalent to HTTP status 415.
        /// UnsupportedMediaType indicates that the request is an unsupported type.
        /// </summary>
        UnsupportedMediaType = 0x4F,

        /// <summary>
        /// Equivalent to HTTP status 500.
        /// InternalServerError indicates that a generic error has occurred on the server.
        /// 
        /// <see cref="T:InTheHand.Net.ObexWebRequest"/> reports errors through 
        /// <see cref="P:InTheHand.Net.ObexWebResponse.StatusCode">ObexWebResponse.StatusCode</see>,
        /// this status code is used by it to report failure to send the object.
        /// </summary>
        InternalServerError = 0x50,
        /// <summary>
        /// Equivalent to HTTP status 501.
        /// NotImplemented indicates that the server does not support the requested function.
        /// </summary>
        NotImplemented = 0x51,
        /// <summary>
        /// Equivalent to HTTP status 502.
        /// BadGateway indicates that an intermediate proxy server received a bad response from another proxy or the origin server.
        /// </summary>
        BadGateway = 0x52,
        /// <summary>
        /// Equivalent to HTTP status 503.
        /// ServiceUnavailable indicates that the server is temporarily unavailable, usually due to high load or maintenance.
        /// </summary>
        ServiceUnavailable = 0x53,
        /// <summary>
        /// Equivalent to HTTP status 504.
        /// GatewayTimeout indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server.
        /// </summary>
        GatewayTimeout = 0x54,
        /// <summary>
        /// Equivalent to HTTP status 505.
        /// HttpVersionNotSupported indicates that the requested HTTP version is not supported by the server.
        /// </summary>
        HttpVersionNotSupported = 0x55,

        /// <summary>
        /// 
        /// </summary>
        DatabaseFull = 0x60,
        /// <summary>
        /// 
        /// </summary>
        DatabaseLocked = 0x61,
    }
}
