using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.GET_FMM_CONFIG)]
public class GetFmmConfigDecoder : BaseMessageDecoder
{
    public byte Revision { get; }
    
    public bool? LeftFindingSupport { get; }
    public bool? LeftE2E { get; }
    public string? LeftSecretKey { get; }
    public int LeftMaxN { get; }
    public byte LeftRegion { get; }
    public string? LeftFmmToken { get; }
    public string? LeftIv { get; }
    public string? LeftSn { get; }
    public bool? LeftLostmodeState { get; }
    public string? LeftLostmodePrefix { get; }
    public string? LeftLostmodeLink { get; }

    public bool? RightFindingSupport { get; }
    public bool? RightE2E { get; }
    public string? RightSecretKey { get; }
    public int RightMaxN { get; }
    public byte RightRegion { get; }
    public string? RightFmmToken { get; }
    public string? RightIv { get; }
    public string? RightSn { get; }
    public bool? RightLostmodeState { get; }
    public string? RightLostmodePrefix { get; }
    public string? RightLostmodeLink { get; }
    
    // TODO Add UI to display & erase FMM data?
    public GetFmmConfigDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var binaryReader = new BinaryReader(stream);
        
        Revision = binaryReader.ReadByte();
        
        var connectionStates = binaryReader.ReadByte();
        var leftConnected = ByteArrayUtils.ValueOfLeft(connectionStates) == 1;
        var rightConnected = ByteArrayUtils.ValueOfRight(connectionStates) == 1;

        if (leftConnected)
        {
            try
            {
                LeftFindingSupport = binaryReader.ReadByte().ToBooleanOrDefault();
                LeftE2E = binaryReader.ReadByte().ToBooleanOrDefault();

                var secretKeyBytes = binaryReader.ReadBytes(16);
                LeftSecretKey = Revision < 3 ? 
                    secretKeyBytes.ToBase64OrDefault() : 
                    MakeXor(secretKeyBytes).ToBase64OrDefault();

                var maxN = binaryReader.ReadInt32();
                LeftMaxN = maxN == 0 ? -1 : maxN;
                LeftRegion = binaryReader.ReadByte();

                var tokenLength = binaryReader.ReadByte();
                if (Revision < 2)
                {
                    var tokenBytes = binaryReader.ReadBytes(26);
                    LeftFmmToken = tokenBytes.ToStringOrDefault();
                }
                else
                {
                    var tokenBytes = binaryReader.ReadBytes(31);
                    LeftFmmToken = tokenBytes.ToStringOrDefault();
                    
                    var iv = binaryReader.ReadBytes(16);
                    LeftIv = Revision < 3 ? 
                        iv.ToBase64OrDefault() : 
                        MakeXor(iv).ToBase64OrDefault();
                }

                LeftSn = binaryReader.ReadBytes(11).ToStringOrDefault();

                if (Revision > 3)
                {
                    LeftLostmodeState = binaryReader.ReadByte().ToBooleanOrDefault();

                    var prefixLength = binaryReader.ReadByte();
                    var prefixBytes = binaryReader.ReadBytes(prefixLength);
                    LeftLostmodePrefix = prefixBytes.ToStringOrDefault();

                    var linkLength = binaryReader.ReadByte();
                    var linkBytes = binaryReader.ReadBytes(linkLength);
                    LeftLostmodeLink = linkBytes.ToStringOrDefault();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        if (rightConnected)
        {
            RightFindingSupport = binaryReader.ReadByte().ToBooleanOrDefault();
            RightE2E = binaryReader.ReadByte().ToBooleanOrDefault();

            var secretKeyBytes = binaryReader.ReadBytes(16);
            RightSecretKey = Revision < 3 ? 
                secretKeyBytes.ToBase64OrDefault() : 
                MakeXor(secretKeyBytes).ToBase64OrDefault();

            var maxN = binaryReader.ReadInt32();
            RightMaxN = maxN == 0 ? -1 : maxN;
            RightRegion = binaryReader.ReadByte();

            // Not sure about this
            var tokenLengthOrOffset = binaryReader.ReadByte();
            if (Revision < 2)
            {
                var tokenBytes = binaryReader.ReadBytes(26);
                RightFmmToken = tokenBytes.ToStringOrDefault();
            }
            else
            {
                var tokenBytes = binaryReader.ReadBytes(31);
                RightFmmToken = tokenBytes.ToStringOrDefault();
    
                var iv = binaryReader.ReadBytes(16);
                RightIv = Revision < 3 ? 
                    iv.ToBase64OrDefault() : 
                    MakeXor(iv).ToBase64OrDefault();
            }

            RightSn = binaryReader.ReadBytes(11).ToStringOrDefault();

            if (Revision > 3)
            {
                RightLostmodeState = binaryReader.ReadByte().ToBooleanOrDefault();

                var prefixLength = binaryReader.ReadByte();
                var prefixBytes = binaryReader.ReadBytes(prefixLength);
                RightLostmodePrefix = prefixBytes.ToStringOrDefault();

                var linkLength = binaryReader.ReadByte();
                var linkBytes = binaryReader.ReadBytes(linkLength);
                RightLostmodeLink = linkBytes.ToStringOrDefault();
            }
        }
    }

    private static byte[] MakeXor(IReadOnlyList<byte> input)
    {
        var output = new byte[input.Count];
        for (var i = 0; i < input.Count; i++)
        {
            output[i] = (byte)(input[i] ^ 0xFD);
        }
        return output;
    }
}

file static class GetFmmConfigDecoderExtensions
{
    internal static bool? ToBooleanOrDefault(this byte b)
    {
        return (b != 0xFF) ? b == 1 : null;
    }
    
    internal static string? ToBase64OrDefault(this IEnumerable<byte> bytes)
    {
        var enumerated = bytes.ToArray();
        return enumerated.Any(b => b != 0xFF) ? Convert.ToBase64String(enumerated) : null;
    }   
    
    internal static string? ToStringOrDefault(this IEnumerable<byte> bytes)
    {
        var enumerated = bytes.ToArray();
        return enumerated.Any(b => b != 0xFF) ? Encoding.UTF8.GetString(enumerated) : null;
    }
}