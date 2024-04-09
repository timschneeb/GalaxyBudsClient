using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Sentry;

namespace GalaxyBudsClient.Message;

public static partial class SppMessageHandlerFactory
{
    public static BaseMessageDecoder? CreateDecoder(SppMessage msg, Models model)
    {
        BaseMessageDecoder? decoder = null;//CreateDecoder(msg.Id);
        if (decoder == null) 
            return null;
        
        decoder.TargetModel = model;
                
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("msg-data-available", "true");
            scope.SetExtra("msg-type", msg.Type.ToString());
            scope.SetExtra("msg-id", msg.Id);
            scope.SetExtra("msg-size", msg.Size);
            scope.SetExtra("msg-total-size", msg.TotalPacketSize);
            scope.SetExtra("msg-crc16", msg.Crc16);
            scope.SetExtra("msg-payload", HexUtils.Dump(msg.Payload, 512, false, false, false));
        });

        decoder.Decode(msg);
            
        foreach (var hook in ScriptManager.Instance.DecoderHooks)
        {
            hook.OnDecoderCreated(msg, ref decoder);
        }

        return decoder;
    }
}