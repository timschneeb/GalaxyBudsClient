namespace GalaxyBudsClient.Scripting.Hooks;

///<summary>Hook raw byte stream</summary>
public interface IRawStreamHook : IHook
{
    ///<summary>Handle incoming byte stream. A byte array is passed as reference so they can be easily modified.</summary>
    void OnRawDataAvailable(ref byte[] msg);
        
    ///<summary>Handle outgoing byte stream. A byte array is passed as reference so they can be easily modified.</summary>
    void OnRawDataSend(ref byte[] msg);
}