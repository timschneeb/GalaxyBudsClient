using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;

namespace Galaxy_Buds_Client.parser
{
    class ExtendedStatusUpdateParser : BaseMessageParser
    {

        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.MSG_ID_EXTENDED_STATUS_UPDATED;

        public int Revision { set; get; }
        public int EarType { set; get; }
        public int BatteryL { set; get; }
        public int BatteryR { set; get; }
        public bool IsCoupled { set; get; }
        public DeviceInv MainConnection { set; get; }
        public WearStates WearState { set; get; }
        public int EqualizerMode { set; get; }
        public bool TouchpadLock { set; get; }
        public TouchOption.Universal TouchpadOptionL { set; get; }
        public TouchOption.Universal TouchpadOptionR { set; get; }
        public bool SeamlessConnectionEnabled { set; get; }


        [Device(new Model[] { Model.Buds, Model.BudsPlus })]
        public bool AmbientSoundEnabled { set; get; }
        [Device(new Model[] { Model.Buds, Model.BudsPlus })]
        public int AmbientSoundVolume { set; get; }


        [Device(Model.Buds)]
        public AmbientType AmbientSoundMode { set; get; }
        [Device(Model.Buds)]
        public bool EqualizerEnabled { set; get; }


        [Device(new Model[] { Model.BudsPlus, Model.BudsLive })]
        public PlacementStates PlacementL { set; get; }
        [Device(new Model[] { Model.BudsPlus, Model.BudsLive })]
        public PlacementStates PlacementR { set; get; }
        [Device(new Model[] { Model.BudsPlus, Model.BudsLive })]
        public int BatteryCase { set; get; }
        [Device(new Model[] { Model.BudsPlus, Model.BudsLive })]
        public bool OutsideDoubleTap { set; get; }
        [Device(new Model[] { Model.BudsPlus, Model.BudsLive })]
        public Color DeviceColor { set; get; }


        [Device(Model.BudsPlus)]
        public bool AdjustSoundSync { set; get; }
        [Device(Model.BudsPlus)]
        public bool SideToneEnabled { set; get; }
        [Device(Model.BudsPlus)]
        public bool ExtraHighAmbientEnabled { set; get; }


