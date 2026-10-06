using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Decoder;

[TestFixture]
public class DebugBuildInfoDecoderTests
{
    [Test]
    public void DecodeBuildString_stripsEmbeddedNul_fromLegacyBudsPayload()
    {
        var payload = "R\0Jun 11 2020/1234567890ABCDEF"u8.ToArray();
        DebugBuildInfoDecoder.DecodeBuildString(payload).Should().Be("RJun 11 2020/1234567890ABCDEF");
    }

    [Test]
    public void CreateDecoder_stripsNul_inBuildString()
    {
        var payload = "R\0Jun 11 2020/1234567890ABCDEF"u8.ToArray();
        var message = new SppMessage(MsgIds.DEBUG_BUILD_INFO, MsgTypes.Response, payload, Models.Buds);
        var decoder = message.CreateDecoder().Should().BeOfType<DebugBuildInfoDecoder>().Subject;
        decoder.BuildString.Should().Be("RJun 11 2020/1234567890ABCDEF");
    }
}
