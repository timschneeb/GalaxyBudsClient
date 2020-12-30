using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Scripting.Hooks
{
    ///<summary>Hook messages</summary>
    public interface IMessageHook : IHook
    {
        ///<summary>Handle incoming messages. Message objects are passed as reference so they can be easily modified.</summary>
        void OnMessageAvailable(ref SPPMessage msg);
        
        ///<summary>Handle outgoing messages. Message objects are passed as reference so they can be easily modified.</summary>
        void OnMessageSend(ref SPPMessage msg);
    }
}