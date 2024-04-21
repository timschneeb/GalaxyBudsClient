using System.Collections.Generic;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_SPP_ALT)]
internal class SppAlternativeMessageDecoder : BaseMessageDecoder
{
    private Dictionary<string, string> PropertyTable { get; } = new();

    public override void Decode(SppMessage msg)
    {
        PropertyTable.Clear();
        
        var alt = new SppAlternativeMessage(msg);
        var properties = new List<SppAlternativeMessage.Property>();
        if (alt.Id == MsgIds.READ_PROPERTY)
        {
            var decoded = SppAlternativeMessage.ReadProperty.Decode(alt);
            PropertyTable["Type"] = decoded.Type.ToString();
            properties = decoded.Response;
        }
        else if (alt.Id == MsgIds.NOTIFY_PROPERTY)
        {
            properties = SppAlternativeMessage.Property.Decode(alt.Payload);
        }
        
        foreach (var prop in properties)
        {
            PropertyTable[prop.Type.ToString()] = HexUtils.Dump(prop.Response, 9999, false, false, false);
        }
    }

    public override Dictionary<string, string> ToStringMap() => PropertyTable;
}