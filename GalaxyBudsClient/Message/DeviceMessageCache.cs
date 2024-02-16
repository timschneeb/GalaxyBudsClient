using Castle.Components.DictionaryAdapter;
using GalaxyBudsClient.Message.Decoder;

namespace GalaxyBudsClient.Message
{
    public class DeviceMessageCache
    {
        private static readonly object Padlock = new object();
        private static DeviceMessageCache? _instance;
        public static DeviceMessageCache Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new DeviceMessageCache();
                }
            }
        }

        public static void Init()
        {
            lock (Padlock)
            { 
                _instance ??= new DeviceMessageCache();
            }
        }

        private DeviceMessageCache()
        {
            SPPMessageHandler.Instance.DebugSkuUpdate += (sender, parser) => DebugSku = parser;
            SPPMessageHandler.Instance.ExtendedStatusUpdate += (sender, parser) => ExtendedStatusUpdate = parser;
            SPPMessageHandler.Instance.StatusUpdate += (sender, parser) => StatusUpdate = parser;
            SPPMessageHandler.Instance.GetAllDataResponse += (sender, parser) => DebugGetAllData = parser;
            SPPMessageHandler.Instance.BaseUpdate += (sender, update) =>
            {
                if (update.BatteryCase <= 100 || BasicStatusUpdateWithValidCase == null) // 101 = Disconnected
                {
                    BasicStatusUpdateWithValidCase = update;
                }
            };
        }

        public void Clear()
        {
            DebugGetAllData = null;
            ExtendedStatusUpdate = null;
            StatusUpdate = null;
            DebugSku = null;
        }
        
        public DebugGetAllDataParser? DebugGetAllData { set; get; }
        public DebugSkuParser? DebugSku { set; get; }
        public ExtendedStatusUpdateParser? ExtendedStatusUpdate { set; get; }
        public StatusUpdateParser? StatusUpdate { set; get; }

        public IBasicStatusUpdate? BasicStatusUpdate => (IBasicStatusUpdate?) StatusUpdate ?? ExtendedStatusUpdate;
        public IBasicStatusUpdate? BasicStatusUpdateWithValidCase { set; get; }
    }
}