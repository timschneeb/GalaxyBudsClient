using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GalaxyBudsClient.Generated.Model.Attributes;
using Serilog;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.SET_FMM_CONFIG)]
public class SetFmmConfigEncoder : BaseMessageEncoder
{
    public byte Revision { init; get; }
    public bool UpdateLeft { init; get; }
    public bool UpdateRight { init; get; }
    
    public bool? LeftFindingSupport { init; get; }
    public bool? LeftE2E { init; get; }
    public string? LeftSecretKey { init; get; }
    public int LeftMaxN { init; get; } = -1;
    public byte LeftRegion { init; get; } = 0xFF;
    public string? LeftFmmToken { init; get; }
    public string? LeftIv { init; get; }
    public bool? LeftLostmodeState { init; get; }
    public string? LeftLostmodePrefix { init; get; }
    public string? LeftLostmodeLink { init; get; }

    public bool? RightFindingSupport { init; get; }
    public bool? RightE2E { init; get; }
    public string? RightSecretKey { init; get; }
    public int RightMaxN { init; get; } = -1;
    public byte RightRegion { init; get; } = 0xFF;
    public string? RightFmmToken { init; get; }
    public string? RightIv { init; get; }
    public bool? RightLostmodeState { init; get; }
    public string? RightLostmodePrefix { init; get; }
    public string? RightLostmodeLink { init; get; }
    
    private static byte[] MakeXor(IReadOnlyList<byte> input)
    {
        var output = new byte[input.Count];
        for (var i = 0; i < input.Count; i++)
        {
            output[i] = (byte)(input[i] ^ 0xFD);
        }
        return output;
    }

    private static byte ToByte(bool? b) => b == null ? (byte)0xFF : (byte)(b == true ? 1 : 0);
    
