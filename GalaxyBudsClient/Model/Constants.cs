using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Model
{
    namespace Constants
    {
        public static class Uuids
        {
            public static readonly Guid SppLegacy = new("{00001102-0000-1000-8000-00805f9b34fd}");
            public static readonly Guid SppStandard = new("{00001101-0000-1000-8000-00805F9B34FB}");
            public static readonly Guid SppNew = new("{2e73a4ad-332d-41fc-90e2-16bef06523f2}");
            public static readonly Guid SmepSpp = new("{f8620674-a1ed-41ab-a8b9-de9ad655729d}"); // alt mode
            public static readonly Guid LeAudio = new("{0000184e-0000-1000-8000-00805f9b34fb}");
            public static readonly Guid Handsfree = new("{0000111e-0000-1000-8000-00805f9b34fb}");
        }
      
        [CompiledEnum]
        public enum BixbyLanguages
        {
            [Description("de-DE")]
            de = 0,
            [Description("en-GB")] 
            enGb = 1,
            [Description("en-US")]
            enUs = 2,
            [Description("es-ES")]
            es = 3,
            [Description("fr-FR")]
            fr = 4,
            [Description("it-IT")]
            it = 5,
            [Description("ko-KR")]
            kr = 6,
            [Description("pt-BR")]
            br = 7,
            [Description("zh-CN")]
            cn = 8,
            [Description("hu-HU")]
            hu = 9
        }
        
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [CompiledEnum]
        public enum Locales
        {
            [Description("English")]
            en,
            [Description("German")]
            de,
            [Description("Spanish")]
            es,
            [Description("Portuguese")]
            pt,
            [Description("Italian")]
            it,
            [Description("Korean")]
            ko,
            [Description("Japanese")]
            ja,
            [Description("Russian")]
            ru,
            [Description("Ukrainian")]
            ua,
            [Description("Czech")]
            cz,
            [Description("Turkish")]
            tr,
            [Description("Simplified-Chinese")]
            cn,
            [Description("Traditional-Chinese")]
            tw,
            [Description("Indonesian")]
            in_,
            [Description("Vietnamese")]
            vn_,
            [Description("Greek")]
            gr,
            [Description("Romanian")]
            ro,
            [Description("Hebrew")]
            il,
            [Description("French")]
            fr,
            [Description("Magyar")]
            hu,
			[Description("Nederlands")]
			nl,
            [IgnoreDataMember, Description("custom_language.xaml")]
            custom
        }
        
        public enum SpatialAudioControl
        {
            Attach = 0,
            Detach = 1,
            AttachSuccess = 2,
            DetachSuccess = 3,
            KeepAlive = 4,
            WearOnOff = 5,
            QuerySensorSupported = 6,
            SpatialBufOn = 7,
            SpatialBufOff = 8,
            QueryGyroBiasExistence = 9,
            ManualGyrocalStart = 10,
            ManualGyrocalCancel = 11,
            ManualGyrocalQueryReady = 12,
            ResetGyroInUseBias = 13,
            
            DebugResetBiasAll = 64,
            DebugResetBiasInUse = 65,
            DebugResetPrintTimestamp = 66
        }
        
        public enum SpatialAudioData
        {
            Unknown,
            BudGrv = 32,
            WearOn = 33,
            WearOff = 34,
            BudGyrocal = 35,
            BudSensorStuck = 36,
            SensorSupported = 37,
            GyroBiasExistence = 38,
            ManualGyrocalReady = 39,
            ManualGyrocalNotReady = 40,
            BudGyrocalFail = 41
        }
        
        [CompiledEnum]
        public enum TemperatureUnits
        {
            [Description("F")]
            Fahrenheit,
            [Description("C")]
            Celsius
        }
        
        [CompiledEnum]
        public enum Themes
        {    
            [LocalizableDescription(Keys.DarkmodeDisabled)]
            Light,
            [LocalizableDescription(Keys.DarkmodeEnabled)]
            Dark,
            [LocalizableDescription(Keys.DarkmodeBlurEnabled)]
            DarkBlur,
            [RequiresPlatform(PlatformUtils.Platforms.Windows, 22000 /* Windows 11 */)]
            [LocalizableDescription(Keys.DarkmodeMicaEnabled)]
            DarkMica,
            
            [IgnoreDataMember, LocalizableDescription(Keys.DarkmodeSystem)]
            System // <- Disabled
        }

        [CompiledEnum]
        public enum PopupPlacement
        {
            [LocalizableDescription(Keys.ConnpopupPlacementTl)]
            TopLeft,
            [LocalizableDescription(Keys.ConnpopupPlacementTc)]
            TopCenter,
            [LocalizableDescription(Keys.ConnpopupPlacementTr)]
            TopRight,
            [LocalizableDescription(Keys.ConnpopupPlacementBl)]
            BottomLeft,
            [LocalizableDescription(Keys.ConnpopupPlacementBc)]
            BottomCenter,
            [LocalizableDescription(Keys.ConnpopupPlacementBr)]
            BottomRight
        }
        
        [CompiledEnum]
        public enum VoiceDetectTimeouts
        {    
            [LocalizableDescription(Keys.NcVoicedetectTimeoutItem)]
            Sec5 = 5,
            [LocalizableDescription(Keys.NcVoicedetectTimeoutItem)]
            Sec10 = 10,
            [LocalizableDescription(Keys.NcVoicedetectTimeoutItem)]
            Sec15 = 15
        }
        
        [CompiledEnum]
        public enum Models
        {
            [ModelMetadata(Name = "", FwPattern = "", BuildPrefix = "")]
            NULL = 0,
            [ModelMetadata(Name = "Galaxy Buds", FwPattern = "R170", BuildPrefix = "R170")]
            Buds = 1,
            [ModelMetadata(Name = "Galaxy Buds+", FwPattern = "SM-R175", BuildPrefix = "R175")]
            BudsPlus = 2,
            [ModelMetadata(Name = "Galaxy Buds Live", FwPattern = "SM-R180", BuildPrefix = "R180")]
            BudsLive = 3,
            [ModelMetadata(Name = "Galaxy Buds Pro", FwPattern = "SM-R190", BuildPrefix = "R190")]
            BudsPro = 4,
            [ModelMetadata(Name = "Galaxy Buds2", FwPattern = "SM-R177", BuildPrefix = "R177")]
            Buds2 = 5,
            [ModelMetadata(Name = "Galaxy Buds2 Pro", FwPattern = "SM-R510", BuildPrefix = "R510")]
            Buds2Pro = 6,
            [ModelMetadata(Name = "Galaxy Buds FE", FwPattern = "SM-R400N", BuildPrefix = "R400N")]
            BudsFe = 7,
            // TODO uncomment when ready [ModelMetadata(Name = "Galaxy Buds3", FwPattern = "SM-R530", BuildPrefix = "R530")]
            Buds3 = 8,
            // TODO uncomment when ready [ModelMetadata(Name = "Galaxy Buds3 Pro", FwPattern = "SM-R630", BuildPrefix = "R630")]
            Buds3Pro = 9
        }

        [CompiledEnum]
        public enum DeviceIds
        {
            Unknown = 0,
            
            [AssociatedModel(Models.Buds)] Buds = 257,
            [AssociatedModel(Models.Buds)] BudsUnknown = 14336, // NOTE: Unknown device id mentioned by Samsung's device API
            
            [AssociatedModel(Models.BudsPlus)] BudsPlusBlue = 258,
            [AssociatedModel(Models.BudsPlus)] BudsPlusPink = 259,
            [AssociatedModel(Models.BudsPlus)] BudsPlusBlack = 260,
            [AssociatedModel(Models.BudsPlus)] BudsPlusWhite = 261,
            [AssociatedModel(Models.BudsPlus)] BudsPlusThomBrown = 262,
            [AssociatedModel(Models.BudsPlus)] BudsPlusRed = 263,
            [AssociatedModel(Models.BudsPlus)] BudsPlusDeepBlue = 264,
            [AssociatedModel(Models.BudsPlus)] BudsPlusOlympic = 265, // NOTE: Unreleased color
            [AssociatedModel(Models.BudsPlus)] BudsPlusPurple = 266,
        
            [AssociatedModel(Models.BudsLive)] BudsLiveBlack = 278,
            [AssociatedModel(Models.BudsLive)] BudsLiveWhite = 279,
            [AssociatedModel(Models.BudsLive)] BudsLiveBronze = 280,
            [AssociatedModel(Models.BudsLive)] BudsLiveRed = 281,
            [AssociatedModel(Models.BudsLive)] BudsLiveBlue = 282,
            [AssociatedModel(Models.BudsLive)] BudsLiveThomBrown = 283,
            [AssociatedModel(Models.BudsLive)] BudsLiveGrey = 284,
        
            [AssociatedModel(Models.BudsPro)] BudsProBlack = 298,
            [AssociatedModel(Models.BudsPro)] BudsProSilver = 299,
            [AssociatedModel(Models.BudsPro)] BudsProViolet = 300,
            [AssociatedModel(Models.BudsPro)] BudsProWhite = 301,
        
            [AssociatedModel(Models.Buds2)] Buds2White = 313,
            [AssociatedModel(Models.Buds2)] Buds2Black = 314,
            [AssociatedModel(Models.Buds2)] Buds2Yellow = 315, // NOTE: Unreleased color
            [AssociatedModel(Models.Buds2)] Buds2Green = 316,
            [AssociatedModel(Models.Buds2)] Buds2Violet = 317,
            [AssociatedModel(Models.Buds2)] Buds2ThomBrown = 318,
            [AssociatedModel(Models.Buds2)] Buds2MaisonKitsune = 319,
            [AssociatedModel(Models.Buds2)] Buds2AbsoluteBlack = 320,
            [AssociatedModel(Models.Buds2)] Buds2Grey = 321,
            [AssociatedModel(Models.Buds2)] Buds2Unknown = 14337, // NOTE: Unknown device id mentioned by Samsung's device API
        
            [AssociatedModel(Models.Buds2Pro)] Buds2ProGrey = 326,
            [AssociatedModel(Models.Buds2Pro)] Buds2ProWhite = 327,
            [AssociatedModel(Models.Buds2Pro)] Buds2ProViolet = 328,
        
            [AssociatedModel(Models.BudsFe)] BudsFeGraphite = 330,
            [AssociatedModel(Models.BudsFe)] BudsFeWhite = 331,
            
            // TODO Add images and find out actual color names
            [AssociatedModel(Models.Buds3)] Buds3UnknownA = 333,
            [AssociatedModel(Models.Buds3)] Buds3UnknownB = 334,
            [AssociatedModel(Models.Buds3Pro)] Buds3ProUnknownA = 340,
            [AssociatedModel(Models.Buds3Pro)] Buds3ProUnknownB = 341
        }

        [CompiledEnum]
        public enum PlacementStates
        {
            [LocalizableDescription(Keys.PlacementDisconnected)]
            Disconnected = 0,
            [LocalizableDescription(Keys.PlacementWearing)]
            Wearing = 1,
            [LocalizableDescription(Keys.PlacementNotWearing)]
            Idle = 2,
            [LocalizableDescription(Keys.PlacementInCase)]
            Case = 3,
            [LocalizableDescription(Keys.PlacementDisconnected)]
            ClosedCase = 4,
            
            // Custom states
            [LocalizableDescription(Keys.PlacementCharging)]
            Charging = 100
        }

        public enum LegacyWearStates
        {
            None = 0,
            R = 1,
            L = 16,
            Both = 17
        }
        public enum DevicesInverted
        {
            L = 1,
            R = 0
        }
        
        [CompiledEnum]
        public enum Devices
        {
            L = 0,
            R = 1
        }
        
        public enum SppRoleStates
        {
            Done = 0,
            Changing = 1,
            Unknown = 2
        }
        
        public enum NoiseControlModes
        {
            Off = 0,
            AmbientSound = 2,
            NoiseReduction = 1
        }
   
        [CompiledEnum]
        public enum NoiseControlCycleModes
        {
            [LocalizableDescription(Keys.TouchpadNoiseControlModeAncAmb)]
            AncAmb = 0,
            [LocalizableDescription(Keys.TouchpadNoiseControlModeAncOff)]
            AncOff = 1,
            [LocalizableDescription(Keys.TouchpadNoiseControlModeAmbOff)]
            AmbOff = 2,
            [IgnoreDataMember, LocalizableDescription(Keys.Unknown)]
            Unknown = 99
        }
        
        [CompiledEnum]
        public enum TouchOptions
        {
            [LocalizableDescription(Keys.TouchoptionVoice)]
            VoiceAssistant,
            [LocalizableDescription(Keys.TouchoptionQuickambientsound)]
            QuickAmbientSound,
            [LocalizableDescription(Keys.TouchoptionVolume)]
            Volume,
            [LocalizableDescription(Keys.TouchoptionAmbientsound)]
            AmbientSound,
            [LocalizableDescription(Keys.Anc)]
            Anc,
            [LocalizableDescription(Keys.TouchoptionSwitchNoisecontrols)]
            NoiseControl,
            [LocalizableDescription(Keys.TouchoptionSpotify)]
            SpotifySpotOn,
            [IgnoreDataMember, LocalizableDescription(Keys.TouchoptionCustom)]
            OtherL,
            [IgnoreDataMember, LocalizableDescription(Keys.TouchoptionCustom)]
            OtherR
        }
        
        public enum ClientDeviceTypes
        {
            Samsung = 1,
            Other = 2
        }
        
        public enum EqPresets
        {
            BassBoost = 0,
            Soft = 1,
            Dynamic = 2,
            Clear = 3,
            TrebleBoost = 4
        }
        
        public enum AmbientTypes
        {
            Default = 0,
            VoiceFocus = 1
        }
        
        [CompiledEnum]
        public enum DynamicTrayIconModes
        {
            [LocalizableDescription(Keys.SettingsDynTrayModeOff)]
            Disabled = 0,
            [LocalizableDescription(Keys.SettingsDynTrayModeBatteryMin)]
            BatteryMin = 1,
            [LocalizableDescription(Keys.SettingsDynTrayModeBatteryAvg)]
            BatteryAvg = 2
        }
    }
}
