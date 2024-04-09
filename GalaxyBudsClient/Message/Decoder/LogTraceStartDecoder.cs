using System;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.LOG_TRACE_START)]
internal class LogTraceStartDecoder : BaseMessageDecoder
{
    public int DataSize { set; get; }
    public short PartialDataMaxSize { set; get; }
    public DevicesInverted DeviceType { set; get; }
    public bool Coupled { set; get; }
    public int FragmentCount { set; get; }

    public override void Decode(SppMessage msg)
    {
        DataSize = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
        DeviceType = (DevicesInverted) msg.Payload[6];
        Coupled = msg.Payload[7] == 0;
        FragmentCount = (int)Math.Ceiling(DataSize/(double)PartialDataMaxSize);
    }
}