using System;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.STATUS_UPDATED)]
public class StatusUpdateDecoder : BaseMessageDecoder, IBasicStatusUpdate
{
    public int BatteryL { get; }
    public int BatteryR { get; }
    public bool IsCoupled { get; }
    public DevicesInverted MainConnection { get; }
    public LegacyWearStates WearState { get; }


    [Device(Models.Buds)]
    public int EarType { get; }


    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public int Revision { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public PlacementStates PlacementL { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public PlacementStates PlacementR { get; }
    [Device(Models.BudsPlus, Selector.GreaterEqual)]
    public int BatteryCase { get; }
    
    [Device(Models.Buds2, Selector.GreaterEqual)]
    public bool IsLeftCharging { get; }
    [Device(Models.Buds2, Selector.GreaterEqual)]
    public bool IsRightCharging { get; } 
    [Device(Models.Buds2, Selector.GreaterEqual)]
    public bool IsCaseCharging { get; }

    public StatusUpdateDecoder(SppMessage msg) : base(msg)
    {
        if (TargetModel == Models.Buds)
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