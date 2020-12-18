// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BluetoothService
// 
// Copyright (c) 2003-2009 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Standard Bluetooth Profile identifiers.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>See the list at <see href="http://www.bluetooth.org/Technical/AssignedNumbers/service_discovery.htm"/>.
    /// </para>
    /// <para>The Bluetooth Base UUID is {00000000-0000-1000-8000-00805F9B34FB}
    /// </para>
    /// </remarks>
    public static class BluetoothService
    {
        /// <summary>
        /// Represents an empty service Guid.
        /// </summary>
        public static readonly Guid Empty = Guid.Empty;


        /// <summary>
        /// Represents the base Guid from which all standard Bluetooth profiles are derived - not used for connections.
        /// Is {00000000-0000-1000-8000-00805F9B34FB}
        /// </summary>
        public static readonly Guid BluetoothBase = new Guid(0x00000000, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

#pragma warning disable 1591
        //Considering moving all the protocol definitions to a separate class from the profiles...
        /// <summary>
        /// [0x0001]
        /// </summary>
        public static readonly Guid SdpProtocol = new Guid(0x00000001, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0002]
        /// </summary>
        public static readonly Guid UdpProtocol = new Guid(0x00000002, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0003]
        /// </summary>
        public static readonly Guid RFCommProtocol = new Guid(0x00000003, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0004]
        /// </summary>
        public static readonly Guid TcpProtocol = new Guid(0x00000004, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0005]
        /// </summary>
        public static readonly Guid TcsBinProtocol = new Guid(0x00000005, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0006]
        /// </summary>
        public static readonly Guid TcsAtProtocol = new Guid(0x00000006, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0008]
        /// </summary>
        public static readonly Guid AttProtocol = new Guid(0x00000007, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0008]
        /// </summary>
        public static readonly Guid ObexProtocol = new Guid(0x00000008, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0009]
        /// </summary>
        public static readonly Guid IPProtocol = new Guid(0x00000009, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x000A]
        /// </summary>
        public static readonly Guid FtpProtocol = new Guid(0x0000000A, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x000C]
        /// </summary>
        public static readonly Guid HttpProtocol = new Guid(0x0000000C, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x000E]
        /// </summary>
        public static readonly Guid WspProtocol = new Guid(0x0000000E, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x000F]
        /// </summary>
        public static readonly Guid BnepProtocol = new Guid(0x0000000F, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0010]
        /// </summary>
        public static readonly Guid UpnpProtocol = new Guid(0x00000010, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0011]
        /// </summary>
        public static readonly Guid HidpProtocol = new Guid(0x00000011, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0012]
        /// </summary>
        public static readonly Guid HardcopyControlChannelProtocol = new Guid(0x00000012, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0014]
        /// </summary>
        public static readonly Guid HardcopyDataChannelProtocol = new Guid(0x00000014, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0016]
        /// </summary>
        public static readonly Guid HardcopyNotificationProtocol = new Guid(0x00000016, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0017]
        /// </summary>
        public static readonly Guid AvctpProtocol = new Guid(0x00000017, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x0019]
        /// </summary>
        public static readonly Guid AvdtpProtocol = new Guid(0x00000019, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x001B]
        /// </summary>
        public static readonly Guid CmtpProtocol = new Guid(0x0000001B, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x001D] ?????
        /// </summary>
        public static readonly Guid UdiCPlaneProtocol = new Guid(0x0000001D, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x001E]
        /// </summary>
        public static readonly Guid McapControlChannelProtocol = new Guid(0x0000001E, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x001F]
        /// </summary>
        public static readonly Guid McapDataChannelProtocol = new Guid(0x0000001F, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x0100]
        /// </summary>
        public static readonly Guid L2CapProtocol = new Guid(0x00000100, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);


        /// <summary>
        /// [0x1000]
        /// </summary>
        public static readonly Guid ServiceDiscoveryServer = new Guid(0x00001000, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1001]
        /// </summary>
        public static readonly Guid BrowseGroupDescriptor = new Guid(0x00001001, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1002]
        /// </summary>
        public static readonly Guid PublicBrowseGroup = new Guid(0x00001002, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Provides a basic Serial emulation connect over Bluetooth. [0x1101]
        /// </summary>
        public static readonly Guid SerialPort = new Guid(0x00001101, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// Used to establish PPP connections over RFComm channels. [0x1102]
        /// </summary>
        public static readonly Guid LanAccessUsingPpp = new Guid(0x00001102, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1103]
        /// </summary>
        public static readonly Guid DialupNetworking = new Guid(0x00001103, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x1104]
        /// </summary>
        public static readonly Guid IrMCSync = new Guid(0x00001104, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Used for sending binary objects between devices.[0x1105]
        /// </summary>
        public static readonly Guid ObexObjectPush = new Guid(0x00001105, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// OBEX version of an FTP server [0x1106]
        /// </summary>
        public static readonly Guid ObexFileTransfer = new Guid(0x00001106, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1107]
        /// </summary>
        public static readonly Guid IrMCSyncCommand = new Guid(0x00001107, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// HSP (Headset Profile) &#x2014; Supports Bluetooth headset devices.[0x1108]
        /// See also 
        /// <see cref="HeadsetHeadset"/>
        /// <see cref="HeadsetAudioGateway"/>
        /// </summary>
        /// <seealso cref="HeadsetHeadset"/>
        /// <seealso cref="HeadsetAudioGateway"/>
        public static readonly Guid Headset = new Guid(0x00001108, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1109]
        /// </summary>
        public static readonly Guid CordlessTelephony = new Guid(0x00001109, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x110A]
        /// </summary>
        public static readonly Guid AudioSource = new Guid(0x0000110A, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x110B]
        /// </summary>
        public static readonly Guid AudioSink = new Guid(0x0000110B, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x110C]
        /// </summary>
        public static readonly Guid AVRemoteControlTarget = new Guid(0x0000110C, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x110D]
        /// </summary>
        public static readonly Guid AdvancedAudioDistribution = new Guid(0x0000110D, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x110E]
        /// </summary>
        public static readonly Guid AVRemoteControl = new Guid(0x0000110E, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x110F]
        /// </summary>
        public static readonly Guid AVRemoteControlController = new Guid(0x0000110F, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x1110]
        /// </summary>
        public static readonly Guid Intercom = new Guid(0x00001110, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1111]
        /// </summary>
        public static readonly Guid Fax = new Guid(0x00001111, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1112]
        /// See also
        /// <see cref="Headset"/>
        /// <see cref="HeadsetHeadset"/>
        /// </summary>
        /// <seealso cref="Headset"/>
        /// <seealso cref="HeadsetHeadset"/>
        public static readonly Guid HeadsetAudioGateway = new Guid(0x00001112, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x1113]
        /// </summary>
        public static readonly Guid Wap = new Guid(0x00001113, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1114]
        /// </summary>
        public static readonly Guid WapClient = new Guid(0x00001114, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x1115]
        /// </summary>
        public static readonly Guid Panu = new Guid(0x00001115, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1116]
        /// </summary>
        public static readonly Guid Nap = new Guid(0x00001116, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1117]
        /// </summary>
        public static readonly Guid GN = new Guid(0x00001117, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x1118]
        /// </summary>
        public static readonly Guid DirectPrinting = new Guid(0x00001118, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1119]
        /// </summary>
        public static readonly Guid ReferencePrinting = new Guid(0x00001119, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x111A]
        /// </summary>
        public static readonly Guid Imaging = new Guid(0x0000111A, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x111B]
        /// </summary>
        public static readonly Guid ImagingResponder = new Guid(0x0000111B, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x111C]
        /// </summary>
        public static readonly Guid ImagingAutomaticArchive = new Guid(0x0000111C, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x111D]
        /// </summary>
        public static readonly Guid ImagingReferenceObjects = new Guid(0x0000111D, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// Supports hands free kits such as a car kits which provide audio and more advanced call control than the Headset profile. [0x111E]
        /// </summary>
        public static readonly Guid Handsfree = new Guid(0x0000111E, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x111F]
        /// </summary>
        public static readonly Guid HandsfreeAudioGateway = new Guid(0x0000111F, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// [0x1120]
        /// </summary>
        public static readonly Guid DirectPrintingReferenceObjects = new Guid(0x00001120, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// [0x1121]
        /// </summary>
        public static readonly Guid ReflectedUI = new Guid(0x00001121, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Used for printing simple text, HTML, vCard objects and similar. [0x1122]
        /// </summary>
        public static readonly Guid BasicPrinting = new Guid(0x00001122, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1123]
        /// </summary>
        public static readonly Guid PrintingStatus = new Guid(0x00001123, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Supports human interface devices such as keyboards and mice. [0x1124]
        /// </summary>
        public static readonly Guid HumanInterfaceDevice = new Guid(0x00001124, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1125]
        /// </summary>
        public static readonly Guid HardcopyCableReplacement = new Guid(0x00001125, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1126]
        /// </summary>
        public static readonly Guid HardcopyCableReplacementPrint = new Guid(0x00001126, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1127]
        /// </summary>
        public static readonly Guid HardcopyCableReplacementScan = new Guid(0x00001127, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Common_ISDN_Access [0x1128]
        /// </summary>
        public static readonly Guid CommonIsdnAccess = new Guid(0x00001128, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1129]
        /// </summary>
        public static readonly Guid VideoConferencingGW = new Guid(0x00001129, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// UDI_MT [0x112A]
        /// </summary>
        public static readonly Guid UdiMT = new Guid(0x0000112A, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// UDI_TA [0x112B]
        /// </summary>
        public static readonly Guid UdiTA = new Guid(0x0000112B, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x112C]
        /// </summary>
        public static readonly Guid AudioVideo = new Guid(0x0000112C, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// SIM_Access [0x112D]
        /// </summary>
        public static readonly Guid SimAccess = new Guid(0x0000112D, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Phonebook Access - PCE [0x112E]
        /// </summary>
        public static readonly Guid PhonebookAccessPce = new Guid(0x0000112E, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Phonebook Access - PSE [0x112F]
        /// </summary>
        public static readonly Guid PhonebookAccessPse = new Guid(0x0000112F, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Phonebook Access [0x1130]
        /// </summary>
        public static readonly Guid PhonebookAccess = new Guid(0x00001130, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Headset [0x1131]
        /// See also
        /// <see cref="Headset"/>
        /// <see cref="HeadsetAudioGateway"/>
        /// </summary>
        /// <seealso cref="Headset"/>
        /// <seealso cref="HeadsetAudioGateway"/>
        public static readonly Guid HeadsetHeadset = new Guid(0x00001131, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Message Access Server [0x1132]
        /// </summary>
        public static readonly Guid MessageAccessServer = new Guid(0x00001132, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Message Notification Server [0x1133]
        /// </summary>
        public static readonly Guid MessageNotificationServer = new Guid(0x00001133, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Message Access Profile [0x1134]
        /// </summary>
        public static readonly Guid MessageAccessProfile = new Guid(0x00001134, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// Bluetooth Device Identification. [0x1200]
        /// </summary>
        public static readonly Guid PnPInformation = new Guid(0x00001200, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1201]
        /// </summary>
        public static readonly Guid GenericNetworking = new Guid(0x00001201, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1202]
        /// </summary>
        public static readonly Guid GenericFileTransfer = new Guid(0x00001202, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1203]
        /// </summary>
        public static readonly Guid GenericAudio = new Guid(0x00001203, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1204]
        /// </summary>
        public static readonly Guid GenericTelephony = new Guid(0x00001204, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        ///  [0x1205]
        /// </summary>
        public static readonly Guid UPnp = new Guid(0x00001205, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        ///  [0x1206]
        /// </summary>
        public static readonly Guid UPnpIP = new Guid(0x00001206, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// ESDP_UPNP_IP_PAN [0x1300]
        /// </summary>
        public static readonly Guid UPnpIPPan = new Guid(0x00001300, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// ESDP_UPNP_IP_LAP [0x1301]
        /// </summary>
        public static readonly Guid UPnpIPLap = new Guid(0x00001301, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// ESDP_UPNP_L2CAP [0x1302]
        /// </summary>
        public static readonly Guid UPnpIPL2Cap = new Guid(0x00001302, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// Video Distribution Profile - Source [0x1303]
        /// </summary>
        public static readonly Guid VideoSource = new Guid(0x00001303, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Video Distribution Profile - Sink [0x1304]
        /// </summary>
        public static readonly Guid VideoSink = new Guid(0x00001304, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Video Distribution Profile [0x1305]
        /// </summary>
        public static readonly Guid VideoDistribution = new Guid(0x00001305, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);

        /// <summary>
        /// Health Device Profile (HDP) [0x1400]
        /// </summary>
        public static readonly Guid HealthDevice = new Guid(0x00001400, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Health Device Profile (HDP) - Source [0x1401]
        /// </summary>
        public static readonly Guid HealthDeviceSource = new Guid(0x00001401, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);
        /// <summary>
        /// Health Device Profile (HDP) - Sink [0x1402]
        /// </summary>
        public static readonly Guid HealthDeviceSink = new Guid(0x00001402, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB);


        #region Get Name
        /// <summary>
        /// Retrieves the name of the Service Class UUID that has the specified value. 
        /// </summary>
        /// <param name="uuid">
        /// The service class UUID as a <see cref="T:System.Guid"/>.
        /// </param>
        /// <returns>
        /// A string containing the name of the service class whose UUID value is <paramref name="uuid"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
        public static String GetName(Guid uuid)
        {
            System.Reflection.FieldInfo[] fields
                = typeof(BluetoothService).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (System.Reflection.FieldInfo curField in fields) {
                object rawValue = curField.GetValue(null);
                Guid fieldValue = (Guid)rawValue;
                if (fieldValue.Equals(uuid)) {
                    string fieldName = curField.Name;
                    return fieldName;
                }
            }//for

            return null;
        }


        /// <summary>
        /// Retrieves the name of the Service Class UUID that has the specified value. 
        /// </summary>
        /// <param name="uuid16">
        /// The service class UUID in the 16-bit UUID short form as a <see cref="T:System.Int16"/>.
        /// </param>
        /// <returns>
        /// A string containing the name of the service class whose UUID value is <paramref name="uuid"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
        public static String GetName(Int16 uuid16)
        {
            return GetName(CreateBluetoothUuid(uuid16));
        }

        /// <summary>
        /// Retrieves the name of the Service Class UUID that has the specified value. 
        /// </summary>
        /// <param name="uuid16">
        /// The service class UUID in the 16-bit short UUID form as a <see cref="T:System.UInt16"/>.
        /// </param>
        /// <returns>
        /// A string containing the name of the service class whose UUID value is <paramref name="uuid"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
        [CLSCompliant(false)] //use Int32 overload instead
        public static String GetName(UInt16 uuid16)
        {
            return GetName(CreateBluetoothUuid(uuid16));
        }

        /// <summary>
        /// Retrieves the name of the Service Class UUID that has the specified value. 
        /// </summary>
        /// <param name="uuid32">
        /// The service class UUID in the 32-bit short UUID form as a <see cref="T:System.Int32"/>.
        /// </param>
        /// <returns>
        /// A string containing the name of the service class whose UUID value is <paramref name="uuid"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
        public static String GetName(Int32 uuid32)
        {
            return GetName(CreateBluetoothUuid(uuid32));
        }

        /// <summary>
        /// Retrieves the name of the Service Class UUID that has the specified value. 
        /// </summary>
        /// <param name="uuid32">
        /// The service class UUID in the 32-bit UUID short form as a <see cref="T:System.UInt32"/>.
        /// </param>
        /// <returns>
        /// A string containing the name of the service class whose UUID value is <paramref name="uuid"/>,
        /// or a null reference (<c>Nothing</c> in Visual Basic) if no such constant is found.
        /// </returns>
        [CLSCompliant(false)] //use Int32 overload instead
        public static String GetName(UInt32 uuid32)
        {
            return GetName(CreateBluetoothUuid(uuid32));
        }
        #endregion

        #region Create Bluetooth UUID
        /// <summary>
        /// Create a full 128-bit Service class UUID from its 16-bit short form.
        /// </summary>
        /// <param name="uuid16">
        /// The service class UUID in the 16-bit UUID short form as a <see cref="T:System.Int16"/>.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Guid"/> containing the full 128-bit form of the
        /// supplied Bluetooth service class UUID.
        /// </returns>
        public static Guid CreateBluetoothUuid(Int16 uuid16)
        {
            return CreateBluetoothUuid(unchecked((Int32)uuid16));
        }

        /// <summary>
        /// Create a full 128-bit Service class UUID from its 16-bit short form.
        /// </summary>
        /// <param name="uuid16">
        /// The service class UUID in the 16-bit UUID short form as a <see cref="T:System.UInt16"/>.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Guid"/> containing the full 128-bit form of the
        /// supplied Bluetooth service class UUID.
        /// </returns>
        [CLSCompliant(false)] //use Int16 overload instead
        public static Guid CreateBluetoothUuid(UInt16 uuid16)
        {
            return CreateBluetoothUuid((UInt32)uuid16);
        }

        /// <summary>
        /// Create a full 128-bit Service class UUID from its 16-bit short form.
        /// </summary>
        /// <param name="uuid32">
        /// The service class UUID in the 32-bit UUID short form as a <see cref="T:System.Int32"/>.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Guid"/> containing the full 128-bit form of the
        /// supplied Bluetooth service class UUID.
        /// </returns>
        public static Guid CreateBluetoothUuid(Int32 uuid32)
        {
            // Base UUID: 00000000-0000-1000-8000-00805f9b34fb.
            Guid uuid = new Guid(uuid32, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb);
            return uuid;
        }

        /// <summary>
        /// Create a full 128-bit Service class UUID from its 16-bit short form.
        /// </summary>
        /// <param name="uuid32">
        /// The service class UUID in the 32-bit UUID short form as a <see cref="T:System.UInt32"/>.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Guid"/> containing the full 128-bit form of the
        /// supplied Bluetooth service class UUID.
        /// </returns>
        [CLSCompliant(false)] //use Int32 overload instead
        public static Guid CreateBluetoothUuid(UInt32 uuid32)
        {
            return CreateBluetoothUuid(unchecked((Int32)uuid32));
        }
        #endregion

        #region To Short form
        internal static ushort? GetAsClassId16(Guid service)
        {
            var barr = service.ToByteArray();
            UInt16 classId16 = BitConverter.ToUInt16(barr, 0);
            var recreated = BluetoothService.CreateBluetoothUuid(classId16);
            if (service == recreated) return classId16;
            else return null;
        }
        #endregion
    }
}
