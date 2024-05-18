using System.ComponentModel;
using System.Net.Http;
using System.Speech.Synthesis;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Platform;

public class NotificationReaderManager
{
    public NotificationReaderManager()
    {
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        PlatformImpl.NotificationListener.NotificationReceived += OnNotificationReceived;
    }

    private void OnNotificationReceived(object? sender, Notification e)
    {
        // TODO
    }

    private static void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(SettingsData.ReadNotificationsAloud))
            PlatformImpl.NotificationListener.IsEnabled = Settings.Data.ReadNotificationsAloud;
    }
    
    #region Singleton
    private static readonly object Padlock = new();
    public static void Init()
    {
        _instance ??= new NotificationReaderManager();
    }
    
    private static NotificationReaderManager? _instance;
    public static NotificationReaderManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new NotificationReaderManager();
            }
        }
    }
    #endregion
}