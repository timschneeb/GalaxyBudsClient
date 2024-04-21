using System;
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
        switch (alt.Id)
        {
            case MsgIds.READ_PROPERTY:
            {
                var decoded = SppAlternativeMessage.ReadProperty.Decode(alt);
                PropertyTable["Type"] = decoded.Type.ToString();
                properties = decoded.Response;
                break;
            }
            case MsgIds.NOTIFY_PROPERTY:
                properties = SppAlternativeMessage.Property.Decode(alt.Payload);
                break;
        }

        foreach (var prop in properties)
        {
            PropertyTable[prop.Type.ToString()] = prop.Response.Length switch
            {
                // Display single byte as decimal number 
                1 => Convert.ToString(prop.Response[0]),
                // Display short as decimal number
                2 => BitConverter.ToUInt16(prop.Response).ToString(),
                // Display hex + ascii representation other values
                _ => HexUtils.Dump(prop.Response, 9999, false, false, false).TrimEnd() + " (ascii: " +
                     HexUtils.DumpAscii(prop.Response) + ")"
            };
        }
    }

    public override Dictionary<string, string> ToStringMap() => PropertyTable;
}