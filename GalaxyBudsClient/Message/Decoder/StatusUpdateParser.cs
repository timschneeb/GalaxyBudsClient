using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    public class StatusUpdateParser : BaseMessageParser, IBasicStatusUpdate
    {

        public override SPPMessage.MessageIds HandledType => SPPMessage.MessageIds.STATUS_UPDATED;
        public int BatteryL { set; get; }
        public int BatteryR { set; get; }
        public bool IsCoupled { set; get; }
        public DeviceInv MainConnection { set; get; }
        public WearStates WearState { set; get; }


        [Device(Models.Buds)]
        public int EarType { set; get; }


        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro })]
        public int Revision { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro })]
        public PlacementStates PlacementL { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro })]
        public PlacementStates PlacementR { set; get; }
        [Device(new[] { Models.BudsPlus, Models.BudsLive, Models.BudsPro })]
        public int BatteryCase { set; get; }

        public override void ParseMessage(SPPMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            if (ActiveModel == Models.Buds)
            {
                EarType = msg.Payload[0];
                BatteryL = msg.Payload[1];
                BatteryR = msg.Payload[2];
                IsCoupled = Convert.ToBoolean(msg.Payload[3]);
                MainConnection = (DeviceInv) msg.Payload[4];
                WearState = (WearStates) msg.Payload[5];
            }
            else
            {
                Revision = msg.Payload[0];
                BatteryL = msg.Payload[1];
                BatteryR = msg.Payload[2];
                IsCoupled = Convert.ToBoolean(msg.Payload[3]);
                MainConnection = (DeviceInv)msg.Payload[4];

                PlacementL = (PlacementStates)((msg.Payload[5] & 240) >> 4);
                PlacementR = (PlacementStates)(msg.Payload[5] & 15);
                if (PlacementL == PlacementStates.Wearing && PlacementR == PlacementStates.Wearing)
                    WearState = WearStates.Both;
                else if (PlacementL == PlacementStates.Wearing)
                    WearState = WearStates.L;
                else if (PlacementR == PlacementStates.Wearing)
                    WearState = WearStates.R;
                else
                    WearState = WearStates.None;

                if (ActiveModel == Models.BudsPro && msg.Payload[6] == 101)
                {
                    /* Cradle disconnected */
                    BatteryCase = 0;
                }
                else
                {
                    BatteryCase = msg.Payload[6];
                }
            }
        }

        public override Dictionary<String, String> ToStringMap()
        {
            Dictionary<String, String> map = new Dictionary<string, string>();
            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "HandledType" || property.Name == "ActiveModel")
                    continue;

                var customAttributes = (DeviceAttribute[])property.GetCustomAttributes(typeof(DeviceAttribute), true);

                if (customAttributes.Length <= 0)
                {
                    map.Add(property.Name, property?.GetValue(this)?.ToString() ?? "null");
                }
                else if (customAttributes[0].Models.Contains(ActiveModel))
                {
                    map.Add($"{property?.Name} ({customAttributes[0]})", property?.GetValue(this)?.ToString() ?? "null");
                }
            }

            return map;
        }
    }
}
