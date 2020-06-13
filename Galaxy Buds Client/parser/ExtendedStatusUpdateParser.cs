using System;
using System.Collections.Generic;
using System.Reflection;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;

namespace Galaxy_Buds_Client.parser
{
    class ExtendedStatusUpdateParser : BaseMessageParser
    {

        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_EXTENDED_STATUS_UPDATED;

        public int VersionOfMR { set; get; }
        public int EarType { set; get; }
        public int BatteryL { set; get; }
        public int BatteryR { set; get; }
        public bool IsCoupled { set; get; }
        public Constants.DeviceInv MainConnection { set; get; }
        public Constants.WearStates WearState { set; get; }
        public bool AmbientSoundEnabled { set; get; }
        public Constants.AmbientType AmbientSoundMode { set; get; }
        public int AmbientSoundVolume { set; get; }
        public bool EqualizerEnabled { set; get; }
        public int EqualizerMode { set; get; }
        public bool TouchpadLock { set; get; }
        public Constants.TouchOption TouchpadOptionL { set; get; }
        public Constants.TouchOption TouchpadOptionR { set; get; }
        public bool SeamlessConnectionEnabled { set; get; }
        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            VersionOfMR = msg.Payload[0];
            EarType = msg.Payload[1];
            BatteryL = msg.Payload[2];
            BatteryR = msg.Payload[3];
            IsCoupled = Convert.ToBoolean(msg.Payload[4]);
            MainConnection = (Constants.DeviceInv) msg.Payload[5];
            WearState = (Constants.WearStates) msg.Payload[6];
            AmbientSoundEnabled = Convert.ToBoolean(msg.Payload[7]);
            AmbientSoundMode = (Constants.AmbientType) msg.Payload[8];
            AmbientSoundVolume = msg.Payload[9];
            EqualizerEnabled = Convert.ToBoolean(msg.Payload[10]);
            EqualizerMode = msg.Payload[11];

            if (msg.Size > 13)
            {
                TouchpadLock = Convert.ToBoolean(msg.Payload[12]);
                TouchpadOptionL = (Constants.TouchOption) ((msg.Payload[13] & 0xF0) >> 4);
                TouchpadOptionR = (Constants.TouchOption) (msg.Payload[13] & 0x0F);
                if (VersionOfMR >= 3)
                {
                    SeamlessConnectionEnabled = msg.Payload[14] == 0;
                }
            }
            else
            {
                TouchpadLock = Convert.ToBoolean((msg.Payload[12] & 0xF0) >> 4);
                TouchpadOptionL = (Constants.TouchOption)(msg.Payload[12] & 0x0F);
                TouchpadOptionR = (Constants.TouchOption)(msg.Payload[12] & 0x0F);
                if (VersionOfMR >= 3)
                {
                    SeamlessConnectionEnabled = msg.Payload[13] == 0;
                }
            }
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType")
                    continue;

                map.Add(property.Name, property.GetValue(this).ToString());
            }

            return map;
        }
    }
}