    public override SppMessage Encode()
    {
        using var stream = new MemoryStream();
        using var buffer = new BinaryWriter(stream);
        
        buffer.Write(Revision);
        buffer.Write((byte)((Convert.ToInt32(UpdateLeft) << 4) | Convert.ToInt32(UpdateRight)));
        
        if (UpdateLeft) {
            try {
                buffer.Write(ToByte(LeftFindingSupport));
                buffer.Write(ToByte(LeftE2E));
                
                var secretKey = new byte[16];
                if(string.IsNullOrEmpty(LeftSecretKey))
                    Array.Fill(secretKey, (byte)0xFF);
                else
                    secretKey = Convert.FromBase64String(LeftSecretKey);
                buffer.Write(Revision < 3 ? secretKey : MakeXor(secretKey));
                
                buffer.Write(LeftMaxN != -1 ? LeftMaxN : 0);
                buffer.Write((byte) (LeftRegion != 0xFF ? LeftRegion : 0xFF));

                var token = Enumerable.Repeat((byte)0xFF, 31).ToArray();
                if(!string.IsNullOrEmpty(LeftFmmToken))
                {
                    var raw = Convert.FromBase64String(LeftFmmToken);
                    Array.Copy(raw, token, Math.Min(raw.Length, 31));
                }
                   
                if (Revision < 2)
                {
                    var length = (byte)Math.Min(token.Length, 26);
                    buffer.Write(length);
                    buffer.Write(token.Take(length).ToArray());
                } else {
                    var length = !string.IsNullOrEmpty(LeftFmmToken) ? Math.Min(token.Length, 31) : 0xFF;
                    buffer.Write((byte) length);
                    buffer.Write(token.Take(Math.Min(token.Length, 31)).ToArray());
                    
                    var ivBytes = new byte[16];
                    if (!string.IsNullOrEmpty(LeftIv))
                    {
                        ivBytes = Convert.FromBase64String(LeftIv);
                    } else
                    {
                        Array.Fill(ivBytes, (byte)0xFF);
                    }
                    
                    if (Revision < 3) {
                        buffer.Write(ivBytes);
                    }
                    else
                    {
                        buffer.Write(MakeXor(ivBytes));
                        
                        if (Revision > 3) {
                            buffer.Write(ToByte(LeftLostmodeState));
                            
                            var lostModePrefix = new byte[42];
                            if(string.IsNullOrEmpty(LeftLostmodePrefix))
                                Array.Fill(lostModePrefix, (byte)0xFF);
                            else
                                lostModePrefix = Convert.FromBase64String(LeftLostmodePrefix);
                            
                            var lostPrefixLength = !string.IsNullOrEmpty(LeftLostmodePrefix) ? Math.Min(lostModePrefix.Length, 42) : 0xFF;
                            buffer.Write((byte) lostPrefixLength);
                            buffer.Write(lostModePrefix.Take(Math.Min(lostModePrefix.Length, 42)).ToArray());
         
                            var lostModeLink = new byte[120];
                            if(string.IsNullOrEmpty(LeftLostmodeLink))
                                Array.Fill(lostModeLink, (byte)0xFF);
                            else
                                lostModeLink = Convert.FromBase64String(LeftLostmodeLink);
                            
                            var lostLinkLength = !string.IsNullOrEmpty(LeftLostmodeLink) ? Math.Min(lostModeLink.Length, 120) : 0xFF;
                            buffer.Write((byte) lostLinkLength);
                            buffer.Write(lostModeLink.Take(Math.Min(lostModeLink.Length, 120)).ToArray());
                        }
                    }
                }
            } catch (Exception e) {
                Log.Error(e, "Failed to serialize SET_FMM_CONFIG for left earbud");
            }
        }
        
        if (UpdateRight) {
            buffer.Write(ToByte(RightFindingSupport));
            buffer.Write(ToByte(RightE2E));
                
            var secretKey = new byte[16];
            if(string.IsNullOrEmpty(RightSecretKey))
                Array.Fill(secretKey, (byte)0xFF);
            else
                secretKey = Convert.FromBase64String(RightSecretKey);
            buffer.Write(Revision < 3 ? secretKey : MakeXor(secretKey));
                
            buffer.Write(RightMaxN != -1 ? RightMaxN : 0);
            buffer.Write((byte) (RightRegion != 0xFF ? RightRegion : 0xFF));

            var token = Enumerable.Repeat((byte)0xFF, 31).ToArray();
            if(!string.IsNullOrEmpty(RightFmmToken))
            {
                var raw = Convert.FromBase64String(RightFmmToken);
                Array.Copy(raw, token, Math.Min(raw.Length, 31));
            }
                   
            if (Revision < 2)
            {
                var length = (byte)Math.Min(token.Length, 26);
                buffer.Write(length);
                buffer.Write(token.Take(length).ToArray());
            } else {
                var length = !string.IsNullOrEmpty(RightFmmToken) ? Math.Min(token.Length, 31) : 0xFF;
                buffer.Write((byte) length);
                buffer.Write(token.Take(Math.Min(token.Length, 31)).ToArray());
                    
                var ivBytes = new byte[16];
                if (!string.IsNullOrEmpty(RightIv))
                {
                    ivBytes = Convert.FromBase64String(RightIv);
                } else
                {
                    Array.Fill(ivBytes, (byte)0xFF);
                }
                    
                if (Revision < 3) {
                    buffer.Write(ivBytes);
                }
                else
                {
                    buffer.Write(MakeXor(ivBytes));
                        
                    if (Revision > 3) {
                        buffer.Write(ToByte(RightLostmodeState));
                            
                        var lostModePrefix = new byte[42];
                        if(string.IsNullOrEmpty(RightLostmodePrefix))
                            Array.Fill(lostModePrefix, (byte)0xFF);
                        else
                            lostModePrefix = Convert.FromBase64String(RightLostmodePrefix);
                            
                        var lostPrefixLength = !string.IsNullOrEmpty(RightLostmodePrefix) ? Math.Min(lostModePrefix.Length, 42) : 0xFF;
                        buffer.Write((byte) lostPrefixLength);
                        buffer.Write(lostModePrefix.Take(Math.Min(lostModePrefix.Length, 42)).ToArray());
         
                        var lostModeLink = new byte[120];
                        if(string.IsNullOrEmpty(RightLostmodeLink))
                            Array.Fill(lostModeLink, (byte)0xFF);
                        else
                            lostModeLink = Convert.FromBase64String(RightLostmodeLink);
                            
                        var lostLinkLength = !string.IsNullOrEmpty(RightLostmodeLink) ? Math.Min(lostModeLink.Length, 120) : 0xFF;
                        buffer.Write((byte) lostLinkLength);
                        buffer.Write(lostModeLink.Take(Math.Min(lostModeLink.Length, 120)).ToArray());
                    }
                }
            }
        }

        return new SppMessage(MsgIds.SET_FMM_CONFIG, payload: stream.ToArray(), model: TargetModel);
    }
}