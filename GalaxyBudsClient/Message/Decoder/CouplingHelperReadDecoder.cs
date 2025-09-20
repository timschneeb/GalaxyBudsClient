using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils.Extensions;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.COUPLING_HELPER_READ)]
public class CouplingHelperReadDecoder : BaseMessageDecoder
{
    public string FirmwareVersion { get; }
    public string LocalAddress { get; }
    public string PeerAddress { get; }
    public DevicesInverted HostDevice { get; }
    public bool BridgeStatus { get; }
    public byte RcvGrade { get; }
    
    public CouplingHelperReadDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);

        // Note: This string is normally 12 bytes long, the last 8 bytes are always null
        FirmwareVersion = new string(reader.ReadChars(20));

        LocalAddress = reader.ReadBytes(6).BytesToMacString();
        PeerAddress = reader.ReadBytes(6).BytesToMacString();
        
        HostDevice = (DevicesInverted)SafeReadByte(reader);
        BridgeStatus = reader.ReadBoolean();
        RcvGrade = SafeReadByte(reader);
    }
}