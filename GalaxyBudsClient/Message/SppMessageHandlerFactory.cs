using System;
using System.Collections.Generic;
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
        RegisteredDecoders = new Lazy<Type[]>(() => types
            .Where(t => t is { Namespace: "GalaxyBudsClient.Message.Decoder", IsClass: true, IsAbstract: false } &&
                        t.IsSubclassOf(typeof(BaseMessageDecoder)))
            .ToArray());
        RegisteredEncoders = new Lazy<Type[]>(() => types
            .Where(t => t is { Namespace: "GalaxyBudsClient.Message.Encoder", IsClass: true, IsAbstract: false } &&
                        t.IsSubclassOf(typeof(BaseMessageEncoder)))
            .ToArray());
    }

    private static readonly Lazy<Type[]> RegisteredDecoders;
    private static readonly Lazy<Type[]> RegisteredEncoders;

    private static readonly Dictionary<MsgIds, Type?> DecoderTypeCache = new();
    
    public static BaseMessageDecoder? CreateDecoder(SppMessage msg, Models model)
    {
        BaseMessageDecoder? b = null;

        if (DecoderTypeCache.TryGetValue(msg.Id, out var cachedType))
        {
            // If cached type is null, no decoder exists for message id
            if (cachedType == null)
                return null;
                
            b = (BaseMessageDecoder?)Activator.CreateInstance(cachedType);
        }

        if (b == null)
        {
            foreach (var t in RegisteredDecoders.Value)
            {
                var act = Activator.CreateInstance(t);
                if (act?.GetType() != t)
                    continue;

                var decoder = (BaseMessageDecoder)act;
                if (decoder.HandledType != msg.Id)
                    continue;

                b = decoder;
                // Cache decoder type for message id
                DecoderTypeCache.TryAdd(msg.Id, t);
                break;
            }

            if (b == null)
            {
                // No decoder for message id exists
                DecoderTypeCache.TryAdd(msg.Id, null);
            }
        }

        if (b == null) 
            return b;
        
        b.TargetModel = model;
                
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

        b.Decode(msg);
            
        foreach (var hook in ScriptManager.Instance.DecoderHooks)
        {
            hook.OnDecoderCreated(msg, ref b);
        }

        return b;
    }
}