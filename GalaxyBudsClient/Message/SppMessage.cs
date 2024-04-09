using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Sentry;
using Serilog;

namespace GalaxyBudsClient.Message;

public partial class SppMessage(
    MsgIds id = MsgIds.UNKNOWN_0, 
    MsgTypes type = MsgTypes.Request,
    byte[]? payload = null, 
    Models? model = null)
{
    private const int CrcSize = 2;
    private const int MsgIdSize = 1;
    private const int SomSize = 1;
    private const int EomSize = 1;
    private const int TypeSize = 1;
    private const int BytesSize = 1;

    public MsgTypes Type { set; get; } = type;
    public MsgIds Id { set; get; } = id;
    public int Size => MsgIdSize + Payload.Length + CrcSize;
    public int TotalPacketSize => SomSize + TypeSize + BytesSize + MsgIdSize + Payload.Length + CrcSize + EomSize;
    public byte[] Payload { set; get; } = payload ?? [];
    public int Crc16 { private set; get; }
        
    /* No Buds support at the moment */
    public bool IsFragment { set; get; }
    
    private Models TargetModel => model ?? BluetoothImpl.ActiveModel;

    public static BaseMessageEncoder? CreateEncoder(MsgIds msgId) => CreateUninitializedEncoder(msgId);
    
    public BaseMessageDecoder? CreateDecoder()
    {
        var decoder = CreateUninitializedDecoder(Id);
        if (decoder == null) 
            return null;
        
        decoder.TargetModel = TargetModel;
                
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("msg-data-available", "true");
            scope.SetExtra("msg-type", Type.ToString());
            scope.SetExtra("msg-id", Id);
            scope.SetExtra("msg-size", Size);
            scope.SetExtra("msg-total-size", TotalPacketSize);
            scope.SetExtra("msg-crc16", Crc16);
            scope.SetExtra("msg-payload", HexUtils.Dump(Payload, 512, false, false, false));
        });

        decoder.Decode(this);
            
        foreach (var hook in ScriptManager.Instance.DecoderHooks)
        {
            hook.OnDecoderCreated(this, ref decoder);
        }
        return decoder;
    }

    public byte[] Encode()
    {
        var spec = DeviceSpecHelper.FindByModel(TargetModel) ?? throw new InvalidOperationException();

        using var stream = new MemoryStream(TotalPacketSize);
        using var writer = new BinaryWriter(stream);
        writer.Write(spec.StartOfMessage);
            
        if (spec.Supports(Features.SppLegacyMessageHeader))
        {
            writer.Write((byte)Type);
            writer.Write((byte)Size);
        }
        else
        {
            /* Generate header */
            var header = BitConverter.GetBytes((short)Size);
            Debug.Assert(header.Length == 2);
            
            if (IsFragment) {
                header[1] = (byte) (header[1] | 32);
            }
            if (Type == MsgTypes.Response) {
                header[1] = (byte) (header[1] | 16);
            }
            
            writer.Write(header);
        }

        writer.Write((byte)Id);
        writer.Write(Payload);
        writer.Write(Utils.Crc16.crc16_ccitt(Id, Payload));
        writer.Write(spec.EndOfMessage);
        
        return stream.ToArray();
    }

    /**
      * Static "constructors"
      */
    public static SppMessage Decode(byte[] raw, Models model)
    {
        try
        {
            var spec = DeviceSpecHelper.FindByModel(model) ?? throw new InvalidOperationException();
            var draft = new SppMessage(model: model);

            using var stream = new MemoryStream(raw);
            using var reader = new BinaryReader(stream);
            
            if (raw.Length < 6)
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.TooSmall, "At least 6 bytes are required");
            
            if (reader.ReadByte() != spec.StartOfMessage)
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Som, "Invalid SOM byte");

            int size;
            if (spec.Supports(Features.SppLegacyMessageHeader))
            {
                draft.Type = (MsgTypes) Convert.ToInt32(reader.ReadByte());
                size = Convert.ToInt32(reader.ReadByte());
            }
            else
            {
                var header = reader.ReadInt16();
                draft.IsFragment = (header & 0x2000) != 0;
                draft.Type = (header & 0x1000) != 0 ? MsgTypes.Request : MsgTypes.Response;
                size = header & 0x3FF;
            }

            draft.Id = (MsgIds)reader.ReadByte();
            
            // Subtract Id and CRC from size
            var payloadSize = size - 3;
            if (payloadSize < 0)
            {
                payloadSize = 0;
                size = 3;
            }
            
            var payload = new byte[payloadSize];
            var crcData = new byte[size];
            crcData[0] = (byte)draft.Id;
            
            for (var i = 0; i < payloadSize; i++)
            {
                payload[i] = crcData[i + 1] = reader.ReadByte();
            }

            var crc1 = reader.ReadByte();
            var crc2 = reader.ReadByte();
            crcData[^2] = crc2;
            crcData[^1] = crc1;

            draft.Payload = payload;
            draft.Crc16 = Utils.Crc16.crc16_ccitt(crcData);

            if (size != draft.Size)
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.SizeMismatch, "Invalid size");
            if (draft.Crc16 != 0)
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Checksum, "Invalid checksum");
            if (reader.ReadByte() != spec.EndOfMessage)
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Eom, "Invalid EOM byte");

            return draft;
        }
        catch (IndexOutOfRangeException)
        {
            throw new InvalidPacketException(InvalidPacketException.ErrorCodes.OutOfRange,"Index was out of range");
        }
        catch (OverflowException)
        {
            throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Overflow,"Overflow. Update your firmware!");
        }
    }
    public static IEnumerable<SppMessage> DecodeRawChunk(List<byte> incomingData, Models model)
    {
        var spec = DeviceSpecHelper.FindByModel(model) ?? throw new InvalidOperationException();
        var messages = new List<SppMessage>();
        var failCount = 0;
        
        do
        {
            int msgSize;
            var raw = incomingData.ToArray();

            try
            {
                foreach (var hook in ScriptManager.Instance.RawStreamHooks)
                {
                    hook.OnRawDataAvailable(ref raw);
                }

                var msg = Decode(raw, model);
                msgSize = msg.TotalPacketSize;

                Log.Verbose(">> Incoming: {Msg}", msg);
                    
                foreach (var hook in ScriptManager.Instance.MessageHooks)
                {
                    hook.OnMessageAvailable(ref msg);
                }

                messages.Add(msg);
            }
            catch (InvalidPacketException e)
            {
                SentrySdk.AddBreadcrumb($"{e.ErrorCode}: {e.Message}", "spp", level: BreadcrumbLevel.Warning);
                Log.Error("{Code}: {Msg}", e.ErrorCode, e.Message);
                if (e.ErrorCode is InvalidPacketException.ErrorCodes.Overflow
                    or InvalidPacketException.ErrorCodes.OutOfRange)
                {
                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.SetTag("raw-data-available", "true");
                        scope.SetExtra("raw-data", HexUtils.Dump(raw, 512, false, false, false));
                    });
                    SentrySdk.CaptureException(e);
                }
                    
                // Attempt to remove broken message, otherwise skip data block
                var somIndex = 0;
                for (var i = 1; i < incomingData.Count; i++)
                {
                    if (incomingData[i] == spec.StartOfMessage)
                    {
                        somIndex = i;
                        break;
                    }
                }

                msgSize = somIndex;
                    
                if (failCount > 5)
                {
                    // Abandon data block
                    throw;
                }
                    
                failCount++;
            }

            if (msgSize >= incomingData.Count)
            {
                incomingData.Clear();
                break;
            }

            incomingData.RemoveRange(0, msgSize);

            if (ByteArrayUtils.IsBufferZeroedOut(incomingData))
            {
                /* No more data remaining */
                break;
            }

        } while (incomingData.Count > 0);

        return messages;
    }

    public override string ToString()
    {
        return $"SPPMessage[MessageID={Id}, PayloadSize={Size}, Type={(IsFragment ? "Fragment/" : string.Empty) + Type}, " +
               $"CRC16={Crc16}, Payload={{{BitConverter.ToString(Payload).Replace("-", " ")}}}]";
    }
}