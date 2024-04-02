using System;
using System.Linq;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Sentry;

namespace GalaxyBudsClient.Message;

public static class SppMessageParserFactory
{

    static SppMessageParserFactory()
    {
        var types = typeof(SppMessageParserFactory).Assembly.GetTypes();
        RegisteredDecoders = types
            .Where(t => t is { Namespace: "GalaxyBudsClient.Message.Decoder", IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseMessageParser)))
            .ToArray();
        RegisteredEncoders = types
            .Where(t => t is { Namespace: "GalaxyBudsClient.Message.Encoder", IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(BaseMessageParser)))
            .ToArray();
    }

    private static readonly Type[] RegisteredDecoders;
    private static readonly Type[] RegisteredEncoders;

    public static BaseMessageParser? BuildParser(SppMessage msg, Models model)
    {
        BaseMessageParser? b = null;
        foreach (var t in RegisteredDecoders)
        {
            var act = Activator.CreateInstance(t);
            if (act?.GetType() != t) 
                continue;
                
            var parser = (BaseMessageParser)act;
            if (parser.HandledType != msg.Id) 
                continue;

            parser.TargetModel = model;
                
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

            parser.ParseMessage(msg);
            b = parser;
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