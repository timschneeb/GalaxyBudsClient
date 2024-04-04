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
        SppMessageReceiver.Instance.DebugSkuUpdate += (_, decoder) => DebugSku = decoder;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += (_, decoder) => ExtendedStatusUpdate = decoder;
        SppMessageReceiver.Instance.StatusUpdate += (_, decoder) => StatusUpdate = decoder;
        SppMessageReceiver.Instance.GetAllDataResponse += (_, decoder) => DebugGetAllData = decoder;
        SppMessageReceiver.Instance.BaseUpdate += (_, update) =>
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

    public DebugGetAllDataDecoder? DebugGetAllData { private set; get; }
    public DebugSkuDecoder? DebugSku { private set; get; }
    public ExtendedStatusUpdateDecoder? ExtendedStatusUpdate { private set; get; }
    public StatusUpdateDecoder? StatusUpdate { private set; get; }

    public IBasicStatusUpdate? BasicStatusUpdate => (IBasicStatusUpdate?) StatusUpdate ?? ExtendedStatusUpdate;
    public IBasicStatusUpdate? BasicStatusUpdateWithValidCase { private set; get; }
}