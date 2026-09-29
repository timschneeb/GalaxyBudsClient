using System.Text;
using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests.Decoder;

[TestFixture]
public class CradleSerialNumberDecoderTests
{
    [Test]
    public void ParsesCaseSoftwareAndSerial_fromTwentyBytePayload()
    {
        var payload = new byte[20];
        Encoding.ASCII.GetBytes("SW1.0.0  ").CopyTo(payload, 0);
        Encoding.ASCII.GetBytes("CASE1234567").CopyTo(payload, 9);

        var message = new SppMessage(MsgIds.CRADLE_SERIAL_NUMBER, MsgTypes.Response, payload, Models.Buds3Pro);
        var decoder = message.CreateDecoder().Should().BeOfType<CradleSerialNumberDecoder>().Subject;

        decoder.SoftwareVersion.Should().Be("SW1.0.0  ");
        decoder.SerialNumber.Should().Be("CASE1234567");
    }
}
