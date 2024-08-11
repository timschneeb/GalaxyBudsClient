using System;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using Serilog;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.HIDDEN_CMD_DATA)]
public class HiddenCmdDataEncoder : BaseMessageEncoder
{
    public string CommandId { get; set; } = "0000";
    public string? Parameter { get; set; }

    public override SppMessage Encode()
    { 
        var command = Encoding.ASCII.GetBytes(CommandId).ToList();
        if (!string.IsNullOrEmpty(Parameter))
        {
            command.Add(0x00);
            command.AddRange(Encoding.ASCII.GetBytes(Parameter));
            command.Add(0x00);
        }

        Log.Debug("DoSendCommand: Sending hidden command: {Command}", Convert.ToHexString(command.ToArray()));
        return new SppMessage(MsgIds.HIDDEN_CMD_DATA, MsgTypes.Request, command.ToArray());
    }
}