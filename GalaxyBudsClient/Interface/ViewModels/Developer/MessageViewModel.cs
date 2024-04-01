using System;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.ViewModels.Developer;

public class MessageViewHolder(SppMessage msg)
{
    public string Id { get; } = GetEnumName(typeof(MsgIds),msg.Id);
    public string Payload { get; } = BitConverter.ToString(msg.Payload).Replace("-", " ");
    public string PayloadAscii { get; } = HexUtils.DumpAscii(msg.Payload);
    public string Type { get; } = msg.IsFragment ? "Fragment/" : string.Empty + GetEnumName(typeof(MsgTypes), msg.Type);
    public string Size { get; } = $"{msg.Size} bytes";
    public string Crc16 { get; } = msg.Crc16 == 0 ? "Pass" : "Fail";
    public SppMessage Message { get; } = msg;

    private static string GetEnumName(Type t, object i)
    {
        try
        {
            var name = Enum.GetName(t, i);
            if (name == null)
                throw new ArgumentException("Enum member name is null");
            return $"{(name.StartsWith("UNKNOWN_") ? "Unknown" : name)} ({Convert.ToInt32(i)})";
        }
        catch (ArgumentNullException)
        {
            return $"Unknown ({Convert.ToInt32(i)})";
        }
    }
}