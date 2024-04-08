using System;
using System.Text;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

public class DebugSkuDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.DEBUG_SKU;

    public string? LeftSku { set; get; }
    public string? RightSku { set; get; }
        
    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        var payload = msg.Payload;
        if (payload.Length >= 14)
        {
            LeftSku = Encoding.UTF8.GetString(payload, 0, 14);
        }
        if (payload.Length >= 14 * 2)
        {
            RightSku = Encoding.UTF8.GetString(payload, 14, 14);
        }
    }
        
                
    public Models? ModelFromSku()
    {
        var build = LeftSku ?? RightSku;
        if (build == null)
            return null;
            
        foreach (var model in ModelsExtensions.GetValues())
        {
            var pattern = model.GetModelMetadataAttribute()?.FwPattern;
            if(pattern == null)
                continue;

            if (build.Contains(pattern))
                return model;
        }
        return null;
    }
}