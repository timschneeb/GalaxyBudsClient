using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Buds3Pro;

[TestFixture, Description("Test ExtendedStatusUpdate parser for the Buds3 Pro"), TestOf(typeof(ExtendedStatusUpdateDecoder))]
public class ExtendedStatusUpdateTests
{
    [Test, Description("ExtraClearCallSound must be consumed to keep later fields aligned")]
    public void Decode_ConsumesExtraClearCallSoundByte()
    {
        var payload = new byte[57];
        payload[0] = 2; // Revision, enables HotCommandLanguageUpdate
        payload[2] = 100; // BatteryL
        payload[3] = 100; // BatteryR
        payload[4] = 1; // IsCoupled
        payload[41] = 1; // SpatialAudioHeadTracking
        payload[43] = 1; // ExtraClearCallSound
        payload[44] = 1; // ExtraHighAmbientEnabled
        payload[45] = 1; // AutoPauseResume
        payload[46] = 1; // HotCommandEnabled
        payload[47] = 7; // HotCommandLanguage
        payload[48] = 3; // PeqMod
        payload[49] = 1; // AdaptiveVolumeEnabled
        payload[50] = 5; // LightingControl
        payload[51] = 6; // DoublePinchAndHoldAdvanced
        payload[52] = 1; // SirenDetect
        payload[53] = 1; // AdaptSoundEnabled
        payload[54] = 1; // HotCommandVersion major
        payload[55] = 2; // HotCommandVersion minor
        payload[56] = 3; // HotCommandVersion patch

        var message = new SppMessage(MsgIds.EXTENDED_STATUS_UPDATED, MsgTypes.Request, payload, Models.Buds3Pro);
        var decoder = new ExtendedStatusUpdateDecoder(message);

        decoder.ExtraClearCallSound.Should().BeTrue();
        decoder.ExtraHighAmbientEnabled.Should().BeTrue();
        decoder.AutoPauseResume.Should().BeTrue();
        decoder.HotCommandEnabled.Should().BeTrue();
        decoder.HotCommandLanguage.Should().Be(7);
        decoder.PeqMod.Should().Be(3);
        decoder.AdaptiveVolumeEnabled.Should().BeTrue();
        decoder.LightingControl.Should().Be(5);
        decoder.DoublePinchAndHoldAdvanced.Should().Be(6);
        decoder.SirenDetect.Should().BeTrue();
        decoder.AdaptSoundEnabled.Should().BeTrue();
        decoder.HotCommandVersion.Should().Be("1.2.3");
    }
}
