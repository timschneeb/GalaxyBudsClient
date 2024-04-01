using System;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class LogTraceStartParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.LOG_TRACE_START;
        
    public int DataSize { set; get; }
    public short PartialDataMaxSize { set; get; }
    public DevicesInverted DeviceType { set; get; }
    public bool Coupled { set; get; }
    public int FragmentCount { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        DataSize = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
        DeviceType = (DevicesInverted) msg.Payload[6];
        Coupled = msg.Payload[7] == 0;
        FragmentCount = (int)Math.Ceiling((double)DataSize/(double)PartialDataMaxSize);
    }
}