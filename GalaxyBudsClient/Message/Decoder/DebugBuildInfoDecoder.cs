using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_BUILD_INFO)]
internal class DebugBuildInfoDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public string? BuildString { get; } = DecodeBuildString(msg.Payload);

    /// <summary>
    /// Legacy Galaxy Buds (2019) sometimes embed NUL bytes in the ASCII build string.
    /// </summary>
    internal static string DecodeBuildString(byte[] payload)
    {
        var raw = Encoding.ASCII.GetString(payload);
        return raw.Replace("\0", string.Empty);
    }
}