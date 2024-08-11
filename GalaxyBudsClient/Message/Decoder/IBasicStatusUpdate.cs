using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

public interface IBasicStatusUpdate
{
    public int BatteryL { get; }
    public int BatteryR { get; }
    
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
}