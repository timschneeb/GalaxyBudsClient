using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    public interface IBasicStatusUpdate
    {
        public int BatteryL { set; get; }
        public int BatteryR { set; get; }

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
    }
}