        [Device(Model.BudsLive)]
        public bool RelieveAmbient { set; get; }
        [Device(Model.BudsLive)]
        public bool VoiceWakeUp { set; get; }
        [Device(Model.BudsLive)]
        public int VoiceWakeUpLang { set; get; }
        [Device(Model.BudsLive)]
        public int FmmRevision { set; get; }
        [Device(Model.BudsLive)]
        public bool NoiceCancelling { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            if (ActiveModel == Model.Buds)
            {
                Revision = msg.Payload[0];
                EarType = msg.Payload[1];
                BatteryL = msg.Payload[2];
                BatteryR = msg.Payload[3];
                IsCoupled = Convert.ToBoolean(msg.Payload[4]);
                MainConnection = (DeviceInv) msg.Payload[5];
                WearState = (WearStates) msg.Payload[6];
                AmbientSoundEnabled = Convert.ToBoolean(msg.Payload[7]);
                AmbientSoundMode = (AmbientType) msg.Payload[8];
                AmbientSoundVolume = msg.Payload[9];
                EqualizerEnabled = Convert.ToBoolean(msg.Payload[10]);
                EqualizerMode = msg.Payload[11];

                if (msg.Size > 13)
                {
                    TouchpadLock = Convert.ToBoolean(msg.Payload[12]);
                    TouchpadOptionL = TouchOption.ToUniversal((msg.Payload[13] & 0xF0) >> 4);
                    TouchpadOptionR = TouchOption.ToUniversal(msg.Payload[13] & 0x0F);
                    if (Revision >= 3)
                    {
                        SeamlessConnectionEnabled = msg.Payload[14] == 0;
                    }
                }
                else
                {
                    TouchpadLock = Convert.ToBoolean((msg.Payload[12] & 0xF0) >> 4);
                    TouchpadOptionL = TouchOption.ToUniversal(msg.Payload[12] & 0x0F);
                    TouchpadOptionR = TouchOption.ToUniversal(msg.Payload[12] & 0x0F);
                    if (Revision >= 3)
                    {
                        SeamlessConnectionEnabled = msg.Payload[13] == 0;
                    }
                }
            }
            else
            {
                Revision = msg.Payload[0];
                EarType = msg.Payload[1];
                BatteryL = msg.Payload[2];
                BatteryR = msg.Payload[3];
                IsCoupled = Convert.ToBoolean(msg.Payload[4]);
                MainConnection = (DeviceInv)msg.Payload[5];

                PlacementL = (PlacementStates)((msg.Payload[6] & 240) >> 4);
                PlacementR = (PlacementStates)(msg.Payload[6] & 15);
                if (PlacementL == PlacementStates.Wearing && PlacementR == PlacementStates.Wearing)
                    WearState = WearStates.Both;
                else if (PlacementL == PlacementStates.Wearing)
                    WearState = WearStates.L;
                else if (PlacementR == PlacementStates.Wearing)
                    WearState = WearStates.R;
                else
                    WearState = WearStates.None;

                BatteryCase = msg.Payload[7];

                if (ActiveModel == Model.BudsPlus)
                {
                    AmbientSoundEnabled = Convert.ToBoolean(msg.Payload[8]);
                    AmbientSoundVolume = msg.Payload[9];

                    AdjustSoundSync = msg.Payload[10] == 1;
                    EqualizerMode = msg.Payload[11];
                    TouchpadLock = Convert.ToBoolean(msg.Payload[12]);

                    TouchpadOptionL = TouchOption.ToUniversal((msg.Payload[13] & 240) >> 4);
                    TouchpadOptionR = TouchOption.ToUniversal(msg.Payload[13] & 15);

                    OutsideDoubleTap = msg.Payload[14] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 15);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 17);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    SideToneEnabled = msg.Payload[19] == 1;
                    ExtraHighAmbientEnabled = msg.Payload[20] == 1;

                    if (Revision >= 11)
                    {
                        SeamlessConnectionEnabled = msg.Payload[21] == 0;
                    }
                }
                else if(ActiveModel == Model.BudsLive)
                {
                    AdjustSoundSync = msg.Payload[8] == 1;
                    EqualizerMode = msg.Payload[9];
                    TouchpadLock = Convert.ToBoolean(msg.Payload[10]);

                    TouchpadOptionL = TouchOption.ToUniversal((msg.Payload[11] & 240) >> 4);
                    TouchpadOptionR = TouchOption.ToUniversal(msg.Payload[11] & 15);

                    NoiceCancelling = msg.Payload[12] == 1;
                    VoiceWakeUp = msg.Payload[13] == 1;

                    short leftColor = BitConverter.ToInt16(msg.Payload, 14);
                    short rightColor = BitConverter.ToInt16(msg.Payload, 16);
                    DeviceColor = (Color)(leftColor != rightColor ? 0 : leftColor);

                    VoiceWakeUpLang = msg.Payload[18];

                    if (Revision >= 3)
                    {
                        SeamlessConnectionEnabled = msg.Payload[19] == 0;
                    }
                    if (Revision >= 4)
                    {
                        FmmRevision = msg.Payload[20];
                    }
                    if (Revision >= 5)
                    {
                        RelieveAmbient = msg.Payload[21] == 1;
                    }
                }
            }
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                var customAttributes = (DeviceAttribute[])property.GetCustomAttributes(typeof(DeviceAttribute), true);

                if (customAttributes.Length <= 0)
                {
                    map.Add(property.Name, property.GetValue(this).ToString());
                }
                else if (customAttributes[0].Models.Contains(ActiveModel))
                {
                    map.Add($"{property.Name} ({customAttributes[0]})", property.GetValue(this).ToString());
                }
            }

            return map;
        }
    }
}
