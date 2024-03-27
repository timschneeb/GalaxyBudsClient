using System;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Decoder;

public class StatusUpdateParser : BaseMessageParser, IBasicStatusUpdate
{

    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.STATUS_UPDATED;
    public int BatteryL { set; get; }
    public int BatteryR { set; get; }
    public bool IsCoupled { set; get; }
    public DevicesInverted MainConnection { set; get; }
    public LegacyWearStates WearState { set; get; }


    [Device(Models.Buds)]
    public int EarType { set; get; }


    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public int Revision { set; get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public PlacementStates PlacementL { set; get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public PlacementStates PlacementR { set; get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public int BatteryCase { set; get; }
    
    [Device(Models.Buds2, Selector.GreaterEqual)]
    public bool IsLeftCharging { set; get; }
    [Device(Models.Buds2, Selector.GreaterEqual)]
    public bool IsRightCharging { set; get; } 
    [Device(Models.Buds2, Selector.GreaterEqual)]
    public bool IsCaseCharging { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        if (ActiveModel == Models.Buds)
        {
            EarType = msg.Payload[0];
            BatteryL = msg.Payload[1];
            BatteryR = msg.Payload[2];
            IsCoupled = Convert.ToBoolean(msg.Payload[3]);
            MainConnection = (DevicesInverted) msg.Payload[4];
            WearState = (LegacyWearStates) msg.Payload[5];
            switch (WearState)
            {
                case LegacyWearStates.Both:
                    PlacementL = PlacementStates.Wearing;
                    PlacementR = PlacementStates.Wearing;
                    break;
                case LegacyWearStates.L:
                    PlacementL = PlacementStates.Wearing;
                    PlacementR = PlacementStates.Idle;
                    break;
                case LegacyWearStates.R:
                    PlacementL = PlacementStates.Idle;
                    PlacementR = PlacementStates.Wearing;
                    break;
                default:
                    PlacementL = PlacementStates.Idle;
                    PlacementR = PlacementStates.Idle;
                    break;
            }
        }
        else
        {
            Revision = msg.Payload[0];
            BatteryL = msg.Payload[1];
            BatteryR = msg.Payload[2];
            IsCoupled = Convert.ToBoolean(msg.Payload[3]);
            MainConnection = (DevicesInverted)msg.Payload[4];

            PlacementL = (PlacementStates)((msg.Payload[5] & 240) >> 4);
            PlacementR = (PlacementStates)(msg.Payload[5] & 15);
            if (PlacementL == PlacementStates.Wearing && PlacementR == PlacementStates.Wearing)
                WearState = LegacyWearStates.Both;
            else if (PlacementL == PlacementStates.Wearing)
                WearState = LegacyWearStates.L;
            else if (PlacementR == PlacementStates.Wearing)
                WearState = LegacyWearStates.R;
            else
                WearState = LegacyWearStates.None;

            BatteryCase = msg.Payload[6];

            if (DeviceSpec.Supports(Features.ChargingState))
            {
                var chargingStatus = msg.Payload[7];
                IsLeftCharging = ByteArrayUtils.ValueOfBinaryDigit(chargingStatus, 4) == 16;
                IsRightCharging = ByteArrayUtils.ValueOfBinaryDigit(chargingStatus, 2) == 4;
                IsCaseCharging = ByteArrayUtils.ValueOfBinaryDigit(chargingStatus, 0) == 1;
            }
        }
        
        if (DeviceSpec.Supports(Features.ChargingState))
        {
            if(IsLeftCharging)
                PlacementL = PlacementStates.Charging;
            if(IsRightCharging)
                PlacementR = PlacementStates.Charging;
        }
    }
}