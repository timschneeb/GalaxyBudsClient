using System;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.ViewModels.Developer;

public class MessageViewHolder
{
    public MessageViewHolder(SppMessage msg)
    {
        Id = msg.Id.ToStringFast();
        Payload = BitConverter.ToString(msg.Payload).Replace("-", " ");
        Type = msg.IsFragment ? "Fragment/" : string.Empty + msg.Type.ToStringFast();
        Size = $"{msg.Size} bytes";
        Crc16 = msg.Crc16 == 0 ? "Pass" : "Fail";
        Message = msg;
    }
    
    public MessageViewHolder(SppAlternativeMessage msg)
    {
        Id = msg.Id.ToStringFast();
        Payload = BitConverter.ToString(msg.Payload).Replace("-", " ");
        Type = msg.Type.ToStringFast();
        Size = $"{msg.Msg.Size} bytes";
        Crc16 = msg.Msg.Crc16 == 0 ? "Pass" : "Fail";
        Message = msg.Msg;
    }
    
    public string Id { get; }
    public string Payload { get; }
    public string PayloadAscii => HexUtils.DumpAscii(Message.Payload);
    public string Type { get; }
    public string Size { get; }
    public string Crc16 { get; }
    public SppMessage Message { get; }
}