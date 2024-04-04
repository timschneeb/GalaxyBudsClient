using System;
using System.Linq;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Sentry;

namespace GalaxyBudsClient.Message;

public static class SppMessageHandlerFactory
{

    static SppMessageHandlerFactory()
    {
        var types = typeof(SppMessageHandlerFactory).Assembly.GetTypes();
        RegisteredDecoders = types
            .Where(t => t is { Namespace: "GalaxyBudsClient.Message.Decoder", IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseMessageDecoder)))
            .ToArray();
        RegisteredEncoders = types
            .Where(t => t is { Namespace: "GalaxyBudsClient.Message.Encoder", IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseMessageEncoder)))
            .ToArray();
    }

    private static readonly Type[] RegisteredDecoders;
    private static readonly Type[] RegisteredEncoders;

    public static BaseMessageDecoder? CreateDecoder(SppMessage msg, Models model)
    {
        BaseMessageDecoder? b = null;
        foreach (var t in RegisteredDecoders)
        {
            var act = Activator.CreateInstance(t);
            if (act?.GetType() != t) 
                continue;
                
            var decoder = (BaseMessageDecoder)act;
            if (decoder.HandledType != msg.Id) 
                continue;

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
            b = decoder;
            break;
        }

        if (b == null) 
            return b;
            
        foreach (var hook in ScriptManager.Instance.DecoderHooks)
        {
            hook.OnDecoderCreated(msg, ref b);
        }

        return b;
    }
}