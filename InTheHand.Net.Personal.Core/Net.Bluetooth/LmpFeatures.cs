// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.LmpFeatures
// 
// Copyright (c) 2011-2012 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth
{
    /* Win7:
     *  hciSubvers: 5276, hciVersion: v2_1wEdr, lmpSubversion : 5276, lmpVersion: v2_1wEdr
     *  IOCTL_BTH_GET_LOCAL_INFO lmpFeatures: 'ThreeSlotPackets, FiveSlotPackets, Encryp
        tion, SlotOffset, TimingAccuracy, RoleSwitch, HoldMode, SniffMode, ParkState, Po
        werControlRequests, ChannelQualityDrivenDataRate, ScoLink, HV2Packets, HV3Packet
        s, MuLawLogSynchronousData, ALawLogSynchronousData, CvsdSynchronousData, PagingP
        arameterNegotiation, PowerControl, TransparentSynchronousData, BroadcastEncrypti
        on, EnhancedDataRateAcl2MbpsMode, EnhancedDataRateAcl3MbpsMode, EnhancedInquiryS
        can, InterlacedInquiryScan, InterlacedPageScan, RssiWithInquiryResults, Extended
        ScoLinkEV3Packets, EV4Packets, EV5Packets, AfhCapableSlave, AfhClassificationSla
        ve, ThreeSlotEnhancedDataRateAclPackets, FiveSlotEnhancedDataRateAclPackets, Sni
        ffSubrating, PauseEncryption, AFHCapableMaster, AFHClassificationMaster, Enhance
        dDataRateESco2MbpsMode, EnhancedDataRateESco3MbpsMode, ThreeSlotEnhancedDataRate
        EScoPackets, ExtendedInquiryResponse, SecureSimplePairing, EncapsulatedPdu, NonF
        lushablePacketBoundaryFlag, LinkSupervisionTimeoutChangedEvent, EnhancedPowerCon
        trol, ExtendedFeatures' 0x8359FF9BFE8FFFFF
     */

    /* WidcommXP remote:
     *  BTH_RADIO_INFO v1_2, 777, Broadcom, 00000808380DFEFF 'ThreeSlotPackets, FiveSlot
        Packets, Encryption, SlotOffset, TimingAccuracy, RoleSwitch, HoldMode, SniffMode
        , PowerControlRequests, ChannelQualityDrivenDataRate, ScoLink, HV2Packets, HV3Pa
        ckets, MuLawLogSynchronousData, ALawLogSynchronousData, CvsdSynchronousData, Pow
        erControl, TransparentSynchronousData, EnhancedInquiryScan, InterlacedInquirySca
        n, InterlacedPageScan, AfhCapableSlave, AFHCapableMaster'
     */

    /// <summary>
    /// Created from v2.1 specification.
    /// </summary>
    [Flags]
    public enum LmpFeatures : long
    {
        /// <summary>
        /// There are no supported features.
        /// </summary>
        None = 0,
#pragma warning disable 1591 // "Missing XML comment for publicly visible type or member ..."
        //----
        /// <summary>
        /// [0]
        /// </summary>
        ThreeSlotPackets = 0x0000000000000001,
        FiveSlotPackets = 0x0000000000000002,
        Encryption = 0x0000000000000004,
        SlotOffset = 0x0000000000000008,
        TimingAccuracy = 0x0000000000000010,
        RoleSwitch = 0x0000000000000020,
        HoldMode = 0x0000000000000040,
        SniffMode = 0x0000000000000080,
        //--
        /// <summary>
        /// [8]
        /// </summary>
        ParkState = 0x0000000000000100,
        PowerControlRequests = 0x0000000000000200,
        ChannelQualityDrivenDataRate = 0x0000000000000400,
        ScoLink = 0x0000000000000800,
        HV2Packets = 0x0000000000001000,
        HV3Packets = 0x0000000000002000,
        MuLawLogSynchronousData = 0x0000000000004000,
        ALawLogSynchronousData = 0x0000000000008000,
        //--
        /// <summary>
        /// [16]
        /// </summary>
        CvsdSynchronousData = 0x0000000000010000,
        PagingParameterNegotiation = 0x0000000000020000,
        PowerControl = 0x0000000000040000,
        TransparentSynchronousData = 0x0000000000080000,
        FlowControlLag_LeastSignificantBit = 0x0000000000100000,
        FlowControlLag_MiddleBit = 0x0000000000200000,
        FlowControlLag_MostSignificantBit = 0x0000000000400000,
        BroadcastEncryption = 0x0000000000800000,
        //--
        //Reserved = 0x0000000001000000,
        /// <summary>
        /// [25]
        /// </summary>
        EnhancedDataRateAcl2MbpsMode = 0x0000000002000000,
        EnhancedDataRateAcl3MbpsMode = 0x0000000004000000,
        EnhancedInquiryScan = 0x0000000008000000,
        InterlacedInquiryScan = 0x0000000010000000,
        InterlacedPageScan = 0x0000000020000000,
        RssiWithInquiryResults = 0x0000000040000000,
        ExtendedScoLinkEV3Packets = 0x0000000080000000,
        //--
        /// <summary>
        /// [32]
        /// </summary>
        EV4Packets = 0x0000000100000000,
        /// <summary>
        /// [33]
        /// </summary>
        EV5Packets = 0x0000000200000000,
        //Reserved = 0x0000000400000000,
        /// <summary>
        /// [35]
        /// </summary>
        AfhCapableSlave = 0x0000000800000000,
        /// <summary>
        /// [36]
        /// </summary>
        AfhClassificationSlave = 0x0000001000000000,
        /// <summary>
        /// [37] v4.0
        /// </summary>
        BrEdrNotSupported = 0x0000002000000000,
        /// <summary>
        /// [38] v4.0
        /// </summary>
        LeSupported_Controller = 0x0000004000000000,
        /// <summary>
        /// [39]
        /// </summary>
        ThreeSlotEnhancedDataRateAclPackets = 0x0000008000000000,
        //--
        /// <summary>
        /// [40]
        /// </summary>
        FiveSlotEnhancedDataRateAclPackets = 0x0000010000000000,
        /// <summary>
        /// [41] v2.1
        /// </summary>
        SniffSubrating = 0x0000020000000000,
        /// <summary>
        /// [42] v2.1
        /// </summary>
        PauseEncryption = 0x0000040000000000,
        AFHCapableMaster = 0x0000080000000000,
        AFHClassificationMaster = 0x0000100000000000,
        EnhancedDataRateESco2MbpsMode = 0x0000200000000000,
        EnhancedDataRateESco3MbpsMode = 0x0000400000000000,
        ThreeSlotEnhancedDataRateEScoPackets = 0x0000800000000000,
        //--
        // From here down are v2.1 or later.

        /// <summary>
        /// [48] v2.1
        /// </summary>
        ExtendedInquiryResponse = 0x0001000000000000,
        /// <summary>
        /// [49]
        /// </summary>
        SimultaneousLeAndBrEdrToSameDeviceCapable_Controller = 0x0002000000000000,
        //	= 0x0004000000000000,
        /// <summary>
        /// [51] v2.1
        /// </summary>
        SecureSimplePairing = 0x0008000000000000,
        /// <summary>
        /// [52] v2.1
        /// </summary>
        EncapsulatedPdu = 0x0010000000000000,
        /// <summary>
        /// [53] v2.1
        /// </summary>
        ErroneousDataReporting = 0x0020000000000000,
        /// <summary>
        /// [54] v2.1
        /// </summary>
        NonFlushablePacketBoundaryFlag = 0x0040000000000000,
        //	= 0x0080000000000000,
        //--
        /// <summary>
        /// [56] v2.1
        /// </summary>
        LinkSupervisionTimeoutChangedEvent = 0x0100000000000000,
        /// <summary>
        /// [57] v2.1
        /// (Changed name from 'InquiryResponseTxPowerLevel' in v2.1 
        /// to 'InquiryTxPowerLevel' in v3.0).
        /// </summary>
        InquiryTxPowerLevel = 0x0200000000000000,
        /// <summary>
        /// [58] v3.0
        /// </summary>
        EnhancedPowerControl = 0x0400000000000000,
        //	= 0x0800000000000000,
        //	= 0x1000000000000000,
        //	= 0x2000000000000000,
        //	= 0x4000000000000000,
        /// <summary>
        /// [63] Present since v2.0 at least.
        /// </summary>
        ExtendedFeatures = unchecked((long)0x8000000000000000),
    }
}
