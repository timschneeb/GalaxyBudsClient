using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.BudsPro;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds Pro"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";
    
    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    private static object[] _testCases =
    [
        new TestCase { Revision = 10, Model = Models.BudsPro, ExpectedResult = new ExtendedStatusUpdateDecoder
        {
            TargetModel = Models.BudsPro,

            Revision = 10,
            EarType = 2,
            BatteryL = 89,
            BatteryR = 94,
            IsCoupled = true,
            MainConnection = DevicesInverted.L,
            WearState = LegacyWearStates.L,
            EqualizerMode = 1,
            TouchpadLock = false,
            TouchpadOptionL = TouchOptions.NoiseControl,
            TouchpadOptionR = TouchOptions.NoiseControl,
            SeamlessConnectionEnabled = true,
            AmbientSoundVolume = 2,
            PlacementL = PlacementStates.Wearing,
            PlacementR = PlacementStates.Case,
            BatteryCase = 100,
            OutsideDoubleTap = true,
            ColorL = DeviceIds.BudsProBlack,
            ColorR = DeviceIds.BudsProBlack,
            AdjustSoundSync = false,
            SideToneEnabled = false,
            ExtraHighAmbientEnabled = false,
            VoiceWakeUp = false,
            VoiceWakeUpLang = 1,
            FmmRevision = 3,
            NoiseControlMode = NoiseControlModes.Off,
            NoiseControlTouchOff = true,
            NoiseControlTouchAnc = true,
            NoiseControlTouchAmbient = false,
            NoiseControlTouchLeftOff = true,
            NoiseControlTouchLeftAnc = true,
            NoiseControlTouchLeftAmbient = false,
            SpeakSeamlessly = false,
            NoiseReductionLevel = 1,
            AutoSwitchAudioOutput = false,
            DetectConversations = false,
            DetectConversationsDuration = 2,
            SpatialAudio = false,
            HearingEnhancements = 16,
            NoiseControlsWithOneEarbud = false,
            AmbientCustomVolumeOn = false,
            AmbientCustomVolumeLeft = 2,
            AmbientCustomVolumeRight = 2,
            AmbientCustomSoundTone = 2,
            CallPathControl = true
        }}
    ];
}