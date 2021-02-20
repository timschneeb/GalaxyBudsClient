using System;
using System.IO;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Sentry;
using Sentry.Protocol;
using Serilog;

namespace GalaxyBudsClient.Message
{
    public partial class SPPMessage
    {

        private readonly int crcSize = 2;
        private readonly int msgIdSize = 1;
        private readonly int somSize = 1;
        private readonly int eomSize = 1;
        private readonly int typeSize = 1;
        private readonly int bytesSize = 1;

        public MsgType Type { set; get; }
        public MessageIds Id { set; get; }
        public int Size => msgIdSize + Payload.Length + crcSize;
        public int TotalPacketSize => somSize + typeSize + bytesSize + msgIdSize + Payload.Length + crcSize + eomSize;
        public byte[] Payload { set; get; }
        public int Crc16 { private set; get; }
        
        /* No Buds support at the moment */
        public bool IsFragment { set; get; }

        public SPPMessage()
        {
            Payload = new byte[0];
        }

        public SPPMessage(MessageIds id, MsgType type, byte[] payload)
        {
            Id = id;
            Type = type;
            Payload = payload;
        }

        public BaseMessageParser? BuildParser()
        {
            return SPPMessageParserFactory.BuildParser(this);
        }

        public byte[] EncodeMessage()
        {
            byte[] msg = new byte[TotalPacketSize];
            
            if (BluetoothImpl.Instance.ActiveModel != Models.Buds)
            {
                msg[0] = (byte)Constants.SOMPlus;
                
                /* Generate header */
                var header = (BitConverter.GetBytes((short)Size));
                if (IsFragment) {
                    header[1] = (byte) (header[1] | 32);
                }
                if (Type == MsgType.Response) {
                    header[1] = (byte) (header[1] | 16);
                }

                msg[1] = header[0];
                msg[2] = header[1];
            }
            else
            {
                msg[0] = (byte)Constants.SOM;
                msg[1] = (byte)Type;
                msg[2] = (byte)Size;
            }

            msg[3] = (byte)Id;

            Array.Copy(Payload, 0, msg, 4, Payload.Length);

            byte[] crcData = new byte[Size - 2];
            crcData[0] = msg[3];
            Array.Copy(Payload, 0, crcData, 1, Payload.Length);
            int crc16 = Utils.CRC16.crc16_ccitt(crcData);
            msg[4 + Payload.Length] = (byte)(crc16 & 255);
            msg[4 + Payload.Length + 1] = (byte)((crc16 >> 8) & 255);

            if (BluetoothImpl.Instance.ActiveModel != Models.Buds)
            {
                msg[TotalPacketSize - 1] = (byte)Constants.EOMPlus;
            }
            else
            {
                msg[TotalPacketSize - 1] = (byte)Constants.EOM;
            }

            return msg;
        }

