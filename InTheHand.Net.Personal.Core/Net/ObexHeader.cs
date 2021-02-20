// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.ObexHeader
// 
// Copyright (c) 2003-2006 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

namespace InTheHand.Net
{


    internal enum ObexHeader : byte
    {
        // OBEX Header codes
        None = 0x0,
        Name = 0x01,
        Description = 0x05,

        Type = 0x42,
        TimeIso8601 = 0x44,
        Target = 0x46,
        Http = 0x47,
        Body = 0x48,
        EndOfBody = 0x49,
        Who = 0x4A,
        ApplicationParameter = 0x4C,
        AuthenticationChallenge = 0x4D,
        AuthenticationResponse = 0x4E,
        ObjectClass = 0x4F,

        WanUuid = 0x50,
        //ObjectClass = 0x51,
        SessionParamters = 0x52,

        SessionSequenceNumber = 0x93,

        Count = 0xC0,
        Length = 0xC3,
        Time4Byte = 0xC4,
        ConnectionID = 0xCB,
        CreatorID = 0xCF,
    }
}
