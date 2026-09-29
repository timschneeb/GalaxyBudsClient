using System;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

/// <summary>
/// Parses case software version (9 bytes) and case serial (11 bytes ASCII) from msg 205.
/// Base Galaxy Buds4 typically does not answer this command; see docs/Android-RFCOMM-host.md.
/// </summary>
[MessageDecoder(MsgIds.CRADLE_SERIAL_NUMBER)]
public class CradleSerialNumberDecoder : BaseMessageDecoder
{
    public string? SoftwareVersion { get; }
    public string? SerialNumber { get; }

    public CradleSerialNumberDecoder(SppMessage msg) : base(msg)
    {
        try
        {
            var left = new byte[9];
            Array.Copy(msg.Payload, 0, left, 0, 9);
            SoftwareVersion = Encoding.ASCII.GetString(left, 0, 9);

            var right = new byte[11];
            Array.Copy(msg.Payload, 9, right, 0, 11);
            SerialNumber = Encoding.ASCII.GetString(right, 0, 11);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse CRADLE_SERIAL_NUMBER");
        }
    }
}