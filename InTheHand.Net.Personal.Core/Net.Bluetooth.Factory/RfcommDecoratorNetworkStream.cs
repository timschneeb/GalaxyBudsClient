// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Factory
{
    sealed class RfcommDecoratorNetworkStream : DecoratorNetworkStream
    {
        readonly CommonRfcommStream m_childWrs; // WRS required for DataAvailable property.

        internal RfcommDecoratorNetworkStream(CommonRfcommStream childWrs)
            : base(childWrs)
        {
            if (!childWrs.Connected) // Although the base constructor will have checked already.
                throw new ArgumentException("Child stream must be connected.");
            m_childWrs = childWrs;
        }

        public override bool DataAvailable
        {
            get { return m_childWrs.DataAvailable; }
        }
    }

}
