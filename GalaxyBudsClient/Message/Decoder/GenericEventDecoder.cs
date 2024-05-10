using System;
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_GENERIC_EVENT)]
public class GenericEventDecoder : BaseMessageDecoder
{
    public Devices Device { get; }
    public TimeSpan Timestamp { get; }
    public byte EventId { get; }
    public MsgTypes MessageType { get; }
    public byte[] EventData { get; } 
    // EventIds and EventData contents are unknown

    public GenericEventDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Device = reader.ReadChar() == 'L' ? Devices.L : Devices.R;
        Timestamp = TimeSpan.FromMilliseconds(reader.ReadUInt32());
        EventId = reader.ReadByte(); 
        MessageType = (MsgTypes)reader.ReadByte();
        
        try
        {
            EventData = msg.Payload[7..];
        }
        catch (Exception)
        {
            EventData = Array.Empty<byte>();
        }
    }
}