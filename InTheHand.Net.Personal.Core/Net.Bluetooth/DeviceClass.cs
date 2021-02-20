// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.DeviceClass
// 
// Copyright (c) 2003-2009 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Class of Device flags as assigned in the Bluetooth specifications.
    /// </summary>
    /// <remarks>
    /// <para>Is returned by the property <see
    /// cref="P:InTheHand.Net.Bluetooth.ClassOfDevice.Device">ClassOfDevice.Device</see>.
    /// </para>
    /// <para>Defined in Bluetooth Specifications <see href="https://www.bluetooth.org/Technical/AssignedNumbers/baseband.htm"/>.
    /// </para>
    /// </remarks>
    public enum DeviceClass
    {

        /// <summary>
        /// Miscellaneous &#x2014; 
        /// [Ref #2: Used where a more specific Major Device Class code
        /// is not suited (but only as specified in this document). Devices
        /// that do not have a major class code assigned can use the all-1 code
        /// (<see cref="F:InTheHand.Net.Bluetooth.DeviceClass.Uncategorized"/>)
        /// until 'classified']
        /// </summary>
        Miscellaneous = 0,

        /// <summary>
        /// Major class: Computer (desktop,notebook, PDA, organizers, .... ).
        /// </summary>
        Computer = 0x000100,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Desktop workstation.
        /// </summary>
        DesktopComputer = 0x000104,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Server-class computer.
        /// </summary>
        ServerComputer = 0x000108,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Laptop.
        /// </summary>
        LaptopComputer = 0x00010c,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Handheld PC/PDA (clam shell).
        /// </summary>
        HandheldComputer = 0x000110,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Palm sized PC/PDA.
        /// </summary>
        PdaComputer = 0x000114,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Wearable computer (Watch sized).
        /// </summary>
        WearableComputer = 0x000118,
        /// <summary>
        /// Major class: Computer
        /// &#x2022; Minor class: Tablet.
        /// </summary>
        TabletComputer = 0x00011c,

        /// <summary>
        /// Major class: Phone (cellular, cordless, payphone, modem, ...).
        /// </summary>
        Phone = 0x000200,
        /// <summary>
        /// Major class: Phone
        /// &#x2022; Minor class: Cellular.
        /// </summary>
        CellPhone = 0x000204,
        /// <summary>
        /// Major class: Phone
        /// &#x2022; Minor class: Cordlss.
        /// </summary>
        CordlessPhone = 0x000208,
        /// <summary>
        /// Major class: Phone
        /// &#x2022; Minor class: Smart phone.
        /// </summary>
        SmartPhone = 0x00020c,
        /// <summary>
        /// Major class: Phone
        /// &#x2022; Minor class: Wired modem or voice gateway.
        /// </summary>
        WiredPhone = 0x000210,
        /// <summary>
        /// Major class: Phone
        /// &#x2022; Minor class: Common ISDN Access.
        /// </summary>
        IsdnAccess = 0x000214,

#pragma warning disable 1591
        /// <summary>
        /// Major class: LAN /Network Access point.
        /// </summary>
        AccessPointAvailable = 0x000300,
        AccessPoint1To17 = 0x000320,
        AccessPoint17To33 = 0x000340,
        AccessPoint33To50 = 0x000360,
        AccessPoint50To67 = 0x000380,
        AccessPoint67To83 = 0x0003a0,
        AccessPoint83To99 = 0x0003c0,
        AccessPointNoService = 0x0003e0,

        /// <summary>
        /// Major class: Audio/Video (headset,speaker,stereo, video display, vcr.....
        /// </summary>
        AudioVideoUnclassified = 0x000400,
        AudioVideoHeadset = 0x000404,
        AudioVideoHandsFree = 0x000408,
        AudioVideoMicrophone = 0x000410,
        AudioVideoLoudSpeaker = 0x000414,
        AudioVideoHeadphones = 0x000418,
        AudioVideoPortable = 0x00041c,
        AudioVideoCar = 0x000420,
        AudioVideoSetTopBox = 0x000424,
        AudioVideoHiFi = 0x000428,
        AudioVideoVcr = 0x00042c,
        AudioVideoVideoCamera = 0x000430,
        AudioVideoCamcorder = 0x000434,
        AudioVideoMonitor = 0x000438,
        AudioVideoDisplayLoudSpeaker = 0x00043c,
        AudioVideoVideoConferencing = 0x000440,
        AudioVideoGaming = 0x000448,

        /// <summary>
        /// Major class: Peripheral (mouse, joystick, keyboards, ..... ).
        /// </summary>
        Peripheral = 0x000500,
        PeripheralJoystick = 0x000504,
        PeripheralGamepad = 0x000508,
        PeripheralRemoteControl = 0x00050c,
        PeripheralSensingDevice = 0x000510,
        PeripheralDigitizerTablet = 0x000514,
        PeripheralCardReader = 0x000518,

        PeripheralKeyboard = 0x000540,
        PeripheralPointingDevice = 0x000580,
        PeripheralCombinedKeyboardPointingDevice = 0x0005c0,

        /// <summary>
        /// Major class: Imaging (printing, scanner, camera, display, ...).
        /// </summary>
        Imaging = 0x000600,
        ImagingDisplay = 0x000610,
        ImagingCamera = 0x000620,
        ImagingScanner = 0x000640,
        ImagingPrinter = 0x000680,

        /// <summary>
        /// Major class: Wearable.
        /// </summary>
        Wearable = 0x000700,
        WearableWristWatch = 0x000704,
        WearablePager = 0x000708,
        WearableJacket = 0x00070c,
        WearableHelmet = 0x000710,
        WearableGlasses = 0x000714,

        /// <summary>
        /// Major class: Toy.
        /// </summary>
        Toy = 0x000800,
        ToyRobot = 0x000804,
        ToyVehicle = 0x000808,
        ToyFigure = 0x00080c,
        ToyController = 0x000810,
        ToyGame = 0x000814,

        /// <summary>
        /// Major class: Medical.
        /// </summary>
        Medical = 0x900,
        MedicalBloodPressureMonitor = 0x904,
        MedicalThermometer = 0x908,
        MedicalWeighingScale = 0x90c,
        MedicalGlucoseMeter = 0x910,
        MedicalPulseOximeter = 0x914,
        MedicalHeartPulseRateMonitor = 0x918,
        MedicalDataDisplay = 0x91c,
        MedicalStepCounter = 0x920,
        MedicalBodyCompositionAnalyzer = 0x924,
        MedicalPeakFlowMonitor = 0x928,
        MedicalMedicationMonitor = 0x92C,
        MedicalKneeProsthesis = 0x930,
        MedicalAnkleProsthesis = 0x934,
        MedicalGenericHealthManager = 0x938,
        MedicalPersonalMobilityDevice = 0x93C,

        /// <summary>
        /// Uncategorized, specific device code not specified
        /// &#x2014; see <see cref="F:InTheHand.Net.Bluetooth.DeviceClass.Miscellaneous"/>
        /// </summary>
        Uncategorized = 0x001f00,
    }

    /// <exclude/>
    public static class DeviceClass_Masks
    {
        //MaskServiceClasses      = 0xffe000,
        public const DeviceClass MaskMajorDeviceClass = (DeviceClass)0x001f00;
        public const DeviceClass MaskMinorDeviceClass = (DeviceClass)0x0000fc;
        public const DeviceClass MaskDeviceClass = (DeviceClass)MaskMajorDeviceClass | MaskMinorDeviceClass;
        //MaskFormatType          = 0x000003,
    }
}
