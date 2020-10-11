using System;
using System.IO;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.util;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.message
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
        public int CRC16 { private set; get; }

        public SPPMessage()
        {
        }

        public SPPMessage(MessageIds id, MsgType type, byte[] payload)
        {
            Id = id;
            Type = type;
            Payload = payload;
        }

        public BaseMessageParser BuildParser()
        {
            return SPPMessageParserFactory.BuildParser(this);
        }

        public byte[] EncodeMessage(MsgType overrideType = MsgType.INVALID)
        {

            MsgType type;
            if (overrideType == MsgType.INVALID)
                type = Type;
            else
                type = overrideType;

            byte[] msg = new byte[TotalPacketSize];
            

            if (BluetoothService.Instance.ActiveModel == Model.BudsPlus || BluetoothService.Instance.ActiveModel == Model.BudsLive)
            {
                msg[0] = (byte)Constants.SOMPlus;
                msg[1] = (byte) this.Size;
                msg[2] = (byte) (type == MsgType.Request ? 0 : 16);
            }
            else
            { 
                msg[0] = (byte) Constants.SOM;
                msg[1] = (byte) type;
                msg[2] = (byte) this.Size;
            }

            msg[3] = (byte) this.Id;

            Array.Copy(Payload, 0, msg, 4, Payload.Length);

            byte[] crcData = new byte[this.Size - 2];
            crcData[0] = msg[3];
            Array.Copy(Payload, 0, crcData, 1, Payload.Length);
            int crc16 = util.CRC16.crc16_ccitt(crcData);
            msg[4 + Payload.Length] = (byte)(crc16 & 255);
            msg[4 + Payload.Length + 1] = (byte) ((crc16 >> 8) & 255);

            if (BluetoothService.Instance.ActiveModel == Model.BudsPlus || BluetoothService.Instance.ActiveModel == Model.BudsLive)
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
            SPPMessage draft = new SPPMessage();

            if(raw.Length < 6)
            {
                throw new InvalidDataException(Loc.GetString("sppmsg_too_small"));
            }

            if(raw[0] != (byte)Constants.SOM && BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                throw new InvalidDataException(Loc.GetString("sppmsg_invalid_som"));
            }
            else if (raw[0] != (byte)Constants.SOMPlus && (BluetoothService.Instance.ActiveModel == Model.BudsPlus || BluetoothService.Instance.ActiveModel == Model.BudsLive))
            {
                throw new InvalidDataException(Loc.GetString("sppmsg_invalid_som"));
            }

            draft.Id = (MessageIds)Convert.ToInt32(raw[3]);
            int size;

            if (BluetoothService.Instance.ActiveModel == Model.BudsPlus || BluetoothService.Instance.ActiveModel == Model.BudsLive)
            {
                size = raw[1] & 1023;
                draft.Type = (raw[2] & 16) == 0 ? MsgType.Request : MsgType.Response;
            }
            else
            {
                draft.Type = (MsgType) Convert.ToInt32(raw[1]);
                size = Convert.ToInt32(raw[2]);
            }

            //Substract Id and CRC from size
            int rawPayloadSize = size - 3;
            byte[] payload = new byte[rawPayloadSize];
            
            byte[] crcData = new byte[size];
            crcData[0] = raw[3]; //Msg ID

            for(int i = 0; i < rawPayloadSize; i++)
            {
                //Start to read at byte 4
                payload[i] = raw[i + 4];
                crcData[i + 1] = raw[i + 4];
            }

            byte crc1 = raw[4 + rawPayloadSize];
            byte crc2 = raw[4 + rawPayloadSize + 1];
            crcData[crcData.Length - 2] = crc2;
            crcData[crcData.Length - 1] = crc1;

            draft.Payload = payload;
            draft.CRC16 = util.CRC16.crc16_ccitt(crcData);

            if (size != draft.Size)
            {
                throw new InvalidDataException(Loc.GetString("sppmsg_size_mismatch"));
            }

            if (raw[4 + rawPayloadSize + 2] != (byte)Constants.EOM && BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                throw new InvalidDataException(Loc.GetString("sppmsg_invalid_eom"));
            }
            else if (raw[4 + rawPayloadSize + 2] != (byte) Constants.EOMPlus &&
                     (BluetoothService.Instance.ActiveModel == Model.BudsPlus ||
                      BluetoothService.Instance.ActiveModel == Model.BudsLive))
            {
                throw new InvalidDataException(Loc.GetString("sppmsg_invalid_eom"));
            }

            return draft;
        }

    }
}
