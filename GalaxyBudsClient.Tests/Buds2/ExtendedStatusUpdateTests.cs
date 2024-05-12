using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Buds2;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds2"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";
    
    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    private static object[] _testCases =
    [
        new TestCase { Revision = 10, Model = Models.Buds2, ExpectedResult = new ExtendedStatusUpdateDecoder
        {
            TargetModel = Models.Buds2,

            Revision = 10,
            EarType = 3,
            BatteryL = 100,
            BatteryR = 100,
            IsCoupled = true,
            MainConnection = DevicesInverted.L,
            WearState = LegacyWearStates.Both,
            EqualizerMode = 3,
            TouchpadLock = true,
            TouchpadOptionL = TouchOptions.NoiseControl,
            TouchpadOptionR = TouchOptions.NoiseControl,
            SeamlessConnectionEnabled = true,
            AmbientSoundVolume = 1,
            PlacementL = PlacementStates.Wearing,
            PlacementR = PlacementStates.Wearing,
            BatteryCase = 51,
            OutsideDoubleTap = true,
            ColorL = DeviceIds.Buds2Green,
            ColorR = DeviceIds.Buds2Green,
            AdjustSoundSync = false,
            SideToneEnabled = true,
            ExtraHighAmbientEnabled = false,
            VoiceWakeUp = false,
            VoiceWakeUpLang = 2,
            FmmRevision = 3,
            NoiseControlMode = NoiseControlModes.NoiseReduction,
            NoiseControlTouchOff = false,
            NoiseControlTouchAnc = true,
            NoiseControlTouchAmbient = true,
            NoiseControlTouchLeftOff = false,
            NoiseControlTouchLeftAnc = true,
            NoiseControlTouchLeftAmbient = true,
            SpeakSeamlessly = false,
            NoiseReductionLevel = 0,
            AutoSwitchAudioOutput = false,
            SpatialAudio = false,
            HearingEnhancements = 16,
            SingleTapOn = true,
            DoubleTapOn = true,
            TripleTapOn = true,
            TouchHoldOn = true,
            DoubleTapForCallOn = true,
            TouchHoldOnForCallOn = true,
            TouchType = 0,
            NoiseControlsWithOneEarbud = false,
            AmbientCustomVolumeOn = false,
            AmbientCustomVolumeLeft = 1,
            AmbientCustomVolumeRight = 1,
            AmbientCustomSoundTone = 2,
            CallPathControl = true,
            IsLeftCharging = false,
            IsRightCharging = false,
            IsCaseCharging = false
        }}
    ];
}