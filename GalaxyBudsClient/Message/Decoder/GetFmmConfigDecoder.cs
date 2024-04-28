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
    public byte Revision { private set; get; }
    
    public bool? LeftFindingSupport { private set; get; }
    public bool? LeftE2E { private set; get; }
    public string? LeftSecretKey { private set; get; }
    public int LeftMaxN { private set; get; }
    public byte LeftRegion { private set; get; }
    public string? LeftFmmToken { private set; get; }
    public string? LeftIv { private set; get; }
    public string? LeftSn { private set; get; }
    public bool? LeftLostmodeState { private set; get; }
    public string? LeftLostmodePrefix { private set; get; }
    public string? LeftLostmodeLink { private set; get; }

    public bool? RightFindingSupport { private set; get; }
    public bool? RightE2E { private set; get; }
    public string? RightSecretKey { private set; get; }
    public int RightMaxN { private set; get; }
    public byte RightRegion { private set; get; }
    public string? RightFmmToken { private set; get; }
    public string? RightIv { private set; get; }
    public string? RightSn { private set; get; }
    public bool? RightLostmodeState { private set; get; }
    public string? RightLostmodePrefix { private set; get; }
    public string? RightLostmodeLink { private set; get; }
    
    // TODO Add UI to display & erase FMM data?
    public override void Decode(SppMessage msg)
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
                    LeftFmmToken = Encoding.UTF8.GetString(tokenBytes, 0, Math.Min((int)tokenLength, 26));
                }
                else
                {
                    var tokenBytes = binaryReader.ReadBytes(31);
                    LeftFmmToken = Encoding.UTF8.GetString(tokenBytes, 0, Math.Min((int)tokenLength, 31));
                    
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
                    LeftLostmodePrefix = Encoding.UTF8.GetString(prefixBytes, 0, Math.Min(prefixBytes.Length, 42));

                    var linkLength = binaryReader.ReadByte();
                    var linkBytes = binaryReader.ReadBytes(linkLength);
                    LeftLostmodeLink = Encoding.UTF8.GetString(linkBytes, 0, Math.Min(linkBytes.Length, 120));
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
                RightFmmToken = Encoding.UTF8.GetString(tokenBytes);
            }
            else
            {
                var tokenBytes = binaryReader.ReadBytes(31);
                RightFmmToken = Encoding.UTF8.GetString(tokenBytes);
    
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
                RightLostmodePrefix = Encoding.UTF8.GetString(prefixBytes, 0, Math.Min(prefixBytes.Length, 42));

                var linkLength = binaryReader.ReadByte();
                var linkBytes = binaryReader.ReadBytes(linkLength);
                RightLostmodeLink = Encoding.UTF8.GetString(linkBytes, 0, Math.Min(linkBytes.Length, 120));
            }
                
        }
    }

    private static byte[] Decode(string str)
    {
        byte[] decode;
        decode = Convert.FromBase64String(str);
        Console.WriteLine($"decode size : {decode.Length}");
        Console.WriteLine($"decode data : {Encoding.UTF8.GetString(decode)}");
        return decode;
    }

    private static string Encode(byte[] bArr)
    {
        var encodeToString = Convert.ToBase64String(bArr);
        Console.WriteLine($"encode data : {encodeToString}");
        return encodeToString;
    }

    private static bool IsSupportedValue(IEnumerable<byte> bArr) => bArr.Any(b => b != 0xFF);

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