// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    interface ISdpService : IDisposable
    {
        void AddServiceClassIdList(IList<Guid> serviceClasses);
        void AddServiceClassIdList(Guid serviceClass);
        void AddRFCommProtocolDescriptor(byte scn);
        void AddServiceName(string serviceName);
        void AddAttribute(ushort id, SdpService.DESC_TYPE dt, int valLen, byte[] val);
        void CommitRecord();
    }
}
