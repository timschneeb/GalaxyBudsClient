using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.BudsPlus;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds+"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests : MessageTests<ExtendedStatusUpdateDecoder>
{
    protected override string TestDataGroup => "ExtendedStatusUpdate";
    
    [Test, TestCaseSource(nameof(_testCases)), Description("Decode ExtendedStatusUpdate messages")]
    public void Decode(TestCase testCase) => DecodeAndVerify(testCase);

    private static object[] _testCases =
    [
        new TestCase { Revision = 13, Model = Models.BudsPlus, ExpectedResult = new ExtendedStatusUpdateDecoder
        {
            TargetModel = Models.BudsPlus,

            Revision = 13,
            EarType = 0,
            BatteryL = 87,
            BatteryR = 84,
            IsCoupled = true,
            MainConnection = DevicesInverted.R,
            WearState = LegacyWearStates.Both,
            EqualizerMode = 3,
            TouchpadLock = false,
            TouchpadOptionL = TouchOptions.AmbientSound,
            TouchpadOptionR = TouchOptions.AmbientSound,
            SeamlessConnectionEnabled = true,
            AmbientSoundVolume = 0,
            PlacementL = PlacementStates.Wearing,
            PlacementR = PlacementStates.Wearing,
            BatteryCase = 101,
            OutsideDoubleTap = true,
            ColorL = DeviceIds.BudsPlusBlack,
            ColorR = DeviceIds.BudsPlusBlack,
            AdjustSoundSync = false,
            SideToneEnabled = false,
            ExtraHighAmbientEnabled = false,
            FmmRevision = 3,
            CallPathControl = true
        }}
    ];
}