        /**
          * Static "constructors" 
          */
        public static SPPMessage DecodeMessage(byte[] raw)
        {
            try
            {
                SPPMessage draft = new SPPMessage();

                if (raw.Length < 6)
                {
                    SentrySdk.AddBreadcrumb($"Message too small (Length: {raw.Length})", "spp",
                        level: BreadcrumbLevel.Warning);
                    Log.Error($"Message too small (Length: {raw.Length})");
                    throw new InvalidPacketException(InvalidPacketException.ErrorCodes.TooSmall,Loc.Resolve("sppmsg_too_small"));
                }
                
                if ((raw[0] != (byte) Constants.SOM &&
                     BluetoothImpl.Instance.ActiveModel == Models.Buds) ||
                    (raw[0] != (byte) Constants.SOMPlus &&
                     BluetoothImpl.Instance.ActiveModel != Models.Buds))
                {
                    SentrySdk.AddBreadcrumb($"Invalid SOM (Received: {raw[0]})", "spp",
                        level: BreadcrumbLevel.Warning);
                    Log.Error($"Invalid SOM (Received: {raw[0]})");
                    throw new InvalidPacketException(InvalidPacketException.ErrorCodes.SOM,Loc.Resolve("sppmsg_invalid_som"));
                }

                draft.Id = (MessageIds) Convert.ToInt32(raw[3]);
                int size;

                if (BluetoothImpl.Instance.ActiveModel != Models.Buds)
                {
                    var p1 = (raw[2] << 8);
                    var p2 = raw[1] & 255;
                    var header = p1 + p2;
                    draft.IsFragment = (header & 8192) != 0;
                    draft.Type = (header & 4096) != 0 ? MsgType.Request : MsgType.Response;
                    size = header & 1023;
                }
                else
                {
                    draft.Type = (MsgType) Convert.ToInt32(raw[1]);
                    size = Convert.ToInt32(raw[2]);
                }

                //Subtract Id and CRC from size
                var rawPayloadSize = size - 3;
                byte[] payload = new byte[rawPayloadSize];

                byte[] crcData = new byte[size];
                crcData[0] = raw[3]; //Msg ID

                for (var i = 0; i < rawPayloadSize; i++)
                {
                    //Start to read at byte 4
                    payload[i] = raw[i + 4];
                    crcData[i + 1] = raw[i + 4];
                }

                var crc1 = raw[4 + rawPayloadSize];
                var crc2 = raw[4 + rawPayloadSize + 1];
                crcData[^2] = crc2;
                crcData[^1] = crc1;

                draft.Payload = payload;
                draft.Crc16 = Utils.CRC16.crc16_ccitt(crcData);

                if (size != draft.Size)
                {
                    SentrySdk.AddBreadcrumb($"Invalid size (Reported: {size}, Calculated: {draft.Size})", "spp",
                        level: BreadcrumbLevel.Warning);
                    Log.Error($"Invalid size (Reported: {size}, Calculated: {draft.Size})");
                    throw new InvalidPacketException(InvalidPacketException.ErrorCodes.SizeMismatch,Loc.Resolve("sppmsg_size_mismatch"));
                }

                if (draft.Crc16 != 0)
                {
                    SentrySdk.AddBreadcrumb($"CRC checksum failed (ID: {draft.Id}, Size: {draft.Size})", "spp",
                        level: BreadcrumbLevel.Warning);
                    Log.Error($"CRC checksum failed (ID: {draft.Id}, Size: {draft.Size})");
                    //throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Checksum,Loc.Resolve("sppmsg_crc_fail"));
                }

                if (raw[draft.TotalPacketSize - 1] != (byte) Constants.EOM &&
                    BluetoothImpl.Instance.ActiveModel == Models.Buds)
                {
                    SentrySdk.AddBreadcrumb($"Invalid EOM (Received: {raw[4 + rawPayloadSize + 2]})", "spp",
                        level: BreadcrumbLevel.Warning);
                    Log.Error($"Invalid EOM (Received: {raw[4 + rawPayloadSize + 2]}");
                    throw new InvalidPacketException(InvalidPacketException.ErrorCodes.SOM,Loc.Resolve("sppmsg_invalid_eom"));
                }

                if (raw[draft.TotalPacketSize - 1] != (byte) Constants.EOMPlus &&
                    BluetoothImpl.Instance.ActiveModel != Models.Buds)
                {
                    SentrySdk.AddBreadcrumb($"Invalid EOM (Received: {raw[4 + rawPayloadSize + 2]})", "spp",
                        level: BreadcrumbLevel.Warning);
                    Log.Error($"Invalid EOM (Received: {raw[4 + rawPayloadSize + 2]}");
                    throw new InvalidPacketException(InvalidPacketException.ErrorCodes.EOM,Loc.Resolve("sppmsg_invalid_eom"));
                }

                return draft;
            }
            catch (IndexOutOfRangeException)
            {
                SentrySdk.AddBreadcrumb("IndexOutOfRangeException");
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetTag("raw-data-available", "true");
                    scope.SetExtra("raw-data", HexUtils.Dump(raw, 512, false, false, false));
                });
                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.OutOfRange,"OutOfRange. Update your firmware!");
            }
            catch (OverflowException ex)
            {
                SentrySdk.AddBreadcrumb("OverflowException");
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.SetTag("raw-data-available", "true");
                    scope.SetExtra("raw-data", HexUtils.Dump(raw, 512, false, false, false));
                });
                SentrySdk.CaptureException(ex);

                throw new InvalidPacketException(InvalidPacketException.ErrorCodes.Overflow,"Overflow. Update your firmware!");
            }
        }

        public override string ToString()
        {
            return $"SPPMessage[MessageID={Id},PayloadSize={Size},Type={(IsFragment ? "Fragment/" : string.Empty) + Type},CRC16={Crc16}," +
                   $"Payload={{{BitConverter.ToString(Payload).Replace("-", " ")}}}]";
        }

    }
}
