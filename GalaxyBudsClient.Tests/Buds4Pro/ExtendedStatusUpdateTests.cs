using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Buds4Pro;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds4 Pro"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";

    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    [Test]
    public void Decode_TouchMask_UsesOfficialBuds4BitOrder()
    {
        var message = SppMessage.Decode(GetTestData(Models.Buds4Pro, 4), Models.Buds4Pro, false);

        // bit6 swipe, bit3 single, bit0 hold
        message.Payload[10] = 0b0100_1001;
        var decoder = (ExtendedStatusUpdateDecoder)message.CreateDecoder()!;

        decoder.SingleTapOn.Should().BeTrue();
        decoder.DoubleTapOn.Should().BeFalse();
        decoder.TripleTapOn.Should().BeFalse();
        decoder.TouchHoldOn.Should().BeTrue();
        decoder.DoubleTapForCallOn.Should().BeFalse();
        decoder.TouchHoldOnForCallOn.Should().BeFalse();
        decoder.SwipeOn.Should().BeTrue();

        // bit5 call hold, bit4 call double, bit2 double, bit1 triple
        message.Payload[10] = 0b0011_0110;
        decoder = (ExtendedStatusUpdateDecoder)message.CreateDecoder()!;

        decoder.SingleTapOn.Should().BeFalse();
        decoder.DoubleTapOn.Should().BeTrue();
        decoder.TripleTapOn.Should().BeTrue();
        decoder.TouchHoldOn.Should().BeFalse();
        decoder.DoubleTapForCallOn.Should().BeTrue();
        decoder.TouchHoldOnForCallOn.Should().BeTrue();
        decoder.SwipeOn.Should().BeFalse();
    }

    [Test]
    public void Decode_Tail_ReadsAdaptiveVolumeBeforeQuickLaunch()
    {
        var message = SppMessage.Decode(GetTestData(Models.Buds4Pro, 4), Models.Buds4Pro, false);
        message.Payload[49] = 1; // adaptive EQ
        message.Payload[50] = 1; // adaptive volume
        message.Payload[51] = 2; // quick launch

        var decoder = (ExtendedStatusUpdateDecoder)message.CreateDecoder()!;

        decoder.AdaptiveEqEnabled.Should().BeTrue();
        decoder.AdaptiveVolumeEnabled.Should().BeTrue();
        decoder.QuickLaunchAdvanced.Should().Be(2);
    }

    private static object[] _testCases =
    [
        new TestCase
        {
            Revision = 4,
            Model = Models.Buds4Pro,
            ExpectedResult = new ExtendedStatusUpdateDecoder
            {
                TargetModel = Models.Buds4Pro,

                Revision = 4,
                EarType = 13,
                BatteryL = 70,
                BatteryR = 89,
                IsCoupled = true,
                MainConnection = DevicesInverted.L,
                PlacementL = PlacementStates.Wearing,
                PlacementR = PlacementStates.Wearing,
                WearState = LegacyWearStates.Both,
                BatteryCase = 0,
                AdjustSoundSync = false,
                EqualizerMode = 0,
                TouchpadLock = false,
                TouchpadOptionL = TouchOptions.NoiseControl,
                TouchpadOptionR = TouchOptions.NoiseControl,
                SingleTapOn = true,
                DoubleTapOn = true,
                TripleTapOn = true,
                TouchHoldOn = true,
                DoubleTapForCallOn = true,
                TouchHoldOnForCallOn = true,
                SwipeOn = true,
                NoiseControlMode = NoiseControlModes.NoiseReduction,
                VoiceWakeUp = false,
                ColorL = DeviceIds.Buds4ProBlack,
                ColorR = DeviceIds.Buds4ProBlack,
                VoiceWakeUpLang = 7,
                SeamlessConnectionEnabled = true,
                FmmRevision = 4,

                // Noise control cycle configuration: left = ANC+Ambient (0x9),
                // right = ANC+Adaptive (0xA)
                NoiseControlTouchLeftAnc = true,
                NoiseControlTouchLeftAmbient = true,
                NoiseControlTouchLeftAdaptive = false,
                NoiseControlTouchLeftOff = false,
                NoiseControlTouchAnc = true,
                NoiseControlTouchAdaptive = true,
                NoiseControlTouchAmbient = false,
                NoiseControlTouchOff = false,

                SpeakSeamlessly = false,
                AmbientSoundVolume = 2,
                NoiseReductionLevel = 4,
                HearingEnhancements = 16,
                DetectConversations = false,
                DetectConversationsDuration = 0,
                NoiseControlsWithOneEarbud = false,
                AmbientCustomVolumeOn = false,
                AmbientCustomVolumeLeft = 1,
                AmbientCustomVolumeRight = 1,
                AmbientCustomSoundTone = 2,
                OutsideDoubleTap = false,
                SideToneEnabled = false,
                CallPathControl = true,
                SpatialAudio = false,
                CustomizeConversationBoost = false,
                CustomizeNoiseReductionLevel = 0,
                NeckStretchCalibration = false,
                BixbyKeyword = 0,
                HearingTestValue = 0,
                AutoAdjustSound = false,
                SpatialAudioHeadTracking = false,

                // Buds4 tail (shifted by the autoAdjustSound byte)
                ExtraClearCallSound = false,
                ExtraHighAmbientEnabled = false,
                AutoPauseResume = false,
                HotCommandEnabled = false,
                HotCommandLanguage = 1,
                AdaptiveEqEnabled = true,
                QuickLaunchAdvanced = 0,
                AdaptiveVolumeEnabled = false,
                SirenDetect = false,
                AdaptSoundEnabled = true,
                HotCommandVersion = "1.1.0",

                IsLeftCharging = false,
                IsRightCharging = false,
                IsCaseCharging = false
            }
        }
    ];
}
