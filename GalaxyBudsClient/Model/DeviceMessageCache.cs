using GalaxyBudsClient.Decoder;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Model
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

        public DeviceMessageCache()
        {
            SPPMessageHandler.Instance.ExtendedStatusUpdate += (sender, parser) =>
            {
                ExtendedStatusUpdate = parser;
            };
        }

        public ExtendedStatusUpdateParser? ExtendedStatusUpdate { set; get; }
    }
}