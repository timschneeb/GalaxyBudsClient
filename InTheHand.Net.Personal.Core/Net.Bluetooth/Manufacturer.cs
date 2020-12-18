// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Manufacturer
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System.Diagnostics.CodeAnalysis;
namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Manufacturer codes.
    /// </summary>
    /// <remarks>Defined in Bluetooth Specifications <see href="https://www.bluetooth.org/Technical/AssignedNumbers/identifiers.htm"/>.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum Manufacturer : short
    {
#pragma warning disable 1591

        Unknown = -1,

        Ericsson = 0,
        Nokia = 1,
        Intel = 2,
        Ibm = 3,
        Toshiba = 4,
        ThreeCom = 5,
        Microsoft = 6,
        Lucent = 7,
        Motorola = 8,
        Infineon = 9,
        CambridgeSiliconRadio = 10,
        SiliconWave = 11,
        DigiAnswer = 12,
        TexasInstruments = 13,
        Parthus = 14,
        Broadcom = 15,
        Mitel = 16,
        Widcomm = 17,
        Zeevo = 18,
        Atmel = 19,
        Mitsubishi = 20,
        RtxTelecom = 21,
        KCTechnology = 22,
        Newlogic = 23,
        Transilica = 24,
        RohdeSchwarz = 25,
        TtpCom = 26,
        Signia = 27,
        Conexant = 28,
        Qualcomm = 29,
        Inventel = 30,
        AvmBerlin = 31,
        BandSpeed = 32,
        Mansella = 33,
        Nec = 34,
        WavePlusTechnology = 35,
        Alcatel = 36,
        PhilipsSemiconductor = 37,
        CTechnologies = 38,
        OpenInterface = 39,
        RFMicroDevices = 40,
        Hitachi = 41,
        SymbolTechnologies = 42,
        Tenovis = 43,
        MacronixInternational = 44,
        GctSemiconducter = 45,
        NorwoodSystems = 46,
        MewTelTechnology = 47,
        STMicroelectronics = 48,
        Synopsys = 49,
        RedM = 50,
        Commil = 51,
        Catc = 52,
        Eclipse = 53,
        RenesasTechnology = 54,
        Mobilian = 55,
        Terax = 56,
        IntegratedSystemSolution = 57,
        Matsushita = 58,
        Gennum = 59,
        ResearchInMotion = 60,
        IPextreme = 61,
        SystemsAndChips = 62,
        BluetoothSig = 63,
        SeikoEpson = 64,
        IntegratedSiliconSolutionTaiwan = 65,
        Conwise = 66,
        Parrot = 67,
        SocketMobile = 68,
        AtherosCommunications = 69,
        MediaTek = 70,
        Bluegiga = 71,
        MarvellTechnologyGroup = 72,
        ThreeDSP = 73,
        AccelSemiconductor = 74,
        ContinentalAutomotiveSystems = 75,
        Apple = 76,
        StaccatoCommunications = 77,
        AvagoTechnologies = 78,
        APT = 79,
        SiRFTechnology = 80,
        TzeroTechnologies = 81,
        JandMCorporation = 82,
        Free2move = 83,
        ThreeDiJoy = 84,
        Plantronics = 85,
        SonyEricsson = 86,
        HarmanInternationalIndustries = 87,
        Vizio = 88,
        NordicSemiconductor = 89,
        EMMicroelectronicMarin = 90,
        RalinkTechnology = 91,
        BelkinInternational = 92,
        RealtekSemiconductor = 93,
        StonestreetOne = 94,
        Wicentric = 95,
        RivieraWaves = 96,
        RdaMicroelectronics = 97,
        GibsonGuitars = 98,
        MiCommand = 99,
        BandXiInternational = 100,
        HewlettPackard = 101,
        NineSolutions = 102,
        GnNetcom = 103,
        GeneralMotors = 104,
        AAndDEngineering = 105,
        MindTree = 106,
        PolarElectro = 107,
        BeautifulEnterpriseCo = 108,
        BriarTek = 109,
        SummitDataCommunications = 110,
        SoundId = 111,
        Monster = 112,
        connectBlue = 113,
        ShangHaiSuperSmartElectronics = 114,
        GroupSense = 115,
        Zomm = 116,
        SamsungElectronics = 117,
        CreativeTechnology = 118,
        LairdTechnologies = 119,
        Nike = 120,
        Lesswire = 121,
        MStarSemiconductor = 122,
        HanlynnTechnologies = 123,
        AAndRCambridge = 124,
        SeersTechnology = 125,
        SportsTrackingTechnologies = 126,
        AutonetMobile = 127,
        DeLormePublishingCompany = 128,
        WuXiVimicro = 129,

        // TEMP
        [System.Obsolete("FAKE TEMPORARY VALUE, DO NOT USE.")]
        IvtBlueSoleilXxxx = unchecked((short)65000),
        [System.Obsolete("FAKE TEMPORARY VALUE, DO NOT USE.")]
        BlueZXxxx = unchecked((short)65001),
        [System.Obsolete("FAKE TEMPORARY VALUE, DO NOT USE.")]
        AndroidXxxx = unchecked((short)65002),

        // <summary>
        // For use in internal and interoperability tests before a Company ID has been assigned.
        // May not be used in products.
        // Only used in Link Manager testing.
        // </summary>
        //InternalUse    = 65535,
    }
}
