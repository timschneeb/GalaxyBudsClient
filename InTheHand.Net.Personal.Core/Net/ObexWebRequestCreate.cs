// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexWebRequestCreate
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Net;
#pragma warning disable SYSLIB0014


namespace InTheHand.Net
{
    /// </summary>
#pragma warning disable SYSLIB0014 // Type or member is obsolete
    internal class ObexWebRequestCreate : IWebRequestCreate
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    {
        #region IWebRequestCreate Members

        public WebRequest Create(Uri uri)
        {
            return new ObexWebRequest(uri);
        }

        #endregion
    }
}
