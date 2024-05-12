using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.BudsFe;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds FE"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";
    
    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    private static object[] _testCases =
    [
        new TestCase { Revision = 2, Model = Models.BudsFe, ExpectedResult = new ExtendedStatusUpdateDecoder
        {
            TargetModel = Models.BudsFe,

            Revision = 2,
            EarType = 6,
            BatteryL = 58,
            BatteryR = 89,
            IsCoupled = true,
            MainConnection = DevicesInverted.R,
            WearState = LegacyWearStates.Both,
            EqualizerMode = 0,
            TouchpadLock = false,
            TouchpadOptionL = TouchOptions.Volume,
            TouchpadOptionR = TouchOptions.Volume,
            SeamlessConnectionEnabled = true,
            AmbientSoundVolume = 1,
            PlacementL = PlacementStates.Wearing,
            PlacementR = PlacementStates.Wearing,
            BatteryCase = 0,
            OutsideDoubleTap = false,
            ColorL = DeviceIds.BudsFeGraphite,
            ColorR = DeviceIds.BudsFeGraphite,
            AdjustSoundSync = false,
            SideToneEnabled = false,
            ExtraHighAmbientEnabled = false,
            VoiceWakeUp = false,
            VoiceWakeUpLang = 3,
            FmmRevision = 4,
            NoiseControlMode = NoiseControlModes.Off,
            NoiseControlTouchOff = true,
            NoiseControlTouchAnc = true,
            NoiseControlTouchAmbient = false,
            NoiseControlTouchLeftOff = false,
            NoiseControlTouchLeftAnc = true,
            NoiseControlTouchLeftAmbient = true,
            SpeakSeamlessly = false,
            NoiseReductionLevel = 0,
            AutoSwitchAudioOutput = false,
            DetectConversations = false,
            DetectConversationsDuration = 0,
            SpatialAudio = false,
            HearingEnhancements = 16,
            SingleTapOn = true,
            DoubleTapOn = true,
            TripleTapOn = true,
            TouchHoldOn = true,
            DoubleTapForCallOn = false,
            TouchHoldOnForCallOn = false,
            TouchType = 0,
            NoiseControlsWithOneEarbud = true,
            AmbientCustomVolumeOn = true,
            AmbientCustomVolumeLeft = 1,
            AmbientCustomVolumeRight = 1,
            AmbientCustomSoundTone = 3,
            CallPathControl = true,
            IsLeftCharging = false,
            IsRightCharging = false,
            IsCaseCharging = false,
            HearingTestValue = 157,
            BixbyKeyword = 0,
            NeckStretchCalibration = false,
            CustomizeNoiseReductionLevel = 3, // unused value; doesn't seem right
            CustomizeConversationBoost = true, // unused
            ExtraClearCallSound = false,
            SpatialAudioHeadTracking = true,
            AutoAdjustSound = true
        }}
    ];
}