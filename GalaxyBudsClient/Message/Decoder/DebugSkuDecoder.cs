using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_SKU)]
public class DebugSkuDecoder : BaseMessageDecoder
{
    public string? LeftSku { get; }
    public string? RightSku { get; }
        
    public DebugSkuDecoder(SppMessage msg) : base(msg)
    {
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

            if (model != Models.NULL && build.Contains(pattern))
                return model;
        }
        return null;
    }
}