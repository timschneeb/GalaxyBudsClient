using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;

namespace GalaxyBudsClient.Scripting.Hooks;

///<summary>Hook decoders of incoming messages</summary>
public interface IDecoderHook : IHook
{
    ///<summary>Postprocess decoder of incoming message. Base decoder objects are passed as reference so they can be easily modified.
    /// The <c>SPPMessage</c> object cannot be modified. If no decoder object exists for an incoming message id, it will not be handled by this callback.</summary>
    void OnDecoderCreated(SppMessage msg, ref BaseMessageParser decoder);
}