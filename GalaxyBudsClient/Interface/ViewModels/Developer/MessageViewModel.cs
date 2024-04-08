using System;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.ViewModels.Developer;

public class MessageViewHolder(SppMessage msg)
{
    public string Id { get; } = msg.Id.ToStringFast();
    public string Payload { get; } = BitConverter.ToString(msg.Payload).Replace("-", " ");
    public string PayloadAscii => HexUtils.DumpAscii(Message.Payload);
    public string Type { get; } = msg.IsFragment ? "Fragment/" : string.Empty + msg.Type.ToStringFast();
    public string Size { get; } = $"{msg.Size} bytes";
    public string Crc16 { get; } = msg.Crc16 == 0 ? "Pass" : "Fail";
    public SppMessage Message { get; } = msg;
}