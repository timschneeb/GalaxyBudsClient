using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.BudsLive;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds Live"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";
    
    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    private static object[] _testCases =
    [
        new TestCase { Revision = 9, Model = Models.BudsLive, ExpectedResult = new ExtendedStatusUpdateDecoder
        {
            TargetModel = Models.BudsLive,

            Revision = 9,
            EarType = 1,
            BatteryL = 15,
            BatteryR = 97,
            IsCoupled = true,
            MainConnection = DevicesInverted.R,
            WearState = LegacyWearStates.Both,
            EqualizerMode = 0,
            TouchpadLock = false,
            TouchpadOptionL = TouchOptions.Anc,
            TouchpadOptionR = TouchOptions.Anc,
            SeamlessConnectionEnabled = true,
            AmbientSoundVolume = 0,
            PlacementL = PlacementStates.Wearing,
            PlacementR = PlacementStates.Wearing,
            BatteryCase = 100,
            OutsideDoubleTap = false,
            ColorL = DeviceIds.BudsLiveWhite,
            ColorR = DeviceIds.BudsLiveWhite,
            AdjustSoundSync = false,
            SideToneEnabled = false,
            ExtraHighAmbientEnabled = false,
            VoiceWakeUp = false,
            VoiceWakeUpLang = 0,
            FmmRevision = 3,
            NoiseCancelling = true,
            HearingEnhancements = 0x10,
            CallPathControl = true
        }}
    ];
}