// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal interface IRfCommIf
    {
        IntPtr PObject { get; }

        // /// <summary>
        // /// To get to HandleDeviceResponded, HandleInquiryCompleted etc
        // /// </summary>
        // void SetParent(WidcommBtInterface parent);
        void Create();
        void Destroy(bool disposing);
        //
        bool ClientAssignScnValue(Guid serviceGuid, int scn);
        bool SetSecurityLevel(byte[] p_service_name, BTM_SEC securityLevel, bool isServer);
        //
        int GetScn();
    }


    [Flags]
    enum BTM_SEC : byte {
        //
        // Valid Security Service Levels 
        //
        NONE               = 0x0000, /* Nothing required */
        IN_AUTHORIZE       = 0x0001, /* Inbound call requires authorization */
        IN_AUTHENTICATE    = 0x0002, /* Inbound call requires authentication */
        IN_ENCRYPT         = 0x0004, /* Inbound call requires encryption */
        OUT_AUTHORIZE      = 0x0008, /* Outbound call requires authorization */
        OUT_AUTHENTICATE   = 0x0010, /* Outbound call requires authentication */
        OUT_ENCRYPT        = 0x0020, /* Outbound call requires encryption */
        BOND               = 0x0040, /* Bonding */
    };

}
