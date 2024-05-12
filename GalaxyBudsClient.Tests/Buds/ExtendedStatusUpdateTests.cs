using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Buds;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the 1st-gen Buds"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";
    
    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    private static object[] _testCases =
    [
        new TestCase { Revision = 3, Model = Models.Buds, ExpectedResult = new ExtendedStatusUpdateDecoder
        {
            TargetModel = Models.Buds,

            Revision = 3,
            EarType = 0,
            BatteryL = 70,
            BatteryR = 95,
            IsCoupled = true,
            MainConnection = DevicesInverted.R,
            WearState = LegacyWearStates.None,
            PlacementL = PlacementStates.Idle,
            PlacementR = PlacementStates.Idle,
            EqualizerMode = 7,
            TouchpadLock = false,
            TouchpadOptionL = TouchOptions.Volume,
            TouchpadOptionR = TouchOptions.Volume,
            SeamlessConnectionEnabled = false,
            AmbientSoundEnabled = false,
            AmbientSoundVolume = 2,
            AmbientSoundMode = AmbientTypes.Default,
            EqualizerEnabled = true
        }}
    ];
}