using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message.Encoder;

public class UpdateTimeEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.UPDATE_TIME;
    public long Timestamp { get; set; } = -1;
    public int Offset { get; set; } = -1;
    
    public override SppMessage Encode()
    {
        if (Timestamp < 0 || Offset < 0)
        {
            var span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            Timestamp = (long)span.TotalMilliseconds;
            Offset = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds;
        }
    
        var payload = new byte[12];
        var timestampRaw = BitConverter.GetBytes(Timestamp);
        var offsetRaw = BitConverter.GetBytes(Offset);
        Array.Copy(timestampRaw, 0, payload, 0, 8);
        Array.Copy(payload, 8, offsetRaw, 0, 4);
        
        return new SppMessage(HandledType, MsgTypes.Request, payload);
    }
}