using GalaxyBudsClient.Message.Decoder;

namespace GalaxyBudsClient.Message;

public class DeviceMessageCache
{
    private static readonly object Padlock = new();
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
        SppMessageHandler.Instance.DebugSkuUpdate += (_, parser) => DebugSku = parser;
        SppMessageHandler.Instance.ExtendedStatusUpdate += (_, parser) => ExtendedStatusUpdate = parser;
        SppMessageHandler.Instance.StatusUpdate += (_, parser) => StatusUpdate = parser;
        SppMessageHandler.Instance.GetAllDataResponse += (_, parser) => DebugGetAllData = parser;
        SppMessageHandler.Instance.BaseUpdate += (_, update) =>
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
        BasicStatusUpdateWithValidCase = null;
    }

    public int ExtendedStatusRevision => ExtendedStatusUpdate?.Revision ?? -1;
    public DebugGetAllDataParser? DebugGetAllData { private set; get; }
    public DebugSkuParser? DebugSku { private set; get; }
    public ExtendedStatusUpdateParser? ExtendedStatusUpdate { private set; get; }
    public StatusUpdateParser? StatusUpdate { private set; get; }

    public IBasicStatusUpdate? BasicStatusUpdate => (IBasicStatusUpdate?) StatusUpdate ?? ExtendedStatusUpdate;
    public IBasicStatusUpdate? BasicStatusUpdateWithValidCase { private set; get; }
}